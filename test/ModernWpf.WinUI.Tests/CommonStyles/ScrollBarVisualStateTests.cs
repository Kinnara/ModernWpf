using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf;
using ModernWpf.Controls.Primitives;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.CommonStyles;

[TestClass]
public class ScrollBarVisualStateTests
{
    [TestMethod]
    public void DefaultScrollBarStyleUsesOfficialWpfFluentSetterAndTriggerShape()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var defaultStyle = (Style)Application.Current.FindResource("DefaultScrollBarStyle");
            var implicitStyle = (Style)Application.Current.FindResource(typeof(ScrollBar));
            Assert.AreEqual(typeof(ScrollBar), defaultStyle.TargetType);
            Assert.AreEqual(typeof(ScrollBar), implicitStyle.TargetType);
            Assert.AreSame(defaultStyle, implicitStyle.BasedOn);

            var setters = defaultStyle.Setters.OfType<Setter>().ToArray();
            AssertDynamicResourceSetter(setters, Control.BackgroundProperty, "ScrollBarTrackFill");
            AssertDynamicResourceSetter(setters, Control.BorderBrushProperty, "ScrollBarTrackStroke");
            AssertSetter(setters, FrameworkElement.MarginProperty, new Thickness(0));
            AssertSetter(setters, Control.PaddingProperty, new Thickness(0));
            AssertSetter(setters, UIElement.SnapsToDevicePixelsProperty, true);
            AssertSetter(setters, Control.OverridesDefaultStyleProperty, true);
            AssertNoSetter(setters, ScrollBarHelper.AutoHideProperty);
            AssertNoSetter(setters, ScrollBarHelper.IndicatorModeProperty);
            AssertNoSetter(setters, ScrollBarHelper.IsEnabledProperty);

            var orientationTriggers = defaultStyle.Triggers.OfType<Trigger>()
                .Where(item => item.Property == ScrollBar.OrientationProperty)
                .ToArray();
            Assert.AreEqual(2, orientationTriggers.Length);

