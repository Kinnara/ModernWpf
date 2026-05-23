using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace ModernWpf.Gallery.Pages.WpfGallery.Collections
{
    public sealed partial class DataGridPage : Page
    {
        public DataGridPage(DataGridPageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
            SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        public DataGridPageViewModel ViewModel { get; }

        private void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            Dispatcher.Invoke(UpdatePageVisuals);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdatePageVisuals();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanged -= OnUserPreferenceChanged;
            Loaded -= OnLoaded;
            Unloaded -= OnUnloaded;
        }

        private void UpdatePageVisuals()
        {
            ApplyPageVisuals(SystemParameters.HighContrast);
        }

        internal void ApplyPageVisuals(bool highContrast)
        {
            if (highContrast)
            {
                SampleDataGrid.SetResourceReference(BackgroundProperty, SystemColors.ControlBrushKey);
                SampleDataGrid.SetResourceReference(ForegroundProperty, SystemColors.ControlTextBrushKey);
            }
            else
            {
                SampleDataGrid.SetResourceReference(BackgroundProperty, DependencyProperty.UnsetValue);
                SampleDataGrid.SetResourceReference(ForegroundProperty, "TextFillColorPrimaryBrush");
            }
        }
    }
}
