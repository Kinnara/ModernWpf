using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using ModernWpf;

namespace ModernWpf.Gallery.Pages.WpfGallery.DesignGuidance
{
    public sealed partial class GeometryPage : UserControl
    {
        public GeometryPage()
        {
            ViewModel = new GeometryPageViewModel();
            DataContext = this;
            InitializeComponent();
            UpdateImageResources();
            ThemeManager.AddActualThemeChangedHandler(this, OnActualThemeChanged);
        }

        public GeometryPageViewModel ViewModel { get; }

        private void OnActualThemeChanged(object sender, RoutedEventArgs e)
        {
            UpdateImageResources();
        }

        private void UpdateImageResources()
        {
            var themeSuffix = ThemeManager.GetActualTheme(this) == ElementTheme.Dark ? "dark" : "light";
            GeometryImage.Source = new BitmapImage(new Uri($"pack://application:,,,/ModernWpf.Gallery;component/Assets/Design/Geometry.{themeSuffix}.png", UriKind.Absolute));
        }
    }
}
