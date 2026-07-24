using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.CommonStyles;

[TestClass]
public class ToolTipVisualStateTests
{
    [TestMethod]
    public void DefaultToolTipStyleUsesOfficialWpfFluentTemplateShape()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var defaultStyle = (Style)Application.Current.FindResource("DefaultToolTipStyle");
            var implicitToolTipStyle = (Style)Application.Current.FindResource(typeof(ToolTip));
            Assert.AreEqual(typeof(ToolTip), defaultStyle.TargetType);
            Assert.AreEqual(typeof(ToolTip), implicitToolTipStyle.TargetType);
            Assert.AreSame(defaultStyle, implicitToolTipStyle.BasedOn);

            var resources = new ResourceDictionary
            {
                Source = new System.Uri("/ModernWpf;component/Styles/ToolTip.xaml", System.UriKind.Relative)
            };
            AssertResource(resources, "ToolTipBorderPadding", new Thickness(9, 6, 9, 8));
            AssertResource(resources, "ToolTipMaxWidth", 320.0);
            AssertResource(resources, "ToolTipBorderThemeThickness", new Thickness(1));

            AssertDynamicResourceSetter(defaultStyle, Control.ForegroundProperty, "ToolTipForegroundBrush");
            AssertDynamicResourceSetter(defaultStyle, Control.FontFamilyProperty, SystemFonts.StatusFontFamilyKey);
            AssertDynamicResourceSetter(defaultStyle, Control.FontSizeProperty, SystemFonts.StatusFontSizeKey);
            AssertDynamicResourceSetter(defaultStyle, Control.FontStyleProperty, SystemFonts.StatusFontStyleKey);
            AssertDynamicResourceSetter(defaultStyle, Control.FontWeightProperty, SystemFonts.StatusFontWeightKey);
            AssertDynamicResourceSetter(defaultStyle, Control.BackgroundProperty, "ToolTipBackgroundBrush");
            AssertDynamicResourceSetter(defaultStyle, Control.BorderBrushProperty, "ToolTipBorderBrush");
            AssertDynamicResourceSetter(defaultStyle, Control.BorderThicknessProperty, "ToolTipBorderThemeThickness");
            AssertSetterValue(defaultStyle, Control.PaddingProperty, new Thickness(9, 6, 9, 8));
            AssertSetterValue(defaultStyle, FrameworkElement.MaxWidthProperty, 320.0);
            AssertSetterValue(defaultStyle, Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Left);
            AssertSetterValue(defaultStyle, Control.VerticalContentAlignmentProperty, VerticalAlignment.Center);
            AssertSetterValue(defaultStyle, RenderOptions.ClearTypeHintProperty, ClearTypeHint.Enabled);
            AssertSetterValue(defaultStyle, UIElement.SnapsToDevicePixelsProperty, true);
            AssertSetterValue(defaultStyle, Control.OverridesDefaultStyleProperty, true);
            Assert.IsInstanceOfType(FindSetter(defaultStyle, Control.TemplateProperty)?.Value, typeof(ControlTemplate));

            var toolTip = new ToolTip
            {
                Content = new TextBlock { Text = "Tip" }
            };
            toolTip.ApplyTemplate();
            toolTip.Measure(new Size(240, 120));
            toolTip.Arrange(new Rect(0, 0, 240, 120));
            toolTip.UpdateLayout();

