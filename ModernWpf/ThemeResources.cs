using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

namespace ModernWpf
{
    public class ThemeResources : ResourceDictionaryEx, ISupportInitialize
    {
        private static ThemeResources _current;

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
            get => ThemeManager.Current.ApplicationTheme;
            set
            {
                if (ThemeManager.Current.ApplicationTheme != value)
                {
                    ThemeManager.Current.SetCurrentValue(ThemeManager.ApplicationThemeProperty, value);

                    if (DesignMode.DesignModeEnabled)
                    {
                        UpdateDesignTimeThemeDictionary();
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the accent color of the app.
        /// </summary>
        public Color? AccentColor
        {
            get => ThemeManager.Current.AccentColor;
            set
            {
                if (ThemeManager.Current.AccentColor != value)
                {
                    ThemeManager.Current.SetCurrentValue(ThemeManager.AccentColorProperty, value);

                    if (DesignMode.DesignModeEnabled)
                    {
                        UpdateDesignTimeSystemColors();
                    }
                }
            }
        }

        #region Design Time

        private void DesignTimeInit()
        {
            Debug.Assert(DesignMode.DesignModeEnabled);
            UpdateDesignTimeSystemColors();
            UpdateDesignTimeThemeDictionary();
            SystemParameters.StaticPropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(SystemParameters.HighContrast))
                {
                    UpdateDesignTimeThemeDictionary();
                }
            };
        }

        private void UpdateDesignTimeSystemColors()
        {
            Debug.Assert(DesignMode.DesignModeEnabled);

            if (IsInitializePending)
            {
                return;
            }

            var colors = GetDesignTimeSystemColors();
            MergedDictionaries.InsertOrReplace(0, colors);

            ThemeManager.UpdateThemeBrushes(colors);
        }

        private void UpdateDesignTimeThemeDictionary()
        {
            Debug.Assert(DesignMode.DesignModeEnabled);

            if (IsInitializePending)
            {
                return;
            }

            if (SystemParameters.HighContrast)
            {
                EnsureHighContrastResources();
                updateTo(_highContrastResources);
            }
            else
            {
                var appTheme = RequestedTheme ?? ApplicationTheme.Light;
                switch (appTheme)
                {
                    case ApplicationTheme.Light:
                        EnsureLightResources();
                        updateTo(_lightResources);
                        break;
                    case ApplicationTheme.Dark:
                        EnsureDarkResources();
                        updateTo(_darkResources);
                        break;
                }
            }

            void updateTo(ResourceDictionary themeDictionary)
            {
                MergedDictionaries.RemoveIfNotNull(_lightResources);
                MergedDictionaries.RemoveIfNotNull(_darkResources);
                MergedDictionaries.RemoveIfNotNull(_highContrastResources);
                MergedDictionaries.Insert(1, themeDictionary);
            }
        }

        private ResourceDictionary GetDesignTimeSystemColors()
        {
            Debug.Assert(DesignMode.DesignModeEnabled);
            if (AccentColor.HasValue)
            {
                return new ColorPaletteResources { Accent = AccentColor };
            }
            else
            {
                return new ResourceDictionary { Source = PackUriHelper.GetAbsoluteUri("DesignTime/SystemColors.xaml") };
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
            IsInitializePending = false;

            if (DesignMode.DesignModeEnabled)
            {
                DesignTimeInit();
            }
            else
            {
                ThemeManager.Current.Initialize();
            }

            EndInit();
        }

        #endregion

        private int MergedThemeDictionaryCount
        {
            get
            {
                int count = 0;
                if (IsMerged(_lightResources)) { count++; };
                if (IsMerged(_darkResources)) { count++; };
                if (IsMerged(_highContrastResources)) { count++; };
                return count;
            }
        }

        internal void ApplyApplicationTheme(ApplicationTheme theme)
        {
            int targetIndex = DesignMode.DesignModeEnabled ? 1 : 0;

            if (SystemParameters.HighContrast)
            {
                EnsureHighContrastResources();
                MergedDictionaries.InsertOrReplace(targetIndex, _highContrastResources);
                MergedDictionaries.RemoveIfNotNull(_lightResources);
                MergedDictionaries.RemoveIfNotNull(_darkResources);
            }
            else
            {
                if (theme == ApplicationTheme.Light)
                {
                    EnsureLightResources();
                    MergedDictionaries.InsertOrReplace(targetIndex, _lightResources);
                    MergedDictionaries.RemoveIfNotNull(_darkResources);
                }
                else if (theme == ApplicationTheme.Dark)
                {
                    EnsureDarkResources();
                    MergedDictionaries.InsertOrReplace(targetIndex, _darkResources);
                    MergedDictionaries.RemoveIfNotNull(_lightResources);
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(theme));
                }

                MergedDictionaries.RemoveIfNotNull(_highContrastResources);
            }

            Debug.Assert(MergedThemeDictionaryCount == 1);
        }

        internal void ApplyElementTheme(ResourceDictionary target, ElementTheme theme)
        {
            ResourceDictionary mergedAppThemeDictionary = null;

            if (SystemParameters.HighContrast)
            {
                target.MergedDictionaries.RemoveIfNotNull(_lightResources);
                target.MergedDictionaries.RemoveIfNotNull(_darkResources);
            }
            else
            {
                if (theme == ElementTheme.Light)
                {
                    EnsureLightResources();
                    target.MergedDictionaries.RemoveIfNotNull(_darkResources);
                    target.MergedDictionaries.InsertIfNotExists(0, _lightResources);
                    mergedAppThemeDictionary = _lightResources;
                }
                else if (theme == ElementTheme.Dark)
                {
                    EnsureDarkResources();
                    target.MergedDictionaries.RemoveIfNotNull(_lightResources);
                    target.MergedDictionaries.InsertIfNotExists(0, _darkResources);
                    mergedAppThemeDictionary = _darkResources;
                }
                else // Default
                {
                    target.MergedDictionaries.RemoveIfNotNull(_lightResources);
                    target.MergedDictionaries.RemoveIfNotNull(_darkResources);
                }
            }

            if (target is ResourceDictionaryEx etr)
            {
                etr.MergedAppThemeDictionary = mergedAppThemeDictionary;
            }
        }

        private void EnsureLightResources()
        {
            if (_lightResources == null)
            {
                _lightResources = GetThemeDictionary(ThemeManager.LightKey);
            }
        }

        private void EnsureDarkResources()
        {
            if (_darkResources == null)
            {
                _darkResources = GetThemeDictionary(ThemeManager.DarkKey);
            }
        }

        private void EnsureHighContrastResources()
        {
            if (_highContrastResources == null)
            {
                _highContrastResources = GetThemeDictionary(ThemeManager.HighContrastKey);
            }
        }

        private bool IsMerged(ResourceDictionary dictionary)
        {
            return dictionary != null && MergedDictionaries.Contains(dictionary);
        }

        private ResourceDictionary GetThemeDictionary(string key)
        {
            ResourceDictionary defaultThemeDictionary = ThemeManager.GetDefaultThemeDictionary(key);

            if (ThemeDictionaries.TryGetValue(key, out ResourceDictionary themeDictionary))
            {
                if (!ContainsDefaultThemeResources(themeDictionary))
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

        private static bool ContainsDefaultThemeResources(ResourceDictionary dictionary)
        {
            if (dictionary is DefaultThemeResources)
            {
                return true;
            }

            foreach (var mergedDictionary in dictionary.MergedDictionaries)
            {
                if (mergedDictionary is DefaultThemeResources ||
                    mergedDictionary != null && ContainsDefaultThemeResources(mergedDictionary))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
