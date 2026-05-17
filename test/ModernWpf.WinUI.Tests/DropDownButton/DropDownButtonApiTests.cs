using System;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using ModernWpf;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.DropDownButton;

[TestClass]
public class DropDownButtonApiTests
{
    [TestMethod]
    public void VerifyDropDownButtonPropertyGettersAndSetters()
    {
        WpfTestHost.Run(() =>
        {
            var flyout = new Flyout
            {
                Content = new TextBlock
                {
                    Text = "Flyout content"
                }
            };
            var transitions = new ModernWpf.Media.Animation.TransitionCollection();
            var button = new ModernWpf.Controls.DropDownButton
            {
                BackgroundSizing = BackgroundSizing.OuterBorderEdge,
                CharacterSpacing = 17,
                Content = "Options",
                ContentTransitions = transitions,
                Flyout = flyout,
                CornerRadius = new CornerRadius(4),
                IsTextScaleFactorEnabled = false,
                UseSystemFocusVisuals = true,
                FocusVisualMargin = new Thickness(2)
            };

            Assert.AreEqual(BackgroundSizing.OuterBorderEdge, button.BackgroundSizing);
            Assert.AreEqual(17, button.CharacterSpacing);
            Assert.AreEqual("Options", button.Content);
            Assert.AreSame(transitions, button.ContentTransitions);
            Assert.AreSame(flyout, button.Flyout);
            Assert.AreEqual(new CornerRadius(4), button.CornerRadius);
            Assert.IsFalse(button.IsTextScaleFactorEnabled);
            Assert.IsTrue(button.UseSystemFocusVisuals);
            Assert.AreEqual(new Thickness(2), button.FocusVisualMargin);

            button.Flyout = null;

            Assert.IsNull(button.Flyout);
        });
    }

    [TestMethod]
    public void VerifyDropDownButtonTemplateAndWinUI2Resources()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var button = new ModernWpf.Controls.DropDownButton
            {
                Content = "Options"
            };

            using var host = new TestWindowHost(button, width: 320, height: 160);
            host.UpdateLayout();

            Assert.AreEqual(HorizontalAlignment.Left, button.HorizontalAlignment);
            Assert.AreEqual(VerticalAlignment.Center, button.VerticalAlignment);
            Assert.AreEqual(HorizontalAlignment.Center, button.HorizontalContentAlignment);
            Assert.AreEqual(VerticalAlignment.Center, button.VerticalContentAlignment);
            Assert.AreEqual(new Thickness(-3), button.FocusVisualMargin);
            Assert.IsNotNull(button.TryFindResource("DropDownButtonForegroundSecondary"));
            Assert.IsNotNull(button.TryFindResource("DropDownButtonForegroundSecondaryPointerOver"));
            Assert.IsNotNull(button.TryFindResource("DropDownButtonForegroundSecondaryPressed"));

            var chevron = VisualTreeTestHelper.FindDescendant<FontIconFallback>(button);

