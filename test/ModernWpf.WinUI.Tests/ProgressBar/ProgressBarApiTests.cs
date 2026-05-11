using System;
using System.Windows;
using System.Windows.Controls;
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

            var progressBar = new ProgressBar
            {
                Width = 120,
                Value = 60
            };

            using var host = new TestWindowHost(progressBar, width: 320, height: 180);
            host.UpdateLayout();

            AssertBrushEquals((Brush)progressBar.TryFindResource("ProgressBarForeground"), progressBar.Foreground);
            AssertBrushEquals((Brush)progressBar.TryFindResource("ProgressBarBackground"), progressBar.Background);
            Assert.AreSame(progressBar.TryFindResource("ProgressBarBorderBrush"), progressBar.BorderBrush);
            Assert.AreEqual(progressBar.TryFindResource("ProgressBarBorderThemeThickness"), progressBar.BorderThickness);
            Assert.AreEqual(3.0, progressBar.MinHeight);
            Assert.AreEqual(new CornerRadius(1.5), progressBar.CornerRadius);
            Assert.AreEqual(100.0, progressBar.Maximum);
            Assert.IsFalse(progressBar.IsTabStop);
            Assert.AreEqual(VerticalAlignment.Center, progressBar.VerticalAlignment);

            var rootBorder = FindNamedDescendant<Border>(progressBar, "ProgressBarRoot");
            Assert.IsNull(rootBorder.Background);
            Assert.AreSame(progressBar.BorderBrush, rootBorder.BorderBrush);
            Assert.AreEqual(progressBar.BorderThickness, rootBorder.BorderThickness);
            Assert.AreEqual(progressBar.CornerRadius, rootBorder.CornerRadius);

            var track = FindNamedDescendant<Rectangle>(progressBar, "ProgressBarTrack");
            AssertBrushEquals(progressBar.Background, track.Fill);
            Assert.AreEqual(1.0, track.Height);
            Assert.AreEqual(VerticalAlignment.Center, track.VerticalAlignment);
            Assert.AreEqual(0.5, track.RadiusX);
            Assert.AreEqual(0.5, track.RadiusY);

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
                AssertThemeResourceValue(themeName, "ProgressBarBorderThemeThickness", new Thickness(0));
            }

            AssertThemeResourceReference("HighContrast", "ProgressBarForeground", "SystemColorHighlightColorBrush");
            AssertThemeResourceReference("HighContrast", "ProgressBarBackground", "SystemColorWindowColorBrush");
            AssertThemeResourceReference("HighContrast", "ProgressBarBorderBrush", "SystemColorWindowTextColorBrush");
            AssertThemeResourceValue("HighContrast", "ProgressBarPausedForegroundColor", SystemColors.WindowTextColor);
            AssertThemeResourceValue("HighContrast", "ProgressBarErrorForegroundColor", SystemColors.HotTrackColor);
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
