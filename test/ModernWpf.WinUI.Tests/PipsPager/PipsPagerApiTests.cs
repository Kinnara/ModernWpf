using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf;
using ModernWpf.Controls;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.PipsPager;

[TestClass]
public class PipsPagerApiTests
{
    [TestMethod]
    public void VerifyDefaultPropertyValues()
    {
        WpfTestHost.Run(() =>
        {
            var pipsPager = new ModernWpf.Controls.PipsPager();

            Assert.AreEqual(-1, pipsPager.NumberOfPages);
            Assert.AreEqual(0, pipsPager.SelectedPageIndex);
            Assert.AreEqual(5, pipsPager.MaxVisiblePips);
            Assert.AreEqual(Orientation.Horizontal, pipsPager.Orientation);
            Assert.AreEqual(PipsPagerButtonVisibility.Collapsed, pipsPager.PreviousButtonVisibility);
            Assert.AreEqual(PipsPagerButtonVisibility.Collapsed, pipsPager.NextButtonVisibility);
            Assert.IsNull(pipsPager.PreviousButtonStyle);
            Assert.IsNull(pipsPager.NextButtonStyle);
            Assert.IsNull(pipsPager.SelectedPipStyle);
            Assert.IsNull(pipsPager.NormalPipStyle);
            Assert.AreEqual(PipsPagerWrapMode.None, pipsPager.WrapMode);
            Assert.IsNotNull(pipsPager.TemplateSettings);
        });
    }

    [TestMethod]
    public void VerifyPropertyGettersAndSetters()
    {
        WpfTestHost.Run(() =>
        {
            var previousButtonStyle = new Style(typeof(Button));
            var nextButtonStyle = new Style(typeof(Button));
            var selectedPipStyle = new Style(typeof(Button));
            var normalPipStyle = new Style(typeof(Button));
            var pipsPager = new ModernWpf.Controls.PipsPager
            {
                NumberOfPages = 10,
                SelectedPageIndex = 4,
                MaxVisiblePips = 3,
                Orientation = Orientation.Vertical,
                PreviousButtonVisibility = PipsPagerButtonVisibility.Visible,
                NextButtonVisibility = PipsPagerButtonVisibility.VisibleOnPointerOver,
                PreviousButtonStyle = previousButtonStyle,
                NextButtonStyle = nextButtonStyle,
                SelectedPipStyle = selectedPipStyle,
                NormalPipStyle = normalPipStyle,
                WrapMode = PipsPagerWrapMode.Wrap
            };

            Assert.AreEqual(10, pipsPager.NumberOfPages);
            Assert.AreEqual(4, pipsPager.SelectedPageIndex);
            Assert.AreEqual(3, pipsPager.MaxVisiblePips);
            Assert.AreEqual(Orientation.Vertical, pipsPager.Orientation);
            Assert.AreEqual(PipsPagerButtonVisibility.Visible, pipsPager.PreviousButtonVisibility);
            Assert.AreEqual(PipsPagerButtonVisibility.VisibleOnPointerOver, pipsPager.NextButtonVisibility);
            Assert.AreSame(previousButtonStyle, pipsPager.PreviousButtonStyle);
            Assert.AreSame(nextButtonStyle, pipsPager.NextButtonStyle);
            Assert.AreSame(selectedPipStyle, pipsPager.SelectedPipStyle);
            Assert.AreSame(normalPipStyle, pipsPager.NormalPipStyle);
            Assert.AreEqual(PipsPagerWrapMode.Wrap, pipsPager.WrapMode);
        });
    }

    [TestMethod]
    public void VerifyAutomationPeerBehavior()
    {
        WpfTestHost.Run(() =>
        {
            var pipsPager = new ModernWpf.Controls.PipsPager
            {
                NumberOfPages = 5
            };

            using var host = new TestWindowHost(pipsPager, width: 300, height: 120);

            var peer = FrameworkElementAutomationPeer.CreatePeerForElement(pipsPager);
            Assert.IsInstanceOfType(peer, typeof(ISelectionProvider));
            Assert.AreEqual(AutomationControlType.Menu, peer.GetAutomationControlType());
            Assert.AreEqual("Pager", peer.GetName());
            var selectionPeer = (ISelectionProvider)peer;

            Assert.IsFalse(selectionPeer.CanSelectMultiple);
            Assert.IsTrue(selectionPeer.IsSelectionRequired);
            Assert.AreEqual(1, selectionPeer.GetSelection().Length);
        });
    }

