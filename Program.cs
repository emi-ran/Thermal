using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading; // Timer için

namespace Thermal
{
    internal static class Program
    {
        // Uygulama bileşenleri (nullable)
        private static HardwareMonitor? hardwareMonitor;
        private static OverlayWindow? overlayWindow;
        private static SystemTrayHandler? systemTrayHandler;
        private static System.Windows.Forms.Timer? updateTimer;
        private static System.Windows.Forms.Timer? mouseCheckTimer;
        private static System.Windows.Forms.Timer? hideDelayTimer; // Genel gizleme gecikme timer'ı

        // Durum değişkenleri
        private static bool autoHideEnabled = false;
        private static bool isHighTemperature = false;
        private static bool mouseIsInHotZone = false;

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

            // Donanım Monitörünü Başlat
            hardwareMonitor = new HardwareMonitor();
            if (!hardwareMonitor.Initialize())
            {
                FreeConsole();
                return; // Başlatma başarısızsa çık
            }

            // Overlay Penceresini Oluştur
            overlayWindow = new OverlayWindow();
            overlayWindow.Shown += OnOverlayShown; // Gösterildiğinde olayını dinle

            // Sistem Tepsisi Yöneticisini Oluştur
            systemTrayHandler = new SystemTrayHandler();
            systemTrayHandler.ExitRequested += OnExitRequested;
            systemTrayHandler.AutoHideChanged += OnAutoHideChanged;

            // Zamanlayıcıları Ayarla
            SetupTimers();

            // İlk Güncellemeyi Yap
            UpdateDisplayAndState();

            // Overlay'i Göster ve Zamanlayıcıları Başlat
            overlayWindow.Show();
            updateTimer?.Start();
            mouseCheckTimer?.Start();

            Console.WriteLine("Uygulama Başlatıldı ve Çalışıyor.");
            Application.Run(); // Ana mesaj döngüsü
        }

        // Zamanlayıcıları Ayarla
        private static void SetupTimers()
        {
            updateTimer = new System.Windows.Forms.Timer { Interval = SHORT_INTERVAL };
            updateTimer.Tick += (s, e) => UpdateDisplayAndState();

            mouseCheckTimer = new System.Windows.Forms.Timer { Interval = MOUSE_CHECK_INTERVAL };
            mouseCheckTimer.Tick += MouseCheckTimer_Tick;

            // hideDelayTimer ihtiyaç anında oluşturulup başlatılacak
        }

        // Overlay Gösterildiğinde Çalışacak Olay
        private static void OnOverlayShown(object? sender, EventArgs e)
        {
            Console.WriteLine("Overlay Penceresi Gösterildi.");
            // Otomatik gizle açıksa ve özel durumlar yoksa gizleme timer'ını başlat
            CheckAndStartHideTimer();
        }

        // Sistem Tepsisi - Otomatik Gizle Değiştiğinde
        private static void OnAutoHideChanged(object? sender, bool isEnabled)
        {
            autoHideEnabled = isEnabled;
            StopAndDisposeTimer(ref hideDelayTimer); // Mevcut gecikmeyi iptal et

            if (autoHideEnabled)
            {
                // Görünürse ve özel durum yoksa gizleme timer'ını başlat
                CheckAndStartHideTimer();
                // Eğer gizliyse interval'i ayarla
                if (overlayWindow != null && !overlayWindow.IsVisible && !isHighTemperature)
                {
                    SetUpdateInterval(LONG_INTERVAL);
                }
            }
            else // Otomatik gizle kapatıldı
            {
                overlayWindow?.FadeIn();
                SetUpdateInterval(SHORT_INTERVAL);
            }
        }

        // Sistem Tepsisi - Çıkış İstendiğinde
        private static void OnExitRequested(object? sender, EventArgs e)
        {
            CleanupAndExit();
        }

