using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;
using ProgressBar = ModernWpf.Controls.ProgressBar;

namespace ModernWpf.WinUI.Tests.ProgressBars;

[TestClass]
public class ProgressBarApiTests
{
    [TestMethod]
    public void VerifyDefaultStyleAndWinUI2Resources()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var resources = new ResourceDictionary
            {
                Source = new Uri("/ModernWpf;component/ProgressBar/ProgressBar.xaml", UriKind.Relative)
            };
            AssertResource(resources, "ProgressBarMinHeight", 3.0);
            AssertResource(resources, "ProgressBarTrackHeight", 1.0);
            AssertResource(resources, "ProgressBarCornerRadius", new CornerRadius(1.5));
            AssertResource(resources, "ProgressBarTrackCornerRadius", new CornerRadius(0.5));

            var style = (Style)resources[typeof(ProgressBar)];
            AssertDynamicResourceSetter(style, Control.ForegroundProperty, "ProgressBarForeground");
            AssertDynamicResourceSetter(style, Control.BackgroundProperty, "ProgressBarBackground");
            AssertDynamicResourceSetter(style, Control.BorderThicknessProperty, "ProgressBarBorderThemeThickness");
            AssertDynamicResourceSetter(style, Control.BorderBrushProperty, "ProgressBarBorderBrush");
            AssertSetterValue(style, FrameworkElement.MinHeightProperty, 3.0);
            AssertSetterValue(style, RangeBase.MaximumProperty, 100.0);
            AssertSetterValue(style, Control.IsTabStopProperty, false);
            AssertSetterValue(style, FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
            AssertSetterValue(style, ProgressBar.CornerRadiusProperty, new CornerRadius(1.5));
            Assert.IsInstanceOfType(FindSetter(style, Control.TemplateProperty)?.Value, typeof(ControlTemplate));

            var progressBar = new ProgressBar
            {
                Style = style,
                Width = 120,
                Value = 60
            };

            using var host = new TestWindowHost(progressBar, width: 320, height: 180);
            host.UpdateLayout();

            AssertBrushEquals((Brush)progressBar.TryFindResource("ProgressBarForeground"), progressBar.Foreground);
            AssertBrushEquals((Brush)progressBar.TryFindResource("ProgressBarBackground"), progressBar.Background);
            Assert.AreSame(progressBar.TryFindResource("ProgressBarBorderBrush"), progressBar.BorderBrush);
            Assert.AreEqual(progressBar.TryFindResource("ProgressBarBorderThemeThickness"), progressBar.BorderThickness);
            Assert.AreSame(style, progressBar.Style);
            Assert.AreEqual(3.0, progressBar.MinHeight);
            Assert.AreEqual(new CornerRadius(1.5), progressBar.CornerRadius);
            Assert.AreEqual(100.0, progressBar.Maximum);
            Assert.IsFalse(progressBar.IsTabStop);
            Assert.AreEqual(VerticalAlignment.Center, progressBar.VerticalAlignment);

            var layoutRoot = FindNamedDescendant<Grid>(progressBar, "LayoutRoot");
            Assert.IsTrue(layoutRoot.SnapsToDevicePixels);

            var rootBorder = FindNamedDescendant<Border>(progressBar, "ProgressBarRoot");
            Assert.IsNull(rootBorder.Background);
            Assert.AreSame(progressBar.BorderBrush, rootBorder.BorderBrush);
            Assert.AreEqual(progressBar.BorderThickness, rootBorder.BorderThickness);
            Assert.AreEqual(progressBar.CornerRadius, rootBorder.CornerRadius);
            Assert.AreEqual(progressBar.Padding, rootBorder.Padding);

            var track = FindNamedDescendant<Rectangle>(progressBar, "ProgressBarTrack");
            AssertBrushEquals(progressBar.Background, track.Fill);
            Assert.AreEqual(1.0, track.Height);
            Assert.AreEqual(VerticalAlignment.Center, track.VerticalAlignment);
            Assert.AreEqual(0.5, track.RadiusX);
            Assert.AreEqual(0.5, track.RadiusY);
            Assert.AreEqual(progressBar.Width, track.Width);
            Assert.IsInstanceOfType(track.RenderTransform, typeof(TranslateTransform));

            AssertProgressIndicator(progressBar, "DeterminateProgressBarIndicator");
            AssertProgressIndicator(progressBar, "IndeterminateProgressBarIndicator");
            AssertProgressIndicator(progressBar, "IndeterminateProgressBarIndicator2");

            foreach (var themeName in new[] { "Light", "Dark" })
            {
                AssertThemeResourceReference(themeName, "ProgressBarForeground", "AccentFillColorDefaultBrush");
                AssertThemeResourceReference(themeName, "ProgressBarBackground", "ControlStrongStrokeColorDefaultBrush");
                AssertThemeResourceReference(themeName, "ProgressBarBorderBrush", "ControlStrokeColorDefaultBrush");
                AssertThemeResourceReference(themeName, "ProgressBarPausedForegroundColor", "SystemFillColorCaution");
                AssertThemeResourceReference(themeName, "ProgressBarErrorForegroundColor", "SystemFillColorCritical");
                AssertThemeResourceValue(themeName, "ProgressBarIndicatorPauseOpacity", 0.6);
                AssertThemeResourceValue(themeName, "ProgressBarThemeMinHeight", 4.0);
                AssertThemeResourceValue(themeName, "ProgressBarBorderThemeThickness", new Thickness(0));
            }

            AssertThemeResourceReference("HighContrast", "ProgressBarForeground", "SystemColorHighlightColorBrush");
            AssertThemeResourceReference("HighContrast", "ProgressBarBackground", "SystemColorWindowColorBrush");
            AssertThemeResourceReference("HighContrast", "ProgressBarBorderBrush", "SystemColorWindowTextColorBrush");
            AssertThemeResourceValue("HighContrast", "ProgressBarPausedForegroundColor", SystemColors.WindowTextColor);
            AssertThemeResourceValue("HighContrast", "ProgressBarErrorForegroundColor", SystemColors.HotTrackColor);
            AssertThemeResourceValue("HighContrast", "ProgressBarIndicatorPauseOpacity", 1.0);
            AssertThemeResourceValue("HighContrast", "ProgressBarThemeMinHeight", 4.0);
            AssertThemeResourceValue("HighContrast", "ProgressBarBorderThemeThickness", new Thickness(1));
        });
    }

    [TestMethod]
    public void ResourceOverridability()
    {
        WpfTestHost.Run(() =>
        {
            var grid = new Grid();
            grid.Resources["ProgressBarTrackHeight"] = 3.0;

            var overriddenProgressBar = new ProgressBar();
            grid.Children.Add(overriddenProgressBar);

            var defaultProgressBar = new ProgressBar();
            var root = new StackPanel();
            root.Children.Add(grid);
            root.Children.Add(defaultProgressBar);

            using var host = new TestWindowHost(root);

            var overriddenTrack = FindNamedDescendant<Rectangle>(overriddenProgressBar, "ProgressBarTrack");
            Assert.AreEqual(3.0, overriddenTrack.Height);

            var defaultTrack = FindNamedDescendant<Rectangle>(defaultProgressBar, "ProgressBarTrack");
            Assert.AreEqual(1.0, defaultTrack.Height);
        });
    }

    private static void AssertProgressIndicator(ProgressBar progressBar, string name)
    {
        var indicator = FindNamedDescendant<Rectangle>(progressBar, name);
        AssertBrushEquals(progressBar.Foreground, indicator.Fill);
        Assert.AreEqual(new Thickness(0), indicator.Margin);
        Assert.AreEqual(HorizontalAlignment.Left, indicator.HorizontalAlignment);
        Assert.AreEqual(1.5, indicator.RadiusX);
        Assert.AreEqual(1.5, indicator.RadiusY);
        Assert.IsInstanceOfType(indicator.RenderTransform, typeof(TranslateTransform));
    }

    private static void AssertResource(ResourceDictionary resources, string key, object expected)
    {
        Assert.IsTrue(resources.Contains(key), $"Expected resource '{key}' to exist.");
        Assert.AreEqual(expected, resources[key], $"Unexpected value for '{key}'.");
    }

    private static void AssertDynamicResourceSetter(Style style, DependencyProperty property, object expectedResourceKey)
    {
        var setter = FindSetter(style, property);
        Assert.IsNotNull(setter, $"Expected local setter for {property.Name}.");
        Assert.IsInstanceOfType(setter!.Value, typeof(DynamicResourceExtension));

        var dynamicResource = (DynamicResourceExtension)setter.Value;
        Assert.AreEqual(expectedResourceKey, dynamicResource.ResourceKey);
    }

    private static void AssertSetterValue(Style style, DependencyProperty property, object expectedValue)
    {
        var setter = FindSetter(style, property);
        Assert.IsNotNull(setter, $"Expected local setter for {property.Name}.");
        Assert.AreEqual(expectedValue, setter!.Value);
    }

    private static Setter? FindSetter(Style style, DependencyProperty property)
    {
        for (var current = style; current != null; current = current.BasedOn)
        {
            var setter = current.Setters
                .OfType<Setter>()
                .FirstOrDefault(item => item.Property == property);

            if (setter != null)
            {
                return setter;
            }
        }

        return null;
    }

    private static void AssertThemeResourceReference(string themeName, string resourceKey, object expectedResourceKey)
    {
        var themeDictionary = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(themeDictionary.Contains(resourceKey), $"{themeName} is missing {resourceKey}.");
        Assert.IsTrue(themeDictionary.Contains(expectedResourceKey), $"{themeName} is missing {expectedResourceKey}.");

        var actual = themeDictionary[resourceKey];
        var expected = themeDictionary[expectedResourceKey];

        if (expected is null || actual is null || expected.GetType().IsValueType || actual.GetType().IsValueType)
        {
            Assert.AreEqual(expected, actual, $"{themeName}:{resourceKey}");
        }
        else
        {
            Assert.AreSame(expected, actual, $"{themeName}:{resourceKey}");
        }
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

    private static void AssertThemeResourceValue<T>(string themeName, string resourceKey, T expectedValue)
    {
        var themeDictionary = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(themeDictionary.Contains(resourceKey), $"{themeName} is missing {resourceKey}.");
        Assert.AreEqual(expectedValue, themeDictionary[resourceKey]);
    }

    private static T FindNamedDescendant<T>(DependencyObject root, string name)
        where T : FrameworkElement
    {
        foreach (var descendant in VisualTreeTestHelper.EnumerateDescendants(root))
        {
            if (descendant is T element && element.Name == name)
            {
                return element;
            }
        }

        throw new InvalidOperationException($"Could not find descendant named '{name}'.");
    }
}
