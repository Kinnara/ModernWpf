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

                    if (!IsInitializePending && DesignMode.DesignModeEnabled)
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

                    if (!IsInitializePending && DesignMode.DesignModeEnabled)
                    {
                        UpdateDesignTimeSystemColors();
                    }
                }
            }
        }

        /// <summary>
        /// Gets a collection of merged resource dictionaries that are specifically keyed
        /// and composed to address theme scenarios, for example supplying theme values for
        /// "HighContrast".
        /// </summary>
        /// <returns>
        /// A dictionary of ResourceDictionary theme dictionaries. Each must be keyed with
        /// **x:Key**.
        /// </returns>
        public Dictionary<object, ResourceDictionary> ThemeDictionaries { get; } = new Dictionary<object, ResourceDictionary>();

        #region Design Time

        private void DesignTimeInit()
        {
            Debug.Assert(DesignMode.DesignModeEnabled);
            UpdateDesignTimeSystemColors();
            UpdateDesignTimeThemeDictionary();
            SystemParameters.StaticPropertyChanged += OnSystemParametersPropertyChanged;
        }

        private void UpdateDesignTimeSystemColors()
        {
            Debug.Assert(DesignMode.DesignModeEnabled);
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

        private void UpdateDesignTimeThemeDictionary()
        {
            Debug.Assert(DesignMode.DesignModeEnabled);
            ApplyApplicationTheme(RequestedTheme ?? ApplicationTheme.Light);
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

            //for (int i = MergedDictionaries.Count - 1; i >= 0; i--)
            //{
            //    if (MergedDictionaries[i] is ThemeDictionary td)
            //    {
            //        ThemeDictionaries[td.Key] = td;
            //        MergedDictionaries.RemoveAt(i);
            //    }
            //}

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
                Update(appTheme.Value);
            }
        }

        internal void UpdateMergedThemeDictionaries(ResourceDictionary target, ElementTheme theme)
        {
            bool containsAppThemeDictionary;

            if (SystemParameters.HighContrast)
            {
                target.MergedDictionaries.RemoveIfNotNull(_lightResources);
                target.MergedDictionaries.RemoveIfNotNull(_darkResources);
                containsAppThemeDictionary = false;
            }
            else
            {
                if (theme == ElementTheme.Light)
                {
                    if (_lightResources == null)
                    {
                        _lightResources = GetThemeDictionary(ThemeManager.LightKey);
                    }

                    target.MergedDictionaries.RemoveIfNotNull(_darkResources);
                    target.MergedDictionaries.Insert(0, _lightResources);
                    containsAppThemeDictionary = true;
                }
                else if (theme == ElementTheme.Dark)
                {
                    if (_darkResources == null)
                    {
                        _darkResources = GetThemeDictionary(ThemeManager.DarkKey);
                    }

                    target.MergedDictionaries.RemoveIfNotNull(_lightResources);
                    target.MergedDictionaries.Insert(0, _darkResources);
                    containsAppThemeDictionary = true;
                }
                else // Default
                {
                    target.MergedDictionaries.RemoveIfNotNull(_lightResources);
                    target.MergedDictionaries.RemoveIfNotNull(_darkResources);
                    containsAppThemeDictionary = false;
                }
            }

            if (target is ElementThemeResources etr)
            {
                etr.ContainsApplicationThemeDictionary = containsAppThemeDictionary;
            }
        }

        private void Update(ApplicationTheme theme)
        {
            int insertIndex = DesignMode.DesignModeEnabled ? 1 : 0;

            if (SystemParameters.HighContrast)
            {
                if (_highContrastResources == null)
                {
                    _highContrastResources = GetThemeDictionary(ThemeManager.HighContrastKey);
                }

                MergedDictionaries.InsertOrReplace(insertIndex, _highContrastResources);
            }
            else
            {
                MergedDictionaries.RemoveIfNotNull(_highContrastResources);

                if (theme == ApplicationTheme.Light)
                {
                    if (_lightResources == null)
                    {
                        _lightResources = GetThemeDictionary(ThemeManager.LightKey);
                    }

                    MergedDictionaries.InsertOrReplace(insertIndex, _lightResources);
                }
                else if (theme == ApplicationTheme.Dark)
                {
                    if (_darkResources == null)
                    {
                        _darkResources = GetThemeDictionary(ThemeManager.DarkKey);
                    }

                    MergedDictionaries.InsertOrReplace(insertIndex, _darkResources);
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(theme));
                }
            }
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
