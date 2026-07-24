using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using ModernWpf;

namespace ModernWpf.Gallery.Pages.WpfGallery.DesignGuidance
{
    /// <summary>
    /// Interaction logic for GeometryPage.xaml
    /// </summary>
    public partial class GeometryPage : Page
    {
        public GeometryPageViewModel ViewModel { get; }

        public GeometryPage(GeometryPageViewModel viewModel)
        {
            InitializeComponent();
            UpdateImageResources();
            ViewModel = viewModel;
            DataContext = this;
            Loaded += OnLoaded;
            SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
            ThemeManager.AddActualThemeChangedHandler(this, OnActualThemeChanged);
            Unloaded += OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateImageResources();
        }

        private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                UpdateImageResources();
            });
        }

        private void OnActualThemeChanged(object sender, RoutedEventArgs e)
        {
            UpdateImageResources();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnLoaded;
            SystemEvents.UserPreferenceChanged -= SystemEvents_UserPreferenceChanged;
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
