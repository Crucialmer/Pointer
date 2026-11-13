using System;
using System.Drawing;
using System.Runtime.InteropServices;
using Accessibility;

namespace PointerMacOS
{
    public class CaretDetector
    {
        private const uint OBJID_CARET = 0xFFFFFFF8;
        private static readonly Guid IID_IAccessible = new Guid("{618736e0-3c3d-11cf-810c-00aa00389b71}");

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left, Top, Right, Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct GUITHREADINFO
        {
            public int cbSize;
            public int flags;
            public IntPtr hwndActive;
            public IntPtr hwndFocus;
            public IntPtr hwndCapture;
            public IntPtr hwndMenuOwner;
            public IntPtr hwndMoveSize;
            public IntPtr hwndCaret;
            public RECT rcCaret;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetGUIThreadInfo(uint idThread, ref GUITHREADINFO lpgui);

        [DllImport("user32.dll")]
        private static extern bool ClientToScreen(IntPtr hWnd, ref POINT lpPoint);

        [DllImport("oleacc.dll")]
        private static extern int AccessibleObjectFromWindow(
            IntPtr hwnd,
            uint dwId,
            ref Guid riid,
            [MarshalAs(UnmanagedType.Interface)] out IAccessible ppvObject);

        public bool GetCaretPos(out Point position, out int caretHeight)
        {
            position = Point.Empty;
            caretHeight = 16;

            // 策略 1: GetGUIThreadInfo (系统原生组件、记事本等)
            var gui = new GUITHREADINFO();
            gui.cbSize = Marshal.SizeOf(gui);
            if (GetGUIThreadInfo(0, ref gui))
            {
                if (gui.hwndCaret != IntPtr.Zero)
                {
                    var pt = new POINT { X = gui.rcCaret.Left, Y = gui.rcCaret.Top };
                    if (ClientToScreen(gui.hwndCaret, ref pt))
                    {
                        int h = gui.rcCaret.Bottom - gui.rcCaret.Top;
                        if (h <= 0) h = 18;
                        position = new Point(pt.X, pt.Y);
                        caretHeight = h;
                        return true;
                    }
                }
            }

            // 策略 2: MSAA OBJID_CARET (VS Code, Chromium, Electron, Office 等)
            IntPtr fore = GetForegroundWindow();
            if (fore != IntPtr.Zero)
            {
                try
                {
                    IAccessible acc = null;
                    Guid iid = IID_IAccessible;
                    int hr = AccessibleObjectFromWindow(fore, OBJID_CARET, ref iid, out acc);
                    if (hr == 0 && acc != null)
                    {
                        int x, y, w, h;
                        acc.accLocation(out x, out y, out w, out h, 0);
                        if (x != 0 || y != 0)
                        {
                            if (h <= 0) h = 18;
                            position = new Point(x + w, y);
                            caretHeight = h;
                            return true;
                        }
                    }
                }
                catch { }
            }

            return false;
        }
    }
}