            AssertOrientationTrigger(
                orientationTriggers.Single(item => Equals(item.Value, Orientation.Vertical)),
                width: 12.0,
                height: double.NaN,
                templateKey: "VerticalScrollBarTemplate");
            AssertOrientationTrigger(
                orientationTriggers.Single(item => Equals(item.Value, Orientation.Horizontal)),
                width: double.NaN,
                height: 12.0,
                templateKey: "HorizontalScrollBarTemplate");
        });
    }

    [TestMethod]
    public void ScrollBarSupportStylesUseOfficialWpfFluentShape()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var lineButtonStyle = (Style)Application.Current.FindResource("ScrollBarLineButtonStyle");
            var lineButtonSetters = lineButtonStyle.Setters.OfType<Setter>().ToArray();
            Assert.AreEqual(typeof(RepeatButton), lineButtonStyle.TargetType);
            AssertDynamicResourceSetter(lineButtonSetters, Control.ForegroundProperty, "ScrollBarButtonArrowForeground");
            AssertSetter(lineButtonSetters, FrameworkElement.WidthProperty, Application.Current.FindResource("LineButtonWidth"));
            AssertSetter(lineButtonSetters, FrameworkElement.HeightProperty, Application.Current.FindResource("LineButtonHeight"));
            AssertSetter(lineButtonSetters, Control.FontSizeProperty, Application.Current.FindResource("ScrollBarButtonArrowIconFontSize"));
            AssertSetter(lineButtonSetters, UIElement.FocusableProperty, false);

            var pageButtonStyle = (Style)Application.Current.FindResource("ScrollBarPageButtonStyle");
            var pageButtonSetters = pageButtonStyle.Setters.OfType<Setter>().ToArray();
            Assert.AreEqual(typeof(RepeatButton), pageButtonStyle.TargetType);
            AssertSetter(pageButtonSetters, Control.IsTabStopProperty, false);
            AssertSetter(pageButtonSetters, UIElement.FocusableProperty, false);

            var thumbStyle = (Style)Application.Current.FindResource("ScrollBarThumbStyle");
            var thumbSetters = thumbStyle.Setters.OfType<Setter>().ToArray();
            Assert.AreEqual(typeof(Thumb), thumbStyle.TargetType);
            AssertDynamicResourceSetter(thumbSetters, Control.BackgroundProperty, "ScrollBarThumbFill");
            AssertSetter(thumbSetters, ControlHelper.CornerRadiusProperty, new CornerRadius(4));
            AssertNoSetter(thumbSetters, Border.CornerRadiusProperty);
            AssertSetter(thumbSetters, Control.IsTabStopProperty, false);
            AssertSetter(thumbSetters, UIElement.FocusableProperty, false);
        });
    }

    [TestMethod]
    public void VerticalScrollBarAppliesOfficialWpfFluentTemplateParts()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var scrollBar = CreateScrollBar(Orientation.Vertical);
            using var host = new TestWindowHost(scrollBar, width: 80, height: 180);
            host.UpdateLayout();

            Assert.AreEqual(12.0, scrollBar.Width);
            Assert.AreEqual(double.NaN, scrollBar.Height);
            Assert.AreSame(scrollBar.TryFindResource("ScrollBarTrackFill"), scrollBar.Background);
            Assert.AreSame(scrollBar.TryFindResource("ScrollBarTrackStroke"), scrollBar.BorderBrush);

            var border = FindTemplatePart<Border>(scrollBar, "PART_Border");
            Assert.AreEqual(12.0, border.Width);
            Assert.AreEqual(new CornerRadius(6), border.CornerRadius);

            Assert.IsNotNull(FindTemplatePart<RepeatButton>(scrollBar, "PART_ButtonScrollUp"));
            Assert.IsNotNull(FindTemplatePart<RepeatButton>(scrollBar, "PART_ButtonScrollDown"));

            var track = FindTemplatePart<Track>(scrollBar, "PART_Track");
            Assert.AreEqual(4.0, track.Width);
            Assert.IsTrue(track.IsDirectionReversed);
            Assert.AreSame((ControlTemplate)Application.Current.FindResource("VerticalScrollBarTemplate"), scrollBar.Template);
        });
    }

    [TestMethod]
    public void HorizontalScrollBarAppliesOfficialWpfFluentTemplateParts()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var scrollBar = CreateScrollBar(Orientation.Horizontal);
            using var host = new TestWindowHost(scrollBar, width: 180, height: 80);
            host.UpdateLayout();

            Assert.AreEqual(double.NaN, scrollBar.Width);
            Assert.AreEqual(12.0, scrollBar.Height);

            var border = FindTemplatePart<Border>(scrollBar, "PART_Border");
            Assert.AreEqual(12.0, border.Height);
            Assert.AreEqual(new CornerRadius(6), border.CornerRadius);

            Assert.IsNotNull(FindTemplatePart<RepeatButton>(scrollBar, "PART_ButtonScrollLeft"));
            Assert.IsNotNull(FindTemplatePart<RepeatButton>(scrollBar, "PART_ButtonScrollRight"));

            var track = FindTemplatePart<Track>(scrollBar, "PART_Track");
            Assert.AreEqual(4.0, track.Height);
            Assert.IsFalse(track.IsDirectionReversed);
            Assert.AreSame((ControlTemplate)Application.Current.FindResource("HorizontalScrollBarTemplate"), scrollBar.Template);
        });
    }

    [TestMethod]
    public void ScrollBarDeletesModernWpfAutoHideTemplateGuesses()
    {
        var repoRoot = FindRepoRoot();
        var text = System.IO.File.ReadAllText(System.IO.Path.Combine(repoRoot, "ModernWpf", "Styles", "ScrollBar.xaml"));

        Assert.IsFalse(text.Contains("VisualStateEx", System.StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("VisualStateManagerEx", System.StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("ScrollBarHelper", System.StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("AutoHideScrollBars", System.StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("PanningThumb", System.StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("RepeatButtonTransparent", System.StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("CornerRadiusFilterConverter", System.StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("FontIconFallback", System.StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("Border.CornerRadius", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ThemeDictionariesExposeOfficialScrollBarAliases()
    {
        WpfTestHost.Run(() =>
        {
            foreach (var themeName in new[] { "Light", "Dark" })
            {
                AssertThemeResourceReference(themeName, "ScrollBarButtonBackground", "SubtleFillColorTransparentBrush");
                AssertThemeResourceReference(themeName, "ScrollBarButtonBackgroundPointerOver", "SubtleFillColorSecondaryBrush");
                AssertThemeResourceReference(themeName, "ScrollBarThumbFill", "ControlStrongFillColorDefaultBrush");
                AssertThemeResourceReference(themeName, "ScrollBarTrackFill", "AcrylicInAppFillColorDefaultBrush");
                AssertThemeResourceReference(themeName, "ScrollBarTrackStroke", "AcrylicInAppFillColorDefaultBrush");
            }

            AssertThemeResourceReference("HighContrast", "ScrollBarButtonBackground", "SystemControlTransparentBrush");
            AssertThemeResourceReference("HighContrast", "ScrollBarThumbFill", "SystemControlForegroundChromeDisabledLowBrush");
            AssertThemeResourceReference("HighContrast", "ScrollBarTrackFill", "SystemControlPageBackgroundChromeLowBrush");
            AssertThemeResourceReference("HighContrast", "ScrollBarTrackStroke", "SystemControlForegroundTransparentBrush");
        });
    }

    private static ScrollBar CreateScrollBar(Orientation orientation)
    {
        return new ScrollBar
        {
            Orientation = orientation,
            Minimum = 0,
            Maximum = 100,
            ViewportSize = 20
        };
    }

    private static void AssertOrientationTrigger(Trigger trigger, object width, object height, object templateKey)
    {
        var setters = trigger.Setters.OfType<Setter>().ToArray();
        AssertSetter(setters, FrameworkElement.WidthProperty, width);
        AssertSetter(setters, FrameworkElement.HeightProperty, height);

        var templateSetter = setters.Single(item => item.Property == Control.TemplateProperty);
        Assert.AreSame(Application.Current.FindResource(templateKey), templateSetter.Value);
    }

    private static void AssertSetter(Setter[] setters, DependencyProperty property, object value)
    {
        var setter = setters.Single(item => item.Property == property);
        Assert.AreEqual(value, setter.Value);
    }

    private static void AssertNoSetter(Setter[] setters, DependencyProperty property)
    {
        Assert.IsFalse(setters.Any(item => item.Property == property), $"Unexpected setter for {property.Name}.");
    }

    private static void AssertDynamicResourceSetter(Setter[] setters, DependencyProperty property, object resourceKey)
    {
        var setter = setters.Single(item => item.Property == property);
        Assert.IsInstanceOfType(setter.Value, typeof(DynamicResourceExtension));
        var dynamicResource = (DynamicResourceExtension)setter.Value;
        Assert.AreEqual(resourceKey, dynamicResource.ResourceKey);
    }

    private static T FindTemplatePart<T>(ScrollBar scrollBar, string name)
        where T : DependencyObject
    {
        return scrollBar.Template.FindName(name, scrollBar) as T
            ?? throw new AssertFailedException($"Expected ScrollBar template part '{name}' to be a {typeof(T).Name}.");
    }

    private static void AssertThemeResourceReference(string themeName, string key, object expectedResourceKey)
    {
        var theme = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(theme.Contains(expectedResourceKey), $"Theme is missing {expectedResourceKey}.");
        Assert.AreSame(theme[expectedResourceKey], theme[key], key);
    }

    private static string FindRepoRoot()
    {
        var directory = new System.IO.DirectoryInfo(System.AppContext.BaseDirectory);

        while (directory != null)
        {
            if (System.IO.File.Exists(System.IO.Path.Combine(directory.FullName, "ModernWpf.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        Assert.Fail("Could not locate ModernWpf.sln from the test output directory.");
        return string.Empty;
    }
}