    [TestMethod]
    public void VerifyPipsPagerButtonUIABehavior()
    {
        WpfTestHost.Run(() =>
        {
            var pipsPager = new ModernWpf.Controls.PipsPager
            {
                NumberOfPages = 5
            };

            using var host = new TestWindowHost(pipsPager, width: 300, height: 120);

            var buttons = GetPipButtons(pipsPager);
            Assert.AreEqual(5, buttons.Count);

            for (var i = 0; i < buttons.Count; i++)
            {
                Assert.AreEqual($"Page {i + 1}", AutomationProperties.GetName(buttons[i]));
                Assert.AreEqual(i + 1, buttons[i].GetValue(AutomationProperties.PositionInSetProperty));
                Assert.AreEqual(5, buttons[i].GetValue(AutomationProperties.SizeOfSetProperty));
            }
        });
    }

    [TestMethod]
    public void VerifyEmptyPagerDoesNotCrash()
    {
        WpfTestHost.Run(() =>
        {
            var pipsPager = new ModernWpf.Controls.PipsPager
            {
                NumberOfPages = 0
            };

            using var host = new TestWindowHost(pipsPager, width: 300, height: 120);

            Assert.IsNotNull(pipsPager);
            Assert.AreEqual(0, pipsPager.TemplateSettings.PipsPagerItems.Count);
            Assert.AreEqual(0, GetPipButtons(pipsPager).Count);
        });
    }

    [TestMethod]
    public void VerifySelectedIndexChangedEventArgs()
    {
        WpfTestHost.Run(() =>
        {
            var pipsPager = new ModernWpf.Controls.PipsPager();
            var newIndex = -2;
            pipsPager.SelectedIndexChanged += (sender, args) => newIndex = sender.SelectedPageIndex;

            using var host = new TestWindowHost(pipsPager, width: 300, height: 120);

            Assert.AreEqual(0, newIndex);

            pipsPager.NumberOfPages = 10;
            Assert.AreEqual(0, newIndex);

            pipsPager.SelectedPageIndex = 9;
            Assert.AreEqual(9, newIndex);

            pipsPager.SelectedPageIndex = 4;
            Assert.AreEqual(4, newIndex);
        });
    }

    [TestMethod]
    public void PipItemsFollowWinUISourceCollectionShape()
    {
        WpfTestHost.Run(() =>
        {
            var pipsPager = new ModernWpf.Controls.PipsPager
            {
                NumberOfPages = 10,
                MaxVisiblePips = 5,
                SelectedPageIndex = 8
            };

            using var host = new TestWindowHost(pipsPager, width: 300, height: 120);

            CollectionAssert.AreEqual(
                Enumerable.Range(1, 10).ToArray(),
                pipsPager.TemplateSettings.PipsPagerItems.ToArray());

            pipsPager.SelectedPageIndex = 1;

            CollectionAssert.AreEqual(
                Enumerable.Range(1, 10).ToArray(),
                pipsPager.TemplateSettings.PipsPagerItems.ToArray());

            var scrollViewer = FindNamedDescendant<ScrollViewer>(pipsPager, "PipsPagerScrollViewer");
            Assert.AreEqual(60.0, scrollViewer.MaxWidth);

            pipsPager.NumberOfPages = -1;
            pipsPager.SelectedPageIndex = 10;
            host.UpdateLayout();

            CollectionAssert.AreEqual(
                Enumerable.Range(1, 11).ToArray(),
                pipsPager.TemplateSettings.PipsPagerItems.ToArray());
        });
    }

    [TestMethod]
    public void PipsAndNavigationButtonsChangePage()
    {
        WpfTestHost.Run(() =>
        {
            var pipsPager = new ModernWpf.Controls.PipsPager
            {
                NumberOfPages = 3,
                PreviousButtonVisibility = PipsPagerButtonVisibility.Visible,
                NextButtonVisibility = PipsPagerButtonVisibility.Visible
            };

            using var host = new TestWindowHost(pipsPager, width: 300, height: 120);

            var nextButton = GetNamedButton(pipsPager, "Next Page");
            var previousButton = GetNamedButton(pipsPager, "Previous Page");

            Assert.IsFalse(previousButton.IsEnabled);
            Assert.IsTrue(nextButton.IsEnabled);

            nextButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            host.UpdateLayout();

            Assert.AreEqual(1, pipsPager.SelectedPageIndex);
            Assert.IsTrue(previousButton.IsEnabled);

            pipsPager.ContainerFromIndex(2).RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            host.UpdateLayout();

            Assert.AreEqual(2, pipsPager.SelectedPageIndex);
            Assert.IsFalse(nextButton.IsEnabled);
        });
    }

