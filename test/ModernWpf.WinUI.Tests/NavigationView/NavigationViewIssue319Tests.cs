using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.NavigationView;

[TestClass]
public class NavigationViewIssue319Tests
{
    [TestMethod]
    [Timeout(15000)]
    public void TopNavigationWithHeaderFooterConstrainsOverflowHostAndRemainsResponsive()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var navView = new ModernWpf.Controls.NavigationView
            {
                Width = 768,
                Height = 480,
                PaneDisplayMode = ModernWpf.Controls.NavigationViewPaneDisplayMode.Top,
                PaneHeader = new Border
                {
                    Width = 160,
                    Child = new TextBlock { Text = "Pane header" }
                },
                PaneFooter = new Border
                {
                    Width = 160,
                    Child = new TextBlock { Text = "Pane footer" }
                },
                IsSettingsVisible = false,
                IsTitleBarAutoPaddingEnabled = false
            };
            for (var index = 0; index < 24; index++)
            {
                navView.MenuItems.Add(new ModernWpf.Controls.NavigationViewItem
                {
                    Content = $"Navigation item {index:00}"
                });
            }

            var stopwatch = Stopwatch.StartNew();
            using var host = new TestWindowHost(navView, width: 768, height: 480);
            host.UpdateLayout();
            WpfTestHost.DoEvents();
            stopwatch.Stop();

            var primaryRepeater = VisualTreeTestHelper.EnumerateDescendants(navView)
                .OfType<ModernWpf.Controls.ItemsRepeater>()
                .Single(element => element.Name == "TopNavMenuItemsHost");
            var primaryScrollHost = VisualTreeTestHelper.EnumerateDescendants(navView)
                .OfType<ModernWpf.Controls.ItemsRepeaterScrollHost>()
                .Single(element => element.Name == "TopNavMenuItemsScrollHost");
            var topNavGrid = VisualTreeTestHelper.EnumerateDescendants(navView)
                .OfType<Grid>()
                .Single(element => element.Name == "TopNavGrid");
            var overflowButton = VisualTreeTestHelper.EnumerateDescendants(navView)
                .OfType<Button>()
                .Single(element => element.Name == "TopNavOverflowButton");
            var topDataProvider = navView.GetTopDataProvider();

            Assert.AreEqual(Visibility.Visible, overflowButton.Visibility);
            Assert.IsTrue(topDataProvider.GetPrimaryListSize() > 0);
            Assert.IsTrue(topDataProvider.GetOverflowItems().Count > 0);
            Assert.AreEqual(
                topDataProvider.GetPrimaryListSize(),
                primaryRepeater.ItemsSourceView.Count);
            Assert.IsTrue(stopwatch.Elapsed < TimeSpan.FromSeconds(5));
            AssertTopNavigationIsConstrained(navView, primaryScrollHost, topNavGrid);

            var idleLayoutUpdates = CountLayoutUpdates(navView, TimeSpan.FromSeconds(1));
            Assert.IsTrue(
                idleLayoutUpdates < 20,
                $"The overflow layout must settle promptly. Count={idleLayoutUpdates}.");

            var initialPrimaryCount = topDataProvider.GetPrimaryListSize();
            Resize(host, navView, 1200);
            var expandedPrimaryCount = topDataProvider.GetPrimaryListSize();

            Assert.IsTrue(expandedPrimaryCount > initialPrimaryCount);
            AssertTopNavigationIsConstrained(navView, primaryScrollHost, topNavGrid);

            Resize(host, navView, 640);

            Assert.IsTrue(topDataProvider.GetPrimaryListSize() < expandedPrimaryCount);
            Assert.IsTrue(topDataProvider.GetOverflowItems().Count > 0);
            AssertTopNavigationIsConstrained(navView, primaryScrollHost, topNavGrid);
        });
    }

    private static void AssertTopNavigationIsConstrained(
        ModernWpf.Controls.NavigationView navView,
        FrameworkElement primaryScrollHost,
        FrameworkElement topNavGrid)
    {
        Assert.IsTrue(double.IsFinite(primaryScrollHost.MaxWidth));
        Assert.IsTrue(primaryScrollHost.MaxWidth <= navView.ActualWidth);
        Assert.IsTrue(primaryScrollHost.ActualWidth <= navView.ActualWidth + 1.0);
        Assert.IsTrue(topNavGrid.ActualWidth <= navView.ActualWidth + 1.0);
    }

    private static int CountLayoutUpdates(FrameworkElement element, TimeSpan duration)
    {
        var count = 0;
        element.LayoutUpdated += OnLayoutUpdated;
        PumpFor(duration);
        element.LayoutUpdated -= OnLayoutUpdated;
        return count;

        void OnLayoutUpdated(object? sender, EventArgs args)
        {
            count++;
        }
    }

    private static void Resize(
        TestWindowHost host,
        ModernWpf.Controls.NavigationView navView,
        double width)
    {
        host.Window.Width = width;
        navView.Width = width;
        host.UpdateLayout();
        PumpFor(TimeSpan.FromMilliseconds(250));
    }

    private static void PumpFor(TimeSpan duration)
    {
        var frame = new DispatcherFrame();
        var timer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = duration
        };
        timer.Tick += (_, _) =>
        {
            timer.Stop();
            frame.Continue = false;
        };
        timer.Start();
        Dispatcher.PushFrame(frame);
    }
}
