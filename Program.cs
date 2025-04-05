using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using LibreHardwareMonitor.Hardware;
using System.Linq;
using System.Threading; // Timer için

namespace Thermal
{
    internal static class Program
    {
        // Win32 API'ları için gerekli sabitleri tanımlama
        private const int WS_EX_LAYERED = 0x80000;
        private const int WS_EX_TRANSPARENT = 0x20;

        // SetWindowPos için sabitler
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOACTIVATE = 0x0010;
        private const uint TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE;

        // SetWindowPos API bildirimi
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        // Uygulama bileşenlerini sınıf seviyesinde tanımla (nullable)
        private static Computer? computer;
        private static System.Windows.Forms.Timer? updateTimer; // Ana güncelleme timer'ı
        private static NotifyIcon? notifyIcon;
        private static Form? overlay;
        private static UpdateVisitor? updateVisitor;
        private static Label? cpuLabel; // Nullable
        private static Label? gpuLabel; // Nullable

        // Otomatik Gizle Durumları
        private static bool autoHideEnabled = false;
        private static bool isOverlayVisible = true; // Başlangıçta görünür
        private static bool isFading = false;
        private static bool isHighTemperature = false;
        private static bool mouseIsInHotZone = false;
        private static Rectangle hotZone = Rectangle.Empty; // Fare algılama bölgesi
        private static bool isFadingIn = false; // Solma yönünü belirtir

        // Zamanlayıcılar
        private static System.Windows.Forms.Timer? fadeTimer; // Solma efekti için
        private static System.Windows.Forms.Timer? initialHideTimer; // Başlangıç gizleme için
        private static System.Windows.Forms.Timer? reHideTimer; // Tekrar gizleme için
        private static System.Windows.Forms.Timer? mouseCheckTimer; // Fare konumu kontrolü için
        private static System.Windows.Forms.Timer? mouseLeaveHideTimer; // Fare ayrılınca gizleme için

        private const int SHORT_INTERVAL = 10000; // 10 saniye
        private static int LONG_INTERVAL = 30000; // 30 saniye
        private const int HIDE_DELAY = 5000; // 5 saniye
        private const int MOUSE_CHECK_INTERVAL = 250; // 250 ms
        private const double FADE_STEP = 0.10; // Solma adımı (%10)
        private const int FADE_INTERVAL = 50; // Solma hızı (ms)

