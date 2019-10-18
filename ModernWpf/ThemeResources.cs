using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace ModernWpf
{
    public class ThemeResources : ResourceDictionary, ISupportInitialize
    {
        private static ThemeResources _current;

        public ThemeResources()
        {
            if (Current == null)
            {
                Current = this;
            }
        }

        internal static ThemeResources Current
        {
            get => _current;
            set
            {
                if (_current != null)
                {
                    throw new InvalidOperationException(nameof(Current) + " cannot be changed after it's set.");
                }

                _current = value;
            }
        }

        private Uri _lightSource = ThemeManager.DefaultLightSource;
        public Uri LightSource
        {
            get => _lightSource;
            set
            {
                if (_lightSource != value)
                {
                    _lightSource = value;

                    if (!IsInitializePending)
                    {
                        UpdateThemeDictionary();
                    }
                }
            }
        }

        private Uri _darkSource = ThemeManager.DefaultDarkSource;
        public Uri DarkSource
        {
            get => _darkSource;
            set
            {
                if (_darkSource != value)
                {
                    _darkSource = value;

                    if (!IsInitializePending)
                    {
                        UpdateThemeDictionary();
                    }
                }
            }
        }

        private Uri _highContrastSource = ThemeManager.DefaultHighContrastSource;
        public Uri HighContrastSource
        {
            get => _highContrastSource;
            set
            {
                if (_highContrastSource != value)
                {
                    _highContrastSource = value;

                    if (!IsInitializePending)
                    {
                        UpdateThemeDictionary();
                    }
                }
            }
        }

        private ApplicationTheme? _requestedTheme;
        /// <summary>
        /// Gets or sets a value that determines the light-dark preference for the overall
        /// theme of an app.
        /// </summary>
        /// <returns>
        /// A value of the enumeration. The initial value is the default theme set by the
        /// user in Windows settings.
        /// </returns>
        public ApplicationTheme? RequestedTheme
        {
            get => _requestedTheme;
            set
            {
                if (_requestedTheme != value)
                {
                    _requestedTheme = value;

                    if (!IsInitializePending)
                    {
                        UpdateThemeDictionary();
                    }
                }
            }
        }

        private Color? _accentColor;
        public Color? AccentColor
        {
            get => _accentColor;
            set
            {
                if (_accentColor != value)
                {
                    _accentColor = value;

                    if (!IsInitializePending)
                    {
                        UpdateSystemColors();
                    }
                }
            }
        }

        private bool IsInitializePending { get; set; }

        private void DesignTimeInit()
        {
            MergedDictionaries.Add(GetSystemColors());
            MergedDictionaries.Add(GetThemeDictionary());
        }

        private void UpdateSystemColors()
        {
            if (!DesignMode.DesignModeEnabled)
            {
                return;
            }

            if (MergedDictionaries.Count >= 1)
            {
                MergedDictionaries[0] = GetSystemColors();
            }
        }

        private void UpdateThemeDictionary()
        {
            if (!DesignMode.DesignModeEnabled)
            {
                return;
            }

            if (MergedDictionaries.Count >= 2)
            {
                MergedDictionaries[1] = GetThemeDictionary();
            }
        }

        private ResourceDictionary GetSystemColors()
        {
            if (AccentColor.HasValue)
            {
                return new ColorPaletteResources { Accent = AccentColor };
            }
            else
            {
                return new ResourceDictionary { Source = PackUriHelper.GetAbsoluteUri("DesignTime/SystemColors.xaml") };
            }
        }

        private ResourceDictionary GetThemeDictionary()
        {
            Uri source;

            if (SystemParameters.HighContrast)
            {
                source = HighContrastSource;
            }
            else
            {
                if (RequestedTheme == ApplicationTheme.Dark)
                {
                    source = DarkSource;
                }
                else
                {
                    source = LightSource;
                }
            }

            return new ResourceDictionary { Source = source };
        }

        void ISupportInitialize.BeginInit()
        {
            BeginInit();
            IsInitializePending = true;
        }

        void ISupportInitialize.EndInit()
        {
            EndInit();
            IsInitializePending = false;

            ThemeManager.Current.Initialize();

            if (DesignMode.DesignModeEnabled)
            {
                DesignTimeInit();
            }
        }
    }
}
