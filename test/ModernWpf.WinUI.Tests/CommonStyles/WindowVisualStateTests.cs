using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Shell;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls.Primitives;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.CommonStyles;

[TestClass]
public class WindowVisualStateTests
{
    [TestMethod]
    public void TitleBarHeightResourceControlsRenderedAndDraggableHeight()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var window = new Window
            {
                Width = 420,
                Height = 240,
                Left = -32000,
                Top = -32000,
                Content = new Border(),
                ShowInTaskbar = false,
                WindowStartupLocation = WindowStartupLocation.Manual
            };
            window.Resources[ModernWpf.Controls.TitleBar.HeightKey] = 56d;
            WindowHelper.SetUseModernWindowStyle(window, true);

            try
            {
                window.Show();
                WpfTestHost.DoEvents();
                window.UpdateLayout();
                WpfTestHost.DoEvents();

                var titleBar = VisualTreeTestHelper.FindDescendant<TitleBarControl>(window);
                var chrome = WindowChrome.GetWindowChrome(window);

                Assert.IsNotNull(titleBar);
                Assert.IsNotNull(chrome);
                Assert.AreEqual(56d, titleBar.ActualHeight, 0.1);
                Assert.AreEqual(56d, ModernWpf.Controls.TitleBar.GetHeight(window), 0.1);
                Assert.AreEqual(56d, chrome.CaptionHeight, 0.1);

                var handle = new WindowInteropHelper(window).Handle;
                var captionHitTests = new[] { 8d, 24d, 36d, 44d, 52d }
                    .Select(offset =>
                    {
                        var point = titleBar.PointToScreen(
                            new Point(titleBar.ActualWidth / 2, offset));
                        return SendMessage(
                            handle,
                            WmNcHitTest,
                            IntPtr.Zero,
                            PackScreenPoint(
                                (int)Math.Floor(point.X),
                                (int)Math.Floor(point.Y))).ToInt32();
                    })
                    .ToArray();
                CollectionAssert.AreEqual(
                    new[] { HtCaption, HtCaption, HtCaption, HtCaption, HtCaption },
                    captionHitTests,
                    $"Actual hit tests: {string.Join(",", captionHitTests)}.");

                window.Resources[ModernWpf.Controls.TitleBar.HeightKey] = 64d;
                WpfTestHost.DoEvents();
                window.UpdateLayout();

                chrome = WindowChrome.GetWindowChrome(window);
                Assert.IsNotNull(chrome);
                Assert.AreEqual(64d, titleBar.ActualHeight, 0.1);
                Assert.AreEqual(64d, ModernWpf.Controls.TitleBar.GetHeight(window), 0.1);
                Assert.AreEqual(64d, chrome!.CaptionHeight, 0.1);

                var replacementChrome = new ModernWindowChrome { CaptionHeight = 32d };
                WindowChrome.SetWindowChrome(window, replacementChrome);
                WpfTestHost.DoEvents();

                var synchronizedReplacement = WindowChrome.GetWindowChrome(window);
                Assert.IsNotNull(synchronizedReplacement);
                Assert.AreSame(replacementChrome, synchronizedReplacement);
                Assert.AreEqual(64d, synchronizedReplacement!.CaptionHeight, 0.1);
            }
            finally
            {
                window.Close();
                WpfTestHost.DoEvents();
            }
        });
    }

    [TestMethod]
    public void WindowStyleUsesOfficialWpfFluentResourceSurfaceWithModernWpfChrome()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var baseStyle = AssertStyle("BaseWindowStyle");
            var defaultStyle = AssertStyle("DefaultWindowStyle");
            Assert.AreSame(baseStyle, defaultStyle.BasedOn);

            AssertDynamicResourceSetter(baseStyle, Control.ForegroundProperty, "WindowForeground");
            AssertDynamicResourceSetter(baseStyle, Control.BackgroundProperty, "WindowBackground");
            AssertDynamicResourceSetter(baseStyle, Control.BorderBrushProperty, "WindowBorder");
            AssertDynamicResourceSetter(baseStyle, WindowChrome.WindowChromeProperty, "DefaultWindowChrome");
            AssertStyleSetter(baseStyle, Control.BorderThicknessProperty, new Thickness(1));
            AssertStyleSetter(baseStyle, WindowHelper.FixMaximizedWindowProperty, true);
            AssertStyleSetter(defaultStyle, FrameworkElement.OverridesDefaultStyleProperty, true);
        });
    }

    [TestMethod]
    public void DefaultWindowChromeKeepsWindows11ResizeEdgesOutOfFullClientGlass()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var baseStyle = AssertStyle("BaseWindowStyle");
            var chrome = Application.Current.FindResource("DefaultWindowChrome") as WindowChrome;
            var highContrastChrome = Application.Current.FindResource("HighContrastWindowChrome") as WindowChrome;
            Assert.IsNotNull(chrome);
            Assert.IsNotNull(highContrastChrome);
            Assert.IsInstanceOfType<ModernWindowChrome>(chrome);
            Assert.IsInstanceOfType<ModernWindowChrome>(highContrastChrome);
            Assert.AreEqual(WindowChrome.GlassFrameCompleteThickness, chrome!.GlassFrameThickness);
            Assert.AreEqual(WindowChrome.GlassFrameCompleteThickness, highContrastChrome!.GlassFrameThickness);
            Assert.AreEqual(NonClientFrameEdges.None, highContrastChrome!.NonClientFrameEdges);

            var resizeEdges =
                NonClientFrameEdges.Left |
                NonClientFrameEdges.Right |
                NonClientFrameEdges.Bottom;
            Assert.AreEqual(
                resizeEdges,
                ModernWindowChrome.GetPreferredNonClientFrameEdges(
                    isHighContrast: false,
                    isWindows11OrGreater: true));
            Assert.AreEqual(
                NonClientFrameEdges.None,
                ModernWindowChrome.GetPreferredNonClientFrameEdges(
                    isHighContrast: true,
                    isWindows11OrGreater: true));
            Assert.AreEqual(
                NonClientFrameEdges.None,
                ModernWindowChrome.GetPreferredNonClientFrameEdges(
                    isHighContrast: false,
                    isWindows11OrGreater: false));
            Assert.AreEqual(
                ModernWindowChrome.GetPreferredNonClientFrameEdges(
                    isHighContrast: false,
                    OSVersionHelper.IsWindows11OrGreater),
                chrome.NonClientFrameEdges);

            var highContrastTrigger = baseStyle.Triggers
                .OfType<DataTrigger>()
                .Single(trigger => trigger.Setters
                    .OfType<Setter>()
                    .Any(setter => setter.Property == WindowChrome.WindowChromeProperty));
            Assert.IsInstanceOfType<Binding>(highContrastTrigger.Binding);
            var chromeSetter = highContrastTrigger.Setters
                .OfType<Setter>()
                .Single(setter => setter.Property == WindowChrome.WindowChromeProperty);
            var highContrastResource = chromeSetter.Value as DynamicResourceExtension;
            Assert.IsNotNull(highContrastResource);
            Assert.AreEqual("HighContrastWindowChrome", highContrastResource!.ResourceKey);
        });
    }

    [TestMethod]
    public void MaximizedWindowLeavesRevealStripForAutoHideTaskbar()
    {
        var monitorBounds = new Int32Rect(0, 0, 1920, 1080);

        Assert.IsFalse(
            MaximizedWindowFixer.IsTaskbarAutoHideState(0x00000000),
            "No taskbar state flags must not be treated as auto-hide.");
        Assert.IsFalse(
            MaximizedWindowFixer.IsTaskbarAutoHideState(0x00000002),
            "ABS_ALWAYSONTOP alone must not be treated as auto-hide.");
        Assert.IsTrue(MaximizedWindowFixer.IsTaskbarAutoHideState(0x00000001));
        Assert.IsTrue(MaximizedWindowFixer.IsTaskbarAutoHideState(0x00000003));

        Assert.IsTrue(
            MaximizedWindowFixer.TryGetTaskbarAdjustedWindowBounds(
                monitorBounds,
                monitorBounds,
                MaximizedWindowFixer.ABEdge.ABE_BOTTOM,
                out Int32Rect adjustedBounds));
        Assert.AreEqual(
            new Int32Rect(0, 0, 1920, 1078),
            adjustedBounds,
            "A monitor-sized maximized window must leave the auto-hide taskbar activation edge uncovered.");

        Assert.IsTrue(
            MaximizedWindowFixer.TryGetTaskbarAdjustedWindowBounds(
                adjustedBounds,
                monitorBounds,
                MaximizedWindowFixer.ABEdge.ABE_BOTTOM,
                out Int32Rect readjustedBounds));
        Assert.AreEqual(
            adjustedBounds,
            readjustedBounds,
            "Repeated window-position messages must not shrink the maximized window more than once.");
    }

    [TestMethod]
    public void WindowTemplateUsesWpfContentPresenterAndKeepsCustomTitleBar()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var content = new TextBlock { Text = "Window content" };
            using var host = new TestWindowHost(content, width: 320, height: 240);
            host.UpdateLayout();

            var window = host.Window;
            Assert.AreSame(window.TryFindResource("WindowForeground"), window.Foreground);
            Assert.AreSame(window.TryFindResource("WindowBackground"), window.Background);
            Assert.IsNotNull(WindowChrome.GetWindowChrome(window));
            Assert.IsTrue(WindowHelper.GetFixMaximizedWindow(window));

            Assert.IsNotNull(VisualTreeTestHelper.FindDescendant<TitleBarControl>(window));
            Assert.IsNotNull(VisualTreeTestHelper.FindDescendant<ResizeGrip>(window));

            var presenter = VisualTreeTestHelper.EnumerateDescendants(window)
                .OfType<ContentPresenter>()
                .FirstOrDefault(item => ReferenceEquals(item.Content, content))
                ?? throw new AssertFailedException("Expected the Window content host to be a WPF ContentPresenter.");

            Assert.AreEqual(typeof(ContentPresenter), presenter.GetType());
        });
    }

    [TestMethod]
    public void WindowStyleNoneRemovesModernTitleBarAndChrome()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var content = new Border();
            var window = new Window
            {
                Width = 320,
                Height = 240,
                Left = -32000,
                Top = -32000,
                BorderThickness = new Thickness(0),
                Content = content,
                ShowInTaskbar = false,
                WindowStartupLocation = WindowStartupLocation.Manual,
                WindowStyle = WindowStyle.None
            };
            WindowHelper.SetUseModernWindowStyle(window, true);

            try
            {
                window.Show();
                WpfTestHost.DoEvents();
                window.UpdateLayout();
                WpfTestHost.DoEvents();

                var titleBar = VisualTreeTestHelper.FindDescendant<TitleBarControl>(window);
                Assert.IsNotNull(titleBar);
                Assert.AreEqual(Visibility.Collapsed, titleBar!.Visibility);
                Assert.IsNull(WindowChrome.GetWindowChrome(window));
                Assert.IsFalse(WindowHelper.GetFixMaximizedWindow(window));
                Assert.AreEqual(new Point(0, 0), content.TranslatePoint(new Point(), window));
            }
            finally
            {
                window.Close();
                WpfTestHost.DoEvents();
            }
        });
    }

    [TestMethod]
    public void ContentImmediatelyBelowTitleBarUsesClientHitTesting()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var button = new Button
            {
                Content = "Button",
                Width = 120,
                Height = 32,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top
            };
            var window = new Window
            {
                Width = 420,
                Height = 240,
                Left = -30000,
                Top = -30000,
                Content = button,
                ResizeMode = ResizeMode.CanResizeWithGrip,
                ShowInTaskbar = false,
                WindowStartupLocation = WindowStartupLocation.Manual
            };
            WindowHelper.SetUseModernWindowStyle(window, true);

            try
            {
                window.Show();
                WpfTestHost.DoEvents();
                window.UpdateLayout();
                WpfTestHost.DoEvents();

                var handle = new WindowInteropHelper(window).Handle;
                var buttonTopLeft = button.PointToScreen(new Point());
                var buttonTopRight = button.PointToScreen(new Point(button.ActualWidth, 0));
                var screenX = (int)Math.Floor((buttonTopLeft.X + buttonTopRight.X) / 2);
                var firstButtonPixelY = (int)Math.Ceiling(buttonTopLeft.Y);
                var hitTests = Enumerable.Range(0, 4)
                    .Select(offset => SendMessage(
                        handle,
                        WmNcHitTest,
                        IntPtr.Zero,
                        PackScreenPoint(screenX, firstButtonPixelY + offset)).ToInt32())
                    .ToArray();

                CollectionAssert.AreEqual(
                    new[] { HtClient, HtClient, HtClient, HtClient },
                    hitTests,
                    $"Expected the first four button rows to be clickable. " +
                    $"Actual={string.Join(",", hitTests)}, button top={buttonTopLeft.Y}, " +
                    $"chrome={WindowChrome.GetWindowChrome(window)?.CaptionHeight}, " +
                    $"resize={WindowChrome.GetWindowChrome(window)?.ResizeBorderThickness.Top}.");

                var titleBar = VisualTreeTestHelper.FindDescendant<TitleBarControl>(window)
                    ?? throw new AssertFailedException("Expected the modern window title bar.");
                var titleBarPoint = titleBar.PointToScreen(
                    new Point(titleBar.ActualWidth / 2, titleBar.ActualHeight / 2));
                Assert.AreEqual(
                    HtCaption,
                    SendMessage(
                        handle,
                        WmNcHitTest,
                        IntPtr.Zero,
                        PackScreenPoint(
                            (int)Math.Floor(titleBarPoint.X),
                            (int)Math.Floor(titleBarPoint.Y))).ToInt32(),
                    "Empty title-bar space must remain draggable.");

                var resizeGrip = VisualTreeTestHelper.FindDescendant<ResizeGrip>(window)
                    ?? throw new AssertFailedException("Expected the modern window resize grip.");
                var resizeGripPoint = resizeGrip.PointToScreen(
                    new Point(resizeGrip.ActualWidth / 2, resizeGrip.ActualHeight / 2));
                Assert.AreEqual(
                    HtBottomRight,
                    SendMessage(
                        handle,
                        WmNcHitTest,
                        IntPtr.Zero,
                        PackScreenPoint(
                            (int)Math.Floor(resizeGripPoint.X),
                            (int)Math.Floor(resizeGripPoint.Y))).ToInt32(),
                    "The content client bridge must preserve the explicit resize grip.");
            }
            finally
            {
                window.Close();
                WpfTestHost.DoEvents();
            }
        });
    }

    [TestMethod]
    public void WindowStyleDocumentsOfficialWpfFluentSubstitutions()
    {
        var repoRoot = FindRepoRoot();
        var stylePath = Path.Combine(repoRoot, "ModernWpf", "Styles", "Window.xaml");
        var text = File.ReadAllText(stylePath);

        Assert.IsTrue(text.Contains("WindowForeground", System.StringComparison.Ordinal));
        Assert.IsTrue(text.Contains("WindowBackground", System.StringComparison.Ordinal));
        Assert.IsTrue(text.Contains("<ContentPresenter x:Name=\"ContentPresenter\"", System.StringComparison.Ordinal));
        Assert.IsTrue(text.Contains("TitleBarControl", System.StringComparison.Ordinal));
        Assert.IsTrue(text.Contains("WindowChrome.WindowChrome", System.StringComparison.Ordinal));
        Assert.IsTrue(text.Contains("WindowHelper.FixMaximizedWindow", System.StringComparison.Ordinal));
        Assert.IsTrue(text.Contains("Path=(SystemParameters.HighContrast)", System.StringComparison.Ordinal));
        Assert.IsTrue(text.Contains("HighContrastWindowChrome", System.StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("ContentPresenterEx", System.StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("MS.Internal", System.StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("Fluent.Controls", System.StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("System.Runtime", System.StringComparison.Ordinal));
    }

    private static Style AssertStyle(object resourceKey)
    {
        var style = Application.Current.FindResource(resourceKey) as Style;
        Assert.IsNotNull(style, $"Expected style resource {resourceKey}.");
        return style!;
    }

    private static void AssertDynamicResourceSetter(Style style, DependencyProperty property, object expectedResourceKey)
    {
        var setter = style.Setters.OfType<Setter>().SingleOrDefault(setter => setter.Property == property);
        Assert.IsNotNull(setter, $"Expected local setter for {property.Name}.");

        var resource = setter!.Value as DynamicResourceExtension;
        Assert.IsNotNull(resource, $"Expected {property.Name} to use DynamicResource.");
        Assert.AreEqual(expectedResourceKey, resource!.ResourceKey);
    }

    private static void AssertStyleSetter(Style style, DependencyProperty property, object expectedValue)
    {
        var setter = style.Setters.OfType<Setter>().SingleOrDefault(setter => setter.Property == property);
        Assert.IsNotNull(setter, $"Expected local setter for {property.Name}.");
        Assert.AreEqual(expectedValue, setter!.Value);
    }

    private static string FindRepoRoot()
    {
        var directory = new DirectoryInfo(System.AppContext.BaseDirectory);

        while (directory != null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "ModernWpf.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        Assert.Fail("Could not locate ModernWpf.sln from the test output directory.");
        return string.Empty;
    }

    private static IntPtr PackScreenPoint(int x, int y)
    {
        return new IntPtr(unchecked((int)((uint)(ushort)x | ((uint)(ushort)y << 16))));
    }

    private const int WmNcHitTest = 0x0084;
    private const int HtClient = 1;
    private const int HtCaption = 2;
    private const int HtBottomRight = 17;

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
}
