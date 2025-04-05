using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Thermal
{
    internal class OverlayWindow : IDisposable
    {
        // Win32 API'ları
        private const int WS_EX_LAYERED = 0x80000;
        private const int WS_EX_TRANSPARENT = 0x20;
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOACTIVATE = 0x0010;
        private const uint TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        private readonly Form overlayForm;
        private readonly Label cpuLabel;
        private readonly Label gpuLabel;
        private readonly FlowLayoutPanel flowPanel;
        private System.Windows.Forms.Timer? fadeTimer;
        private bool isFading = false;
        private bool isFadingIn = false;
        private bool isVisible = true;
        private Rectangle hotZone = Rectangle.Empty;

        // Renk Ayarları (Varsayılan)
        private float tempThreshold1 = 50.0f;
        private Color colorLowTemp = Color.LimeGreen;
        private float tempThreshold2 = 70.0f;
        private Color colorMidTemp = Color.Yellow;
        private Color colorHighTemp = Color.Red;

        private const double FADE_STEP = 0.10;
        private const int FADE_INTERVAL = 50;

        public event EventHandler? Shown; // Form gösterildiğinde tetiklenecek event

        public bool IsVisible => isVisible;
        public bool IsFading => isFading;
        public bool IsFadingIn => isFadingIn;
        public Rectangle HotZone => hotZone;
        public bool IsHandleCreated => overlayForm.IsHandleCreated;
        public IntPtr Handle => overlayForm.Handle;
        public double CurrentOpacity => overlayForm.IsDisposed ? 0 : overlayForm.Opacity;

        public OverlayWindow()
        {
            cpuLabel = new Label { AutoSize = true, BackColor = Color.Transparent, ForeColor = Color.White, Font = new Font("Arial", 10, FontStyle.Bold), Text = "C: --°C", Padding = new Padding(3, 5, 3, 5), Margin = new Padding(0), TextAlign = ContentAlignment.MiddleCenter, Visible = false };
            gpuLabel = new Label { AutoSize = true, BackColor = Color.Transparent, ForeColor = Color.White, Font = new Font("Arial", 10, FontStyle.Bold), Text = "G: --°C", Padding = new Padding(3, 5, 5, 5), Margin = new Padding(0), TextAlign = ContentAlignment.MiddleCenter, Visible = false };
            flowPanel = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.LeftToRight, BackColor = Color.Transparent, Margin = new Padding(0), Padding = new Padding(0) };
            flowPanel.Controls.Add(cpuLabel);
            flowPanel.Controls.Add(gpuLabel);

            overlayForm = new TransparentClickThroughForm();
            overlayForm.FormBorderStyle = FormBorderStyle.None;
            overlayForm.ShowInTaskbar = false;
            overlayForm.TopMost = true;
            overlayForm.StartPosition = FormStartPosition.Manual;
            overlayForm.BackColor = Color.Black;
            overlayForm.TransparencyKey = overlayForm.BackColor;
            overlayForm.AutoSize = true;
            overlayForm.Padding = new Padding(0);
            overlayForm.Margin = new Padding(0);
            overlayForm.Opacity = 1.0;
            overlayForm.Controls.Add(flowPanel);

            overlayForm.HandleCreated += OnHandleCreated;
            overlayForm.Shown += OnFormShown;
        }

        private void OnHandleCreated(object? sender, EventArgs e)
        {
            if (overlayForm.IsHandleCreated)
                SetWindowPos(overlayForm.Handle, HWND_TOPMOST, 0, 0, 0, 0, TOPMOST_FLAGS);
        }

        private void OnFormShown(object? sender, EventArgs e)
        {
            PositionOverlay(); // İlk konumlandırmayı yap
            SetTopMost(); // Tekrar en üste al
            Shown?.Invoke(this, EventArgs.Empty); // Formun gösterildiğini bildir
        }

        public void Show()
        {
            if (!overlayForm.IsDisposed)
                overlayForm.Show();
        }

        public void HideOverlay()
        {
            if (!overlayForm.IsDisposed)
            {
                overlayForm.Hide();
                isVisible = false;
            }
        }

        public void ApplyColorSettings(float threshold1, Color lowColor, float threshold2, Color midColor, Color highColor)
        {
            tempThreshold1 = threshold1;
            colorLowTemp = lowColor;
            tempThreshold2 = threshold2;
            colorMidTemp = midColor;
            colorHighTemp = highColor;
            Console.WriteLine("OverlayWindow: Renk ayarları uygulandı.");
            // Mevcut labelların rengini hemen güncellemek için UpdateLabel'ı tekrar çağırmak gerekebilir,
            // ancak bir sonraki tick'te zaten güncellenecek.
        }

        public void UpdateLabel(string labelType, float temperature)
        {
            Label? targetLabel = labelType.Equals("CPU", StringComparison.OrdinalIgnoreCase) ? cpuLabel : gpuLabel;
            if (targetLabel == null) return;

            if (temperature < 10)
            {
                targetLabel.Visible = false;
                return;
            }

            targetLabel.Visible = true;
            targetLabel.Text = $"{(labelType == "CPU" ? "C" : "G")}: {temperature:F0}°C";

            if (temperature < tempThreshold1) targetLabel.ForeColor = colorLowTemp;
            else if (temperature < tempThreshold2) targetLabel.ForeColor = colorMidTemp;
            else targetLabel.ForeColor = colorHighTemp;
        }

        public void PositionOverlay()
        {
            if (overlayForm.IsDisposed) return;
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
                    overlayForm.Location = new Point(screen.WorkingArea.Width - totalWidth - 5, 5);
                    if (overlayForm.IsHandleCreated)
                        hotZone = new Rectangle(overlayForm.Left - 10, overlayForm.Top - 5, overlayForm.Width + 20, overlayForm.Height + 10);
                }
                else
                {
                    overlayForm.Location = new Point(1000, 5);
                }
            }
            catch (Exception ex)
            { Console.WriteLine($"OverlayWindow: Konumlandırma hatası: {ex.Message}"); }
        }

        public void FadeIn()
        {
            if (overlayForm.IsDisposed) return;
            if ((!isFading && isVisible && overlayForm.Opacity >= 1.0) || (isFading && isFadingIn))
            {
                return;
            }

            Console.WriteLine("OverlayWindow: Sıcak bölgeye girildi/Gösteriliyor -> FadeIn Başlatılıyor.");
            StopAndDisposeFadeTimer();
            fadeTimer = new System.Windows.Forms.Timer { Interval = FADE_INTERVAL };
            fadeTimer.Tick += FadeTimer_Tick;

            isFading = true;
            isFadingIn = true;

            if (!overlayForm.Visible || overlayForm.Opacity < 1.0)
            {
                overlayForm.Opacity = Math.Max(0.0, overlayForm.Opacity);
                Show();
                PositionOverlay();
                SetTopMost();
            }
            fadeTimer.Start();
        }

        public void FadeOut()
        {
            if (overlayForm.IsDisposed) return;
            if (isFading && !isFadingIn) return;
            if (!isFading && !isVisible) return;

            Console.WriteLine("OverlayWindow: Fade Out Tetiklendi.");
            StopAndDisposeFadeTimer();
            fadeTimer = new System.Windows.Forms.Timer { Interval = FADE_INTERVAL };
            fadeTimer.Tick += FadeTimer_Tick;

            isFading = true;
            isFadingIn = false;
            fadeTimer.Start();
        }

        private void FadeTimer_Tick(object? sender, EventArgs e)
        {
            if (overlayForm.IsDisposed || fadeTimer == null) return;

            try
            {
                if (!isFading)
                {
                    StopAndDisposeFadeTimer();
                    isVisible = overlayForm.Opacity > 0.0;
                    return;
                }

                if (isFadingIn)
                {
                    overlayForm.Opacity += FADE_STEP;
                    if (overlayForm.Opacity >= 1.0)
                    {
                        overlayForm.Opacity = 1.0;
                        StopAndDisposeFadeTimer();
                        isVisible = true;
                        Console.WriteLine("OverlayWindow: Fade In tamamlandı.");
                    }
                }
                else // Fade Out
                {
                    overlayForm.Opacity -= FADE_STEP;
                    if (overlayForm.Opacity <= 0.0)
                    {
                        overlayForm.Opacity = 0.0;
                        StopAndDisposeFadeTimer();
                        HideOverlay();
                        isVisible = false;
                        Console.WriteLine("OverlayWindow: Fade Out tamamlandı.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OverlayWindow: Fade timer hatası: {ex.Message}");
                StopAndDisposeFadeTimer();
                if (!overlayForm.IsDisposed) overlayForm.Opacity = isVisible ? 1.0 : 0.0;
            }
        }

        private void StopAndDisposeFadeTimer()
        {
            if (fadeTimer != null)
            {
                fadeTimer.Stop();
                fadeTimer.Dispose();
                fadeTimer = null;
            }
            isFading = false;
        }

        public void SetTopMost()
        {
            if (overlayForm.IsHandleCreated)
            {
                overlayForm.TopMost = true;
                SetWindowPos(overlayForm.Handle, HWND_TOPMOST, 0, 0, 0, 0, TOPMOST_FLAGS);
            }
        }

        public void Dispose()
        {
            StopAndDisposeFadeTimer();
            overlayForm?.Close(); // Formu kapatır, bu da Dispose eder
            Console.WriteLine("OverlayWindow: Kapatıldı.");
        }
    }

    // Click-through form sınıfı buraya veya ayrı bir dosyaya taşınabilir.
    // Şimdilik burada kalsın.
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
}