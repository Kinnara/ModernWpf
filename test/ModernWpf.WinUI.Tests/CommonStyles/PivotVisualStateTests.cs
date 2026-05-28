using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.CommonStyles;

[TestClass]
public class PivotVisualStateTests
{
    [TestMethod]
    public void DefaultPivotStyleUsesSourceVisualStatesWithoutTemplateTriggers()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var pivot = CreatePivot();
            using var host = new TestWindowHost(pivot, width: 320, height: 180);
            host.UpdateLayout();

            var selectedItem = (TabItem)pivot.Items[0];
            var pivotRoot = FindTemplatePart<Grid>(pivot, "templateRoot");
            var headerRoot = FindTemplatePart<FrameworkElement>(selectedItem, "Border");
            var previousButton = FindTemplatePart<RepeatButton>(pivot, "PreviousButton");
            var nextButton = FindTemplatePart<RepeatButton>(pivot, "NextButton");
            var previousRoot = FindTemplatePart<FrameworkElement>(previousButton, "Root");
            var nextRoot = FindTemplatePart<FrameworkElement>(nextButton, "Root");

            Assert.AreEqual(0, pivot.Template.Triggers.Count);
            Assert.AreEqual(0, selectedItem.Template.Triggers.Count);
            Assert.AreEqual(0, previousButton.Template.Triggers.Count);
            Assert.AreEqual(0, nextButton.Template.Triggers.Count);

            Assert.IsTrue(PivotHelper.GetNavigationButtonsVisualStateSettersEnabled(pivot));
            Assert.IsTrue(PivotHelper.GetHeaderItemVisualStateSettersEnabled(selectedItem));
            Assert.IsTrue(ButtonHelper.GetVisualStateSettersEnabled(previousButton));
            Assert.IsTrue(ButtonHelper.GetVisualStateSettersEnabled(nextButton));

            AssertStateSetters(
                headerRoot,
                "SelectionStates",
                "SelectedPressed",
                "ContentPresenter.Foreground",
                "Border.Background",
                "(Panel.ZIndex)");
            AssertStateSetters(
                headerRoot,
                "SelectionStates",
                "UnselectedPressed",
                "SelectedPipe.Visibility",
                "ContentPresenter.Foreground",
                "Border.Background");

            AssertStateSetters(
                pivotRoot,
                "NavigationButtonsVisibility",
                "NavigationButtonsVisible",
                "PreviousButton.Opacity",
                "PreviousButton.IsEnabled",
                "NextButton.Opacity",
                "NextButton.IsEnabled");
            AssertStateSetters(
                pivotRoot,
                "NavigationButtonsVisibility",
                "PreviousButtonVisible",
                "PreviousButton.Opacity",
                "PreviousButton.IsEnabled",
                "NextButton.Opacity",
                "NextButton.IsEnabled");
            AssertStateSetters(
                pivotRoot,
                "NavigationButtonsVisibility",
                "NextButtonVisible",
                "PreviousButton.Opacity",
                "PreviousButton.IsEnabled",
                "NextButton.Opacity",
                "NextButton.IsEnabled");

