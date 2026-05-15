using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf;
using ModernWpf.Controls;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.PullToRefresh;

[TestClass]
public class RefreshContainerApiTests
{
    [TestMethod]
    public void RefreshVisualizerDefaultsAndDefaultContent()
    {
        WpfTestHost.Run(() =>
        {
            var visualizer = new RefreshVisualizer();

            Assert.AreEqual(RefreshVisualizerOrientation.Auto, visualizer.Orientation);
            Assert.AreEqual(RefreshVisualizerState.Idle, visualizer.State);
            Assert.IsNull(visualizer.Content);

            using var host = new TestWindowHost(visualizer, width: 160, height: 140);

            Assert.AreEqual(100.0, visualizer.Height);
            Assert.IsFalse(visualizer.IsTabStop);
            Assert.IsInstanceOfType(visualizer.Content, typeof(SymbolIcon));
            Assert.AreEqual(Symbol.Refresh, ((SymbolIcon)visualizer.Content).Symbol);
        });
    }

    [TestMethod]
    public void RefreshVisualizerRequestRefreshRaisesRequestedAndStateChanged()
    {
        WpfTestHost.Run(() =>
        {
            var visualizer = new RefreshVisualizer();
            var requestedCount = 0;
            var states = new List<(RefreshVisualizerState OldState, RefreshVisualizerState NewState)>();

            visualizer.RefreshRequested += (_, _) => requestedCount++;
            visualizer.RefreshStateChanged += (_, args) => states.Add((args.OldState, args.NewState));

            visualizer.RequestRefresh();

            Assert.AreEqual(1, requestedCount);
            Assert.AreEqual(RefreshVisualizerState.Idle, visualizer.State);
            CollectionAssert.AreEqual(
                new[]
                {
                    (RefreshVisualizerState.Idle, RefreshVisualizerState.Refreshing),
                    (RefreshVisualizerState.Refreshing, RefreshVisualizerState.Idle)
                },
                states);
        });
    }

    [TestMethod]
    public void RefreshVisualizerDeferralKeepsRefreshingUntilCompleted()
    {
        WpfTestHost.Run(() =>
        {
            var visualizer = new RefreshVisualizer();
            RefreshDeferral? deferral = null;

            visualizer.RefreshRequested += (_, args) => deferral = args.GetDeferral();

            visualizer.RequestRefresh();

            Assert.IsNotNull(deferral);
            Assert.AreEqual(RefreshVisualizerState.Refreshing, visualizer.State);

            deferral!.Complete();

            Assert.AreEqual(RefreshVisualizerState.Idle, visualizer.State);
        });
    }

    [TestMethod]
    public void RefreshContainerDefaultsAndRequestRefreshProxy()
    {
        WpfTestHost.Run(() =>
        {
            var content = new TextBlock { Text = "Refreshable content" };
            var container = new RefreshContainer
            {
                Content = content
            };

            Assert.AreEqual(RefreshPullDirection.TopToBottom, container.PullDirection);
            Assert.IsNull(container.Visualizer);

            using var host = new TestWindowHost(container, width: 240, height: 180);

            Assert.IsNotNull(container.Visualizer);
            Assert.AreSame(container.Visualizer, FindNamedDescendant<RefreshVisualizer>(container, string.Empty, allowUnnamed: true));

            var contentPresenter = FindNamedDescendant<ContentPresenterEx>(container, "ContentPresenter");
            Assert.AreSame(content, contentPresenter.Content);
            AssertTransparentBrush(contentPresenter.Background);

            var containerRequestCount = 0;
            var visualizerRequestCount = 0;
            container.RefreshRequested += (_, _) => containerRequestCount++;
            container.Visualizer.RefreshRequested += (_, _) => visualizerRequestCount++;

            container.RequestRefresh();

            Assert.AreEqual(1, containerRequestCount);
            Assert.AreEqual(1, visualizerRequestCount);
            Assert.AreEqual(RefreshVisualizerState.Idle, container.Visualizer.State);
        });
    }

