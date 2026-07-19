using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ModernWpf.Automation.Peers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;
using WpfAutomation = System.Windows.Automation.Automation;

namespace ModernWpf.WinUI.Tests.ToggleSwitchControl;

[TestClass]
public class ToggleSwitchApiTests
{
    private static readonly (string Key, Color Dark, Color Light)[] ToggleSwitchLegacyThemeBrushColors =
    {
        ("ToggleSwitchCurtainBackgroundThemeBrush", Color.FromRgb(0x57, 0x29, 0xC1), Color.FromRgb(0x46, 0x17, 0xB4)),
        ("ToggleSwitchCurtainDisabledBackgroundThemeBrush", Colors.Transparent, Colors.Transparent),
        ("ToggleSwitchCurtainPointerOverBackgroundThemeBrush", Color.FromRgb(0x6E, 0x46, 0xCA), Color.FromRgb(0x5F, 0x37, 0xBE)),
        ("ToggleSwitchCurtainPressedBackgroundThemeBrush", Color.FromRgb(0x7E, 0x4F, 0xEC), Color.FromRgb(0x72, 0x41, 0xE4)),
        ("ToggleSwitchDisabledForegroundThemeBrush", Color.FromArgb(0x66, 0xFF, 0xFF, 0xFF), Color.FromArgb(0x66, 0x00, 0x00, 0x00)),
        ("ToggleSwitchForegroundThemeBrush", Colors.White, Colors.Black),
        ("ToggleSwitchHeaderDisabledForegroundThemeBrush", Color.FromArgb(0x66, 0xFF, 0xFF, 0xFF), Color.FromArgb(0x66, 0x00, 0x00, 0x00)),
        ("ToggleSwitchHeaderForegroundThemeBrush", Colors.White, Colors.Black),
        ("ToggleSwitchOuterBorderBorderThemeBrush", Color.FromArgb(0x59, 0xFF, 0xFF, 0xFF), Color.FromArgb(0x59, 0x00, 0x00, 0x00)),
        ("ToggleSwitchOuterBorderDisabledBorderThemeBrush", Color.FromArgb(0x33, 0xFF, 0xFF, 0xFF), Color.FromArgb(0x33, 0x00, 0x00, 0x00)),
        ("ToggleSwitchThumbBackgroundThemeBrush", Colors.White, Colors.Black),
        ("ToggleSwitchThumbBorderThemeBrush", Colors.White, Colors.Black),
        ("ToggleSwitchThumbDisabledBackgroundThemeBrush", Color.FromRgb(0x7E, 0x7E, 0x7E), Color.FromRgb(0x92, 0x92, 0x92)),
        ("ToggleSwitchThumbDisabledBorderThemeBrush", Color.FromRgb(0x7E, 0x7E, 0x7E), Color.FromRgb(0x92, 0x92, 0x92)),
        ("ToggleSwitchThumbPointerOverBackgroundThemeBrush", Colors.White, Colors.Black),
        ("ToggleSwitchThumbPointerOverBorderThemeBrush", Colors.White, Colors.Black),
        ("ToggleSwitchThumbPressedBackgroundThemeBrush", Colors.White, Colors.Black),
        ("ToggleSwitchThumbPressedForegroundThemeBrush", Colors.White, Colors.Black),
        ("ToggleSwitchTrackBackgroundThemeBrush", Color.FromArgb(0x42, 0xFF, 0xFF, 0xFF), Color.FromArgb(0x59, 0x00, 0x00, 0x00)),
        ("ToggleSwitchTrackBorderThemeBrush", Colors.Transparent, Colors.Transparent),
        ("ToggleSwitchTrackDisabledBackgroundThemeBrush", Color.FromArgb(0x1F, 0xFF, 0xFF, 0xFF), Color.FromArgb(0x1F, 0x00, 0x00, 0x00)),
        ("ToggleSwitchTrackPointerOverBackgroundThemeBrush", Color.FromArgb(0x4A, 0xFF, 0xFF, 0xFF), Color.FromArgb(0x4A, 0x00, 0x00, 0x00)),
        ("ToggleSwitchTrackPressedBackgroundThemeBrush", Color.FromArgb(0x59, 0xFF, 0xFF, 0xFF), Color.FromArgb(0x42, 0x00, 0x00, 0x00)),
    };

    private static readonly (string Key, string ColorResourceKey)[] ToggleSwitchLegacyHighContrastThemeBrushes =
    {
        ("ToggleSwitchCurtainBackgroundThemeBrush", "SystemColorHighlightColor"),
        ("ToggleSwitchCurtainDisabledBackgroundThemeBrush", "SystemColorButtonFaceColor"),
        ("ToggleSwitchCurtainPointerOverBackgroundThemeBrush", "SystemColorHighlightColor"),
        ("ToggleSwitchCurtainPressedBackgroundThemeBrush", "SystemColorHighlightColor"),
        ("ToggleSwitchDisabledForegroundThemeBrush", "SystemColorGrayTextColor"),
        ("ToggleSwitchForegroundThemeBrush", "SystemColorButtonTextColor"),
        ("ToggleSwitchHeaderDisabledForegroundThemeBrush", "SystemColorGrayTextColor"),
        ("ToggleSwitchHeaderForegroundThemeBrush", "SystemColorButtonTextColor"),
        ("ToggleSwitchOuterBorderBorderThemeBrush", "SystemColorButtonTextColor"),
        ("ToggleSwitchOuterBorderDisabledBorderThemeBrush", "SystemColorGrayTextColor"),
        ("ToggleSwitchThumbBackgroundThemeBrush", "SystemColorButtonTextColor"),
        ("ToggleSwitchThumbBorderThemeBrush", "SystemColorButtonTextColor"),
        ("ToggleSwitchThumbDisabledBackgroundThemeBrush", "SystemColorGrayTextColor"),
        ("ToggleSwitchThumbDisabledBorderThemeBrush", "SystemColorGrayTextColor"),
        ("ToggleSwitchThumbPointerOverBackgroundThemeBrush", "SystemColorHighlightColor"),
        ("ToggleSwitchThumbPointerOverBorderThemeBrush", "SystemColorButtonTextColor"),
        ("ToggleSwitchThumbPressedBackgroundThemeBrush", "SystemColorButtonFaceColor"),
        ("ToggleSwitchThumbPressedForegroundThemeBrush", "SystemColorButtonTextColor"),
        ("ToggleSwitchTrackBackgroundThemeBrush", "SystemColorButtonFaceColor"),
        ("ToggleSwitchTrackDisabledBackgroundThemeBrush", "SystemColorButtonFaceColor"),
        ("ToggleSwitchTrackPointerOverBackgroundThemeBrush", "SystemColorButtonFaceColor"),
        ("ToggleSwitchTrackPressedBackgroundThemeBrush", "SystemColorButtonFaceColor"),
    };

    [TestMethod]
    public void CanInstantiateAndEnterLeaveLiveTreeLikeWinUINativeTests()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var toggleSwitch = new ModernWpf.Controls.ToggleSwitch();
            Assert.IsNotNull(toggleSwitch.TemplateSettings);

            var root = new StackPanel();
            using var host = new TestWindowHost(root, width: 260, height: 160);

            root.Children.Add(toggleSwitch);
            host.UpdateLayout();

            Assert.AreSame(root, toggleSwitch.Parent);
            Assert.IsNotNull(FindNamedDescendant<Thumb>(toggleSwitch, "SwitchThumb"));
            Assert.IsNotNull(FindNamedDescendant<FrameworkElement>(toggleSwitch, "SwitchKnob"));
            Assert.IsNotNull(FindNamedDescendant<FrameworkElement>(toggleSwitch, "SwitchKnobBounds"));

            root.Children.Remove(toggleSwitch);
            host.UpdateLayout();

            Assert.IsNull(toggleSwitch.Parent);

            root.Children.Add(toggleSwitch);
            host.UpdateLayout();

