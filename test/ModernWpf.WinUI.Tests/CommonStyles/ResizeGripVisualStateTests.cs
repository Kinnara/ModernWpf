using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.CommonStyles;

[TestClass]
public class ResizeGripVisualStateTests
{
    [TestMethod]
    public void DefaultResizeGripStyleUsesOfficialWpfFluentTemplateShape()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var defaultStyle = (Style)Application.Current.FindResource("DefaultResizeGripStyle");
            var implicitStyle = (Style)Application.Current.FindResource(typeof(ResizeGrip));
            Assert.AreEqual(typeof(ResizeGrip), defaultStyle.TargetType);
            Assert.AreEqual(typeof(ResizeGrip), implicitStyle.TargetType);
            Assert.AreSame(defaultStyle, implicitStyle.BasedOn);

            var setters = defaultStyle.Setters.OfType<Setter>().ToArray();
            Assert.AreEqual(5, setters.Length);
            AssertSetter(setters, Control.OverridesDefaultStyleProperty, true);
            AssertDynamicResourceSetter(setters, FrameworkElement.MinWidthProperty, "ResizeGripMinWidth");
            AssertDynamicResourceSetter(setters, FrameworkElement.MinHeightProperty, "ResizeGripMinHeight");
            AssertBrushSetter(setters, Control.BackgroundProperty, Colors.Transparent);
            Assert.IsTrue(setters.Any(item => item.Property == Control.TemplateProperty));

            var resizeGrip = new ResizeGrip();
            using var host = new TestWindowHost(resizeGrip, width: 80, height: 80);
            host.UpdateLayout();

            AssertRuntimeValues(resizeGrip);
            AssertTemplateShape(resizeGrip);
        });
    }

    [TestMethod]
    public void ThemeDictionariesExposeOfficialResizeGripForegroundAlias()
    {
        WpfTestHost.Run(() =>
        {
            AssertThemeResourceReference("Light", "ResizeGripForeground", "ControlStrongFillColorDefaultBrush");
            AssertThemeResourceReference("Dark", "ResizeGripForeground", "ControlStrongFillColorDefaultBrush");
            AssertThemeResourceReference("HighContrast", "ResizeGripForeground", "SystemColorButtonTextColorBrush");
        });
    }

    private static void AssertRuntimeValues(ResizeGrip resizeGrip)
    {
        Assert.IsTrue(resizeGrip.OverridesDefaultStyle);
        Assert.AreEqual((double)resizeGrip.TryFindResource("ResizeGripMinWidth"), resizeGrip.MinWidth);
        Assert.AreEqual((double)resizeGrip.TryFindResource("ResizeGripMinHeight"), resizeGrip.MinHeight);
        Assert.AreEqual(Brushes.Transparent.ToString(), resizeGrip.Background.ToString());
        Assert.AreEqual(12.0, (double)resizeGrip.TryFindResource("ResizeGripMinWidth"));
        Assert.AreEqual(12.0, (double)resizeGrip.TryFindResource("ResizeGripMinHeight"));
        Assert.AreEqual(8.0, (double)resizeGrip.TryFindResource("ResizeGripIconSize"));
        Assert.AreEqual("\uF169", resizeGrip.TryFindResource("ResizeGripIconGlyph"));
    }

    private static void AssertTemplateShape(ResizeGrip resizeGrip)
    {
        var textBlock = VisualTreeTestHelper.FindDescendant<TextBlock>(resizeGrip)
            ?? throw new AssertFailedException("Expected official WPF Fluent ResizeGrip glyph TextBlock.");

        Assert.AreSame(resizeGrip.TryFindResource("SymbolThemeFontFamily"), textBlock.FontFamily);
        Assert.AreEqual((double)resizeGrip.TryFindResource("ResizeGripIconSize"), textBlock.FontSize);
        Assert.AreSame(resizeGrip.TryFindResource("ResizeGripForeground"), textBlock.Foreground);
        Assert.AreEqual(resizeGrip.TryFindResource("ResizeGripIconGlyph"), textBlock.Text);
        Assert.IsFalse(VisualTreeTestHelper.EnumerateDescendants(resizeGrip).OfType<Path>().Any());
    }

    private static void AssertSetter(Setter[] setters, DependencyProperty property, object value)
    {
        var setter = setters.Single(item => item.Property == property);
        Assert.AreEqual(value, setter.Value);
    }

    private static void AssertBrushSetter(Setter[] setters, DependencyProperty property, Color color)
    {
        var setter = setters.Single(item => item.Property == property);
        Assert.IsInstanceOfType(setter.Value, typeof(SolidColorBrush));
        Assert.AreEqual(color, ((SolidColorBrush)setter.Value).Color);
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