    [TestMethod]
    public void NavigationButtonsWrapWhenWrapModeIsEnabled()
    {
        WpfTestHost.Run(() =>
        {
            var pipsPager = new ModernWpf.Controls.PipsPager
            {
                NumberOfPages = 3,
                PreviousButtonVisibility = PipsPagerButtonVisibility.Visible,
                NextButtonVisibility = PipsPagerButtonVisibility.Visible,
                WrapMode = PipsPagerWrapMode.Wrap
            };

            using var host = new TestWindowHost(pipsPager, width: 300, height: 120);

            var previousButton = GetNamedButton(pipsPager, "Previous Page");
            var nextButton = GetNamedButton(pipsPager, "Next Page");

            Assert.IsTrue(previousButton.IsEnabled);
            previousButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            host.UpdateLayout();

            Assert.AreEqual(2, pipsPager.SelectedPageIndex);

            nextButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            host.UpdateLayout();

            Assert.AreEqual(0, pipsPager.SelectedPageIndex);

            var repeater = FindNamedDescendant<ItemsRepeater>(pipsPager, "PipsPagerItemsRepeater");
            Assert.IsInstanceOfType(repeater.Layout, typeof(StackLayout));
            Assert.IsFalse(((StackLayout)repeater.Layout).IsVirtualizationEnabled);
        });
    }

    [TestMethod]
    public void NavigationButtonStatesUseVisualStateSetters()
    {
        WpfTestHost.Run(() =>
        {
            var pipsPager = new ModernWpf.Controls.PipsPager
            {
                NumberOfPages = 3,
                PreviousButtonVisibility = PipsPagerButtonVisibility.Visible,
                NextButtonVisibility = PipsPagerButtonVisibility.Visible
            };

            using var host = new TestWindowHost(pipsPager, width: 300, height: 120);
            host.UpdateLayout();

            var rootPanel = FindNamedDescendant<StackPanel>(pipsPager, "RootPanel");
            var previousButton = FindNamedDescendant<Button>(pipsPager, "PreviousPageButton");
            var nextButton = FindNamedDescendant<Button>(pipsPager, "NextPageButton");

            AssertStateSetter(rootPanel, "PreviousPageButtonVisibilityStates", "PreviousPageButtonHidden", "PreviousPageButton.Opacity");
            AssertStateSetter(rootPanel, "PreviousPageButtonVisibilityStates", "PreviousPageButtonCollapsed", "PreviousPageButton.Visibility");
            AssertStateSetter(rootPanel, "PreviousPageButtonIsEnabledStates", "PreviousPageButtonDisabled", "PreviousPageButton.IsEnabled");
            AssertStateSetter(rootPanel, "NextPageButtonVisibilityStates", "NextPageButtonHidden", "NextPageButton.Opacity");
            AssertStateSetter(rootPanel, "NextPageButtonVisibilityStates", "NextPageButtonCollapsed", "NextPageButton.Visibility");
            AssertStateSetter(rootPanel, "NextPageButtonIsEnabledStates", "NextPageButtonDisabled", "NextPageButton.IsEnabled");

            Assert.AreEqual("PreviousPageButtonHidden", GetCurrentStateName(rootPanel, "PreviousPageButtonVisibilityStates"));
            Assert.AreEqual("PreviousPageButtonDisabled", GetCurrentStateName(rootPanel, "PreviousPageButtonIsEnabledStates"));
            Assert.AreEqual(0, previousButton.Opacity);
            Assert.IsFalse(previousButton.IsEnabled);
            Assert.AreEqual(Visibility.Visible, previousButton.Visibility);

            Assert.AreEqual("NextPageButtonVisible", GetCurrentStateName(rootPanel, "NextPageButtonVisibilityStates"));
            Assert.AreEqual("NextPageButtonEnabled", GetCurrentStateName(rootPanel, "NextPageButtonIsEnabledStates"));
            Assert.AreEqual(1, nextButton.Opacity);
            Assert.IsTrue(nextButton.IsEnabled);

            pipsPager.SelectedPageIndex = 1;
            host.UpdateLayout();

            Assert.AreEqual("PreviousPageButtonVisible", GetCurrentStateName(rootPanel, "PreviousPageButtonVisibilityStates"));
            Assert.AreEqual("PreviousPageButtonEnabled", GetCurrentStateName(rootPanel, "PreviousPageButtonIsEnabledStates"));
            Assert.AreEqual(1, previousButton.Opacity);
            Assert.IsTrue(previousButton.IsEnabled);

            pipsPager.PreviousButtonVisibility = PipsPagerButtonVisibility.Collapsed;
            host.UpdateLayout();

            Assert.AreEqual("PreviousPageButtonCollapsed", GetCurrentStateName(rootPanel, "PreviousPageButtonVisibilityStates"));
            Assert.AreEqual("PreviousPageButtonDisabled", GetCurrentStateName(rootPanel, "PreviousPageButtonIsEnabledStates"));
            Assert.AreEqual(Visibility.Collapsed, previousButton.Visibility);
            Assert.IsFalse(previousButton.IsEnabled);
        });
    }