            Assert.AreSame(root, toggleSwitch.Parent);
            Assert.IsNotNull(FindNamedDescendant<Thumb>(toggleSwitch, "SwitchThumb"));
        });
    }

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
    public void HorizontalDragTogglesOnAndOffLikeWinUISource()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var toggleSwitch = new ModernWpf.Controls.ToggleSwitch();
            using var host = new TestWindowHost(toggleSwitch, width: 260, height: 120);
            host.UpdateLayout();

            var thumb = FindNamedDescendant<Thumb>(toggleSwitch, "SwitchThumb");

            RaiseDragStarted(thumb);
            RaiseDragDelta(thumb, 12);
            RaiseDragCompleted(thumb);
            host.UpdateLayout();

            Assert.IsTrue(toggleSwitch.IsOn);

            RaiseDragStarted(thumb);
            RaiseDragDelta(thumb, -12);
            RaiseDragCompleted(thumb);
            host.UpdateLayout();

            Assert.IsFalse(toggleSwitch.IsOn);
        });
    }

    [TestMethod]
    public void ManipulationStartingUsesWinUITranslateXModeSubstitute()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var toggleSwitch = new ManipulationProbeToggleSwitch();
            var args = CreateManipulationStartingArgs();

            toggleSwitch.InvokeManipulationStarting(args);

            Assert.AreEqual(ManipulationModes.TranslateX, args.Mode);
            Assert.AreSame(toggleSwitch, args.ManipulationContainer);
        });
    }

    [TestMethod]
    public void HorizontalManipulationTogglesOnAndOffLikeWinUIPanSource()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var toggleSwitch = new ManipulationProbeToggleSwitch();
            using var host = new TestWindowHost(toggleSwitch, width: 260, height: 120);
            host.UpdateLayout();

            toggleSwitch.InvokeManipulationStarted(CreateManipulationStartedArgs(toggleSwitch));
            var deltaOn = CreateManipulationDeltaArgs(toggleSwitch, horizontalChange: 12, verticalChange: 5);
            toggleSwitch.InvokeManipulationDelta(deltaOn);
            toggleSwitch.InvokeManipulationCompleted(CreateManipulationCompletedArgs(toggleSwitch));
            host.UpdateLayout();

            Assert.IsTrue(deltaOn.Handled);
            Assert.IsTrue(toggleSwitch.IsOn);

            toggleSwitch.InvokeManipulationStarted(CreateManipulationStartedArgs(toggleSwitch));
            var deltaOff = CreateManipulationDeltaArgs(toggleSwitch, horizontalChange: -12, verticalChange: 5);
            toggleSwitch.InvokeManipulationDelta(deltaOff);
            toggleSwitch.InvokeManipulationCompleted(CreateManipulationCompletedArgs(toggleSwitch));
            host.UpdateLayout();

            Assert.IsTrue(deltaOff.Handled);
            Assert.IsFalse(toggleSwitch.IsOn);
        });
    }

    [TestMethod]
    public void VerticalManipulationDoesNotToggleLikeWinUISource()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var toggleSwitch = new ManipulationProbeToggleSwitch();
            using var host = new TestWindowHost(toggleSwitch, width: 260, height: 120);
            host.UpdateLayout();

            toggleSwitch.InvokeManipulationStarted(CreateManipulationStartedArgs(toggleSwitch));
            var delta = CreateManipulationDeltaArgs(toggleSwitch, horizontalChange: 0, verticalChange: 24);
            toggleSwitch.InvokeManipulationDelta(delta);
            toggleSwitch.InvokeManipulationCompleted(CreateManipulationCompletedArgs(toggleSwitch));
            host.UpdateLayout();

            Assert.IsFalse(delta.Handled);
            Assert.IsFalse(toggleSwitch.IsOn);
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
    public void PointerCaptureLostClearsPointerOverAfterVerticalPanLikeWinUISource()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var toggleSwitch = new ModernWpf.Controls.ToggleSwitch();
            using var host = new TestWindowHost(toggleSwitch, width: 260, height: 120);
            host.UpdateLayout();

            var thumb = FindNamedDescendant<Thumb>(toggleSwitch, "SwitchThumb");
            var stateGroupsRoot = FindStateGroupsRoot(toggleSwitch);

            RaiseMouseEnter(toggleSwitch);
            host.UpdateLayout();

            Assert.AreEqual("PointerOver", GetCurrentStateName(stateGroupsRoot, "CommonStates"));

            RaiseDragStarted(thumb);
            RaiseDragDelta(thumb, horizontalChange: 0, verticalChange: 24);
            RaiseDragCompleted(thumb);
            host.UpdateLayout();

            Assert.AreEqual("PointerOver", GetCurrentStateName(stateGroupsRoot, "CommonStates"));

            RaiseLostMouseCapture(thumb);
            host.UpdateLayout();

            Assert.AreEqual("Normal", GetCurrentStateName(stateGroupsRoot, "CommonStates"));
        });
    }

    [TestMethod]
    public void IsEnabledFalseClearsTransientStatesLikeWinUISource()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var toggleSwitch = new ModernWpf.Controls.ToggleSwitch();
            using var host = new TestWindowHost(toggleSwitch, width: 260, height: 120);
            host.UpdateLayout();

            var thumb = FindNamedDescendant<Thumb>(toggleSwitch, "SwitchThumb");
            var stateGroupsRoot = FindStateGroupsRoot(toggleSwitch);

            RaiseMouseEnter(toggleSwitch);
            RaiseDragStarted(thumb);
            RaiseDragDelta(thumb, 6);
            host.UpdateLayout();

            Assert.AreEqual("Pressed", GetCurrentStateName(stateGroupsRoot, "CommonStates"));
            Assert.AreEqual("Dragging", GetCurrentStateName(stateGroupsRoot, "ToggleStates"));

            toggleSwitch.IsEnabled = false;
            host.UpdateLayout();

            Assert.IsFalse(toggleSwitch.IsOn);
            Assert.AreEqual("Disabled", GetCurrentStateName(stateGroupsRoot, "CommonStates"));
            Assert.AreEqual("Off", GetCurrentStateName(stateGroupsRoot, "ToggleStates"));

            toggleSwitch.IsEnabled = true;
            host.UpdateLayout();

            Assert.AreEqual("Normal", GetCurrentStateName(stateGroupsRoot, "CommonStates"));
            Assert.AreEqual("Off", GetCurrentStateName(stateGroupsRoot, "ToggleStates"));
        });
    }

    [TestMethod]
    public void VisibilityNonVisibleClearsTransientStatesLikeWinUISource()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var toggleSwitch = new ModernWpf.Controls.ToggleSwitch();
            using var host = new TestWindowHost(toggleSwitch, width: 260, height: 120);
            host.UpdateLayout();

            var thumb = FindNamedDescendant<Thumb>(toggleSwitch, "SwitchThumb");
            var stateGroupsRoot = FindStateGroupsRoot(toggleSwitch);

            RaiseMouseEnter(toggleSwitch);
            RaiseDragStarted(thumb);
            RaiseDragDelta(thumb, 6);
            host.UpdateLayout();

            Assert.AreEqual("Pressed", GetCurrentStateName(stateGroupsRoot, "CommonStates"));
            Assert.AreEqual("Dragging", GetCurrentStateName(stateGroupsRoot, "ToggleStates"));

            toggleSwitch.Visibility = Visibility.Collapsed;
            host.UpdateLayout();

            Assert.IsFalse(toggleSwitch.IsOn);
            Assert.AreEqual("Normal", GetCurrentStateName(stateGroupsRoot, "CommonStates"));
            Assert.AreEqual("Off", GetCurrentStateName(stateGroupsRoot, "ToggleStates"));

            toggleSwitch.Visibility = Visibility.Visible;
            host.UpdateLayout();

            Assert.AreEqual("Normal", GetCurrentStateName(stateGroupsRoot, "CommonStates"));
            Assert.AreEqual("Off", GetCurrentStateName(stateGroupsRoot, "ToggleStates"));
        });
    }

    [TestMethod]
    public void CanceledDragCompletionLeavesDragStateLikeWinUISource()
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
            RaiseDragDelta(thumb, 6);
            var completed = RaiseDragCompleted(thumb, canceled: true);
            host.UpdateLayout();

            Assert.IsFalse(completed.Handled);
            Assert.IsFalse(toggleSwitch.IsOn);
            Assert.AreEqual("Pressed", GetCurrentStateName(stateGroupsRoot, "CommonStates"));
            Assert.AreEqual("Dragging", GetCurrentStateName(stateGroupsRoot, "ToggleStates"));
            Assert.AreEqual(6d, toggleSwitch.TemplateSettings.KnobCurrentToOffOffset, 0.1);
            Assert.AreEqual(-14d, toggleSwitch.TemplateSettings.KnobCurrentToOnOffset, 0.1);
        });
    }

    [TestMethod]
    public void DragRoutedEventsRemainUnhandledLikeWinUISource()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var root = new StackPanel();
            var toggleSwitch = new ModernWpf.Controls.ToggleSwitch();
            root.Children.Add(toggleSwitch);
            using var host = new TestWindowHost(root, width: 260, height: 120);
            host.UpdateLayout();

            var thumb = FindNamedDescendant<Thumb>(toggleSwitch, "SwitchThumb");
            int startedCount = 0;
            int deltaCount = 0;
            int completedCount = 0;

            root.AddHandler(Thumb.DragStartedEvent, new DragStartedEventHandler((sender, args) =>
            {
                startedCount++;
                Assert.IsFalse(args.Handled);
            }));
            root.AddHandler(Thumb.DragDeltaEvent, new DragDeltaEventHandler((sender, args) =>
            {
                deltaCount++;
                Assert.IsFalse(args.Handled);
            }));
            root.AddHandler(Thumb.DragCompletedEvent, new DragCompletedEventHandler((sender, args) =>
            {
                completedCount++;
                Assert.IsFalse(args.Handled);
            }));

            var started = RaiseDragStarted(thumb);
            var delta = RaiseDragDelta(thumb, 3);
            var completed = RaiseDragCompleted(thumb);

            Assert.IsFalse(started.Handled);
            Assert.IsFalse(delta.Handled);
            Assert.IsFalse(completed.Handled);
            Assert.AreEqual(1, startedCount);
            Assert.AreEqual(1, deltaCount);
            Assert.AreEqual(1, completedCount);
            Assert.IsFalse(toggleSwitch.IsOn);
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
            WaitForVisualStateAnimations();

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
    public void SwitchAreaGridColorAnimationsTargetWpfBorderSubstitute()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var toggleSwitch = new ModernWpf.Controls.ToggleSwitch();
            using var host = new TestWindowHost(toggleSwitch, width: 260, height: 120);
            host.UpdateLayout();

            var stateGroupsRoot = FindStateGroupsRoot(toggleSwitch);
            foreach (var stateName in new[] { "Normal", "PointerOver", "Pressed", "Disabled" })
            {
                AssertSwitchAreaGridBackgroundColorAnimation(FindVisualState(stateGroupsRoot, "CommonStates", stateName));
            }
        });
    }

    [TestMethod]
    public void TemplateGeometryMatchesWinUICommonStylesSource()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var toggleSwitch = new ModernWpf.Controls.ToggleSwitch();
            using var host = new TestWindowHost(toggleSwitch, width: 260, height: 120);
            host.UpdateLayout();

            var outerBorder = FindNamedDescendant<Rectangle>(toggleSwitch, "OuterBorder");
            var switchKnobBounds = FindNamedDescendant<Rectangle>(toggleSwitch, "SwitchKnobBounds");
            var switchKnob = FindNamedDescendant<Grid>(toggleSwitch, "SwitchKnob");
            var switchKnobOn = FindNamedDescendant<Border>(toggleSwitch, "SwitchKnobOn");
            var switchKnobOff = FindNamedDescendant<Rectangle>(toggleSwitch, "SwitchKnobOff");

            Assert.AreEqual(40d, outerBorder.Width, 0.1);
            Assert.AreEqual(20d, outerBorder.Height, 0.1);
            Assert.AreEqual(40d, switchKnobBounds.Width, 0.1);
            Assert.AreEqual(20d, switchKnobBounds.Height, 0.1);
            Assert.AreEqual(20d, switchKnob.Width, 0.1);
            Assert.AreEqual(20d, switchKnob.Height, 0.1);

            Assert.AreEqual(12d, switchKnobOn.Width, 0.1);
            Assert.AreEqual(12d, switchKnobOn.Height, 0.1);
            Assert.IsInstanceOfType(switchKnobOn, typeof(BorderEx));
            Assert.AreEqual(BackgroundSizing.OuterBorderEdge, ((BorderEx)switchKnobOn).BackgroundSizing);
            Assert.AreEqual(new CornerRadius(7), switchKnobOn.CornerRadius);
            Assert.AreEqual(HorizontalAlignment.Center, switchKnobOn.HorizontalAlignment);
            Assert.AreEqual(new Thickness(0, 0, 1, 0), switchKnobOn.Margin);

            Assert.AreEqual(12d, switchKnobOff.Width, 0.1);
            Assert.AreEqual(12d, switchKnobOff.Height, 0.1);
            Assert.AreEqual(7d, switchKnobOff.RadiusX, 0.1);
            Assert.AreEqual(7d, switchKnobOff.RadiusY, 0.1);
            Assert.AreEqual(HorizontalAlignment.Center, switchKnobOff.HorizontalAlignment);
            Assert.AreEqual(new Thickness(-1, 0, 0, 0), switchKnobOff.Margin);

            var stateGroupsRoot = FindStateGroupsRoot(toggleSwitch);
            AssertKnobSizeAnimations(FindVisualState(stateGroupsRoot, "CommonStates", "Normal"), 12, 12);
            AssertKnobSizeAnimations(FindVisualState(stateGroupsRoot, "CommonStates", "PointerOver"), 14, 14);
            AssertKnobSizeAnimations(FindVisualState(stateGroupsRoot, "CommonStates", "Pressed"), 17, 14);
            AssertKnobSizeAnimations(FindVisualState(stateGroupsRoot, "CommonStates", "Disabled"), 12, 12);

            var offState = FindVisualState(stateGroupsRoot, "ToggleStates", "Off");
            Assert.IsTrue(
                offState.Storyboard == null || offState.Storyboard.Children.Count == 0,
                "WinUI CommonStyles leaves the Off visual state empty; repositioning is handled by transitions/default transform.");
        });
    }

    [TestMethod]
    public void ThemeResourcesUseWinUI3ToggleSwitchColorAliases()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            foreach (var themeName in new[] { "Light", "Dark" })
            {
                AssertThemeResourceValue(themeName, "SystemControlTransparentColor", Color.FromArgb(0x00, 0x00, 0x00, 0x00));
                AssertThemeResourceReferences(themeName,
                    ("ToggleSwitchContainerBackground", "SubtleFillColorTransparentBrush"),
                    ("ToggleSwitchContainerBackgroundPointerOver", "SubtleFillColorTransparent"),
                    ("ToggleSwitchContainerBackgroundPressed", "SubtleFillColorTransparent"),
                    ("ToggleSwitchContainerBackgroundDisabled", "SubtleFillColorTransparent"),
                    ("ToggleSwitchFillOff", "ControlAltFillColorSecondaryBrush"),
                    ("ToggleSwitchFillOffPointerOver", "ControlAltFillColorTertiary"),
                    ("ToggleSwitchFillOffPressed", "ControlAltFillColorQuarternary"),
                    ("ToggleSwitchFillOffDisabled", "ControlAltFillColorDisabled"),
                    ("ToggleSwitchStrokeOff", "ControlStrongStrokeColorDefaultBrush"),
                    ("ToggleSwitchStrokeOffPointerOver", "ControlStrongStrokeColorDefault"),
                    ("ToggleSwitchStrokeOffPressed", "ControlStrongStrokeColorDefault"),
                    ("ToggleSwitchStrokeOffDisabled", "ControlStrongStrokeColorDisabled"),
                    ("ToggleSwitchFillOn", "AccentFillColorDefaultBrush"),
                    ("ToggleSwitchFillOnPointerOver", "AccentFillColorSecondaryBrush"),
                    ("ToggleSwitchFillOnPressed", "AccentFillColorTertiaryBrush"),
                    ("ToggleSwitchFillOnDisabled", "AccentFillColorDisabledBrush"),
                    ("ToggleSwitchStrokeOn", "AccentFillColorDefaultBrush"),
                    ("ToggleSwitchStrokeOnPointerOver", "AccentFillColorSecondaryBrush"),
                    ("ToggleSwitchStrokeOnPressed", "AccentFillColorTertiaryBrush"),
                    ("ToggleSwitchStrokeOnDisabled", "AccentFillColorDisabledBrush"),
                    ("ToggleSwitchKnobFillOff", "TextFillColorSecondaryBrush"),
                    ("ToggleSwitchKnobFillOffPointerOver", "TextFillColorSecondaryBrush"),
                    ("ToggleSwitchKnobFillOffPressed", "TextFillColorSecondaryBrush"),
                    ("ToggleSwitchKnobFillOffDisabled", "TextFillColorDisabledBrush"),
                    ("ToggleSwitchKnobFillOn", "TextOnAccentFillColorPrimaryBrush"),
                    ("ToggleSwitchKnobFillOnPointerOver", "TextOnAccentFillColorPrimaryBrush"),
                    ("ToggleSwitchKnobFillOnPressed", "TextOnAccentFillColorPrimaryBrush"),
                    ("ToggleSwitchKnobFillOnDisabled", "TextOnAccentFillColorDisabledBrush"),
                    ("ToggleSwitchKnobStrokeOn", "CircleElevationBorderBrush"));
            }
        });
    }

    private static void WaitForVisualStateAnimations()
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(250);
        while (DateTime.UtcNow < deadline)
        {
            Thread.Sleep(10);
            WpfTestHost.DoEvents();
        }
    }

    [TestMethod]
    public void HighContrastThemeResourcesUseWinUI3ToggleSwitchAliases()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

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
        });
    }

    [TestMethod]
    public void LegacyThemeBrushResourcesMatchWinUICommonStylesSource()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            foreach (var (key, dark, light) in ToggleSwitchLegacyThemeBrushColors)
            {
                AssertThemeSolidColorBrushValue("Dark", key, dark);
                AssertThemeSolidColorBrushValue("Light", key, light);
            }

            foreach (var (key, colorResourceKey) in ToggleSwitchLegacyHighContrastThemeBrushes)
            {
                AssertThemeSolidColorBrushColorReference("HighContrast", key, colorResourceKey);
            }

            AssertThemeSolidColorBrushValue("HighContrast", "ToggleSwitchTrackBorderThemeBrush", Colors.Transparent);
        });
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
    public void DefaultTemplateOmitsHeaderStatesLikeWinUI3Theme()
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
            Assert.IsFalse(
                VisualStateManager.GetVisualStateGroups(stateGroupsRoot)
                    .OfType<VisualStateGroup>()
                    .Any(group => group.Name == "HeaderStates"));

            var headerPresenter = FindNamedDescendant<ContentPresenterEx>(toggleSwitch, "HeaderContentPresenter");
            Assert.AreEqual(ControlHeaderPlacement.Top, toggleSwitch.HeaderPlacement);
            Assert.AreEqual(0, Grid.GetRow(headerPresenter));
            Assert.AreEqual(0, Grid.GetColumn(headerPresenter));
            Assert.AreEqual(1, Grid.GetColumnSpan(headerPresenter));
            Assert.AreEqual((Thickness)headerPresenter.TryFindResource("ToggleSwitchTopHeaderMargin"), headerPresenter.Margin);
            Assert.AreEqual(double.PositiveInfinity, headerPresenter.MaxWidth);

            toggleSwitch.HeaderPlacement = ControlHeaderPlacement.Left;
            host.UpdateLayout();

            Assert.AreEqual(0, Grid.GetRow(headerPresenter));
            Assert.AreEqual(0, Grid.GetColumn(headerPresenter));
            Assert.AreEqual(1, Grid.GetColumnSpan(headerPresenter));
            Assert.AreEqual((Thickness)headerPresenter.TryFindResource("ToggleSwitchTopHeaderMargin"), headerPresenter.Margin);
            Assert.AreEqual(double.PositiveInfinity, headerPresenter.MaxWidth);
        });
    }

    [TestMethod]
    public void HeaderPlacementDispatchesWinUIStateNamesForCustomTemplates()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var toggleSwitch = new ModernWpf.Controls.ToggleSwitch
            {
                Template = CreateFocusStateTemplate()
            };
            using var host = new TestWindowHost(toggleSwitch, width: 500, height: 160);
            host.UpdateLayout();

            var stateGroupsRoot = FindStateGroupsRoot(toggleSwitch);
            Assert.AreEqual("TopHeader", GetCurrentStateName(stateGroupsRoot, "HeaderStates"));

            toggleSwitch.HeaderPlacement = ControlHeaderPlacement.Left;
            host.UpdateLayout();

            Assert.AreEqual("LeftHeader", GetCurrentStateName(stateGroupsRoot, "HeaderStates"));
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
    public void TemplateSettingsTrackWinUISizeChangedKnobOffsets()
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

            var knob = FindNamedDescendant<FrameworkElement>(toggleSwitch, "SwitchKnob");
            var knobBounds = FindNamedDescendant<FrameworkElement>(toggleSwitch, "SwitchKnobBounds");
            knobBounds.Width = knobBounds.ActualWidth + 12d;
            host.UpdateLayout();

            double expectedKnobTranslation = knobBounds.ActualWidth - knob.ActualWidth;
            if (knob.Margin.Left < 0)
            {
                expectedKnobTranslation -= knob.Margin.Left;
            }

            if (knob.Margin.Right < 0)
            {
                expectedKnobTranslation -= knob.Margin.Right;
            }

            Assert.AreEqual(-expectedKnobTranslation, settings.KnobOffToOnOffset, 0.1);
            Assert.AreEqual(expectedKnobTranslation, settings.KnobOnToOffOffset, 0.1);

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
            Assert.AreEqual(6d - expectedKnobTranslation, settings.KnobCurrentToOnOffset, 0.1);
        });
    }

    [TestMethod]
    public void TemplatePartDiscoveryUsesSwitchKnobRenderTransformLikeWinUI()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var toggleSwitch = new ModernWpf.Controls.ToggleSwitch
            {
                Template = CreateRenderTransformPartDiscoveryTemplate()
            };
            using var host = new TestWindowHost(toggleSwitch, width: 160, height: 100);
            host.UpdateLayout();

            var knob = FindNamedDescendant<FrameworkElement>(toggleSwitch, "SwitchKnob");
            var knobBounds = FindNamedDescendant<FrameworkElement>(toggleSwitch, "SwitchKnobBounds");
            Assert.IsInstanceOfType(knob.RenderTransform, typeof(TranslateTransform));
            var knobTransform = (TranslateTransform)knob.RenderTransform;
            Assert.IsFalse(knobTransform.IsFrozen);

            knobBounds.Width = 48d;
            host.UpdateLayout();

            ToggleSwitchTemplateSettings settings = toggleSwitch.TemplateSettings;
            Assert.AreEqual(-28d, settings.KnobOffToOnOffset, 0.1);
            Assert.AreEqual(28d, settings.KnobOnToOffOffset, 0.1);

            var thumb = FindNamedDescendant<Thumb>(toggleSwitch, "SwitchThumb");
            RaiseDragStarted(thumb);
            RaiseDragDelta(thumb, 6d);

            Assert.AreEqual(6d, knobTransform.X, 0.1);
            Assert.AreEqual(6d, settings.KnobCurrentToOffOffset, 0.1);
            Assert.AreEqual(-22d, settings.KnobCurrentToOnOffset, 0.1);
        });
    }

    [TestMethod]
    public void FocusableDefaultsToWinUIControlBehavior()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var toggleSwitch = new ModernWpf.Controls.ToggleSwitch();
            using var host = new TestWindowHost(toggleSwitch, width: 260, height: 120);
            host.UpdateLayout();

            Assert.IsTrue(toggleSwitch.Focusable);
            Assert.IsTrue(toggleSwitch.UseSystemFocusVisuals);
            Assert.AreEqual(new Thickness(-7, -3, -7, -3), toggleSwitch.FocusVisualMargin);
            Assert.IsNotNull(toggleSwitch.FocusVisualStyle);
            Assert.IsTrue(toggleSwitch.Focus());
            Assert.IsTrue(toggleSwitch.IsKeyboardFocusWithin);
        });
    }

    [TestMethod]
    public void DefaultStyleSettersMatchWinUICommonStylesSource()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var toggleSwitch = new ModernWpf.Controls.ToggleSwitch();
            using var host = new TestWindowHost(toggleSwitch, width: 260, height: 120);
            host.UpdateLayout();

            Assert.AreEqual(HorizontalAlignment.Left, toggleSwitch.HorizontalContentAlignment);
            Assert.AreEqual(
                BaseValueSource.DefaultStyle,
                DependencyPropertyHelper.GetValueSource(toggleSwitch, Control.HorizontalContentAlignmentProperty).BaseValueSource);

            Assert.IsTrue(toggleSwitch.IsManipulationEnabled);
            Assert.AreEqual(
                BaseValueSource.DefaultStyle,
                DependencyPropertyHelper.GetValueSource(toggleSwitch, UIElement.IsManipulationEnabledProperty).BaseValueSource);

            Assert.AreEqual(VerticalAlignment.Center, toggleSwitch.VerticalContentAlignment);
            Assert.AreEqual(
                BaseValueSource.DefaultStyle,
                DependencyPropertyHelper.GetValueSource(toggleSwitch, Control.VerticalContentAlignmentProperty).BaseValueSource);
        });
    }

    [TestMethod]
    public void ToggleSwitchStyleUsesWinUIResourceAliases()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var style = (Style)Application.Current.FindResource(typeof(ModernWpf.Controls.ToggleSwitch));
            var toggleSwitch = new ModernWpf.Controls.ToggleSwitch
            {
                Header = "Header text",
                OffContent = "Off text",
                OnContent = "On text"
            };

            using var host = new TestWindowHost(toggleSwitch, width: 260, height: 120);
            host.UpdateLayout();

            Assert.AreEqual(typeof(ModernWpf.Controls.ToggleSwitch), style.TargetType);
            AssertDynamicResourceSetter(style, Control.ForegroundProperty, "ToggleSwitchContentForeground");
            AssertSetterValue(style, FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Left);
            AssertSetterValue(style, FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
            AssertSetterValue(style, Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Left);
            AssertSetterValue(style, Control.VerticalContentAlignmentProperty, VerticalAlignment.Center);
            AssertDynamicResourceSetter(style, Control.FontFamilyProperty, "ContentControlThemeFontFamily");
            AssertDynamicResourceSetter(style, Control.FontSizeProperty, "ControlContentThemeFontSize");
            AssertSetterValue(style, UIElement.IsManipulationEnabledProperty, true);
            AssertDynamicResourceSetter(style, FrameworkElement.MinWidthProperty, "ToggleSwitchThemeMinWidth");
            AssertDynamicResourceSetter(style, ModernWpf.Controls.ToggleSwitch.UseSystemFocusVisualsProperty, "UseSystemFocusVisuals");
            AssertDynamicResourceSetter(style, Control.FocusVisualStyleProperty, SystemParameters.FocusVisualStyleKey);
            AssertSetterValue(style, ModernWpf.Controls.ToggleSwitch.FocusVisualMarginProperty, new Thickness(-7, -3, -7, -3));
            AssertDynamicResourceSetter(style, ModernWpf.Controls.ToggleSwitch.CornerRadiusProperty, "ControlCornerRadius");

            AssertBrushEquals((Brush)toggleSwitch.TryFindResource("ToggleSwitchContentForeground"), toggleSwitch.Foreground);
            Assert.AreEqual(HorizontalAlignment.Left, toggleSwitch.HorizontalAlignment);
            Assert.AreEqual(VerticalAlignment.Center, toggleSwitch.VerticalAlignment);
            Assert.AreEqual(HorizontalAlignment.Left, toggleSwitch.HorizontalContentAlignment);
            Assert.AreEqual(VerticalAlignment.Center, toggleSwitch.VerticalContentAlignment);
            Assert.AreSame(toggleSwitch.TryFindResource("ContentControlThemeFontFamily"), toggleSwitch.FontFamily);
            Assert.AreEqual(toggleSwitch.TryFindResource("ControlContentThemeFontSize"), toggleSwitch.FontSize);
            Assert.IsTrue(toggleSwitch.IsManipulationEnabled);
            Assert.AreEqual(toggleSwitch.TryFindResource("ToggleSwitchThemeMinWidth"), toggleSwitch.MinWidth);
            Assert.AreEqual(toggleSwitch.TryFindResource("UseSystemFocusVisuals"), toggleSwitch.UseSystemFocusVisuals);
            Assert.AreEqual(new Thickness(-7, -3, -7, -3), toggleSwitch.FocusVisualMargin);
            Assert.AreEqual(toggleSwitch.TryFindResource("ControlCornerRadius"), toggleSwitch.CornerRadius);

            var stateGroupsRoot = FindStateGroupsRoot(toggleSwitch);
            var normalState = FindVisualStateEx(stateGroupsRoot, "CommonStates", "Normal");
            var pointerOverState = FindVisualStateEx(stateGroupsRoot, "CommonStates", "PointerOver");
            var disabledState = FindVisualStateEx(stateGroupsRoot, "CommonStates", "Disabled");
            AssertStateSetterDynamicResource(normalState, "SwitchKnobOff.Fill", "ToggleSwitchKnobFillOff");
            AssertStateSetterDynamicResource(normalState, "SwitchKnobBounds.Stroke", "ToggleSwitchStrokeOn");
            AssertStateSetterDynamicResource(pointerOverState, "SwitchKnobBounds.Fill", "ToggleSwitchFillOnPointerOver");
            AssertStateSetterDynamicResource(disabledState, "HeaderContentPresenter.Foreground", "ToggleSwitchHeaderForegroundDisabled");
            AssertStateSetterDynamicResource(disabledState, "SwitchKnobOn.Background", "ToggleSwitchKnobFillOnDisabled");

            var headerPresenter = FindNamedDescendant<ContentPresenterEx>(toggleSwitch, "HeaderContentPresenter");
            var offPresenter = FindNamedDescendant<ContentPresenterEx>(toggleSwitch, "OffContentPresenter");
            var onPresenter = FindNamedDescendant<ContentPresenterEx>(toggleSwitch, "OnContentPresenter");
            var switchAreaGrid = FindNamedDescendant<Border>(toggleSwitch, "SwitchAreaGrid");
            var outerBorder = FindNamedDescendant<Rectangle>(toggleSwitch, "OuterBorder");
            var switchKnobBounds = FindNamedDescendant<Rectangle>(toggleSwitch, "SwitchKnobBounds");
            var switchKnobOn = FindNamedDescendant<Border>(toggleSwitch, "SwitchKnobOn");
            var switchKnobOff = FindNamedDescendant<Rectangle>(toggleSwitch, "SwitchKnobOff");

            AssertBrushEquals((Brush)headerPresenter.TryFindResource("ToggleSwitchHeaderForeground"), headerPresenter.Foreground);
            Assert.AreEqual(headerPresenter.TryFindResource("ToggleSwitchTopHeaderMargin"), headerPresenter.Margin);
            AssertBrushEquals(toggleSwitch.Foreground, offPresenter.Foreground);
            AssertBrushEquals(toggleSwitch.Foreground, onPresenter.Foreground);
            Assert.AreEqual(toggleSwitch.HorizontalContentAlignment, offPresenter.HorizontalAlignment);
            Assert.AreEqual(toggleSwitch.HorizontalContentAlignment, onPresenter.HorizontalAlignment);
            Assert.AreEqual(toggleSwitch.VerticalContentAlignment, offPresenter.VerticalAlignment);
            Assert.AreEqual(toggleSwitch.VerticalContentAlignment, onPresenter.VerticalAlignment);
            Assert.AreEqual(toggleSwitch.CornerRadius, switchAreaGrid.CornerRadius);
            AssertBrushEquals((Brush)switchAreaGrid.TryFindResource("ToggleSwitchContainerBackground"), switchAreaGrid.Background);
            AssertBrushEquals((Brush)outerBorder.TryFindResource("ToggleSwitchFillOff"), outerBorder.Fill);
            AssertBrushEquals((Brush)outerBorder.TryFindResource("ToggleSwitchStrokeOff"), outerBorder.Stroke);
            Assert.AreEqual(outerBorder.TryFindResource("ToggleSwitchOuterBorderStrokeThickness"), outerBorder.StrokeThickness);
            AssertBrushEquals((Brush)switchKnobBounds.TryFindResource("ToggleSwitchFillOn"), switchKnobBounds.Fill);
            AssertBrushEquals((Brush)switchKnobBounds.TryFindResource("ToggleSwitchStrokeOn"), switchKnobBounds.Stroke);
            Assert.AreEqual(switchKnobBounds.TryFindResource("ToggleSwitchOnStrokeThickness"), switchKnobBounds.StrokeThickness);
            AssertBrushEquals((Brush)switchKnobOn.TryFindResource("ToggleSwitchKnobFillOn"), switchKnobOn.Background);
            AssertBrushEquals((Brush)switchKnobOn.TryFindResource("ToggleSwitchKnobStrokeOn"), switchKnobOn.BorderBrush);
            AssertBrushEquals((Brush)switchKnobOff.TryFindResource("ToggleSwitchKnobFillOff"), switchKnobOff.Fill);

            var switchAreaLayout = (Grid)switchAreaGrid.Parent;
            Assert.AreEqual(toggleSwitch.TryFindResource("ToggleSwitchPreContentMargin"), RowDefinitionHelper.GetPixelHeight(switchAreaLayout.RowDefinitions[0]));
            Assert.AreEqual(toggleSwitch.TryFindResource("ToggleSwitchPostContentMargin"), RowDefinitionHelper.GetPixelHeight(switchAreaLayout.RowDefinitions[2]));
        });
    }

    [TestMethod]
    public void FocusStatesDistinguishKeyboardAndPointerFocus()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var root = new StackPanel();
            var toggleSwitch = new ModernWpf.Controls.ToggleSwitch
            {
                Template = CreateFocusStateTemplate()
            };
            var other = new Button
            {
                Content = "Other"
            };

            root.Children.Add(toggleSwitch);
            root.Children.Add(other);

            using var host = new TestWindowHost(root, width: 260, height: 120);
            host.UpdateLayout();

            var stateGroupsRoot = FindStateGroupsRoot(toggleSwitch);
            Assert.AreEqual("Unfocused", GetCurrentStateName(stateGroupsRoot, "FocusStates"));

            Assert.IsTrue(toggleSwitch.Focus());
            host.UpdateLayout();
            Assert.AreEqual("Focused", GetCurrentStateName(stateGroupsRoot, "FocusStates"));

            var focusedMouseDown = RaiseMouseLeftButtonDown(toggleSwitch);
            host.UpdateLayout();
            Assert.IsFalse(focusedMouseDown.Handled);
            Assert.AreEqual("PointerFocused", GetCurrentStateName(stateGroupsRoot, "FocusStates"));

            Assert.IsTrue(other.Focus());
            host.UpdateLayout();
            Assert.AreEqual("Unfocused", GetCurrentStateName(stateGroupsRoot, "FocusStates"));

            var mouseDown = RaiseMouseLeftButtonDown(toggleSwitch);
            host.UpdateLayout();

            Assert.IsTrue(mouseDown.Handled);
            Assert.AreEqual("PointerFocused", GetCurrentStateName(stateGroupsRoot, "FocusStates"));
        });
    }

    [TestMethod]
    public void FocusStatesUseToggleSwitchOwnerFocusLikeWinUI()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var toggleSwitch = new ModernWpf.Controls.ToggleSwitch
            {
                Template = CreateFocusableChildFocusStateTemplate()
            };

            using var host = new TestWindowHost(toggleSwitch, width: 260, height: 120);
            host.UpdateLayout();

            var stateGroupsRoot = FindStateGroupsRoot(toggleSwitch);
            var childButton = FindNamedDescendant<Button>(toggleSwitch, "TemplateButton");

            Assert.IsTrue(childButton.Focus());
            toggleSwitch.HeaderPlacement = ControlHeaderPlacement.Left;
            host.UpdateLayout();

            Assert.IsTrue(toggleSwitch.IsKeyboardFocusWithin);
            Assert.IsFalse(toggleSwitch.IsKeyboardFocused);
            Assert.AreEqual("Unfocused", GetCurrentStateName(stateGroupsRoot, "FocusStates"));

            var mouseDown = RaiseMouseLeftButtonDown(toggleSwitch);
            host.UpdateLayout();

            Assert.IsTrue(mouseDown.Handled);
            Assert.IsTrue(toggleSwitch.IsKeyboardFocused);
            Assert.AreEqual("PointerFocused", GetCurrentStateName(stateGroupsRoot, "FocusStates"));
        });
    }

    [TestMethod]
    public void TapInputTogglesOnAndOffLikeWinUINativeTest()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var toggleSwitch = new ModernWpf.Controls.ToggleSwitch
            {
                FontSize = 20
            };
            using var host = new TestWindowHost(toggleSwitch, width: 260, height: 120);
            host.UpdateLayout();

            var thumb = FindNamedDescendant<Thumb>(toggleSwitch, "SwitchThumb");

            Assert.IsFalse(toggleSwitch.IsOn);

            var firstTap = RaiseThumbMouseLeftButtonUp(thumb);
            host.UpdateLayout();

            Assert.IsTrue(firstTap.Handled);
            Assert.IsTrue(toggleSwitch.IsOn);

            var secondTap = RaiseThumbMouseLeftButtonUp(thumb);
            host.UpdateLayout();

            Assert.IsTrue(secondTap.Handled);
            Assert.IsFalse(toggleSwitch.IsOn);
        });
    }

    [TestMethod]
    public void ThumbMouseUpTapRunsAfterDragCompletionLikeWinUITapped()
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

            RaiseThumbMouseLeftButtonUp(thumb, handled: true);
            host.UpdateLayout();

            Assert.IsTrue(toggleSwitch.IsOn);
        });
    }

    [TestMethod]
    public void ThumbMouseUpAfterHorizontalDragDoesNotDoubleToggle()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var toggleSwitch = new ModernWpf.Controls.ToggleSwitch();
            using var host = new TestWindowHost(toggleSwitch, width: 260, height: 120);
            host.UpdateLayout();

            var thumb = FindNamedDescendant<Thumb>(toggleSwitch, "SwitchThumb");

            RaiseDragStarted(thumb);
            RaiseDragDelta(thumb, 12);
            RaiseDragCompleted(thumb);
            host.UpdateLayout();

            Assert.IsTrue(toggleSwitch.IsOn);

            var postDragMouseUp = RaiseThumbMouseLeftButtonUp(thumb, handled: true);
            host.UpdateLayout();

            Assert.IsTrue(postDragMouseUp.Handled);
            Assert.IsTrue(toggleSwitch.IsOn);

            RaiseThumbMouseLeftButtonUp(thumb);
            host.UpdateLayout();

            Assert.IsFalse(toggleSwitch.IsOn);
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
    public void DirectionalKeysDoNotToggleInEitherFlowDirection()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var toggleSwitch = new ModernWpf.Controls.ToggleSwitch();
            using var host = new TestWindowHost(toggleSwitch, width: 260, height: 120);
            host.UpdateLayout();

            foreach (var flowDirection in new[] { FlowDirection.LeftToRight, FlowDirection.RightToLeft })
            {
                toggleSwitch.FlowDirection = flowDirection;
                toggleSwitch.IsOn = false;

                foreach (var key in new[] { Key.Home, Key.End, Key.Up, Key.Down, Key.Left, Key.Right })
                {
                    var keyDown = RaiseKey(toggleSwitch, Keyboard.KeyDownEvent, key);
                    var keyUp = RaiseKey(toggleSwitch, Keyboard.KeyUpEvent, key);

                    Assert.IsFalse(keyDown.Handled, $"{flowDirection} {key} key-down should not be handled.");
                    Assert.IsFalse(keyUp.Handled, $"{flowDirection} {key} key-up should not be handled.");
                    Assert.IsFalse(toggleSwitch.IsOn, $"{flowDirection} {key} should not toggle the switch.");
                }
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
    public void AutomationNameMatchesWinUIExplicitAndStyledContentCases()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var style = new Style(typeof(ModernWpf.Controls.ToggleSwitch));
            style.Setters.Add(new Setter(ModernWpf.Controls.ToggleSwitch.OnContentProperty, "Yes"));
            style.Setters.Add(new Setter(ModernWpf.Controls.ToggleSwitch.OffContentProperty, "No"));

            var toggleSwitchWithAutomationName = new ModernWpf.Controls.ToggleSwitch
            {
                Header = "Header",
                OnContent = "Yes",
                OffContent = "No"
            };
            AutomationProperties.SetName(toggleSwitchWithAutomationName, "APName");

            var toggleSwitchStyled = new ModernWpf.Controls.ToggleSwitch
            {
                Header = "Header",
                Style = style
            };

            var root = new StackPanel();
            root.Children.Add(toggleSwitchWithAutomationName);
            root.Children.Add(toggleSwitchStyled);
            using var host = new TestWindowHost(root, width: 260, height: 160);
            host.UpdateLayout();

            Assert.AreEqual("APName", CreatePeer(toggleSwitchWithAutomationName).GetName());
            Assert.AreEqual("Header No", CreatePeer(toggleSwitchStyled).GetName());

            toggleSwitchWithAutomationName.IsOn = true;
            toggleSwitchStyled.IsOn = true;
            host.UpdateLayout();

            Assert.AreEqual("APName", CreatePeer(toggleSwitchWithAutomationName).GetName());
            Assert.AreEqual("Header Yes", CreatePeer(toggleSwitchStyled).GetName());
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
            var toggleSwitchPropertyValues = new ModernWpf.Controls.ToggleSwitch
            {
                Header = 7,
                OnContent = true,
                OffContent = false
            };
            var toggleSwitchUnknownObjects = new ModernWpf.Controls.ToggleSwitch
            {
                Header = new WinUIStringExtractionProbe(),
                OnContent = new WinUIStringExtractionProbe(),
                OffContent = new WinUIStringExtractionProbe()
            };

            root.Children.Add(toggleSwitchWithOnOffContent);
            root.Children.Add(toggleSwitchNonTextHeader);
            root.Children.Add(toggleSwitchNonTextOnOffContent);
            root.Children.Add(toggleSwitchTextElements);
            root.Children.Add(toggleSwitchPropertyValues);
            root.Children.Add(toggleSwitchUnknownObjects);

            using var host = new TestWindowHost(root, width: 260, height: 280);
            host.UpdateLayout();

            Assert.AreEqual("No", CreatePeer(toggleSwitchWithOnOffContent).GetName());
            Assert.AreEqual("No", CreatePeer(toggleSwitchNonTextHeader).GetName());
            Assert.AreEqual("Header", CreatePeer(toggleSwitchNonTextOnOffContent).GetName());
            Assert.AreEqual("Header No", CreatePeer(toggleSwitchTextElements).GetName());
            Assert.AreEqual("7 0", CreatePeer(toggleSwitchPropertyValues).GetName());
            Assert.AreEqual(string.Empty, CreatePeer(toggleSwitchUnknownObjects).GetName());

            toggleSwitchWithOnOffContent.IsOn = true;
            toggleSwitchNonTextHeader.IsOn = true;
            toggleSwitchNonTextOnOffContent.IsOn = true;
            toggleSwitchTextElements.IsOn = true;
            toggleSwitchPropertyValues.IsOn = true;
            toggleSwitchUnknownObjects.IsOn = true;

            Assert.AreEqual("Yes", CreatePeer(toggleSwitchWithOnOffContent).GetName());
            Assert.AreEqual("Yes", CreatePeer(toggleSwitchNonTextHeader).GetName());
            Assert.AreEqual("Header", CreatePeer(toggleSwitchNonTextOnOffContent).GetName());
            Assert.AreEqual("Header Yes", CreatePeer(toggleSwitchTextElements).GetName());
            Assert.AreEqual("7 1", CreatePeer(toggleSwitchPropertyValues).GetName());
            Assert.AreEqual(string.Empty, CreatePeer(toggleSwitchUnknownObjects).GetName());
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
            var resourceAccessor = new ResourceAccessor(typeof(ModernWpf.Controls.ToggleSwitch));

            Assert.AreEqual(nameof(ModernWpf.Controls.ToggleSwitch), peer.GetClassName());
            Assert.AreEqual(AutomationControlType.Button, peer.GetAutomationControlType());
            Assert.AreEqual(
                resourceAccessor.GetLocalizedStringResource(ResourceAccessor.SR_ToggleSwitchLocalizedControlType),
                peer.GetLocalizedControlType());
            Assert.AreEqual(0, peer.GetChildren()?.Count ?? 0);

            var toggleProvider = (IToggleProvider)peer.GetPattern(PatternInterface.Toggle);
            Assert.AreEqual(ToggleState.Off, toggleProvider.ToggleState);

            toggleSwitch.AutomationToggleSwitchOnToggle();

            Assert.IsTrue(toggleSwitch.IsOn);
            Assert.AreEqual(ToggleState.On, toggleProvider.ToggleState);

            toggleProvider.Toggle();

            Assert.IsFalse(toggleSwitch.IsOn);
            Assert.AreEqual(ToggleState.Off, toggleProvider.ToggleState);

            toggleSwitch.IsEnabled = false;

            Assert.ThrowsException<ElementNotEnabledException>(() => toggleProvider.Toggle());
            Assert.IsFalse(toggleSwitch.IsOn);
        });
    }

    [TestMethod]
    public void ToggleStateAutomationNotificationCreatesPeerWhenListenerExistsLikeWinUI()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var toggleSwitch = new AutomationPeerCreationProbeToggleSwitch();

            Assert.IsNull(UIElementAutomationPeer.FromElement(toggleSwitch));

            using var host = new TestWindowHost(new Grid(), width: 100, height: 100);
            var rootElement = AutomationElement.FromHandle(new WindowInteropHelper(host.Window).Handle);
            AutomationPropertyChangedEventHandler handler = (sender, args) => { };

            WpfAutomation.AddAutomationPropertyChangedEventHandler(
                rootElement,
                TreeScope.Descendants,
                handler,
                TogglePatternIdentifiers.ToggleStateProperty);

            try
            {
                WpfTestHost.DoEvents();
                Assert.IsTrue(AutomationPeer.ListenerExists(AutomationEvents.PropertyChanged));

                toggleSwitch.IsOn = true;

                Assert.AreEqual(1, toggleSwitch.AutomationPeerCreations);
                Assert.IsInstanceOfType(
                    UIElementAutomationPeer.FromElement(toggleSwitch),
                    typeof(ToggleSwitchAutomationPeer));
            }
            finally
            {
                WpfAutomation.RemoveAutomationPropertyChangedEventHandler(rootElement, handler);
            }
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
    public void TemplateRootCarriesWinUICommonStylesGridChrome()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var background = Brushes.Red;
            var borderBrush = Brushes.Blue;
            var borderThickness = new Thickness(2);
            var cornerRadius = new CornerRadius(6);
            var toggleSwitch = new ModernWpf.Controls.ToggleSwitch
            {
                Background = background,
                BorderBrush = borderBrush,
                BorderThickness = borderThickness,
                CornerRadius = cornerRadius
            };
            using var host = new TestWindowHost(toggleSwitch, width: 260, height: 120);
            host.UpdateLayout();

            Assert.AreEqual(1, VisualTreeHelper.GetChildrenCount(toggleSwitch));

            var templateRoot = VisualTreeHelper.GetChild(toggleSwitch, 0);
            Assert.IsInstanceOfType(templateRoot, typeof(Grid));

            var templateRootGrid = (Grid)templateRoot;
            var templateRootChrome = FindNamedDescendant<BorderEx>(toggleSwitch, "TemplateRootChrome");
            Assert.AreSame(background, templateRootChrome.Background);
            Assert.AreSame(borderBrush, templateRootChrome.BorderBrush);
            Assert.AreEqual(borderThickness, templateRootChrome.BorderThickness);
            Assert.AreEqual(cornerRadius, templateRootChrome.CornerRadius);
            Assert.AreEqual(2, templateRootGrid.RowDefinitions.Count);
            Assert.AreEqual("ContentStates", VisualStateManager.GetVisualStateGroups(templateRootGrid)
                .OfType<VisualStateGroup>()
                .Last().Name);
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

    private static DragStartedEventArgs RaiseDragStarted(Thumb thumb)
    {
        var args = new DragStartedEventArgs(0, 0)
        {
            RoutedEvent = Thumb.DragStartedEvent
        };

        thumb.RaiseEvent(args);
        return args;
    }

    private static DragDeltaEventArgs RaiseDragDelta(Thumb thumb, double horizontalChange, double verticalChange = 0)
    {
        var args = new DragDeltaEventArgs(horizontalChange, verticalChange)
        {
            RoutedEvent = Thumb.DragDeltaEvent
        };

        thumb.RaiseEvent(args);
        return args;
    }

    private static DragCompletedEventArgs RaiseDragCompleted(Thumb thumb, bool canceled = false)
    {
        var args = new DragCompletedEventArgs(0, 0, canceled)
        {
            RoutedEvent = Thumb.DragCompletedEvent
        };

        thumb.RaiseEvent(args);
        return args;
    }

    private static MouseButtonEventArgs RaiseThumbMouseLeftButtonUp(Thumb thumb, bool handled = false)
    {
        var args = new MouseButtonEventArgs(
            Mouse.PrimaryDevice,
            Environment.TickCount,
            MouseButton.Left)
        {
            RoutedEvent = UIElement.MouseLeftButtonUpEvent,
            Handled = handled
        };

        thumb.RaiseEvent(args);
        return args;
    }

    private static MouseButtonEventArgs RaiseMouseLeftButtonDown(UIElement element)
    {
        var args = new MouseButtonEventArgs(
            Mouse.PrimaryDevice,
            Environment.TickCount,
            MouseButton.Left)
        {
            RoutedEvent = UIElement.MouseLeftButtonDownEvent
        };

        element.RaiseEvent(args);
        return args;
    }

    private static MouseEventArgs RaiseMouseEnter(UIElement element)
    {
        var args = new MouseEventArgs(
            Mouse.PrimaryDevice,
            Environment.TickCount)
        {
            RoutedEvent = UIElement.MouseEnterEvent
        };

        element.RaiseEvent(args);
        return args;
    }

    private static MouseEventArgs RaiseLostMouseCapture(UIElement element)
    {
        var args = new MouseEventArgs(
            Mouse.PrimaryDevice,
            Environment.TickCount)
        {
            RoutedEvent = UIElement.LostMouseCaptureEvent
        };

        element.RaiseEvent(args);
        return args;
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

    private static ManipulationStartingEventArgs CreateManipulationStartingArgs()
    {
        var args = CreateNonPublicInstance<ManipulationStartingEventArgs>(null, Environment.TickCount);
        args.RoutedEvent = UIElement.ManipulationStartingEvent;
        return args;
    }

    private static ManipulationStartedEventArgs CreateManipulationStartedArgs(IInputElement container)
    {
        var args = CreateNonPublicInstance<ManipulationStartedEventArgs>(
            null,
            Environment.TickCount,
            container,
            new Point());
        args.RoutedEvent = UIElement.ManipulationStartedEvent;
        return args;
    }

    private static ManipulationDeltaEventArgs CreateManipulationDeltaArgs(
        IInputElement container,
        double horizontalChange,
        double verticalChange)
    {
        var delta = new ManipulationDelta(
            new Vector(horizontalChange, verticalChange),
            0,
            new Vector(1, 1),
            new Vector());
        var velocities = new ManipulationVelocities(new Vector(), 0, new Vector());

        var args = CreateNonPublicInstance<ManipulationDeltaEventArgs>(
            null,
            Environment.TickCount,
            container,
            new Point(),
            delta,
            delta,
            velocities,
            false);
        args.RoutedEvent = UIElement.ManipulationDeltaEvent;
        return args;
    }

    private static ManipulationCompletedEventArgs CreateManipulationCompletedArgs(IInputElement container)
    {
        var total = new ManipulationDelta(new Vector(), 0, new Vector(1, 1), new Vector());
        var velocities = new ManipulationVelocities(new Vector(), 0, new Vector());

        var args = CreateNonPublicInstance<ManipulationCompletedEventArgs>(
            null,
            Environment.TickCount,
            container,
            new Point(),
            total,
            velocities,
            false);
        args.RoutedEvent = UIElement.ManipulationCompletedEvent;
        return args;
    }

    private static T CreateNonPublicInstance<T>(params object?[] args)
    {
        var argumentTypes = args
            .Select(arg => arg?.GetType())
            .ToArray();

        var constructor = typeof(T)
            .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
            .Single(item =>
            {
                var parameters = item.GetParameters();
                if (parameters.Length != args.Length)
                {
                    return false;
                }

                for (int i = 0; i < parameters.Length; i++)
                {
                    if (argumentTypes[i] != null && !parameters[i].ParameterType.IsAssignableFrom(argumentTypes[i]))
                    {
                        return false;
                    }
                }

                return true;
            });

        return (T)constructor.Invoke(args);
    }

    private static DataTemplate CreateTextTemplate()
    {
        return (DataTemplate)XamlReader.Parse(
            @"<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>
                <TextBlock Text='{Binding}'/>
            </DataTemplate>");
    }

    private static ControlTemplate CreateFocusStateTemplate()
    {
        return (ControlTemplate)XamlReader.Parse(
            @"<ControlTemplate
                xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
                xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
                xmlns:controls='clr-namespace:ModernWpf.Controls;assembly=ModernWpf.Controls'
                TargetType='{x:Type controls:ToggleSwitch}'>
                <Grid>
                    <VisualStateManager.VisualStateGroups>
                        <VisualStateGroup x:Name='CommonStates'>
                            <VisualState x:Name='Normal' />
                            <VisualState x:Name='PointerOver' />
                            <VisualState x:Name='Pressed' />
                            <VisualState x:Name='Disabled' />
                        </VisualStateGroup>
                        <VisualStateGroup x:Name='FocusStates'>
                            <VisualState x:Name='PointerFocused' />
                            <VisualState x:Name='Focused' />
                            <VisualState x:Name='Unfocused' />
                        </VisualStateGroup>
                        <VisualStateGroup x:Name='ContentStates'>
                            <VisualState x:Name='OffContent' />
                            <VisualState x:Name='OnContent' />
                        </VisualStateGroup>
                        <VisualStateGroup x:Name='ToggleStates'>
                            <VisualState x:Name='Dragging' />
                            <VisualState x:Name='Off' />
                            <VisualState x:Name='On' />
                        </VisualStateGroup>
                        <VisualStateGroup x:Name='HeaderStates'>
                            <VisualState x:Name='TopHeader' />
                            <VisualState x:Name='LeftHeader' />
                        </VisualStateGroup>
                    </VisualStateManager.VisualStateGroups>
                </Grid>
            </ControlTemplate>");
    }

    private static ControlTemplate CreateRenderTransformPartDiscoveryTemplate()
    {
        return (ControlTemplate)XamlReader.Parse(
            @"<ControlTemplate
                xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
                xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
                xmlns:controls='clr-namespace:ModernWpf.Controls;assembly=ModernWpf.Controls'
                TargetType='{x:Type controls:ToggleSwitch}'>
                <Grid Width='72' Height='40'>
                    <VisualStateManager.VisualStateGroups>
                        <VisualStateGroup x:Name='CommonStates'>
                            <VisualState x:Name='Normal' />
                            <VisualState x:Name='PointerOver' />
                            <VisualState x:Name='Pressed' />
                            <VisualState x:Name='Disabled' />
                        </VisualStateGroup>
                        <VisualStateGroup x:Name='FocusStates'>
                            <VisualState x:Name='PointerFocused' />
                            <VisualState x:Name='Focused' />
                            <VisualState x:Name='Unfocused' />
                        </VisualStateGroup>
                        <VisualStateGroup x:Name='ContentStates'>
                            <VisualState x:Name='OffContent' />
                            <VisualState x:Name='OnContent' />
                        </VisualStateGroup>
                        <VisualStateGroup x:Name='ToggleStates'>
                            <VisualState x:Name='Dragging' />
                            <VisualState x:Name='Off' />
                            <VisualState x:Name='On' />
                        </VisualStateGroup>
                        <VisualStateGroup x:Name='HeaderStates'>
                            <VisualState x:Name='TopHeader' />
                            <VisualState x:Name='LeftHeader' />
                        </VisualStateGroup>
                    </VisualStateManager.VisualStateGroups>
                    <Grid x:Name='SwitchCurtain' Width='40' Height='20' HorizontalAlignment='Left' VerticalAlignment='Top'>
                        <Grid.RenderTransform>
                            <TranslateTransform />
                        </Grid.RenderTransform>
                    </Grid>
                    <Rectangle x:Name='SwitchCurtainBounds' Width='40' Height='20' HorizontalAlignment='Left' VerticalAlignment='Top' />
                    <Rectangle x:Name='SwitchCurtainClip' Width='40' Height='20' HorizontalAlignment='Left' VerticalAlignment='Top' />
                    <Grid x:Name='SwitchKnobBounds' Width='40' Height='20' HorizontalAlignment='Left' VerticalAlignment='Top' />
                    <Grid x:Name='SwitchKnob' Width='20' Height='20' HorizontalAlignment='Left' VerticalAlignment='Top'>
                        <Grid.RenderTransform>
                            <TranslateTransform />
                        </Grid.RenderTransform>
                    </Grid>
                    <Thumb x:Name='SwitchThumb' Width='40' Height='20' HorizontalAlignment='Left' VerticalAlignment='Top' />
                </Grid>
            </ControlTemplate>");
    }

    private static ControlTemplate CreateFocusableChildFocusStateTemplate()
    {
        return (ControlTemplate)XamlReader.Parse(
            @"<ControlTemplate
                xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
                xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
                xmlns:controls='clr-namespace:ModernWpf.Controls;assembly=ModernWpf.Controls'
                TargetType='{x:Type controls:ToggleSwitch}'>
                <Grid>
                    <VisualStateManager.VisualStateGroups>
                        <VisualStateGroup x:Name='CommonStates'>
                            <VisualState x:Name='Normal' />
                            <VisualState x:Name='PointerOver' />
                            <VisualState x:Name='Pressed' />
                            <VisualState x:Name='Disabled' />
                        </VisualStateGroup>
                        <VisualStateGroup x:Name='FocusStates'>
                            <VisualState x:Name='PointerFocused' />
                            <VisualState x:Name='Focused' />
                            <VisualState x:Name='Unfocused' />
                        </VisualStateGroup>
                        <VisualStateGroup x:Name='ContentStates'>
                            <VisualState x:Name='OffContent' />
                            <VisualState x:Name='OnContent' />
                        </VisualStateGroup>
                        <VisualStateGroup x:Name='ToggleStates'>
                            <VisualState x:Name='Dragging' />
                            <VisualState x:Name='Off' />
                            <VisualState x:Name='On' />
                        </VisualStateGroup>
                        <VisualStateGroup x:Name='HeaderStates'>
                            <VisualState x:Name='TopHeader' />
                            <VisualState x:Name='LeftHeader' />
                        </VisualStateGroup>
                    </VisualStateManager.VisualStateGroups>
                    <Button x:Name='TemplateButton' Content='Template child' />
                </Grid>
            </ControlTemplate>");
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

    private static VisualState FindVisualState(
        FrameworkElement stateGroupsRoot,
        string groupName,
        string stateName)
    {
        var group = VisualStateManager.GetVisualStateGroups(stateGroupsRoot)
            .OfType<VisualStateGroup>()
            .Single(item => item.Name == groupName);
        return group.States
            .OfType<VisualState>()
            .Single(item => item.Name == stateName);
    }

    private static global::ModernWpf.VisualStateEx FindVisualStateEx(
        FrameworkElement stateGroupsRoot,
        string groupName,
        string stateName)
    {
        var state = FindVisualState(stateGroupsRoot, groupName, stateName);
        Assert.IsInstanceOfType(state, typeof(global::ModernWpf.VisualStateEx));
        return (global::ModernWpf.VisualStateEx)state;
    }

    private static void AssertStateSetter(global::ModernWpf.VisualStateEx state, string target)
    {
        Assert.IsTrue(
            state.Setters.Any(setter => setter.Target == target),
            $"Expected VisualStateEx setter target '{target}'.");
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

    private static void AssertStateSetterDynamicResource(global::ModernWpf.VisualStateEx state, string target, object expectedResourceKey)
    {
        var setter = state.Setters.Single(item => item.Target == target);
        AssertResourceReferenceExpression(
            setter.ReadLocalValue(global::ModernWpf.VisualStateSetter.ValueProperty),
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

    private static void AssertSwitchAreaGridBackgroundColorAnimation(VisualState state)
    {
        var animation = state.Storyboard.Children
            .OfType<ColorAnimationUsingKeyFrames>()
            .Single(item => Storyboard.GetTargetName(item) == "SwitchAreaGrid");
        var targetProperty = Storyboard.GetTargetProperty(animation);

        Assert.AreSame(Border.BackgroundProperty, targetProperty.PathParameters[0], state.Name);
        Assert.AreSame(SolidColorBrush.ColorProperty, targetProperty.PathParameters[1], state.Name);
    }

    private static void AssertKnobSizeAnimations(VisualState state, double expectedWidth, double expectedHeight)
    {
        AssertDoubleKeyFrameValue(state, "SwitchKnobOn", "Width", expectedWidth);
        AssertDoubleKeyFrameValue(state, "SwitchKnobOn", "Height", expectedHeight);
        AssertDoubleKeyFrameValue(state, "SwitchKnobOff", "Width", expectedWidth);
        AssertDoubleKeyFrameValue(state, "SwitchKnobOff", "Height", expectedHeight);
    }

    private static void AssertDoubleKeyFrameValue(
        VisualState state,
        string targetName,
        string targetProperty,
        double expectedValue)
    {
        var animation = state.Storyboard.Children
            .OfType<DoubleAnimationUsingKeyFrames>()
            .Single(item =>
                Storyboard.GetTargetName(item) == targetName &&
                Storyboard.GetTargetProperty(item).Path == targetProperty);
        var keyFrame = animation.KeyFrames
            .OfType<DoubleKeyFrame>()
            .Single();

        Assert.AreEqual(expectedValue, keyFrame.Value, 0.1, $"{state.Name}:{targetName}.{targetProperty}");
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

    private static void AssertThemeResourceReferences(
        string themeName,
        params (string ResourceKey, object ExpectedResourceKey)[] expectedResources)
    {
        foreach (var expectedResource in expectedResources)
        {
            AssertThemeResourceReference(themeName, expectedResource.ResourceKey, expectedResource.ExpectedResourceKey);
        }
    }

    private static void AssertThemeSolidColorBrushValue(string themeName, string resourceKey, Color expectedColor)
    {
        var themeDictionary = global::ModernWpf.ThemeResources.Current.GetThemeDictionary(themeName);

        Assert.IsTrue(themeDictionary.Contains(resourceKey), $"{themeName} is missing {resourceKey}.");
        Assert.IsInstanceOfType(themeDictionary[resourceKey], typeof(SolidColorBrush), $"{themeName}:{resourceKey}");
        Assert.AreEqual(expectedColor, ((SolidColorBrush)themeDictionary[resourceKey]).Color, $"{themeName}:{resourceKey}");
    }

    private static void AssertThemeSolidColorBrushColorReference(string themeName, string resourceKey, string expectedColorResourceKey)
    {
        var themeDictionary = global::ModernWpf.ThemeResources.Current.GetThemeDictionary(themeName);

        Assert.IsTrue(themeDictionary.Contains(resourceKey), $"{themeName} is missing {resourceKey}.");
        Assert.IsTrue(themeDictionary.Contains(expectedColorResourceKey), $"{themeName} is missing {expectedColorResourceKey}.");
        Assert.IsInstanceOfType(themeDictionary[resourceKey], typeof(SolidColorBrush), $"{themeName}:{resourceKey}");
        Assert.AreEqual(
            themeDictionary[expectedColorResourceKey],
            ((SolidColorBrush)themeDictionary[resourceKey]).Color,
            $"{themeName}:{resourceKey}");
    }

    private static ToggleSwitchAutomationPeer CreatePeer(ModernWpf.Controls.ToggleSwitch toggleSwitch)
    {
        return new ToggleSwitchAutomationPeer(toggleSwitch);
    }

    private sealed class WinUIStringExtractionProbe
    {
        public override string ToString()
        {
            return "ShouldNotAppearInAutomationName";
        }
    }

    private sealed class AutomationPeerCreationProbeToggleSwitch : ModernWpf.Controls.ToggleSwitch
    {
        public int AutomationPeerCreations { get; private set; }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            AutomationPeerCreations++;
            return base.OnCreateAutomationPeer();
        }
    }

    private sealed class ManipulationProbeToggleSwitch : ModernWpf.Controls.ToggleSwitch
    {
        public void InvokeManipulationStarting(ManipulationStartingEventArgs args)
        {
            OnManipulationStarting(args);
        }

        public void InvokeManipulationStarted(ManipulationStartedEventArgs args)
        {
            OnManipulationStarted(args);
        }

        public void InvokeManipulationDelta(ManipulationDeltaEventArgs args)
        {
            OnManipulationDelta(args);
        }

        public void InvokeManipulationCompleted(ManipulationCompletedEventArgs args)
        {
            OnManipulationCompleted(args);
        }
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
