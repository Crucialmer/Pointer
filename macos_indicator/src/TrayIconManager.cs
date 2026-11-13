using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;

namespace PointerMacOS
{
    public class TrayIconManager : IDisposable
    {
        private NotifyIcon _notifyIcon;
        private Config _config;
        private Action _onConfigChanged;

        private const string RUN_REG_KEY = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string APP_REG_NAME = "PointerMacOS";

        public TrayIconManager(Config config, Action onConfigChanged)
        {
            _config = config;
            _onConfigChanged = onConfigChanged;

            _notifyIcon = new NotifyIcon
            {
                Text = "Pointer: Windows 输入法指示器 (macOS 26 纯绿色版)",
                Icon = CreateAppIcon(),
                Visible = true
            };

            BuildContextMenu();
        }

        private Icon CreateAppIcon()
        {
            // 动态生成精致的 macOS 晶透风格应用托盘图标 (16x16 / 32x32)
            using (var bmp = new Bitmap(32, 32))
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // 外部圆角底座 (Apple Azure)
                using (var path = new GraphicsPath())
                {
                    path.AddArc(2, 2, 8, 8, 180, 90);
                    path.AddArc(22, 2, 8, 8, 270, 90);
                    path.AddArc(22, 22, 8, 8, 0, 90);
                    path.AddArc(2, 22, 8, 8, 90, 90);
                    path.CloseFigure();

                    using (var brush = new LinearGradientBrush(new Rectangle(2, 2, 28, 28),
                        Color.FromArgb(255, 0, 122, 255), Color.FromArgb(255, 10, 132, 255), LinearGradientMode.Vertical))
                    {
                        g.FillPath(brush, path);
                    }

                    using (var pen = new Pen(Color.FromArgb(120, 255, 255, 255), 1.5f))
                    {
                        g.DrawPath(pen, path);
                    }
                }

                // 中间白色精工 "P" 字符
                using (var font = new Font("Segoe UI", 13, FontStyle.Bold, GraphicsUnit.Pixel))
                using (var brush = new SolidBrush(Color.White))
                using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                {
                    g.DrawString("P", font, brush, new RectangleF(0, 1, 32, 32), sf);
                }

                IntPtr hIcon = bmp.GetHicon();
                return Icon.FromHandle(hIcon);
            }
        }

        private void BuildContextMenu()
        {
            var menu = new ContextMenuStrip();

            // 标题项
            var headerItem = new ToolStripMenuItem("Pointer (macOS 26 纯绿版)") { Enabled = false };
            headerItem.Font = new Font(headerItem.Font, FontStyle.Bold);
            menu.Items.Add(headerItem);
            menu.Items.Add(new ToolStripSeparator());

            // 1. 指示器形态
            var styleMenu = new ToolStripMenuItem("🎨 显示形态 (Style)");
            var badgeItem = new ToolStripMenuItem("macOS 晶透微标「中/A」", null, (s, e) => SwitchStyle("badge"))
            {
                Checked = string.Equals(_config.IndicatorStyle, "badge", StringComparison.OrdinalIgnoreCase)
            };
            var dotItem = new ToolStripMenuItem("极光呼吸微点", null, (s, e) => SwitchStyle("dot"))
            {
                Checked = string.Equals(_config.IndicatorStyle, "dot", StringComparison.OrdinalIgnoreCase)
            };
            styleMenu.DropDownItems.Add(badgeItem);
            styleMenu.DropDownItems.Add(dotItem);
            menu.Items.Add(styleMenu);

            // 2. 色彩主题
            var themeMenu = new ToolStripMenuItem("🌓 色彩模式 (Theme)");
            var autoTheme = new ToolStripMenuItem("跟随系统深浅色自适应", null, (s, e) => SwitchTheme("auto"))
            {
                Checked = string.Equals(_config.ThemeMode, "auto", StringComparison.OrdinalIgnoreCase)
            };
            var lightTheme = new ToolStripMenuItem("浅色模式 (Light)", null, (s, e) => SwitchTheme("light"))
            {
                Checked = string.Equals(_config.ThemeMode, "light", StringComparison.OrdinalIgnoreCase)
            };
            var darkTheme = new ToolStripMenuItem("深色模式 (Dark)", null, (s, e) => SwitchTheme("dark"))
            {
                Checked = string.Equals(_config.ThemeMode, "dark", StringComparison.OrdinalIgnoreCase)
            };
            themeMenu.DropDownItems.Add(autoTheme);
            themeMenu.DropDownItems.Add(lightTheme);
            themeMenu.DropDownItems.Add(darkTheme);
            menu.Items.Add(themeMenu);

            // 3. 追踪模式
            var trackMenu = new ToolStripMenuItem("🎯 追踪模式 (Track)");
            var bothTrack = new ToolStripMenuItem("智能双追踪 (光标 + 鼠标)", null, (s, e) => SwitchTrack("both"))
            {
                Checked = string.Equals(_config.TrackMode, "both", StringComparison.OrdinalIgnoreCase)
            };
            var caretTrack = new ToolStripMenuItem("仅文本光标 (Caret Only)", null, (s, e) => SwitchTrack("caret"))
            {
                Checked = string.Equals(_config.TrackMode, "caret", StringComparison.OrdinalIgnoreCase)
            };
            var mouseTrack = new ToolStripMenuItem("仅鼠标指针 (Mouse Only)", null, (s, e) => SwitchTrack("mouse"))
            {
                Checked = string.Equals(_config.TrackMode, "mouse", StringComparison.OrdinalIgnoreCase)
            };
            trackMenu.DropDownItems.Add(bothTrack);
            trackMenu.DropDownItems.Add(caretTrack);
            trackMenu.DropDownItems.Add(mouseTrack);
            menu.Items.Add(trackMenu);

            // 4. 功能复选
            var enItem = new ToolStripMenuItem("🔤 英文状态显示", null, (s, e) => ToggleShowEnglish())
            {
                Checked = _config.ShowEnglish
            };
            menu.Items.Add(enItem);

            var animItem = new ToolStripMenuItem("🌊 平滑呼吸动画", null, (s, e) => ToggleSmoothAnimation())
            {
                Checked = _config.SmoothAnimation
            };
            menu.Items.Add(animItem);

            menu.Items.Add(new ToolStripSeparator());

            // 5. 便携操作
            menu.Items.Add("⚙️ 编辑配置文件 (config.ini)", null, (s, e) => OpenConfigFile());
            menu.Items.Add("📂 打开本软件目录 (Portable Folder)", null, (s, e) => OpenAppFolder());

            var autoStartItem = new ToolStripMenuItem("🚀 开机自启动 (纯绿色)", null, (s, e) => ToggleAutoStart())
            {
                Checked = IsAutoStartEnabled()
            };
            menu.Items.Add(autoStartItem);

            menu.Items.Add(new ToolStripSeparator());

            // 6. 重启与退出
            menu.Items.Add("🔄 重启程序 (Restart)", null, (s, e) => RestartApp());
            menu.Items.Add("❌ 退出 (Exit)", null, (s, e) => ExitApp());

            _notifyIcon.ContextMenuStrip = menu;
        }

