using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using ModernWpf;

namespace ModernWpf.Gallery.Pages.WpfGallery.DesignGuidance
{
    public partial class SpacingPage : Page
    {
        public SpacingPageViewModel ViewModel { get; }

        public SpacingPage(SpacingPageViewModel viewModel)
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

            CardImage.Source = CreateDesignImage($"Cards.{themeSuffix}.png");
            DialogImage.Source = CreateDesignImage($"Dialog.{themeSuffix}.png");
        }

        private static BitmapImage CreateDesignImage(string fileName)
        {
            return new BitmapImage(new Uri($"pack://application:,,,/ModernWpf.Gallery;component/Assets/Design/{fileName}", UriKind.Absolute));
        }
    }
}