            Assert.IsNotNull(chevron);
            Assert.AreEqual("ChevronIcon", chevron!.Name);
            Assert.AreEqual(12d, chevron.Width);
            Assert.AreEqual(12d, chevron.Height);
            Assert.IsNotNull(chevron.Foreground);
        });
    }

    [TestMethod]
    public void VerifyDropDownButtonTemplateUsesWinUIContentPresenterShape()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var transitions = new ModernWpf.Media.Animation.TransitionCollection();
            var button = new ModernWpf.Controls.DropDownButton
            {
                Width = 140,
                Height = 44,
                BackgroundSizing = BackgroundSizing.OuterBorderEdge,
                CharacterSpacing = 23,
                Content = "Options",
                ContentTransitions = transitions,
                IsTextScaleFactorEnabled = false
            };

            using var host = new TestWindowHost(button, width: 220, height: 120);
            host.UpdateLayout();

            var rootGrid = VisualTreeTestHelper.FindDescendant<GridEx>(button)
                ?? throw new AssertFailedException("Expected DropDownButton template root to use GridEx chrome.");
            var presenter = VisualTreeTestHelper.FindDescendant<ContentPresenterEx>(button)
                ?? throw new AssertFailedException("Expected DropDownButton template to use ContentPresenterEx.");

            Assert.AreEqual(BackgroundSizing.OuterBorderEdge, rootGrid.BackgroundSizing);
            Assert.IsNotNull(rootGrid.BackgroundTransition);
            Assert.AreEqual(TimeSpan.FromMilliseconds(83), rootGrid.BackgroundTransition.Duration);
            Assert.AreEqual(23, presenter.CharacterSpacing);
            Assert.AreEqual("Options", presenter.Content);
            Assert.AreSame(transitions, presenter.ContentTransitions);
            Assert.IsFalse(presenter.IsTextScaleFactorEnabled);
            Assert.IsNull(VisualTreeTestHelper.FindDescendant<ContentControlEx>(button));
        });
    }

    [TestMethod]
    public void AnimatedChevronStateUsesWinUIVisualStateSetters()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var button = new ModernWpf.Controls.DropDownButton
            {
                Content = "Options"
            };

            using var host = new TestWindowHost(button, width: 220, height: 120);
            host.UpdateLayout();

            var rootGrid = VisualTreeTestHelper.FindDescendant<GridEx>(button)
                ?? throw new AssertFailedException("Expected DropDownButton template root to use GridEx chrome.");
            var chevron = VisualTreeTestHelper.FindDescendant<FontIconFallback>(button)
                ?? throw new AssertFailedException("Expected DropDownButton chevron icon.");

            AssertStateSetter(rootGrid, "CommonStates", "PointerOver", "ChevronIcon.(ui:AnimatedIcon.State)", "PointerOver");
            AssertStateSetter(rootGrid, "CommonStates", "PointerOver", "RootGrid.Background", button.TryFindResource("ButtonBackgroundPointerOver"));
            AssertStateSetter(rootGrid, "CommonStates", "PointerOver", "RootGrid.BorderBrush", button.TryFindResource("ButtonBorderBrushPointerOver"));
            AssertStateSetter(rootGrid, "CommonStates", "PointerOver", "ContentPresenter.Foreground", button.TryFindResource("ButtonForegroundPointerOver"));
            AssertStateSetter(rootGrid, "CommonStates", "PointerOver", "ChevronIcon.Foreground", button.TryFindResource("DropDownButtonForegroundSecondaryPointerOver"));
            AssertStateSetter(rootGrid, "CommonStates", "Pressed", "ChevronIcon.(ui:AnimatedIcon.State)", "Pressed");
            AssertStateSetter(rootGrid, "CommonStates", "Pressed", "RootGrid.Background", button.TryFindResource("ButtonBackgroundPressed"));
            AssertStateSetter(rootGrid, "CommonStates", "Pressed", "RootGrid.BorderBrush", button.TryFindResource("ButtonBorderBrushPressed"));
            AssertStateSetter(rootGrid, "CommonStates", "Pressed", "ContentPresenter.Foreground", button.TryFindResource("ButtonForegroundPressed"));
            AssertStateSetter(rootGrid, "CommonStates", "Pressed", "ChevronIcon.Foreground", button.TryFindResource("DropDownButtonForegroundSecondaryPressed"));
            AssertStateSetter(rootGrid, "CommonStates", "Disabled", "ChevronIcon.(ui:AnimatedIcon.State)", "Normal");
            AssertStateSetter(rootGrid, "CommonStates", "Disabled", "RootGrid.Background", button.TryFindResource("ButtonBackgroundDisabled"));
            AssertStateSetter(rootGrid, "CommonStates", "Disabled", "RootGrid.BorderBrush", button.TryFindResource("ButtonBorderBrushDisabled"));
            AssertStateSetter(rootGrid, "CommonStates", "Disabled", "ContentPresenter.Foreground", button.TryFindResource("ButtonForegroundDisabled"));
            AssertStateSetter(rootGrid, "CommonStates", "Disabled", "ChevronIcon.Foreground", button.TryFindResource("ButtonForegroundDisabled"));

            Assert.AreEqual("Normal", AnimatedIcon.GetState(chevron));

            Assert.IsTrue(VisualStateManager.GoToState(button, "PointerOver", false));
            Assert.AreEqual("PointerOver", AnimatedIcon.GetState(chevron));

            Assert.IsTrue(VisualStateManager.GoToState(button, "Pressed", false));
            Assert.AreEqual("Pressed", AnimatedIcon.GetState(chevron));

            Assert.IsTrue(VisualStateManager.GoToState(button, "Disabled", false));
            Assert.AreEqual("Normal", AnimatedIcon.GetState(chevron));

            Assert.IsTrue(VisualStateManager.GoToState(button, "Normal", false));
            Assert.AreEqual("Normal", AnimatedIcon.GetState(chevron));
        });
    }

    [TestMethod]
    public void FlyoutEventsTrackExpandCollapseState()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var flyout = new Flyout
            {
                Content = new TextBlock
                {
                    Text = "Flyout content",
                    MinWidth = 120,
                    MinHeight = 32
                }
            };
            var button = new ModernWpf.Controls.DropDownButton
            {
                Content = "Options",
                Flyout = flyout
            };

            using var host = new TestWindowHost(button, width: 320, height: 160);
            host.UpdateLayout();

            var provider = GetExpandCollapseProvider(button);

            Assert.AreEqual(ExpandCollapseState.Collapsed, provider.ExpandCollapseState);
            Assert.IsFalse(button.IsFlyoutOpen);

            provider.Expand();
            WpfTestHost.DoEvents();

            Assert.IsTrue(flyout.IsOpen);
            Assert.IsTrue(button.IsFlyoutOpen);
            Assert.AreEqual(ExpandCollapseState.Expanded, provider.ExpandCollapseState);

            provider.Collapse();
            WpfTestHost.DoEvents();

            Assert.IsFalse(flyout.IsOpen);
            Assert.IsFalse(button.IsFlyoutOpen);
            Assert.AreEqual(ExpandCollapseState.Collapsed, provider.ExpandCollapseState);
        });
    }

    private static void AssertStateSetter(FrameworkElement stateGroupsRoot, string groupName, string stateName, string target, object expectedValue)
    {
        var group = VisualStateManager.GetVisualStateGroups(stateGroupsRoot)
            .OfType<VisualStateGroup>()
            .Single(item => item.Name == groupName);
        var state = group.States.OfType<VisualStateEx>().Single(item => item.Name == stateName);
        var setter = state.Setters.Single(item => item.Target == target);

        Assert.AreEqual(expectedValue, setter.Value);
    }

    private static IExpandCollapseProvider GetExpandCollapseProvider(ModernWpf.Controls.DropDownButton button)
    {
        var peer = FrameworkElementAutomationPeer.CreatePeerForElement(button);
        Assert.IsNotNull(peer);

        if (peer!.GetPattern(PatternInterface.ExpandCollapse) is IExpandCollapseProvider provider)
        {
            return provider;
        }

        Assert.Fail("DropDownButton should expose IExpandCollapseProvider.");
        throw new InvalidOperationException();
    }
}