        private void SwitchStyle(string style)
        {
            _config.IndicatorStyle = style;
            _config.Save();
            BuildContextMenu();
            _onConfigChanged();
        }

        private void SwitchTheme(string theme)
        {
            _config.ThemeMode = theme;
            _config.Save();
            BuildContextMenu();
            _onConfigChanged();
        }

        private void SwitchTrack(string track)
        {
            _config.TrackMode = track;
            _config.Save();
            BuildContextMenu();
            _onConfigChanged();
        }

        private void ToggleShowEnglish()
        {
            _config.ShowEnglish = !_config.ShowEnglish;
            _config.Save();
            BuildContextMenu();
            _onConfigChanged();
        }

        private void ToggleSmoothAnimation()
        {
            _config.SmoothAnimation = !_config.SmoothAnimation;
            _config.Save();
            BuildContextMenu();
            _onConfigChanged();
        }

        private void OpenConfigFile()
        {
            string cfg = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.ini");
            if (File.Exists(cfg))
            {
                Process.Start("notepad.exe", cfg);
            }
        }

        private void OpenAppFolder()
        {
            Process.Start("explorer.exe", AppDomain.CurrentDomain.BaseDirectory);
        }

        private bool IsAutoStartEnabled()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(RUN_REG_KEY, false))
                {
                    if (key != null)
                    {
                        return key.GetValue(APP_REG_NAME) != null;
                    }
                    return false;
                }
            }
            catch { return false; }
        }

        private void ToggleAutoStart()
        {
            bool current = IsAutoStartEnabled();
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(RUN_REG_KEY, true))
                {
                    if (key != null)
                    {
                        if (current)
                        {
                            key.DeleteValue(APP_REG_NAME, false);
                            _config.AutoStart = false;
                        }
                        else
                        {
                            string exePath = Process.GetCurrentProcess().MainModule.FileName;
                            key.SetValue(APP_REG_NAME, "\"" + exePath + "\"");
                            _config.AutoStart = true;
                        }
                        _config.Save();
                    }
                }
            }
            catch { }
            BuildContextMenu();
        }

        private void RestartApp()
        {
            string exe = Process.GetCurrentProcess().MainModule.FileName;
            Process.Start(exe);
            Application.Exit();
        }

        private void ExitApp()
        {
            Application.Exit();
        }

        public void Dispose()
        {
            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
                _notifyIcon = null;
            }
        }
    }
}
