using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading; // Timer için
using LibreHardwareMonitor.Hardware; // Gerekli using ifadesi

namespace Thermal
{
    internal static class Program
    {
        // Uygulama bileşenleri (nullable)
        private static HardwareMonitor? hardwareMonitor;
        private static OverlayWindow? overlayWindow;
        private static SystemTrayHandler? systemTrayHandler;
        private static AppSettings appSettings = new AppSettings(); // Ayarları tutacak nesne
        private static System.Windows.Forms.Timer? updateTimer;
        private static System.Windows.Forms.Timer? mouseCheckTimer;
        private static Point lastMousePosition = Point.Empty;
        private static DateTime lastMouseMoveTime = DateTime.MinValue;
        private static bool isMouseOverHotZone = false; // Anlık durumu hala tutabiliriz, ancak kararları stabil olana göre vereceğiz.
        private static bool isMouseOverHotZoneStable = false; // Farenin bölgedeki kararlı durumu
        private static int hotZoneConsecutiveTicks = 0; // Kararlı duruma geçiş için sayaç
        private const int HOT_ZONE_STABILITY_THRESHOLD = 5; // Kararlılık için gereken tick sayısı (5 * 100ms = 500ms)
        private static bool autoHideEnabled = false; // Başlangıçta otomatik gizleme KAPALI

        // Sabitler
        private const int SHORT_INTERVAL = 10000; // 10 saniye
        private const int LONG_INTERVAL = 30000; // 30 saniye
        private const int HIDE_DELAY = 5000; // 5 saniye
        private const int MOUSE_CHECK_INTERVAL = 250; // 250 ms

