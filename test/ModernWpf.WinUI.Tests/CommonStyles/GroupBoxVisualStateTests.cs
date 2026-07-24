using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.CommonStyles;

[TestClass]
public class GroupBoxVisualStateTests
{
    [TestMethod]
    public void DefaultGroupBoxStyleUsesOfficialWpfFluentTemplateShape()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var defaultStyle = (Style)Application.Current.FindResource("DefaultGroupBoxStyle");
            var implicitGroupBoxStyle = (Style)Application.Current.FindResource(typeof(GroupBox));
            Assert.AreEqual(typeof(GroupBox), defaultStyle.TargetType);
            Assert.AreEqual(typeof(GroupBox), implicitGroupBoxStyle.TargetType);
            Assert.AreSame(defaultStyle, implicitGroupBoxStyle.BasedOn);

            var setters = defaultStyle.Setters.OfType<Setter>().ToArray();
            Assert.AreEqual(6, setters.Length);
            AssertSetter(setters, Control.OverridesDefaultStyleProperty, true);
            AssertDynamicResourceSetter(setters, Control.BorderThicknessProperty, "GroupBoxBorderThickness");
            AssertDynamicResourceSetter(setters, Control.BackgroundProperty, "GroupBoxBackground");
            AssertDynamicResourceSetter(setters, Control.BorderBrushProperty, "GroupBoxBorderBrush");
            AssertDynamicResourceSetter(setters, Control.PaddingProperty, "GroupBoxPadding");
            Assert.IsTrue(setters.Any(item => item.Property == Control.TemplateProperty));

            var groupBox = new GroupBox
            {
                Header = "_Header",
                Content = "Content"
            };

            using var host = new TestWindowHost(groupBox, width: 240, height: 140);
            host.UpdateLayout();

            AssertOfficialRuntimeValues(groupBox);
            AssertOfficialTemplateShape(groupBox);
        });
    }

    [TestMethod]
    public void ThemeDictionariesExposeOfficialGroupBoxAliases()
    {
        WpfTestHost.Run(() =>
        {
            AssertThemeResourceReference("Light", "GroupBoxBackground", "SystemControlTransparentBrush");
            AssertThemeResourceReference("Light", "GroupBoxBorderBrush", "SystemControlTransparentBrush");
            AssertThemeResourceReference("Light", "GroupBoxHeaderForeground", "TextFillColorPrimaryBrush");

            AssertThemeResourceReference("Dark", "GroupBoxBackground", "SystemControlTransparentBrush");
            AssertThemeResourceReference("Dark", "GroupBoxBorderBrush", "SystemControlTransparentBrush");
            AssertThemeResourceReference("Dark", "GroupBoxHeaderForeground", "TextFillColorPrimaryBrush");

            AssertThemeResourceReference("HighContrast", "GroupBoxBackground", "SystemColorButtonFaceColorBrush");
            AssertThemeResourceReference("HighContrast", "GroupBoxBorderBrush", "SystemColorButtonTextColorBrush");
            AssertThemeResourceReference("HighContrast", "GroupBoxHeaderForeground", "SystemColorButtonTextColorBrush");
        });
    }

    private static void AssertOfficialRuntimeValues(GroupBox groupBox)
    {
        Assert.IsTrue(groupBox.OverridesDefaultStyle);
        Assert.AreEqual(new Thickness(0, 0, 0, 16), groupBox.Padding);
        Assert.AreEqual(new Thickness(0), groupBox.BorderThickness);
        Assert.AreSame(groupBox.TryFindResource("GroupBoxBackground"), groupBox.Background);
        Assert.AreSame(groupBox.TryFindResource("GroupBoxBorderBrush"), groupBox.BorderBrush);
    }

    private static void AssertOfficialTemplateShape(GroupBox groupBox)
    {
        var border = VisualTreeTestHelper.FindDescendant<Border>(groupBox)
            ?? throw new AssertFailedException("Expected official WPF Fluent GroupBox border chrome.");
        var presenters = VisualTreeTestHelper.EnumerateDescendants(groupBox)
            .OfType<ContentPresenter>()
            .ToArray();
        var headerPresenter = presenters.Single(item => Equals(item.Content, groupBox.Header));
        var contentPresenter = presenters.Single(item => Equals(item.Content, groupBox.Content));

        Assert.AreSame(groupBox.Background, border.Background);
        Assert.AreSame(groupBox.BorderBrush, border.BorderBrush);
        Assert.AreEqual(groupBox.BorderThickness, border.BorderThickness);
        Assert.AreEqual(typeof(ContentPresenter), headerPresenter.GetType());
        Assert.AreEqual(typeof(ContentPresenter), contentPresenter.GetType());
        Assert.AreEqual(0, Grid.GetRow(headerPresenter));
        Assert.AreEqual(1, Grid.GetRow(contentPresenter));
        Assert.AreEqual((double)groupBox.TryFindResource("GroupBoxHeaderFontSize"), TextElement.GetFontSize(headerPresenter));
        Assert.AreSame(groupBox.TryFindResource("GroupBoxHeaderForeground"), TextElement.GetForeground(headerPresenter));
        Assert.AreEqual((Thickness)groupBox.TryFindResource("GroupBoxHeaderMargin"), headerPresenter.Margin);
        Assert.AreEqual(groupBox.Padding, contentPresenter.Margin);
        Assert.IsTrue(headerPresenter.RecognizesAccessKey);
        Assert.IsNull(VisualTreeTestHelper.FindDescendant<ContentPresenterEx>(groupBox));
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