            AssertOfficialSetters(toolTip);
            AssertOfficialTemplateShape(toolTip);
        });
    }

    [TestMethod]
    public void ThemeDictionariesExposeOfficialToolTipAliases()
    {
        WpfTestHost.Run(() =>
        {
            foreach (var themeName in new[] { "Light", "Dark" })
            {
                AssertThemeResourceReference(themeName, "ToolTipForegroundBrush", "TextFillColorPrimaryBrush");
                AssertThemeResourceReference(themeName, "ToolTipBackgroundBrush", "AcrylicBackgroundFillColorDefaultBrush");
                AssertThemeResourceReference(themeName, "ToolTipBorderBrush", "SurfaceStrokeColorFlyoutBrush");
                AssertThemeResourceReference(themeName, "ToolTipForeground", "TextFillColorPrimaryBrush");
                AssertThemeResourceReference(themeName, "ToolTipBackground", "AcrylicBackgroundFillColorDefaultBrush");
                AssertThemeResourceValue(themeName, "ToolTipContentThemeFontSize", 12.0);
                AssertThemeResourceValue(themeName, "ToolTipBorderThemeThickness", new Thickness(1));
            }

            AssertThemeResourceReference("HighContrast", "ToolTipForegroundBrush", "SystemColorWindowTextColorBrush");
            AssertThemeResourceReference("HighContrast", "ToolTipBackgroundBrush", "SystemColorWindowColorBrush");
            AssertThemeResourceReference("HighContrast", "ToolTipBorderBrush", "SystemColorWindowTextColorBrush");
            AssertThemeResourceReference("HighContrast", "ToolTipForeground", "SystemColorWindowTextColorBrush");
            AssertThemeResourceReference("HighContrast", "ToolTipBackground", "SystemColorWindowColorBrush");
            AssertThemeResourceValue("HighContrast", "ToolTipContentThemeFontSize", 12.0);
            AssertThemeResourceValue("HighContrast", "ToolTipBorderThemeThickness", new Thickness(1));
        });
    }

    private static void AssertOfficialSetters(ToolTip toolTip)
    {
        Assert.AreSame(toolTip.TryFindResource("ToolTipForegroundBrush"), toolTip.Foreground);
        Assert.AreEqual(SystemFonts.StatusFontFamily, toolTip.FontFamily);
        Assert.AreEqual(SystemFonts.StatusFontSize, toolTip.FontSize);
        Assert.AreEqual(SystemFonts.StatusFontStyle, toolTip.FontStyle);
        Assert.AreEqual(SystemFonts.StatusFontWeight, toolTip.FontWeight);
        Assert.AreSame(toolTip.TryFindResource("ToolTipBackgroundBrush"), toolTip.Background);
        Assert.AreSame(toolTip.TryFindResource("ToolTipBorderBrush"), toolTip.BorderBrush);
        Assert.AreEqual(new Thickness(1), toolTip.BorderThickness);
        Assert.AreEqual(new Thickness(9, 6, 9, 8), toolTip.Padding);
        Assert.AreEqual(320.0, toolTip.MaxWidth);
        Assert.AreEqual(HorizontalAlignment.Left, toolTip.HorizontalContentAlignment);
        Assert.AreEqual(VerticalAlignment.Center, toolTip.VerticalContentAlignment);
        Assert.IsTrue(toolTip.SnapsToDevicePixels);
        Assert.IsTrue(toolTip.OverridesDefaultStyle);
    }

    private static void AssertOfficialTemplateShape(ToolTip toolTip)
    {
        toolTip.ApplyTemplate();

        var border = GetTemplateChild<Border>(toolTip, "Border");
        var presenter = VisualTreeTestHelper.EnumerateDescendants(border).OfType<ContentPresenter>().Single();

        Assert.AreSame(toolTip.Background, border.Background);
        Assert.AreSame(toolTip.BorderBrush, border.BorderBrush);
        Assert.AreEqual(toolTip.BorderThickness, border.BorderThickness);
        Assert.AreEqual(new CornerRadius(4), border.CornerRadius);
        Assert.IsInstanceOfType(border.Effect, typeof(DropShadowEffect));

        var shadow = (DropShadowEffect)border.Effect;
        Assert.AreEqual(30.0, shadow.BlurRadius);
        Assert.AreEqual(0.0, shadow.Direction);
        Assert.AreEqual(0.4, shadow.Opacity);
        Assert.AreEqual(0.0, shadow.ShadowDepth);

        Assert.AreEqual(toolTip.Padding, presenter.Margin);
        Assert.AreEqual(toolTip.HorizontalContentAlignment, presenter.HorizontalAlignment);
        Assert.AreEqual(toolTip.VerticalContentAlignment, presenter.VerticalAlignment);
        Assert.AreEqual(typeof(ContentPresenter), presenter.GetType());
        Assert.IsNull(VisualTreeTestHelper.FindDescendant<ContentPresenterEx>(toolTip));
        Assert.IsNull(VisualTreeTestHelper.FindDescendant<ThemeShadowChrome>(toolTip));

        var textBlockStyle = presenter.Resources[typeof(TextBlock)] as Style
            ?? throw new AssertFailedException("Expected official WPF Fluent ToolTip TextBlock wrapping style.");
        var wrappingSetter = textBlockStyle.Setters.OfType<Setter>()
            .Single(item => item.Property == TextBlock.TextWrappingProperty);
        Assert.AreEqual(TextWrapping.WrapWithOverflow, wrappingSetter.Value);
    }

    private static void AssertResource(ResourceDictionary resources, string key, object expected)
    {
        Assert.IsTrue(resources.Contains(key), $"Expected resource '{key}' to exist.");
        Assert.AreEqual(expected, resources[key], $"Unexpected value for '{key}'.");
    }

    private static void AssertDynamicResourceSetter(Style style, DependencyProperty property, object expectedResourceKey)
    {
        var setter = FindSetter(style, property);
        Assert.IsNotNull(setter, $"Expected setter for {property.Name}.");
        Assert.IsInstanceOfType(setter!.Value, typeof(DynamicResourceExtension));

        var dynamicResource = (DynamicResourceExtension)setter.Value;
        Assert.AreEqual(expectedResourceKey, dynamicResource.ResourceKey);
    }

    private static void AssertSetterValue(Style style, DependencyProperty property, object expectedValue)
    {
        var setter = FindSetter(style, property);
        Assert.IsNotNull(setter, $"Expected setter for {property.Name}.");
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

    private static T GetTemplateChild<T>(Control control, string name)
        where T : DependencyObject
    {
        return control.Template.FindName(name, control) as T
            ?? throw new AssertFailedException($"Expected {control.GetType().Name} template child '{name}' to be a {typeof(T).Name}.");
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
        Assert.IsTrue(theme.Contains(key), $"Theme is missing {key}.");
        Assert.AreEqual(expectedValue, theme[key], key);
    }
}
