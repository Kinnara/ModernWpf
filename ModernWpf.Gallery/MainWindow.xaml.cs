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
            ConfigureWindowChrome();
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
            MainGrid.Margin = GetMainGridMargin(WindowState, SystemParameters.HighContrast);

            UpdateTitleBarButtonsVisibility();

            if (SystemParameters.HighContrast)
            {
                HighContrastBorder.SetResourceReference(
                    System.Windows.Controls.Border.BorderBrushProperty,
                    IsActive ? SystemColors.ActiveCaptionBrushKey : SystemColors.InactiveCaptionBrushKey);
            }
            else
            {
                HighContrastBorder.BorderBrush = Brushes.Transparent;
            }

            HighContrastBorder.BorderThickness = GetHighContrastBorderThickness(SystemParameters.HighContrast);

            var chrome = WindowChrome.GetWindowChrome(this);
            if (chrome != null)
            {
                chrome.NonClientFrameEdges = GetPreferredNonClientFrameEdges();
            }
        }

        private void ConfigureWindowChrome()
        {
            WindowChrome.SetWindowChrome(this, CreateWpfGalleryWindowChrome(ResizeMode));
        }

        internal static WindowChrome CreateWpfGalleryWindowChrome(ResizeMode resizeMode)
        {
            return new WindowChrome
            {
                CaptionHeight = 44,
                CornerRadius = new CornerRadius(12),
                GlassFrameThickness = new Thickness(-1),
                ResizeBorderThickness = resizeMode == ResizeMode.NoResize ? default : new Thickness(4),
                UseAeroCaptionButtons = true,
                NonClientFrameEdges = GetPreferredNonClientFrameEdges()
            };
        }

        internal static Thickness GetMainGridMargin(WindowState windowState, bool isHighContrast)
        {
            if (windowState == WindowState.Maximized)
            {
                return isHighContrast ? new Thickness(0, 8, 0, 0) : new Thickness(8);
            }

            return default;
        }

        internal static Thickness GetHighContrastBorderThickness(bool isHighContrast)
        {
            return isHighContrast ? new Thickness(8, 1, 8, 8) : new Thickness(0);
        }

        private void UpdateTitleBarButtonsVisibility()
        {
            MinimizeButton.Visibility = Visibility.Visible;
            MaximizeButton.Visibility = Visibility.Visible;
            CloseButton.Visibility = Visibility.Visible;
        }

        internal static NonClientFrameEdges GetPreferredNonClientFrameEdges()
        {
            return GetPreferredNonClientFrameEdges(SystemParameters.HighContrast, IsWindows11OrGreater());
        }

        internal static NonClientFrameEdges GetPreferredNonClientFrameEdges(bool isHighContrast, bool isWindows11OrGreater)
        {
            if (isHighContrast || !isWindows11OrGreater)
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
