using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Shapes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.CommonStyles;

[TestClass]
public class GridSplitterVisualStateTests
{
    [TestMethod]
    public void DefaultGridSplitterStyleUsesOfficialWpfFluentTemplateShape()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var defaultStyle = (Style)Application.Current.FindResource("DefaultGridSplitterStyle");
            var implicitStyle = (Style)Application.Current.FindResource(typeof(GridSplitter));
            Assert.AreEqual(typeof(GridSplitter), defaultStyle.TargetType);
            Assert.AreEqual(typeof(GridSplitter), implicitStyle.TargetType);
            Assert.AreSame(defaultStyle, implicitStyle.BasedOn);

            var setters = defaultStyle.Setters.OfType<Setter>().ToArray();
            Assert.AreEqual(11, setters.Length);
            AssertSetter(setters, Control.IsTabStopProperty, true);
            AssertSetter(setters, FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Stretch);
            AssertSetter(setters, FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Stretch);
            AssertDynamicResourceSetter(setters, FrameworkElement.MinHeightProperty, "GridsplitterMinHeight");
            AssertDynamicResourceSetter(setters, FrameworkElement.MinWidthProperty, "GridsplitterMinHeight");
            AssertDynamicResourceSetter(setters, Control.PaddingProperty, "GridsplitterPadding");
            AssertDynamicResourceSetter(setters, Control.ForegroundProperty, "GridsplitterForeground");
            AssertDynamicResourceSetter(setters, Control.BackgroundProperty, "GridsplitterBackground");
            AssertSetter(setters, Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Center);
            AssertSetter(setters, Control.VerticalContentAlignmentProperty, VerticalAlignment.Center);
            Assert.IsTrue(setters.Any(item => item.Property == Control.TemplateProperty));
            Assert.IsFalse(setters.Any(item => item.Property == Control.OverridesDefaultStyleProperty));
            Assert.IsFalse(setters.Any(item => item.Property == GridSplitter.PreviewStyleProperty));

            var gridSplitter = new GridSplitter();
            using var host = new TestWindowHost(gridSplitter, width: 80, height: 80);
            host.UpdateLayout();

