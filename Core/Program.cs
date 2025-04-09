using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading; // Timer için
using LibreHardwareMonitor.Hardware; // Gerekli using ifadesi
using Thermal.Monitoring;
using Thermal.Presentation;
using Thermal.Persistence;

namespace Thermal.Core
{
    internal static class Program
    {
        // Uygulama bileşenleri (nullable)
        private static HardwareMonitor? hardwareMonitor;
        private static OverlayWindow? overlayWindow;
        private static SystemTrayHandler? systemTrayHandler;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable. (Appsettings is initialized in Main)
        private static AppSettings appSettings; // Non-nullable kalacak
#pragma warning restore CS8618
        private static System.Windows.Forms.Timer? updateTimer;
        private static System.Windows.Forms.Timer? mouseCheckTimer;
        private static Point lastMousePosition = Point.Empty;
        private static DateTime lastMouseMoveTime = DateTime.MinValue;
        private static bool isMouseOverHotZone = false; // Anlık durumu hala tutabiliriz, ancak kararları stabil olana göre vereceğiz.
        private static bool isMouseOverHotZoneStable = false; // Farenin bölgedeki kararlı durumu
        private static DateTime mouseLeftHotZoneTime = DateTime.MinValue; // Farenin sıcak bölgeden çıktığı zaman
        private static int hotZoneConsecutiveTicks = 0; // Kararlı duruma geçiş için sayaç
        private const int HOT_ZONE_STABILITY_THRESHOLD = 5; // Kararlılık için gereken tick sayısı (5 * 100ms = 500ms)
        private static bool autoHideEnabled = false; // Başlangıçta otomatik gizleme KAPALI
        private static bool isHighTemperatureOverrideActive = false; // Yüksek sıcaklık override durumu

        // Sabitler (Kaldırıldı - Artık AppSettings kullanılıyor)
        // private const int SHORT_INTERVAL = 10000;
        // private const int LONG_INTERVAL = 30000;
        // private const int HIDE_DELAY = 5000;
        // private const int MOUSE_CHECK_INTERVAL = 250;

