using System;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using ModernWpf.Automation.Peers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.ToggleSwitchControl;

[TestClass]
public class ToggleSwitchApiTests
{
    [TestMethod]
    public void DragDoesNotToggleUntilCrossingHalfRange()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var toggleSwitch = new ModernWpf.Controls.ToggleSwitch();
            using var host = new TestWindowHost(toggleSwitch, width: 260, height: 120);
            host.UpdateLayout();

            var thumb = FindNamedDescendant<Thumb>(toggleSwitch, "SwitchThumb");

            RaiseDragStarted(thumb);
            RaiseDragDelta(thumb, 3);
            RaiseDragCompleted(thumb);
            host.UpdateLayout();

            Assert.IsFalse(toggleSwitch.IsOn);
        });
    }

    [TestMethod]
    public void DragDeltaAccumulatesBeforeThresholdToggle()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var toggleSwitch = new ModernWpf.Controls.ToggleSwitch();
            using var host = new TestWindowHost(toggleSwitch, width: 260, height: 120);
            host.UpdateLayout();

            var thumb = FindNamedDescendant<Thumb>(toggleSwitch, "SwitchThumb");

            RaiseDragStarted(thumb);
            RaiseDragDelta(thumb, 6);
            RaiseDragDelta(thumb, 6);
            RaiseDragCompleted(thumb);
            host.UpdateLayout();

            Assert.IsTrue(toggleSwitch.IsOn);
        });
    }

    [TestMethod]
    public void VerticalDragDeltaDoesNotMarkDragAsMoved()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var toggleSwitch = new ModernWpf.Controls.ToggleSwitch();
            using var host = new TestWindowHost(toggleSwitch, width: 260, height: 120);
            host.UpdateLayout();

            var thumb = FindNamedDescendant<Thumb>(toggleSwitch, "SwitchThumb");
            var stateGroupsRoot = FindStateGroupsRoot(toggleSwitch);

            RaiseDragStarted(thumb);
            RaiseDragDelta(thumb, horizontalChange: 0, verticalChange: 24);
            RaiseDragCompleted(thumb);
            host.UpdateLayout();

            Assert.IsFalse(toggleSwitch.IsOn);
            Assert.AreEqual("Off", GetCurrentStateName(stateGroupsRoot, "ToggleStates"));
        });
    }

    [TestMethod]
    public void NormalStateUsesWinUIVisualStateSetters()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var toggleSwitch = new ModernWpf.Controls.ToggleSwitch();
            using var host = new TestWindowHost(toggleSwitch, width: 260, height: 120);
            host.UpdateLayout();

            var stateGroupsRoot = FindStateGroupsRoot(toggleSwitch);
            var normalState = FindVisualStateEx(stateGroupsRoot, "CommonStates", "Normal");
            AssertStateSetter(normalState, "SwitchKnobOff.Fill");
            AssertStateSetter(normalState, "SwitchKnobOn.Background");
            AssertStateSetter(normalState, "SwitchKnobBounds.Fill");
            AssertStateSetter(normalState, "SwitchKnobBounds.Stroke");

            var outerBorder = FindNamedDescendant<Rectangle>(toggleSwitch, "OuterBorder");
            var switchKnobBounds = FindNamedDescendant<Rectangle>(toggleSwitch, "SwitchKnobBounds");
            var switchKnobOff = FindNamedDescendant<Rectangle>(toggleSwitch, "SwitchKnobOff");
            var switchKnobOn = FindNamedDescendant<Border>(toggleSwitch, "SwitchKnobOn");
            var switchAreaGrid = FindNamedDescendant<Border>(toggleSwitch, "SwitchAreaGrid");

            Assert.IsTrue(System.Windows.VisualStateManager.GoToState(toggleSwitch, "PointerOver", false));
            host.UpdateLayout();
            Assert.IsTrue(System.Windows.VisualStateManager.GoToState(toggleSwitch, "Normal", false));
            host.UpdateLayout();

            AssertBrushEquals((Brush)outerBorder.TryFindResource("ToggleSwitchStrokeOff"), outerBorder.Stroke);
            AssertBrushEquals((Brush)outerBorder.TryFindResource("ToggleSwitchFillOff"), outerBorder.Fill);
            AssertBrushEquals((Brush)switchKnobOff.TryFindResource("ToggleSwitchKnobFillOff"), switchKnobOff.Fill);
            AssertBrushEquals((Brush)switchKnobOn.TryFindResource("ToggleSwitchKnobFillOn"), switchKnobOn.Background);
            AssertBrushEquals((Brush)switchKnobBounds.TryFindResource("ToggleSwitchFillOn"), switchKnobBounds.Fill);
            AssertBrushEquals((Brush)switchKnobBounds.TryFindResource("ToggleSwitchStrokeOn"), switchKnobBounds.Stroke);
            AssertBrushEquals((Brush)switchAreaGrid.TryFindResource("ToggleSwitchContainerBackground"), switchAreaGrid.Background);
        });
    }

    [TestMethod]
    public void PressedStateUsesWinUIVisualStateSetters()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var toggleSwitch = new ModernWpf.Controls.ToggleSwitch();
            using var host = new TestWindowHost(toggleSwitch, width: 260, height: 120);
            host.UpdateLayout();

            var switchKnobOn = FindNamedDescendant<Border>(toggleSwitch, "SwitchKnobOn");
            var switchKnobOff = FindNamedDescendant<Rectangle>(toggleSwitch, "SwitchKnobOff");
            var switchKnobBounds = FindNamedDescendant<Rectangle>(toggleSwitch, "SwitchKnobBounds");
            var stateGroupsRoot = FindStateGroupsRoot(toggleSwitch);
            var pressedState = FindVisualStateEx(stateGroupsRoot, "CommonStates", "Pressed");
            AssertStateSetter(pressedState, "SwitchKnobOn.HorizontalAlignment");
            AssertStateSetter(pressedState, "SwitchKnobOn.Margin");
            AssertStateSetter(pressedState, "SwitchKnobOff.HorizontalAlignment");
            AssertStateSetter(pressedState, "SwitchKnobOff.Margin");
            AssertStateSetter(pressedState, "SwitchKnobBounds.Fill");
            AssertStateSetter(pressedState, "SwitchKnobBounds.Stroke");
            AssertStateSetter(pressedState, "SwitchKnobOff.Fill");
            AssertStateSetter(pressedState, "SwitchKnobOn.Background");

            Assert.IsTrue(System.Windows.VisualStateManager.GoToState(toggleSwitch, "Pressed", false));
            host.UpdateLayout();

            Assert.AreEqual(HorizontalAlignment.Right, switchKnobOn.HorizontalAlignment);
            Assert.AreEqual(new Thickness(0, 0, 3, 0), switchKnobOn.Margin);
            Assert.AreEqual(HorizontalAlignment.Left, switchKnobOff.HorizontalAlignment);
            Assert.AreEqual(new Thickness(3, 0, 0, 0), switchKnobOff.Margin);
            AssertBrushEquals((Brush)switchKnobBounds.TryFindResource("ToggleSwitchFillOnPressed"), switchKnobBounds.Fill);
            AssertBrushEquals((Brush)switchKnobBounds.TryFindResource("ToggleSwitchStrokeOnPressed"), switchKnobBounds.Stroke);
            AssertBrushEquals((Brush)switchKnobOff.TryFindResource("ToggleSwitchKnobFillOffPressed"), switchKnobOff.Fill);
            AssertBrushEquals((Brush)switchKnobOn.TryFindResource("ToggleSwitchKnobFillOnPressed"), switchKnobOn.Background);
        });
    }

    [TestMethod]
    public void PointerOverStateUsesWinUIVisualStateSetters()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var toggleSwitch = new ModernWpf.Controls.ToggleSwitch();
            using var host = new TestWindowHost(toggleSwitch, width: 260, height: 120);
            host.UpdateLayout();

            var stateGroupsRoot = FindStateGroupsRoot(toggleSwitch);
            var pointerOverState = FindVisualStateEx(stateGroupsRoot, "CommonStates", "PointerOver");
            AssertStateSetter(pointerOverState, "SwitchKnobBounds.Fill");
            AssertStateSetter(pointerOverState, "SwitchKnobBounds.Stroke");
            AssertStateSetter(pointerOverState, "SwitchKnobOff.Fill");
            AssertStateSetter(pointerOverState, "SwitchKnobOn.Background");

            var switchKnobBounds = FindNamedDescendant<Rectangle>(toggleSwitch, "SwitchKnobBounds");
            var switchKnobOff = FindNamedDescendant<Rectangle>(toggleSwitch, "SwitchKnobOff");
            var switchKnobOn = FindNamedDescendant<Border>(toggleSwitch, "SwitchKnobOn");

            Assert.IsTrue(System.Windows.VisualStateManager.GoToState(toggleSwitch, "PointerOver", false));
            host.UpdateLayout();

            AssertBrushEquals((Brush)switchKnobBounds.TryFindResource("ToggleSwitchFillOnPointerOver"), switchKnobBounds.Fill);
            AssertBrushEquals((Brush)switchKnobBounds.TryFindResource("ToggleSwitchStrokeOnPointerOver"), switchKnobBounds.Stroke);
            AssertBrushEquals((Brush)switchKnobOff.TryFindResource("ToggleSwitchKnobFillOffPointerOver"), switchKnobOff.Fill);
            AssertBrushEquals((Brush)switchKnobOn.TryFindResource("ToggleSwitchKnobFillOnPointerOver"), switchKnobOn.Background);
        });
    }

    [TestMethod]
    public void DisabledStateUsesWinUIVisualStateSetters()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var toggleSwitch = new ModernWpf.Controls.ToggleSwitch
            {
                Header = "Header text",
                OffContent = "Off text",
                OnContent = "On text"
            };
            using var host = new TestWindowHost(toggleSwitch, width: 260, height: 120);
            host.UpdateLayout();

            var stateGroupsRoot = FindStateGroupsRoot(toggleSwitch);
            var disabledState = FindVisualStateEx(stateGroupsRoot, "CommonStates", "Disabled");
            AssertStateSetter(disabledState, "HeaderContentPresenter.Foreground");
            AssertStateSetter(disabledState, "OffContentPresenter.Foreground");
            AssertStateSetter(disabledState, "OnContentPresenter.Foreground");
            AssertStateSetter(disabledState, "SwitchKnobBounds.Fill");
            AssertStateSetter(disabledState, "SwitchKnobBounds.Stroke");
            AssertStateSetter(disabledState, "SwitchKnobOff.Fill");
            AssertStateSetter(disabledState, "SwitchKnobOn.Background");

            var headerPresenter = FindNamedDescendant<ContentPresenterEx>(toggleSwitch, "HeaderContentPresenter");
            var offPresenter = FindNamedDescendant<ContentPresenterEx>(toggleSwitch, "OffContentPresenter");
            var onPresenter = FindNamedDescendant<ContentPresenterEx>(toggleSwitch, "OnContentPresenter");
            var switchKnobBounds = FindNamedDescendant<Rectangle>(toggleSwitch, "SwitchKnobBounds");
            var switchKnobOff = FindNamedDescendant<Rectangle>(toggleSwitch, "SwitchKnobOff");
            var switchKnobOn = FindNamedDescendant<Border>(toggleSwitch, "SwitchKnobOn");

            toggleSwitch.IsEnabled = false;
            host.UpdateLayout();

            AssertBrushEquals((Brush)headerPresenter.TryFindResource("ToggleSwitchHeaderForegroundDisabled"), headerPresenter.Foreground);
            AssertBrushEquals((Brush)offPresenter.TryFindResource("ToggleSwitchContentForegroundDisabled"), offPresenter.Foreground);
            AssertBrushEquals((Brush)onPresenter.TryFindResource("ToggleSwitchContentForegroundDisabled"), onPresenter.Foreground);
            AssertBrushEquals((Brush)switchKnobBounds.TryFindResource("ToggleSwitchFillOnDisabled"), switchKnobBounds.Fill);
            AssertBrushEquals((Brush)switchKnobBounds.TryFindResource("ToggleSwitchStrokeOnDisabled"), switchKnobBounds.Stroke);
            AssertBrushEquals((Brush)switchKnobOff.TryFindResource("ToggleSwitchKnobFillOffDisabled"), switchKnobOff.Fill);
            AssertBrushEquals((Brush)switchKnobOn.TryFindResource("ToggleSwitchKnobFillOnDisabled"), switchKnobOn.Background);
        });
    }

    [TestMethod]
    public void ThemeResourcesUseWinUI3ToggleSwitchColorAliases()
    {
        foreach (var themeName in new[] { "Light", "Dark" })
        {
            AssertThemeResourceValue(themeName, "SystemControlTransparentColor", Color.FromArgb(0x00, 0x00, 0x00, 0x00));
            AssertThemeResourceReference(themeName, "ToggleSwitchContainerBackgroundPointerOver", "SubtleFillColorTransparent");
            AssertThemeResourceReference(themeName, "ToggleSwitchContainerBackgroundPressed", "SubtleFillColorTransparent");
            AssertThemeResourceReference(themeName, "ToggleSwitchContainerBackgroundDisabled", "SubtleFillColorTransparent");
            AssertThemeResourceReference(themeName, "ToggleSwitchStrokeOffPointerOver", "ControlStrongStrokeColorDefault");
            AssertThemeResourceReference(themeName, "ToggleSwitchStrokeOffPressed", "ControlStrongStrokeColorDefault");
            AssertThemeResourceReference(themeName, "ToggleSwitchStrokeOffDisabled", "ControlStrongStrokeColorDisabled");
        }
    }

    [TestMethod]
    public void HighContrastThemeResourcesUseWinUI3ToggleSwitchAliases()
    {
        AssertThemeResourceValue("HighContrast", "SystemControlTransparentColor", Color.FromArgb(0x00, 0x00, 0x00, 0x00));
        AssertThemeResourceReference("HighContrast", "ToggleSwitchContainerBackgroundPointerOver", "SystemControlTransparentColor");
        AssertThemeResourceReference("HighContrast", "ToggleSwitchContainerBackgroundPressed", "SystemControlTransparentColor");
        AssertThemeResourceReference("HighContrast", "ToggleSwitchContainerBackgroundDisabled", "SystemControlTransparentColor");
        AssertThemeResourceReference("HighContrast", "ToggleSwitchFillOff", "SystemColorButtonFaceColorBrush");
        AssertThemeResourceReference("HighContrast", "ToggleSwitchFillOffPointerOver", "SystemColorHighlightTextColor");
        AssertThemeResourceReference("HighContrast", "ToggleSwitchFillOffPressed", "SystemColorHighlightTextColor");
        AssertThemeResourceReference("HighContrast", "ToggleSwitchFillOffDisabled", "SystemColorWindowColor");
        AssertThemeResourceReference("HighContrast", "ToggleSwitchStrokeOff", "SystemColorButtonTextColorBrush");
        AssertThemeResourceReference("HighContrast", "ToggleSwitchStrokeOffPointerOver", "SystemColorHighlightColor");
        AssertThemeResourceReference("HighContrast", "ToggleSwitchStrokeOffPressed", "SystemColorHighlightColor");
        AssertThemeResourceReference("HighContrast", "ToggleSwitchStrokeOffDisabled", "SystemColorGrayTextColor");
        AssertThemeResourceReference("HighContrast", "ToggleSwitchFillOn", "SystemColorHighlightColorBrush");
        AssertThemeResourceReference("HighContrast", "ToggleSwitchFillOnPointerOver", "SystemColorButtonFaceColorBrush");
        AssertThemeResourceReference("HighContrast", "ToggleSwitchFillOnPressed", "SystemColorButtonFaceColorBrush");
        AssertThemeResourceReference("HighContrast", "ToggleSwitchFillOnDisabled", "SystemColorWindowColorBrush");
        AssertThemeResourceReference("HighContrast", "ToggleSwitchStrokeOn", "SystemColorHighlightColorBrush");
        AssertThemeResourceReference("HighContrast", "ToggleSwitchStrokeOnPointerOver", "SystemColorButtonTextColorBrush");
        AssertThemeResourceReference("HighContrast", "ToggleSwitchStrokeOnPressed", "SystemColorButtonTextColorBrush");
        AssertThemeResourceReference("HighContrast", "ToggleSwitchStrokeOnDisabled", "SystemColorGrayTextColorBrush");
        AssertThemeResourceReference("HighContrast", "ToggleSwitchKnobFillOff", "SystemColorButtonTextColorBrush");
        AssertThemeResourceReference("HighContrast", "ToggleSwitchKnobFillOffPointerOver", "SystemColorHighlightColorBrush");
        AssertThemeResourceReference("HighContrast", "ToggleSwitchKnobFillOffPressed", "SystemColorHighlightColorBrush");
        AssertThemeResourceReference("HighContrast", "ToggleSwitchKnobFillOffDisabled", "SystemColorGrayTextColorBrush");
        AssertThemeResourceReference("HighContrast", "ToggleSwitchKnobFillOn", "SystemColorHighlightTextColorBrush");
        AssertThemeResourceReference("HighContrast", "ToggleSwitchKnobFillOnPointerOver", "SystemColorButtonTextColorBrush");
        AssertThemeResourceReference("HighContrast", "ToggleSwitchKnobFillOnPressed", "SystemColorButtonTextColorBrush");
        AssertThemeResourceReference("HighContrast", "ToggleSwitchKnobFillOnDisabled", "SystemColorGrayTextColorBrush");
        AssertThemeResourceReference("HighContrast", "ToggleSwitchKnobStrokeOn", "SystemControlTransparentBrush");
    }

    [TestMethod]
    public void HeaderPresenterVisibilityMatchesWinUI3NullRules()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var toggleSwitch = new ModernWpf.Controls.ToggleSwitch();
            using var host = new TestWindowHost(toggleSwitch, width: 260, height: 120);
            host.UpdateLayout();

            var headerPresenter = FindNamedDescendant<ContentPresenterEx>(toggleSwitch, "HeaderContentPresenter");
            Assert.AreEqual(Visibility.Collapsed, headerPresenter.Visibility);

            toggleSwitch.Header = string.Empty;
            host.UpdateLayout();
            Assert.AreEqual(Visibility.Visible, headerPresenter.Visibility);

            toggleSwitch.Header = null;
            host.UpdateLayout();
            Assert.AreEqual(Visibility.Collapsed, headerPresenter.Visibility);

            toggleSwitch.HeaderTemplate = CreateTextTemplate();
            host.UpdateLayout();
            Assert.AreEqual(Visibility.Visible, headerPresenter.Visibility);

            toggleSwitch.HeaderTemplate = null;
            host.UpdateLayout();
            Assert.AreEqual(Visibility.Collapsed, headerPresenter.Visibility);
        });
    }

    [TestMethod]
    public void HeaderPlacementUsesWinUIHeaderStates()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var toggleSwitch = new ModernWpf.Controls.ToggleSwitch
            {
                Header = "Network"
            };
            using var host = new TestWindowHost(toggleSwitch, width: 500, height: 160);
            host.UpdateLayout();

            var stateGroupsRoot = FindStateGroupsRoot(toggleSwitch);
            var leftHeaderState = FindVisualStateEx(stateGroupsRoot, "HeaderStates", "LeftHeader");
            AssertStateSetter(leftHeaderState, "HeaderContentPresenter.(Grid.Row)");
            AssertStateSetter(leftHeaderState, "HeaderContentPresenter.(Grid.Column)");
            AssertStateSetter(leftHeaderState, "HeaderContentPresenter.(Grid.ColumnSpan)");
            AssertStateSetter(leftHeaderState, "HeaderContentPresenter.Margin");
            AssertStateSetter(leftHeaderState, "HeaderContentPresenter.MaxWidth");

            var headerPresenter = FindNamedDescendant<ContentPresenterEx>(toggleSwitch, "HeaderContentPresenter");
            Assert.AreEqual(ControlHeaderPlacement.Top, toggleSwitch.HeaderPlacement);
            Assert.AreEqual("TopHeader", GetCurrentStateName(stateGroupsRoot, "HeaderStates"));
            Assert.AreEqual(0, Grid.GetRow(headerPresenter));
            Assert.AreEqual(1, Grid.GetColumn(headerPresenter));
            Assert.AreEqual(1, Grid.GetColumnSpan(headerPresenter));
            Assert.AreEqual((Thickness)headerPresenter.TryFindResource("ToggleSwitchTopHeaderMargin"), headerPresenter.Margin);
            Assert.AreEqual(double.PositiveInfinity, headerPresenter.MaxWidth);

            toggleSwitch.HeaderPlacement = ControlHeaderPlacement.Left;
            host.UpdateLayout();

            Assert.AreEqual("LeftHeader", GetCurrentStateName(stateGroupsRoot, "HeaderStates"));
            Assert.AreEqual(1, Grid.GetRow(headerPresenter));
            Assert.AreEqual(0, Grid.GetColumn(headerPresenter));
            Assert.AreEqual(1, Grid.GetColumnSpan(headerPresenter));
            Assert.AreEqual((Thickness)headerPresenter.TryFindResource("ToggleSwitchLeftHeaderMargin"), headerPresenter.Margin);
            Assert.AreEqual((double)headerPresenter.TryFindResource("ToggleSwitchLeftHeaderMaxWidth"), headerPresenter.MaxWidth);

            toggleSwitch.HeaderPlacement = ControlHeaderPlacement.Top;
            host.UpdateLayout();

            Assert.AreEqual("TopHeader", GetCurrentStateName(stateGroupsRoot, "HeaderStates"));
            Assert.AreEqual(0, Grid.GetRow(headerPresenter));
            Assert.AreEqual(1, Grid.GetColumn(headerPresenter));
            Assert.AreEqual(1, Grid.GetColumnSpan(headerPresenter));
            Assert.AreEqual((Thickness)headerPresenter.TryFindResource("ToggleSwitchTopHeaderMargin"), headerPresenter.Margin);
            Assert.AreEqual(double.PositiveInfinity, headerPresenter.MaxWidth);
        });
    }

    [TestMethod]
    public void DependencyPropertyMetadataMatchesWinUIAffectsMeasureFlags()
    {
        var ownerType = typeof(ModernWpf.Controls.ToggleSwitch);

        AssertAffectsMeasure(ModernWpf.Controls.ToggleSwitch.IsOnProperty, ownerType, expected: true);
        AssertAffectsMeasure(ModernWpf.Controls.ToggleSwitch.HeaderProperty, ownerType, expected: true);
        AssertAffectsMeasure(ModernWpf.Controls.ToggleSwitch.HeaderTemplateProperty, ownerType, expected: true);
        AssertAffectsMeasure(ModernWpf.Controls.ToggleSwitch.OnContentProperty, ownerType, expected: false);
        AssertAffectsMeasure(ModernWpf.Controls.ToggleSwitch.OnContentTemplateProperty, ownerType, expected: true);
        AssertAffectsMeasure(ModernWpf.Controls.ToggleSwitch.OffContentProperty, ownerType, expected: true);
        AssertAffectsMeasure(ModernWpf.Controls.ToggleSwitch.OffContentTemplateProperty, ownerType, expected: true);
    }

    [TestMethod]
    public void TemplateSettingsTrackWinUIKnobOffsets()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var toggleSwitch = new ModernWpf.Controls.ToggleSwitch();
            using var host = new TestWindowHost(toggleSwitch, width: 260, height: 120);
            host.UpdateLayout();

            ToggleSwitchTemplateSettings settings = toggleSwitch.TemplateSettings;
            Assert.IsNotNull(settings);
            Assert.AreEqual(-20d, settings.KnobOffToOnOffset, 0.1);
            Assert.AreEqual(20d, settings.KnobOnToOffOffset, 0.1);

            var initiallyOnToggleSwitch = new ModernWpf.Controls.ToggleSwitch
            {
                IsOn = true
            };
            using var initiallyOnHost = new TestWindowHost(initiallyOnToggleSwitch, width: 260, height: 120);
            initiallyOnHost.UpdateLayout();

            Assert.AreEqual(-20d, initiallyOnToggleSwitch.TemplateSettings.KnobOffToOnOffset, 0.1);
            Assert.AreEqual(20d, initiallyOnToggleSwitch.TemplateSettings.KnobOnToOffOffset, 0.1);

            var thumb = FindNamedDescendant<Thumb>(toggleSwitch, "SwitchThumb");
            RaiseDragStarted(thumb);
            RaiseDragDelta(thumb, 6);

            Assert.AreEqual(6d, settings.KnobCurrentToOffOffset, 0.1);
            Assert.AreEqual(-14d, settings.KnobCurrentToOnOffset, 0.1);
        });
    }

    [TestMethod]
    public void TapAndDragCompletionFollowWinUISplitHandlers()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var toggleSwitch = new ModernWpf.Controls.ToggleSwitch();
            using var host = new TestWindowHost(toggleSwitch, width: 260, height: 120);
            host.UpdateLayout();

            var thumb = FindNamedDescendant<Thumb>(toggleSwitch, "SwitchThumb");

            RaiseDragStarted(thumb);
            RaiseDragCompleted(thumb);
            host.UpdateLayout();

            Assert.IsFalse(toggleSwitch.IsOn, "DragCompleted without movement should not toggle; WinUI toggles taps through the tap handler.");

            RaiseThumbPreviewMouseLeftButtonUp(thumb);
            host.UpdateLayout();

            Assert.IsTrue(toggleSwitch.IsOn);
        });
    }

    [TestMethod]
    public void ToggledEventFiresWhenIsOnChanges()
    {
        WpfTestHost.Run(() =>
        {
            var toggleSwitch = new ModernWpf.Controls.ToggleSwitch();
            int eventCount = 0;
            RoutedEventArgs? lastArgs = null;

            toggleSwitch.Toggled += (sender, args) =>
            {
                Assert.AreSame(toggleSwitch, sender);
                eventCount++;
                lastArgs = args;
            };

            toggleSwitch.IsOn = true;

            Assert.AreEqual(1, eventCount);
            Assert.AreSame(toggleSwitch, lastArgs?.OriginalSource);

            toggleSwitch.IsOn = false;

            Assert.AreEqual(2, eventCount);
            Assert.AreSame(toggleSwitch, lastArgs?.OriginalSource);
        });
    }

    [TestMethod]
    public void DraggingDoesNotChangeContentState()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var toggleSwitch = new ModernWpf.Controls.ToggleSwitch();
            using var host = new TestWindowHost(toggleSwitch, width: 260, height: 120);
            host.UpdateLayout();

            var thumb = FindNamedDescendant<Thumb>(toggleSwitch, "SwitchThumb");
            var stateGroupsRoot = FindStateGroupsRoot(toggleSwitch);
            Assert.AreEqual("OffContent", GetCurrentStateName(stateGroupsRoot, "ContentStates"));

            Assert.IsTrue(System.Windows.VisualStateManager.GoToState(toggleSwitch, "OnContent", false));
            Assert.AreEqual("OnContent", GetCurrentStateName(stateGroupsRoot, "ContentStates"));

            RaiseDragStarted(thumb);
            host.UpdateLayout();

            Assert.AreEqual("Dragging", GetCurrentStateName(stateGroupsRoot, "ToggleStates"));
            Assert.AreEqual("OnContent", GetCurrentStateName(stateGroupsRoot, "ContentStates"));
        });
    }

    [TestMethod]
    public void SpaceKeyHandlesDownAndTogglesOnUp()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var toggleSwitch = new ModernWpf.Controls.ToggleSwitch();
            using var host = new TestWindowHost(toggleSwitch, width: 260, height: 120);
            host.UpdateLayout();

            var keyDown = RaiseKey(toggleSwitch, Keyboard.KeyDownEvent, Key.Space);
            Assert.IsTrue(keyDown.Handled);
            Assert.IsFalse(toggleSwitch.IsOn);

            var keyUp = RaiseKey(toggleSwitch, Keyboard.KeyUpEvent, Key.Space);
            Assert.IsTrue(keyUp.Handled);
            Assert.IsTrue(toggleSwitch.IsOn);
        });
    }

    [TestMethod]
    public void DirectionalKeysDoNotToggle()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var toggleSwitch = new ModernWpf.Controls.ToggleSwitch();
            using var host = new TestWindowHost(toggleSwitch, width: 260, height: 120);
            host.UpdateLayout();

            foreach (var key in new[] { Key.Home, Key.End, Key.Up, Key.Down, Key.Left, Key.Right })
            {
                var keyDown = RaiseKey(toggleSwitch, Keyboard.KeyDownEvent, key);
                var keyUp = RaiseKey(toggleSwitch, Keyboard.KeyUpEvent, key);

                Assert.IsFalse(keyDown.Handled, $"{key} key-down should not be handled.");
                Assert.IsFalse(keyUp.Handled, $"{key} key-up should not be handled.");
                Assert.IsFalse(toggleSwitch.IsOn, $"{key} should not toggle the switch.");
            }
        });
    }

    [TestMethod]
    public void SpaceKeyUpWithoutPriorKeyDownDoesNotToggleOrHandle()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var toggleSwitch = new ModernWpf.Controls.ToggleSwitch();
            using var host = new TestWindowHost(toggleSwitch, width: 260, height: 120);
            host.UpdateLayout();

            var keyUp = RaiseKey(toggleSwitch, Keyboard.KeyUpEvent, Key.Space);

            Assert.IsFalse(keyUp.Handled);
            Assert.IsFalse(toggleSwitch.IsOn);
        });
    }

    [TestMethod]
    public void NonSpaceKeyDownClearsPendingSpaceToggle()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var toggleSwitch = new ModernWpf.Controls.ToggleSwitch();
            using var host = new TestWindowHost(toggleSwitch, width: 260, height: 120);
            host.UpdateLayout();

            var keyDown = RaiseKey(toggleSwitch, Keyboard.KeyDownEvent, Key.Space);
            Assert.IsTrue(keyDown.Handled);

            var nonToggleKeyDown = RaiseKey(toggleSwitch, Keyboard.KeyDownEvent, Key.A);
            Assert.IsFalse(nonToggleKeyDown.Handled);

            var keyUp = RaiseKey(toggleSwitch, Keyboard.KeyUpEvent, Key.Space);
            Assert.IsFalse(keyUp.Handled);
            Assert.IsFalse(toggleSwitch.IsOn);
        });
    }

    [TestMethod]
    public void SpaceKeyUpWhileDraggingDoesNotToggleOrHandle()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var toggleSwitch = new ModernWpf.Controls.ToggleSwitch();
            using var host = new TestWindowHost(toggleSwitch, width: 260, height: 120);
            host.UpdateLayout();

            var thumb = FindNamedDescendant<Thumb>(toggleSwitch, "SwitchThumb");
            var keyDown = RaiseKey(toggleSwitch, Keyboard.KeyDownEvent, Key.Space);
            Assert.IsTrue(keyDown.Handled);

            RaiseDragStarted(thumb);
            var keyUp = RaiseKey(toggleSwitch, Keyboard.KeyUpEvent, Key.Space);
            RaiseDragCompleted(thumb);

            Assert.IsFalse(keyUp.Handled);
            Assert.IsFalse(toggleSwitch.IsOn);
        });
    }

    [TestMethod]
    public void DefaultOnOffContentUsesWinUIDefaultValueModel()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var toggleSwitch = new ModernWpf.Controls.ToggleSwitch();

            Assert.AreEqual(global::ModernWpf.Strings.ToggleSwitchOff, toggleSwitch.OffContent);
            Assert.AreEqual(global::ModernWpf.Strings.ToggleSwitchOn, toggleSwitch.OnContent);
            Assert.AreEqual(
                BaseValueSource.Default,
                DependencyPropertyHelper.GetValueSource(toggleSwitch, ModernWpf.Controls.ToggleSwitch.OffContentProperty).BaseValueSource);
            Assert.AreEqual(
                BaseValueSource.Default,
                DependencyPropertyHelper.GetValueSource(toggleSwitch, ModernWpf.Controls.ToggleSwitch.OnContentProperty).BaseValueSource);

            toggleSwitch.OffContent = "Disconnected";
            toggleSwitch.OnContent = "Connected";

            Assert.AreEqual(
                BaseValueSource.Local,
                DependencyPropertyHelper.GetValueSource(toggleSwitch, ModernWpf.Controls.ToggleSwitch.OffContentProperty).BaseValueSource);
            Assert.AreEqual(
                BaseValueSource.Local,
                DependencyPropertyHelper.GetValueSource(toggleSwitch, ModernWpf.Controls.ToggleSwitch.OnContentProperty).BaseValueSource);
        });
    }

    [TestMethod]
    public void AutomationNameOmitsDefaultOnOffContent()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var toggleSwitch = new ModernWpf.Controls.ToggleSwitch
            {
                Header = "Wi-Fi"
            };
            using var host = new TestWindowHost(toggleSwitch, width: 260, height: 120);
            host.UpdateLayout();

            Assert.AreEqual("Wi-Fi", CreatePeer(toggleSwitch).GetName());

            toggleSwitch.OffContent = "Disconnected";
            Assert.AreEqual("Wi-Fi Disconnected", CreatePeer(toggleSwitch).GetName());

            toggleSwitch.IsOn = true;
            Assert.AreEqual("Wi-Fi", CreatePeer(toggleSwitch).GetName());

            toggleSwitch.OnContent = "Connected";
            Assert.AreEqual("Wi-Fi Connected", CreatePeer(toggleSwitch).GetName());
        });
    }

    [TestMethod]
    public void AutomationNameUsesWinUIStringExtraction()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var root = new StackPanel();
            var toggleSwitchWithOnOffContent = new ModernWpf.Controls.ToggleSwitch
            {
                OnContent = "Yes",
                OffContent = "No"
            };
            var toggleSwitchNonTextHeader = new ModernWpf.Controls.ToggleSwitch
            {
                Header = new Rectangle { Width = 10, Height = 10 },
                OnContent = "Yes",
                OffContent = "No"
            };
            var toggleSwitchNonTextOnOffContent = new ModernWpf.Controls.ToggleSwitch
            {
                Header = "Header",
                OnContent = new Rectangle { Width = 10, Height = 10 },
                OffContent = new Rectangle { Width = 10, Height = 10 }
            };
            var toggleSwitchTextElements = new ModernWpf.Controls.ToggleSwitch
            {
                Header = new TextBlock { Text = "Header" },
                OnContent = new TextBlock { Text = "Yes" },
                OffContent = new TextBlock { Text = "No" }
            };

            root.Children.Add(toggleSwitchWithOnOffContent);
            root.Children.Add(toggleSwitchNonTextHeader);
            root.Children.Add(toggleSwitchNonTextOnOffContent);
            root.Children.Add(toggleSwitchTextElements);

            using var host = new TestWindowHost(root, width: 260, height: 220);
            host.UpdateLayout();

            Assert.AreEqual("No", CreatePeer(toggleSwitchWithOnOffContent).GetName());
            Assert.AreEqual("No", CreatePeer(toggleSwitchNonTextHeader).GetName());
            Assert.AreEqual("Header", CreatePeer(toggleSwitchNonTextOnOffContent).GetName());
            Assert.AreEqual("Header No", CreatePeer(toggleSwitchTextElements).GetName());

            toggleSwitchWithOnOffContent.IsOn = true;
            toggleSwitchNonTextHeader.IsOn = true;
            toggleSwitchNonTextOnOffContent.IsOn = true;
            toggleSwitchTextElements.IsOn = true;

            Assert.AreEqual("Yes", CreatePeer(toggleSwitchWithOnOffContent).GetName());
            Assert.AreEqual("Yes", CreatePeer(toggleSwitchNonTextHeader).GetName());
            Assert.AreEqual("Header", CreatePeer(toggleSwitchNonTextOnOffContent).GetName());
            Assert.AreEqual("Header Yes", CreatePeer(toggleSwitchTextElements).GetName());
        });
    }

    [TestMethod]
    public void AutomationClickablePointTargetsThumb()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var toggleSwitch = new ModernWpf.Controls.ToggleSwitch
            {
                Header = "Multi\nline\nheader"
            };
            using var host = new TestWindowHost(toggleSwitch, width: 260, height: 160);
            host.UpdateLayout();

            var thumb = FindNamedDescendant<Thumb>(toggleSwitch, "SwitchThumb");
            var thumbPeer = UIElementAutomationPeer.FromElement(thumb) ??
                            UIElementAutomationPeer.CreatePeerForElement(thumb);
            Assert.IsNotNull(thumbPeer);

            var expected = thumbPeer.GetClickablePoint();
            var actual = CreatePeer(toggleSwitch).GetClickablePoint();

            Assert.AreEqual(expected.X, actual.X, 0.1);
            Assert.AreEqual(expected.Y, actual.Y, 0.1);
        });
    }

    [TestMethod]
    public void AutomationPeerMatchesWinUISourceShape()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var toggleSwitch = new ModernWpf.Controls.ToggleSwitch
            {
                Header = "Header",
                OnContent = "Yes",
                OffContent = "No"
            };
            using var host = new TestWindowHost(toggleSwitch, width: 260, height: 120);
            host.UpdateLayout();

            var peer = CreatePeer(toggleSwitch);

            Assert.AreEqual(nameof(ModernWpf.Controls.ToggleSwitch), peer.GetClassName());
            Assert.AreEqual(AutomationControlType.Button, peer.GetAutomationControlType());
            Assert.AreEqual("toggle switch", peer.GetLocalizedControlType());
            Assert.AreEqual(0, peer.GetChildren()?.Count ?? 0);

            var toggleProvider = (IToggleProvider)peer.GetPattern(PatternInterface.Toggle);
            Assert.AreEqual(ToggleState.Off, toggleProvider.ToggleState);

            toggleProvider.Toggle();

            Assert.IsTrue(toggleSwitch.IsOn);
            Assert.AreEqual(ToggleState.On, toggleProvider.ToggleState);

            toggleSwitch.IsEnabled = false;

            Assert.ThrowsException<ElementNotEnabledException>(() => toggleProvider.Toggle());
            Assert.IsTrue(toggleSwitch.IsOn);
        });
    }

    [TestMethod]
    public void GeneratedProtectedCallbacksMatchWinUIModel()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var toggleSwitch = new CallbackToggleSwitch();
            int initialHeaderChanges = toggleSwitch.HeaderChanges;
            int initialOnContentChanges = toggleSwitch.OnContentChanges;
            int initialOffContentChanges = toggleSwitch.OffContentChanges;
            int initialToggleChanges = toggleSwitch.ToggleChanges;

            Assert.AreEqual(0, initialHeaderChanges);
            Assert.AreEqual(0, initialOnContentChanges);
            Assert.AreEqual(0, initialOffContentChanges);
            Assert.AreEqual(0, initialToggleChanges);

            toggleSwitch.Header = "Label";

            Assert.AreEqual(initialHeaderChanges + 1, toggleSwitch.HeaderChanges);
            Assert.AreEqual(initialOnContentChanges, toggleSwitch.OnContentChanges);
            Assert.AreEqual(initialOffContentChanges, toggleSwitch.OffContentChanges);
            Assert.AreEqual(initialToggleChanges, toggleSwitch.ToggleChanges);

            toggleSwitch.OffContent = "Disabled";

            Assert.AreEqual(initialHeaderChanges + 1, toggleSwitch.HeaderChanges);
            Assert.AreEqual(initialOnContentChanges, toggleSwitch.OnContentChanges);
            Assert.AreEqual(initialOffContentChanges + 1, toggleSwitch.OffContentChanges);
            Assert.AreEqual(initialToggleChanges, toggleSwitch.ToggleChanges);

            toggleSwitch.OnContent = "Enabled";

            Assert.AreEqual(initialHeaderChanges + 1, toggleSwitch.HeaderChanges);
            Assert.AreEqual(initialOnContentChanges + 1, toggleSwitch.OnContentChanges);
            Assert.AreEqual(initialOffContentChanges + 1, toggleSwitch.OffContentChanges);
            Assert.AreEqual(initialToggleChanges, toggleSwitch.ToggleChanges);

            toggleSwitch.IsOn = true;

            Assert.AreEqual(initialHeaderChanges + 1, toggleSwitch.HeaderChanges);
            Assert.AreEqual(initialOnContentChanges + 1, toggleSwitch.OnContentChanges);
            Assert.AreEqual(initialOffContentChanges + 1, toggleSwitch.OffContentChanges);
            Assert.AreEqual(initialToggleChanges + 1, toggleSwitch.ToggleChanges);
        });
    }

    [TestMethod]
    public void VerifyContentPresentersMatchWinUITemplate()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var headerTemplate = CreateTextTemplate();
            var offContentTemplate = CreateTextTemplate();
            var onContentTemplate = CreateTextTemplate();
            var toggleSwitch = new ModernWpf.Controls.ToggleSwitch
            {
                Header = "Header text",
                HeaderTemplate = headerTemplate,
                OffContent = "Off text",
                OffContentTemplate = offContentTemplate,
                OnContent = "On text",
                OnContentTemplate = onContentTemplate
            };

            using var host = new TestWindowHost(toggleSwitch, width: 260, height: 120);
            host.UpdateLayout();

            var headerPresenter = FindNamedDescendant<ContentPresenterEx>(toggleSwitch, "HeaderContentPresenter");
            var offPresenter = FindNamedDescendant<ContentPresenterEx>(toggleSwitch, "OffContentPresenter");
            var onPresenter = FindNamedDescendant<ContentPresenterEx>(toggleSwitch, "OnContentPresenter");
            var thumb = FindNamedDescendant<Thumb>(toggleSwitch, "SwitchThumb");

            Assert.AreEqual("Header text", headerPresenter.Content);
            Assert.AreSame(headerTemplate, headerPresenter.ContentTemplate);
            AssertBrushEquals((Brush)headerPresenter.TryFindResource("ToggleSwitchHeaderForeground"), headerPresenter.Foreground);

            Assert.AreEqual("Off text", offPresenter.Content);
            Assert.AreSame(offContentTemplate, offPresenter.ContentTemplate);
            AssertBrushEquals(toggleSwitch.Foreground, offPresenter.Foreground);

            Assert.AreEqual("On text", onPresenter.Content);
            Assert.AreSame(onContentTemplate, onPresenter.ContentTemplate);
            AssertBrushEquals(toggleSwitch.Foreground, onPresenter.Foreground);

            Assert.IsNull(thumb.CacheMode);

            toggleSwitch.IsEnabled = false;
            host.UpdateLayout();

            AssertBrushEquals((Brush)headerPresenter.TryFindResource("ToggleSwitchHeaderForegroundDisabled"), headerPresenter.Foreground);
            AssertBrushEquals((Brush)offPresenter.TryFindResource("ToggleSwitchContentForegroundDisabled"), offPresenter.Foreground);
            AssertBrushEquals((Brush)onPresenter.TryFindResource("ToggleSwitchContentForegroundDisabled"), onPresenter.Foreground);
        });
    }

    [TestMethod]
    public void ValidateWinUIFootprint()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var root = new StackPanel();
            var toggleSwitch = new ModernWpf.Controls.ToggleSwitch();
            var toggleSwitchWithHeader = new ModernWpf.Controls.ToggleSwitch
            {
                Header = "H"
            };
            var toggleSwitchWithWideHeader = new ModernWpf.Controls.ToggleSwitch
            {
                Header = new Rectangle { Height = 19, Width = 200 }
            };
            var toggleSwitchWithWideOnContent = new ModernWpf.Controls.ToggleSwitch
            {
                OnContent = new Rectangle { Height = 20, Width = 200 }
            };
            var toggleSwitchWithWideOffContent = new ModernWpf.Controls.ToggleSwitch
            {
                OffContent = new Rectangle { Height = 20, Width = 200 }
            };

            root.Children.Add(toggleSwitch);
            root.Children.Add(toggleSwitchWithHeader);
            root.Children.Add(toggleSwitchWithWideHeader);
            root.Children.Add(toggleSwitchWithWideOnContent);
            root.Children.Add(toggleSwitchWithWideOffContent);

            using var host = new TestWindowHost(root, width: 500, height: 320);
            host.UpdateLayout();

            Assert.AreEqual(154d, toggleSwitch.ActualWidth, 0.1);
            Assert.AreEqual(40d, toggleSwitch.ActualHeight, 0.1);

            var thumb = FindNamedDescendant<Thumb>(toggleSwitch, "SwitchThumb");
            Assert.AreEqual(72d, thumb.ActualWidth, 1.0);
            Assert.AreEqual(40d, thumb.ActualHeight, 0.1);

            Assert.AreEqual(154d, toggleSwitchWithHeader.ActualWidth, 0.1);
            Assert.AreEqual(63d, toggleSwitchWithHeader.ActualHeight, 1.0);

            thumb = FindNamedDescendant<Thumb>(toggleSwitchWithHeader, "SwitchThumb");
            Assert.AreEqual(72d, thumb.ActualWidth, 1.0);
            Assert.AreEqual(40d, thumb.ActualHeight, 0.1);

            Assert.AreEqual(200d, toggleSwitchWithWideHeader.ActualWidth, 0.1);
            Assert.AreEqual(63d, toggleSwitchWithWideHeader.ActualHeight, 0.1);

            double expectedWideContentWidth = 200 + 40 + 12;
            Assert.AreEqual(expectedWideContentWidth, toggleSwitchWithWideOnContent.ActualWidth, 0.1);
            Assert.AreEqual(40d, toggleSwitchWithWideOnContent.ActualHeight, 0.1);
            Assert.AreEqual(expectedWideContentWidth, toggleSwitchWithWideOffContent.ActualWidth, 0.1);
            Assert.AreEqual(40d, toggleSwitchWithWideOffContent.ActualHeight, 0.1);
        });
    }

    private static void RaiseDragStarted(Thumb thumb)
    {
        thumb.RaiseEvent(new DragStartedEventArgs(0, 0)
        {
            RoutedEvent = Thumb.DragStartedEvent
        });
    }

    private static void RaiseDragDelta(Thumb thumb, double horizontalChange, double verticalChange = 0)
    {
        thumb.RaiseEvent(new DragDeltaEventArgs(horizontalChange, verticalChange)
        {
            RoutedEvent = Thumb.DragDeltaEvent
        });
    }

    private static void RaiseDragCompleted(Thumb thumb)
    {
        thumb.RaiseEvent(new DragCompletedEventArgs(0, 0, false)
        {
            RoutedEvent = Thumb.DragCompletedEvent
        });
    }

    private static void RaiseThumbPreviewMouseLeftButtonUp(Thumb thumb)
    {
        thumb.RaiseEvent(new MouseButtonEventArgs(
            Mouse.PrimaryDevice,
            Environment.TickCount,
            MouseButton.Left)
        {
            RoutedEvent = UIElement.PreviewMouseLeftButtonUpEvent
        });
    }

    private static KeyEventArgs RaiseKey(UIElement element, RoutedEvent routedEvent, Key key)
    {
        var args = new KeyEventArgs(
            Keyboard.PrimaryDevice,
            PresentationSource.FromVisual(element),
            Environment.TickCount,
            key)
        {
            RoutedEvent = routedEvent
        };

        element.RaiseEvent(args);
        return args;
    }

    private static DataTemplate CreateTextTemplate()
    {
        return (DataTemplate)XamlReader.Parse(
            @"<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>
                <TextBlock Text='{Binding}'/>
            </DataTemplate>");
    }

    private static void AssertBrushEquals(Brush expected, Brush actual)
    {
        Assert.IsNotNull(expected);
        Assert.IsNotNull(actual);

        if (expected is SolidColorBrush expectedSolid && actual is SolidColorBrush actualSolid)
        {
            Assert.AreEqual(expectedSolid.Color, actualSolid.Color);
            Assert.AreEqual(expectedSolid.Opacity, actualSolid.Opacity);
            return;
        }

        Assert.AreEqual(expected.ToString(), actual.ToString());
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

        throw new InvalidOperationException($"Could not find descendant named '{name}'.");
    }

    private static FrameworkElement FindStateGroupsRoot(DependencyObject root)
    {
        foreach (var descendant in VisualTreeTestHelper.EnumerateDescendants(root))
        {
            if (descendant is FrameworkElement element &&
                VisualStateManager.GetVisualStateGroups(element)
                    .OfType<VisualStateGroup>()
                    .Any(group => group.Name == "ContentStates"))
            {
                return element;
            }
        }

        throw new InvalidOperationException("Could not find ToggleSwitch state groups root.");
    }

    private static string GetCurrentStateName(FrameworkElement stateGroupsRoot, string groupName)
    {
        var group = VisualStateManager.GetVisualStateGroups(stateGroupsRoot)
            .OfType<VisualStateGroup>()
            .Single(item => item.Name == groupName);
        Assert.IsNotNull(group.CurrentState);
        return group.CurrentState.Name;
    }

    private static global::ModernWpf.VisualStateEx FindVisualStateEx(
        FrameworkElement stateGroupsRoot,
        string groupName,
        string stateName)
    {
        var group = VisualStateManager.GetVisualStateGroups(stateGroupsRoot)
            .OfType<VisualStateGroup>()
            .Single(item => item.Name == groupName);
        var state = group.States
            .OfType<VisualState>()
            .Single(item => item.Name == stateName);
        Assert.IsInstanceOfType(state, typeof(global::ModernWpf.VisualStateEx));
        return (global::ModernWpf.VisualStateEx)state;
    }

    private static void AssertStateSetter(global::ModernWpf.VisualStateEx state, string target)
    {
        Assert.IsTrue(
            state.Setters.Any(setter => setter.Target == target),
            $"Expected VisualStateEx setter target '{target}'.");
    }

    private static void AssertAffectsMeasure(DependencyProperty property, Type ownerType, bool expected)
    {
        var metadata = (FrameworkPropertyMetadata)property.GetMetadata(ownerType);
        Assert.AreEqual(expected, metadata.AffectsMeasure, $"{property.Name}.AffectsMeasure");
    }

    private static void AssertThemeResourceReference(string themeName, string resourceKey, object expectedResourceKey)
    {
        var themeDictionary = global::ModernWpf.ThemeResources.Current.GetThemeDictionary(themeName);

        Assert.IsTrue(themeDictionary.Contains(resourceKey), $"{themeName} is missing {resourceKey}.");
        Assert.IsTrue(themeDictionary.Contains(expectedResourceKey), $"{themeName} is missing {expectedResourceKey}.");
        Assert.AreSame(themeDictionary[expectedResourceKey], themeDictionary[resourceKey], $"{themeName}:{resourceKey}");
    }

    private static void AssertThemeResourceValue<T>(string themeName, string resourceKey, T expectedValue)
    {
        var themeDictionary = global::ModernWpf.ThemeResources.Current.GetThemeDictionary(themeName);

        Assert.IsTrue(themeDictionary.Contains(resourceKey), $"{themeName} is missing {resourceKey}.");
        Assert.AreEqual(expectedValue, themeDictionary[resourceKey], $"{themeName}:{resourceKey}");
    }

    private static ToggleSwitchAutomationPeer CreatePeer(ModernWpf.Controls.ToggleSwitch toggleSwitch)
    {
        return new ToggleSwitchAutomationPeer(toggleSwitch);
    }

    private sealed class CallbackToggleSwitch : ModernWpf.Controls.ToggleSwitch
    {
        public int HeaderChanges { get; private set; }

        public int OffContentChanges { get; private set; }

        public int OnContentChanges { get; private set; }

        public int ToggleChanges { get; private set; }

        protected override void OnHeaderChanged(object oldContent, object newContent)
        {
            HeaderChanges++;
        }

        protected override void OnOffContentChanged(object oldContent, object newContent)
        {
            OffContentChanges++;
        }

        protected override void OnOnContentChanged(object oldContent, object newContent)
        {
            OnContentChanges++;
        }

        protected override void OnToggled()
        {
            ToggleChanges++;
            base.OnToggled();
        }
    }
}