        [STAThread]
        static void Main()
        {
            // Hata ayıklama için konsolu aktif et
            AllocConsole();
            Console.WriteLine("Uygulama Başlatıldı...");

            ApplicationConfiguration.Initialize();

            // Hardware monitor ayarları
            computer = new Computer
            {
                IsCpuEnabled = true,
                IsGpuEnabled = true, // GPU izlemeyi etkinleştir
                IsMemoryEnabled = false,
                IsMotherboardEnabled = false,
                IsControllerEnabled = false,
                IsNetworkEnabled = false,
                IsStorageEnabled = false
            };

            try
            {
                computer.Open();
                Console.WriteLine("LibreHardwareMonitor Açıldı.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Donanım bilgileri okunurken hata oluştu: {ex.Message}\nUygulama kapatılacak.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return; // Hata durumunda uygulamayı başlatma
            }

            // UpdateVisitor kullanarak sensör verilerini alalım
            updateVisitor = new UpdateVisitor();
            computer.Accept(updateVisitor);
            Console.WriteLine("İlk sensör güncellemesi yapıldı.");

            // CPU Label oluşturma
            cpuLabel = new Label
            {
                AutoSize = true,
                BackColor = Color.Transparent,
                ForeColor = Color.White, // Başlangıç rengi
                Font = new Font("Arial", 10, FontStyle.Bold), // Font boyutu küçültüldü
                Text = "C: --°C",
                Padding = new Padding(3, 5, 3, 5), // Padding azaltıldı
                Margin = new Padding(0), // Margin sıfırlandı
                TextAlign = ContentAlignment.MiddleCenter,
                Visible = false // Başlangıçta gizli
            };

            // GPU Label oluşturma
            gpuLabel = new Label
            {
                AutoSize = true,
                BackColor = Color.Transparent,
                ForeColor = Color.White, // Başlangıç rengi
                Font = new Font("Arial", 10, FontStyle.Bold), // Font boyutu küçültüldü
                Text = "G: --°C",
                Padding = new Padding(3, 5, 5, 5), // Padding azaltıldı
                Margin = new Padding(0),
                TextAlign = ContentAlignment.MiddleCenter,
                Visible = false // Başlangıçta gizli
            };

            // FlowLayoutPanel ile labelleri yan yana dizme
            var flowPanel = new FlowLayoutPanel
            {
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.Transparent,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            flowPanel.Controls.Add(cpuLabel);
            flowPanel.Controls.Add(gpuLabel);

            // Click-through özelliğine sahip tıklanamaz form
            overlay = new TransparentClickThroughForm();
            overlay.FormBorderStyle = FormBorderStyle.None;
            overlay.ShowInTaskbar = false;
            overlay.TopMost = true; // Başlangıçta ayarla
            overlay.StartPosition = FormStartPosition.Manual;
            overlay.BackColor = Color.Black;
            overlay.TransparencyKey = overlay.BackColor;
            overlay.AutoSize = true; // Paneli içerecek şekilde boyutlanacak
            overlay.Padding = new Padding(0);
            overlay.Margin = new Padding(0);
            overlay.Opacity = 1.0; // Başlangıçta tam görünür
            overlay.Controls.Add(flowPanel);

            // Overlay Handle oluşturulduktan sonra TopMost ayarını pekiştir
            overlay.HandleCreated += OnOverlayHandleCreated;
            overlay.Shown += OnOverlayShown;
            overlay.FormClosing += OnOverlayClosing; // Kapatma olayını yakala

            // System Tray Icon ve Menüsü
            SetupTrayIcon();

            // Sıcaklığı güncelleyen metot
            Action updateTemperatureDisplay = () =>
            {
                if (computer == null || overlay == null) return;
                try
                {
                    computer.Accept(updateVisitor);

                    float cpuTemperature = GetCpuTemperature(computer);
                    UpdateLabel(cpuLabel, "C", cpuTemperature);

                    float gpuTemperature = GetGpuTemperature(computer);
                    UpdateLabel(gpuLabel, "G", gpuTemperature);

                    PositionOverlay(overlay, cpuLabel, gpuLabel);
                }
                catch
                {
                    cpuLabel.Visible = false;
                    gpuLabel.Visible = false;
                    if (overlay != null)
                        PositionOverlay(overlay, cpuLabel, gpuLabel);
                }
            };

            // Timer event - sıcaklık güncelleme
            updateTimer = new System.Windows.Forms.Timer();
            updateTimer.Interval = SHORT_INTERVAL;
            updateTimer.Tick += (sender, e) => updateTemperatureDisplay();

            // Başlangıçta hemen bir güncelleme yap (ancak konumlama Shown event'inde yapılacak)
            updateTemperatureDisplay();

            // Formu göster (Shown event'i tetiklenecek)
            if (overlay != null)
                overlay.Show();
            if (updateTimer != null)
                updateTimer.Start();

            // Ana mesaj döngüsünü başlat (artık ApplicationContext kullanmıyoruz, Run() yeterli)
            Application.Run();

            // Uygulama kapatılırken computer kaynağını serbest bırak
            // Console.WriteLine("Uygulama kapatılıyor...");
            if (computer != null)
                computer.Close();
            // FreeConsole(); // Konsolu açtıysak kapat
        }

        // System Tray Icon ve Menüsünü Ayarlama
        private static void SetupTrayIcon()
        {
            var contextMenu = new ContextMenuStrip();

            // Çalışıyor etiketi (tıklanamaz)
            var statusLabel = new ToolStripMenuItem("Thermal Çalışıyor");
            statusLabel.Enabled = false;
            contextMenu.Items.Add(statusLabel);

            // Ayırıcı
            contextMenu.Items.Add(new ToolStripSeparator());

            // Otomatik Gizle
            var autoHideMenuItem = new ToolStripMenuItem("Otomatik Gizle") { CheckOnClick = true, Checked = autoHideEnabled };
            autoHideMenuItem.CheckedChanged += AutoHideMenuItem_CheckedChanged;

            // Çıkış butonu
            var exitMenuItem = new ToolStripMenuItem("Çıkış");
            exitMenuItem.Click += ExitApplication;
            contextMenu.Items.AddRange(new ToolStripItem[] { statusLabel, autoHideMenuItem, new ToolStripSeparator(), exitMenuItem });

            // NotifyIcon oluşturma
            notifyIcon = new NotifyIcon();
            notifyIcon.ContextMenuStrip = contextMenu;
            notifyIcon.Text = "Thermal Monitor"; // Fare üzerine gelince çıkan yazı

            // İkon ayarı (Varsayılan uygulama ikonu)
            // Özel bir ikon için: notifyIcon.Icon = new Icon("path/to/your/icon.ico");
            try
            {
                notifyIcon.Icon = SystemIcons.Application;
                // Alternatif: notifyIcon.Icon = SystemIcons.Information; 
            }
            catch { }

            notifyIcon.Visible = true;
        }

        // Otomatik Gizle Menü Olayı
        private static void AutoHideMenuItem_CheckedChanged(object? sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem item)
            {
                autoHideEnabled = item.Checked;
                Console.WriteLine($"Otomatik Gizle: {autoHideEnabled}");

                // Tüm gizleme timerlarını durdur
                StopAndDisposeTimer(ref initialHideTimer);
                StopAndDisposeTimer(ref reHideTimer);
                StopAndDisposeTimer(ref mouseLeaveHideTimer);

                if (autoHideEnabled)
                {
                    // Eğer görünürse, yüksek sıcaklık yoksa ve fare bölgede değilse, gizleme timer'ını başlat
                    if (isOverlayVisible && !isHighTemperature && !mouseIsInHotZone)
                    {
                        Console.WriteLine("Otomatik gizle açıldı, initial hide timer başlatılıyor.");
                        StartInitialHideTimer();
                    }
                    // Eğer zaten gizliyse ve yüksek sıcaklık yoksa, interval'i uzun yap
                    else if (!isOverlayVisible && !isHighTemperature)
                    {
                        Console.WriteLine("Otomatik gizle açıldı, zaten gizli, interval LONG yapılıyor.");
                        SetUpdateInterval(LONG_INTERVAL);
                    }
                    // Görünür durumda (yüksek sıcaklık veya fare nedeniyle) interval kısa kalmalı
                    else
                    {
                        Console.WriteLine("Otomatik gizle açıldı, görünür durumda (yüksek sıcaklık/fare), interval SHORT kalıyor.");
                        SetUpdateInterval(SHORT_INTERVAL);
                    }
                }
                else // Otomatik Gizle Kapatıldı
                {
                    Console.WriteLine("Otomatik gizle kapatıldı, görünür yapılıyor, interval SHORT yapılıyor.");
                    FadeIn();
                    SetUpdateInterval(SHORT_INTERVAL);
                }
            }
        }

        // Ana Sıcaklık Güncelleme Mantığı
        private static void UpdateTemperatureDisplay()
        {
            if (computer == null || overlay == null || cpuLabel == null || gpuLabel == null || updateVisitor == null) return;

            try
            {
                computer.Accept(updateVisitor);

                float cpuTemperature = GetCpuTemperature(computer);
                float gpuTemperature = GetGpuTemperature(computer);
                bool currentHighTemp = (cpuTemperature >= 70 || (gpuTemperature >= 70 && gpuLabel.Visible));
                Console.WriteLine($" Tick - CPU: {cpuTemperature:F0}, GPU: {gpuTemperature:F0}, HighTemp: {currentHighTemp}, AutoHide: {autoHideEnabled}, Visible: {isOverlayVisible}, Fading: {isFading}, MouseIn: {mouseIsInHotZone}");

                UpdateLabel(cpuLabel, "C", cpuTemperature);
                UpdateLabel(gpuLabel, "G", gpuTemperature);

                if (currentHighTemp)
                {
                    if (!isHighTemperature)
                    {
                        Console.WriteLine(" Yüksek Sıcaklık Algılandı! Gösteriliyor...");
                        isHighTemperature = true;
                        StopAllHideTimers();
                        FadeIn();
                        SetUpdateInterval(SHORT_INTERVAL);
                    }
                }
                else
                {
                    if (isHighTemperature)
                    {
                        Console.WriteLine(" Yüksek Sıcaklık Düştü.");
                        isHighTemperature = false;
                        if (autoHideEnabled && !mouseIsInHotZone)
                        {
                            Console.WriteLine("  -> Re-Hide timer başlatılıyor.");
                            StartReHideTimer();
                        }
                    }
                    // Yüksek sıcaklık yoksa, interval'i güncelle
                    else if (autoHideEnabled && !isOverlayVisible && !isFading)
                    {
                        if (updateTimer?.Interval != LONG_INTERVAL)
                            Console.WriteLine("  -> Yüksek sıcaklık yok, gizli & fade olmuyor, interval LONG yapılıyor.");
                        SetUpdateInterval(LONG_INTERVAL);
                    }
                    else if (!autoHideEnabled || (isOverlayVisible && !isFading)) // Oto gizle kapalı veya görünür & fade olmuyorsa
                    {
                        if (updateTimer?.Interval != SHORT_INTERVAL)
                            Console.WriteLine("  -> Yüksek sıcaklık yok, görünür & fade olmuyor, interval SHORT yapılıyor.");
                        SetUpdateInterval(SHORT_INTERVAL);
                    }
                }

                PositionOverlay(overlay, cpuLabel, gpuLabel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Güncelleme hatası: {ex.Message}");
                cpuLabel.Visible = false;
                gpuLabel.Visible = false;
                PositionOverlay(overlay, cpuLabel, gpuLabel);
            }
        }

        // Fare Konumu Kontrolü
        private static void MouseCheckTimer_Tick(object? sender, EventArgs e)
        {
            if (overlay == null || !autoHideEnabled || isHighTemperature || isFading || !overlay.IsHandleCreated)
            {
                if (mouseIsInHotZone)
                {
                    mouseIsInHotZone = false;
                    // Console.WriteLine("Fare kontrolü devre dışı/beklemede, bölgeden çıkıldı sayılıyor, hide timer başlatılıyor.");
                    if (!isHighTemperature) StartMouseLeaveHideTimer(); // Yüksek sıcaklık yoksa gizle
                }
                return;
            }

            try
            {
                Point cursorPos = Cursor.Position;
                bool currentlyInHotZone = hotZone.Contains(cursorPos);

                if (currentlyInHotZone && !mouseIsInHotZone)
                {
                    Console.WriteLine("Fare sıcak bölgeye girdi. Gösteriliyor...");
                    mouseIsInHotZone = true;
                    StopAllHideTimers();
                    FadeIn();
                    SetUpdateInterval(SHORT_INTERVAL);
                }
                else if (!currentlyInHotZone && mouseIsInHotZone)
                {
                    Console.WriteLine("Fare sıcak bölgeden çıktı. Hide timer başlatılıyor...");
                    mouseIsInHotZone = false;
                    StartMouseLeaveHideTimer();
                }
            }
            catch (Exception ex) // Nadiren Cursor.Position hata verebilir
            {
                Console.WriteLine($"Mouse check hatası: {ex.Message}");
            }
        }

        // Solma Efekti Zamanlayıcısı
        private static void FadeTimer_Tick(object? sender, EventArgs e)
        {
            if (overlay == null || fadeTimer == null || !overlay.IsHandleCreated) return;

            try
            {
                if (!isFading) // Eğer bir şekilde fade işlemi bitmiş ama timer durmamışsa
                {
                    Console.WriteLine("Fade Tick: isFading false, timer durduruluyor.");
                    fadeTimer.Stop();
                    isOverlayVisible = overlay.Opacity > 0.0;
                    return;
                }

                if (isFadingIn) // Fade In işlemi
                {
                    overlay.Opacity += FADE_STEP;
                    if (overlay.Opacity >= 1.0)
                    {
                        overlay.Opacity = 1.0;
                        fadeTimer.Stop();
                        isFading = false;
                        isOverlayVisible = true;
                        Console.WriteLine(" Fade In tamamlandı.");
                    }
                }
                else // Fade Out işlemi
                {
                    overlay.Opacity -= FADE_STEP;
                    if (overlay.Opacity <= 0.0)
                    {
                        overlay.Opacity = 0.0;
                        fadeTimer.Stop();
                        overlay.Hide();
                        isFading = false;
                        isOverlayVisible = false;
                        Console.WriteLine(" Fade Out tamamlandı.");
                        if (autoHideEnabled && !isHighTemperature)
                        {
                            Console.WriteLine("  -> Fade out sonrası interval LONG yapılıyor.");
                            SetUpdateInterval(LONG_INTERVAL);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fade timer hatası: {ex.Message}");
                StopAndDisposeTimer(ref fadeTimer);
                isFading = false;
                if (overlay != null) overlay.Opacity = isOverlayVisible ? 1.0 : 0.0;
            }
        }

        // Fade In Başlatma
        private static void FadeIn()
        {
            if (overlay == null || cpuLabel == null || gpuLabel == null) return;
            // if (isFading && isFadingIn) { /*Console.WriteLine("FadeIn: Zaten Fade In oluyor, işlem yok.");*/ return; } // isFadingIn kontrolü daha doğru
            if (!isFading && isOverlayVisible) { /*Console.WriteLine("FadeIn: Zaten görünür, işlem yok.");*/ return; }

            Console.WriteLine("Fade In Tetiklendi.");
            StopAndDisposeTimer(ref fadeTimer);
            fadeTimer = new System.Windows.Forms.Timer { Interval = FADE_INTERVAL };
            fadeTimer.Tick += FadeTimer_Tick;

            isFading = true;
            isFadingIn = true; // Yönü belirt

            if (!overlay.Visible || overlay.Opacity < 1.0)
            {
                overlay.Opacity = Math.Max(0.0, overlay.Opacity);
                overlay.Show();
                PositionOverlay(overlay, cpuLabel, gpuLabel);
                if (overlay.IsHandleCreated) SetWindowPos(overlay.Handle, HWND_TOPMOST, 0, 0, 0, 0, TOPMOST_FLAGS);
            }
            fadeTimer.Start();
        }

        // Fade Out Başlatma
        private static void FadeOut()
        {
            if (overlay == null) return;
            // if (isFading && !isFadingIn) { /*Console.WriteLine("FadeOut: Zaten Fade Out oluyor, işlem yok.");*/ return; } // isFadingIn kontrolü daha doğru
            if (!isFading && !isOverlayVisible) { /*Console.WriteLine("FadeOut: Zaten gizli, işlem yok.");*/ return; }
            if (isHighTemperature) { Console.WriteLine("FadeOut: Yüksek sıcaklık nedeniyle engellendi."); return; }
            if (mouseIsInHotZone) { Console.WriteLine("FadeOut: Fare sıcak bölgede olduğu için engellendi."); return; }

            Console.WriteLine("Fade Out Tetiklendi.");
            StopAndDisposeTimer(ref fadeTimer);
            fadeTimer = new System.Windows.Forms.Timer { Interval = FADE_INTERVAL };
            fadeTimer.Tick += FadeTimer_Tick;

            isFading = true;
            isFadingIn = false; // Yönü belirt
            fadeTimer.Start();
        }

        // Gizleme Timerlarını Başlatma
        private static void StartInitialHideTimer()
        {
            StopAllHideTimers();
            initialHideTimer = new System.Windows.Forms.Timer { Interval = HIDE_DELAY };
            initialHideTimer.Tick += (s, e) =>
            {
                Console.WriteLine("Initial Hide Timer Tick - FadeOut çağrılıyor.");
                StopAndDisposeTimer(ref initialHideTimer);
                FadeOut();
            };
            Console.WriteLine("Initial Hide Timer başlatıldı.");
            initialHideTimer.Start();
        }
        private static void StartReHideTimer()
        {
            StopAllHideTimers();
            reHideTimer = new System.Windows.Forms.Timer { Interval = HIDE_DELAY };
            reHideTimer.Tick += (s, e) =>
            {
                Console.WriteLine("Re-Hide Timer Tick - FadeOut çağrılıyor.");
                StopAndDisposeTimer(ref reHideTimer);
                FadeOut();
            };
            Console.WriteLine("Re-Hide Timer başlatıldı.");
            reHideTimer.Start();
        }
        private static void StartMouseLeaveHideTimer()
        {
            StopAllHideTimers();
            mouseLeaveHideTimer = new System.Windows.Forms.Timer { Interval = HIDE_DELAY };
            mouseLeaveHideTimer.Tick += (s, e) =>
            {
                Console.WriteLine("Mouse Leave Hide Timer Tick - FadeOut çağrılıyor.");
                StopAndDisposeTimer(ref mouseLeaveHideTimer);
                FadeOut();
            };
            Console.WriteLine("Mouse Leave Hide Timer başlatıldı.");
            mouseLeaveHideTimer.Start();
        }
        private static void StopAllHideTimers()
        {
            StopAndDisposeTimer(ref initialHideTimer);
            StopAndDisposeTimer(ref reHideTimer);
            StopAndDisposeTimer(ref mouseLeaveHideTimer);
        }

        // Timer Durdurma ve Dispose Etme Yardımcısı
        private static void StopAndDisposeTimer(ref System.Windows.Forms.Timer? timerInstance)
        {
            if (timerInstance != null)
            {
                bool wasFadeTimer = (timerInstance == fadeTimer);
                timerInstance.Stop();
                timerInstance.Dispose();
                timerInstance = null;
                if (wasFadeTimer) isFading = false; // Fade timer durduğunda isFading'i sıfırla
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

        // Overlay Olayları
        private static void OnOverlayHandleCreated(object? sender, EventArgs e)
        {
            if (overlay != null && overlay.IsHandleCreated)
                SetWindowPos(overlay.Handle, HWND_TOPMOST, 0, 0, 0, 0, TOPMOST_FLAGS);
        }
        private static void OnOverlayShown(object? sender, EventArgs e)
        {
            if (overlay == null || cpuLabel == null || gpuLabel == null) return;
            PositionOverlay(overlay, cpuLabel, gpuLabel);
            hotZone = new Rectangle(overlay.Left - 10, overlay.Top - 5, overlay.Width + 20, overlay.Height + 10);
            Console.WriteLine($"Hot Zone Ayarlandı: {hotZone}");
            overlay.TopMost = true;
            if (overlay.IsHandleCreated) SetWindowPos(overlay.Handle, HWND_TOPMOST, 0, 0, 0, 0, TOPMOST_FLAGS);

            if (autoHideEnabled)
            {
                Console.WriteLine("Overlay gösterildi, otomatik gizle açık, initial hide timer başlatılıyor.");
                StartInitialHideTimer();
            }
        }
        private static void OnOverlayClosing(object? sender, FormClosingEventArgs e)
        {
            Console.WriteLine("Overlay kapanıyor (OnOverlayClosing)...");
        }

        // Uygulamadan çıkış
        private static void ExitApplication(object? sender, EventArgs e)
        {
            Console.WriteLine("ExitApplication çağrıldı.");
            try
            {
                StopAllHideTimers();
                StopAndDisposeTimer(ref updateTimer);
                StopAndDisposeTimer(ref fadeTimer);
                StopAndDisposeTimer(ref mouseCheckTimer);

                computer?.Close();
                notifyIcon?.Dispose();
                overlay?.Close();
                Console.WriteLine("Kaynaklar serbest bırakıldı.");
            }
            catch (Exception ex) { Console.WriteLine($"Çıkış sırasında hata: {ex.Message}"); }
            finally
            {
                Console.WriteLine("Application.Exit() çağrılıyor.");
                Application.Exit();
                FreeConsole(); // Konsolu kapat
            }
        }

        // Overlay Konumlandırma
        private static void PositionOverlay(Form overlay, Label cpuLabel, Label gpuLabel)
        {
            try
            {
                int totalWidth = 0;
                bool cpuVisible = cpuLabel.Visible;
                bool gpuVisible = gpuLabel.Visible;

                if (cpuVisible) totalWidth += cpuLabel.Width;
                if (gpuVisible) totalWidth += gpuLabel.Width;
                if (cpuVisible && gpuVisible) totalWidth += 3;
                if (totalWidth == 0) totalWidth = 10;

                var screen = Screen.PrimaryScreen;
                if (screen != null)
                {
                    overlay.Location = new Point(screen.WorkingArea.Width - totalWidth - 5, 5);
                    // Hot zone'u da güncelle (konum değişirse diye)
                    if (overlay.IsHandleCreated) // Handle oluşmadan Width/Height 0 olabilir
                        hotZone = new Rectangle(overlay.Left - 10, overlay.Top - 5, overlay.Width + 20, overlay.Height + 10);
                }
                else
                {
                    overlay.Location = new Point(1000, 5);
                }
            }
            catch
            {
                // Hata durumunda bir şey yapma (loglama kapalı)
            }
        }

        // Label Güncelleme
        private static void UpdateLabel(Label label, string prefix, float temperature)
        {
            if (label == null) return;
            // Sıcaklık 10 dereceden düşükse label'ı gizle
            if (temperature < 10)
            {
                label.Visible = false;
                return; // İşlemi bitir
            }

            // Sıcaklık geçerliyse label'ı görünür yap ve güncelle
            label.Visible = true;
            label.Text = $"{prefix}: {temperature:F0}°C"; // Ondalık kısmı kaldırdık

            // Sıcaklığa göre rengi ayarla
            if (temperature < 50)
            {
                label.ForeColor = Color.LimeGreen;
            }
            else if (temperature < 70)
            {
                label.ForeColor = Color.Yellow;
            }
            else
            {
                label.ForeColor = Color.Red;
            }
        }

        // CPU sıcaklığını alma metodu
        private static float GetCpuTemperature(Computer computer)
        {
            float packageTemp = 0;
            float coreMaxTemp = 0;
            float highestCoreTemp = 0;
            bool packageFound = false;
            bool coreMaxFound = false;

            IHardware? cpu = computer.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.Cpu);
            if (cpu == null) return 0;

            foreach (var sensor in cpu.Sensors)
            {
                if (sensor.SensorType == SensorType.Temperature && sensor.Value.HasValue)
                {
                    if (sensor.Name.Contains("Package"))
                    {
                        packageTemp = sensor.Value.Value;
                        packageFound = true;
                        break; // Package öncelikli, bulduysak devam etmeye gerek yok
                    }
                    else if (sensor.Name.Contains("Core Max"))
                    {
                        coreMaxTemp = Math.Max(coreMaxTemp, sensor.Value.Value);
                        coreMaxFound = true;
                    }
                    else if (sensor.Name.Contains("Core") && !sensor.Name.Contains("Distance"))
                    {
                        highestCoreTemp = Math.Max(highestCoreTemp, sensor.Value.Value);
                    }
                }
            }

            if (packageFound) return packageTemp;
            if (coreMaxFound) return coreMaxTemp;
            return highestCoreTemp;
        }

        // Aktif GPU sıcaklığını alma metodu
        private static float GetGpuTemperature(Computer computer)
        {
            // Console.WriteLine("GPU Sıcaklığı aranıyor...");
            IHardware? activeGpu = null;
            float highestTemp = 0;
            // string activeGpuType = "Yok";

            // Tüm GPU donanımlarını bul
            var gpus = computer.Hardware.Where(h =>
                h.HardwareType == HardwareType.GpuNvidia ||
                h.HardwareType == HardwareType.GpuAmd ||
                h.HardwareType == HardwareType.GpuIntel).ToList();

            if (!gpus.Any())
            {
                // Console.WriteLine(" Hiç GPU donanımı bulunamadı.");
                return 0;
            }

            // Console.WriteLine($" Bulunan GPU sayısı: {gpus.Count}");
            // foreach(var gpu in gpus) { Console.WriteLine($"  - GPU: {gpu.Name} ({gpu.HardwareType})"); }

            // 1. Nvidia GPU'yu dene
            IHardware? nvidiaGpu = gpus.FirstOrDefault(h => h.HardwareType == HardwareType.GpuNvidia);
            if (nvidiaGpu != null)
            {
                // Console.WriteLine($" Nvidia GPU ({nvidiaGpu.Name}) kontrol ediliyor...");
                highestTemp = GetGpuTempFromHardware(nvidiaGpu);
                // Console.WriteLine($"  -> Nvidia Sıcaklık: {highestTemp:F1}°C");
                if (highestTemp > 0)
                {
                    activeGpu = nvidiaGpu;
                    // activeGpuType = "Nvidia";
                }
            }
            // else { Console.WriteLine(" Nvidia GPU bulunamadı."); }

            // 2. Nvidia aktif değilse veya yoksa, Intel GPU'yu dene
            if (activeGpu == null)
            {
                IHardware? intelGpu = gpus.FirstOrDefault(h => h.HardwareType == HardwareType.GpuIntel);
                if (intelGpu != null)
                {
                    // Console.WriteLine($" Intel GPU ({intelGpu.Name}) kontrol ediliyor...");
                    float intelTemp = GetGpuTempFromHardware(intelGpu);
                    // Console.WriteLine($"  -> Intel Sıcaklık: {intelTemp:F1}°C");
                    if (intelTemp > 0)
                    {
                        activeGpu = intelGpu;
                        highestTemp = intelTemp;
                        // activeGpuType = "Intel";
                    }
                }
                // else { Console.WriteLine(" Intel GPU bulunamadı."); }
            }

            // 3. Hala aktif GPU yoksa AMD'yi dene
            if (activeGpu == null)
            {
                IHardware? amdGpu = gpus.FirstOrDefault(h => h.HardwareType == HardwareType.GpuAmd);
                if (amdGpu != null)
                {
                    // Console.WriteLine($" AMD GPU ({amdGpu.Name}) kontrol ediliyor...");
                    float amdTemp = GetGpuTempFromHardware(amdGpu);
                    // Console.WriteLine($"  -> AMD Sıcaklık: {amdTemp:F1}°C");
                    if (amdTemp > 0)
                    {
                        activeGpu = amdGpu;
                        highestTemp = amdTemp;
                        // activeGpuType = "AMD";
                    }
                }
                // else { Console.WriteLine(" AMD GPU bulunamadı."); }
            }

            // Console.WriteLine($" Aktif GPU Tipi: {activeGpuType}");
            return highestTemp; // Bulunan en yüksek sıcaklığı veya 0 döndür
        }

        // Belirli bir GPU donanımından sıcaklık alma yardımcısı
        private static float GetGpuTempFromHardware(IHardware gpu)
        {
            if (gpu == null) return 0;
            // Console.WriteLine($"  GetGpuTempFromHardware çalışıyor: {gpu.Name}");

            float coreTemp = 0;
            float hotSpotTemp = 0;
            float genericTemp = 0; // Genel bir sıcaklık sensörü için
            bool coreFound = false;
            bool hotSpotFound = false;
            bool genericFound = false;

            foreach (var sensor in gpu.Sensors)
            {
                if (sensor.SensorType == SensorType.Temperature && sensor.Value.HasValue)
                {
                    // Console.WriteLine($"   - Sensör: {sensor.Name}, Değer: {sensor.Value.Value:F1}°C");
                    // Intel GPU'lar genellikle sadece 'GPU Temperature' gibi basit bir ada sahip olabilir
                    if (gpu.HardwareType == HardwareType.GpuIntel && sensor.Name.Equals("GPU Temperature", StringComparison.OrdinalIgnoreCase))
                    {
                        genericTemp = Math.Max(genericTemp, sensor.Value.Value);
                        genericFound = true;
                        // Console.WriteLine("    * Intel Genel Sıcaklık bulundu.");
                    }
                    else if (sensor.Name.Contains("GPU Core", StringComparison.OrdinalIgnoreCase))
                    {
                        coreTemp = Math.Max(coreTemp, sensor.Value.Value);
                        coreFound = true;
                        // Console.WriteLine("    * Core Sıcaklık bulundu.");
                    }
                    else if (sensor.Name.Contains("Hot Spot", StringComparison.OrdinalIgnoreCase) || sensor.Name.Contains("Junction", StringComparison.OrdinalIgnoreCase))
                    {
                        hotSpotTemp = Math.Max(hotSpotTemp, sensor.Value.Value);
                        hotSpotFound = true;
                        // Console.WriteLine("    * Hot Spot/Junction Sıcaklık bulundu.");
                    }
                    // Eğer belirli isimler yoksa, ilk bulunan sıcaklık sensörünü dene (fallback)
                    else if (!coreFound && !hotSpotFound && !genericFound)
                    {
                        genericTemp = Math.Max(genericTemp, sensor.Value.Value);
                        genericFound = true;
                        // Console.WriteLine("    * Fallback Genel Sıcaklık bulundu.");
                    }
                }
            }

            // Öncelik: Hot Spot > Core > Intel Generic > Fallback Generic
            if (hotSpotFound && hotSpotTemp > 0) { /*Console.WriteLine("  -> Hot Spot döndürülüyor.");*/ return hotSpotTemp; }
            if (coreFound && coreTemp > 0) { /*Console.WriteLine("  -> Core döndürülüyor.");*/ return coreTemp; }
            if (genericFound && genericTemp > 0) { /*Console.WriteLine("  -> Genel Sıcaklık döndürülüyor.");*/ return genericTemp; }

            // Console.WriteLine("  -> Sıcaklık bulunamadı (0 döndürülüyor).");
            return 0; // Sıcaklık bulunamadı
        }

        // Konsol penceresini açmak ve kapatmak için Win32 API'leri (isteğe bağlı)
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool FreeConsole();
    }

    // Click-through form sınıfı
    public class TransparentClickThroughForm : Form
    {
        private const int WS_EX_LAYERED = 0x80000;
        private const int WS_EX_TRANSPARENT = 0x20;

        public TransparentClickThroughForm()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= WS_EX_LAYERED | WS_EX_TRANSPARENT;
                return cp;
            }
        }
    }

    // Donanım monitörü için gerekli sınıf
    public class UpdateVisitor : IVisitor
    {
        public void VisitComputer(IComputer computer)
        {
            computer.Traverse(this);
        }

        public void VisitHardware(IHardware hardware)
        {
            hardware.Update();
            foreach (var subHardware in hardware.SubHardware)
            {
                subHardware.Accept(this);
            }
        }

        public void VisitSensor(ISensor sensor) { }
        public void VisitParameter(IParameter parameter) { }
    }
}