    [TestMethod]
    public void OrientationStatesUseVisualStateSetters()
    {
        WpfTestHost.Run(() =>
        {
            var pipsPager = new ModernWpf.Controls.PipsPager
            {
                NumberOfPages = 3,
                PreviousButtonVisibility = PipsPagerButtonVisibility.Visible,
                NextButtonVisibility = PipsPagerButtonVisibility.Visible
            };

            using var host = new TestWindowHost(pipsPager, width: 300, height: 120);
            host.UpdateLayout();

            var rootPanel = FindNamedDescendant<StackPanel>(pipsPager, "RootPanel");
            var repeater = FindNamedDescendant<ItemsRepeater>(pipsPager, "PipsPagerItemsRepeater");
            var previousButton = FindNamedDescendant<Button>(pipsPager, "PreviousPageButton");
            var nextButton = FindNamedDescendant<Button>(pipsPager, "NextPageButton");
            var orientationState = AssertStateSetter(
                rootPanel,
                "RootPanelOrientationStates",
                "HorizontalOrientationView",
                "RootPanel.Orientation");

            Assert.AreEqual(7, orientationState.Setters.Count);
            Assert.AreEqual("HorizontalOrientationView", GetCurrentStateName(rootPanel, "RootPanelOrientationStates"));
            Assert.AreEqual(Orientation.Horizontal, rootPanel.Orientation);
            Assert.AreEqual(Orientation.Horizontal, ((StackLayout)repeater.Layout).Orientation);
            Assert.AreEqual(PlacementMode.Left, ToolTipService.GetPlacement(previousButton));
            Assert.AreEqual(PlacementMode.Right, ToolTipService.GetPlacement(nextButton));
            AssertRotateTransform(previousButton.RenderTransform);
            AssertRotateTransform(nextButton.RenderTransform);

            pipsPager.Orientation = Orientation.Vertical;
            host.UpdateLayout();

            Assert.AreEqual("VerticalOrientationView", GetCurrentStateName(rootPanel, "RootPanelOrientationStates"));
            Assert.AreEqual(Orientation.Vertical, rootPanel.Orientation);
            Assert.AreEqual(Orientation.Vertical, ((StackLayout)repeater.Layout).Orientation);
            Assert.AreEqual(PlacementMode.Top, ToolTipService.GetPlacement(previousButton));
            Assert.AreEqual(PlacementMode.Bottom, ToolTipService.GetPlacement(nextButton));
            Assert.IsFalse(previousButton.RenderTransform is RotateTransform);
            Assert.IsFalse(nextButton.RenderTransform is RotateTransform);
        });
    }

    [TestMethod]
    public void DefaultPipButtonOrientationUsesVisualStateSetters()
    {
        WpfTestHost.Run(() =>
        {
            var pipsPager = new ModernWpf.Controls.PipsPager
            {
                NumberOfPages = 3
            };

            using var host = new TestWindowHost(pipsPager, width: 300, height: 120);
            host.UpdateLayout();

            var pipButton = pipsPager.ContainerFromIndex(0);
            var rootGrid = FindNamedDescendant<GridEx>(pipButton, "RootGrid");

            AssertStateSetter(rootGrid, "OrientationStates", "VerticalOrientation", "RootGrid.Width");
            AssertStateSetter(rootGrid, "OrientationStates", "VerticalOrientation", "RootGrid.Height");
            Assert.AreEqual("HorizontalOrientation", GetCurrentStateName(rootGrid, "OrientationStates"));
            Assert.AreEqual(12.0, rootGrid.Width);
            Assert.AreEqual(24.0, rootGrid.Height);

            pipsPager.Orientation = Orientation.Vertical;
            host.UpdateLayout();

            pipButton = pipsPager.ContainerFromIndex(0);
            rootGrid = FindNamedDescendant<GridEx>(pipButton, "RootGrid");

            Assert.AreEqual("VerticalOrientation", GetCurrentStateName(rootGrid, "OrientationStates"));
            Assert.AreEqual(24.0, rootGrid.Width);
            Assert.AreEqual(12.0, rootGrid.Height);
        });
    }

