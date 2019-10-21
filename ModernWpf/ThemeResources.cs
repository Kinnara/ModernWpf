using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

namespace ModernWpf
{
    public class ThemeResources : ResourceDictionary, ISupportInitialize
    {
        private static ThemeResources _current;

        private readonly Dictionary<string, ResourceDictionary> _themeDictionaries = new Dictionary<string, ResourceDictionary>();

        private ResourceDictionary _lightResources;
        private ResourceDictionary _darkResources;
        private ResourceDictionary _highContrastResources;

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
                    OnThemePropertyChanged();
                }
            }
        }

        private Color? _accentColor;
        /// <summary>
        /// Gets or sets the accent color of the app.
        /// </summary>
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
                        UpdateDesignTimeSystemColors();
                    }
                }
            }
        }

        #region Design Time

        private void DesignTimeInit()
        {
            UpdateDesignTimeSystemColors();
            UpdateDesignTimeThemeDictionary();
            SystemParameters.StaticPropertyChanged += OnSystemParametersPropertyChanged;
        }

        private void UpdateDesignTimeSystemColors()
        {
            if (DesignMode.DesignModeEnabled)
            {
                var rd = GetDesignTimeSystemColors();
                if (MergedDictionaries.Count == 0)
                {
                    MergedDictionaries.Add(rd);
                }
                else
                {
                    MergedDictionaries[0] = rd;

                }
            }
        }

        private void OnThemePropertyChanged()
        {
            if (!IsInitializePending && DesignMode.DesignModeEnabled)
            {
                UpdateDesignTimeThemeDictionary();
            }
        }

        private void UpdateDesignTimeThemeDictionary()
        {
            ApplyApplicationTheme(RequestedTheme ?? ApplicationTheme.Light);
        }

        private ResourceDictionary GetDesignTimeSystemColors()
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

        private void OnSystemParametersPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SystemParameters.HighContrast))
            {
                UpdateDesignTimeThemeDictionary();
            }
        }

        #endregion

        #region ISupportInitialize

        private bool IsInitializePending { get; set; }

        void ISupportInitialize.BeginInit()
        {
            BeginInit();
            IsInitializePending = true;
        }

        void ISupportInitialize.EndInit()
        {
            EndInit();
            IsInitializePending = false;

            for (int i = MergedDictionaries.Count - 1; i >= 0; i--)
            {
                if (MergedDictionaries[i] is ThemeDictionary td)
                {
                    _themeDictionaries[td.Key] = td;
                    MergedDictionaries.RemoveAt(i);
                }
            }

            ThemeManager.Current.Initialize();

            if (DesignMode.DesignModeEnabled)
            {
                DesignTimeInit();
            }
        }

        #endregion

        internal void ApplyApplicationTheme(ApplicationTheme? appTheme)
        {
            Debug.Assert(appTheme.HasValue);
            if (appTheme.HasValue)
            {
                ElementTheme theme = ElementTheme.Default;
                switch (appTheme.Value)
                {
                    case ApplicationTheme.Light:
                        theme = ElementTheme.Light;
                        break;
                    case ApplicationTheme.Dark:
                        theme = ElementTheme.Dark;
                        break;
                }

                Debug.Assert(theme != ElementTheme.Default);
                UpdateThemeResources(this, theme, DesignMode.DesignModeEnabled);
            }
        }

        internal void UpdateThemeResources(ResourceDictionary target, ElementTheme theme, bool isDesignTime = false)
        {
            int insertIndex = isDesignTime ? 1 : 0;

            if (SystemParameters.HighContrast)
            {
                if (_highContrastResources == null)
                {
                    _highContrastResources = GetThemeDictionary(ThemeManager.HighContrastKey);
                }

                target.MergedDictionaries.InsertOrReplace(insertIndex, _highContrastResources);
            }
            else
            {
                if (theme == ElementTheme.Light)
                {
                    if (_lightResources == null)
                    {
                        _lightResources = GetThemeDictionary(ThemeManager.LightKey);
                    }

                    target.MergedDictionaries.InsertOrReplace(insertIndex, _lightResources);
                }
                else if (theme == ElementTheme.Dark)
                {
                    if (_darkResources == null)
                    {
                        _darkResources = GetThemeDictionary(ThemeManager.DarkKey);
                    }

                    target.MergedDictionaries.InsertOrReplace(insertIndex, _darkResources);
                }
                else // Default
                {
                    target.MergedDictionaries.RemoveIfNotNull(_lightResources);
                    target.MergedDictionaries.RemoveIfNotNull(_darkResources);
                }
            }
        }

        private ResourceDictionary GetThemeDictionary(string key)
        {
            ResourceDictionary defaultThemeDictionary = ThemeManager.GetDefaultThemeDictionary(key);

            if (_themeDictionaries.TryGetValue(key, out ResourceDictionary themeDictionary))
            {
                if (themeDictionary.MergedDictionaries.Count > 0 &&
                    themeDictionary.MergedDictionaries[0] is DefaultThemeResources)
                {
                    // Already contains the default theme resources
                }
                else
                {
                    themeDictionary.MergedDictionaries.Insert(0, defaultThemeDictionary);
                }
            }
            else
            {
                themeDictionary = defaultThemeDictionary;
            }

            return themeDictionary;
        }
    }
}
