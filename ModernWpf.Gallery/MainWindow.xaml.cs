using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shell;
using Microsoft.Win32;

namespace ModernWpf.Gallery
{
    public partial class MainWindow
    {
        private static readonly Version OSVersion = GetOSVersion();

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

            var chrome = WindowChrome.GetWindowChrome(this);
            if (chrome != null)
            {
                chrome.NonClientFrameEdges = GetPreferredNonClientFrameEdges();
            }
        }

        private void UpdateTitleBarButtonsVisibility()
        {
            MinimizeButton.Visibility = Visibility.Visible;
            MaximizeButton.Visibility = Visibility.Visible;
            CloseButton.Visibility = Visibility.Visible;
        }

        internal static NonClientFrameEdges GetPreferredNonClientFrameEdges()
        {
            if (SystemParameters.HighContrast || !IsWindows11OrGreater())
            {
                return NonClientFrameEdges.None;
            }

            return NonClientFrameEdges.Right | NonClientFrameEdges.Bottom | NonClientFrameEdges.Left;
        }

        private static bool IsWindows11OrGreater()
        {
            return Environment.OSVersion.Platform == PlatformID.Win32NT
                && OSVersion >= new Version(10, 0, 22000);
        }

        private static Version GetOSVersion()
        {
            var versionInfo = new RTL_OSVERSIONINFOEX
            {
                dwOSVersionInfoSize = (uint)Marshal.SizeOf(typeof(RTL_OSVERSIONINFOEX))
            };

            return RtlGetVersion(out versionInfo) == 0
                ? new Version((int)versionInfo.dwMajorVersion, (int)versionInfo.dwMinorVersion, (int)versionInfo.dwBuildNumber)
                : Environment.OSVersion.Version;
        }

        [DllImport("ntdll.dll")]
        private static extern int RtlGetVersion(out RTL_OSVERSIONINFOEX versionInfo);

        [StructLayout(LayoutKind.Sequential)]
        private struct RTL_OSVERSIONINFOEX
        {
            internal uint dwOSVersionInfoSize;
            internal uint dwMajorVersion;
            internal uint dwMinorVersion;
            internal uint dwBuildNumber;
            internal uint dwPlatformId;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            internal string szCSDVersion;
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
