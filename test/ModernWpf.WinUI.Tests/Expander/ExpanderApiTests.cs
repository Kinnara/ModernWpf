using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf;
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
    public void VerifyExpanderDefaultStyleAndWinUI2Resources()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var expander = new WpfExpander
            {
                Header = "Header",
                Content = new Button { Content = "Content" },
                IsExpanded = true
            };

            using var host = new TestWindowHost(expander, width: 400, height: 240);
            host.UpdateLayout();

            Assert.AreSame(expander.TryFindResource("ExpanderContentBackground"), expander.Background);
            Assert.AreSame(expander.TryFindResource("ExpanderContentBorderBrush"), expander.BorderBrush);
            Assert.AreEqual(expander.TryFindResource("ExpanderContentDownBorderThickness"), expander.BorderThickness);
            Assert.AreEqual(expander.TryFindResource("ExpanderContentPadding"), expander.Padding);
            Assert.AreEqual(HorizontalAlignment.Left, expander.HorizontalAlignment);
            Assert.AreEqual(VerticalAlignment.Center, expander.VerticalAlignment);
            Assert.AreEqual(expander.TryFindResource("FlyoutThemeMinWidth"), expander.MinWidth);
            Assert.AreEqual(expander.TryFindResource("ExpanderMinHeight"), expander.MinHeight);
            Assert.IsFalse(expander.Focusable);

            var header = FindTemplateChild<ToggleButton>(expander, "HeaderSite");
            Assert.IsTrue(header.Focusable);
            Assert.AreSame(expander.TryFindResource("ExpanderHeaderBackground"), header.Background);
            Assert.AreSame(expander.TryFindResource("ExpanderHeaderForeground"), header.Foreground);
            Assert.AreSame(expander.TryFindResource("ExpanderHeaderBorderBrush"), header.BorderBrush);
            Assert.AreEqual(expander.TryFindResource("ExpanderHeaderBorderThickness"), header.BorderThickness);
            Assert.AreEqual(expander.TryFindResource("ExpanderHeaderPadding"), header.Padding);
            Assert.AreEqual(expander.TryFindResource("ExpanderHeaderHorizontalContentAlignment"), header.HorizontalContentAlignment);
            Assert.AreEqual(expander.TryFindResource("ExpanderHeaderVerticalContentAlignment"), header.VerticalContentAlignment);
            Assert.AreEqual(expander.MinHeight, header.MinHeight);
            Assert.IsTrue(FocusVisualHelper.GetUseSystemFocusVisuals(header));
            Assert.AreEqual(new Thickness(-3), FocusVisualHelper.GetFocusVisualMargin(header));

            var content = FindTemplateChild<ContentPresenter>(expander, "ExpandSite");
            Assert.AreEqual(expander.TryFindResource("ExpanderContentPadding"), content.Margin);
            Assert.AreEqual(Visibility.Visible, content.Visibility);

            expander.IsExpanded = false;
            host.UpdateLayout();
            Assert.AreEqual(Visibility.Collapsed, content.Visibility);

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
                AssertThemeResourceReference(themeName, "ExpanderChevronBackground", "SubtleFillColorTransparentBrush");
                AssertThemeResourceReference(themeName, "ExpanderChevronPointerOverBackground", "SubtleFillColorSecondaryBrush");
                AssertThemeResourceReference(themeName, "ExpanderChevronPressedBackground", "SubtleFillColorTertiaryBrush");
                AssertThemeResourceReference(themeName, "ExpanderChevronForeground", "TextFillColorPrimaryBrush");
                AssertThemeResourceReference(themeName, "ExpanderChevronPointerOverForeground", "TextFillColorPrimaryBrush");
                AssertThemeResourceReference(themeName, "ExpanderChevronPressedForeground", "TextFillColorPrimaryBrush");
                AssertThemeResourceReference(themeName, "ExpanderChevronBorderBrush", "SubtleFillColorTransparentBrush");
                AssertThemeResourceReference(themeName, "ExpanderChevronBorderPointerOverBrush", "SubtleFillColorTransparentBrush");
                AssertThemeResourceReference(themeName, "ExpanderChevronBorderPressedBrush", "SubtleFillColorTransparentBrush");
                AssertThemeResourceReference(themeName, "ExpanderContentBackground", "CardBackgroundFillColorSecondaryBrush");
                AssertThemeResourceReference(themeName, "ExpanderContentBorderBrush", "CardStrokeColorDefaultBrush");
                AssertThemeResourceValue(themeName, "ExpanderHeaderBorderThickness", new Thickness(1));
                AssertThemeResourceValue(themeName, "ExpanderChevronBorderThickness", new Thickness(0));
                AssertSharedExpanderResourceValues(themeName);
            }

            AssertThemeResourceReference("HighContrast", "ExpanderHeaderBackground", "SystemColorButtonFaceColorBrush");
            AssertThemeResourceReference("HighContrast", "ExpanderHeaderForeground", "SystemColorButtonTextColorBrush");
            AssertThemeResourceReference("HighContrast", "ExpanderHeaderForegroundPointerOver", "SystemColorHighlightColorBrush");
            AssertThemeResourceReference("HighContrast", "ExpanderHeaderForegroundPressed", "SystemColorHighlightColorBrush");
            AssertThemeResourceReference("HighContrast", "ExpanderHeaderBorderBrush", "SystemColorButtonTextColorBrush");
            AssertThemeResourceReference("HighContrast", "ExpanderHeaderBorderPointerOverBrush", "SystemColorHighlightColorBrush");
            AssertThemeResourceReference("HighContrast", "ExpanderHeaderBorderPressedBrush", "SystemColorHighlightColorBrush");
            AssertThemeResourceReference("HighContrast", "ExpanderHeaderDisabledForeground", "TextFillColorDisabledBrush");
            AssertThemeResourceReference("HighContrast", "ExpanderHeaderDisabledBorderBrush", "TextFillColorDisabledBrush");
            AssertThemeResourceReference("HighContrast", "ExpanderChevronBackground", "SystemColorButtonFaceColorBrush");
            AssertThemeResourceReference("HighContrast", "ExpanderChevronPointerOverBackground", "SystemControlTransparentBrush");
            AssertThemeResourceReference("HighContrast", "ExpanderChevronPressedBackground", "SystemControlTransparentBrush");
            AssertThemeResourceReference("HighContrast", "ExpanderChevronForeground", "SystemColorButtonTextColorBrush");
            AssertThemeResourceReference("HighContrast", "ExpanderChevronPointerOverForeground", "SystemColorHighlightColorBrush");
            AssertThemeResourceReference("HighContrast", "ExpanderChevronPressedForeground", "SystemColorHighlightColorBrush");
            AssertThemeResourceReference("HighContrast", "ExpanderChevronBorderBrush", "SystemColorButtonTextColorBrush");
            AssertThemeResourceReference("HighContrast", "ExpanderChevronBorderPointerOverBrush", "SystemColorHighlightColorBrush");
            AssertThemeResourceReference("HighContrast", "ExpanderChevronBorderPressedBrush", "SystemColorHighlightColorBrush");
            AssertThemeResourceReference("HighContrast", "ExpanderContentBackground", "SystemColorWindowColorBrush");
            AssertThemeResourceReference("HighContrast", "ExpanderContentBorderBrush", "SystemColorWindowTextColorBrush");
            AssertThemeResourceValue("HighContrast", "ExpanderHeaderBorderThickness", new Thickness(2));
            AssertThemeResourceValue("HighContrast", "ExpanderChevronBorderThickness", new Thickness(2));
            AssertSharedExpanderResourceValues("HighContrast");
        });
    }

    [TestMethod]
    public void VerifyExpanderVisualStateSetters()
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

            var expanderBorder = FindTemplateChild<FrameworkElement>(expander, "ExpanderBorder");
            Assert.AreEqual(0, expander.Template.Triggers.Count);
            AssertStateSetter(expanderBorder, "CommonStates", "Disabled", "Foreground");
            AssertStateSetter(expanderBorder, "ExpansionStates", "Expanded", "ExpandSite.Visibility");
            AssertStateSetter(
                expanderBorder,
                "ExpandDirectionStates",
                "ExpandUp",
                "ExpandSite.(DockPanel.Dock)",
                "HeaderSite.(DockPanel.Dock)",
                "HeaderSite.Style",
                "ExpanderBorder.BorderThickness");
            AssertStateSetter(
                expanderBorder,
                "ExpandDirectionStates",
                "ExpandLeft",
                "ExpandSite.(DockPanel.Dock)",
                "HeaderSite.(DockPanel.Dock)",
                "HeaderSite.Style");
            AssertStateSetter(
                expanderBorder,
                "ExpandDirectionStates",
                "ExpandRight",
                "ExpandSite.(DockPanel.Dock)",
                "HeaderSite.(DockPanel.Dock)",
                "HeaderSite.Style");

            var expandSite = FindTemplateChild<ContentPresenterEx>(expander, "ExpandSite");
            var headerSite = FindTemplateChild<ToggleButton>(expander, "HeaderSite");
            AssertHeaderToggleVisualStateSetters(headerSite);

            Assert.AreEqual(Visibility.Visible, expandSite.Visibility);

            expander.IsExpanded = false;
            host.UpdateLayout();
            Assert.AreEqual(Visibility.Collapsed, expandSite.Visibility);

            expander.ExpandDirection = ExpandDirection.Up;
            host.UpdateLayout();
            Assert.AreEqual(Dock.Top, DockPanel.GetDock(expandSite));
            Assert.AreEqual(Dock.Bottom, DockPanel.GetDock(headerSite));
            Assert.AreSame(expander.TryFindResource("ExpanderUpHeaderStyle"), headerSite.Style);
            AssertHeaderToggleVisualStateSetters(headerSite);
            Assert.AreEqual(expander.TryFindResource("ExpanderContentUpBorderThickness"), ((Border)expanderBorder).BorderThickness);

            expander.ExpandDirection = ExpandDirection.Down;
            host.UpdateLayout();
            Assert.AreEqual(Dock.Bottom, DockPanel.GetDock(expandSite));
            Assert.AreEqual(Dock.Top, DockPanel.GetDock(headerSite));
            Assert.AreSame(expander.TryFindResource("ExpanderDownHeaderStyle"), headerSite.Style);
            AssertHeaderToggleVisualStateSetters(headerSite);
            Assert.AreEqual(expander.BorderThickness, ((Border)expanderBorder).BorderThickness);

            expander.ExpandDirection = ExpandDirection.Left;
            host.UpdateLayout();
            Assert.AreSame(expander.TryFindResource("ExpanderLeftHeaderStyle"), headerSite.Style);
            AssertHeaderToggleVisualStateSetters(headerSite);

            expander.ExpandDirection = ExpandDirection.Right;
            host.UpdateLayout();
            Assert.AreSame(expander.TryFindResource("ExpanderRightHeaderStyle"), headerSite.Style);
            AssertHeaderToggleVisualStateSetters(headerSite);
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

            Assert.IsFalse(contentButton.IsVisible, "Collapsed Expander content should not be visible to UI automation.");
        });
    }

    private static bool IsContentElement(FrameworkElement element)
    {
        return FrameworkElementAutomationPeer.CreatePeerForElement(element)?.IsContentElement() == true;
    }

    private static bool IsControlElement(FrameworkElement element)
    {
        return FrameworkElementAutomationPeer.CreatePeerForElement(element)?.IsControlElement() == true;
    }

    private static T FindTemplateChild<T>(Control control, string name)
        where T : FrameworkElement
    {
        return control.Template?.FindName(name, control) as T
            ?? throw new AssertFailedException($"Could not find template child '{name}'.");
    }

    private static void AssertHeaderToggleVisualStateSetters(ToggleButton headerSite)
    {
        Assert.IsTrue(ToggleButtonHelper.GetVisualStateSettersEnabled(headerSite));

        headerSite.ApplyTemplate();
        Assert.AreEqual(0, headerSite.Template.Triggers.Count);

        var headerRoot = FindTemplateChild<FrameworkElement>(headerSite, "HeaderRoot");
        var arrow = FindTemplateChild<FontIconFallback>(headerSite, "arrow");

        AssertStateSetter(
            headerRoot,
            "CommonStates",
            "MouseOver",
            "Foreground",
            "BorderBrush",
            "arrow.Foreground",
            "arrow.(local:AnimatedIcon.State)");
        AssertStateSetter(
            headerRoot,
            "CommonStates",
            "PointerOver",
            "Foreground",
            "BorderBrush",
            "arrow.Foreground",
            "arrow.(local:AnimatedIcon.State)");
        AssertStateSetter(
            headerRoot,
            "CommonStates",
            "Pressed",
            "Foreground",
            "BorderBrush",
            "arrow.Foreground",
            "arrow.(local:AnimatedIcon.State)");
        AssertStateSetter(
            headerRoot,
            "CommonStates",
            "Disabled",
            "Foreground",
            "BorderBrush",
            "arrow.Foreground");
        AssertStateSetter(headerRoot, "CheckStates", "Unchecked", "arrow.Data", "arrow.(local:AnimatedIcon.State)");
        AssertStateSetter(headerRoot, "CheckStates", "Checked", "arrow.Data", "arrow.(local:AnimatedIcon.State)");
        AssertStateSetter(headerRoot, "CheckStates", "CheckedPointerOver", "arrow.Data", "arrow.(local:AnimatedIcon.State)");
        AssertStateSetter(headerRoot, "CheckStates", "CheckedPressed", "arrow.Data", "arrow.(local:AnimatedIcon.State)");
        AssertStateSetter(headerRoot, "CheckStates", "CheckedDisabled", "arrow.Data", "arrow.(local:AnimatedIcon.State)");

        AssertStateSetterValue(headerRoot, "CommonStates", "PointerOver", "arrow.(local:AnimatedIcon.State)", "PointerOverOff");
        AssertStateSetterValue(headerRoot, "CommonStates", "Pressed", "arrow.(local:AnimatedIcon.State)", "PressedOff");
        AssertStateSetterValue(headerRoot, "CheckStates", "Unchecked", "arrow.(local:AnimatedIcon.State)", "NormalOff");
        AssertStateSetterValue(headerRoot, "CheckStates", "Checked", "arrow.(local:AnimatedIcon.State)", "NormalOn");
        AssertStateSetterValue(headerRoot, "CheckStates", "CheckedPointerOver", "arrow.(local:AnimatedIcon.State)", "PointerOverOn");
        AssertStateSetterValue(headerRoot, "CheckStates", "CheckedPressed", "arrow.(local:AnimatedIcon.State)", "PressedOn");
        AssertStateSetterValue(headerRoot, "CheckStates", "CheckedDisabled", "arrow.(local:AnimatedIcon.State)", "NormalOn");

        AssertHeaderCommonStateTransition(
            headerSite,
            arrow,
            "PointerOver",
            "ExpanderHeaderForegroundPointerOver",
            "ExpanderHeaderBorderPointerOverBrush",
            "ExpanderChevronPointerOverForeground");
        AssertHeaderCommonStateTransition(
            headerSite,
            arrow,
            "Pressed",
            "ExpanderHeaderForegroundPressed",
            "ExpanderHeaderBorderPressedBrush",
            "ExpanderChevronPressedForeground");
        AssertHeaderCommonStateTransition(
            headerSite,
            arrow,
            "Disabled",
            "ExpanderHeaderDisabledForeground",
            "ExpanderHeaderDisabledBorderBrush",
            "ExpanderHeaderDisabledForeground");
        AssertHeaderCommonStateTransition(
            headerSite,
            arrow,
            "Normal",
            "ExpanderHeaderForeground",
            "ExpanderHeaderBorderBrush",
            "ExpanderChevronForeground");

        Assert.AreEqual(headerSite.IsChecked == true ? "NormalOn" : "NormalOff", AnimatedIcon.GetState(arrow));
        AssertAnimatedIconStateTransition(headerSite, arrow, "Unchecked", "NormalOff");
        AssertAnimatedIconStateTransition(headerSite, arrow, "PointerOver", "PointerOverOff");
        AssertAnimatedIconStateTransition(headerSite, arrow, "Pressed", "PressedOff");
        AssertAnimatedIconStateTransition(headerSite, arrow, "Checked", "NormalOn");
        AssertAnimatedIconStateTransition(headerSite, arrow, "CheckedPointerOver", "PointerOverOn");
        AssertAnimatedIconStateTransition(headerSite, arrow, "CheckedPressed", "PressedOn");
        AssertAnimatedIconStateTransition(headerSite, arrow, "CheckedDisabled", "NormalOn");
    }

    private static void AssertHeaderCommonStateTransition(
        ToggleButton headerSite,
        FontIconFallback arrow,
        string stateName,
        string foregroundResourceKey,
        string borderResourceKey,
        string arrowForegroundResourceKey)
    {
        Assert.IsTrue(VisualStateManager.GoToState(headerSite, stateName, false), stateName);
        Assert.AreSame(headerSite.TryFindResource(foregroundResourceKey), headerSite.Foreground, $"{stateName}:Foreground");
        Assert.AreSame(headerSite.TryFindResource(borderResourceKey), headerSite.BorderBrush, $"{stateName}:BorderBrush");
        Assert.AreSame(headerSite.TryFindResource(arrowForegroundResourceKey), arrow.Foreground, $"{stateName}:arrow.Foreground");
    }

    private static VisualStateEx AssertStateSetter(
        FrameworkElement stateGroupsRoot,
        string groupName,
        string stateName,
        params string[] expectedTargets)
    {
        var group = VisualStateManager.GetVisualStateGroups(stateGroupsRoot)
            .OfType<VisualStateGroup>()
            .Single(candidate => candidate.Name == groupName);
        var state = group.States
            .OfType<VisualState>()
            .Single(candidate => candidate.Name == stateName);

        Assert.IsInstanceOfType(state, typeof(VisualStateEx));

        var stateEx = (VisualStateEx)state;
        CollectionAssert.AreEquivalent(expectedTargets, stateEx.Setters.Select(setter => setter.Target ?? setter.Property).ToArray());
        return stateEx;
    }

    private static void AssertStateSetterValue(
        FrameworkElement stateGroupsRoot,
        string groupName,
        string stateName,
        string target,
        object expectedValue)
    {
        var group = VisualStateManager.GetVisualStateGroups(stateGroupsRoot)
            .OfType<VisualStateGroup>()
            .Single(candidate => candidate.Name == groupName);
        var state = group.States
            .OfType<VisualState>()
            .Single(candidate => candidate.Name == stateName);

        Assert.IsInstanceOfType(state, typeof(VisualStateEx));

        var stateEx = (VisualStateEx)state;
        var setter = stateEx.Setters.Single(candidate => candidate.Target == target);
        Assert.AreEqual(expectedValue, setter.Value);
    }

    private static void AssertAnimatedIconStateTransition(
        ToggleButton headerSite,
        DependencyObject arrow,
        string stateName,
        string expectedState)
    {
        Assert.IsTrue(VisualStateManager.GoToState(headerSite, stateName, false), stateName);
        Assert.AreEqual(expectedState, AnimatedIcon.GetState(arrow), stateName);
    }

    private static void AssertThemeResourceReference(string themeName, string resourceKey, string expectedResourceKey)
    {
        var themeDictionary = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(themeDictionary.Contains(resourceKey), $"{themeName} is missing {resourceKey}.");
        Assert.IsTrue(themeDictionary.Contains(expectedResourceKey), $"{themeName} is missing {expectedResourceKey}.");
        Assert.AreSame(themeDictionary[expectedResourceKey], themeDictionary[resourceKey], $"{themeName}:{resourceKey}");
    }

    private static void AssertThemeResourceValue<T>(string themeName, string resourceKey, T expectedValue)
    {
        var themeDictionary = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(themeDictionary.Contains(resourceKey), $"{themeName} is missing {resourceKey}.");
        Assert.AreEqual(expectedValue, themeDictionary[resourceKey]);
    }

    private static void AssertSharedExpanderResourceValues(string themeName)
    {
        AssertThemeResourceValue(themeName, "ExpanderMinHeight", 48d);
        AssertThemeResourceValue(themeName, "ExpanderHeaderHorizontalContentAlignment", HorizontalAlignment.Stretch);
        AssertThemeResourceValue(themeName, "ExpanderHeaderVerticalContentAlignment", VerticalAlignment.Center);
        AssertThemeResourceValue(themeName, "ExpanderHeaderPadding", new Thickness(16, 0, 0, 0));
        AssertThemeResourceValue(themeName, "ExpanderChevronMargin", new Thickness(20, 0, 8, 0));
        AssertThemeResourceValue(themeName, "ExpanderChevronUpGlyph", "\uE70E");
        AssertThemeResourceValue(themeName, "ExpanderChevronDownGlyph", "\uE70D");
        AssertThemeResourceValue(themeName, "ExpanderChevronButtonSize", 32d);
        AssertThemeResourceValue(themeName, "ExpanderChevronGlyphSize", 12d);
        AssertThemeResourceValue(themeName, "ExpanderContentPadding", new Thickness(16));
        AssertThemeResourceValue(themeName, "ExpanderContentDownBorderThickness", new Thickness(1, 0, 1, 1));
        AssertThemeResourceValue(themeName, "ExpanderContentUpBorderThickness", new Thickness(1, 1, 1, 0));
    }
}