        [STAThread]
        static void Main()
        {
            // Hata ayıklama için konsolu aktif et (isteğe bağlı)
            AllocConsole();
            Console.WriteLine("Uygulama Başlatılıyor...");

            ApplicationConfiguration.Initialize();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Mutex mutex = new Mutex(true, "ThermalAppMutex", out bool createdNew);
            if (!createdNew)
            {
                MessageBox.Show("Uygulama zaten çalışıyor.", "Thermal", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // TODO: AppSettings'i dosyadan yükleme (varsa)
            // appSettings = AppSettings.Load() ?? new AppSettings();
            autoHideEnabled = appSettings.EnableMouseHoverShow; // Ayarlardan başlangıç değerini al

            // Donanım Monitörü başlat
            try
            {
                hardwareMonitor = new HardwareMonitor();
                if (!hardwareMonitor.Initialize()) // Initialize çağır
                {
                    throw new Exception("LibreHardwareMonitor başlatılamadı.");
                }
                Console.WriteLine("HardwareMonitor başlatıldı.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Donanım izleyici başlatılamadı: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Cleanup(mutex);
                return;
            }

            // OverlayWindow oluştur ve başlangıç ayarlarını uygula
            overlayWindow = new OverlayWindow();
            ApplySettings(); // Başlangıç renk/eşik ayarlarını uygula

            // << YENİ BAŞLANGIÇ GÜNCELLEMESİ >>
            try
            {
                if (hardwareMonitor != null)
                {
                    hardwareMonitor.UpdateSensors(); // İlk okumayı yap
                    float initialCpuTemp = hardwareMonitor.GetCpuTemperature();
                    float initialGpuTemp = hardwareMonitor.GetGpuTemperature();
                    overlayWindow.UpdateLabel("CPU", initialCpuTemp > 0 ? initialCpuTemp : -1);
                    overlayWindow.UpdateLabel("GPU", initialGpuTemp > 0 ? initialGpuTemp : -1);
                    overlayWindow.PositionOverlay(); // Labellar güncellendikten sonra ilk konumlandırmayı yap
                    Console.WriteLine("İlk sıcaklık okuması ve konumlandırma yapıldı.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"İlk güncelleme sırasında hata: {ex.Message}");
                // Hata olsa bile devam et, timer ile düzelebilir.
            }
            // << YENİ BAŞLANGIÇ GÜNCELLEMESİ SONU >>

            overlayWindow.Shown += OverlayWindow_Shown;
            Console.WriteLine("OverlayWindow oluşturuldu.");

            // Sistem Tepsisi Yöneticisi oluştur ve olayları bağla
            systemTrayHandler = new SystemTrayHandler();
            systemTrayHandler.ExitRequested += OnExitRequested;
            systemTrayHandler.AutoHideChanged += OnAutoHideChanged; // Doğru olay adı
            systemTrayHandler.SettingsRequested += OnSettingsRequested;
            systemTrayHandler.SetAutoHideState(autoHideEnabled); // Doğru metot adı
            Console.WriteLine("SystemTrayHandler oluşturuldu.");

            // Timer'ları ayarla
            SetupTimers();
            Console.WriteLine("Timer'lar ayarlandı.");

            // Overlay'ı göster
            overlayWindow.Show();

            // Uygulama mesaj döngüsünü başlat
            Application.Run();

            // Uygulama kapanırken temizlik yap
            Cleanup(mutex);
            Console.WriteLine("Uygulama kapatıldı.");
        }

        private static void OverlayWindow_Shown(object? sender, EventArgs e)
        {
            StartTimers();
            Console.WriteLine("Overlay gösterildi, timer'lar başlatıldı.");
        }

        private static void SetupTimers()
        {
            updateTimer = new System.Windows.Forms.Timer();
            updateTimer.Tick += UpdateTimer_Tick;
            SetUpdateInterval();

            mouseCheckTimer = new System.Windows.Forms.Timer { Interval = 100 };
            mouseCheckTimer.Tick += MouseCheckTimer_Tick;
        }

        private static void StartTimers()
        {
            lastMouseMoveTime = DateTime.Now; // Fare hareket zamanını başlat
            updateTimer?.Start();
            mouseCheckTimer?.Start();
            Console.WriteLine("Timer'lar başlatıldı.");
        }

        private static void StopTimers()
        {
            updateTimer?.Stop();
            mouseCheckTimer?.Stop();
            Console.WriteLine("Timer'lar durduruldu.");
        }

        private static void SetUpdateInterval()
        {
            if (updateTimer != null)
            {
                // Kararlı durumu kullan
                bool useShortInterval = isMouseOverHotZoneStable || !autoHideEnabled;
                int newInterval = useShortInterval ? appSettings.ShortUpdateIntervalMs : appSettings.LongUpdateIntervalMs;
                if (updateTimer.Interval != newInterval)
                {
                    updateTimer.Interval = newInterval;
                    Console.WriteLine($"Update interval ayarlandı: {updateTimer.Interval}ms (Stabil Bölgede: {isMouseOverHotZoneStable}, AutoHide: {autoHideEnabled}, Kısa mı: {useShortInterval})");
                }
            }
        }

        private static void UpdateTimer_Tick(object? sender, EventArgs e)
        {
            if (hardwareMonitor == null || overlayWindow == null || !overlayWindow.IsHandleCreated)
                return;
            try
            {
                hardwareMonitor.UpdateSensors(); // Doğru metot adı

                float cpuTemp = hardwareMonitor.GetCpuTemperature(); // Doğru metot adı
                float gpuTemp = hardwareMonitor.GetGpuTemperature(); // Doğru metot adı

                overlayWindow.UpdateLabel("CPU", cpuTemp > 0 ? cpuTemp : -1);
                overlayWindow.UpdateLabel("GPU", gpuTemp > 0 ? gpuTemp : -1);
                overlayWindow.PositionOverlay();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UpdateTimer_Tick Hata: {ex.Message}");
            }
            // Kararlı durumu kullanarak CheckAutoHideLogic çağır
            if (autoHideEnabled) CheckAutoHideLogic(isMouseOverHotZoneStable);
        }

        private static void MouseCheckTimer_Tick(object? sender, EventArgs e)
        {
            if (overlayWindow == null || !overlayWindow.IsHandleCreated)
                return;
            try
            {
                Point currentMousePosition = Cursor.Position;
                bool mouseMoved = currentMousePosition != lastMousePosition;
                if (mouseMoved)
                {
                    lastMouseMoveTime = DateTime.Now;
                    lastMousePosition = currentMousePosition;
                }

                // Farenin anlık konumunu al
                bool currentlyInHotZone = overlayWindow.HotZone.Contains(currentMousePosition) && appSettings.EnableMouseHoverShow;
                isMouseOverHotZone = currentlyInHotZone; // Anlık durumu güncelle (opsiyonel, loglama için vs.)

                // Kararlılık kontrolü
                if (currentlyInHotZone == isMouseOverHotZoneStable)
                {
                    hotZoneConsecutiveTicks = 0; // Durum stabil, sayacı sıfırla
                }
                else
                {
                    hotZoneConsecutiveTicks++; // Durum farklı, sayacı artır
                    // Kararlılık eşiğine ulaşıldı mı?
                    if (hotZoneConsecutiveTicks >= HOT_ZONE_STABILITY_THRESHOLD)
                    {
                        isMouseOverHotZoneStable = currentlyInHotZone; // Kararlı durumu güncelle
                        hotZoneConsecutiveTicks = 0; // Sayacı sıfırla
                        Console.WriteLine($"Stabil Sıcak Bölge Durumu Değişti: {isMouseOverHotZoneStable}");
                        SetUpdateInterval(); // Interval'i GÜNCELLE
                    }
                }

                // FadeIn mantığı - Kararlı durumu kullan
                if ((isMouseOverHotZoneStable || !autoHideEnabled) && (!overlayWindow.IsVisible || (overlayWindow.IsFading && !overlayWindow.IsFadingIn)))
                {
                    // Sadece fare gerçekten hareket ettiyse VEYA otomatik gizleme kapalıysa FadeIn yap
                    // (Fare hareket etmese bile durum değişmiş olabilir, örn. Ayarlardan AutoHide kapatıldı)
                    if (mouseMoved || !autoHideEnabled)
                    {
                        Console.WriteLine("Fare hareketi/durumu (stabil) ile FadeIn tetikleniyor.");
                        overlayWindow.FadeIn();
                    }
                }

                // Fare hareket etmese de gizleme mantığını kontrol et - Kararlı durumu kullan
                if (autoHideEnabled) CheckAutoHideLogic(isMouseOverHotZoneStable);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MouseCheckTimer_Tick Hata: {ex.Message}");
            }
        }

        private static void CheckAutoHideLogic(bool stableMouseOverHotZone)
        {
            if (overlayWindow == null || !overlayWindow.IsHandleCreated || !autoHideEnabled)
                return;
            try
            {
                // Kararlı durumu kullan
                bool shouldHide = !stableMouseOverHotZone && (DateTime.Now - lastMouseMoveTime).TotalMilliseconds > appSettings.HideDelayMs;
                if (shouldHide && overlayWindow.IsVisible && !overlayWindow.IsFading)
                {
                    Console.WriteLine("Otomatik gizleme için FadeOut tetikleniyor.");
                    overlayWindow.FadeOut();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CheckAutoHideLogic Hata: {ex.Message}");
            }
        }

        private static void OnAutoHideChanged(object? sender, bool enabled)
        {
            autoHideEnabled = enabled;
            Console.WriteLine($"Otomatik Gizleme Değişti: {enabled}");
            // systemTrayHandler?.SetAutoHideState(enabled); // Menüden geldiği için zaten ayarlı

            if (!autoHideEnabled)
            {
                if (overlayWindow != null && (!overlayWindow.IsVisible || overlayWindow.IsFading))
                {
                    Console.WriteLine("Otomatik gizleme kapatıldı, FadeIn tetikleniyor.");
                    overlayWindow.FadeIn();
                }
                // isMouseOverHotZone = false; // Bunu yapmaya gerek yok, stabil durumu kullanıyoruz
                SetUpdateInterval(); // Durum değiştiği için intervali güncelle
            }
            else
            {
                SetUpdateInterval(); // Durum değiştiği için intervali güncelle
                                     // Kararlı durumu kullanarak CheckAutoHideLogic çağır
                CheckAutoHideLogic(isMouseOverHotZoneStable);
            }
        }

        private static void OnSettingsRequested(object? sender, EventArgs e)
        {
            Console.WriteLine("Ayarlar formu açılıyor...");
            StopTimers();
            using (SettingsForm settingsForm = new SettingsForm(appSettings))
            {
                DialogResult result = settingsForm.ShowDialog();
                if (result == DialogResult.OK)
                {
                    // settingsForm, appSettings nesnesini doğrudan güncelledi.
                    Console.WriteLine("Ayarlar kaydedildi.");
                    ApplySettings();
                    // autoHideEnabled durumunu ayarlardan gelen değere göre güncelle
                    autoHideEnabled = appSettings.EnableMouseHoverShow;
                    systemTrayHandler?.SetAutoHideState(autoHideEnabled); // Tepsi menüsündeki işareti de güncelle
                    // TODO: Ayarları dosyaya kaydetme işlemi burada çağrılabilir.
                    // appSettings.Save();
                }
                else
                {
                    Console.WriteLine("Ayarlar iptal edildi.");
                }
            }
            StartTimers();
            Console.WriteLine("Ayarlar formu kapatıldı.");
        }

        private static void ApplySettings()
        {
            overlayWindow?.ApplyColorSettings(
                appSettings.TempThreshold1, appSettings.ColorLowTemp,
                appSettings.TempThreshold2, appSettings.ColorMidTemp,
                appSettings.ColorHighTemp);
            SetUpdateInterval();
            Console.WriteLine("Yeni ayarlar uygulandı.");
        }

        private static void OnExitRequested(object? sender, EventArgs e)
        {
            Console.WriteLine("Çıkış isteği alındı.");
            Application.Exit();
        }

        private static void Cleanup(Mutex? mutex = null)
        {
            Console.WriteLine("Temizlik yapılıyor...");
            StopTimers();
            updateTimer?.Dispose();
            mouseCheckTimer?.Dispose();
            systemTrayHandler?.Dispose();
            overlayWindow?.Dispose();
            hardwareMonitor?.Dispose(); // Doğru metot adı
            mutex?.ReleaseMutex();
            mutex?.Dispose();
        }

        // Konsol API'leri
        [DllImport("kernel32.dll", SetLastError = true)][return: MarshalAs(UnmanagedType.Bool)] static extern bool AllocConsole();
        [DllImport("kernel32.dll", SetLastError = true)][return: MarshalAs(UnmanagedType.Bool)] static extern bool FreeConsole();
    }
}