    [TestMethod]
    public void PipsPagerStylesUseWinUIResourceAliases()
    {
        WpfTestHost.Run(() =>
        {
            var resources = new ResourceDictionary
            {
                Source = new System.Uri("/ModernWpf.Controls;component/PipsPager/PipsPager.xaml", System.UriKind.Relative)
            };
            var pipsPagerStyle = (Style)resources[typeof(ModernWpf.Controls.PipsPager)];
            var navigationButtonBaseStyle = (Style)resources["PipsPagerNavigationButtonBaseStyle"];
            var previousPageButtonStyle = (Style)resources["PipsPagerPreviousPageButtonStyle"];
            var nextPageButtonStyle = (Style)resources["PipsPagerNextPageButtonStyle"];
            var pipButtonBaseStyle = (Style)resources["PipsPagerButtonBaseStyle"];
            var selectedPipButtonStyle = (Style)resources["PipsPagerSelectedPipButtonStyle"];
            var normalPipButtonStyle = (Style)resources["PipsPagerNormalPipButtonStyle"];
            var pipsPager = new ModernWpf.Controls.PipsPager
            {
                NumberOfPages = 3,
                PreviousButtonVisibility = PipsPagerButtonVisibility.Visible,
                NextButtonVisibility = PipsPagerButtonVisibility.Visible,
                Style = pipsPagerStyle
            };
            pipsPager.Resources.MergedDictionaries.Add(resources);

            using var host = new TestWindowHost(pipsPager, width: 300, height: 120);
            host.UpdateLayout();

            AssertDynamicResourceSetter(navigationButtonBaseStyle, Control.BackgroundProperty, "PipsPagerNavigationButtonBackground");
            AssertDynamicResourceSetter(navigationButtonBaseStyle, Control.ForegroundProperty, "PipsPagerNavigationButtonForeground");
            AssertDynamicResourceSetter(navigationButtonBaseStyle, Control.BorderBrushProperty, "PipsPagerNavigationButtonBorderBrush");
            AssertDynamicResourceSetter(navigationButtonBaseStyle, Control.FontFamilyProperty, "SymbolThemeFontFamily");
            AssertDynamicResourceSetter(navigationButtonBaseStyle, Control.FocusVisualStyleProperty, SystemParameters.FocusVisualStyleKey);
            AssertDynamicResourceSetter(navigationButtonBaseStyle, ModernWpf.Controls.Primitives.FocusVisualHelper.UseSystemFocusVisualsProperty, "UseSystemFocusVisuals");
            AssertDynamicResourceSetter(navigationButtonBaseStyle, Border.CornerRadiusProperty, "ControlCornerRadius");
            AssertSetterValue(navigationButtonBaseStyle, Control.BorderThicknessProperty, new Thickness(1));
            AssertSetterValue(navigationButtonBaseStyle, Control.FontSizeProperty, 8.0);
            AssertSetterValue(navigationButtonBaseStyle, FrameworkElement.WidthProperty, 24.0);
            AssertSetterValue(navigationButtonBaseStyle, FrameworkElement.HeightProperty, 24.0);

            Assert.AreSame(navigationButtonBaseStyle, previousPageButtonStyle.BasedOn);
            Assert.AreSame(navigationButtonBaseStyle, nextPageButtonStyle.BasedOn);
            AssertSetterValue(previousPageButtonStyle, ContentControl.ContentProperty, "\uEDDB");
            AssertSetterValue(nextPageButtonStyle, ContentControl.ContentProperty, "\uEDDC");

            AssertDynamicResourceSetter(pipButtonBaseStyle, Control.BackgroundProperty, "PipsPagerSelectionIndicatorBackground");
            AssertDynamicResourceSetter(pipButtonBaseStyle, Control.ForegroundProperty, "PipsPagerSelectionIndicatorForeground");
            AssertDynamicResourceSetter(pipButtonBaseStyle, Control.BorderBrushProperty, "PipsPagerSelectionIndicatorBorderBrush");
            AssertDynamicResourceSetter(pipButtonBaseStyle, Control.FontFamilyProperty, "SymbolThemeFontFamily");
            AssertDynamicResourceSetter(pipButtonBaseStyle, Control.FocusVisualStyleProperty, SystemParameters.FocusVisualStyleKey);
            AssertDynamicResourceSetter(pipButtonBaseStyle, ModernWpf.Controls.Primitives.FocusVisualHelper.UseSystemFocusVisualsProperty, "UseSystemFocusVisuals");
            AssertDynamicResourceSetter(pipButtonBaseStyle, Border.CornerRadiusProperty, "ControlCornerRadius");
            AssertSetterValue(pipButtonBaseStyle, Control.BorderThicknessProperty, new Thickness(1));
            AssertSetterValue(pipButtonBaseStyle, ContentControl.ContentProperty, "\uEA3B");
            AssertSetterValue(pipButtonBaseStyle, Control.FontSizeProperty, 4.0);

            Assert.AreSame(pipButtonBaseStyle, selectedPipButtonStyle.BasedOn);
            AssertDynamicResourceSetter(selectedPipButtonStyle, Control.BackgroundProperty, "PipsPagerSelectionIndicatorBackgroundSelected");
            AssertDynamicResourceSetter(selectedPipButtonStyle, Control.BorderBrushProperty, "PipsPagerSelectionIndicatorBorderBrushSelected");
            AssertDynamicResourceSetter(selectedPipButtonStyle, Control.ForegroundProperty, "PipsPagerSelectionIndicatorForegroundSelected");
            AssertSetterValue(selectedPipButtonStyle, ContentControl.ContentProperty, "\uEA3B");
            AssertSetterValue(selectedPipButtonStyle, Control.FontSizeProperty, 6.0);

            Assert.AreSame(pipButtonBaseStyle, normalPipButtonStyle.BasedOn);
            AssertSetterValue(normalPipButtonStyle, ContentControl.ContentProperty, "\uEA3B");
            AssertSetterValue(normalPipButtonStyle, Control.FontSizeProperty, 4.0);

            AssertSetterValue(pipsPagerStyle, Control.IsTabStopProperty, false);
            AssertSetterValue(pipsPagerStyle, ModernWpf.Controls.PipsPager.PreviousButtonStyleProperty, previousPageButtonStyle);
            AssertSetterValue(pipsPagerStyle, ModernWpf.Controls.PipsPager.NextButtonStyleProperty, nextPageButtonStyle);
            AssertSetterValue(pipsPagerStyle, ModernWpf.Controls.PipsPager.SelectedPipStyleProperty, selectedPipButtonStyle);
            AssertSetterValue(pipsPagerStyle, ModernWpf.Controls.PipsPager.NormalPipStyleProperty, normalPipButtonStyle);
            Assert.AreSame(previousPageButtonStyle, pipsPager.PreviousButtonStyle);
            Assert.AreSame(nextPageButtonStyle, pipsPager.NextButtonStyle);
            Assert.AreSame(selectedPipButtonStyle, pipsPager.SelectedPipStyle);
            Assert.AreSame(normalPipButtonStyle, pipsPager.NormalPipStyle);
            Assert.IsFalse(pipsPager.IsTabStop);

            var previousButton = FindNamedDescendant<Button>(pipsPager, "PreviousPageButton");
            var nextButton = FindNamedDescendant<Button>(pipsPager, "NextPageButton");
            Assert.AreSame(pipsPager.TryFindResource("PipsPagerNavigationButtonBackground"), previousButton.Background);
            Assert.AreSame(pipsPager.TryFindResource("PipsPagerNavigationButtonForeground"), previousButton.Foreground);
            Assert.AreSame(pipsPager.TryFindResource("PipsPagerNavigationButtonBorderBrush"), previousButton.BorderBrush);
            Assert.AreEqual(new Thickness(1), previousButton.BorderThickness);
            Assert.AreEqual(24.0, previousButton.Width);
            Assert.AreEqual(24.0, previousButton.Height);
            Assert.AreEqual("\uEDDB", previousButton.Content);
            Assert.AreEqual("\uEDDC", nextButton.Content);

            var selectedPipButton = pipsPager.ContainerFromIndex(0);
            var normalPipButton = pipsPager.ContainerFromIndex(1);
            Assert.AreSame(pipsPager.TryFindResource("PipsPagerSelectionIndicatorBackgroundSelected"), selectedPipButton.Background);
            Assert.AreSame(pipsPager.TryFindResource("PipsPagerSelectionIndicatorForegroundSelected"), selectedPipButton.Foreground);
            Assert.AreSame(pipsPager.TryFindResource("PipsPagerSelectionIndicatorBorderBrushSelected"), selectedPipButton.BorderBrush);
            Assert.AreEqual(6.0, selectedPipButton.FontSize);
            Assert.AreSame(pipsPager.TryFindResource("PipsPagerSelectionIndicatorBackground"), normalPipButton.Background);
            Assert.AreSame(pipsPager.TryFindResource("PipsPagerSelectionIndicatorForeground"), normalPipButton.Foreground);
            Assert.AreSame(pipsPager.TryFindResource("PipsPagerSelectionIndicatorBorderBrush"), normalPipButton.BorderBrush);
            Assert.AreEqual(4.0, normalPipButton.FontSize);

            var navigationRoot = FindNamedDescendant<GridEx>(previousButton, "RootGrid");
            AssertStateSetterDynamicResource(navigationRoot, "CommonStates", "PointerOver", "RootGrid.Background", "PipsPagerNavigationButtonBackgroundPointerOver");
            AssertStateSetterDynamicResource(navigationRoot, "CommonStates", "PointerOver", "RootGrid.BorderBrush", "PipsPagerNavigationButtonBorderBrushPointerOver");
            AssertStateSetterDynamicResource(navigationRoot, "CommonStates", "PointerOver", "Content.Foreground", "PipsPagerNavigationButtonForegroundPointerOver");
            AssertStateSetterDynamicResource(navigationRoot, "CommonStates", "Pressed", "RootGrid.Background", "PipsPagerNavigationButtonBackgroundPressed");
            AssertStateSetterDynamicResource(navigationRoot, "CommonStates", "Pressed", "RootGrid.BorderBrush", "PipsPagerNavigationButtonBorderBrushPressed");
            AssertStateSetterDynamicResource(navigationRoot, "CommonStates", "Pressed", "Content.Foreground", "PipsPagerNavigationButtonForegroundPressed");
            AssertStateSetterDynamicResource(navigationRoot, "CommonStates", "Disabled", "RootGrid.Background", "PipsPagerNavigationButtonBackgroundDisabled");
            AssertStateSetterDynamicResource(navigationRoot, "CommonStates", "Disabled", "RootGrid.BorderBrush", "PipsPagerNavigationButtonBorderBrushDisabled");
            AssertStateSetterDynamicResource(navigationRoot, "CommonStates", "Disabled", "Content.Foreground", "PipsPagerNavigationButtonForegroundDisabled");

            var normalPipRoot = FindNamedDescendant<GridEx>(normalPipButton, "RootGrid");
            AssertStateSetterDynamicResource(normalPipRoot, "CommonStates", "PointerOver", "RootGrid.Background", "PipsPagerSelectionIndicatorBackgroundPointerOver");
            AssertStateSetterDynamicResource(normalPipRoot, "CommonStates", "PointerOver", "RootGrid.BorderBrush", "PipsPagerSelectionIndicatorBorderBrushPointerOver");
            AssertStateSetterDynamicResource(normalPipRoot, "CommonStates", "PointerOver", "Content.Foreground", "PipsPagerSelectionIndicatorForegroundPointerOver");
            AssertStateSetterDynamicResource(normalPipRoot, "CommonStates", "Pressed", "RootGrid.Background", "PipsPagerSelectionIndicatorBackgroundPressed");
            AssertStateSetterDynamicResource(normalPipRoot, "CommonStates", "Pressed", "RootGrid.BorderBrush", "PipsPagerSelectionIndicatorBorderBrushPressed");
            AssertStateSetterDynamicResource(normalPipRoot, "CommonStates", "Pressed", "Content.Foreground", "PipsPagerSelectionIndicatorForegroundPressed");
            AssertStateSetterDynamicResource(normalPipRoot, "CommonStates", "Disabled", "RootGrid.Background", "PipsPagerSelectionIndicatorBackgroundDisabled");
            AssertStateSetterDynamicResource(normalPipRoot, "CommonStates", "Disabled", "RootGrid.BorderBrush", "PipsPagerSelectionIndicatorBorderBrushDisabled");
            AssertStateSetterDynamicResource(normalPipRoot, "CommonStates", "Disabled", "Content.Foreground", "PipsPagerSelectionIndicatorForegroundDisabled");
        });
    }

