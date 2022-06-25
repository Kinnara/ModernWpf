using ModernWpf.SampleApp.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Windows.Storage;

namespace ModernWpf.SampleApp.Helper
{
    /// <summary>
    /// Class providing functionality around switching and restoring theme settings
    /// </summary>
    public static class ThemeHelper
    {
        private const string SelectedAppThemeKey = "SelectedAppTheme";

        /// <summary>
        /// Gets the current actual theme of the app based on the requested theme of the
        /// root element, or if that value is Default, the requested theme of the Application.
        /// </summary>
        public static ElementTheme ActualTheme
        {
            get
            {
                foreach (Window window in WindowHelper.ActiveWindows)
                {
                    if (ThemeManager.GetActualTheme(window) != ElementTheme.Default)
                    {
                        return ThemeManager.GetActualTheme(window);
                    }
                }

                return App.GetEnum<ElementTheme>(ElementTheme.Default.ToString());
            }
        }

        /// <summary>
        /// Gets or sets (with LocalSettings persistence) the RequestedTheme of the root element.
        /// </summary>
        public static ElementTheme RootTheme
        {
            get
            {
                foreach (Window window in WindowHelper.ActiveWindows)
                {
                    return ThemeManager.GetActualTheme(window);
                }

                return ElementTheme.Default;
            }
            set
            {
                foreach (Window window in WindowHelper.ActiveWindows)
                {
                    ThemeManager.SetRequestedTheme(window, value);
                }

                if (PackagedAppHelper.IsPackagedApp)
                {
                    ApplicationData.Current.LocalSettings.Values[SelectedAppThemeKey] = value.ToString();
                }
                else
                {
                    Properties.Settings.Default.SelectedAppTheme = value.ToString();
                    Properties.Settings.Default.Save();
                }

                UpdateSystemCaptionButtonColors();
            }
        }

        public static void Initialize()
        {
            string savedTheme = PackagedAppHelper.IsPackagedApp ? ApplicationData.Current.LocalSettings.Values[SelectedAppThemeKey]?.ToString() : Properties.Settings.Default.SelectedAppTheme;

            if (savedTheme != null)
            {
                RootTheme = App.GetEnum<ElementTheme>(savedTheme);
            }
        }

        public static bool IsDarkTheme()
        {
            if (RootTheme == ElementTheme.Default)
            {
                return ThemeManager.Current.ApplicationTheme == ApplicationTheme.Dark;
            }
            return RootTheme == ElementTheme.Dark;
        }

        public static void UpdateSystemCaptionButtonColors()
        {
        }
    }
}
