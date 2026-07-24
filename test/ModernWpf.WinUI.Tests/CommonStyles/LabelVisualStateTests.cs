using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.CommonStyles;

[TestClass]
public class LabelVisualStateTests
{
    [TestMethod]
    public void DefaultLabelStyleUsesOfficialWpfFluentStyleSurface()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var defaultStyle = (Style)Application.Current.FindResource("DefaultLabelStyle");
            var implicitLabelStyle = (Style)Application.Current.FindResource(typeof(Label));
            Assert.AreEqual(typeof(Label), defaultStyle.TargetType);
            Assert.AreEqual(typeof(Label), implicitLabelStyle.TargetType);
            Assert.AreSame(defaultStyle, implicitLabelStyle.BasedOn);

            var setters = defaultStyle.Setters.OfType<Setter>().ToArray();
            Assert.AreEqual(4, setters.Length);
            AssertSetter(setters, Control.PaddingProperty, new Thickness(0, 0, 0, 4));
            AssertSetter(setters, UIElement.FocusableProperty, false);
            AssertDynamicResourceSetter(setters, Control.ForegroundProperty, "LabelForeground");
            AssertSetter(setters, UIElement.SnapsToDevicePixelsProperty, true);
            Assert.IsFalse(setters.Any(item => item.Property == Control.TemplateProperty));
            Assert.IsFalse(setters.Any(item => item.Property == Control.OverridesDefaultStyleProperty));

            var label = new Label { Content = "_Label content" };
            using var host = new TestWindowHost(label, width: 180, height: 60);
            host.UpdateLayout();

            Assert.AreEqual(new Thickness(0, 0, 0, 4), label.Padding);
            Assert.IsFalse(label.Focusable);
            Assert.IsTrue(label.SnapsToDevicePixels);
            Assert.IsFalse(label.OverridesDefaultStyle);
            Assert.AreSame(label.TryFindResource("LabelForeground"), label.Foreground);
            Assert.IsNull(VisualTreeTestHelper.FindDescendant<ContentPresenterEx>(label));
        });
    }

    [TestMethod]
    public void ThemeDictionariesExposeOfficialLabelForegroundAlias()
    {
        WpfTestHost.Run(() =>
        {
            AssertThemeResourceReference("Light", "LabelForeground", "TextFillColorPrimaryBrush");
            AssertThemeResourceReference("Dark", "LabelForeground", "TextFillColorPrimaryBrush");
            AssertThemeResourceReference("HighContrast", "LabelForeground", "SystemColorGrayTextColorBrush");
        });
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
