using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Text;

namespace PointerMacOS
{
    public class Config
    {
        // General
        public string IndicatorStyle = "badge"; // "badge" or "dot"
        public string ThemeMode = "auto";       // "auto", "light", "dark"
        public string TrackMode = "both";       // "both", "caret", "mouse"
        public bool ShowEnglish = true;
        public bool SmoothAnimation = true;

        // Badge Dimensions
        public int BadgeWidth = 26;
        public int BadgeHeight = 20;
        public int BadgeCornerRadius = 6;
        public float BadgeFontSize = 10f;

        // Dot Dimensions
        public int DotSize = 10;

        // Colors
        public Color LightCnColor = Color.FromArgb(255, 90, 0);     // Apple Warm Amber
        public Color LightEnColor = Color.FromArgb(0, 122, 255);    // Apple Azure Blue
        public Color DarkCnColor = Color.FromArgb(255, 159, 10);    // Apple Radiant Gold
        public Color DarkEnColor = Color.FromArgb(10, 132, 255);    // Apple Neon Cyan

        // Offsets
        public int CaretOffsetX = 2;
        public int CaretOffsetY = 2;
        public int MouseOffsetX = 16;
        public int MouseOffsetY = 16;

        // System
        public bool AutoStart = false;
        public int PollIntervalMs = 16; // ~60fps smooth polling

        private static string ConfigPath
        {
            get
            {
                return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.ini");
            }
        }

        public static Config Load()
        {
            var config = new Config();
            string path = ConfigPath;

            if (!File.Exists(path))
            {
                config.Save();
                return config;
            }

            try
            {
                string currentSection = "";
                foreach (string rawLine in File.ReadAllLines(path, Encoding.UTF8))
                {
                    string line = rawLine.Trim();
                    if (string.IsNullOrEmpty(line) || line.StartsWith("#") || line.StartsWith(";"))
                        continue;

                    if (line.StartsWith("[") && line.EndsWith("]"))
                    {
                        currentSection = line.Substring(1, line.Length - 2).Trim().ToLowerInvariant();
                        continue;
                    }

                    int eqIdx = line.IndexOf('=');
                    if (eqIdx <= 0) continue;

                    string key = line.Substring(0, eqIdx).Trim().ToLowerInvariant();
                    string val = line.Substring(eqIdx + 1).Trim();
                    int commentIdx = val.IndexOf('#');
                    if (commentIdx >= 0) val = val.Substring(0, commentIdx).Trim();

                    switch (key)
                    {
                        case "indicatorstyle": config.IndicatorStyle = val.ToLowerInvariant(); break;
                        case "thememode": config.ThemeMode = val.ToLowerInvariant(); break;
                        case "trackmode": config.TrackMode = val.ToLowerInvariant(); break;
                        case "showenglish": config.ShowEnglish = ParseBool(val, true); break;
                        case "smoothanimation": config.SmoothAnimation = ParseBool(val, true); break;

                        case "badgewidth": int.TryParse(val, out config.BadgeWidth); break;
                        case "badgeheight": int.TryParse(val, out config.BadgeHeight); break;
                        case "badgecornerradius": int.TryParse(val, out config.BadgeCornerRadius); break;
                        case "badgefontsize": float.TryParse(val, out config.BadgeFontSize); break;

                        case "dotsize": int.TryParse(val, out config.DotSize); break;

                        case "lightcncolor": config.LightCnColor = ParseColor(val, config.LightCnColor); break;
                        case "lightencolor": config.LightEnColor = ParseColor(val, config.LightEnColor); break;
                        case "darkcncolor": config.DarkCnColor = ParseColor(val, config.DarkCnColor); break;
                        case "darkencolor": config.DarkEnColor = ParseColor(val, config.DarkEnColor); break;

                        case "caretoffsetx": int.TryParse(val, out config.CaretOffsetX); break;
                        case "caretoffsety": int.TryParse(val, out config.CaretOffsetY); break;
                        case "mouseoffsetx": int.TryParse(val, out config.MouseOffsetX); break;
                        case "mouseoffsety": int.TryParse(val, out config.MouseOffsetY); break;

                        case "autostart": config.AutoStart = ParseBool(val, false); break;
                        case "pollintervalms": int.TryParse(val, out config.PollIntervalMs); break;
                    }
                }
            }
            catch
            {
                // Fallback to default
            }

            return config;
        }

