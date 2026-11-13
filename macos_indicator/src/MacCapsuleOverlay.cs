using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Runtime.InteropServices;

namespace PointerMacOS
{
    public class MacCapsuleOverlay : IDisposable
    {
        private const int WS_EX_TOPMOST = 0x00000008;
        private const int WS_EX_TOOLWINDOW = 0x00000080;
        private const int WS_EX_LAYERED = 0x00080000;
        private const int WS_EX_TRANSPARENT = 0x00000020;
        private const int WS_EX_NOACTIVATE = 0x08000000;
        private const int WS_POPUP = unchecked((int)0x80000000);

        private const int SWP_NOSIZE = 0x0001;
        private const int SWP_NOMOVE = 0x0002;
        private const int SWP_NOACTIVATE = 0x0010;
        private const int SWP_SHOWWINDOW = 0x0040;
        private const int SWP_HIDEWINDOW = 0x0080;

        private const byte AC_SRC_OVER = 0x00;
        private const byte AC_SRC_ALPHA = 0x01;
        private const int ULW_ALPHA = 0x00000002;

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
            public POINT(int x, int y) { this.x = x; this.y = y; }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SIZE
        {
            public int cx;
            public int cy;
            public SIZE(int cx, int cy) { this.cx = cx; this.cy = cy; }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct BLENDFUNCTION
        {
            public byte BlendOp;
            public byte BlendFlags;
            public byte SourceConstantAlpha;
            public byte AlphaFormat;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr CreateWindowEx(
            int dwExStyle, string lpClassName, string lpWindowName, int dwStyle,
            int x, int y, int nWidth, int nHeight, IntPtr hWndParent,
            IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UpdateLayeredWindow(
            IntPtr hwnd, IntPtr hdcDst, ref POINT pptDst, ref SIZE psize,
            IntPtr hdcSrc, ref POINT pptSrc, int crKey, ref BLENDFUNCTION pblend, int dwFlags);

        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);

        private IntPtr _hwnd;
        private Config _config;
        private float _currentAlpha = 0f;
        private float _targetAlpha = 0f;
        private bool _lastChineseMode = false;
        private bool _isDisposed = false;

        private const int PADDING = 10; // 边距供软阴影和微光漫反射

        public MacCapsuleOverlay(Config config)
        {
            _config = config;
            CreateOverlayWindow();
        }

        private void CreateOverlayWindow()
        {
            int exStyle = WS_EX_LAYERED | WS_EX_TRANSPARENT | WS_EX_TOPMOST | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE;
            _hwnd = CreateWindowEx(
                exStyle, "Static", "PointerMacOS_Overlay", WS_POPUP,
                0, 0, 100, 100, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
        }

        public void UpdateTarget(bool shouldShow, Point targetPos, bool isChinese, bool isDarkMode)
        {
            if (_isDisposed) return;

            _lastChineseMode = isChinese;
            _targetAlpha = shouldShow ? 1.0f : 0.0f;

            // 呼吸式平滑过渡动效
            if (_config.SmoothAnimation)
            {
                if (Math.Abs(_currentAlpha - _targetAlpha) > 0.01f)
                {
                    _currentAlpha += (_targetAlpha - _currentAlpha) * 0.35f;
                }
                else
                {
                    _currentAlpha = _targetAlpha;
                }
            }
            else
            {
                _currentAlpha = _targetAlpha;
            }

            if (_currentAlpha < 0.02f)
            {
                SetWindowPos(_hwnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE | SWP_HIDEWINDOW);
                return;
            }

            RenderAndPresent(targetPos, isChinese, isDarkMode, _currentAlpha);
        }

        private void RenderAndPresent(Point targetPos, bool isChinese, bool isDarkMode, float alpha)
        {
            bool isBadge = string.Equals(_config.IndicatorStyle, "badge", StringComparison.OrdinalIgnoreCase);

            int coreWidth = isBadge ? _config.BadgeWidth : _config.DotSize;
            int coreHeight = isBadge ? _config.BadgeHeight : _config.DotSize;

            int totalWidth = coreWidth + PADDING * 2;
            int totalHeight = coreHeight + PADDING * 2;

            using (var bmp = new Bitmap(totalWidth, totalHeight, PixelFormat.Format32bppPArgb))
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                // 确定色彩方案
                Color baseColor;
                if (isChinese)
                {
                    baseColor = isDarkMode ? _config.DarkCnColor : _config.LightCnColor;
                }
                else
                {
                    baseColor = isDarkMode ? _config.DarkEnColor : _config.LightEnColor;
                }

                RectangleF coreRect = new RectangleF(PADDING, PADDING, coreWidth, coreHeight);

                if (isBadge)
                {
                    DrawMacBadge(g, coreRect, baseColor, isChinese, isDarkMode);
                }
                else
                {
                    DrawMacDot(g, coreRect, baseColor, isDarkMode);
                }

                // 计算屏幕绘制位置 (居中偏置)
                int screenX = targetPos.X - PADDING;
                int screenY = targetPos.Y - PADDING;

                PresentBitmap(bmp, screenX, screenY, (byte)(alpha * 255));
            }
        }

        private void DrawMacBadge(Graphics g, RectangleF rect, Color baseColor, bool isChinese, bool isDarkMode)
        {
            float radius = _config.BadgeCornerRadius;

            // 1. 绘制多阶环境柔和发光投影 (Soft Ambient Shadow)
            for (int i = 3; i >= 1; i--)
            {
                RectangleF shadowRect = new RectangleF(rect.X - i, rect.Y - i + 1, rect.Width + i * 2, rect.Height + i * 2);
                int shadowAlpha = isDarkMode ? (12 / i) : (18 / i);
                using (var path = CreateRoundRectPath(shadowRect, radius + i))
                using (var shadowBrush = new SolidBrush(Color.FromArgb(shadowAlpha, 0, 0, 0)))
                {
                    g.FillPath(shadowBrush, path);
                }
            }

            // 2. 绘制连续平滑圆角胶囊主体 (Squircle Acrylic Body)
            using (var bodyPath = CreateRoundRectPath(rect, radius))
            {
                // 微渐变 (模拟 macOS 顶部微光)
                Color topColor = Lighten(baseColor, isDarkMode ? 0.15f : 0.08f);
                Color btmColor = Darken(baseColor, 0.05f);

                using (var gradBrush = new LinearGradientBrush(rect, topColor, btmColor, LinearGradientMode.Vertical))
                {
                    g.FillPath(gradBrush, bodyPath);
                }

                // 3. 绘制 1px 晶透微光内高光边框 (Inner Highlight Border)
                int borderAlpha = isDarkMode ? 75 : 55;
                using (var pen = new Pen(Color.FromArgb(borderAlpha, 255, 255, 255), 1f))
                {
                    pen.Alignment = PenAlignment.Inset;
                    g.DrawPath(pen, bodyPath);
                }
            }

            // 4. 绘制精细字重中英徽标文字「中」/「A」
            string text = isChinese ? "中" : "A";
            using (var font = new Font("Segoe UI", _config.BadgeFontSize, FontStyle.Bold, GraphicsUnit.Pixel))
            using (var brush = new SolidBrush(Color.White))
            using (var sf = new StringFormat())
            {
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;

                // 垂直位置精调 (视觉微调居中)
                RectangleF textRect = new RectangleF(rect.X, rect.Y + 0.5f, rect.Width, rect.Height);
                g.DrawString(text, font, brush, textRect, sf);
            }
        }

        private void DrawMacDot(Graphics g, RectangleF rect, Color baseColor, bool isDarkMode)
        {
            // 1. 外部极光呼吸微光圈 (Ambient Glow Halo)
            for (int i = 3; i >= 1; i--)
            {
                RectangleF glowRect = new RectangleF(rect.X - i, rect.Y - i, rect.Width + i * 2, rect.Height + i * 2);
                int glowAlpha = (int)(baseColor.A * (0.15f / i));
                using (var brush = new SolidBrush(Color.FromArgb(glowAlpha, baseColor.R, baseColor.G, baseColor.B)))
                {
                    g.FillEllipse(brush, glowRect);
                }
            }

            // 2. 内部晶透实体圆球
            using (var brush = new SolidBrush(baseColor))
            {
                g.FillEllipse(brush, rect);
            }

            // 3. 1px 边缘钛晶高光
            using (var pen = new Pen(Color.FromArgb(90, 255, 255, 255), 1f))
            {
                g.DrawEllipse(pen, rect);
            }
        }

        private void PresentBitmap(Bitmap bmp, int x, int y, byte alpha)
        {
            IntPtr screenDc = GetDC(IntPtr.Zero);
            IntPtr memDc = CreateCompatibleDC(screenDc);
            IntPtr hBitmap = bmp.GetHbitmap(Color.FromArgb(0));
            IntPtr oldBitmap = SelectObject(memDc, hBitmap);

            try
            {
                var ptDst = new POINT(x, y);
                var size = new SIZE(bmp.Width, bmp.Height);
                var ptSrc = new POINT(0, 0);

                var blend = new BLENDFUNCTION
                {
                    BlendOp = AC_SRC_OVER,
                    BlendFlags = 0,
                    SourceConstantAlpha = alpha,
                    AlphaFormat = AC_SRC_ALPHA
                };

                UpdateLayeredWindow(
                    _hwnd, screenDc, ref ptDst, ref size,
                    memDc, ref ptSrc, 0, ref blend, ULW_ALPHA);

                SetWindowPos(_hwnd, HWND_TOPMOST, 0, 0, 0, 0,
                    SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE | SWP_SHOWWINDOW);
            }
            finally
            {
                SelectObject(memDc, oldBitmap);
                DeleteObject(hBitmap);
                DeleteDC(memDc);
                ReleaseDC(IntPtr.Zero, screenDc);
            }
        }

        private static GraphicsPath CreateRoundRectPath(RectangleF rect, float r)
        {
            var path = new GraphicsPath();
            float d = r * 2f;
            if (d > rect.Width) d = rect.Width;
            if (d > rect.Height) d = rect.Height;

            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private static Color Lighten(Color c, float factor)
        {
            int r = Math.Min(255, (int)(c.R + (255 - c.R) * factor));
            int g = Math.Min(255, (int)(c.G + (255 - c.G) * factor));
            int b = Math.Min(255, (int)(c.B + (255 - c.B) * factor));
            return Color.FromArgb(c.A, r, g, b);
        }

        private static Color Darken(Color c, float factor)
        {
            int r = Math.Max(0, (int)(c.R * (1f - factor)));
            int g = Math.Max(0, (int)(c.G * (1f - factor)));
            int b = Math.Max(0, (int)(c.B * (1f - factor)));
            return Color.FromArgb(c.A, r, g, b);
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _isDisposed = true;
                if (_hwnd != IntPtr.Zero)
                {
                    DestroyWindow(_hwnd);
                    _hwnd = IntPtr.Zero;
                }
            }
        }
    }
}