            AssertRuntimeValues(gridSplitter);
            AssertTemplateShape(gridSplitter);
            AssertTriggerShape(gridSplitter);
        });
    }

    [TestMethod]
    public void ThemeDictionariesExposeOfficialGridSplitterAliases()
    {
        WpfTestHost.Run(() =>
        {
            foreach (var themeName in new[] { "Light", "Dark" })
            {
                AssertThemeResourceReference(themeName, "GridsplitterBackground", "ControlAltFillColorTransparentBrush");
                AssertThemeResourceReference(themeName, "GridsplitterBackgroundPointerOver", "ControlAltFillColorTertiaryBrush");
                AssertThemeResourceReference(themeName, "GridsplitterBackgroundPressed", "ControlAltFillColorQuarternaryBrush");
                AssertThemeResourceReference(themeName, "GridsplitterBackgroundDisabled", "ControlAltFillColorDisabledBrush");
                AssertThemeResourceReference(themeName, "GridsplitterForeground", "ControlStrongFillColorDefaultBrush");
            }

            AssertThemeResourceReference("HighContrast", "GridsplitterBackground", "SystemControlTransparentBrush");
            AssertThemeResourceReference("HighContrast", "GridsplitterBackgroundPointerOver", "SystemColorHighlightTextColorBrush");
            AssertThemeResourceReference("HighContrast", "GridsplitterBackgroundPressed", "SystemColorHighlightColorBrush");
            AssertThemeResourceReference("HighContrast", "GridsplitterBackgroundDisabled", "SystemColorWindowColorBrush");
            AssertThemeResourceReference("HighContrast", "GridsplitterForeground", "SystemColorButtonTextColorBrush");
        });
    }

    private static void AssertRuntimeValues(GridSplitter gridSplitter)
    {
        Assert.IsTrue(gridSplitter.IsTabStop);
        Assert.AreEqual(HorizontalAlignment.Stretch, gridSplitter.HorizontalAlignment);
        Assert.AreEqual(VerticalAlignment.Stretch, gridSplitter.VerticalAlignment);
        Assert.AreEqual((double)gridSplitter.TryFindResource("GridsplitterMinHeight"), gridSplitter.MinHeight);
        Assert.AreEqual((double)gridSplitter.TryFindResource("GridsplitterMinHeight"), gridSplitter.MinWidth);
        Assert.AreEqual(new Thickness(4), gridSplitter.TryFindResource("GridsplitterPadding"));
        Assert.AreEqual((Thickness)gridSplitter.TryFindResource("GridsplitterPadding"), gridSplitter.Padding);
        Assert.AreSame(gridSplitter.TryFindResource("GridsplitterForeground"), gridSplitter.Foreground);
        Assert.AreSame(gridSplitter.TryFindResource("GridsplitterBackground"), gridSplitter.Background);
        Assert.AreEqual(HorizontalAlignment.Center, gridSplitter.HorizontalContentAlignment);
        Assert.AreEqual(VerticalAlignment.Center, gridSplitter.VerticalContentAlignment);
        Assert.AreEqual(24d, (double)gridSplitter.TryFindResource("GridsplitterThumbHeight"));
        Assert.AreEqual(4d, (double)gridSplitter.TryFindResource("GridsplitterThumbWidth"));
        Assert.AreEqual(2d, (double)gridSplitter.TryFindResource("GridsplitterThumbRadius"));
        Assert.AreEqual(8d, (double)gridSplitter.TryFindResource("GridsplitterMinHeight"));
        Assert.AreEqual(8d, (double)gridSplitter.TryFindResource("GridsplitterMinWidth"));
    }

    private static void AssertTemplateShape(GridSplitter gridSplitter)
    {
        var rootGrid = FindTemplateChild<Border>(gridSplitter, "RootGrid");
        var thumb = FindTemplateChild<Rectangle>(gridSplitter, "PART_Thumb");

        gridSplitter.Cursor = Cursors.SizeWE;
        gridSplitter.UpdateLayout();

        Assert.AreSame(gridSplitter.Background, rootGrid.Background);
        Assert.AreSame(gridSplitter.Foreground, thumb.Fill);
        Assert.AreEqual((double)gridSplitter.TryFindResource("GridsplitterThumbWidth"), thumb.Width);
        Assert.AreEqual((double)gridSplitter.TryFindResource("GridsplitterThumbHeight"), thumb.Height);
        Assert.AreEqual(gridSplitter.Padding, thumb.Margin);
        Assert.AreEqual((double)gridSplitter.TryFindResource("GridsplitterThumbRadius"), thumb.RadiusX);
        Assert.AreEqual((double)gridSplitter.TryFindResource("GridsplitterThumbRadius"), thumb.RadiusY);

        gridSplitter.Cursor = Cursors.SizeNS;
        gridSplitter.UpdateLayout();
        Assert.AreEqual((double)gridSplitter.TryFindResource("GridsplitterThumbHeight"), thumb.Width);
        Assert.AreEqual((double)gridSplitter.TryFindResource("GridsplitterThumbWidth"), thumb.Height);
    }

    private static void AssertTriggerShape(GridSplitter gridSplitter)
    {
        var triggers = gridSplitter.Template.Triggers.OfType<Trigger>().ToArray();
        Assert.AreEqual(4, triggers.Length);

        AssertSingleSetterTrigger(
            triggers,
            UIElement.IsMouseOverProperty,
            true,
            "RootGrid",
            Control.BackgroundProperty,
            "GridsplitterBackgroundPointerOver");
        AssertSingleSetterTrigger(
            triggers,
            Thumb.IsDraggingProperty,
            true,
            "RootGrid",
            Control.BackgroundProperty,
            "GridsplitterBackgroundPressed");
        AssertTrigger(
            triggers,
            UIElement.IsEnabledProperty,
            false,
            ("RootGrid", Control.BackgroundProperty, "GridsplitterBackgroundDisabled"),
            ("PART_Thumb", UIElement.OpacityProperty, 0.45));
        AssertTrigger(
            triggers,
            FrameworkElement.CursorProperty,
            Cursors.SizeNS,
            ("PART_Thumb", FrameworkElement.WidthProperty, "GridsplitterThumbHeight"),
            ("PART_Thumb", FrameworkElement.HeightProperty, "GridsplitterThumbWidth"));
    }

    private static T FindTemplateChild<T>(Control control, string name)
        where T : FrameworkElement
    {
        return control.Template.FindName(name, control) as T
            ?? throw new AssertFailedException($"Could not find template child '{name}'.");
    }

    private static void AssertSingleSetterTrigger(
        Trigger[] triggers,
        DependencyProperty property,
        object value,
        string targetName,
        DependencyProperty setterProperty,
        object setterValue)
    {
        AssertTrigger(triggers, property, value, (targetName, setterProperty, setterValue));
    }

    private static void AssertTrigger(
        Trigger[] triggers,
        DependencyProperty property,
        object value,
        params (string TargetName, DependencyProperty Property, object Value)[] expectedSetters)
    {
        var trigger = triggers.Single(item => item.Property == property && Equals(item.Value, value));
        var setters = trigger.Setters.OfType<Setter>().ToArray();
        Assert.AreEqual(expectedSetters.Length, setters.Length);

        foreach (var expected in expectedSetters)
        {
            var setter = setters.Single(item => item.TargetName == expected.TargetName && item.Property == expected.Property);
            if (expected.Value is string resourceKey)
            {
                Assert.IsInstanceOfType(setter.Value, typeof(DynamicResourceExtension));
                Assert.AreEqual(resourceKey, ((DynamicResourceExtension)setter.Value).ResourceKey);
            }
            else
            {
                Assert.AreEqual(expected.Value, setter.Value);
            }
        }
    }

    private static void AssertSetter(Setter[] setters, DependencyProperty property, object value)
    {
        var setter = setters.Single(item => item.Property == property);
        Assert.AreEqual(value, setter.Value);
    }

    private static void AssertDynamicResourceSetter(Setter[] setters, DependencyProperty property, object resourceKey)
    {
        var setter = setters.Single(item => item.Property == property);
        Assert.IsInstanceOfType(setter.Value, typeof(DynamicResourceExtension));
        var dynamicResource = (DynamicResourceExtension)setter.Value;
        Assert.AreEqual(resourceKey, dynamicResource.ResourceKey);
    }

    private static void AssertThemeResourceReference(string themeName, string key, object expectedResourceKey)
    {
        var theme = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(theme.Contains(expectedResourceKey), $"Theme is missing {expectedResourceKey}.");
        Assert.AreSame(theme[expectedResourceKey], theme[key], key);
    }
}
