using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Thermal.Presentation
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

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern bool UpdateLayeredWindow(IntPtr hwnd, IntPtr hdcDst, ref Point pptDst, ref Size psize, IntPtr hdcSrc, ref Point pptSrc, int crKey, ref BLENDFUNCTION pblend, int dwFlags);

        [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern IntPtr CreateCompatibleDC(IntPtr hDC);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern bool DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll", ExactSpelling = true)]
        private static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern bool DeleteObject(IntPtr hObject);

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct BLENDFUNCTION
        {
            public byte BlendOp;
            public byte BlendFlags;
            public byte SourceConstantAlpha;
            public byte AlphaFormat;
        }

        private const byte AC_SRC_OVER = 0x00;
        private const byte AC_SRC_ALPHA = 0x01;
        private const int ULW_ALPHA = 0x02;

        private readonly Form overlayForm;
        private System.Windows.Forms.Timer? fadeTimer;
        private bool isFading = false;
        private bool isFadingIn = false;
        private bool isVisible = true;
        private Rectangle hotZone = Rectangle.Empty;

        // Sıcaklıklar ve Görünürlük
        private float cpuTemp = -1;
        private float gpuTemp = -1;
        private bool cpuVisible = false;
        private bool gpuVisible = false;

        // Renk Ayarları (Varsayılan)
        private float tempThreshold1 = 50.0f;
        private Color colorLowTemp = Color.LimeGreen;
        private float tempThreshold2 = 70.0f;
        private Color colorMidTemp = Color.Yellow;
        private Color colorHighTemp = Color.Red;

        private const double FADE_STEP = 0.10;
        private const int FADE_INTERVAL = 40; // Daha akıcı geçiş için 40ms yapıldı
        private double currentOpacity = 1.0;

        public event EventHandler? Shown;

        public bool IsVisible => isVisible;
        public bool IsFading => isFading;
        public bool IsFadingIn => isFadingIn;
        public Rectangle HotZone => hotZone;
        public bool IsHandleCreated => overlayForm.IsHandleCreated;
        public IntPtr Handle => overlayForm.Handle;
        public double CurrentOpacity => currentOpacity;

        public OverlayWindow()
        {
            overlayForm = new TransparentClickThroughForm();
            overlayForm.FormBorderStyle = FormBorderStyle.None;
            overlayForm.ShowInTaskbar = false;
            overlayForm.TopMost = true;
            overlayForm.StartPosition = FormStartPosition.Manual;
            overlayForm.BackColor = Color.Black; // Gerek kalmadı ama varsayılan olarak kalsın
            overlayForm.Opacity = 1.0;
            overlayForm.Padding = new Padding(0);
            overlayForm.Margin = new Padding(0);

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
            PositionOverlay();
            SetTopMost();
            Shown?.Invoke(this, EventArgs.Empty);
        }

        public void Show()
        {
            if (!overlayForm.IsDisposed)
            {
                overlayForm.Show();
                isVisible = true;
            }
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
            RenderOverlay(); // Ayarlar değişince hemen yeniden çiz
        }

        public void UpdateLabel(string labelType, float temperature)
        {
            bool isCpu = labelType.Equals("CPU", StringComparison.OrdinalIgnoreCase);
            if (temperature < 10)
            {
                if (isCpu) cpuVisible = false; else gpuVisible = false;
                return;
            }

            if (isCpu)
            {
                cpuTemp = temperature;
                cpuVisible = true;
            }
            else
            {
                gpuTemp = temperature;
                gpuVisible = true;
            }
        }

        public void PositionOverlay()
        {
            if (overlayForm.IsDisposed) return;
            try
            {
                RenderOverlay();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OverlayWindow: Konumlandırma hatası: {ex.Message}");
            }
        }

        private void RenderOverlay()
        {
            if (overlayForm.IsDisposed || !overlayForm.IsHandleCreated) return;

            using (Font font = new Font("Segoe UI", 10f, FontStyle.Bold))
            {
                string cpuText = cpuVisible ? $"C: {cpuTemp:F0}°C" : "";
                string gpuText = gpuVisible ? $"G: {gpuTemp:F0}°C" : "";

                int cpuWidth = 0;
                int gpuWidth = 0;
                int textHeight = 0;

                using (Bitmap measureBmp = new Bitmap(1, 1))
                using (Graphics measureG = Graphics.FromImage(measureBmp))
                {
                    if (cpuVisible)
                    {
                        SizeF size = measureG.MeasureString(cpuText, font);
                        cpuWidth = (int)Math.Ceiling(size.Width);
                        textHeight = Math.Max(textHeight, (int)Math.Ceiling(size.Height));
                    }
                    if (gpuVisible)
                    {
                        SizeF size = measureG.MeasureString(gpuText, font);
                        gpuWidth = (int)Math.Ceiling(size.Width);
                        textHeight = Math.Max(textHeight, (int)Math.Ceiling(size.Height));
                    }
                }

                if (!cpuVisible && !gpuVisible)
                {
                    // Her ikisi de görünür değilse boş çiz
                    UpdateWindow(new Bitmap(1, 1), 0, 0, 0f);
                    return;
                }

                int paddingX = 12;
                int paddingY = 6;
                int dotSize = 8;
                int dotGap = 6;
                int sepGap = 12;

                int width = paddingX * 2;
                if (cpuVisible)
                {
                    width += dotSize + dotGap + cpuWidth;
                }
                if (gpuVisible)
                {
                    if (cpuVisible) width += sepGap * 2 + 1;
                    width += dotSize + dotGap + gpuWidth;
                }
                int height = paddingY * 2 + textHeight;

                var screen = Screen.PrimaryScreen;
                int x = 1000;
                int y = 5;
                if (screen != null)
                {
                    x = screen.WorkingArea.Right - width - 10;
                }

                hotZone = new Rectangle(x - 10, y - 5, width + 20, height + 10);

                using (Bitmap bmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                    // Arka planı çiz (Semi-transparent dark rounded card)
                    using (GraphicsPath path = GetRoundedRectPath(new Rectangle(0, 0, width, height), 6))
                    {
                        using (Brush bgBrush = new SolidBrush(Color.FromArgb(180, 20, 20, 20)))
                        {
                            g.FillPath(bgBrush, path);
                        }
                        using (Pen borderPen = new Pen(Color.FromArgb(40, 255, 255, 255), 1.0f))
                        {
                            g.DrawPath(borderPen, path);
                        }
                    }

                    int currentX = paddingX;

                    // CPU Çizimi
                    if (cpuVisible)
                    {
                        Color cpuColor = GetTempColor(cpuTemp);
                        int dotY = paddingY + (textHeight - dotSize) / 2;
                        using (Brush dotBrush = new SolidBrush(cpuColor))
                        {
                            g.FillEllipse(dotBrush, currentX, dotY, dotSize, dotSize);
                        }
                        currentX += dotSize + dotGap;

                        using (Brush textBrush = new SolidBrush(Color.FromArgb(240, 240, 240)))
                        {
                            g.DrawString(cpuText, font, textBrush, currentX, paddingY - 1);
                        }
                        currentX += cpuWidth;
                    }

                    // Ayırıcı Çizgi
                    if (cpuVisible && gpuVisible)
                    {
                        currentX += sepGap;
                        using (Pen sepPen = new Pen(Color.FromArgb(40, 255, 255, 255), 1.0f))
                        {
                            g.DrawLine(sepPen, currentX, paddingY + 2, currentX, height - paddingY - 2);
                        }
                        currentX += sepGap + 1;
                    }

                    // GPU Çizimi
                    if (gpuVisible)
                    {
                        Color gpuColor = GetTempColor(gpuTemp);
                        int dotY = paddingY + (textHeight - dotSize) / 2;
                        using (Brush dotBrush = new SolidBrush(gpuColor))
                        {
                            g.FillEllipse(dotBrush, currentX, dotY, dotSize, dotSize);
                        }
                        currentX += dotSize + dotGap;

                        using (Brush textBrush = new SolidBrush(Color.FromArgb(240, 240, 240)))
                        {
                            g.DrawString(gpuText, font, textBrush, currentX, paddingY - 1);
                        }
                    }

                    UpdateWindow(bmp, x, y, (float)currentOpacity);
                }
            }
        }

        private Color GetTempColor(float temp)
        {
            if (temp < tempThreshold1) return colorLowTemp;
            if (temp < tempThreshold2) return colorMidTemp;
            return colorHighTemp;
        }

        private GraphicsPath GetRoundedRectPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            float diameter = radius * 2;
            RectangleF arcRect = new RectangleF(rect.Location, new SizeF(diameter, diameter));

            path.AddArc(arcRect, 180, 90);
            arcRect.X = rect.Right - diameter;
            path.AddArc(arcRect, 270, 90);
            arcRect.Y = rect.Bottom - diameter;
            path.AddArc(arcRect, 0, 90);
            arcRect.X = rect.Left;
            path.AddArc(arcRect, 90, 90);

            path.CloseFigure();
            return path;
        }

        private void UpdateWindow(Bitmap bmp, int x, int y, float opacity)
        {
            IntPtr screenDc = GetDC(IntPtr.Zero);
            IntPtr memDc = CreateCompatibleDC(screenDc);
            IntPtr hBitmap = bmp.GetHbitmap(Color.FromArgb(0));
            IntPtr oldBitmap = SelectObject(memDc, hBitmap);

            Point newLocation = new Point(x, y);
            Size newSize = bmp.Size;
            Point sourceLocation = new Point(0, 0);

            BLENDFUNCTION blend = new BLENDFUNCTION
            {
                BlendOp = AC_SRC_OVER,
                BlendFlags = 0,
                SourceConstantAlpha = (byte)(opacity * 255),
                AlphaFormat = AC_SRC_ALPHA
            };

            UpdateLayeredWindow(overlayForm.Handle, screenDc, ref newLocation, ref newSize, memDc, ref sourceLocation, 0, ref blend, ULW_ALPHA);

            SelectObject(memDc, oldBitmap);
            DeleteObject(hBitmap);
            DeleteDC(memDc);
            ReleaseDC(IntPtr.Zero, screenDc);
        }

        public void FadeIn()
        {
            if (overlayForm.IsDisposed) return;
            if ((!isFading && isVisible && currentOpacity >= 1.0) || (isFading && isFadingIn))
            {
                return;
            }

            Console.WriteLine("OverlayWindow: FadeIn Başlatılıyor.");
            StopAndDisposeFadeTimer();
            fadeTimer = new System.Windows.Forms.Timer { Interval = FADE_INTERVAL };
            fadeTimer.Tick += FadeTimer_Tick;

            isFading = true;
            isFadingIn = true;

            if (!overlayForm.Visible || currentOpacity < 1.0)
            {
                Show();
                RenderOverlay();
            }
            fadeTimer.Start();
        }

        public void FadeOut()
        {
            if (overlayForm.IsDisposed) return;
            if (isFading && !isFadingIn) return;
            if (!isFading && !isVisible) return;

            Console.WriteLine("OverlayWindow: FadeOut Başlatılıyor.");
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
                    return;
                }

                if (isFadingIn)
                {
                    currentOpacity += FADE_STEP;
                    if (currentOpacity >= 1.0)
                    {
                        currentOpacity = 1.0;
                        StopAndDisposeFadeTimer();
                        isVisible = true;
                        Console.WriteLine("OverlayWindow: Fade In tamamlandı.");
                    }
                }
                else // Fade Out
                {
                    currentOpacity -= FADE_STEP;
                    if (currentOpacity <= 0.0)
                    {
                        currentOpacity = 0.0;
                        StopAndDisposeFadeTimer();
                        HideOverlay();
                        isVisible = false;
                        Console.WriteLine("OverlayWindow: Fade Out tamamlandı.");
                    }
                }

                RenderOverlay();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OverlayWindow: Fade timer hatası: {ex.Message}");
                StopAndDisposeFadeTimer();
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
            overlayForm?.Close();
            Console.WriteLine("OverlayWindow: Kapatıldı.");
        }
    }

    public class TransparentClickThroughForm : Form
    {
        private const int WS_EX_LAYERED = 0x80000;
        private const int WS_EX_TRANSPARENT = 0x20;
        private const int WS_EX_TOOLWINDOW = 0x80;

        public TransparentClickThroughForm()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= WS_EX_LAYERED | WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW;
                return cp;
            }
        }
    }
}