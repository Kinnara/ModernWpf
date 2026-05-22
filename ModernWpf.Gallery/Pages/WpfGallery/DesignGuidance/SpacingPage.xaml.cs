using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using ModernWpf;
using ModernWpf.Gallery.Pages.WpfGallery;

namespace ModernWpf.Gallery.Pages.WpfGallery.DesignGuidance
{
    public sealed partial class SpacingPage : UserControl
    {
        public SpacingPage()
        {
            ViewModel = new WpfGalleryPageViewModel("Spacing", "Guide showing how to use spacing in your app");
            DataContext = this;
            InitializeComponent();
            UpdateImageResources();
            ThemeManager.AddActualThemeChangedHandler(this, OnActualThemeChanged);
        }

        public WpfGalleryPageViewModel ViewModel { get; }

        private void OnActualThemeChanged(object sender, RoutedEventArgs e)
        {
            UpdateImageResources();
        }

        private void UpdateImageResources()
        {
            var themeSuffix = ThemeManager.GetActualTheme(this) == ElementTheme.Dark ? "dark" : "light";

            CardImage.Source = CreateDesignImage($"Cards.{themeSuffix}.png");
            DialogImage.Source = CreateDesignImage($"Dialog.{themeSuffix}.png");
        }

        private static BitmapImage CreateDesignImage(string fileName)
        {
            return new BitmapImage(new Uri($"pack://application:,,,/ModernWpf.Gallery;component/Assets/Design/{fileName}", UriKind.Absolute));
        }
    }
}
