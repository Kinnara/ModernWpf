using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf;
using ModernWpf.Controls;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.ProgressRing;

[TestClass]
public class ProgressRingApiTests
{
    [TestMethod]
    public void VerifyDefaultStyleAndWinUI3Resources()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var progressRing = new ModernWpf.Controls.ProgressRing();

            using var host = new TestWindowHost(progressRing, width: 240, height: 180);
            host.UpdateLayout();

            AssertBrushEquals((Brush)progressRing.TryFindResource("ProgressRingForegroundThemeBrush"), progressRing.Foreground);
            AssertBrushEquals((Brush)progressRing.TryFindResource("ProgressRingBackgroundThemeBrush"), progressRing.Background);
            Assert.IsFalse(progressRing.IsHitTestVisible);
            Assert.AreEqual(HorizontalAlignment.Center, progressRing.HorizontalAlignment);
            Assert.AreEqual(VerticalAlignment.Center, progressRing.VerticalAlignment);
            Assert.AreEqual(16.0, progressRing.MinHeight);
            Assert.AreEqual(16.0, progressRing.MinWidth);
            Assert.IsFalse(progressRing.IsTabStop);
            Assert.AreEqual(32.0, progressRing.Width);
            Assert.AreEqual(32.0, progressRing.Height);
            Assert.AreEqual(100.0, progressRing.Maximum);

            var layoutRoot = FindNamedDescendant<Grid>(progressRing, "LayoutRoot");
            var lottiePlayer = FindNamedDescendant<Grid>(progressRing, "LottiePlayer");
            AssertBrushEquals(Brushes.Transparent, layoutRoot.Background);
            Assert.AreEqual(FlowDirection.LeftToRight, lottiePlayer.FlowDirection);
            Assert.AreEqual(progressRing.TemplateSettings.MaxSideLength, lottiePlayer.MaxWidth);
            Assert.AreEqual(progressRing.TemplateSettings.MaxSideLength, lottiePlayer.MaxHeight);
            Assert.AreEqual(Visibility.Visible, layoutRoot.Visibility);
            Assert.AreEqual(1.0, layoutRoot.Opacity);
            Assert.IsNull(TryFindNamedDescendant<FrameworkElement>(progressRing, "Ring"));
            AssertTemplateEllipses(progressRing);

            foreach (var themeName in new[] { "Light", "Dark" })
            {
                AssertThemeResourceReference(themeName, "ProgressRingForegroundThemeBrush", "AccentFillColorDefaultBrush");
                AssertThemeResourceReference(themeName, "ProgressRingBackgroundThemeBrush", "ControlFillColorTransparentBrush");
                AssertThemeResourceValue(themeName, "ProgressRingStrokeThickness", 4.0);
            }