        [STAThread]
        static void Main()
        {
            // Hata ayıklama için konsolu aktif et (isteğe bağlı)
            // AllocConsole();
            Console.WriteLine("Uygulama Başlatılıyor...");

            ApplicationConfiguration.Initialize();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Tek örnek kontrolü
            Mutex mutex = new Mutex(true, "ThermalAppMutex", out bool createdNew);
            if (!createdNew)
            {
                MessageBox.Show("Uygulama zaten çalışıyor.", "Thermal", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Ayarları Yükle
            appSettings = RegistryHandler.LoadSettings();
            // AutoHideEnabled durumu artık Registry'den yüklenen ayarlara göre değil,
            // programın mantığına göre (varsayılan kapalı, menüden kontrol) belirlenmeli.
            // Bu yüzden aşağıdaki satıra gerek yok:
            // autoHideEnabled = appSettings.EnableMouseHoverShow;

            // Donanım Monitörü başlat
            try
            {
                hardwareMonitor = new HardwareMonitor();
                if (!hardwareMonitor.Initialize())
                { throw new Exception("LibreHardwareMonitor başlatılamadı."); }
                Console.WriteLine("HardwareMonitor başlatıldı.");
            }
            catch (Exception ex)
            { MessageBox.Show($"Donanım izleyici başlatılamadı: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error); Cleanup(mutex); return; }

            // OverlayWindow oluştur ve başlangıç ayarlarını uygula
            overlayWindow = new OverlayWindow();
            ApplySettings(); // Yüklenen veya varsayılan ayarları uygula

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
            { Console.WriteLine($"İlk güncelleme sırasında hata: {ex.Message}"); }
            // << YENİ BAŞLANGIÇ GÜNCELLEMESİ SONU >>

            overlayWindow.Shown += OverlayWindow_Shown;
            Console.WriteLine("OverlayWindow oluşturuldu.");

            // Sistem Tepsisi Yöneticisi oluştur ve olayları bağla
            systemTrayHandler = new SystemTrayHandler();
            systemTrayHandler.ExitRequested += OnExitRequested;
            systemTrayHandler.AutoHideChanged += OnAutoHideChanged;
            systemTrayHandler.SettingsRequested += OnSettingsRequested;
            // Başlangıç işaretini ayarla (hala 'false' olmalı)
            systemTrayHandler.SetAutoHideState(autoHideEnabled);
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
                // Yüksek sıcaklık öncelikli
                bool useShortInterval = isHighTemperatureOverrideActive || isMouseOverHotZoneStable || !autoHideEnabled;
                int newInterval = useShortInterval ? appSettings.ShortUpdateIntervalMs : appSettings.LongUpdateIntervalMs;
                if (updateTimer.Interval != newInterval)
                {
                    updateTimer.Interval = newInterval;
                    Console.WriteLine($"Update interval ayarlandı: {updateTimer.Interval}ms (Yüksek Sıcaklık: {isHighTemperatureOverrideActive}, Stabil Bölgede: {isMouseOverHotZoneStable}, AutoHide: {autoHideEnabled}, Kısa mı: {useShortInterval})");
                }
            }
        }

        private static void UpdateTimer_Tick(object? sender, EventArgs e)
        {
            if (hardwareMonitor == null || overlayWindow == null || !overlayWindow.IsHandleCreated)
                return;

            float cpuTemp = -1, gpuTemp = -1; // Başlangıç değeri
            try
            {
                hardwareMonitor.UpdateSensors();
                cpuTemp = hardwareMonitor.GetCpuTemperature();
                gpuTemp = hardwareMonitor.GetGpuTemperature();

                overlayWindow.UpdateLabel("CPU", cpuTemp > 0 ? cpuTemp : -1);
                overlayWindow.UpdateLabel("GPU", gpuTemp > 0 ? gpuTemp : -1);
                overlayWindow.PositionOverlay();
            }
            catch (Exception ex) { Console.WriteLine($"UpdateTimer_Tick Sıcaklık Okuma/Güncelleme Hatası: {ex.Message}"); }

            // Yüksek Sıcaklık Override Kontrolü
            bool previousHighTempState = isHighTemperatureOverrideActive;
            // Eşik 2'nin *üstündeyse* yüksek sıcaklık kabul edilir.
            isHighTemperatureOverrideActive = (cpuTemp > appSettings.TempThreshold2) || (gpuTemp > appSettings.TempThreshold2);

            if (previousHighTempState != isHighTemperatureOverrideActive)
            {
                Console.WriteLine($"Yüksek Sıcaklık Override Durumu Değişti: {isHighTemperatureOverrideActive}");
                SetUpdateInterval(); // Yüksek sıcaklık durumuna göre intervali hemen ayarla

                if (isHighTemperatureOverrideActive)
                {   // Yüksek sıcaklık YENİ BAŞLADI
                    mouseLeftHotZoneTime = DateTime.MinValue; // Yüksek sıcaklık varken çıkış zamanı anlamsız
                    if (overlayWindow != null && overlayWindow.CurrentOpacity < 1.0)
                    {
                        Console.WriteLine("Yüksek sıcaklık algılandı, FadeIn tetikleniyor.");
                        overlayWindow.FadeIn();
                    }
                }
                else
                {   // Yüksek sıcaklık YENİ BİTTİ
                    // Sadece otomatik gizleme açıksa VE fare dışarıdaysa VE
                    // zaten bir gizleme bekleme süreci (mouseLeftHotZoneTime ayarlı) başlamamışsa,
                    // çıkış zamanını şimdi başlatarak gizleme timer'ını aktif et.
                    if (autoHideEnabled && !isMouseOverHotZoneStable && mouseLeftHotZoneTime == DateTime.MinValue)
                    {
                        mouseLeftHotZoneTime = DateTime.Now;
                        Console.WriteLine("Yüksek sıcaklık bitti, fare dışarıda ve gizleme beklemiyordu. Çıkış zamanı şimdi ayarlandı.");
                    }
                    // Gizleme gerekip gerekmediğini kontrol et (CheckAutoHideLogic zaten timer tick sonunda çağrılıyor)
                }
            }

            // CheckAutoHideLogic'i sadece yüksek sıcaklık durumu aktif DEĞİLSE çalıştır
            if (autoHideEnabled && !isHighTemperatureOverrideActive) CheckAutoHideLogic(isMouseOverHotZoneStable);
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

                bool currentlyInHotZone = overlayWindow.HotZone.Contains(currentMousePosition) && appSettings.EnableMouseHoverShow;
                isMouseOverHotZone = currentlyInHotZone;

                // Kararlılık kontrolü
                if (currentlyInHotZone == isMouseOverHotZoneStable)
                { hotZoneConsecutiveTicks = 0; }
                else
                {
                    hotZoneConsecutiveTicks++;
                    if (hotZoneConsecutiveTicks >= HOT_ZONE_STABILITY_THRESHOLD)
                    {
                        bool previousStableState = isMouseOverHotZoneStable;
                        isMouseOverHotZoneStable = currentlyInHotZone;
                        hotZoneConsecutiveTicks = 0;
                        Console.WriteLine($"Stabil Sıcak Bölge Durumu Değişti: {isMouseOverHotZoneStable}");

                        if (previousStableState == true && isMouseOverHotZoneStable == false)
                        { // Bölgeden yeni çıktı (Yüksek sıcaklık yoksa çıkış zamanını kaydet)
                            if (!isHighTemperatureOverrideActive)
                            {
                                mouseLeftHotZoneTime = DateTime.Now;
                                Console.WriteLine($"Sıcak bölgeden çıkıldı (Normal): {mouseLeftHotZoneTime:HH:mm:ss.fff}");
                            }
                        }
                        else if (previousStableState == false && isMouseOverHotZoneStable == true)
                        { // Bölgeye yeni girdi
                            mouseLeftHotZoneTime = DateTime.MinValue;
                        }
                        SetUpdateInterval(); // Interval'i GÜNCELLE
                    }
                }

                // FadeIn mantığı - Yüksek sıcaklık VEYA Fare bölgede VEYA OtoGizleme kapalı (ve görünür değilse)
                if ((isHighTemperatureOverrideActive || isMouseOverHotZoneStable || !autoHideEnabled) && overlayWindow != null && overlayWindow.CurrentOpacity < 1.0)
                {
                    overlayWindow.FadeIn(); // Log mesajı zaten OverlayWindow içinde
                }

                // CheckAutoHideLogic çağrısı UpdateTimer_Tick içine taşındı.
                // if (autoHideEnabled && !isHighTemperatureOverrideActive) CheckAutoHideLogic(isMouseOverHotZoneStable);
            }
            catch (Exception ex) { Console.WriteLine($"MouseCheckTimer_Tick Hata: {ex.Message}"); }
        }

        private static void CheckAutoHideLogic(bool stableMouseOverHotZone)
        {
            if (overlayWindow == null || !overlayWindow.IsHandleCreated || !autoHideEnabled || isHighTemperatureOverrideActive) // Ek kontrol
                return;

            try
            {
                bool shouldHide = !stableMouseOverHotZone &&
                                  mouseLeftHotZoneTime != DateTime.MinValue &&
                                  (DateTime.Now - mouseLeftHotZoneTime).TotalMilliseconds > appSettings.HideDelayMs;

                if (shouldHide && overlayWindow.IsVisible && !overlayWindow.IsFading)
                {
                    Console.WriteLine($"Otomatik gizleme için FadeOut tetikleniyor (Normal - Çıkış: {mouseLeftHotZoneTime:HH:mm:ss.fff}, Gecikme: {appSettings.HideDelayMs}ms).");
                    overlayWindow.FadeOut();
                    mouseLeftHotZoneTime = DateTime.MinValue;
                }
            }
            catch (Exception ex) { Console.WriteLine($"CheckAutoHideLogic Hata: {ex.Message}"); }
        }

        private static void OnAutoHideChanged(object? sender, bool enabled)
        {
            autoHideEnabled = enabled;
            Console.WriteLine($"Otomatik Gizleme Değişti: {enabled}");

            if (!autoHideEnabled)
            {
                // Otomatik gizleme kapatıldıysa görünür yap ve intervali ayarla
                if (overlayWindow != null && (!overlayWindow.IsVisible || overlayWindow.IsFading))
                {
                    Console.WriteLine("Otomatik gizleme kapatıldı, FadeIn tetikleniyor.");
                    overlayWindow.FadeIn();
                }
                mouseLeftHotZoneTime = DateTime.MinValue; // Kapatılınca çıkış zamanını sıfırla
                SetUpdateInterval();
            }
            else // Otomatik gizleme AÇILDI
            {
                SetUpdateInterval();
                // Otomatik gizleme açıldığında, eğer fare dışarıdaysa hemen gizleme kontrolü yapalım.
                // lastMouseMoveTime'ı sıfırlamaya gerek yok, yeni mantık mouseLeftHotZoneTime'a bakıyor.
                if (!isMouseOverHotZoneStable)
                {
                    mouseLeftHotZoneTime = DateTime.Now; // Hemen gizleme için çıkış zamanını şimdi olarak ayarla
                    Console.WriteLine("Otomatik gizleme açıldı, fare dışarıda. Çıkış zamanı ayarlandı.");
                    CheckAutoHideLogic(isMouseOverHotZoneStable); // Hemen kontrol et
                }
                else
                {
                    mouseLeftHotZoneTime = DateTime.MinValue; // Fare içerideyse çıkış zamanı olmamalı
                }
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
                    // Güncellenen Ayarları Kayıt Defteri'ne Kaydet
                    RegistryHandler.SaveSettings(appSettings);
                    // Başlangıç Durumunu Güncelle
                    RegistryHandler.SetStartup(appSettings.StartWithWindows);
                }
                else
                { Console.WriteLine("Ayarlar iptal edildi."); }
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

