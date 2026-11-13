using System;
using System.Runtime.InteropServices;

namespace PointerMacOS
{
    public static class ImeDetector
    {
        private const uint WM_IME_CONTROL = 0x0283;
        private const uint IMC_GETOPENSTATUS = 0x0005;
        private const uint IMC_GETCONVERSIONMODE = 0x0001;
        private const uint IME_CMODE_NATIVE = 0x0001;

        private const uint SMTO_ABORTIFHUNG = 0x0002;

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

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetGUIThreadInfo(uint idThread, ref GUITHREADINFO lpgui);

        [DllImport("imm32.dll")]
        private static extern IntPtr ImmGetDefaultIMEWnd(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessageTimeout(
            IntPtr hWnd,
            uint Msg,
            IntPtr wParam,
            IntPtr lParam,
            uint fuFlags,
            uint uTimeout,
            out IntPtr lpdwResult);

        private static IntPtr GetFocusedWindow()
        {
            IntPtr foreHwnd = GetForegroundWindow();
            if (foreHwnd == IntPtr.Zero) return IntPtr.Zero;

            uint pid;
            uint tid = GetWindowThreadProcessId(foreHwnd, out pid);

            var gui = new GUITHREADINFO();
            gui.cbSize = Marshal.SizeOf(gui);

            if (GetGUIThreadInfo(tid, ref gui))
            {
                if (gui.hwndFocus != IntPtr.Zero)
                    return gui.hwndFocus;
                if (gui.hwndActive != IntPtr.Zero)
                    return gui.hwndActive;
            }

            return foreHwnd;
        }

        public static bool IsChineseMode()
        {
            try
            {
                IntPtr focused = GetFocusedWindow();
                if (focused == IntPtr.Zero) return false;

                IntPtr imeHwnd = ImmGetDefaultIMEWnd(focused);
                if (imeHwnd == IntPtr.Zero) return false;

                // 1. 检查 IME 是否开启
                IntPtr openResult;
                IntPtr ret = SendMessageTimeout(
                    imeHwnd,
                    WM_IME_CONTROL,
                    new IntPtr(IMC_GETOPENSTATUS),
                    IntPtr.Zero,
                    SMTO_ABORTIFHUNG,
                    200,
                    out openResult);

                if (ret == IntPtr.Zero || openResult == IntPtr.Zero)
                {
                    return false; // IME 未开启 -> 纯英文输入
                }

                // 2. 检查转化模式 (Conversion Mode 是否包含 NATIVE 标志)
                IntPtr convResult;
                ret = SendMessageTimeout(
                    imeHwnd,
                    WM_IME_CONTROL,
                    new IntPtr(IMC_GETCONVERSIONMODE),
                    IntPtr.Zero,
                    SMTO_ABORTIFHUNG,
                    200,
                    out convResult);

                if (ret != IntPtr.Zero)
                {
                    uint mode = (uint)convResult.ToInt64();
                    return (mode & IME_CMODE_NATIVE) != 0;
                }
            }
            catch { }

            return false;
        }
    }
}