    [TestMethod]
    public void ThemeResourcesUseWinUI3PipsPagerHighContrastTokens()
    {
        WpfTestHost.Run(() =>
        {
            var resourceReferences = new[]
            {
                ("PipsPagerSelectionIndicatorBackground", "ControlFillColorTransparentBrush", "SystemColorButtonFaceColorBrush"),
                ("PipsPagerSelectionIndicatorBackgroundPointerOver", "ControlFillColorTransparentBrush", "SystemColorHighlightColorBrush"),
                ("PipsPagerSelectionIndicatorBackgroundPressed", "ControlFillColorTransparentBrush", "SystemColorHighlightColorBrush"),
                ("PipsPagerSelectionIndicatorBackgroundSelected", "ControlFillColorTransparentBrush", "SystemColorHighlightColorBrush"),
                ("PipsPagerSelectionIndicatorBackgroundDisabled", "ControlFillColorTransparentBrush", "SystemColorButtonFaceColorBrush"),
                ("PipsPagerSelectionIndicatorBorderBrush", "ControlFillColorTransparentBrush", "SystemColorButtonFaceColorBrush"),
                ("PipsPagerSelectionIndicatorBorderBrushPointerOver", "ControlFillColorTransparentBrush", "SystemColorHighlightColorBrush"),
                ("PipsPagerSelectionIndicatorBorderBrushPressed", "ControlFillColorTransparentBrush", "SystemColorHighlightColorBrush"),
                ("PipsPagerSelectionIndicatorBorderBrushSelected", "ControlFillColorTransparentBrush", "SystemColorHighlightColorBrush"),
                ("PipsPagerSelectionIndicatorBorderBrushDisabled", "ControlFillColorTransparentBrush", "SystemColorButtonFaceColorBrush"),
                ("PipsPagerSelectionIndicatorForeground", "ControlStrongFillColorDefaultBrush", "SystemColorButtonTextColorBrush"),
                ("PipsPagerSelectionIndicatorForegroundPointerOver", "TextFillColorSecondaryBrush", "SystemColorHighlightTextColorBrush"),
                ("PipsPagerSelectionIndicatorForegroundPressed", "TextFillColorSecondaryBrush", "SystemColorHighlightTextColorBrush"),
                ("PipsPagerSelectionIndicatorForegroundSelected", "ControlStrongFillColorDefaultBrush", "SystemColorHighlightTextColorBrush"),
                ("PipsPagerSelectionIndicatorForegroundDisabled", "ControlStrongFillColorDisabledBrush", "SystemColorGrayTextColorBrush"),
                ("PipsPagerNavigationButtonBackground", "ControlFillColorTransparentBrush", "SystemColorButtonFaceColorBrush"),
                ("PipsPagerNavigationButtonBackgroundPointerOver", "ControlFillColorTransparentBrush", "SystemColorButtonFaceColorBrush"),
                ("PipsPagerNavigationButtonBackgroundPressed", "ControlFillColorTransparentBrush", "SystemColorButtonFaceColorBrush"),
                ("PipsPagerNavigationButtonBackgroundDisabled", "ControlFillColorTransparentBrush", "SystemColorButtonFaceColorBrush"),
                ("PipsPagerNavigationButtonBorderBrush", "ControlFillColorTransparentBrush", "SystemColorButtonFaceColorBrush"),
                ("PipsPagerNavigationButtonBorderBrushPointerOver", "ControlFillColorTransparentBrush", "SystemColorHighlightColorBrush"),
                ("PipsPagerNavigationButtonBorderBrushPressed", "ControlFillColorTransparentBrush", "SystemColorHighlightColorBrush"),
                ("PipsPagerNavigationButtonBorderBrushDisabled", "ControlFillColorTransparentBrush", "SystemColorButtonFaceColorBrush"),
                ("PipsPagerNavigationButtonForeground", "ControlStrongFillColorDefaultBrush", "SystemColorButtonTextColorBrush"),
                ("PipsPagerNavigationButtonForegroundPointerOver", "TextFillColorSecondaryBrush", "SystemColorHighlightColorBrush"),
                ("PipsPagerNavigationButtonForegroundPressed", "TextFillColorSecondaryBrush", "SystemColorHighlightColorBrush"),
                ("PipsPagerNavigationButtonForegroundDisabled", "ControlStrongFillColorDisabledBrush", "SystemColorGrayTextColorBrush")
            };

            foreach (var (resourceKey, lightDarkResourceKey, highContrastResourceKey) in resourceReferences)
            {
                AssertThemeResourceReference("Light", resourceKey, lightDarkResourceKey);
                AssertThemeResourceReference("Dark", resourceKey, lightDarkResourceKey);
                AssertThemeResourceReference("HighContrast", resourceKey, highContrastResourceKey);
            }
        });
    }