        public void Save()
        {
            var sb = new StringBuilder();
            sb.AppendLine("; ==========================================================================");
            sb.AppendLine("; Pointer: Windows 输入法中英状态指示器 (macOS 26 纯绿色重构版)");
            sb.AppendLine("; 零安装 · 零依赖 · 配置文件严格仅保存在本程序同级目录");
            sb.AppendLine("; ==========================================================================");
            sb.AppendLine();
            sb.AppendLine("[General]");
            sb.AppendLine("; 指示器形态: badge (macOS 晶透微标「中/A」) 或 dot (极光呼吸微点)");
            sb.AppendLine("IndicatorStyle = " + IndicatorStyle);
            sb.AppendLine("; 色彩主题: auto (跟随系统深浅色自适应), light (浅色模式), dark (深色模式)");
            sb.AppendLine("ThemeMode = " + ThemeMode);
            sb.AppendLine("; 追踪模式: both (光标+鼠标), caret (仅文本光标), mouse (仅鼠标指针)");
            sb.AppendLine("TrackMode = " + TrackMode);
            sb.AppendLine("; 英文输入状态下是否显示指示器 (true / false)");
            sb.AppendLine("ShowEnglish = " + (ShowEnglish ? "true" : "false"));
            sb.AppendLine("; 是否启用平滑呼吸淡入淡出过渡动画");
            sb.AppendLine("SmoothAnimation = " + (SmoothAnimation ? "true" : "false"));
            sb.AppendLine();
            sb.AppendLine("[Badge]");
            sb.AppendLine("BadgeWidth = " + BadgeWidth);
            sb.AppendLine("BadgeHeight = " + BadgeHeight);
            sb.AppendLine("BadgeCornerRadius = " + BadgeCornerRadius);
            sb.AppendLine("BadgeFontSize = " + BadgeFontSize);
            sb.AppendLine();
            sb.AppendLine("[Dot]");
            sb.AppendLine("DotSize = " + DotSize);
            sb.AppendLine();
            sb.AppendLine("[Colors]");
            sb.AppendLine("LightCnColor = #" + ColorToHex(LightCnColor));
            sb.AppendLine("LightEnColor = #" + ColorToHex(LightEnColor));
            sb.AppendLine("DarkCnColor = #" + ColorToHex(DarkCnColor));
            sb.AppendLine("DarkEnColor = #" + ColorToHex(DarkEnColor));
            sb.AppendLine();
            sb.AppendLine("[Offsets]");
            sb.AppendLine("CaretOffsetX = " + CaretOffsetX);
            sb.AppendLine("CaretOffsetY = " + CaretOffsetY);
            sb.AppendLine("MouseOffsetX = " + MouseOffsetX);
            sb.AppendLine("MouseOffsetY = " + MouseOffsetY);
            sb.AppendLine();
            sb.AppendLine("[System]");
            sb.AppendLine("; 纯绿色开机启动 (true 写入当前用户的 HKCU Run，false 彻底清理)");
            sb.AppendLine("AutoStart = " + (AutoStart ? "true" : "false"));
            sb.AppendLine("PollIntervalMs = " + PollIntervalMs);

            try
            {
                File.WriteAllText(ConfigPath, sb.ToString(), Encoding.UTF8);
            }
            catch { }
        }

        private static bool ParseBool(string val, bool def)
        {
            if (string.IsNullOrEmpty(val)) return def;
            val = val.ToLowerInvariant();
            if (val == "true" || val == "1" || val == "yes") return true;
            if (val == "false" || val == "0" || val == "no") return false;
            return def;
        }

        private static Color ParseColor(string hex, Color def)
        {
            if (string.IsNullOrEmpty(hex)) return def;
            hex = hex.Trim().TrimStart('#');
            try
            {
                if (hex.Length == 6)
                {
                    int r = Convert.ToInt32(hex.Substring(0, 2), 16);
                    int g = Convert.ToInt32(hex.Substring(2, 2), 16);
                    int b = Convert.ToInt32(hex.Substring(4, 2), 16);
                    return Color.FromArgb(255, r, g, b);
                }
                else if (hex.Length == 8)
                {
                    int a = Convert.ToInt32(hex.Substring(0, 2), 16);
                    int r = Convert.ToInt32(hex.Substring(2, 2), 16);
                    int g = Convert.ToInt32(hex.Substring(4, 2), 16);
                    int b = Convert.ToInt32(hex.Substring(6, 2), 16);
                    return Color.FromArgb(a, r, g, b);
                }
            }
            catch { }
            return def;
        }

        private static string ColorToHex(Color c)
        {
            return string.Format("{0:X2}{1:X2}{2:X2}", c.R, c.G, c.B);
        }
    }
}
