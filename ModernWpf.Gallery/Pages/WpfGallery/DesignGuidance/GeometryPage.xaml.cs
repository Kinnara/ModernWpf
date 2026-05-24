using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using ModernWpf;

namespace ModernWpf.Gallery.Pages.WpfGallery.DesignGuidance
{
    public sealed partial class GeometryPage : Page
    {
        public GeometryPage(GeometryPageViewModel viewModel)
        {
            InitializeComponent();
            UpdateImageResources();
            ViewModel = viewModel;
            DataContext = this;
            Loaded += OnLoaded;
            SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;
            ThemeManager.AddActualThemeChangedHandler(this, OnActualThemeChanged);
            Unloaded += OnUnloaded;
        }

        public GeometryPageViewModel ViewModel { get; }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateImageResources();
        }

        private void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            Dispatcher.Invoke(UpdateImageResources);
        }

        private void OnActualThemeChanged(object sender, RoutedEventArgs e)
        {
            UpdateImageResources();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnLoaded;
            SystemEvents.UserPreferenceChanged -= OnUserPreferenceChanged;
            ThemeManager.RemoveActualThemeChangedHandler(this, OnActualThemeChanged);
            Unloaded -= OnUnloaded;
        }

        private void UpdateImageResources()
        {
            ApplyImageResources(DesignImageTheme.Resolve(this));
        }

        internal void ApplyImageResources(ElementTheme actualTheme)
        {
            var themeSuffix = actualTheme == ElementTheme.Dark ? "dark" : "light";
            GeometryImage.Source = new BitmapImage(new Uri($"pack://application:,,,/ModernWpf.Gallery;component/Assets/Design/Geometry.{themeSuffix}.png", UriKind.Absolute));
        }
    }
}
