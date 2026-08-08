using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Shell;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Automation.Peers;
using ModernWpf.Controls;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.TitleBar
{
    [TestClass]
    public class TitleBarControlApiTests
    {
        [TestMethod]
        public void DefaultsAndSettersMatchCurrentWinUIV11Contract()
        {
            WpfTestHost.Run(() =>
            {
                var titleBar = new ModernWpf.Controls.TitleBar();

                Assert.AreEqual(string.Empty, titleBar.Title);
                Assert.AreEqual(string.Empty, titleBar.Subtitle);
                Assert.IsNull(titleBar.IconSource);
                Assert.IsNull(titleBar.LeftHeader);
                Assert.IsNull(titleBar.Content);
                Assert.IsNull(titleBar.RightHeader);
                Assert.IsFalse(titleBar.IsBackButtonVisible);
                Assert.IsTrue(titleBar.IsBackButtonEnabled);
                Assert.IsFalse(titleBar.IsPaneToggleButtonVisible);
                Assert.IsFalse(titleBar.AutoRefreshDragRegions);
                Assert.IsNotNull(titleBar.TemplateSettings);
                Assert.IsNull(titleBar.TemplateSettings.IconElement);

                var left = new Border();
                var content = new Border();
                var right = new Border();
                var icon = new SymbolIconSource { Symbol = Symbol.Library };
                titleBar.Title = "Documents";
                titleBar.Subtitle = "Contoso";
                titleBar.IconSource = icon;
                titleBar.LeftHeader = left;
                titleBar.Content = content;
                titleBar.RightHeader = right;
                titleBar.IsBackButtonVisible = true;
                titleBar.IsBackButtonEnabled = false;
                titleBar.IsPaneToggleButtonVisible = true;
                titleBar.AutoRefreshDragRegions = true;

                Assert.AreEqual("Documents", titleBar.Title);
                Assert.AreEqual("Contoso", titleBar.Subtitle);
                Assert.AreSame(icon, titleBar.IconSource);
                Assert.AreSame(left, titleBar.LeftHeader);
                Assert.AreSame(content, titleBar.Content);
                Assert.AreSame(right, titleBar.RightHeader);
                Assert.IsTrue(titleBar.IsBackButtonVisible);
                Assert.IsFalse(titleBar.IsBackButtonEnabled);
                Assert.IsTrue(titleBar.IsPaneToggleButtonVisible);
                Assert.IsTrue(titleBar.AutoRefreshDragRegions);
                Assert.IsInstanceOfType<SymbolIcon>(titleBar.TemplateSettings.IconElement);
            });
        }

        [TestMethod]
        public void TemplateUsesSourceHeightsAndRaisesButtonEvents()
        {
            WpfTestHost.Run(() =>
            {
                var titleBar = new ModernWpf.Controls.TitleBar
                {
                    Title = "Documents"
                };
                var backRequests = 0;
                var paneRequests = 0;
                titleBar.BackRequested += (sender, args) =>
                {
                    Assert.AreSame(titleBar, sender);
                    Assert.IsNull(args);
                    backRequests++;
                };
                titleBar.PaneToggleRequested += (sender, args) =>
                {
                    Assert.AreSame(titleBar, sender);
                    Assert.IsNull(args);
                    paneRequests++;
                };

                using var host = new TestWindowHost(titleBar, width: 760, height: 240);
                host.UpdateLayout();

                Assert.AreEqual(32d, Part<Grid>(titleBar, "PART_LayoutRoot").ActualHeight, 0.1);
                Assert.AreEqual(Visibility.Collapsed, Part<Button>(titleBar, "PART_BackButton").Visibility);
                Assert.AreEqual(Visibility.Collapsed, Part<Button>(titleBar, "PART_PaneToggleButton").Visibility);

                titleBar.Content = new Border { Width = 240 };
                titleBar.IsBackButtonVisible = true;
                titleBar.IsPaneToggleButtonVisible = true;
                host.UpdateLayout();

                Assert.AreEqual(48d, Part<Grid>(titleBar, "PART_LayoutRoot").ActualHeight, 0.1);
                var backButton = Part<Button>(titleBar, "PART_BackButton");
                var paneToggleButton = Part<Button>(titleBar, "PART_PaneToggleButton");
                Assert.AreEqual(Visibility.Visible, backButton.Visibility);
                Assert.AreEqual(Visibility.Visible, paneToggleButton.Visibility);
                Assert.AreEqual("Back", AutomationProperties.GetName(backButton));
                Assert.AreEqual("Toggle Navigation", AutomationProperties.GetName(paneToggleButton));
                Assert.AreEqual("Back", ((ToolTip)backButton.ToolTip).Content);
                Assert.AreEqual("Toggle Navigation", ((ToolTip)paneToggleButton.ToolTip).Content);

                backButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                paneToggleButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

                Assert.AreEqual(1, backRequests);
                Assert.AreEqual(1, paneRequests);
            });
        }

        [TestMethod]
        public void WindowTitleIsAppliedAndRestoredWithoutOverwritingExternalChanges()
        {
            WpfTestHost.Run(() =>
            {
                var titleBar = new ModernWpf.Controls.TitleBar
                {
                    Title = "ModernWPF sample"
                };
                var window = new Window
                {
                    Title = "Original title",
                    Width = 600,
                    Height = 240,
                    Left = -32000,
                    Top = -32000,
                    ShowInTaskbar = false,
                    Content = titleBar
                };

                try
                {
                    window.Show();
                    WpfTestHost.DoEvents();
                    Assert.AreEqual("ModernWPF sample", window.Title);

                    titleBar.Title = string.Empty;
                    Assert.AreEqual("Original title", window.Title);

                    titleBar.Title = "Managed title";
                    Assert.AreEqual("Managed title", window.Title);
                    window.Title = "Application override";
                    window.Content = null;
                    WpfTestHost.DoEvents();

                    Assert.AreEqual("Application override", window.Title);
                }
                finally
                {
                    window.Close();
                    WpfTestHost.DoEvents();
                }
            });
        }

        [TestMethod]
        public void DragRegionOverridesInteractiveControlDefaults()
        {
            WpfTestHost.Run(() =>
            {
                var button = new Button { Content = "Status" };
                var label = new TextBlock { Text = "Drag here" };
                var nonFocusableControl = new ContentControl { Focusable = false };
                var leftHeader = new TextBlock { Text = "Left header" };
                var rightHeader = new TextBlock { Text = "Right header" };
                var content = new Grid();
                content.Children.Add(label);
                content.Children.Add(button);
                content.Children.Add(nonFocusableControl);
                var titleBar = new ModernWpf.Controls.TitleBar
                {
                    LeftHeader = leftHeader,
                    RightHeader = rightHeader,
                    Content = content
                };

                using var host = new TestWindowHost(titleBar, width: 760, height: 240);
                host.UpdateLayout();

                Assert.IsFalse(IsDragTarget(titleBar, button));
                Assert.IsFalse(IsDragTarget(titleBar, nonFocusableControl));
                Assert.IsFalse(IsDragTarget(titleBar, leftHeader));
                Assert.IsFalse(IsDragTarget(titleBar, rightHeader));
                Assert.IsTrue(IsDragTarget(titleBar, label));

                button.IsEnabled = false;
                Assert.IsTrue(IsDragTarget(titleBar, button));
                ModernWpf.Controls.TitleBar.SetIsDragRegion(button, false);
                Assert.IsFalse(IsDragTarget(titleBar, button));
                button.ClearValue(ModernWpf.Controls.TitleBar.IsDragRegionProperty);
                button.IsEnabled = true;

                ModernWpf.Controls.TitleBar.SetIsDragRegion(button, true);
                Assert.AreEqual(true, ModernWpf.Controls.TitleBar.GetIsDragRegion(button));
                Assert.IsTrue(IsDragTarget(titleBar, button));

                button.ClearValue(ModernWpf.Controls.TitleBar.IsDragRegionProperty);
                ModernWpf.Controls.TitleBar.SetIsDragRegion(content, false);
                Assert.IsFalse(IsDragTarget(titleBar, label));

                content.ClearValue(ModernWpf.Controls.TitleBar.IsDragRegionProperty);
                titleBar.AutoRefreshDragRegions = true;
                content.Children.Add(new TextBlock { Text = "Dynamic" });
                titleBar.RecomputeDragRegions();
                Assert.IsTrue(IsDragTarget(titleBar, label));
            });
        }

        [TestMethod]
        public void AutomationPeerUsesTitleBarRoleAndTitleFallback()
        {
            WpfTestHost.Run(() =>
            {
                var titleBar = new ModernWpf.Controls.TitleBar
                {
                    Title = "Documents"
                };
                using var host = new TestWindowHost(titleBar, width: 600, height: 240);
                host.UpdateLayout();

                var peer = UIElementAutomationPeer.CreatePeerForElement(titleBar)
                    ?? throw new AssertFailedException("TitleBar did not create an automation peer.");
                Assert.IsInstanceOfType<TitleBarAutomationPeer>(peer);
                Assert.AreEqual(AutomationControlType.TitleBar, peer.GetAutomationControlType());
                Assert.AreEqual(nameof(ModernWpf.Controls.TitleBar), peer.GetClassName());
                Assert.AreEqual("Documents", peer.GetName());

                AutomationProperties.SetName(titleBar, "Explicit title bar name");
                Assert.AreEqual("Explicit title bar name", peer.GetName());
            });
        }

        [TestMethod]
        public void BackAndPaneButtonsUseTheirOwnStateResourceContracts()
        {
            WpfTestHost.Run(() =>
            {
                var titleBar = new ModernWpf.Controls.TitleBar
                {
                    IsBackButtonVisible = true,
                    IsPaneToggleButtonVisible = true
                };
                using var host = new TestWindowHost(titleBar, width: 600, height: 240);
                host.UpdateLayout();

                var expectedFocusVisual = titleBar.TryFindResource(SystemParameters.FocusVisualStyleKey);
                Assert.IsNotNull(expectedFocusVisual);
                Assert.AreSame(
                    expectedFocusVisual,
                    Part<Button>(titleBar, "PART_BackButton").FocusVisualStyle);
                Assert.AreSame(
                    expectedFocusVisual,
                    Part<Button>(titleBar, "PART_PaneToggleButton").FocusVisualStyle);

                CollectionAssert.AreEquivalent(
                    new[]
                    {
                        "TitleBarBackButtonBackgroundPointerOver",
                        "TitleBarBackButtonForegroundPointerOver",
                        "TitleBarBackButtonBackgroundPressed",
                        "TitleBarBackButtonForegroundPressed",
                        "TitleBarBackButtonBackgroundDisabled",
                        "TitleBarBackButtonForegroundDisabled"
                    },
                    GetTemplateDynamicResourceKeys(Part<Button>(titleBar, "PART_BackButton")));
                CollectionAssert.AreEquivalent(
                    new[]
                    {
                        "TitleBarPaneToggleButtonBackgroundPointerOver",
                        "TitleBarPaneToggleButtonForegroundPointerOver",
                        "TitleBarPaneToggleButtonBackgroundPressed",
                        "TitleBarPaneToggleButtonForegroundPressed",
                        "TitleBarPaneToggleButtonBackgroundDisabled",
                        "TitleBarPaneToggleButtonForegroundDisabled"
                    },
                    GetTemplateDynamicResourceKeys(Part<Button>(titleBar, "PART_PaneToggleButton")));
            });
        }

        [TestMethod]
        public void PublicLayoutResourcesOverrideTheLiveTemplate()
        {
            WpfTestHost.Run(() =>
            {
                var titleBar = new ModernWpf.Controls.TitleBar
                {
                    IsBackButtonVisible = true,
                    IsPaneToggleButtonVisible = true,
                    LeftHeader = new TextBlock { Text = "Header" },
                    Content = new Border { Width = 80 }
                };
                titleBar.Resources["TitleBarBackButtonWidth"] = 44d;
                titleBar.Resources["TitleBarPaneToggleButtonWidth"] = 46d;
                titleBar.Resources["TitleBarLeftPaddingWidth"] = 12d;
                titleBar.Resources["TitleBarLeftHeaderPaddingWidth"] = 21d;
                titleBar.Resources["TitleBarHeaderNegativeInsetPaddingWidth"] = 21d;
                titleBar.Resources["TitleBarMinDragRegionWidth"] = 73d;
                titleBar.Resources["TitleBarRightPaddingWidth"] = 9d;
                titleBar.Resources["TitleBarContentHorizontalAlignment"] =
                    HorizontalAlignment.Stretch;

                using var host = new TestWindowHost(titleBar, width: 760, height: 240);
                host.UpdateLayout();

                Assert.AreEqual(44d, Part<Button>(titleBar, "PART_BackButton").ActualWidth, 0.1);
                Assert.AreEqual(46d, Part<Button>(titleBar, "PART_PaneToggleButton").ActualWidth, 0.1);
                Assert.AreEqual(12d, Part<Border>(titleBar, "PART_LeftPadding").ActualWidth, 0.5);
                Assert.AreEqual(21d, Part<Border>(titleBar, "PART_LeftHeaderPadding").ActualWidth, 0.5);
                Assert.AreEqual(73d, Part<Border>(titleBar, "PART_MinDragRegion").ActualWidth, 0.5);
                Assert.AreEqual(9d, Part<Border>(titleBar, "PART_RightPadding").ActualWidth, 0.5);
                Assert.AreEqual(
                    HorizontalAlignment.Stretch,
                    Part<ContentPresenter>(titleBar, "PART_ContentPresenter").HorizontalAlignment);
            });
        }

        [TestMethod]
        public void LeftHeaderSpacingMatchesTheCurrentWinUIButtonCombinationRule()
        {
            WpfTestHost.Run(() =>
            {
                var titleBar = new ModernWpf.Controls.TitleBar
                {
                    LeftHeader = new TextBlock { Text = "Header" }
                };
                titleBar.Resources["TitleBarLeftHeaderPaddingWidth"] = 14d;
                titleBar.Resources["TitleBarHeaderNegativeInsetPaddingWidth"] = 2d;
                using var host = new TestWindowHost(titleBar, width: 600, height: 240);

                AssertPadding(14d);
                titleBar.IsBackButtonVisible = true;
                host.UpdateLayout();
                AssertPadding(2d);
                titleBar.IsPaneToggleButtonVisible = true;
                host.UpdateLayout();
                AssertPadding(14d);
                titleBar.IsBackButtonVisible = false;
                host.UpdateLayout();
                AssertPadding(2d);

                void AssertPadding(double expected)
                {
                    host.UpdateLayout();
                    Assert.AreEqual(
                        expected,
                        Part<Border>(titleBar, "PART_LeftHeaderPadding").ActualWidth,
                        0.5);
                }
            });
        }

        [TestMethod]
        public void ExtendedChromeTreatsTheTitleBarTreeAsClientInput()
        {
            WpfTestHost.Run(() =>
            {
                TestApplication.EnsureInitialized();
                var titleBar = new ModernWpf.Controls.TitleBar
                {
                    Title = "Documents",
                    IsBackButtonVisible = true,
                    Content = new TextBox { Width = 180, Text = "Search" }
                };
                var window = new Window
                {
                    Width = 640,
                    Height = 260,
                    Left = -30000,
                    Top = -30000,
                    ShowInTaskbar = false,
                    Content = titleBar
                };
                ModernWpf.Controls.WindowTitleBar.SetExtendsContentIntoTitleBar(window, true);

                try
                {
                    window.Show();
                    WpfTestHost.DoEvents();
                    window.UpdateLayout();

                    var backButton = Part<Button>(titleBar, "PART_BackButton");
                    var dragRegion = Part<Border>(titleBar, "PART_MinDragRegion");
                    Assert.IsTrue(WindowChrome.GetIsHitTestVisibleInChrome(titleBar));
                    Assert.IsTrue(WindowChrome.GetIsHitTestVisibleInChrome(backButton));
                    Assert.IsTrue(WindowChrome.GetIsHitTestVisibleInChrome(dragRegion));

                    var handle = new WindowInteropHelper(window).Handle;
                    Assert.AreNotEqual(IntPtr.Zero, handle);
                    Assert.AreEqual(HtClient, SendHitTest(handle, backButton));
                    Assert.AreEqual(HtClient, SendHitTest(handle, dragRegion));
                }
                finally
                {
                    window.Close();
                    WpfTestHost.DoEvents();
                }
            });
        }

        private static T Part<T>(ModernWpf.Controls.TitleBar titleBar, string name)
            where T : DependencyObject
        {
            titleBar.ApplyTemplate();
            return (T)(titleBar.Template.FindName(name, titleBar)
                ?? throw new AssertFailedException($"Missing TitleBar template part '{name}'."));
        }

        private static bool IsDragTarget(ModernWpf.Controls.TitleBar titleBar, DependencyObject source)
        {
            var method = typeof(ModernWpf.Controls.TitleBar).GetMethod(
                "IsDragTarget",
                BindingFlags.Instance | BindingFlags.NonPublic)
                ?? throw new AssertFailedException("Missing TitleBar drag-target classifier.");
            return (bool)method.Invoke(titleBar, new object[] { source })!;
        }

        private static string[] GetTemplateDynamicResourceKeys(Button button)
        {
            return button.Template.Triggers
                .OfType<Trigger>()
                .SelectMany(trigger => trigger.Setters.OfType<Setter>())
                .Select(setter => setter.Value as DynamicResourceExtension)
                .Where(extension => extension != null)
                .Select(extension => extension!.ResourceKey.ToString()!)
                .ToArray();
        }

        private static int SendHitTest(IntPtr windowHandle, FrameworkElement element)
        {
            var point = element.PointToScreen(
                new Point(element.ActualWidth / 2, element.ActualHeight / 2));
            var packedPoint = new IntPtr(
                unchecked((int)((uint)(ushort)Math.Floor(point.X) |
                    ((uint)(ushort)Math.Floor(point.Y) << 16))));
            return SendMessage(windowHandle, WmNcHitTest, IntPtr.Zero, packedPoint).ToInt32();
        }

        private const int WmNcHitTest = 0x0084;
        private const int HtClient = 1;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(
            IntPtr windowHandle,
            int message,
            IntPtr wParam,
            IntPtr lParam);
    }
}
