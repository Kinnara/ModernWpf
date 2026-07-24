using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

using WpfExpander = System.Windows.Controls.Expander;

namespace ModernWpf.WinUI.Tests.Expander;

[TestClass]
public class ExpanderApiTests
{
    [TestMethod]
    public void VerifyExpanderDefaultStyleUsesOfficialWpfFluentShape()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var defaultStyle = AssertStyleResource("DefaultExpanderStyle");
            var implicitStyle = AssertStyleResource(typeof(WpfExpander));
            Assert.AreEqual(typeof(WpfExpander), defaultStyle.TargetType);
            Assert.AreEqual(typeof(WpfExpander), implicitStyle.TargetType);
            Assert.AreSame(defaultStyle, implicitStyle.BasedOn);
            AssertStyleHasSetter(defaultStyle, Control.FocusVisualStyleProperty);

            var contentButton = new Button { Content = "Content" };
            var expander = new WpfExpander
            {
                Style = implicitStyle,
                Header = "Header",
                Content = contentButton,
                IsExpanded = true
            };
            expander.SetValue(System.Windows.Controls.Border.CornerRadiusProperty, new CornerRadius(6));

            using var host = new TestWindowHost(expander, width: 400, height: 240);
            host.UpdateLayout();

            Assert.IsNotNull(expander.FocusVisualStyle);
            Assert.AreSame(expander.TryFindResource("ExpanderHeaderBackground"), expander.Background);
            Assert.AreSame(expander.TryFindResource("ExpanderHeaderForeground"), expander.Foreground);
            Assert.AreSame(expander.TryFindResource("ExpanderHeaderBorderBrush"), expander.BorderBrush);
            Assert.AreEqual(new Thickness(1), expander.BorderThickness);
            Assert.AreEqual(new Thickness(11), expander.Padding);
            Assert.AreEqual(HorizontalAlignment.Stretch, expander.HorizontalAlignment);
            Assert.AreEqual(VerticalAlignment.Center, expander.VerticalAlignment);
            Assert.AreEqual(HorizontalAlignment.Stretch, expander.HorizontalContentAlignment);
            Assert.AreEqual(VerticalAlignment.Center, expander.VerticalContentAlignment);
            Assert.AreEqual(FontWeights.Normal, expander.FontWeight);
            Assert.AreEqual(new CornerRadius(6), ((CornerRadius)expander.GetValue(System.Windows.Controls.Border.CornerRadiusProperty)));
            Assert.IsTrue(expander.IsExpanded);
            Assert.IsTrue(expander.OverridesDefaultStyle);
            Assert.IsTrue(expander.SnapsToDevicePixels);

            Assert.AreEqual(new Thickness(11), expander.TryFindResource("ExpanderPadding"));
            Assert.AreEqual(new Thickness(1), expander.TryFindResource("ExpanderBorderThemeThickness"));
            Assert.AreEqual(12.0, expander.TryFindResource("ExpanderChevronSize"));
            Assert.AreEqual("\uE70E", expander.TryFindResource("ExpanderChevronUpGlyph"));
            Assert.AreEqual("\uE70D", expander.TryFindResource("ExpanderChevronDownGlyph"));
            Assert.AreEqual("\uE76B", expander.TryFindResource("ExpanderChevronLeftGlyph"));
            Assert.AreEqual("\uE76C", expander.TryFindResource("ExpanderChevronRightGlyph"));
            Assert.IsInstanceOfType(expander.TryFindResource("AnimationFactorToValueConverter"), typeof(AnimationFactorToValueConverter));

            var toggleButtonBorder = FindTemplateChild<Border>(expander, "ToggleButtonBorder");
            Assert.AreSame(expander.Background, toggleButtonBorder.Background);
            Assert.AreSame(expander.BorderBrush, toggleButtonBorder.BorderBrush);
            Assert.AreEqual(expander.BorderThickness, toggleButtonBorder.BorderThickness);
            Assert.AreEqual(new CornerRadius(6), toggleButtonBorder.CornerRadius);