    private static List<Button> GetPipButtons(DependencyObject root)
    {
        return VisualTreeTestHelper
            .EnumerateDescendants(root)
            .OfType<Button>()
            .Where(button => button.Tag is int)
            .OrderBy(button => (int)button.Tag)
            .ToList();
    }

    private static Button GetNamedButton(DependencyObject root, string name)
    {
        var button = VisualTreeTestHelper
            .EnumerateDescendants(root)
            .OfType<Button>()
            .FirstOrDefault(candidate => AutomationProperties.GetName(candidate) == name);

        if (button == null)
        {
            Assert.Fail($"Could not find button named '{name}'.");
            throw new AssertFailedException();
        }

        return button;
    }

    private static VisualStateEx AssertStateSetter(
        FrameworkElement stateGroupsRoot,
        string groupName,
        string stateName,
        string setterTarget)
    {
        var group = VisualStateManager.GetVisualStateGroups(stateGroupsRoot)
            .OfType<VisualStateGroup>()
            .Single(item => item.Name == groupName);
        var state = group.States
            .Cast<VisualState>()
            .Single(item => item.Name == stateName);

        Assert.IsInstanceOfType(state, typeof(VisualStateEx));

        var stateEx = (VisualStateEx)state;
        Assert.IsTrue(
            stateEx.Setters.Any(setter => setter.Target == setterTarget),
            $"{groupName}.{stateName} should set {setterTarget}.");
        return stateEx;
    }