    [TestMethod]
    public void RefreshContainerDeferralCompletesVisualizerRefresh()
    {
        WpfTestHost.Run(() =>
        {
            var container = new RefreshContainer();
            RefreshDeferral? deferral = null;

            using var host = new TestWindowHost(container, width: 240, height: 180);

            container.RefreshRequested += (_, args) => deferral = args.GetDeferral();

            container.RequestRefresh();

            Assert.IsNotNull(deferral);
            Assert.AreEqual(RefreshVisualizerState.Refreshing, container.Visualizer.State);

            deferral!.Complete();

            Assert.AreEqual(RefreshVisualizerState.Idle, container.Visualizer.State);
        });
    }

    [TestMethod]
    public void RefreshContainerPullDirectionAlignsVisualizerPresenter()
    {
        WpfTestHost.Run(() =>
        {
            var container = new RefreshContainer();
            using var host = new TestWindowHost(container, width: 240, height: 180);

            var presenter = FindNamedDescendant<Panel>(container, "RefreshVisualizerPresenter");

            Assert.AreEqual(HorizontalAlignment.Stretch, presenter.HorizontalAlignment);
            Assert.AreEqual(VerticalAlignment.Top, presenter.VerticalAlignment);
            Assert.IsTrue(double.IsNaN(container.Visualizer.Width));
            Assert.AreEqual(100.0, container.Visualizer.Height);

            container.PullDirection = RefreshPullDirection.LeftToRight;
            host.UpdateLayout();

            Assert.AreEqual(HorizontalAlignment.Left, presenter.HorizontalAlignment);
            Assert.AreEqual(VerticalAlignment.Stretch, presenter.VerticalAlignment);
            Assert.AreEqual(100.0, container.Visualizer.Width);
            Assert.IsTrue(double.IsNaN(container.Visualizer.Height));

            container.PullDirection = RefreshPullDirection.BottomToTop;
            host.UpdateLayout();

            Assert.AreEqual(HorizontalAlignment.Stretch, presenter.HorizontalAlignment);
            Assert.AreEqual(VerticalAlignment.Bottom, presenter.VerticalAlignment);
            Assert.IsTrue(double.IsNaN(container.Visualizer.Width));
            Assert.AreEqual(100.0, container.Visualizer.Height);
        });
    }

    [TestMethod]
    public void PullFromScrollViewerBoundaryRequestsRefreshAndHonorsDeferral()
    {
        WpfTestHost.Run(() =>
        {
            var scrollViewer = new ScrollViewer
            {
                Height = 80,
                Content = CreateTallContent()
            };
            var container = new RefreshContainer
            {
                Content = scrollViewer
            };
            RefreshDeferral? deferral = null;
            var states = new List<RefreshVisualizerState>();

            using var host = new TestWindowHost(container, width: 240, height: 140);

            container.Visualizer.RefreshStateChanged += (_, args) => states.Add(args.NewState);
            container.RefreshRequested += (_, args) => deferral = args.GetDeferral();

            Assert.IsTrue(container.CanStartPullForTesting);

            container.PullForTesting(120, complete: true);
            host.UpdateLayout();

            Assert.IsNotNull(deferral);
            CollectionAssert.Contains(states, RefreshVisualizerState.Pending);
            CollectionAssert.Contains(states, RefreshVisualizerState.Refreshing);
            Assert.AreEqual(RefreshVisualizerState.Refreshing, container.Visualizer.State);

            deferral!.Complete();
            host.UpdateLayout();

            Assert.AreEqual(RefreshVisualizerState.Idle, container.Visualizer.State);
        });
    }

    [TestMethod]
    public void PullBelowThresholdReturnsIdleWithoutRefresh()
    {
        WpfTestHost.Run(() =>
        {
            var container = new RefreshContainer
            {
                Content = new ScrollViewer
                {
                    Height = 80,
                    Content = CreateTallContent()
                }
            };
            var requestCount = 0;

            using var host = new TestWindowHost(container, width: 240, height: 140);
            container.RefreshRequested += (_, _) => requestCount++;

            container.PullForTesting(32, complete: true);
            host.UpdateLayout();

            Assert.AreEqual(0, requestCount);
            Assert.AreEqual(0d, container.PullRatioForTesting);
            Assert.AreEqual(RefreshVisualizerState.Idle, container.Visualizer.State);
        });
    }