            var headerSite = FindTemplateChild<ToggleButton>(expander, "HeaderSite");
            Assert.IsTrue(headerSite.Focusable);
            Assert.IsTrue(headerSite.IsChecked == true);
            Assert.AreEqual(expander.Header, headerSite.Content);
            Assert.AreSame(expander.TryFindResource("DefaultExpanderToggleButtonDownStyle"), headerSite.Template);
            Assert.IsNotNull(headerSite.FocusVisualStyle);
            Assert.IsNotNull(headerSite.Background);
            Assert.IsNotNull(headerSite.Foreground);
            Assert.IsNotNull(headerSite.BorderBrush);
            Assert.AreEqual(expander.BorderThickness, headerSite.BorderThickness);
            Assert.AreEqual(expander.Padding, headerSite.Padding);
            Assert.AreEqual(expander.HorizontalContentAlignment, headerSite.HorizontalContentAlignment);
            Assert.AreEqual(expander.VerticalContentAlignment, headerSite.VerticalContentAlignment);
            Assert.IsTrue(headerSite.OverridesDefaultStyle);
            Assert.IsFalse(ToggleButtonHelper.GetVisualStateSettersEnabled(headerSite));

            headerSite.ApplyTemplate();
            var headerPresenter = FindTemplateChild<ContentPresenter>(headerSite, "ContentPresenter");
            Assert.AreEqual(typeof(ContentPresenter), headerPresenter.GetType());
            Assert.AreEqual(expander.Header, headerPresenter.Content);
            var chevron = FindTemplateChild<TextBlock>(headerSite, "ControlChevronIcon");
            Assert.IsNotNull(chevron.Foreground);
            Assert.IsTrue(headerSite.Template.Triggers.Count > 0);
            Assert.IsNull(VisualTreeTestHelper.FindDescendant<FontIconFallback>(headerSite));

            var contentGrid = FindTemplateChild<Grid>(expander, "ContentPresenterGrid");
            Assert.AreEqual(Dock.Bottom, DockPanel.GetDock(contentGrid));

            var contentBorder = FindTemplateChild<Border>(expander, "ContentPresenterBorder");
            Assert.AreSame(expander.TryFindResource("ExpanderContentBackground"), contentBorder.Background);
            Assert.AreSame(expander.BorderBrush, contentBorder.BorderBrush);
            Assert.AreEqual(new Thickness(1, 0, 1, 1), contentBorder.BorderThickness);
            Assert.AreEqual(new CornerRadius(0, 0, 4, 4), contentBorder.CornerRadius);
            Assert.AreEqual(Visibility.Visible, contentBorder.Visibility);

            var contentPresenter = FindTemplateChild<ContentPresenter>(expander, "ContentPresenter");
            Assert.AreEqual(typeof(ContentPresenter), contentPresenter.GetType());
            Assert.AreEqual(expander.Content, contentPresenter.Content);
            Assert.AreEqual(expander.Padding, contentPresenter.Margin);

