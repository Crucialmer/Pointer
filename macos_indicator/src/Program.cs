using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace PointerMacOS
{
    static class Program
    {
        private static Mutex _appMutex;
        private static Config _config;
        private static MacCapsuleOverlay _overlay;
        private static CaretDetector _caretDetector;
        private static CursorDetector _cursorDetector;
        private static TrayIconManager _trayManager;
        private static System.Windows.Forms.Timer _loopTimer;

        private static bool _isChinese = false;
        private static int _pollTickCounter = 0;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetProcessDpiAwarenessContext(IntPtr dpiFlag);

        [DllImport("winmm.dll", EntryPoint = "timeBeginPeriod")]
        private static extern uint TimeBeginPeriod(uint uMilliseconds);

        [DllImport("winmm.dll", EntryPoint = "timeEndPeriod")]
        private static extern uint TimeEndPeriod(uint uMilliseconds);

        private static readonly IntPtr DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2 = new IntPtr(-4);

        [STAThread]
        static void Main()
        {
            // 1. 纯绿色化第一准则：强制纠正工作目录为程序所在绝对路径
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            Directory.SetCurrentDirectory(baseDir);

            // 2. 单实例互斥锁检查 (防止重复启动占用系统资源)
            bool createdNew;
            _appMutex = new Mutex(true, "PointerMacOS_SingleInstance_Mutex", out createdNew);
            if (!createdNew)
            {
                MessageBox.Show("Pointer (macOS 26 纯绿版) 已在后台运行。\n请在任务栏右下角系统托盘中查找并管理。",
                    "Pointer 提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // 3. 启用顶级 PerMonitorV2 高 DPI 缩放感知
            try
            {
                SetProcessDpiAwarenessContext(DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2);
            }
            catch { }

            // 4. 突破 Windows 默认 15.6ms 时钟中断限制，开启 1ms 硬件级高精度调度
            TimeBeginPeriod(1);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // 5. 加载纯绿色便携配置
            _config = Config.Load();

            // 6. 初始化核心模块
            _overlay = new MacCapsuleOverlay(_config);
            _caretDetector = new CaretDetector();
            _cursorDetector = new CursorDetector();

            // 7. 创建系统托盘图标
            _trayManager = new TrayIconManager(_config, () =>
            {
                // 配置发生改变时的回调
                _overlay.Dispose();
                _overlay = new MacCapsuleOverlay(_config);
            });

            // 8. 启动高刷调度定时器 (16ms ≈ 60fps 丝滑跟随)
            _loopTimer = new System.Windows.Forms.Timer();
            _loopTimer.Interval = Math.Max(10, _config.PollIntervalMs);
            _loopTimer.Tick += OnLoopTick;
            _loopTimer.Start();

            // 9. 运行主消息循环
            Application.Run();

            // 退出清理
            _loopTimer.Stop();
            _loopTimer.Dispose();
            _overlay.Dispose();
            _trayManager.Dispose();
            TimeEndPeriod(1);
            _appMutex.ReleaseMutex();
        }

        private static void OnLoopTick(object sender, EventArgs e)
        {
            _pollTickCounter++;

            // A. 每 5 次 Tick (~80ms) 轮询一次 IME 中英状态，极大降低 CPU 开销
            if (_pollTickCounter % 5 == 0)
            {
                _isChinese = ImeDetector.IsChineseMode();
            }

            // 英文模式下是否显示判定
            bool canShow = _isChinese || _config.ShowEnglish;
            bool isDarkMode = ThemeManager.IsEffectiveDarkMode(_config.ThemeMode);

            Point targetPos = Point.Empty;
            bool hasValidTarget = false;

            string track = _config.TrackMode.ToLowerInvariant();

            // B. 优先探测文本输入光标 (Caret)
            if (canShow && (track == "both" || track == "caret"))
            {
                Point caretPt;
                int caretH;
                if (_caretDetector.GetCaretPos(out caretPt, out caretH))
                {
                    targetPos = new Point(
                        caretPt.X + _config.CaretOffsetX,
                        caretPt.Y + caretH + _config.CaretOffsetY);
                    hasValidTarget = true;
                }
            }

            // C. 次级回退至鼠标指针跟随 (Mouse)
            if (canShow && !hasValidTarget && (track == "both" || track == "mouse"))
            {
                Point mousePt;
                if (_cursorDetector.IsTargetCursor(out mousePt))
                {
                    targetPos = new Point(
                        mousePt.X + _config.MouseOffsetX,
                        mousePt.Y + _config.MouseOffsetY);
                    hasValidTarget = true;
                }
            }

            // D. 驱动 macOS 晶透胶囊指示器
            _overlay.UpdateTarget(hasValidTarget && canShow, targetPos, _isChinese, isDarkMode);
        }
    }
}
