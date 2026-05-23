using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;

namespace ModernWpf.Gallery
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            UpdateMainWindowVisuals();
            SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;
            StateChanged += OnWindowStateChanged;
            Activated += OnWindowActivationChanged;
            Deactivated += OnWindowActivationChanged;
            Closed += OnClosed;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            RootPage.GoBack();
        }

        internal void SetBackButtonVisible(bool canGoBack)
        {
            BackButton.IsEnabled = canGoBack;
        }

        internal void NavigateTo(string uniqueId)
        {
            RootPage.NavigateTo(uniqueId);
        }

        private void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            Dispatcher.Invoke(UpdateMainWindowVisuals);
        }

        private void OnWindowStateChanged(object sender, System.EventArgs e)
        {
            UpdateMaximizeIcon();
            UpdateMainWindowVisuals();
        }

        private void OnWindowActivationChanged(object sender, System.EventArgs e)
        {
            UpdateMainWindowVisuals();
        }

        private void OnClosed(object sender, System.EventArgs e)
        {
            SystemEvents.UserPreferenceChanged -= OnUserPreferenceChanged;
            StateChanged -= OnWindowStateChanged;
            Activated -= OnWindowActivationChanged;
            Deactivated -= OnWindowActivationChanged;
            Closed -= OnClosed;
        }

        private void UpdateMainWindowVisuals()
        {
            MainGrid.Margin = default;
            if (WindowState == WindowState.Maximized)
            {
                MainGrid.Margin = SystemParameters.HighContrast ? new Thickness(0, 8, 0, 0) : new Thickness(8);
            }

            UpdateTitleBarButtonsVisibility();

            if (SystemParameters.HighContrast)
            {
                HighContrastBorder.SetResourceReference(
                    System.Windows.Controls.Border.BorderBrushProperty,
                    IsActive ? SystemColors.ActiveCaptionBrushKey : SystemColors.InactiveCaptionBrushKey);
                HighContrastBorder.BorderThickness = new Thickness(8, 1, 8, 8);
            }
            else
            {
                HighContrastBorder.BorderBrush = Brushes.Transparent;
                HighContrastBorder.BorderThickness = new Thickness(0);
            }
        }

        private void UpdateTitleBarButtonsVisibility()
        {
            var visibility = SystemParameters.HighContrast ? Visibility.Visible : Visibility.Collapsed;
            MinimizeButton.Visibility = visibility;
            MaximizeButton.Visibility = visibility;
            CloseButton.Visibility = visibility;
        }

        private void MinimizeWindow(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MaximizeWindow(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
            UpdateMaximizeIcon();
        }

        private void UpdateMaximizeIcon()
        {
            MaximizeIcon.Text = WindowState == WindowState.Maximized ? "\uE923" : "\uE922";
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