        // Ana Güncelleme ve Durum Kontrolü
        private static void UpdateDisplayAndState()
        {
            if (hardwareMonitor == null || overlayWindow == null) return;

            // Console.WriteLine("\nUpdateDisplayAndState Başladı...");
            try
            {
                hardwareMonitor.UpdateSensors();

                float cpuTemperature = hardwareMonitor.GetCpuTemperature();
                float gpuTemperature = hardwareMonitor.GetGpuTemperature();
                overlayWindow.UpdateLabel("CPU", cpuTemperature);
                overlayWindow.UpdateLabel("GPU", gpuTemperature);

                bool wasHighTemperature = isHighTemperature;
                isHighTemperature = (cpuTemperature >= 70 || (gpuTemperature >= 70 && overlayWindow.IsVisible)); // Sadece görünür GPU

                // Console.WriteLine($" Tick - CPU: {cpuTemperature:F0}, GPU: {gpuTemperature:F0}, HighTemp: {isHighTemperature}, AutoHide: {autoHideEnabled}, Visible: {overlayWindow.IsVisible}, Fading: {overlayWindow.IsFading}, MouseIn: {mouseIsInHotZone}");

                // Durum değişikliklerini işle
                if (isHighTemperature && !wasHighTemperature) // Yüksek sıcaklık yeni başladı
                {
                    Console.WriteLine("Yüksek Sıcaklık Algılandı! Gösteriliyor...");
                    StopAndDisposeTimer(ref hideDelayTimer);
                    overlayWindow.FadeIn();
                    SetUpdateInterval(SHORT_INTERVAL);
                }
                else if (!isHighTemperature && wasHighTemperature) // Yüksek sıcaklık yeni bitti
                {
                    Console.WriteLine("Yüksek Sıcaklık Düştü.");
                    CheckAndStartHideTimer(); // Gizleme koşulları uygunsa timer'ı başlat
                }

                // Yüksek sıcaklık yoksa interval'i ayarla
                if (!isHighTemperature)
                {
                    if (autoHideEnabled && !overlayWindow.IsVisible && !overlayWindow.IsFading)
                    {
                        SetUpdateInterval(LONG_INTERVAL);
                    }
                    else // Oto gizle kapalı veya görünürse (ve fade olmuyorsa)
                    {
                        SetUpdateInterval(SHORT_INTERVAL);
                    }
                }

                // Konumu her zaman ayarla (label boyutları değişmiş olabilir)
                overlayWindow.PositionOverlay();
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Güncelleme hatası: {ex.Message}");
                // Hata durumunda belki labelleri gizlemek iyi olabilir
                overlayWindow?.UpdateLabel("CPU", -1); // Gizle
                overlayWindow?.UpdateLabel("GPU", -1); // Gizle
            }
            // Console.WriteLine(" UpdateDisplayAndState Bitti.");
        }

        // Fare Konumu Kontrolü
        private static void MouseCheckTimer_Tick(object? sender, EventArgs e)
        {
            if (overlayWindow == null || !autoHideEnabled || isHighTemperature || overlayWindow.IsFading || !overlayWindow.IsHandleCreated)
            {
                if (mouseIsInHotZone)
                {
                    mouseIsInHotZone = false;
                    CheckAndStartHideTimer(); // Bölgeden çıkıldı, gizleme koşulları uygunsa başlat
                }
                return;
            }

            try
            {
                Point cursorPos = Cursor.Position;
                bool currentlyInHotZone = overlayWindow.HotZone.Contains(cursorPos);

                if (currentlyInHotZone && !mouseIsInHotZone)
                {
                    Console.WriteLine("Fare sıcak bölgeye girdi. Gösteriliyor...");
                    mouseIsInHotZone = true;
                    StopAndDisposeTimer(ref hideDelayTimer);
                    overlayWindow.FadeIn();
                    SetUpdateInterval(SHORT_INTERVAL);
                }
                else if (!currentlyInHotZone && mouseIsInHotZone)
                {
                    Console.WriteLine("Fare sıcak bölgeden çıktı. Hide timer başlatılıyor...");
                    mouseIsInHotZone = false;
                    StartHideDelayTimer(); // Fare ayrıldı, gecikmeli gizle
                }
            }
            catch (Exception ex) { Console.WriteLine($"Mouse check hatası: {ex.Message}"); }
        }