            AssertStateSetters(
                previousRoot,
                "CommonStates",
                "PointerOver",
                "Root.Background",
                "Root.BorderBrush",
                "Arrow.Foreground");
            AssertStateSetters(
                nextRoot,
                "CommonStates",
                "Pressed",
                "Root.Background",
                "Root.BorderBrush",
                "Arrow.Foreground");
        });
    }

    [TestMethod]
    public void PivotStylesUseWinUIResourceAliases()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var tabItemStyle = FindStyleResource("TabItemPivotStyle");
            var tabControlStyle = FindStyleResource("TabControlPivotStyle");
            var pivot = CreatePivot(8);
            PivotHelper.SetTitle(pivot, "Inbox");

            using var host = new TestWindowHost(pivot, width: 260, height: 180);
            host.UpdateLayout();

            AssertSetterValue(tabItemStyle, Control.OverridesDefaultStyleProperty, true);
            AssertDynamicResourceSetter(tabItemStyle, Control.BackgroundProperty, "PivotHeaderItemBackgroundUnselected");
            AssertDynamicResourceSetter(tabItemStyle, Control.PaddingProperty, "PivotHeaderItemMargin");
            AssertSetterValue(tabItemStyle, FrameworkElement.HeightProperty, 48.0);
            AssertSetterValue(tabItemStyle, Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch);
            AssertSetterValue(tabItemStyle, Control.VerticalContentAlignmentProperty, VerticalAlignment.Stretch);
            AssertDynamicResourceSetter(tabItemStyle, Control.FocusVisualStyleProperty, SystemParameters.FocusVisualStyleKey);
            AssertDynamicResourceSetter(tabItemStyle, FocusVisualHelper.UseSystemFocusVisualsProperty, "UseSystemFocusVisuals");
            AssertDynamicResourceSetter(tabItemStyle, Border.CornerRadiusProperty, "ControlCornerRadius");
            AssertSetterValue(tabItemStyle, PivotHelper.HeaderItemVisualStateSettersEnabledProperty, true);

            AssertSetterValue(tabControlStyle, Control.OverridesDefaultStyleProperty, true);
            AssertDynamicResourceSetter(tabControlStyle, Control.BackgroundProperty, "PivotBackground");
            AssertDynamicResourceSetter(tabControlStyle, Control.FontFamilyProperty, "ContentControlThemeFontFamily");
            AssertDynamicResourceSetter(tabControlStyle, Control.FontSizeProperty, "ControlContentThemeFontSize");
            AssertSetterValue(tabControlStyle, ItemsControl.ItemContainerStyleProperty, tabItemStyle);
            AssertSetterValue(tabControlStyle, PivotHelper.NavigationButtonsVisualStateSettersEnabledProperty, true);

            Assert.AreSame(pivot.TryFindResource("PivotBackground"), pivot.Background);
            Assert.AreSame(pivot.TryFindResource("ContentControlThemeFontFamily"), pivot.FontFamily);
            Assert.AreEqual(pivot.TryFindResource("ControlContentThemeFontSize"), pivot.FontSize);
            Assert.AreSame(tabItemStyle, pivot.ItemContainerStyle);
            Assert.IsTrue(PivotHelper.GetNavigationButtonsVisualStateSettersEnabled(pivot));

            var selectedItem = (TabItem)pivot.Items[0];
            var unselectedItem = (TabItem)pivot.Items[1];
            var pivotRoot = FindTemplatePart<Grid>(pivot, "templateRoot");
            var titleControl = FindTemplatePart<ContentControl>(pivot, "TitleContentControl");
            var selectedHeaderRoot = FindTemplatePart<Border>(selectedItem, "Border");
            var unselectedHeaderRoot = FindTemplatePart<Border>(unselectedItem, "Border");
            var unselectedPresenter = FindTemplatePart<ContentPresenterEx>(unselectedItem, "ContentPresenter");
            var selectedPipe = FindTemplatePart<Rectangle>(selectedItem, "SelectedPipe");
            var previousButton = FindTemplatePart<RepeatButton>(pivot, "PreviousButton");
            var nextButton = FindTemplatePart<RepeatButton>(pivot, "NextButton");
            var previousRoot = FindTemplatePart<Border>(previousButton, "Root");
            var nextRoot = FindTemplatePart<Border>(nextButton, "Root");
            var previousArrow = FindTemplatePart<FontIconFallback>(previousButton, "Arrow");
            var nextArrow = FindTemplatePart<FontIconFallback>(nextButton, "Arrow");

            Assert.AreSame(pivot.Background, pivotRoot.Background);
            Assert.AreEqual(pivot.TryFindResource("PivotPortraitThemePadding"), titleControl.Margin);
            Assert.AreSame(pivot.TryFindResource("PivotTitleFontFamily"), titleControl.FontFamily);
            Assert.AreEqual(pivot.TryFindResource("PivotTitleThemeFontWeight"), titleControl.FontWeight);
            Assert.AreEqual(pivot.TryFindResource("PivotTitleFontSize"), titleControl.FontSize);

            Assert.AreSame(selectedItem.TryFindResource("PivotHeaderItemBackgroundSelected"), selectedHeaderRoot.Background);
            Assert.AreSame(unselectedItem.TryFindResource("PivotHeaderItemBackgroundUnselected"), unselectedHeaderRoot.Background);
            Assert.AreEqual(unselectedItem.TryFindResource("PivotHeaderItemMargin"), unselectedItem.Padding);
            Assert.AreEqual(48.0, unselectedItem.Height);
            Assert.IsTrue(PivotHelper.GetHeaderItemVisualStateSettersEnabled(unselectedItem));
            Assert.AreSame(unselectedItem.TryFindResource("PivotHeaderItemForegroundUnselected"), unselectedPresenter.Foreground);
            Assert.AreEqual(unselectedItem.TryFindResource("PivotHeaderItemFontSize"), unselectedPresenter.FontSize);
            Assert.AreSame(unselectedItem.TryFindResource("PivotHeaderItemFontFamily"), unselectedPresenter.FontFamily);
            Assert.AreEqual(unselectedItem.TryFindResource("PivotHeaderItemThemeFontWeight"), unselectedPresenter.FontWeight);
            Assert.AreSame(selectedItem.TryFindResource("PivotHeaderItemSelectedPipeFill"), selectedPipe.Fill);

            Assert.AreSame(nextButton.TryFindResource("PivotNextButtonBackground"), nextRoot.Background);
            Assert.AreEqual(nextButton.TryFindResource("PivotNavButtonBorderThemeThickness"), nextRoot.BorderThickness);
            Assert.AreSame(nextButton.TryFindResource("PivotNextButtonBorderBrush"), nextRoot.BorderBrush);
            Assert.AreSame(nextButton.TryFindResource("PivotNextButtonForeground"), nextArrow.Foreground);
            Assert.AreSame(nextButton.TryFindResource("SymbolThemeFontFamily"), nextArrow.FontFamily);
            Assert.AreSame(previousButton.TryFindResource("PivotPreviousButtonBackground"), previousRoot.Background);
            Assert.AreEqual(previousButton.TryFindResource("PivotNavButtonBorderThemeThickness"), previousRoot.BorderThickness);
            Assert.AreSame(previousButton.TryFindResource("PivotPreviousButtonBorderBrush"), previousRoot.BorderBrush);
            Assert.AreSame(previousButton.TryFindResource("PivotPreviousButtonForeground"), previousArrow.Foreground);
            Assert.AreEqual(pivot.TryFindResource("PivotNavButtonMargin"), nextButton.Margin);
            Assert.AreEqual(20.0, nextButton.Width);
            Assert.AreEqual(36.0, nextButton.Height);
            Assert.IsFalse(nextButton.IsTabStop);
            Assert.IsTrue(ButtonHelper.GetVisualStateSettersEnabled(nextButton));

            AssertStateSetterDynamicResource(unselectedHeaderRoot, "SelectionStates", "Disabled", "ContentPresenter.Foreground", "PivotHeaderItemForegroundDisabled");
            AssertStateSetterDynamicResource(unselectedHeaderRoot, "SelectionStates", "Disabled", "Border.Background", "PivotHeaderItemBackgroundDisabled");
            AssertStateSetterDynamicResource(unselectedHeaderRoot, "SelectionStates", "UnselectedPointerOver", "ContentPresenter.Foreground", "PivotHeaderItemForegroundUnselectedPointerOver");
            AssertStateSetterDynamicResource(unselectedHeaderRoot, "SelectionStates", "UnselectedPointerOver", "Border.Background", "PivotHeaderItemBackgroundUnselectedPointerOver");
            AssertStateSetterDynamicResource(unselectedHeaderRoot, "SelectionStates", "Selected", "ContentPresenter.Foreground", "PivotHeaderItemForegroundSelected");
            AssertStateSetterDynamicResource(unselectedHeaderRoot, "SelectionStates", "Selected", "Border.Background", "PivotHeaderItemBackgroundSelected");
            AssertStateSetterDynamicResource(unselectedHeaderRoot, "SelectionStates", "SelectedPressed", "ContentPresenter.Foreground", "PivotHeaderItemForegroundSelectedPressed");
            AssertStateSetterDynamicResource(unselectedHeaderRoot, "SelectionStates", "SelectedPressed", "Border.Background", "PivotHeaderItemBackgroundSelectedPressed");

            AssertStateSetterDynamicResource(nextRoot, "CommonStates", "PointerOver", "Root.Background", "PivotNextButtonBackgroundPointerOver");
            AssertStateSetterDynamicResource(nextRoot, "CommonStates", "PointerOver", "Root.BorderBrush", "PivotNextButtonBorderBrushPointerOver");
            AssertStateSetterDynamicResource(nextRoot, "CommonStates", "PointerOver", "Arrow.Foreground", "PivotNextButtonForegroundPointerOver");
            AssertStateSetterDynamicResource(nextRoot, "CommonStates", "Pressed", "Root.Background", "PivotNextButtonBackgroundPressed");
            AssertStateSetterDynamicResource(nextRoot, "CommonStates", "Pressed", "Root.BorderBrush", "PivotNextButtonBorderBrushPressed");
            AssertStateSetterDynamicResource(nextRoot, "CommonStates", "Pressed", "Arrow.Foreground", "PivotNextButtonForegroundPressed");
            AssertStateSetterDynamicResource(previousRoot, "CommonStates", "PointerOver", "Root.Background", "PivotPreviousButtonBackgroundPointerOver");
            AssertStateSetterDynamicResource(previousRoot, "CommonStates", "PointerOver", "Root.BorderBrush", "PivotPreviousButtonBorderBrushPointerOver");
            AssertStateSetterDynamicResource(previousRoot, "CommonStates", "PointerOver", "Arrow.Foreground", "PivotPreviousButtonForegroundPointerOver");
            AssertStateSetterDynamicResource(previousRoot, "CommonStates", "Pressed", "Root.Background", "PivotPreviousButtonBackgroundPressed");
            AssertStateSetterDynamicResource(previousRoot, "CommonStates", "Pressed", "Root.BorderBrush", "PivotPreviousButtonBorderBrushPressed");
            AssertStateSetterDynamicResource(previousRoot, "CommonStates", "Pressed", "Arrow.Foreground", "PivotPreviousButtonForegroundPressed");
        });
    }

    [TestMethod]
    public void TitleVisibilityFollowsSourceTitleOrTemplate()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var pivot = CreatePivot();
            using var host = new TestWindowHost(pivot, width: 320, height: 180);
            host.UpdateLayout();

            var titleControl = FindTemplatePart<ContentControl>(pivot, "TitleContentControl");
            Assert.AreEqual(Visibility.Collapsed, titleControl.Visibility);

            PivotHelper.SetTitleTemplate(pivot, new DataTemplate());
            host.UpdateLayout();
            Assert.AreEqual(Visibility.Visible, titleControl.Visibility);

            PivotHelper.SetTitleTemplate(pivot, null);
            PivotHelper.SetTitle(pivot, string.Empty);
            host.UpdateLayout();
            Assert.AreEqual(Visibility.Visible, titleControl.Visibility);

            PivotHelper.SetTitle(pivot, null);
            host.UpdateLayout();
            Assert.AreEqual(Visibility.Collapsed, titleControl.Visibility);
        });
    }

    [TestMethod]
    public void HeaderSelectionStatesApplySourceResources()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var pivot = CreatePivot();
            using var host = new TestWindowHost(pivot, width: 320, height: 180);
            host.UpdateLayout();

            var selectedItem = (TabItem)pivot.Items[0];
            var headerRoot = FindTemplatePart<Border>(selectedItem, "Border");
            var presenter = FindTemplatePart<ContentPresenterEx>(selectedItem, "ContentPresenter");

            Assert.AreEqual("Selected", GetCurrentStateName(headerRoot, "SelectionStates"));
            Assert.AreSame(presenter.TryFindResource("PivotHeaderItemForegroundSelected"), presenter.Foreground);
            Assert.AreSame(headerRoot.TryFindResource("PivotHeaderItemBackgroundSelected"), headerRoot.Background);

            Assert.IsTrue(VisualStateManager.GoToState(selectedItem, "SelectedPressed", false));
            Assert.AreSame(presenter.TryFindResource("PivotHeaderItemForegroundSelectedPressed"), presenter.Foreground);
            Assert.AreSame(headerRoot.TryFindResource("PivotHeaderItemBackgroundSelectedPressed"), headerRoot.Background);
            Assert.AreEqual(1, Panel.GetZIndex(selectedItem));
        });
    }

    [TestMethod]
    public void NavigationButtonStatesApplySourceResources()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var pivot = CreatePivot(8);
            using var host = new TestWindowHost(pivot, width: 260, height: 180);
            host.UpdateLayout();

            var previousButton = FindTemplatePart<RepeatButton>(pivot, "PreviousButton");
            var nextButton = FindTemplatePart<RepeatButton>(pivot, "NextButton");
            var nextRoot = FindTemplatePart<Border>(nextButton, "Root");
            var nextArrow = FindTemplatePart<FontIconFallback>(nextButton, "Arrow");

            Assert.IsTrue(VisualStateManager.GoToState(pivot, "NextButtonVisible", false));
            Assert.AreEqual(0d, previousButton.Opacity);
            Assert.IsFalse(previousButton.IsEnabled);
            Assert.AreEqual(1d, nextButton.Opacity);
            Assert.IsTrue(nextButton.IsEnabled);

            Assert.IsTrue(VisualStateManager.GoToState(nextButton, "PointerOver", false));
            Assert.AreSame(nextRoot.TryFindResource("PivotNextButtonBackgroundPointerOver"), nextRoot.Background);
            Assert.AreSame(nextRoot.TryFindResource("PivotNextButtonBorderBrushPointerOver"), nextRoot.BorderBrush);
            Assert.AreSame(nextArrow.TryFindResource("PivotNextButtonForegroundPointerOver"), nextArrow.Foreground);
        });
    }

    [TestMethod]
    public void ThemeResourcesUseWinUI2PivotHighContrastTokens()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            foreach (var themeName in new[] { "Light", "Dark", "HighContrast" })
            {
                AssertThemeResourceValue(themeName, "PivotHeaderItemFontSize", 24.0);
                AssertThemeResourceValue(themeName, "PivotHeaderItemLockedTranslation", 40.0);
                AssertThemeResourceValue(themeName, "PivotTitleFontSize", 14.0);
                AssertThemeResourceValue(themeName, "PivotHeaderItemCharacterSpacing", -25);
                AssertThemeResourceValue(themeName, "PivotHeaderItemMargin", new Thickness(12, 0, 12, 0));
                AssertThemeResourceValue(themeName, "PivotItemMargin", new Thickness(12, 0, 12, 0));
                AssertThemeResourceValue(themeName, "PivotNavButtonMargin", new Thickness(0, 6, 0, 0));
                AssertThemeResourceValue(themeName, "PivotPortraitThemePadding", new Thickness(12, 14, 0, 13));
                AssertThemeResourceValue(themeName, "PivotHeaderItemThemeFontWeight", FontWeight.FromOpenTypeWeight(350));
                AssertThemeResourceValue(themeName, "PivotTitleThemeFontWeight", FontWeights.Bold);

                AssertThemeResourceReference(themeName, "PivotBackground", "SystemControlTransparentBrush");
                AssertThemeResourceReference(themeName, "PivotHeaderBackground", "SystemControlTransparentBrush");
                AssertThemeResourceReference(themeName, "PivotNextButtonBackground", "SystemControlBackgroundBaseMediumLowBrush");
                AssertThemeResourceReference(themeName, "PivotNextButtonBackgroundPointerOver", "SystemControlHighlightBaseMediumBrush");
                AssertThemeResourceReference(themeName, "PivotNextButtonBackgroundPressed", "SystemControlHighlightBaseMediumHighBrush");
                AssertThemeResourceReference(themeName, "PivotNextButtonBorderBrush", "SystemControlForegroundTransparentBrush");
                AssertThemeResourceReference(themeName, "PivotNextButtonBorderBrushPointerOver", "SystemControlForegroundTransparentBrush");
                AssertThemeResourceReference(themeName, "PivotNextButtonBorderBrushPressed", "SystemControlForegroundTransparentBrush");
                AssertThemeResourceReference(themeName, "PivotNextButtonForeground", "SystemControlForegroundAltMediumHighBrush");
                AssertThemeResourceReference(themeName, "PivotNextButtonForegroundPointerOver", "SystemControlHighlightAltAltMediumHighBrush");
                AssertThemeResourceReference(themeName, "PivotNextButtonForegroundPressed", "SystemControlHighlightAltAltMediumHighBrush");
                AssertThemeResourceReference(themeName, "PivotPreviousButtonBackground", "SystemControlBackgroundBaseMediumLowBrush");
                AssertThemeResourceReference(themeName, "PivotPreviousButtonBackgroundPointerOver", "SystemControlHighlightBaseMediumBrush");
                AssertThemeResourceReference(themeName, "PivotPreviousButtonBackgroundPressed", "SystemControlHighlightBaseMediumHighBrush");
                AssertThemeResourceReference(themeName, "PivotPreviousButtonBorderBrush", "SystemControlForegroundTransparentBrush");
                AssertThemeResourceReference(themeName, "PivotPreviousButtonBorderBrushPointerOver", "SystemControlForegroundTransparentBrush");
                AssertThemeResourceReference(themeName, "PivotPreviousButtonBorderBrushPressed", "SystemControlForegroundTransparentBrush");
                AssertThemeResourceReference(themeName, "PivotPreviousButtonForeground", "SystemControlForegroundAltMediumHighBrush");
                AssertThemeResourceReference(themeName, "PivotPreviousButtonForegroundPointerOver", "SystemControlHighlightAltAltMediumHighBrush");
                AssertThemeResourceReference(themeName, "PivotPreviousButtonForegroundPressed", "SystemControlHighlightAltAltMediumHighBrush");
                AssertThemeResourceReference(themeName, "PivotItemBackground", "SystemControlTransparentBrush");
                AssertThemeResourceReference(themeName, "PivotHeaderItemBackgroundUnselected", "SystemControlTransparentBrush");
                AssertThemeResourceReference(themeName, "PivotHeaderItemBackgroundUnselectedPointerOver", "SystemControlHighlightTransparentBrush");
                AssertThemeResourceReference(themeName, "PivotHeaderItemBackgroundUnselectedPressed", "SystemControlHighlightTransparentBrush");
                AssertThemeResourceReference(themeName, "PivotHeaderItemBackgroundSelected", "SystemControlHighlightTransparentBrush");
                AssertThemeResourceReference(themeName, "PivotHeaderItemBackgroundSelectedPointerOver", "SystemControlHighlightTransparentBrush");
                AssertThemeResourceReference(themeName, "PivotHeaderItemBackgroundSelectedPressed", "SystemControlHighlightTransparentBrush");
                AssertThemeResourceReference(themeName, "PivotHeaderItemBackgroundDisabled", "SystemControlTransparentBrush");
                AssertThemeResourceReference(themeName, "PivotHeaderItemForegroundUnselected", "SystemControlForegroundBaseMediumBrush");
                AssertThemeResourceReference(themeName, "PivotHeaderItemForegroundUnselectedPointerOver", "SystemControlHighlightAltBaseMediumHighBrush");
                AssertThemeResourceReference(themeName, "PivotHeaderItemForegroundUnselectedPressed", "SystemControlHighlightAltBaseMediumHighBrush");
                AssertThemeResourceReference(themeName, "PivotHeaderItemForegroundSelected", "SystemControlHighlightAltBaseHighBrush");
                AssertThemeResourceReference(themeName, "PivotHeaderItemForegroundSelectedPointerOver", "SystemControlHighlightAltBaseMediumHighBrush");
                AssertThemeResourceReference(themeName, "PivotHeaderItemForegroundSelectedPressed", "SystemControlHighlightAltBaseMediumHighBrush");
                AssertThemeResourceReference(themeName, "PivotHeaderItemForegroundDisabled", "SystemControlDisabledBaseMediumLowBrush");
                AssertThemeResourceReference(themeName, "PivotHeaderItemFocusPipeFill", "SystemControlHighlightAltAccentBrush");
                AssertThemeResourceReference(themeName, "PivotHeaderItemSelectedPipeFill", "SystemControlHighlightAltAccentBrush");
            }

            AssertThemeResourceValue("Light", "PivotNavButtonBorderThemeThickness", new Thickness(0));
            AssertThemeResourceValue("Dark", "PivotNavButtonBorderThemeThickness", new Thickness(0));
            AssertThemeResourceValue("HighContrast", "PivotNavButtonBorderThemeThickness", new Thickness(1));
        });
    }

    private static TabControl CreatePivot(int itemCount = 2)
    {
        var pivot = new TabControl
        {
            Style = FindStyleResource("TabControlPivotStyle"),
            Width = 260,
            Height = 140
        };

        for (int i = 0; i < itemCount; i++)
        {
            pivot.Items.Add(new TabItem
            {
                Header = "Header " + i,
                Content = "Content " + i
            });
        }

        pivot.SelectedIndex = 0;
        return pivot;
    }

    private static void AssertStateSetters(
        FrameworkElement stateGroupsRoot,
        string groupName,
        string stateName,
        params string[] setterTargets)
    {
        var group = VisualStateManager.GetVisualStateGroups(stateGroupsRoot)
            .OfType<VisualStateGroup>()
            .Single(item => item.Name == groupName);
        var state = group.States
            .Cast<VisualState>()
            .Single(item => item.Name == stateName);

        Assert.IsInstanceOfType(state, typeof(VisualStateEx));

        var stateEx = (VisualStateEx)state;
        foreach (var setterTarget in setterTargets)
        {
            Assert.IsTrue(
                stateEx.Setters.Any(setter => setter.Target == setterTarget || setter.Property == setterTarget),
                $"{groupName}.{stateName} should set {setterTarget}.");
        }
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
        Assert.IsInstanceOfType(setter!.Value, typeof(DynamicResourceExtension));

        var dynamicResource = (DynamicResourceExtension)setter.Value;
        Assert.AreEqual(expectedResourceKey, dynamicResource.ResourceKey);
    }

    private static void AssertStateSetterDynamicResource(
        FrameworkElement stateGroupsRoot,
        string groupName,
        string stateName,
        string target,
        object expectedResourceKey)
    {
        var group = VisualStateManager.GetVisualStateGroups(stateGroupsRoot)
            .OfType<VisualStateGroup>()
            .Single(item => item.Name == groupName);
        var state = group.States
            .OfType<VisualStateEx>()
            .Single(item => item.Name == stateName);
        var setter = state.Setters.Single(item => item.Target == target);

        AssertResourceReferenceExpression(
            setter.ReadLocalValue(VisualStateSetter.ValueProperty),
            expectedResourceKey);
    }

    private static void AssertResourceReferenceExpression(object value, object expectedResourceKey)
    {
        Assert.IsNotNull(value, "Expected dynamic resource local value.");
        Assert.AreEqual("System.Windows.ResourceReferenceExpression", value.GetType().FullName);
        var resourceKeyProperty = value.GetType().GetProperty(
            "ResourceKey",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        Assert.IsNotNull(resourceKeyProperty, "Expected ResourceReferenceExpression.ResourceKey.");
        Assert.AreEqual(expectedResourceKey, resourceKeyProperty!.GetValue(value));
    }

    private static string GetCurrentStateName(FrameworkElement stateGroupsRoot, string groupName)
    {
        var group = VisualStateManager.GetVisualStateGroups(stateGroupsRoot)
            .OfType<VisualStateGroup>()
            .Single(item => item.Name == groupName);
        Assert.IsNotNull(group.CurrentState);
        return group.CurrentState.Name;
    }

    private static void AssertThemeResourceReference(string themeName, object resourceKey, object expectedResourceKey)
    {
        var themeDictionary = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(themeDictionary.Contains(resourceKey), $"{themeName} is missing {resourceKey}.");
        Assert.IsTrue(themeDictionary.Contains(expectedResourceKey), $"{themeName} is missing {expectedResourceKey}.");
        Assert.AreSame(themeDictionary[expectedResourceKey], themeDictionary[resourceKey], $"{themeName}:{resourceKey}");
    }

    private static void AssertThemeResourceValue<T>(string themeName, object resourceKey, T expectedValue)
    {
        var themeDictionary = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(themeDictionary.Contains(resourceKey), $"{themeName} is missing {resourceKey}.");
        Assert.AreEqual(expectedValue, themeDictionary[resourceKey]);
    }

    private static T FindTemplatePart<T>(Control control, string name)
        where T : FrameworkElement
    {
        control.ApplyTemplate();
        return control.Template?.FindName(name, control) as T
            ?? throw new AssertFailedException($"Expected template part '{name}' on {control.GetType().Name}.");
    }

    private static Style FindStyleResource(string resourceId)
    {
        return Application.Current.TryFindResource(resourceId) as Style
            ?? throw new AssertFailedException($"Expected style resource '{resourceId}'.");
    }
}