    [TestMethod]
    public void PullDoesNotStartAwayFromScrollViewerBoundary()
    {
        WpfTestHost.Run(() =>
        {
            var scrollViewer = new ScrollViewer
            {
                Height = 80,
                Content = CreateTallContent()
            };
            var container = new RefreshContainer
            {
                Content = scrollViewer
            };
            var requestCount = 0;

            using var host = new TestWindowHost(container, width: 240, height: 140);
            container.RefreshRequested += (_, _) => requestCount++;

            scrollViewer.ScrollToVerticalOffset(40);
            host.UpdateLayout();

            Assert.IsFalse(container.CanStartPullForTesting);

            container.PullForTesting(120, complete: true);
            host.UpdateLayout();

            Assert.AreEqual(0, requestCount);
            Assert.AreEqual(RefreshVisualizerState.Idle, container.Visualizer.State);
        });
    }

    [TestMethod]
    public void VerifyFinalWinUI2RefreshThemeResources()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            AssertBrushColor("Light", "RefreshContainerForegroundBrush", Colors.Black);
            AssertBrushColor("Light", "RefreshContainerBackgroundBrush", Colors.Transparent);
            AssertBrushColor("Light", "RefreshVisualizerForeground", Colors.Black);
            AssertBrushColor("Light", "RefreshVisualizerBackground", Colors.Transparent);

            AssertBrushColor("Dark", "RefreshContainerForegroundBrush", Colors.White);
            AssertBrushColor("Dark", "RefreshContainerBackgroundBrush", Colors.Transparent);
            AssertBrushColor("Dark", "RefreshVisualizerForeground", Colors.White);
            AssertBrushColor("Dark", "RefreshVisualizerBackground", Colors.Transparent);

            AssertThemeResourceReference("HighContrast", "RefreshContainerForegroundBrush", "SystemColorHighlightTextColorBrush");
            AssertBrushColor("HighContrast", "RefreshContainerBackgroundBrush", Colors.Transparent);
            AssertThemeResourceReference("HighContrast", "RefreshVisualizerForeground", "SystemColorHighlightTextColorBrush");
            AssertBrushColor("HighContrast", "RefreshVisualizerBackground", Colors.Transparent);
        });
    }

    private static void AssertBrushColor(string themeName, string resourceKey, Color expectedColor)
    {
        var themeDictionary = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(themeDictionary.Contains(resourceKey), $"{themeName} is missing {resourceKey}.");
        Assert.IsInstanceOfType(themeDictionary[resourceKey], typeof(SolidColorBrush));
        Assert.AreEqual(expectedColor, ((SolidColorBrush)themeDictionary[resourceKey]).Color, $"{themeName}:{resourceKey}");
    }

    private static void AssertThemeResourceReference(string themeName, string resourceKey, object expectedResourceKey)
    {
        var themeDictionary = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(themeDictionary.Contains(resourceKey), $"{themeName} is missing {resourceKey}.");
        Assert.IsTrue(themeDictionary.Contains(expectedResourceKey), $"{themeName} is missing {expectedResourceKey}.");
        Assert.AreSame(themeDictionary[expectedResourceKey], themeDictionary[resourceKey], $"{themeName}:{resourceKey}");
    }

    private static void AssertTransparentBrush(Brush brush)
    {
        Assert.IsInstanceOfType(brush, typeof(SolidColorBrush));
        var solid = (SolidColorBrush)brush;
        Assert.AreEqual(Colors.Transparent, solid.Color);
    }

    private static T FindNamedDescendant<T>(DependencyObject root, string name, bool allowUnnamed = false)
        where T : FrameworkElement
    {
        foreach (var descendant in VisualTreeTestHelper.EnumerateDescendants(root).OfType<T>())
        {
            if (allowUnnamed || descendant.Name == name)
            {
                return descendant;
            }
        }

        throw new InvalidOperationException($"Could not find descendant named '{name}'.");
    }

    private static FrameworkElement CreateTallContent()
    {
        var panel = new StackPanel();
        for (var i = 0; i < 20; i++)
        {
            panel.Children.Add(new TextBlock
            {
                Height = 24,
                Text = "Item " + i
            });
        }

        return panel;
    }
}
