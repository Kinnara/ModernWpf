using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace ModernWpf
{
    public class ThemeManager : DependencyObject
    {
        internal const string LightKey = "Light";
        internal const string DarkKey = "Dark";
        internal const string HighContrastKey = "HighContrast";

        private static readonly Binding _highContrastBinding = new Binding(nameof(BindableSystemParameters.HighContrast))
        {
            Source = BindableSystemParameters.Current
        };

        private bool _applicationStarted;

        #region ApplicationTheme

        /// <summary>
        /// Identifies the ApplicationTheme dependency property.
        /// </summary>
        public static readonly DependencyProperty ApplicationThemeProperty =
            DependencyProperty.Register(
                nameof(ApplicationTheme),
                typeof(ApplicationTheme?),
                typeof(ThemeManager),
                new PropertyMetadata(OnApplicationThemeChanged));

        /// <summary>
        /// Gets or sets a value that determines the light-dark preference for the overall
        /// theme of an app.
        /// </summary>
        /// <returns>
        /// A value of the enumeration. The initial value is the default theme set by the
        /// user in Windows settings.
        /// </returns>
        public ApplicationTheme? ApplicationTheme
        {
            get => (ApplicationTheme?)GetValue(ApplicationThemeProperty);
            set => SetValue(ApplicationThemeProperty, value);
        }

        private static void OnApplicationThemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ThemeManager)d).UpdateActualApplicationTheme();
        }

        #endregion

        #region ActualApplicationTheme

        internal static readonly DependencyProperty ActualApplicationThemeProperty =
            DependencyProperty.Register(
                nameof(ActualApplicationTheme),
                typeof(ApplicationTheme?),
                typeof(ThemeManager),
                new PropertyMetadata(OnActualApplicationThemeChanged));

        internal ApplicationTheme? ActualApplicationTheme
        {
            get => (ApplicationTheme?)GetValue(ActualApplicationThemeProperty);
            set => SetValue(ActualApplicationThemeProperty, value);
        }

        private static void OnActualApplicationThemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ThemeManager)d).OnActualApplicationThemeChanged(e);
        }

        private void OnActualApplicationThemeChanged(DependencyPropertyChangedEventArgs e)
        {
            Debug.Assert(e.NewValue != null);
            ApplyApplicationTheme();
        }

        private void UpdateActualApplicationTheme()
        {
            if (!ColorsHelper.IsInitialized)
            {
                return;
            }

            if (UsingSystemTheme)
            {
                ActualApplicationTheme = GetDefaultAppTheme();
            }
            else
            {
                ActualApplicationTheme = ApplicationTheme ?? ModernWpf.ApplicationTheme.Light;
            }
        }

        private void ApplyApplicationTheme()
        {
            if (!_applicationStarted)
            {
                return;
            }

            var resources = Application.Current.Resources;
            if (SystemParameters.HighContrast)
            {
                resources.MergedDictionaries.Remove(LightSource);
                resources.MergedDictionaries.Remove(DarkSource);
                resources.MergedDictionaries.Insert(1, HighContrastSource);
                //InitializeDefaultHighContrastResources(resources);
            }
            else
            {
                resources.MergedDictionaries.Remove(HighContrastSource);
                Debug.Assert(ActualApplicationTheme.HasValue);
                switch (ActualApplicationTheme)
                {
                    case ModernWpf.ApplicationTheme.Light:
                        resources.MergedDictionaries.Remove(DarkSource);
                        resources.MergedDictionaries.Insert(1, LightSource);
                        InitializeDefaultLightResources(resources);
                        break;
                    case ModernWpf.ApplicationTheme.Dark:
                        resources.MergedDictionaries.Remove(LightSource);
                        resources.MergedDictionaries.Insert(1, DarkSource);
                        InitializeDefaultDarkResources(resources);
                        break;
                }
            }
        }

        #endregion

        #region AccentColor

        public static readonly DependencyProperty AccentColorProperty =
            DependencyProperty.Register(
                nameof(AccentColor),
                typeof(Color?),
                typeof(ThemeManager),
                new PropertyMetadata(OnAccentColorChanged));

        public Color? AccentColor
        {
            get => (Color?)GetValue(AccentColorProperty);
            set => SetValue(AccentColorProperty, value);
        }

        private static void OnAccentColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ThemeManager)d).UpdateActualAccentColor();
        }

        #endregion

        #region ActualAccentColor

        private static readonly DependencyProperty ActualAccentColorProperty =
            DependencyProperty.Register(
                nameof(ActualAccentColor),
                typeof(Color),
                typeof(ThemeManager),
                new PropertyMetadata(OnActualAccentColorChanged));

        private Color ActualAccentColor
        {
            get => (Color)GetValue(ActualAccentColorProperty);
            set => SetValue(ActualAccentColorProperty, value);
        }

        private static void OnActualAccentColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ThemeManager)d).ApplyAccentColor();
        }

        private void ApplyAccentColor()
        {
            if (ColorsHelper.IsInitialized)
            {
                UpdateAccentColors();

                if (ResourceDictionaryCache.TryGetDictionary(LightSource, out ResourceDictionary light))
                {
                    var defaultResources = FindDictionary(light, DefaultLightSource);
                    if (defaultResources != null)
                    {
                        ColorsHelper.UpdateBrushes(defaultResources);
                    }
                }

                if (ResourceDictionaryCache.TryGetDictionary(DarkSource, out ResourceDictionary dark))
                {
                    var defaultResources = FindDictionary(dark, DefaultDarkSource);
                    if (defaultResources != null)
                    {
                        ColorsHelper.UpdateBrushes(defaultResources);
                    }
                }

                if (ResourceDictionaryCache.TryGetDictionary(HighContrastSource, out ResourceDictionary highContrast))
                {
                    var defaultResources = FindDictionary(highContrast, DefaultHighContrastSource);
                    if (defaultResources != null)
                    {
                        ColorsHelper.UpdateBrushes(defaultResources);
                    }
                }
            }
        }

        private void UpdateActualAccentColor()
        {
            if (!ColorsHelper.IsInitialized)
            {
                return;
            }

            if (UsingSystemAccentColor)
            {
                ActualAccentColor = ColorsHelper.GetSystemAccentColor();
            }
            else
            {
                ActualAccentColor = AccentColor ?? ColorsHelper.DefaultAccentColor;
            }
        }

        private void UpdateAccentColors()
        {
            if (UsingSystemAccentColor)
            {
                ColorsHelper.FetchSystemAccentColors();
            }
            else
            {
                ColorsHelper.SetAccent(ActualAccentColor);
            }
        }

        #endregion

        #region RequestedTheme

        /// <summary>
        /// Gets the UI theme that is used by the UIElement (and its child elements)
        /// for resource determination. The UI theme you specify with RequestedTheme can
        /// override the app-level RequestedTheme.
        /// </summary>
        /// <param name="element">The element from which to read the property value.</param>
        /// <returns>A value of the enumeration, for example **Light**.</returns>
        public static ElementTheme GetRequestedTheme(FrameworkElement element)
        {
            return (ElementTheme)element.GetValue(RequestedThemeProperty);
        }

        /// <summary>
        /// Sets the UI theme that is used by the UIElement (and its child elements)
        /// for resource determination. The UI theme you specify with RequestedTheme can
        /// override the app-level RequestedTheme.
        /// </summary>
        /// <param name="element">The element on which to set the attached property.</param>
        /// <param name="value">The property value to set.</param>
        public static void SetRequestedTheme(FrameworkElement element, ElementTheme value)
        {
            element.SetValue(RequestedThemeProperty, value);
        }

        /// <summary>
        /// Identifies the RequestedTheme dependency property.
        /// </summary>
        public static readonly DependencyProperty RequestedThemeProperty = DependencyProperty.RegisterAttached(
            "RequestedTheme",
            typeof(ElementTheme),
            typeof(ThemeManager),
            new PropertyMetadata(ElementTheme.Default, OnRequestedThemeChanged));

        private static void OnRequestedThemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var element = (FrameworkElement)d;
            StartOrStopListeningForHighContrastChanges(element);
            ApplyRequestedTheme(element);
        }

        private static void ApplyRequestedTheme(FrameworkElement element)
        {
            Uri lightSource;
            Uri darkSource;
            Uri highContrastSource;

            if (Current != null)
            {
                lightSource = Current.LightSource;
                darkSource = Current.DarkSource;
                highContrastSource = Current.HighContrastSource;
            }
            //else if (DesignTimeThemeResources.Current is DesignTimeThemeResources designTimeResources)
            //{
            //    lightSource = designTimeResources.LightSource;
            //    darkSource = designTimeResources.DarkSource;
            //    highContrastSource = DefaultHighContrastSource;
            //}
            else
            {
                lightSource = DefaultLightSource;
                darkSource = DefaultDarkSource;
                highContrastSource = DefaultHighContrastSource;
            }

            var requestedTheme = GetRequestedTheme(element);
            var resources = element.Resources;
            var useCache = true;

            if (SystemParameters.HighContrast)
            {
                resources.MergedDictionaries.Remove(lightSource);
                resources.MergedDictionaries.Remove(darkSource);
                resources.MergedDictionaries.AddFirst(highContrastSource, useCache);
                //InitializeDefaultHighContrastResources(resources);
            }
            else
            {
                resources.MergedDictionaries.Remove(highContrastSource);

                switch (requestedTheme)
                {
                    case ElementTheme.Light:
                        resources.MergedDictionaries.Remove(darkSource);
                        resources.MergedDictionaries.AddFirst(lightSource, useCache);
                        InitializeDefaultLightResources(resources);
                        break;
                    case ElementTheme.Dark:
                        resources.MergedDictionaries.Remove(lightSource);
                        resources.MergedDictionaries.AddFirst(darkSource, useCache);
                        InitializeDefaultDarkResources(resources);
                        break;
                    default:
                        resources.MergedDictionaries.Remove(lightSource);
                        resources.MergedDictionaries.Remove(darkSource);
                        break;
                }
            }

            if (element is Window window)
            {
                UpdateWindowActualTheme(window);
            }
            else if (requestedTheme == ElementTheme.Default)
            {
                element.ClearValue(ActualThemePropertyKey);
            }
            else
            {
                SetActualTheme(element, requestedTheme);
            }
        }

        #endregion

        #region ActualTheme

        /// <summary>
        /// Gets the UI theme that is currently used by the element, which might be different
        /// than the RequestedTheme.
        /// </summary>
        /// <param name="element">The element from which to read the property value.</param>
        /// <returns>A value of the enumeration, for example **Light**.</returns>
        public static ElementTheme GetActualTheme(FrameworkElement element)
        {
            return (ElementTheme)element.GetValue(ActualThemeProperty);
        }

        internal static void SetActualTheme(FrameworkElement element, ElementTheme value)
        {
            element.SetValue(ActualThemePropertyKey, value);
        }

        private static readonly DependencyPropertyKey ActualThemePropertyKey =
            DependencyProperty.RegisterAttachedReadOnly(
                "ActualTheme",
                typeof(ElementTheme),
                typeof(ThemeManager),
                new FrameworkPropertyMetadata(
                    ElementTheme.Light,
                    FrameworkPropertyMetadataOptions.Inherits,
                    OnActualThemeChanged));

        /// <summary>
        /// Identifies the ActualTheme dependency property.
        /// </summary>
        public static readonly DependencyProperty ActualThemeProperty =
            ActualThemePropertyKey.DependencyProperty;

        private static void OnActualThemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement fe)
            {
                ApplyThemeDictionary(fe);
            }
        }

        internal static void UpdateWindowActualTheme(Window window)
        {
            var requestedTheme = GetRequestedTheme(window);
            if (requestedTheme == ElementTheme.Default)
            {
                SetActualTheme(window, Current.ActualApplicationTheme.Value == ModernWpf.ApplicationTheme.Dark ? ElementTheme.Dark : ElementTheme.Light);
            }
            else
            {
                SetActualTheme(window, requestedTheme);
            }
        }

        #endregion

        #region ThemeDictionaries

        internal static IDictionary GetThemeDictionaries(FrameworkElement element)
        {
            var value = element.GetValue(ThemeDictionariesProperty) as Dictionary<object, ResourceDictionary>;
            if (value == null)
            {
                value = new Dictionary<object, ResourceDictionary>();
                SetThemeDictionaries(element, value);
            }
            return value;
        }

        private static void SetThemeDictionaries(FrameworkElement element, IDictionary value)
        {
            element.SetValue(ThemeDictionariesPropertyKey, value);
        }

        private static readonly DependencyPropertyKey ThemeDictionariesPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly(
                "ThemeDictionariesInternal",
                typeof(IDictionary),
                typeof(ThemeManager),
                new PropertyMetadata(OnThemeDictionariesChanged));

        internal static readonly DependencyProperty ThemeDictionariesProperty =
            ThemeDictionariesPropertyKey.DependencyProperty;

        private static void OnThemeDictionariesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var element = (FrameworkElement)d;
            element.InvokeOnInitialized(OnThemeDictionariesOwnerInitialized);
        }

        private static void OnThemeDictionariesOwnerInitialized(FrameworkElement element)
        {
            StartOrStopListeningForHighContrastChanges(element);
            ApplyThemeDictionary(element);
        }

        private static void ApplyThemeDictionary(FrameworkElement element)
        {
            if (element.GetValue(ThemeDictionariesProperty) is Dictionary<object, ResourceDictionary> td)
            {
                var md = element.Resources.MergedDictionaries;
                var actualTheme = GetActualTheme(element);

                td.TryGetValue(LightKey, out ResourceDictionary light);
                td.TryGetValue(DarkKey, out ResourceDictionary dark);
                td.TryGetValue(HighContrastKey, out ResourceDictionary highContrast);

                if (SystemParameters.HighContrast)
                {
                    md.RemoveIfNotNull(light);
                    md.RemoveIfNotNull(dark);
                    md.AddIfNotNull(highContrast);
                }
                else
                {
                    md.RemoveIfNotNull(highContrast);

                    if (actualTheme == ElementTheme.Dark)
                    {
                        md.RemoveIfNotNull(light);
                        md.AddIfNotNull(dark);
                    }
                    else
                    {
                        md.RemoveIfNotNull(dark);
                        md.AddIfNotNull(light);
                    }
                }
            }
        }

        #endregion

        #region HighContrast

        //private static bool GetHighContrast(FrameworkElement element)
        //{
        //    return (bool)element.GetValue(HighContrastProperty);
        //}

        //private static void SetHighContrast(FrameworkElement element, bool value)
        //{
        //    element.SetValue(HighContrastProperty, value);
        //}

        private static readonly DependencyProperty HighContrastProperty =
            DependencyProperty.RegisterAttached(
                "HighContrast",
                typeof(bool),
                typeof(ThemeManager),
                new PropertyMetadata(OnHighContrastChanged));

        private static void OnHighContrastChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var element = (FrameworkElement)d;
            ApplyRequestedTheme(element);
            ApplyThemeDictionary(element);
        }

        #endregion

        #region IsListeningForHighContrastChanges

        private static bool GetIsListeningForHighContrastChanges(FrameworkElement element)
        {
            return (bool)element.GetValue(IsListeningForHighContrastChangesProperty);
        }

        private static void SetIsListeningForHighContrastChanges(FrameworkElement element, bool value)
        {
            element.SetValue(IsListeningForHighContrastChangesProperty, value);
        }

        private static readonly DependencyProperty IsListeningForHighContrastChangesProperty =
            DependencyProperty.RegisterAttached(
                "IsListeningForHighContrastChanges",
                typeof(bool),
                typeof(ThemeManager),
                new PropertyMetadata(OnIsListeningForHighContrastChangesChanged));

        private static void OnIsListeningForHighContrastChangesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var element = (FrameworkElement)d;

            if ((bool)e.OldValue)
            {
                element.ClearValue(HighContrastProperty);
            }

            if ((bool)e.NewValue)
            {
                element.SetBinding(HighContrastProperty, _highContrastBinding);
            }
        }

        private static bool NeedsToListenForHighContrastChanges(FrameworkElement element)
        {
            return GetRequestedTheme(element) != ElementTheme.Default || element.GetValue(ThemeDictionariesProperty) != null;
        }

        private static void StartOrStopListeningForHighContrastChanges(FrameworkElement element)
        {
            if (NeedsToListenForHighContrastChanges(element))
            {
                SetIsListeningForHighContrastChanges(element, true);
            }
            else
            {
                element.ClearValue(IsListeningForHighContrastChangesProperty);
            }
        }

        #endregion

        static ThemeManager()
        {
            MenuDropAlignmentHelper.EnsureStandardPopupAlignment();
        }

        private ThemeManager()
        {
        }

        public static ThemeManager Current { get; } = new ThemeManager();

        internal static Uri DefaultLightSource { get; } = GetDefaultSource(LightKey);

        internal static Uri DefaultDarkSource { get; } = GetDefaultSource(DarkKey);

        internal static Uri DefaultHighContrastSource { get; } = GetDefaultSource(HighContrastKey);

        private static ResourceDictionary _defaultLightResources;
        internal static ResourceDictionary DefaultLightResources =>
            GetDefaultResources(ref _defaultLightResources, DefaultLightSource);

        private static ResourceDictionary _defaultDarkResources;
        internal static ResourceDictionary DefaultDarkResources =>
            GetDefaultResources(ref _defaultDarkResources, DefaultLightSource);

        //private static ResourceDictionary _defaultHighContrastResources;
        //internal static ResourceDictionary DefaultHighContrastResources =>
        //    GetDefaultResources(ref _defaultHighContrastResources, DefaultHighContrastSource);

        internal Uri LightSource { get; set; } = DefaultLightSource;

        internal Uri DarkSource { get; set; } = DefaultDarkSource;

        internal Uri HighContrastSource { get; set; } = DefaultHighContrastSource;

        internal bool UsingSystemTheme => ColorsHelper.SystemColorsSupported && ApplicationTheme == null;

        internal bool UsingSystemAccentColor => ColorsHelper.SystemColorsSupported && AccentColor == null;

        internal static Uri GetDefaultSource(string theme)
        {
            return PackUriHelper.GetAbsoluteUri($"ThemeResources/{theme}.xaml");
        }

        private static ApplicationTheme GetDefaultAppTheme()
        {
            try
            {
                var personalize = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize");
                if (personalize != null)
                {
                    var appsUseLightTheme = personalize.GetValue("AppsUseLightTheme");
                    if (appsUseLightTheme is int value && value != 1)
                    {
                        return ModernWpf.ApplicationTheme.Dark;
                    }
                }
            }
            catch (Exception)
            {
            }

            return ModernWpf.ApplicationTheme.Light;
        }

        private static ResourceDictionary GetDefaultResources(ref ResourceDictionary field, Uri source)
        {
            if (field == null)
            {
                field = ResourceDictionaryCache.GetOrCreateDictionary(source);
            }

            return field;
        }

        private static void InitializeDefaultLightResources(ResourceDictionary resources)
        {
            InitializeResources(ref _defaultLightResources, resources, DefaultLightSource);
        }

        private static void InitializeDefaultDarkResources(ResourceDictionary resources)
        {
            InitializeResources(ref _defaultDarkResources, resources, DefaultDarkSource);
        }

        //private static void InitializeDefaultHighContrastResources(ResourceDictionary resources)
        //{
        //    InitializeResources(ref _defaultHighContrastResources, resources, DefaultHighContrastSource);
        //}

        private static void InitializeResources(ref ResourceDictionary field, ResourceDictionary resources, Uri source)
        {
            if (field == null)
            {
                var result = FindDictionary(resources, source);
                if (result != null)
                {
                    field = result;
                    if (!ResourceDictionaryCache.TryGetDictionary(source, out _))
                    {
                        ResourceDictionaryCache.Add(source, result);
                    }
                }
            }
        }

        private static ResourceDictionary FindDictionary(ResourceDictionary parent, Uri source)
        {
            if (parent.Source == source)
            {
                return parent;
            }
            else
            {
                foreach (var md in parent.MergedDictionaries)
                {
                    if (md != null)
                    {
                        if (md.Source == source)
                        {
                            return md;
                        }
                        else
                        {
                            var result = FindDictionary(md, source);
                            if (result != null)
                            {
                                return result;
                            }
                        }
                    }
                }
            }

            return null;
        }
        
        internal void Initialize()
        {
            Debug.Assert(ThemeResources.Current != null);

            LightSource = ThemeResources.Current.LightSource;
            DarkSource = ThemeResources.Current.DarkSource;
            HighContrastSource = ThemeResources.Current.HighContrastSource;

            if (ReadLocalValue(ApplicationThemeProperty) == DependencyProperty.UnsetValue)
            {
                ApplicationTheme = ThemeResources.Current.RequestedTheme;
            }

            if (ReadLocalValue(AccentColorProperty) == DependencyProperty.UnsetValue)
            {
                AccentColor = ThemeResources.Current.AccentColor;
            }

            SystemParameters.StaticPropertyChanged += OnSystemParametersChanged;

            if (Application.Current != null)
            {
                Application.Current.Startup += OnApplicationStartup;
            }
        }

        private void OnApplicationStartup(object sender, StartupEventArgs e)
        {
            _applicationStarted = true;
            InitializeThemeResources();
        }

        private void InitializeThemeResources()
        {
            ColorsHelper.BackgroundColorChanged += OnSystemBackgroundColorChanged;
            ColorsHelper.AccentColorChanged += OnSystemAccentColorChanged;
            ColorsHelper.Initialize();
            UpdateActualAccentColor();
            UpdateActualApplicationTheme();
        }

        private void OnSystemBackgroundColorChanged(object sender, EventArgs e)
        {
            if (UsingSystemTheme)
            {
                UpdateActualApplicationTheme();
            }
        }

        private void OnSystemAccentColorChanged(object sender, EventArgs e)
        {
            if (UsingSystemAccentColor)
            {
                UpdateActualAccentColor();
            }
        }

        private void OnSystemParametersChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SystemParameters.HighContrast))
            {
                ApplyApplicationTheme();
            }
        }
    }

    /// <summary>
    /// Declares the theme preference for an app.
    /// </summary>
    public enum ApplicationTheme
    {
        /// <summary>
        /// Use the **Light** default theme.
        /// </summary>
        Light = 0,
        /// <summary>
        /// Use the **Dark** default theme.
        /// </summary>
        Dark = 1
    }

    /// <summary>
    /// Specifies a UI theme that should be used for individual UIElement parts of an app UI.
    /// </summary>
    public enum ElementTheme
    {
        /// <summary>
        /// Use the Application.RequestedTheme value for the element. This is the default.
        /// </summary>
        Default = 0,
        /// <summary>
        /// Use the **Light** default theme.
        /// </summary>
        Light = 1,
        /// <summary>
        /// Use the **Dark** default theme.
        /// </summary>
        Dark = 2
    }
}
