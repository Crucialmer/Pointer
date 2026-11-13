using System;
using Microsoft.Win32;

namespace PointerMacOS
{
    public static class ThemeManager
    {
        public static bool IsSystemDarkMode()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                {
                    if (key != null)
                    {
                        object val = key.GetValue("AppsUseLightTheme");
                        if (val is int)
                        {
                            int intVal = (int)val;
                            return intVal == 0;
                        }
                    }
                }
            }
            catch { }
            return false;
        }

        public static bool IsEffectiveDarkMode(string themeConfig)
        {
            if (string.Equals(themeConfig, "dark", StringComparison.OrdinalIgnoreCase))
                return true;
            if (string.Equals(themeConfig, "light", StringComparison.OrdinalIgnoreCase))
                return false;
            return IsSystemDarkMode();
        }
    }
}
