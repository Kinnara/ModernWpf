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

        private bool _canBeAccessedAcrossThreads;

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

        public bool CanBeAccessedAcrossThreads
        {
            get => _canBeAccessedAcrossThreads;
            set
            {
                if (DesignMode.DesignModeEnabled)
                {
                    return;
                }

                if (IsInitialized)
                {
                    throw new InvalidOperationException();
                }

                _canBeAccessedAcrossThreads = value;
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

        private bool IsInitialized { get; set; }

        private bool IsInitializePending { get; set; }

        public new void BeginInit()
        {
            base.BeginInit();
            IsInitializePending = true;
            IsInitialized = false;
        }

        public new void EndInit()
        {
            IsInitializePending = false;
            IsInitialized = true;

            if (DesignMode.DesignModeEnabled)
            {
                DesignTimeInit();
            }
            else
            {
                ThemeManager.Current.Initialize();

                if (CanBeAccessedAcrossThreads)
                {
                    EnsureLightResources();
                    EnsureDarkResources();
                    EnsureHighContrastResources();

                    _lightResources.SealValues();
                    _darkResources.SealValues();
                    _highContrastResources.SealValues();
                }
            }

            base.EndInit();
        }

        void ISupportInitialize.BeginInit()
        {
            BeginInit();
        }

        void ISupportInitialize.EndInit()
        {
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

                if (IsMerged(_highContrastResources))
                {
                    if (CanBeAccessedAcrossThreads)
                    {
                        RefreshHighContrastResources();
                    }
                }
                else
                {
                    MergedDictionaries.InsertOrReplace(targetIndex, _highContrastResources);
                    MergedDictionaries.RemoveIfNotNull(_lightResources);
                    MergedDictionaries.RemoveIfNotNull(_darkResources);
                }
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

        internal ResourceDictionary GetThemeDictionary(string key)
        {
            switch (key)
            {
                case ThemeManager.LightKey:
                    EnsureLightResources();
                    return _lightResources;
                case ThemeManager.DarkKey:
                    EnsureDarkResources();
                    return _darkResources;
                case ThemeManager.HighContrastKey:
                    EnsureHighContrastResources();
                    return _highContrastResources;
                default:
                    throw new ArgumentException();
            }
        }

        internal ResourceDictionary TryGetThemeDictionary(string key)
        {
            return key switch
            {
                ThemeManager.LightKey => _lightResources,
                ThemeManager.DarkKey => _darkResources,
                ThemeManager.HighContrastKey => _highContrastResources,
                _ => null,
            };
        }

        private void EnsureLightResources()
        {
            if (_lightResources == null)
            {
                _lightResources = InitializeThemeDictionary(ThemeManager.LightKey);
            }
        }

        private void EnsureDarkResources()
        {
            if (_darkResources == null)
            {
                _darkResources = InitializeThemeDictionary(ThemeManager.DarkKey);
            }
        }

        private void EnsureHighContrastResources()
        {
            if (_highContrastResources == null)
            {
                _highContrastResources = InitializeThemeDictionary(ThemeManager.HighContrastKey);
            }
        }

        private void RefreshHighContrastResources()
        {
            Debug.Assert(_highContrastResources != null);

            var hcResources = _highContrastResources;
            var mergedDictionaries = hcResources.MergedDictionaries;
            var oldDefault = ThemeManager.GetDefaultThemeDictionary(ThemeManager.HighContrastKey);

            for (int i = 0; i < mergedDictionaries.Count; i++)
            {
                var md = mergedDictionaries[i];
                if (md.Source != null)
                {
                    var newMD = new ResourceDictionary { Source = md.Source };
                    newMD.SealValues();
                    if (md == oldDefault)
                    {
                        ThemeManager.SetDefaultThemeDictionary(ThemeManager.HighContrastKey, newMD);
                    }
                    mergedDictionaries[i] = newMD;
                }
            }
        }

        private bool IsMerged(ResourceDictionary dictionary)
        {
            return dictionary != null && MergedDictionaries.Contains(dictionary);
        }

        private ResourceDictionary InitializeThemeDictionary(string key)
        {
            ResourceDictionary defaultThemeDictionary = ThemeManager.GetDefaultThemeDictionary(key);

            if (ThemeDictionaries.TryGetValue(key, out ResourceDictionary themeDictionary))
            {
                if (!ContainsDefaultThemeResources(themeDictionary, defaultThemeDictionary))
                {
                    themeDictionary.MergedDictionaries.Insert(0, defaultThemeDictionary);
                }
            }
            else if (key == ThemeManager.HighContrastKey)
            {
                themeDictionary = new ResourceDictionary();
                themeDictionary.MergedDictionaries.Add(defaultThemeDictionary);
            }
            else
            {
                themeDictionary = defaultThemeDictionary;
            }

            return themeDictionary;
        }

        private static bool ContainsDefaultThemeResources(ResourceDictionary dictionary, ResourceDictionary defaultResources)
        {
            if (dictionary == defaultResources ||
                SourceEquals(dictionary.Source, defaultResources.Source))
            {
                return true;
            }

            foreach (var mergedDictionary in dictionary.MergedDictionaries)
            {
                if (mergedDictionary != null && ContainsDefaultThemeResources(mergedDictionary, defaultResources))
                {
                    return true;
                }
            }

            return false;

            static bool SourceEquals(Uri x, Uri y)
            {
                if (x == null || y == null)
                    return false;

                string xString = x.IsAbsoluteUri ? x.LocalPath : x.ToString();
                string yString = y.IsAbsoluteUri ? y.LocalPath : y.ToString();

                return string.Equals(xString, yString, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}
