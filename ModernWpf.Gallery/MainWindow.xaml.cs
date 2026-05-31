using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Media;
using System.Windows.Shell;
using Microsoft.Win32;
using ModernWpf.Gallery.Shell;
using ModernWpf.Gallery.Testing;
using ModernWpf.Gallery.ViewModels;

namespace ModernWpf.Gallery
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private static readonly Version OSVersion = GetOSVersion();

        public MainWindowViewModel ViewModel { get; }

        public MainWindow()
        {
            ViewModel = new MainWindowViewModel(GoBack, OpenSettings, GoForward);
            DataContext = this;
            InitializeComponent();
            if (GalleryDiagnostics.IsEnabled)
            {
                AutomationProperties.SetAutomationId(this, "ModernWpfGalleryMainWindow");
            }

            UpdateWindowBackground();
            ConfigureWindowChrome();
            UpdateMainWindowVisuals();
            SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
            StateChanged += OnWindowStateChanged;
            Activated += OnWindowActivationChanged;
            Deactivated += OnWindowActivationChanged;
            Closed += OnClosed;
        }

        private void GoBack()
        {
            GetNavigationRootPage().GoBack();
        }

        private void GoForward()
        {
            GetNavigationRootPage().GoForward();
        }

        private void OpenSettings()
        {
            GetNavigationRootPage().OpenSettings();
        }

        internal void SetBackButtonVisible(bool canGoBack)
        {
            ViewModel.CanNavigateback = canGoBack;
        }

        internal void NavigateTo(string uniqueId)
        {
            GetNavigationRootPage().NavigateTo(uniqueId);
        }

        private NavigationRootPage GetNavigationRootPage()
        {
            return MainGrid.Children.OfType<NavigationRootPage>().Single();
        }

        private void UpdateWindowBackground()
        {
            SetResourceReference(BackgroundProperty, "WindowBackground");
        }

        private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                UpdateMainWindowVisuals();
            });
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
            SystemEvents.UserPreferenceChanged -= SystemEvents_UserPreferenceChanged;
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
                chrome.NonClientFrameEdges = GetPrefferedNonClientFrameEdges();
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
                CaptionHeight = 50,
                CornerRadius = new CornerRadius(12),
                GlassFrameThickness = new Thickness(-1),
                ResizeBorderThickness = resizeMode == ResizeMode.NoResize ? default : new Thickness(4),
                UseAeroCaptionButtons = true,
                NonClientFrameEdges = GetPrefferedNonClientFrameEdges()
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

        internal static NonClientFrameEdges GetPrefferedNonClientFrameEdges()
        {
            return GetPrefferedNonClientFrameEdges(SystemParameters.HighContrast, IsWindows11OrGreater());
        }

        internal static NonClientFrameEdges GetPrefferedNonClientFrameEdges(bool isHighContrast, bool isWindows11OrGreater)
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
            Application.Current.Shutdown();
        }
    }
}
