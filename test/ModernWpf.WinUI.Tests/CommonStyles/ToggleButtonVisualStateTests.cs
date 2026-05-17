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
public class ToggleButtonVisualStateTests
{
    [TestMethod]
    public void DefaultToggleButtonStyleUsesSourceVisualStateSetters()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var toggleButton = CreateToggleButton();
            using var host = new TestWindowHost(toggleButton, width: 140, height: 80);
            host.UpdateLayout();

            var presenter = GetContentPresenter(toggleButton);

            Assert.AreEqual(0, toggleButton.Template.Triggers.Count);
            Assert.IsTrue(ToggleButtonHelper.GetVisualStateSettersEnabled(toggleButton));

            AssertStateSetters(presenter, "PointerOver", "ContentPresenter.Background", "ContentPresenter.BorderBrush", "ContentPresenter.Foreground");
            AssertStateSetters(presenter, "Pressed", "ContentPresenter.Background", "ContentPresenter.BorderBrush", "ContentPresenter.Foreground");
            AssertStateSetters(presenter, "Disabled", "ContentPresenter.Background", "ContentPresenter.BorderBrush", "ContentPresenter.Foreground");
            AssertStateSetters(presenter, "Checked", CheckedStateTargets());
            AssertStateSetters(presenter, "CheckedPointerOver", CheckedStateTargets());
            AssertStateSetters(presenter, "CheckedPressed", CheckedStateTargets());
            AssertStateSetters(presenter, "CheckedDisabled", "ContentPresenter.Background", "ContentPresenter.Foreground", "ContentPresenter.BorderBrush");
            AssertStateSetters(presenter, "Indeterminate", "ContentPresenter.Background", "ContentPresenter.Foreground", "ContentPresenter.BorderBrush");
            AssertStateSetters(presenter, "IndeterminatePointerOver", "ContentPresenter.Background", "ContentPresenter.BorderBrush", "ContentPresenter.Foreground");
            AssertStateSetters(presenter, "IndeterminatePressed", "ContentPresenter.Background", "ContentPresenter.BorderBrush", "ContentPresenter.Foreground");
            AssertStateSetters(presenter, "IndeterminateDisabled", "ContentPresenter.Background", "ContentPresenter.Foreground", "ContentPresenter.BorderBrush");
        });
    }

    [TestMethod]
    public void CheckedAndIndeterminateStatesApplySourceResources()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var toggleButton = CreateToggleButton();
            toggleButton.IsThreeState = true;
            using var host = new TestWindowHost(toggleButton, width: 140, height: 80);
            host.UpdateLayout();

            var presenter = GetContentPresenter(toggleButton);
            Assert.AreEqual("Normal", GetCurrentStateName(presenter));

            toggleButton.IsChecked = true;
            host.UpdateLayout();

            Assert.AreEqual("Checked", GetCurrentStateName(presenter));
            AssertPresenterResources(
                presenter,
                "ToggleButtonBackgroundChecked",
                "ToggleButtonBorderBrushChecked",
                "ToggleButtonForegroundChecked");
            Assert.AreEqual(BackgroundSizing.OuterBorderEdge, presenter.BackgroundSizing);

            toggleButton.IsChecked = null;
            host.UpdateLayout();

            Assert.AreEqual("Indeterminate", GetCurrentStateName(presenter));
            AssertPresenterResources(
                presenter,
                "ToggleButtonBackgroundIndeterminate",
                "ToggleButtonBorderBrushIndeterminate",
                "ToggleButtonForegroundIndeterminate");

            Assert.IsTrue(VisualStateManager.GoToState(toggleButton, "IndeterminatePointerOver", false));
            AssertPresenterResources(
                presenter,
                "ToggleButtonBackgroundIndeterminatePointerOver",
                "ToggleButtonBorderBrushIndeterminatePointerOver",
                "ToggleButtonForegroundIndeterminatePointerOver");
        });
    }

    [TestMethod]
    public void IndeterminateDisabledStateIsDrivenByHelper()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var toggleButton = CreateToggleButton();
            toggleButton.IsThreeState = true;
            toggleButton.IsChecked = null;
            using var host = new TestWindowHost(toggleButton, width: 140, height: 80);
            host.UpdateLayout();

            var presenter = GetContentPresenter(toggleButton);

            toggleButton.IsEnabled = false;
            host.UpdateLayout();

            Assert.AreEqual("IndeterminateDisabled", GetCurrentStateName(presenter));
            AssertPresenterResources(
                presenter,
                "ToggleButtonBackgroundIndeterminateDisabled",
                "ToggleButtonBorderBrushIndeterminateDisabled",
                "ToggleButtonForegroundIndeterminateDisabled");
        });
    }

    private static ToggleButton CreateToggleButton()
    {
        return new ToggleButton
        {
            Width = 100,
            Height = 40,
            Content = "Toggle"
        };
    }

    private static ContentPresenterEx GetContentPresenter(ToggleButton toggleButton)
    {
        toggleButton.ApplyTemplate();
        return toggleButton.Template.FindName("ContentPresenter", toggleButton) as ContentPresenterEx
            ?? throw new AssertFailedException("Expected ToggleButton template to use ContentPresenterEx directly.");
    }

    private static string[] CheckedStateTargets()
    {
        return new[]
        {
            "ContentPresenter.Background",
            "ContentPresenter.Foreground",
            "ContentPresenter.BorderBrush",
            "ContentPresenter.BackgroundSizing"
        };
    }

    private static void AssertStateSetters(
        FrameworkElement stateGroupsRoot,
        string stateName,
        params string[] setterTargets)
    {
        var stateEx = GetCommonState(stateGroupsRoot, stateName);
        foreach (var setterTarget in setterTargets)
        {
            Assert.IsTrue(
                stateEx.Setters.Any(item => item.Target == setterTarget),
                $"CommonStates.{stateName} should set {setterTarget}.");
        }
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
