using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.MenuFlyoutVisualStates;

[TestClass]
public class MenuFlyoutVisualStateTests
{
    [TestMethod]
    public void SubmenuItemTemplateUsesOfficialWpfFluentStateTriggers()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var template = FindMenuItemTemplate(MenuItem.SubmenuItemTemplateKey);
            var root = (Border)template.LoadContent();
            Assert.AreEqual("Border", root.Name);
            Assert.AreEqual(new Thickness(4, 1, 4, 1), root.Margin);
            Assert.AreEqual(new CornerRadius(4), root.CornerRadius);

            AssertDynamicResourceSetter(
                template,
                MenuItem.IsHighlightedProperty,
                true,
                "Border",
                Border.BackgroundProperty,
                "MenuBarItemBackgroundSelected");
            AssertSetter(
                template,
                MenuItem.IsCheckableProperty,
                true,
                "CheckBoxIconBorder",
                UIElement.VisibilityProperty,
                Visibility.Visible);
            AssertSetter(
                template,
                MenuItem.IsCheckedProperty,
                true,
                "CheckBoxIcon",
                TextBlock.TextProperty,
                "\uE73E");
            AssertSetter(
                template,
                MenuItem.InputGestureTextProperty,
                string.Empty,
                "InputGestureText",
                UIElement.VisibilityProperty,
                Visibility.Collapsed);
        });
    }

    [TestMethod]
    public void SubmenuHeaderTemplateKeepsWpfTriggersAndCurrentRadioCheckPlaceholder()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var template = FindMenuItemTemplate(MenuItem.SubmenuHeaderTemplateKey);
            var root = (Grid)template.LoadContent();
            var elements = VisualTreeTestHelper.EnumerateDescendants(root)
                .OfType<FrameworkElement>()
                .ToArray();
            var border = elements.Single(item => item.Name == "Border");
            var checkGlyph = (TextBlock)elements.Single(item => item.Name == "CheckGlyph");

            Assert.AreEqual(new Thickness(4, 1, 4, 1), border.Margin);
            Assert.AreEqual(Visibility.Collapsed, checkGlyph.Visibility);
            Assert.AreEqual(0.0, checkGlyph.Opacity);
            Assert.AreEqual("\uE915", checkGlyph.Text);

            AssertDynamicResourceSetter(
                template,
                MenuItem.IsHighlightedProperty,
                true,
                "Border",
                Border.BackgroundProperty,
                "MenuBarItemBackgroundSelected");
            AssertSetter(
                template,
                MenuItem.IsCheckableProperty,
                true,
                "CheckGlyph",
                UIElement.VisibilityProperty,
                Visibility.Visible);
            AssertSetter(
                template,
                MenuItem.IsCheckedProperty,
                true,
                "CheckGlyph",
                UIElement.OpacityProperty,
                1.0);
        });
    }

    private static ControlTemplate FindMenuItemTemplate(object resourceKey)
    {
        return Application.Current.FindResource(resourceKey) as ControlTemplate
            ?? throw new AssertFailedException($"Expected MenuItem template resource '{resourceKey}'.");
    }

    private static void AssertSetter(
        ControlTemplate template,
        DependencyProperty triggerProperty,
        object triggerValue,
        string targetName,
        DependencyProperty property,
        object expectedValue)
    {
        var setter = FindTrigger(template, triggerProperty, triggerValue)
            .Setters
            .OfType<Setter>()
            .Single(item => item.TargetName == targetName && item.Property == property);

        Assert.AreEqual(expectedValue, setter.Value);
    }

    private static void AssertDynamicResourceSetter(
        ControlTemplate template,
        DependencyProperty triggerProperty,
        object triggerValue,
        string targetName,
        DependencyProperty property,
        object resourceKey)
    {
        var setter = FindTrigger(template, triggerProperty, triggerValue)
            .Setters
            .OfType<Setter>()
            .Single(item => item.TargetName == targetName && item.Property == property);

        Assert.IsInstanceOfType(setter.Value, typeof(DynamicResourceExtension));
        Assert.AreEqual(resourceKey, ((DynamicResourceExtension)setter.Value).ResourceKey);
    }

    private static Trigger FindTrigger(
        ControlTemplate template,
        DependencyProperty property,
        object value)
    {
        return template.Triggers
            .OfType<Trigger>()
            .Single(item => item.Property == property && Equals(item.Value, value));
    }
}