            Assert.IsTrue(expander.Template.Triggers.Count > 0);
            Assert.IsNull(VisualTreeTestHelper.FindDescendant<ContentPresenterEx>(expander));
            Assert.IsNull(VisualTreeTestHelper.FindDescendant<FontIconFallback>(expander));
        });
    }

    [TestMethod]
    public void VerifyExpanderDirectionTriggersUseOfficialWpfFluentTemplates()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            AssertExpandedDirection(
                ExpandDirection.Down,
                Dock.Bottom,
                Dock.Top,
                "DefaultExpanderToggleButtonDownStyle",
                new Thickness(1, 0, 1, 1),
                new CornerRadius(0, 0, 4, 4),
                new CornerRadius(4));
            AssertExpandedDirection(
                ExpandDirection.Up,
                Dock.Top,
                Dock.Bottom,
                "DefaultExpanderToggleButtonUpStyle",
                new Thickness(1, 1, 1, 0),
                new CornerRadius(4, 4, 0, 0),
                new CornerRadius(0, 0, 4, 4));
            AssertExpandedDirection(
                ExpandDirection.Left,
                Dock.Left,
                Dock.Right,
                "DefaultExpanderToggleButtonLeftStyle",
                new Thickness(1, 1, 0, 1),
                new CornerRadius(4, 0, 0, 4),
                new CornerRadius(0, 4, 4, 0));
            AssertExpandedDirection(
                ExpandDirection.Right,
                Dock.Right,
                Dock.Left,
                "DefaultExpanderToggleButtonRightStyle",
                new Thickness(0, 1, 1, 1),
                new CornerRadius(0, 4, 4, 0),
                new CornerRadius(4, 0, 0, 4));
        });
    }

    [TestMethod]
    public void ExpanderStyleUsesOfficialWpfFluentResourceAliases()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var defaultStyle = AssertStyleResource("DefaultExpanderStyle");
            var implicitStyle = AssertStyleResource(typeof(WpfExpander));
            var expander = new WpfExpander
            {
                Style = implicitStyle,
                Header = "Header",
                Content = "Content",
                IsExpanded = true
            };

            using var host = new TestWindowHost(expander, width: 400, height: 240);
            host.UpdateLayout();

            Assert.AreSame(defaultStyle, implicitStyle.BasedOn);
            AssertDynamicResourceSetter(defaultStyle, Control.FocusVisualStyleProperty, "DefaultControlFocusVisualStyle");
            AssertDynamicResourceSetter(defaultStyle, Control.BackgroundProperty, "ExpanderHeaderBackground");
            AssertDynamicResourceSetter(defaultStyle, Control.ForegroundProperty, "ExpanderHeaderForeground");
            AssertDynamicResourceSetter(defaultStyle, Control.BorderBrushProperty, "ExpanderHeaderBorderBrush");
            AssertSetterValue(defaultStyle, Control.BorderThicknessProperty, new Thickness(1));
            AssertSetterValue(defaultStyle, Control.PaddingProperty, new Thickness(11));
            AssertSetterValue(defaultStyle, FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Stretch);
            AssertSetterValue(defaultStyle, FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
            AssertSetterValue(defaultStyle, Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch);
            AssertSetterValue(defaultStyle, Control.VerticalContentAlignmentProperty, VerticalAlignment.Center);
            AssertSetterValue(defaultStyle, Control.FontWeightProperty, FontWeights.Normal);
            AssertDynamicResourceSetter(defaultStyle, System.Windows.Controls.Border.CornerRadiusProperty, "ControlCornerRadius");
            AssertSetterValue(defaultStyle, WpfExpander.IsExpandedProperty, false);
            AssertSetterValue(defaultStyle, UIElement.SnapsToDevicePixelsProperty, true);
            AssertSetterValue(defaultStyle, FrameworkElement.OverridesDefaultStyleProperty, true);

            Assert.AreSame(expander.TryFindResource("DefaultControlFocusVisualStyle"), expander.FocusVisualStyle);
            Assert.AreSame(expander.TryFindResource("ExpanderHeaderBackground"), expander.Background);
            Assert.AreSame(expander.TryFindResource("ExpanderHeaderForeground"), expander.Foreground);
            Assert.AreSame(expander.TryFindResource("ExpanderHeaderBorderBrush"), expander.BorderBrush);
            Assert.AreEqual(expander.TryFindResource("ExpanderBorderThemeThickness"), expander.BorderThickness);
            Assert.AreEqual(expander.TryFindResource("ExpanderPadding"), expander.Padding);
            Assert.AreEqual(expander.TryFindResource("ControlCornerRadius"), expander.GetValue(System.Windows.Controls.Border.CornerRadiusProperty));

            var toggleButtonBorder = FindTemplateChild<Border>(expander, "ToggleButtonBorder");
            var headerSite = FindTemplateChild<ToggleButton>(expander, "HeaderSite");
            var contentBorder = FindTemplateChild<Border>(expander, "ContentPresenterBorder");
            var contentPresenter = FindTemplateChild<ContentPresenter>(expander, "ContentPresenter");

            Assert.AreSame(expander.Background, toggleButtonBorder.Background);
            Assert.AreSame(expander.BorderBrush, toggleButtonBorder.BorderBrush);
            Assert.AreEqual(expander.BorderThickness, toggleButtonBorder.BorderThickness);
            Assert.AreEqual(expander.GetValue(System.Windows.Controls.Border.CornerRadiusProperty), toggleButtonBorder.CornerRadius);
            Assert.AreSame(expander.TryFindResource("DefaultControlFocusVisualStyle"), headerSite.FocusVisualStyle);
            Assert.AreSame(expander.Foreground, headerSite.Foreground);
            Assert.AreEqual(expander.BorderThickness, headerSite.BorderThickness);
            Assert.AreEqual(expander.Padding, headerSite.Padding);
            Assert.AreSame(expander.TryFindResource("DefaultExpanderToggleButtonDownStyle"), headerSite.Template);
            Assert.AreSame(expander.TryFindResource("ExpanderContentBackground"), contentBorder.Background);
            Assert.AreSame(expander.BorderBrush, contentBorder.BorderBrush);
            Assert.AreEqual(new Thickness(1, 0, 1, 1), contentBorder.BorderThickness);
            Assert.AreEqual(expander.Padding, contentPresenter.Margin);

            headerSite.ApplyTemplate();
            var chevron = FindTemplateChild<TextBlock>(headerSite, "ControlChevronIcon");
            Assert.AreEqual(expander.TryFindResource("ExpanderChevronSize"), chevron.FontSize);
            Assert.AreSame(expander.TryFindResource("SymbolThemeFontFamily"), chevron.FontFamily);
            Assert.AreEqual(expander.TryFindResource("ExpanderChevronDownGlyph"), chevron.Text);
            Assert.AreSame(headerSite.Foreground, chevron.Foreground);

            AssertTemplateTriggerDynamicResource(
                expander.Template,
                UIElement.IsEnabledProperty,
                false,
                null,
                "ContentPresenter",
                TextElement.ForegroundProperty,
                "ExpanderHeaderDisabledForeground");
            AssertTemplateTriggerDynamicResource(
                expander.Template,
                UIElement.IsEnabledProperty,
                false,
                null,
                "HeaderSite",
                Control.ForegroundProperty,
                "ExpanderHeaderDisabledForeground");
            AssertTemplateTriggerDynamicResource(
                expander.Template,
                UIElement.IsEnabledProperty,
                false,
                null,
                "HeaderSite",
                Control.BorderBrushProperty,
                "ExpanderHeaderDisabledBorderBrush");
            AssertTemplateTriggerDynamicResource(
                expander.Template,
                UIElement.IsMouseOverProperty,
                true,
                "HeaderSite",
                "HeaderSite",
                Control.BorderBrushProperty,
                "ExpanderHeaderBorderPointerOverBrush");
        });
    }

    [TestMethod]
    public void VerifyExpanderCollapsedContentFollowsOfficialWpfFluentAnimation()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var expander = new WpfExpander
            {
                Header = "Header",
                Content = "Content",
                IsExpanded = true
            };

            using var host = new TestWindowHost(expander, width: 400, height: 240);
            host.UpdateLayout();

            var contentBorder = FindTemplateChild<Border>(expander, "ContentPresenterBorder");
            Assert.AreEqual(Visibility.Visible, contentBorder.Visibility);

            expander.IsExpanded = false;
            host.UpdateLayout();

            WaitFor(
                () => contentBorder.Visibility == Visibility.Collapsed,
                "Official WPF Fluent Expander collapse animation did not hide the content border.");
        });
    }

    [TestMethod]
    public void VerifyExpanderThemeAliasesRetainOfficialWpfFluentKeys()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            foreach (var themeName in new[] { "Light", "Dark" })
            {
                AssertThemeResourceReference(themeName, "ExpanderHeaderBackground", "CardBackgroundFillColorDefaultBrush");
                AssertThemeResourceReference(themeName, "ExpanderHeaderForeground", "TextFillColorPrimaryBrush");
                AssertThemeResourceReference(themeName, "ExpanderHeaderForegroundPointerOver", "TextFillColorPrimaryBrush");
                AssertThemeResourceReference(themeName, "ExpanderHeaderForegroundPressed", "TextFillColorPrimaryBrush");
                AssertThemeResourceReference(themeName, "ExpanderHeaderBorderBrush", "CardStrokeColorDefaultBrush");
                AssertThemeResourceReference(themeName, "ExpanderHeaderBorderPointerOverBrush", "CardStrokeColorDefaultBrush");
                AssertThemeResourceReference(themeName, "ExpanderHeaderBorderPressedBrush", "CardStrokeColorDefaultBrush");
                AssertThemeResourceReference(themeName, "ExpanderHeaderDisabledForeground", "TextFillColorDisabledBrush");
                AssertThemeResourceReference(themeName, "ExpanderHeaderDisabledBorderBrush", "CardStrokeColorDefaultBrush");
                AssertThemeResourceValue(themeName, "ExpanderHeaderBorderThickness", new Thickness(1));

                AssertThemeResourceReference(themeName, "ExpanderChevronBackground", "SubtleFillColorTransparentBrush");
                AssertThemeResourceReference(themeName, "ExpanderChevronPointerOverBackground", "SubtleFillColorSecondaryBrush");
                AssertThemeResourceReference(themeName, "ExpanderChevronPressedBackground", "SubtleFillColorTertiaryBrush");
                AssertThemeResourceReference(themeName, "ExpanderChevronForeground", "TextFillColorPrimaryBrush");
                AssertThemeResourceReference(themeName, "ExpanderChevronPointerOverForeground", "TextFillColorPrimaryBrush");
                AssertThemeResourceReference(themeName, "ExpanderChevronPressedForeground", "TextFillColorPrimaryBrush");
                AssertThemeResourceReference(themeName, "ExpanderChevronBorderBrush", "SubtleFillColorTransparentBrush");
                AssertThemeResourceReference(themeName, "ExpanderChevronBorderPointerOverBrush", "SubtleFillColorTransparentBrush");
                AssertThemeResourceReference(themeName, "ExpanderChevronBorderPressedBrush", "SubtleFillColorTransparentBrush");
                AssertThemeResourceValue(themeName, "ExpanderChevronBorderThickness", new Thickness(0));

                AssertThemeResourceReference(themeName, "ExpanderContentBackground", "CardBackgroundFillColorSecondaryBrush");
                AssertThemeResourceReference(themeName, "ExpanderContentBorderBrush", "CardStrokeColorDefaultBrush");

                AssertCommonExpanderThemeResourceMetrics(themeName);
            }

            AssertThemeResourceReference("HighContrast", "ExpanderHeaderBackground", "SystemColorButtonFaceColorBrush");
            AssertThemeResourceReference("HighContrast", "ExpanderHeaderForeground", "SystemColorButtonTextColorBrush");
            AssertThemeResourceReference("HighContrast", "ExpanderHeaderForegroundPointerOver", "SystemColorHighlightColorBrush");
            AssertThemeResourceReference("HighContrast", "ExpanderHeaderForegroundPressed", "SystemColorHighlightColorBrush");
            AssertThemeResourceReference("HighContrast", "ExpanderHeaderBorderBrush", "SystemColorWindowTextColorBrush");
            AssertThemeResourceReference("HighContrast", "ExpanderHeaderBorderPointerOverBrush", "SystemColorHighlightColorBrush");
            AssertThemeResourceReference("HighContrast", "ExpanderHeaderBorderPressedBrush", "SystemColorHighlightColorBrush");
            AssertThemeResourceReference("HighContrast", "ExpanderHeaderDisabledForeground", "SystemColorGrayTextColorBrush");
            AssertThemeResourceReference("HighContrast", "ExpanderHeaderDisabledBorderBrush", "SystemColorGrayTextColorBrush");
            AssertThemeResourceValue("HighContrast", "ExpanderHeaderBorderThickness", new Thickness(2));

            AssertThemeResourceReference("HighContrast", "ExpanderChevronBackground", "SystemColorButtonFaceColorBrush");
            AssertThemeResourceReference("HighContrast", "ExpanderChevronPointerOverBackground", "SystemControlTransparentBrush");
            AssertThemeResourceReference("HighContrast", "ExpanderChevronPressedBackground", "SystemControlTransparentBrush");
            AssertThemeResourceReference("HighContrast", "ExpanderChevronForeground", "SystemColorButtonTextColorBrush");
            AssertThemeResourceReference("HighContrast", "ExpanderChevronPointerOverForeground", "SystemColorHighlightColorBrush");
            AssertThemeResourceReference("HighContrast", "ExpanderChevronPressedForeground", "SystemColorHighlightColorBrush");
            AssertThemeResourceReference("HighContrast", "ExpanderChevronBorderBrush", "SystemColorButtonTextColorBrush");
            AssertThemeResourceReference("HighContrast", "ExpanderChevronBorderPointerOverBrush", "SystemColorHighlightColorBrush");
            AssertThemeResourceReference("HighContrast", "ExpanderChevronBorderPressedBrush", "SystemColorHighlightColorBrush");
            AssertThemeResourceValue("HighContrast", "ExpanderChevronBorderThickness", new Thickness(2));

            AssertThemeResourceReference("HighContrast", "ExpanderContentBackground", "SystemColorWindowColorBrush");
            AssertThemeResourceReference("HighContrast", "ExpanderContentBorderBrush", "SystemColorWindowTextColorBrush");

            AssertCommonExpanderThemeResourceMetrics("HighContrast");
        });
    }

    [TestMethod]
    public void ExpanderAutomationPeerTest()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var firstLine = new TextBlock
            {
                Text = "This expander is expanded by default.",
                Margin = new Thickness(0, 0, 0, 4)
            };
            AutomationProperties.SetName(firstLine, "test");

            var secondLine = new TextBlock
            {
                Text = "This is the second line of text."
            };

            var headerText = new StackPanel
            {
                Margin = new Thickness(0, 14, 0, 16)
            };
            headerText.Children.Add(firstLine);
            headerText.Children.Add(secondLine);

            var toggleSwitch = new ToggleSwitch();
            var header = new Grid();
            header.ColumnDefinitions.Add(new ColumnDefinition());
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
            header.Children.Add(headerText);
            Grid.SetColumn(toggleSwitch, 1);
            header.Children.Add(toggleSwitch);

            var contentButton = new Button { Content = "Content" };
            AutomationProperties.SetAutomationId(contentButton, "ExpandedExpanderContent");

            var expander = new WpfExpander
            {
                Header = header,
                Content = contentButton,
                IsExpanded = true,
                Margin = new Thickness(12),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            AutomationProperties.SetName(expander, "ExpandedExpander");

            using var host = new TestWindowHost(expander, width: 500, height: 300);

            Assert.AreEqual("ExpandedExpander", AutomationProperties.GetName(expander));
            Assert.IsTrue(IsContentElement(firstLine));
            Assert.IsTrue(IsContentElement(secondLine));
            Assert.IsTrue(IsControlElement(toggleSwitch));
            Assert.IsTrue(IsControlElement(contentButton));
            Assert.IsTrue(contentButton.IsVisible);

            var peer = FrameworkElementAutomationPeer.CreatePeerForElement(expander);
            Assert.IsNotNull(peer);
            Assert.AreEqual("Expander", peer!.GetClassName());

            expander.IsExpanded = false;
            host.UpdateLayout();

            WaitFor(
                () => !contentButton.IsVisible,
                "Collapsed Expander content should not be visible to UI automation.");
        });
    }

    private static void AssertExpandedDirection(
        ExpandDirection direction,
        Dock expectedContentDock,
        Dock expectedHeaderDock,
        string expectedHeaderTemplateKey,
        Thickness expectedContentBorderThickness,
        CornerRadius expectedContentCornerRadius,
        CornerRadius expectedHeaderCornerRadius)
    {
        var expander = new WpfExpander
        {
            Header = "Header",
            Content = "Content",
            IsExpanded = true,
            ExpandDirection = direction
        };
        expander.SetValue(System.Windows.Controls.Border.CornerRadiusProperty, new CornerRadius(4));

        using var host = new TestWindowHost(expander, width: 400, height: 240);
        host.UpdateLayout();

        var headerSite = FindTemplateChild<ToggleButton>(expander, "HeaderSite");
        var contentGrid = FindTemplateChild<Grid>(expander, "ContentPresenterGrid");
        var toggleButtonBorder = FindTemplateChild<Border>(expander, "ToggleButtonBorder");
        var contentBorder = FindTemplateChild<Border>(expander, "ContentPresenterBorder");

        Assert.AreEqual(expectedContentDock, DockPanel.GetDock(contentGrid), direction.ToString());
        Assert.AreEqual(expectedHeaderDock, DockPanel.GetDock(toggleButtonBorder), direction.ToString());
        Assert.AreSame(expander.TryFindResource(expectedHeaderTemplateKey), headerSite.Template, direction.ToString());
        Assert.AreEqual(expectedContentBorderThickness, contentBorder.BorderThickness, direction.ToString());
        Assert.AreEqual(expectedContentCornerRadius, contentBorder.CornerRadius, direction.ToString());
        Assert.AreEqual(expectedHeaderCornerRadius, toggleButtonBorder.CornerRadius, direction.ToString());
        Assert.AreEqual(Visibility.Visible, contentBorder.Visibility, direction.ToString());
    }

    private static bool IsContentElement(FrameworkElement element)
    {
        return FrameworkElementAutomationPeer.CreatePeerForElement(element)?.IsContentElement() == true;
    }

    private static bool IsControlElement(FrameworkElement element)
    {
        return FrameworkElementAutomationPeer.CreatePeerForElement(element)?.IsControlElement() == true;
    }

    private static Style AssertStyleResource(object key)
    {
        return Application.Current.TryFindResource(key) as Style
            ?? throw new AssertFailedException($"Expected style resource '{key}'.");
    }

    private static void AssertStyleHasSetter(Style style, DependencyProperty property)
    {
        Assert.IsTrue(
            style.Setters.OfType<Setter>().Any(setter => setter.Property == property),
            $"{style.TargetType.Name} should set {property.Name}.");
    }

    private static void AssertSetterValue(Style style, DependencyProperty property, object expectedValue)
    {
        var setter = style.Setters.OfType<Setter>().SingleOrDefault(item => item.Property == property);
        Assert.IsNotNull(setter, $"Expected setter for {property.Name}.");
        Assert.AreEqual(expectedValue, setter!.Value);
    }

    private static void AssertDynamicResourceSetter(Style style, DependencyProperty property, object expectedResourceKey)
    {
        var setter = style.Setters.OfType<Setter>().SingleOrDefault(item => item.Property == property);
        Assert.IsNotNull(setter, $"Expected setter for {property.Name}.");
        AssertDynamicResourceSetter(setter!, expectedResourceKey);
    }

    private static void AssertDynamicResourceSetter(Setter setter, object expectedResourceKey)
    {
        Assert.IsInstanceOfType(setter.Value, typeof(DynamicResourceExtension));

        var dynamicResource = (DynamicResourceExtension)setter.Value;
        Assert.AreEqual(expectedResourceKey, dynamicResource.ResourceKey);
    }

    private static void AssertTemplateTriggerDynamicResource(
        ControlTemplate template,
        DependencyProperty triggerProperty,
        object triggerValue,
        string? sourceName,
        string targetName,
        DependencyProperty setterProperty,
        object expectedResourceKey)
    {
        var trigger = template.Triggers.OfType<Trigger>().Single(item =>
            item.Property == triggerProperty &&
            Equals(item.Value, triggerValue) &&
            item.SourceName == sourceName);
        var setter = trigger.Setters.OfType<Setter>().Single(item =>
            item.TargetName == targetName &&
            item.Property == setterProperty);

        AssertDynamicResourceSetter(setter, expectedResourceKey);
    }

    private static T FindTemplateChild<T>(Control control, string name)
        where T : FrameworkElement
    {
        control.ApplyTemplate();
        return control.Template?.FindName(name, control) as T
            ?? throw new AssertFailedException($"Could not find template child '{name}' on {control.GetType().Name}.");
    }

    private static void WaitFor(Func<bool> predicate, string failureMessage, int timeoutMilliseconds = 1500)
    {
        var stopwatch = Stopwatch.StartNew();
        while (stopwatch.ElapsedMilliseconds < timeoutMilliseconds)
        {
            WpfTestHost.DoEvents();
            if (predicate())
            {
                return;
            }

            Thread.Sleep(10);
        }

        Assert.Fail(failureMessage);
    }

    private static void AssertThemeResourceReference(string themeName, string resourceKey, string expectedResourceKey)
    {
        var themeDictionary = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(themeDictionary.Contains(resourceKey), $"{themeName} is missing {resourceKey}.");
        Assert.IsTrue(themeDictionary.Contains(expectedResourceKey), $"{themeName} is missing {expectedResourceKey}.");
        Assert.AreSame(themeDictionary[expectedResourceKey], themeDictionary[resourceKey], $"{themeName}:{resourceKey}");
    }

    private static void AssertCommonExpanderThemeResourceMetrics(string themeName)
    {
        AssertThemeResourceValue(themeName, "ExpanderMinHeight", 48.0);
        AssertThemeResourceValue(themeName, "ExpanderHeaderHorizontalContentAlignment", HorizontalAlignment.Stretch);
        AssertThemeResourceValue(themeName, "ExpanderHeaderVerticalContentAlignment", VerticalAlignment.Center);
        AssertThemeResourceValue(themeName, "ExpanderHeaderPadding", new Thickness(16, 0, 0, 0));
        AssertThemeResourceValue(themeName, "ExpanderChevronMargin", new Thickness(20, 0, 8, 0));
        AssertThemeResourceValue(themeName, "ExpanderChevronUpGlyph", "\uE70E");
        AssertThemeResourceValue(themeName, "ExpanderChevronDownGlyph", "\uE70D");
        AssertThemeResourceValue(themeName, "ExpanderChevronButtonSize", 32.0);
        AssertThemeResourceValue(themeName, "ExpanderChevronGlyphSize", 12.0);
        AssertThemeResourceValue(themeName, "ExpanderContentPadding", new Thickness(16));
        AssertThemeResourceValue(themeName, "ExpanderContentDownBorderThickness", new Thickness(1, 0, 1, 1));
        AssertThemeResourceValue(themeName, "ExpanderContentUpBorderThickness", new Thickness(1, 1, 1, 0));
    }

    private static void AssertThemeResourceValue(string themeName, string resourceKey, object expectedValue)
    {
        var themeDictionary = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(themeDictionary.Contains(resourceKey), $"{themeName} is missing {resourceKey}.");
        Assert.AreEqual(expectedValue, themeDictionary[resourceKey], $"{themeName}:{resourceKey}");
    }
}
