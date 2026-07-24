using System.Windows;

namespace ModernWpf
{
    internal static class PlatformThemeModeBridge
    {
#if NET10_0_OR_GREATER
#pragma warning disable WPF0001
        internal static void ApplyApplicationTheme(ApplicationTheme? theme)
        {
            var application = Application.Current;
            if (application == null)
            {
                return;
            }

            var themeMode = ToThemeMode(theme);
            if (application.ThemeMode != themeMode)
            {
                application.ThemeMode = themeMode;
            }
        }

        internal static void ApplyWindowTheme(Window window, ElementTheme requestedTheme)
        {
            var themeMode = ToThemeMode(requestedTheme);
            if (window.ThemeMode != themeMode)
            {
                window.ThemeMode = themeMode;
            }
        }

        private static ThemeMode ToThemeMode(ApplicationTheme? theme)
        {
            return theme switch
            {
                ApplicationTheme.Light => ThemeMode.Light,
                ApplicationTheme.Dark => ThemeMode.Dark,
                null => ThemeMode.System,
                _ => ThemeMode.System
            };
        }

        private static ThemeMode ToThemeMode(ElementTheme requestedTheme)
        {
            return requestedTheme switch
            {
                ElementTheme.Light => ThemeMode.Light,
                ElementTheme.Dark => ThemeMode.Dark,
                _ => ThemeMode.None
            };
        }
#pragma warning restore WPF0001
#else
        internal static void ApplyApplicationTheme(ApplicationTheme? theme)
        {
        }

        internal static void ApplyWindowTheme(Window window, ElementTheme requestedTheme)
        {
        }
#endif
    }
}
