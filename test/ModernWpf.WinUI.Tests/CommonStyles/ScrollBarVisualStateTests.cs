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
            AssertSetter(thumbSetters, System.Windows.Controls.Border.CornerRadiusProperty, new CornerRadius(4));
            AssertSetter(thumbSetters, Control.IsTabStopProperty, false);
            AssertSetter(thumbSetters, UIElement.FocusableProperty, false);
        });
    }

    [TestMethod]
    public void ScrollBarStylesUseWinUIResourceAliases()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var defaultStyle = (Style)Application.Current.FindResource("DefaultScrollBarStyle");
            var lineButtonStyle = (Style)Application.Current.FindResource("ScrollBarLineButtonStyle");
            var pageButtonStyle = (Style)Application.Current.FindResource("ScrollBarPageButtonStyle");
            var thumbStyle = (Style)Application.Current.FindResource("ScrollBarThumbStyle");
            var verticalTemplate = (ControlTemplate)Application.Current.FindResource("VerticalScrollBarTemplate");
            var horizontalTemplate = (ControlTemplate)Application.Current.FindResource("HorizontalScrollBarTemplate");

            AssertDynamicResourceSetter(defaultStyle.Setters.OfType<Setter>().ToArray(), Control.BackgroundProperty, "ScrollBarTrackFill");
            AssertDynamicResourceSetter(defaultStyle.Setters.OfType<Setter>().ToArray(), Control.BorderBrushProperty, "ScrollBarTrackStroke");
            AssertDynamicResourceSetter(lineButtonStyle.Setters.OfType<Setter>().ToArray(), Control.ForegroundProperty, "ScrollBarButtonArrowForeground");
            AssertDynamicResourceSetter(thumbStyle.Setters.OfType<Setter>().ToArray(), Control.BackgroundProperty, "ScrollBarThumbFill");
            AssertMouseOverTrackFillSetter(verticalTemplate, "ScrollBarTrackFillPointerOver");
            AssertMouseOverTrackFillSetter(horizontalTemplate, "ScrollBarTrackFillPointerOver");

            var verticalScrollBar = CreateScrollBar(Orientation.Vertical);
            using (var host = new TestWindowHost(verticalScrollBar, width: 80, height: 180))
            {
                host.UpdateLayout();
                AssertScrollBarLiveResources(verticalScrollBar, verticalTemplate, lineButtonStyle, pageButtonStyle, thumbStyle);
                AssertLineButtonLiveResources(
                    FindTemplatePart<RepeatButton>(verticalScrollBar, "PART_ButtonScrollUp"),
                    verticalScrollBar.TryFindResource("ScrollBarCaretUpGlyph"));
                AssertLineButtonLiveResources(
                    FindTemplatePart<RepeatButton>(verticalScrollBar, "PART_ButtonScrollDown"),
                    verticalScrollBar.TryFindResource("ScrollBarCaretDownGlyph"));
            }

            var horizontalScrollBar = CreateScrollBar(Orientation.Horizontal);
            using (var host = new TestWindowHost(horizontalScrollBar, width: 180, height: 80))
            {
                host.UpdateLayout();
                AssertScrollBarLiveResources(horizontalScrollBar, horizontalTemplate, lineButtonStyle, pageButtonStyle, thumbStyle);
                AssertLineButtonLiveResources(
                    FindTemplatePart<RepeatButton>(horizontalScrollBar, "PART_ButtonScrollLeft"),
                    horizontalScrollBar.TryFindResource("ScrollBarCaretLeftGlyph"));
                AssertLineButtonLiveResources(
                    FindTemplatePart<RepeatButton>(horizontalScrollBar, "PART_ButtonScrollRight"),
                    horizontalScrollBar.TryFindResource("ScrollBarCaretRightGlyph"));
            }
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
        Assert.IsFalse(text.Contains("ControlHelper.CornerRadius", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ThemeDictionariesExposeOfficialScrollBarAliases()
    {
        WpfTestHost.Run(() =>
        {
            foreach (var themeName in new[] { "Light", "Dark" })
            {
                AssertThemeResourceValue(themeName, "ScrollBarTrackBorderThemeThickness", 0.0);
                AssertThemeResourceReference(themeName, "ScrollBarBackground", "SubtleFillColorTransparentBrush");
                AssertThemeResourceReference(themeName, "ScrollBarBackgroundPointerOver", "SubtleFillColorTransparentBrush");
                AssertThemeResourceReference(themeName, "ScrollBarBackgroundDisabled", "SubtleFillColorTransparentBrush");
                AssertThemeResourceReference(themeName, "ScrollBarForeground", "SubtleFillColorTransparentBrush");
                AssertThemeResourceReference(themeName, "ScrollBarBorderBrush", "SubtleFillColorTransparentBrush");
                AssertThemeResourceReference(themeName, "ScrollBarBorderBrushPointerOver", "SubtleFillColorTransparentBrush");
                AssertThemeResourceReference(themeName, "ScrollBarBorderBrushDisabled", "SubtleFillColorTransparentBrush");
                AssertThemeResourceReference(themeName, "ScrollBarButtonBackground", "SubtleFillColorTransparentBrush");
                AssertThemeResourceReference(themeName, "ScrollBarButtonBackgroundPointerOver", "SubtleFillColorSecondaryBrush");
                AssertThemeResourceReference(themeName, "ScrollBarButtonBackgroundPressed", "ControlStrongFillColorDefaultBrush");
                AssertThemeResourceReference(themeName, "ScrollBarButtonBackgroundDisabled", "SubtleFillColorTransparentBrush");
                AssertThemeResourceReference(themeName, "ScrollBarButtonBorderBrush", "SubtleFillColorTransparentBrush");
                AssertThemeResourceReference(themeName, "ScrollBarButtonBorderBrushPointerOver", "SubtleFillColorTransparentBrush");
                AssertThemeResourceReference(themeName, "ScrollBarButtonBorderBrushPressed", "SubtleFillColorTransparentBrush");
                AssertThemeResourceReference(themeName, "ScrollBarButtonBorderBrushDisabled", "SubtleFillColorTransparentBrush");
                AssertThemeResourceReference(themeName, "ScrollBarButtonArrowForeground", "ControlStrongFillColorDefaultBrush");
                AssertThemeResourceReference(themeName, "ScrollBarButtonArrowForegroundPointerOver", "TextFillColorSecondaryBrush");
                AssertThemeResourceReference(themeName, "ScrollBarButtonArrowForegroundPressed", "TextFillColorSecondaryBrush");
                AssertThemeResourceReference(themeName, "ScrollBarButtonArrowForegroundDisabled", "ControlStrongFillColorDisabledBrush");
                AssertThemeResourceReference(themeName, "ScrollBarThumbFill", "ControlStrongFillColorDefaultBrush");
                AssertThemeResourceReference(themeName, "ScrollBarThumbFillPointerOver", "ControlStrongFillColorDefaultBrush");
                AssertThemeResourceReference(themeName, "ScrollBarThumbFillPressed", "ControlStrongFillColorDefaultBrush");
                AssertThemeResourceReference(themeName, "ScrollBarThumbFillDisabled", "ControlStrongFillColorDisabledBrush");
                AssertThemeResourceReference(themeName, "ScrollBarThumbBorderBrush", "ControlFillColorTransparentBrush");
                AssertThemeResourceReference(themeName, "ScrollBarTrackFill", "AcrylicInAppFillColorDefaultBrush");
                AssertThemeResourceReference(themeName, "ScrollBarTrackFillPointerOver", "AcrylicInAppFillColorDefaultBrush");
                AssertThemeResourceReference(themeName, "ScrollBarTrackFillDisabled", "AcrylicInAppFillColorDefaultBrush");
                AssertThemeResourceReference(themeName, "ScrollBarTrackStroke", "AcrylicInAppFillColorDefaultBrush");
                AssertThemeResourceReference(themeName, "ScrollBarTrackStrokePointerOver", "AcrylicInAppFillColorDefaultBrush");
                AssertThemeResourceReference(themeName, "ScrollBarTrackStrokeDisabled", "AcrylicInAppFillColorDefaultBrush");
                AssertThemeResourceReference(themeName, "ScrollBarThumbBackground", "ControlStrongFillColorDefaultBrush");
                AssertThemeResourceReference(themeName, "ScrollBarPanningThumbBackground", "ControlStrongFillColorDefaultBrush");
                AssertThemeResourceReference(themeName, "ScrollBarPanningThumbBackgroundDisabled", "ControlStrongFillColorDisabledBrush");
                AssertThemeResourceReference(themeName, "ScrollBarThumbBackgroundColor", "ControlAAFillColorDefault");
                AssertThemeResourceReference(themeName, "ScrollBarPanningThumbBackgroundColor", "ControlAAFillColorDefault");
                AssertThemeResourceReference(themeName, "ScrollViewerScrollBarSeparatorBackground", "ControlFillColorTransparentBrush");
            }

            AssertThemeResourceValue("HighContrast", "ScrollBarTrackBorderThemeThickness", 1.0);
            AssertThemeResourceReference("HighContrast", "ScrollBarBackground", "SystemControlTransparentBrush");
            AssertThemeResourceReference("HighContrast", "ScrollBarBackgroundPointerOver", "SystemControlTransparentBrush");
            AssertThemeResourceReference("HighContrast", "ScrollBarBackgroundDisabled", "SystemControlTransparentBrush");
            AssertThemeResourceReference("HighContrast", "ScrollBarForeground", "SystemControlTransparentBrush");
            AssertThemeResourceReference("HighContrast", "ScrollBarBorderBrush", "SystemControlTransparentBrush");
            AssertThemeResourceReference("HighContrast", "ScrollBarBorderBrushPointerOver", "SystemControlTransparentBrush");
            AssertThemeResourceReference("HighContrast", "ScrollBarBorderBrushDisabled", "SystemControlTransparentBrush");
            AssertThemeResourceReference("HighContrast", "ScrollBarButtonBackground", "SystemControlTransparentBrush");
            AssertThemeResourceReference("HighContrast", "ScrollBarButtonBackgroundPointerOver", "SystemControlBackgroundListLowBrush");
            AssertThemeResourceReference("HighContrast", "ScrollBarButtonBackgroundPressed", "SystemControlBackgroundBaseMediumBrush");
            AssertThemeResourceReference("HighContrast", "ScrollBarButtonBackgroundDisabled", "SystemControlTransparentBrush");
            AssertThemeResourceReference("HighContrast", "ScrollBarButtonBorderBrush", "SystemControlTransparentBrush");
            AssertThemeResourceReference("HighContrast", "ScrollBarButtonBorderBrushPointerOver", "SystemControlTransparentBrush");
            AssertThemeResourceReference("HighContrast", "ScrollBarButtonBorderBrushPressed", "SystemControlTransparentBrush");
            AssertThemeResourceReference("HighContrast", "ScrollBarButtonBorderBrushDisabled", "SystemControlTransparentBrush");
            AssertThemeResourceReference("HighContrast", "ScrollBarButtonArrowForeground", "SystemControlForegroundBaseHighBrush");
            AssertThemeResourceReference("HighContrast", "ScrollBarButtonArrowForegroundPointerOver", "SystemControlHighlightAltBaseHighBrush");
            AssertThemeResourceReference("HighContrast", "ScrollBarButtonArrowForegroundPressed", "SystemControlHighlightAltAltHighBrush");
            AssertThemeResourceReference("HighContrast", "ScrollBarButtonArrowForegroundDisabled", "SystemControlDisabledBaseHighBrush");
            AssertThemeResourceReference("HighContrast", "ScrollBarThumbFill", "SystemControlForegroundChromeDisabledLowBrush");
            AssertThemeResourceReference("HighContrast", "ScrollBarThumbFillPointerOver", "SystemControlHighlightBaseMediumLowBrush");
            AssertThemeResourceReference("HighContrast", "ScrollBarThumbFillPressed", "SystemControlHighlightBaseMediumBrush");
            AssertThemeResourceReference("HighContrast", "ScrollBarThumbFillDisabled", "SystemControlDisabledBaseHighBrush");
            AssertThemeResourceReference("HighContrast", "ScrollBarThumbBorderBrush", "SystemControlTransparentBrush");
            AssertThemeResourceReference("HighContrast", "ScrollBarTrackFill", "SystemControlPageBackgroundChromeLowBrush");
            AssertThemeResourceReference("HighContrast", "ScrollBarTrackFillPointerOver", "SystemControlPageBackgroundChromeLowBrush");
            AssertThemeResourceReference("HighContrast", "ScrollBarTrackFillDisabled", "SystemControlDisabledTransparentBrush");
            AssertThemeResourceReference("HighContrast", "ScrollBarTrackStroke", "SystemControlForegroundTransparentBrush");
            AssertThemeResourceReference("HighContrast", "ScrollBarTrackStrokePointerOver", "SystemControlForegroundTransparentBrush");
            AssertThemeResourceReference("HighContrast", "ScrollBarTrackStrokeDisabled", "SystemControlDisabledTransparentBrush");
            AssertThemeResourceValue("HighContrast", "ScrollBarThumbBackgroundColor", SystemColors.ControlTextColor);
            AssertThemeResourceValue("HighContrast", "ScrollBarPanningThumbBackgroundColor", SystemColors.ControlTextColor);
            AssertThemeResourceReference("HighContrast", "ScrollBarPanningThumbBackgroundDisabled", "SystemControlDisabledChromeHighBrush");
            AssertThemeResourceReference("HighContrast", "ScrollViewerScrollBarSeparatorBackground", "SystemControlTransparentBrush");
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
        AssertDynamicResourceSetter(setter, resourceKey);
    }

    private static void AssertDynamicResourceSetter(Setter setter, object resourceKey)
    {
        Assert.IsInstanceOfType(setter.Value, typeof(DynamicResourceExtension));
        var dynamicResource = (DynamicResourceExtension)setter.Value;
        Assert.AreEqual(resourceKey, dynamicResource.ResourceKey);
    }

    private static void AssertMouseOverTrackFillSetter(ControlTemplate template, object resourceKey)
    {
        var trigger = template.Triggers.OfType<Trigger>()
            .Single(item => item.Property == UIElement.IsMouseOverProperty && Equals(item.Value, true));
        var setter = trigger.Setters.OfType<Setter>().Single(item => item.Property == Control.BackgroundProperty);
        AssertDynamicResourceSetter(setter, resourceKey);
    }

    private static void AssertScrollBarLiveResources(
        ScrollBar scrollBar,
        ControlTemplate expectedTemplate,
        Style lineButtonStyle,
        Style pageButtonStyle,
        Style thumbStyle)
    {
        Assert.AreSame(expectedTemplate, scrollBar.Template);
        Assert.AreSame(scrollBar.TryFindResource("ScrollBarTrackFill"), scrollBar.Background);
        Assert.AreSame(scrollBar.TryFindResource("ScrollBarTrackStroke"), scrollBar.BorderBrush);

        var track = FindTemplatePart<Track>(scrollBar, "PART_Track");
        Assert.AreSame(pageButtonStyle, track.DecreaseRepeatButton.Style);
        Assert.AreSame(pageButtonStyle, track.IncreaseRepeatButton.Style);
        Assert.IsFalse(track.DecreaseRepeatButton.IsTabStop);
        Assert.IsFalse(track.DecreaseRepeatButton.Focusable);
        Assert.IsFalse(track.IncreaseRepeatButton.IsTabStop);
        Assert.IsFalse(track.IncreaseRepeatButton.Focusable);

        var thumb = track.Thumb;
        Assert.AreSame(thumbStyle, thumb.Style);
        Assert.AreSame(thumb.TryFindResource("ScrollBarThumbFill"), thumb.Background);
        Assert.AreEqual(new CornerRadius(4), thumb.GetValue(System.Windows.Controls.Border.CornerRadiusProperty));
        Assert.IsFalse(thumb.IsTabStop);
        Assert.IsFalse(thumb.Focusable);

        thumb.ApplyTemplate();
        var thumbBorder = FindVisualDescendant<Border>(thumb);
        Assert.AreSame(thumb.Background, thumbBorder.Background);
        Assert.AreSame(thumb.BorderBrush, thumbBorder.BorderBrush);
        Assert.AreEqual(new Thickness(1), thumbBorder.BorderThickness);
        Assert.AreEqual(thumb.GetValue(System.Windows.Controls.Border.CornerRadiusProperty), thumbBorder.CornerRadius);

        foreach (var lineButton in GetLineButtons(scrollBar))
        {
            Assert.AreSame(lineButtonStyle, lineButton.Style);
            Assert.AreSame(lineButton.TryFindResource("ScrollBarButtonArrowForeground"), lineButton.Foreground);
            Assert.AreEqual(lineButton.TryFindResource("LineButtonWidth"), lineButton.Width);
            Assert.AreEqual(lineButton.TryFindResource("LineButtonHeight"), lineButton.Height);
            Assert.AreEqual(lineButton.TryFindResource("ScrollBarButtonArrowIconFontSize"), lineButton.FontSize);
            Assert.IsFalse(lineButton.Focusable);
        }
    }

    private static void AssertLineButtonLiveResources(RepeatButton button, object glyph)
    {
        button.ApplyTemplate();

        var border = FindTemplatePart<Border>(button, "Border");
        var textBlock = FindVisualDescendant<TextBlock>(button);

        Assert.AreSame(button.TryFindResource("ScrollBarButtonBackground"), border.Background);
        Assert.AreSame(button.Foreground, textBlock.Foreground);
        Assert.AreSame(button.TryFindResource("SymbolThemeFontFamily"), textBlock.FontFamily);
        Assert.AreEqual(button.FontSize, textBlock.FontSize);
        Assert.AreEqual(glyph, textBlock.Text);
    }

    private static RepeatButton[] GetLineButtons(ScrollBar scrollBar)
    {
        return scrollBar.Orientation == Orientation.Vertical
            ? new[]
            {
                FindTemplatePart<RepeatButton>(scrollBar, "PART_ButtonScrollUp"),
                FindTemplatePart<RepeatButton>(scrollBar, "PART_ButtonScrollDown")
            }
            : new[]
            {
                FindTemplatePart<RepeatButton>(scrollBar, "PART_ButtonScrollLeft"),
                FindTemplatePart<RepeatButton>(scrollBar, "PART_ButtonScrollRight")
            };
    }

    private static T FindTemplatePart<T>(ScrollBar scrollBar, string name)
        where T : DependencyObject
    {
        return scrollBar.Template.FindName(name, scrollBar) as T
            ?? throw new AssertFailedException($"Expected ScrollBar template part '{name}' to be a {typeof(T).Name}.");
    }

    private static T FindTemplatePart<T>(Control control, string name)
        where T : DependencyObject
    {
        return control.Template.FindName(name, control) as T
            ?? throw new AssertFailedException($"Expected {control.GetType().Name} template part '{name}' to be a {typeof(T).Name}.");
    }

    private static T FindVisualDescendant<T>(DependencyObject root)
        where T : DependencyObject
    {
        return VisualTreeTestHelper.FindDescendant<T>(root)
            ?? throw new AssertFailedException($"Expected visual descendant of type {typeof(T).Name}.");
    }

    private static void AssertThemeResourceReference(string themeName, string key, object expectedResourceKey)
    {
        var theme = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(theme.Contains(expectedResourceKey), $"Theme is missing {expectedResourceKey}.");
        Assert.AreSame(theme[expectedResourceKey], theme[key], key);
    }

    private static void AssertThemeResourceValue<T>(string themeName, string key, T expectedValue)
    {
        var theme = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.AreEqual(expectedValue, theme[key], key);
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
