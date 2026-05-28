using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
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
            var pivotRoot = FindTemplatePart<FrameworkElement>(pivot, "templateRoot");
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
