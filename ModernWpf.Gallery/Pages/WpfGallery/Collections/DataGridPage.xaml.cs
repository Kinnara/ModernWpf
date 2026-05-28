using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace ModernWpf.Gallery.Pages.WpfGallery.Collections
{
    public partial class DataGridPage : Page
    {
        public DataGridPageViewModel ViewModel { get; }
        public DataGridPage(DataGridPageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();

            SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
            this.Loaded += (s, e) => UpdatePageVisuals();
            Unloaded += OnUnloaded;
        }

        private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                UpdatePageVisuals();
            });
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.UserPreferenceChanged -= SystemEvents_UserPreferenceChanged;
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
