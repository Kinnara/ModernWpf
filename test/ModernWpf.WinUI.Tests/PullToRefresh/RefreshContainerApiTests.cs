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
            var defaultIcon = (SymbolIcon)visualizer.Content;
            Assert.AreEqual(Symbol.Refresh, defaultIcon.Symbol);
            Assert.AreEqual(30.0, defaultIcon.Width);
            Assert.AreEqual(30.0, defaultIcon.Height);

            var root = FindNamedDescendant<Panel>(visualizer, "Root");
            Assert.AreEqual(1, root.Children.Count);
            Assert.AreSame(defaultIcon, root.Children[0]);
            Assert.AreEqual(HorizontalAlignment.Center, defaultIcon.HorizontalAlignment);
            Assert.AreEqual(VerticalAlignment.Center, defaultIcon.VerticalAlignment);
        });
    }

    [TestMethod]
    public void RefreshStylesUseWinUIResourceAliases()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var containerResources = new ResourceDictionary
            {
                Source = new Uri("/ModernWpf.Controls;component/PullToRefresh/RefreshContainer.xaml", UriKind.Relative)
            };
            var visualizerResources = new ResourceDictionary
            {
                Source = new Uri("/ModernWpf.Controls;component/PullToRefresh/RefreshVisualizer.xaml", UriKind.Relative)
            };
            var containerStyle = (Style)containerResources["DefaultRefreshContainerStyle"];
            var implicitContainerStyle = (Style)containerResources[typeof(RefreshContainer)];
            var visualizerStyle = (Style)visualizerResources[typeof(RefreshVisualizer)];
            var visualizer = new RefreshVisualizer
            {
                Style = visualizerStyle
            };
            var container = new RefreshContainer
            {
                Content = new TextBlock { Text = "Refreshable content" },
                Style = containerStyle,
                Visualizer = visualizer
            };
            container.Resources.MergedDictionaries.Add(containerResources);
            visualizer.Resources.MergedDictionaries.Add(visualizerResources);

            using var host = new TestWindowHost(container, width: 240, height: 180);
            host.UpdateLayout();

            Assert.AreSame(containerStyle, implicitContainerStyle.BasedOn);
            AssertDynamicResourceSetter(containerStyle, Control.ForegroundProperty, "RefreshContainerForegroundBrush");
            AssertDynamicResourceSetter(containerStyle, Control.BackgroundProperty, "RefreshContainerBackgroundBrush");
            Assert.AreEqual(false, GetSetterValue(containerStyle, Control.IsTabStopProperty));
            AssertDynamicResourceSetter(visualizerStyle, Control.BackgroundProperty, "RefreshVisualizerBackground");
            AssertDynamicResourceSetter(visualizerStyle, Control.ForegroundProperty, "RefreshVisualizerForeground");
            Assert.AreEqual(false, GetSetterValue(visualizerStyle, Control.IsTabStopProperty));
            Assert.AreEqual(100.0, GetSetterValue(visualizerStyle, FrameworkElement.HeightProperty));

            Assert.AreSame(container.TryFindResource("RefreshContainerForegroundBrush"), container.Foreground);
            Assert.AreSame(container.TryFindResource("RefreshContainerBackgroundBrush"), container.Background);
            Assert.IsFalse(container.IsTabStop);
            Assert.AreSame(visualizer.TryFindResource("RefreshVisualizerForeground"), visualizer.Foreground);
            Assert.AreSame(visualizer.TryFindResource("RefreshVisualizerBackground"), visualizer.Background);
            Assert.IsFalse(visualizer.IsTabStop);
            Assert.AreEqual(100.0, visualizer.Height);

            var contentPresenter = FindNamedDescendant<ContentPresenterEx>(container, "ContentPresenter");
            var refreshPresenter = FindNamedDescendant<Panel>(container, "RefreshVisualizerPresenter");
            var visualizerRoot = FindNamedDescendant<Panel>(visualizer, "Root");

            Assert.AreSame(container.Content, contentPresenter.Content);
            AssertTransparentBrush(contentPresenter.Background);
            Assert.IsFalse(refreshPresenter.IsHitTestVisible);
            Assert.AreEqual(80.0, visualizerRoot.MinHeight);
            Assert.AreSame(visualizer.Background, visualizerRoot.Background);
        });
    }

    [TestMethod]
    public void RefreshVisualizerHostsContentInWinUIRootPanel()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var visualizer = new RefreshVisualizer();
            using var host = new TestWindowHost(visualizer, width: 160, height: 140);
            var root = FindNamedDescendant<Panel>(visualizer, "Root");

            var replacement = new TextBlock { Text = "Refreshing" };
            visualizer.Content = replacement;
            host.UpdateLayout();

            Assert.AreEqual(1, root.Children.Count);
            Assert.AreSame(replacement, root.Children[0]);
            Assert.AreEqual(HorizontalAlignment.Center, replacement.HorizontalAlignment);
            Assert.AreEqual(VerticalAlignment.Center, replacement.VerticalAlignment);

            visualizer.Content = null;
            host.UpdateLayout();

            Assert.IsNull(visualizer.Content);
            Assert.AreEqual(1, root.Children.Count);
            Assert.IsInstanceOfType(root.Children[0], typeof(SymbolIcon));
            var fallbackIcon = (SymbolIcon)root.Children[0];
            Assert.AreEqual(Symbol.Refresh, fallbackIcon.Symbol);
            Assert.AreEqual(30.0, fallbackIcon.Width);
            Assert.AreEqual(30.0, fallbackIcon.Height);
            Assert.AreEqual(HorizontalAlignment.Center, fallbackIcon.HorizontalAlignment);
            Assert.AreEqual(VerticalAlignment.Center, fallbackIcon.VerticalAlignment);
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
    public void RefreshVisualizerSourceInfoProviderDrivesStateMachine()
    {
        WpfTestHost.Run(() =>
        {
            var visualizer = new RefreshVisualizer();
            var states = new List<RefreshVisualizerState>();
            var requestedCount = 0;
            var provider = new RefreshInfoProviderImpl(RefreshPullDirection.TopToBottom, new Size(100, 100));

            using var host = new TestWindowHost(visualizer, width: 160, height: 140);

            visualizer.RefreshStateChanged += (_, args) => states.Add(args.NewState);
            visualizer.RefreshRequested += (_, _) => requestedCount++;
            visualizer.InfoProvider = provider;

            provider.UpdateIsInteractingForRefresh(true);
            provider.RaiseInteractionRatioChanged(0.25);
            Assert.AreEqual(RefreshVisualizerState.Interacting, visualizer.State);

            provider.RaiseInteractionRatioChanged(0.83);
            Assert.AreEqual(RefreshVisualizerState.Pending, visualizer.State);

            provider.UpdateIsInteractingForRefresh(false);

            Assert.AreEqual(1, requestedCount);
            Assert.AreEqual(RefreshVisualizerState.Idle, visualizer.State);
            CollectionAssert.Contains(states, RefreshVisualizerState.Interacting);
            CollectionAssert.Contains(states, RefreshVisualizerState.Pending);
            CollectionAssert.Contains(states, RefreshVisualizerState.Refreshing);
            CollectionAssert.Contains(states, RefreshVisualizerState.Idle);
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
    public void RefreshContainerUsesSourceAdapterAndPreservesCustomVisualizerSize()
    {
        WpfTestHost.Run(() =>
        {
            var customVisualizer = new RefreshVisualizer
            {
                Width = 42,
                Height = 44
            };
            var container = new RefreshContainer
            {
                Content = new ScrollViewer
                {
                    Height = 80,
                    Content = CreateTallContent()
                },
                PullDirection = RefreshPullDirection.LeftToRight,
                Visualizer = customVisualizer
            };

            using var host = new TestWindowHost(container, width: 240, height: 140);

            Assert.IsInstanceOfType(container.RefreshInfoProviderAdapter, typeof(ScrollViewerIRefreshInfoProviderAdapter));
            Assert.AreSame(customVisualizer, container.Visualizer);
            Assert.AreEqual(42.0, customVisualizer.Width);
            Assert.AreEqual(44.0, customVisualizer.Height);
            Assert.IsNotNull(customVisualizer.InfoProvider);
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

            var visualizer = new RefreshVisualizer();
            var container = new RefreshContainer
            {
                Content = new TextBlock { Text = "Refreshable content" },
                Visualizer = visualizer
            };

            using var host = new TestWindowHost(container, width: 240, height: 180);

            Assert.AreSame(container.TryFindResource("RefreshContainerForegroundBrush"), container.Foreground);
            Assert.AreSame(container.TryFindResource("RefreshContainerBackgroundBrush"), container.Background);
            Assert.IsFalse(container.IsTabStop);

            var containerRoot = FindNamedDescendant<Grid>(container, "Root");
            Assert.AreSame(container.Background, containerRoot.Background);

            Assert.AreSame(visualizer.TryFindResource("RefreshVisualizerForeground"), visualizer.Foreground);
            Assert.AreSame(visualizer.TryFindResource("RefreshVisualizerBackground"), visualizer.Background);
            Assert.IsFalse(visualizer.IsTabStop);
            Assert.AreEqual(100.0, visualizer.Height);

            var visualizerRoot = FindNamedDescendant<Panel>(visualizer, "Root");
            Assert.AreEqual(80.0, visualizerRoot.MinHeight);
            Assert.AreSame(visualizer.Background, visualizerRoot.Background);

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

    private static object? GetSetterValue(Style style, DependencyProperty property)
    {
        var setter = style.Setters.OfType<Setter>().SingleOrDefault(setter => setter.Property == property);
        Assert.IsNotNull(setter, $"Expected local setter for {property.Name}.");
        return setter!.Value;
    }

    private static void AssertDynamicResourceSetter(Style style, DependencyProperty property, object expectedResourceKey)
    {
        var dynamicResource = GetSetterValue(style, property) as DynamicResourceExtension;
        Assert.IsNotNull(dynamicResource, $"Expected {property.Name} to use a dynamic resource.");
        Assert.AreEqual(expectedResourceKey, dynamicResource!.ResourceKey);
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
