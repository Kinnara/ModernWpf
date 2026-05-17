using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.CommonStyles;

[TestClass]
public class RepeatButtonVisualStateTests
{
    [TestMethod]
    public void DefaultRepeatButtonStyleUsesSourceVisualStateSetters()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var repeatButton = CreateRepeatButton();
            using var host = new TestWindowHost(repeatButton, width: 140, height: 80);

            var presenter = GetContentPresenter(repeatButton);

            Assert.AreEqual(0, repeatButton.Template.Triggers.Count);
            Assert.AreEqual(ClickMode.Press, repeatButton.ClickMode);
            Assert.IsTrue(ButtonHelper.GetVisualStateSettersEnabled(repeatButton));
            AssertStateSetters(presenter, "PointerOver");
            AssertStateSetters(presenter, "Pressed");
            AssertStateSetters(presenter, "Disabled");
            AssertVisualStateAppliesResources(repeatButton, presenter, "PointerOver", "RepeatButtonBackgroundPointerOver", "RepeatButtonBorderBrushPointerOver", "RepeatButtonForegroundPointerOver");
            AssertVisualStateAppliesResources(repeatButton, presenter, "Pressed", "RepeatButtonBackgroundPressed", "RepeatButtonBorderBrushPressed", "RepeatButtonForegroundPressed");
            AssertVisualStateAppliesResources(repeatButton, presenter, "Disabled", "RepeatButtonBackgroundDisabled", "RepeatButtonBorderBrushDisabled", "RepeatButtonForegroundDisabled");
        });
    }

    [TestMethod]
    public void DisabledStateIsDrivenByButtonHelper()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var repeatButton = CreateRepeatButton();
            using var host = new TestWindowHost(repeatButton, width: 140, height: 80);
            host.UpdateLayout();

            var presenter = GetContentPresenter(repeatButton);

            repeatButton.IsEnabled = false;
            host.UpdateLayout();

            Assert.AreEqual("Disabled", GetCurrentStateName(presenter));
            AssertPresenterResources(
                presenter,
                "RepeatButtonBackgroundDisabled",
                "RepeatButtonBorderBrushDisabled",
                "RepeatButtonForegroundDisabled");
        });
    }

    private static RepeatButton CreateRepeatButton()
    {
        return new RepeatButton
        {
            Width = 100,
            Height = 40,
            Content = "Repeat"
        };
    }

    private static ContentPresenterEx GetContentPresenter(RepeatButton repeatButton)
    {
        repeatButton.ApplyTemplate();
        return repeatButton.Template.FindName("ContentPresenter", repeatButton) as ContentPresenterEx
            ?? throw new AssertFailedException("Expected RepeatButton template to use ContentPresenterEx directly.");
    }

    private static void AssertStateSetters(FrameworkElement stateGroupsRoot, string stateName)
    {
        var stateEx = GetCommonState(stateGroupsRoot, stateName);
        AssertStateSetter(stateEx, "ContentPresenter.Background");
        AssertStateSetter(stateEx, "ContentPresenter.BorderBrush");
        AssertStateSetter(stateEx, "ContentPresenter.Foreground");
    }

    private static void AssertStateSetter(VisualStateEx stateEx, string target)
    {
        Assert.IsTrue(
            stateEx.Setters.Any(item => item.Target == target),
            $"{stateEx.Name} should set {target}.");
    }

    private static VisualStateEx GetCommonState(FrameworkElement stateGroupsRoot, string stateName)
    {
        var group = VisualStateManager.GetVisualStateGroups(stateGroupsRoot)
            .OfType<VisualStateGroup>()
            .Single(item => item.Name == "CommonStates");
        var state = group.States
            .Cast<VisualState>()
            .Single(item => item.Name == stateName);

        Assert.IsInstanceOfType(state, typeof(VisualStateEx));
        return (VisualStateEx)state;
    }

    private static string GetCurrentStateName(FrameworkElement stateGroupsRoot)
    {
        var group = VisualStateManager.GetVisualStateGroups(stateGroupsRoot)
            .OfType<VisualStateGroup>()
            .Single(item => item.Name == "CommonStates");
        Assert.IsNotNull(group.CurrentState);
        return group.CurrentState.Name;
    }

    private static void AssertVisualStateAppliesResources(
        RepeatButton repeatButton,
        ContentPresenterEx presenter,
        string stateName,
        string backgroundKey,
        string borderBrushKey,
        string foregroundKey)
    {
        Assert.IsTrue(VisualStateManager.GoToState(repeatButton, stateName, false));
        AssertPresenterResources(presenter, backgroundKey, borderBrushKey, foregroundKey);
    }

    private static void AssertPresenterResources(
        ContentPresenterEx presenter,
        string backgroundKey,
        string borderBrushKey,
        string foregroundKey)
    {
        Assert.AreSame(presenter.TryFindResource(backgroundKey), presenter.Background);
        Assert.AreSame(presenter.TryFindResource(borderBrushKey), presenter.BorderBrush);
        Assert.AreSame(presenter.TryFindResource(foregroundKey), presenter.Foreground);
    }
}