            AssertThemeResourceReference("HighContrast", "ProgressRingForegroundThemeBrush", "SystemControlHighlightAccentBrush");
            AssertThemeResourceReference("HighContrast", "ProgressRingBackgroundThemeBrush", "SystemControlBackgroundBaseLowBrush");
            AssertThemeResourceValue("HighContrast", "ProgressRingStrokeThickness", 4.0);
        });
    }

    [TestMethod]
    public void VerifyDefaults()
    {
        WpfTestHost.Run(() =>
        {
            var progressRing = new ModernWpf.Controls.ProgressRing();

            Assert.IsTrue(progressRing.IsActive);
            Assert.IsTrue(progressRing.IsIndeterminate);
            Assert.AreEqual(0.0, progressRing.Minimum);
            Assert.AreEqual(100.0, progressRing.Maximum);
            Assert.AreEqual(0.0, progressRing.Value);
        });
    }

    [TestMethod]
    public void VerifyAccessibilityView()
    {
        WpfTestHost.Run(() =>
        {
            var progressRing = new ModernWpf.Controls.ProgressRing
            {
                IsActive = true
            };

            var peer = FrameworkElementAutomationPeer.CreatePeerForElement(progressRing);
            Assert.IsTrue(peer.IsControlElement());

            progressRing.IsActive = false;
            Assert.IsFalse(peer.IsControlElement());
        });
    }

    [TestMethod]
    public void VerifySourceAutomationNameShape()
    {
        WpfTestHost.Run(() =>
        {
            var progressRing = new ModernWpf.Controls.ProgressRing
            {
                IsActive = true,
                IsIndeterminate = true
            };
            AutomationProperties.SetName(progressRing, "Loading");

            var peer = FrameworkElementAutomationPeer.CreatePeerForElement(progressRing);
            Assert.AreEqual("Busy Loading", peer.GetName());

            progressRing.IsIndeterminate = false;
            Assert.AreEqual("Loading", peer.GetName());
        });
    }

    [TestMethod]
    public void InactiveStateUsesSourceVisualStateSetterAndAutomationPeerFallback()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var progressRing = new ModernWpf.Controls.ProgressRing
            {
                IsActive = true
            };

            using var host = new TestWindowHost(progressRing, width: 240, height: 180);
            host.UpdateLayout();

            var layoutRoot = FindNamedDescendant<Grid>(progressRing, "LayoutRoot");
            AssertStateSetter(layoutRoot, "CommonStates", "Inactive", "LayoutRoot.Opacity");

            var peer = FrameworkElementAutomationPeer.CreatePeerForElement(progressRing);
            Assert.IsTrue(peer.IsControlElement());

            progressRing.IsActive = false;
            host.UpdateLayout();

            AssertCurrentState(layoutRoot, "CommonStates", "Inactive");
            Assert.AreEqual(0.0, layoutRoot.Opacity);
            Assert.IsFalse(peer.IsControlElement());
        });
    }

    private static void AssertThemeResourceReference(string themeName, string resourceKey, object expectedResourceKey)
    {
        var themeDictionary = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(themeDictionary.Contains(resourceKey), $"{themeName} is missing {resourceKey}.");
        Assert.IsTrue(themeDictionary.Contains(expectedResourceKey), $"{themeName} is missing {expectedResourceKey}.");
        Assert.AreSame(themeDictionary[expectedResourceKey], themeDictionary[resourceKey], $"{themeName}:{resourceKey}");
    }

    private static void AssertThemeResourceValue<T>(string themeName, string resourceKey, T expectedValue)
    {
        var themeDictionary = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(themeDictionary.Contains(resourceKey), $"{themeName} is missing {resourceKey}.");
        Assert.AreEqual(expectedValue, themeDictionary[resourceKey]);
    }

    private static void AssertBrushEquals(Brush expected, Brush actual)
    {
        Assert.IsNotNull(expected);
        Assert.IsNotNull(actual);

        if (expected is SolidColorBrush expectedSolid && actual is SolidColorBrush actualSolid)
        {
            Assert.AreEqual(expectedSolid.Color, actualSolid.Color);
            Assert.AreEqual(expectedSolid.Opacity, actualSolid.Opacity);
            return;
        }

        Assert.AreEqual(expected.ToString(), actual.ToString());
    }

    private static void AssertStateSetter(FrameworkElement stateGroupsRoot, string groupName, string stateName, params string[] expectedTargets)
    {
        var group = FindVisualStateGroup(stateGroupsRoot, groupName);
        var state = group.States.OfType<VisualStateEx>().Single(item => item.Name == stateName);
        var actualTargets = state.Setters
            .Select(setter => string.IsNullOrEmpty(setter.Target) ? setter.Property : setter.Target)
            .ToArray();

        CollectionAssert.IsSubsetOf(expectedTargets, actualTargets);
    }

    private static void AssertCurrentState(FrameworkElement stateGroupsRoot, string groupName, string expectedStateName)
    {
        Assert.AreEqual(expectedStateName, FindVisualStateGroup(stateGroupsRoot, groupName).CurrentState?.Name);
    }

    private static void AssertTemplateEllipses(ModernWpf.Controls.ProgressRing progressRing)
    {
        var ellipses = Enumerable.Range(1, 6)
            .Select(index => FindNamedDescendant<Ellipse>(progressRing, $"E{index}"))
            .ToArray();

        Assert.AreEqual(6, ellipses.Length);
        foreach (var ellipse in ellipses)
        {
            Assert.AreEqual(progressRing.TemplateSettings.EllipseDiameter, ellipse.Width);
            Assert.AreEqual(progressRing.TemplateSettings.EllipseDiameter, ellipse.Height);
            Assert.AreEqual(progressRing.TemplateSettings.EllipseOffset, ellipse.Margin);
            AssertBrushEquals(progressRing.Foreground, ellipse.Fill);

            var canvas = VisualTreeHelper.GetParent(ellipse) as Canvas;
            Assert.IsNotNull(canvas, ellipse.Name);
            Assert.AreEqual(new Point(0.5, 0.5), canvas!.RenderTransformOrigin);
            Assert.IsInstanceOfType(canvas.RenderTransform, typeof(RotateTransform), ellipse.Name);
        }
    }

    private static VisualStateGroup FindVisualStateGroup(FrameworkElement stateGroupsRoot, string groupName)
    {
        return VisualStateManager.GetVisualStateGroups(stateGroupsRoot)
            .OfType<VisualStateGroup>()
            .Single(item => item.Name == groupName);
    }

    private static T FindNamedDescendant<T>(DependencyObject root, string name)
        where T : FrameworkElement
    {
        var descendant = TryFindNamedDescendant<T>(root, name);
        if (descendant != null)
        {
            return descendant;
        }

        throw new System.InvalidOperationException($"Could not find descendant named '{name}'.");
    }

    private static T? TryFindNamedDescendant<T>(DependencyObject root, string name)
        where T : FrameworkElement
    {
        foreach (var descendant in VisualTreeTestHelper.EnumerateDescendants(root))
        {
            if (descendant is T element && element.Name == name)
            {
                return element;
            }
        }

        return null;
    }
}
