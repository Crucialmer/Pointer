using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace PointerMacOS
{
    public class CursorDetector
    {
        private const int CURSOR_SHOWING = 0x00000001;
        private const int IDC_ARROW = 32512;
        private const int IDC_IBEAM = 32513;

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct CURSORINFO
        {
            public int cbSize;
            public int flags;
            public IntPtr hCursor;
            public POINT ptScreenPos;
        }

        [DllImport("user32.dll")]
        private static extern bool GetCursorInfo(out CURSORINFO pci);

        [DllImport("user32.dll")]
        private static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);

        private IntPtr _hIBeam;
        private IntPtr _hArrow;

        public CursorDetector()
        {
            _hIBeam = LoadCursor(IntPtr.Zero, IDC_IBEAM);
            _hArrow = LoadCursor(IntPtr.Zero, IDC_ARROW);
        }

        public bool IsTargetCursor(out Point cursorPos)
        {
            cursorPos = Point.Empty;
            var ci = new CURSORINFO();
            ci.cbSize = Marshal.SizeOf(ci);

            if (GetCursorInfo(out ci))
            {
                cursorPos = new Point(ci.ptScreenPos.X, ci.ptScreenPos.Y);
                if ((ci.flags & CURSOR_SHOWING) != 0)
                {
                    // 检测是否为工字光标 (文本输入状态) 或常规鼠标指针
                    if (ci.hCursor == _hIBeam || ci.hCursor == _hArrow)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
