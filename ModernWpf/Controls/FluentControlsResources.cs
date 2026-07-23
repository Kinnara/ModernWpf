using System;
using System.Windows;

namespace ModernWpf.Controls
{
    /// <summary>
    /// Default styles for ModernWpf controls with the platform Fluent theme when available.
    /// </summary>
    public sealed class FluentControlsResources : ResourceDictionary
    {
        /// <summary>
        /// Initializes a new instance of the FluentControlsResources class.
        /// </summary>
        public FluentControlsResources()
        {
#if NET10_0_OR_GREATER
            ModernWpf.ThemeManager.Current.EnablePlatformFluentTheme();
            if (DesignMode.DesignModeEnabled)
            {
                MergedDictionaries.Add(PlatformFluentResources);
            }
            MergedDictionaries.Add(ModernWpfControlsResources);
#else
            MergedDictionaries.Add(XamlControlsResources.ControlsResources);
#endif
            MergedDictionaries.Add(XamlControlsResources.UISettingsResources);

            if (DesignMode.DesignModeEnabled)
            {
                _ = XamlControlsResources.CompactResources;
            }
        }

        public bool UseCompactResources
        {
            get => _useCompactResources;
            set
            {
                if (_useCompactResources != value)
                {
                    _useCompactResources = value;

                    if (UseCompactResources)
                    {
                        MergedDictionaries.Add(XamlControlsResources.CompactResources);
                    }
                    else
                    {
                        MergedDictionaries.Remove(XamlControlsResources.CompactResources);
                    };
                }
            }
        }

#if NET10_0_OR_GREATER
        private static ResourceDictionary PlatformFluentResources
        {
            get
            {
                if (_platformFluentResources == null)
                {
                    _platformFluentResources = new ResourceDictionary
                    {
                        Source = new Uri("pack://application:,,,/PresentationFramework.Fluent;component/Themes/Fluent.xaml", UriKind.Absolute)
                    };
                }
                return _platformFluentResources;
            }
        }

        private static ResourceDictionary ModernWpfControlsResources
        {
            get
            {
                if (_modernWpfControlsResources == null)
                {
                    _modernWpfControlsResources = new ResourceDictionary { Source = PackUriHelper.GetAbsoluteUri("ModernWpfControlsResources.xaml") };
                }
                return _modernWpfControlsResources;
            }
        }

        private static ResourceDictionary _platformFluentResources;
        private static ResourceDictionary _modernWpfControlsResources;
#endif

        private bool _useCompactResources;
    }
}