        // Gizleme koşullarını kontrol edip timer'ı başlatan yardımcı metot
        private static void CheckAndStartHideTimer()
        {
            if (autoHideEnabled && overlayWindow != null && overlayWindow.IsVisible &&
                !isHighTemperature && !mouseIsInHotZone && !overlayWindow.IsFading)
            {
                Console.WriteLine("Koşullar uygun, Hide Delay Timer başlatılıyor.");
                StartHideDelayTimer();
            }
            else
            {
                Console.WriteLine("Gizleme koşulları uygun değil, timer başlatılmadı.");
            }
        }

        // Gecikmeli Gizleme Timer'ını Başlatma
        private static void StartHideDelayTimer()
        {
            StopAndDisposeTimer(ref hideDelayTimer); // Öncekini durdur
            hideDelayTimer = new System.Windows.Forms.Timer { Interval = HIDE_DELAY };
            hideDelayTimer.Tick += OnHideDelayTimerTick;
            Console.WriteLine("Hide Delay Timer başlatıldı.");
            hideDelayTimer.Start();
        }

        // Gecikme Timer'ı Tick Olayı
        private static void OnHideDelayTimerTick(object? sender, EventArgs e)
        {
            StopAndDisposeTimer(ref hideDelayTimer);
            Console.WriteLine("Hide Delay Timer Tick - FadeOut çağrılıyor.");
            // Tekrar kontrol et (durum değişmiş olabilir)
            if (autoHideEnabled && overlayWindow != null && overlayWindow.IsVisible &&
               !isHighTemperature && !mouseIsInHotZone && !overlayWindow.IsFading)
            {
                overlayWindow.FadeOut();
            }
            else
            {
                Console.WriteLine(" -> FadeOut engellendi (koşullar değişti).");
            }
        }

        // Timer Durdurma ve Dispose Etme Yardımcısı
        private static void StopAndDisposeTimer(ref System.Windows.Forms.Timer? timerInstance)
        {
            if (timerInstance != null)
            {
                timerInstance.Stop();
                timerInstance.Dispose();
                timerInstance = null;
            }
        }

        // Güncelleme Aralığını Ayarlama
        private static void SetUpdateInterval(int newInterval)
        {
            if (updateTimer != null && updateTimer.Interval != newInterval)
            {
                Console.WriteLine($" -> Update interval ayarlanıyor: {newInterval}ms");
                updateTimer.Interval = newInterval;
            }
        }

        // Uygulamayı Temizleme ve Kapatma
        private static void CleanupAndExit()
        {
            Console.WriteLine("CleanupAndExit çağrıldı.");
            try
            {
                StopAndDisposeTimer(ref updateTimer);
                StopAndDisposeTimer(ref mouseCheckTimer);
                StopAndDisposeTimer(ref hideDelayTimer);

                overlayWindow?.Dispose(); // Bu, içindeki fade timer'ı da durdurmalı
                systemTrayHandler?.Dispose();
                hardwareMonitor?.Dispose();

                Console.WriteLine("Tüm kaynaklar serbest bırakıldı.");
            }
            catch (Exception ex) { Console.WriteLine($"Çıkış sırasında hata: {ex.Message}"); }
            finally
            {
                Console.WriteLine("Application.Exit() çağrılıyor.");
                Application.Exit();
                FreeConsole();
            }
        }

        // Konsol API'leri
        [DllImport("kernel32.dll", SetLastError = true)][return: MarshalAs(UnmanagedType.Bool)] static extern bool AllocConsole();
        [DllImport("kernel32.dll", SetLastError = true)][return: MarshalAs(UnmanagedType.Bool)] static extern bool FreeConsole();
    }
}