    private static string GetCurrentStateName(FrameworkElement stateGroupsRoot, string groupName)
    {
        var group = VisualStateManager.GetVisualStateGroups(stateGroupsRoot)
            .OfType<VisualStateGroup>()
            .Single(item => item.Name == groupName);
        Assert.IsNotNull(group.CurrentState);
        return group.CurrentState.Name;
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
        var state = AssertStateSetter(stateGroupsRoot, groupName, stateName, target);
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

    private static void AssertThemeResourceReference(string themeName, object resourceKey, object expectedResourceKey)
    {
        var themeDictionary = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(themeDictionary.Contains(resourceKey), $"{themeName} is missing {resourceKey}.");
        Assert.IsTrue(themeDictionary.Contains(expectedResourceKey), $"{themeName} is missing {expectedResourceKey}.");
        Assert.AreSame(themeDictionary[expectedResourceKey], themeDictionary[resourceKey], $"{themeName}:{resourceKey}");
    }

    private static void AssertRotateTransform(Transform transform)
    {
        Assert.IsInstanceOfType(transform, typeof(RotateTransform));
        Assert.AreEqual(-90, ((RotateTransform)transform).Angle);
    }

    private static T FindNamedDescendant<T>(DependencyObject root, string name)
        where T : FrameworkElement
    {
        foreach (var descendant in VisualTreeTestHelper.EnumerateDescendants(root))
        {
            if (descendant is T element && element.Name == name)
            {
                return element;
            }
        }

        throw new AssertFailedException($"Could not find descendant named '{name}'.");
    }
}
