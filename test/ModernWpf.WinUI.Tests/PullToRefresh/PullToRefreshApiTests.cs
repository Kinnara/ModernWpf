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
public class PullToRefreshApiTests
{
    [TestMethod]
    public void VerifyRefreshVisualizerDefaultsAndContent()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var visualizer = new RefreshVisualizer();

            Assert.AreEqual(RefreshVisualizerOrientation.Auto, visualizer.Orientation);
            Assert.AreEqual(RefreshVisualizerState.Idle, visualizer.State);
            Assert.IsNull(visualizer.Content);

            using var host = new TestWindowHost(visualizer, width: 160, height: 80);

            var defaultIcon = visualizer.Content as SymbolIcon;
            Assert.IsNotNull(defaultIcon);
            Assert.AreEqual(Symbol.Refresh, defaultIcon!.Symbol);

            var replacement = new TextBlock { Text = "Refreshing" };
            visualizer.Content = replacement;
            Assert.AreSame(replacement, visualizer.Content);

            visualizer.Content = null;
            visualizer.RequestRefresh();
            Assert.IsNull(visualizer.Content);
        });
    }

    [TestMethod]
    public void VerifyRefreshVisualizerRequestEventsAndDeferrals()
    {
        WpfTestHost.Run(() =>
        {
            var visualizer = new RefreshVisualizer();
            var states = new List<RefreshVisualizerState>();
            var requestedCount = 0;
            RefreshDeferral? deferral = null;

            visualizer.RefreshStateChanged += (_, args) => states.Add(args.NewState);
            visualizer.RefreshRequested += (_, args) =>
            {
                requestedCount++;
                deferral = args.GetDeferral();
            };

            visualizer.RequestRefresh();

            Assert.AreEqual(1, requestedCount);
            CollectionAssert.AreEqual(
                new[] { RefreshVisualizerState.Refreshing },
                states);
            Assert.AreEqual(RefreshVisualizerState.Refreshing, visualizer.State);

            deferral!.Complete();

            CollectionAssert.AreEqual(
                new[] { RefreshVisualizerState.Refreshing, RefreshVisualizerState.Idle },
                states);
            Assert.AreEqual(RefreshVisualizerState.Idle, visualizer.State);
        });
    }

    [TestMethod]
    public void VerifyRefreshContainerDefaultsAndRequestPropagation()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var container = new RefreshContainer
            {
                Content = new ListBox
                {
                    ItemsSource = Enumerable.Range(0, 3)
                }
            };

            Assert.AreEqual(RefreshPullDirection.TopToBottom, container.PullDirection);
            Assert.IsNull(container.Visualizer);

            using var host = new TestWindowHost(container, width: 240, height: 160);

            Assert.IsNotNull(container.Visualizer);
            Assert.AreSame(container.TryFindResource("RefreshContainerForegroundBrush"), container.Foreground);
            Assert.AreSame(container.TryFindResource("RefreshContainerBackgroundBrush"), container.Background);

            var containerRequestCount = 0;
            var visualizerRequestCount = 0;
            container.RefreshRequested += (_, _) => containerRequestCount++;
            container.Visualizer.RefreshRequested += (_, _) => visualizerRequestCount++;

            container.RequestRefresh();

            Assert.AreEqual(1, containerRequestCount);
            Assert.AreEqual(1, visualizerRequestCount);

            container.PullDirection = RefreshPullDirection.RightToLeft;
            Assert.AreEqual(RefreshPullDirection.RightToLeft, container.PullDirection);
        });
    }

    [TestMethod]
    public void VerifyRefreshContainerRewiresCustomVisualizer()
    {
        WpfTestHost.Run(() =>
        {
            var container = new RefreshContainer();
            using var host = new TestWindowHost(container, width: 160, height: 80);

            var oldVisualizer = container.Visualizer;
            var newVisualizer = new RefreshVisualizer();
            var containerRequestCount = 0;
            container.RefreshRequested += (_, _) => containerRequestCount++;

            container.Visualizer = newVisualizer;

            oldVisualizer.RequestRefresh();
            Assert.AreEqual(0, containerRequestCount);

            newVisualizer.RequestRefresh();
            Assert.AreEqual(1, containerRequestCount);
        });
    }

    [TestMethod]
    public void VerifyFinalWinUI2PullToRefreshThemeResources()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            AssertThemeBrushColor("Light", "RefreshContainerForegroundBrush", Colors.Black);
            AssertThemeBrushColor("Light", "RefreshContainerBackgroundBrush", Colors.Transparent);
            AssertThemeBrushColor("Light", "RefreshVisualizerForeground", Colors.Black);
            AssertThemeBrushColor("Light", "RefreshVisualizerBackground", Colors.Transparent);

            AssertThemeBrushColor("Dark", "RefreshContainerForegroundBrush", Colors.White);
            AssertThemeBrushColor("Dark", "RefreshContainerBackgroundBrush", Colors.Transparent);
            AssertThemeBrushColor("Dark", "RefreshVisualizerForeground", Colors.White);
            AssertThemeBrushColor("Dark", "RefreshVisualizerBackground", Colors.Transparent);

            AssertThemeResourceReference("HighContrast", "RefreshContainerForegroundBrush", "SystemColorHighlightTextColorBrush");
            AssertThemeBrushColor("HighContrast", "RefreshContainerBackgroundBrush", Colors.Transparent);
            AssertThemeResourceReference("HighContrast", "RefreshVisualizerForeground", "SystemColorHighlightTextColorBrush");
            AssertThemeBrushColor("HighContrast", "RefreshVisualizerBackground", Colors.Transparent);
        });
    }

    private static void AssertThemeBrushColor(string themeName, string resourceKey, Color expectedColor)
    {
        var themeDictionary = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(themeDictionary.Contains(resourceKey), $"{themeName} is missing {resourceKey}.");

        var brush = themeDictionary[resourceKey] as SolidColorBrush;
        Assert.IsNotNull(brush, $"{themeName}:{resourceKey} should be a SolidColorBrush.");
        Assert.AreEqual(expectedColor, brush!.Color, $"{themeName}:{resourceKey}");
    }

    private static void AssertThemeResourceReference(string themeName, string resourceKey, object expectedResourceKey)
    {
        var themeDictionary = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(themeDictionary.Contains(resourceKey), $"{themeName} is missing {resourceKey}.");
        Assert.IsTrue(themeDictionary.Contains(expectedResourceKey), $"{themeName} is missing {expectedResourceKey}.");
        Assert.AreSame(themeDictionary[expectedResourceKey], themeDictionary[resourceKey], $"{themeName}:{resourceKey}");
    }
}
