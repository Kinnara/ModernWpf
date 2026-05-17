using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.CommonStyles;

[TestClass]
public class ButtonVisualStateTests
{
    [TestMethod]
    public void DefaultButtonStyleUsesSourceVisualStateSetters()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var button = CreateButton("Default");
            using var host = new TestWindowHost(button, width: 140, height: 80);

            var presenter = GetContentPresenter(button);

            Assert.AreEqual(0, button.Template.Triggers.Count);
            Assert.IsTrue(ButtonHelper.GetVisualStateSettersEnabled(button));
            AssertStateSetters(presenter, "PointerOver", includeAnimatedIconState: true);
            AssertStateSetters(presenter, "Pressed", includeAnimatedIconState: true);
            AssertStateSetters(presenter, "Disabled", includeAnimatedIconState: true);
            AssertVisualStateAppliesResources(button, presenter, "PointerOver", "ButtonBackgroundPointerOver", "ButtonBorderBrushPointerOver", "ButtonForegroundPointerOver");
            AssertVisualStateAppliesResources(button, presenter, "Pressed", "ButtonBackgroundPressed", "ButtonBorderBrushPressed", "ButtonForegroundPressed");
            AssertVisualStateAppliesResources(button, presenter, "Disabled", "ButtonBackgroundDisabled", "ButtonBorderBrushDisabled", "ButtonForegroundDisabled");
            AssertAnimatedIconStateTransitions(button, presenter);
        });
    }

    [TestMethod]
    public void AccentButtonStyleUsesSourceVisualStateSettersWithoutAnimatedIconState()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var button = CreateButton("Accent");
            button.Style = (Style)Application.Current.FindResource("AccentButtonStyle");
            using var host = new TestWindowHost(button, width: 140, height: 80);

            var presenter = GetContentPresenter(button);

            Assert.AreEqual(0, button.Template.Triggers.Count);
            Assert.IsTrue(ButtonHelper.GetVisualStateSettersEnabled(button));
            Assert.AreEqual(BackgroundSizing.OuterBorderEdge, presenter.BackgroundSizing);
            AssertStateSetters(presenter, "PointerOver", includeAnimatedIconState: false);
            AssertStateSetters(presenter, "Pressed", includeAnimatedIconState: false);
            AssertStateSetters(presenter, "Disabled", includeAnimatedIconState: false);
            AssertVisualStateAppliesResources(button, presenter, "PointerOver", "AccentButtonBackgroundPointerOver", "AccentButtonBorderBrushPointerOver", "AccentButtonForegroundPointerOver");
            AssertVisualStateAppliesResources(button, presenter, "Pressed", "AccentButtonBackgroundPressed", "AccentButtonBorderBrushPressed", "AccentButtonForegroundPressed");
            AssertVisualStateAppliesResources(button, presenter, "Disabled", "AccentButtonBackgroundDisabled", "AccentButtonBorderBrushDisabled", "AccentButtonForegroundDisabled");
            Assert.IsNull(AnimatedIcon.GetState(presenter));
        });
    }

    [TestMethod]
    public void SubtleButtonStyleIsSourceBacked()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var button = CreateButton("Subtle");
            button.Style = (Style)Application.Current.FindResource("SubtleButtonStyle");
            using var host = new TestWindowHost(button, width: 140, height: 80);

            var presenter = GetContentPresenter(button);

            Assert.AreEqual(0, button.Template.Triggers.Count);
            Assert.IsTrue(ButtonHelper.GetVisualStateSettersEnabled(button));
            Assert.AreEqual(BackgroundSizing.InnerBorderEdge, presenter.BackgroundSizing);
            AssertStateSetters(presenter, "PointerOver", includeAnimatedIconState: true);
            AssertStateSetters(presenter, "Pressed", includeAnimatedIconState: true);
            AssertStateSetters(presenter, "Disabled", includeAnimatedIconState: true);
            AssertVisualStateAppliesResources(button, presenter, "PointerOver", "SubtleButtonBackgroundPointerOver", "SubtleButtonBorderBrushPointerOver", "SubtleButtonForegroundPointerOver");
            AssertVisualStateAppliesResources(button, presenter, "Pressed", "SubtleButtonBackgroundPressed", "SubtleButtonBorderBrushPressed", "SubtleButtonForegroundPressed");
            AssertVisualStateAppliesResources(button, presenter, "Disabled", "SubtleButtonBackgroundDisabled", "SubtleButtonBorderBrushDisabled", "SubtleButtonForegroundDisabled");
            AssertAnimatedIconStateTransitions(button, presenter);
        });
    }

    [TestMethod]
    public void SubtleButtonThemeResourcesMatchWinUISource()
    {
        WpfTestHost.Run(() =>
        {
            AssertSubtleTheme("Light",
                "SubtleFillColorTransparentBrush",
                "SubtleFillColorSecondaryBrush",
                "SubtleFillColorTertiaryBrush",
                "SubtleFillColorTransparentBrush",
                "TextFillColorPrimaryBrush",
                "TextFillColorPrimaryBrush",
                "TextFillColorSecondaryBrush",
                "TextFillColorDisabledBrush",
                "SubtleFillColorTransparentBrush",
                "SubtleFillColorSecondaryBrush",
                "SubtleFillColorTertiaryBrush",
                "SubtleFillColorTransparentBrush");

            AssertSubtleTheme("Dark",
                "SubtleFillColorTransparentBrush",
                "SubtleFillColorSecondaryBrush",
                "SubtleFillColorTertiaryBrush",
                "SubtleFillColorTransparentBrush",
                "TextFillColorPrimaryBrush",
                "TextFillColorPrimaryBrush",
                "TextFillColorSecondaryBrush",
                "TextFillColorDisabledBrush",
                "SubtleFillColorTransparentBrush",
                "SubtleFillColorSecondaryBrush",
                "SubtleFillColorTertiaryBrush",
                "SubtleFillColorTransparentBrush");

            AssertSubtleTheme("HighContrast",
                "SystemControlBackgroundBaseLowBrush",
                "SystemColorHighlightTextColorBrush",
                "SystemColorHighlightTextColorBrush",
                "SystemControlBackgroundBaseLowBrush",
                "SystemColorButtonTextColorBrush",
                "SystemControlHighlightBaseHighBrush",
                "SystemControlHighlightBaseHighBrush",
                "SystemControlDisabledBaseMediumLowBrush",
                "SystemControlForegroundTransparentBrush",
                "SystemColorHighlightColorBrush",
                "SystemColorHighlightTextColorBrush",
                "SystemControlDisabledTransparentBrush");
        });
    }

    private static Button CreateButton(string content)
    {
        return new Button
        {
            Width = 100,
            Height = 40,
            Content = content
        };
    }

    private static ContentPresenterEx GetContentPresenter(Button button)
    {
        button.ApplyTemplate();
        return button.Template.FindName("ContentPresenter", button) as ContentPresenterEx
            ?? throw new AssertFailedException("Expected Button template to use ContentPresenterEx directly.");
    }

    private static void AssertStateSetters(
        FrameworkElement stateGroupsRoot,
        string stateName,
        bool includeAnimatedIconState)
    {
        var stateEx = GetCommonState(stateGroupsRoot, stateName);
        AssertStateSetter(stateEx, "ContentPresenter.Background");
        AssertStateSetter(stateEx, "ContentPresenter.BorderBrush");
        AssertStateSetter(stateEx, "ContentPresenter.Foreground");

        var animatedIconSetter = stateEx.Setters.SingleOrDefault(setter => setter.Target == "ContentPresenter.(local:AnimatedIcon.State)");
        if (includeAnimatedIconState)
        {
            Assert.IsNotNull(animatedIconSetter, $"CommonStates.{stateName} should set AnimatedIcon.State.");
        }
        else
        {
            Assert.IsNull(animatedIconSetter, $"CommonStates.{stateName} should not set AnimatedIcon.State.");
        }
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

    private static void AssertVisualStateAppliesResources(
        Button button,
        ContentPresenterEx presenter,
        string stateName,
        string backgroundKey,
        string borderBrushKey,
        string foregroundKey)
    {
        Assert.IsTrue(VisualStateManager.GoToState(button, stateName, false));
        Assert.AreSame(presenter.TryFindResource(backgroundKey), presenter.Background);
        Assert.AreSame(presenter.TryFindResource(borderBrushKey), presenter.BorderBrush);
        Assert.AreSame(presenter.TryFindResource(foregroundKey), presenter.Foreground);
    }

    private static void AssertAnimatedIconStateTransitions(Button button, DependencyObject stateTarget)
    {
        Assert.AreEqual("Normal", AnimatedIcon.GetState(stateTarget));

        Assert.IsTrue(VisualStateManager.GoToState(button, "PointerOver", false));
        Assert.AreEqual("PointerOver", AnimatedIcon.GetState(stateTarget));

        Assert.IsTrue(VisualStateManager.GoToState(button, "Pressed", false));
        Assert.AreEqual("Pressed", AnimatedIcon.GetState(stateTarget));

        Assert.IsTrue(VisualStateManager.GoToState(button, "Disabled", false));
        Assert.AreEqual("Normal", AnimatedIcon.GetState(stateTarget));
    }

    private static void AssertSubtleTheme(
        string themeName,
        string background,
        string backgroundPointerOver,
        string backgroundPressed,
        string backgroundDisabled,
        string foreground,
        string foregroundPointerOver,
        string foregroundPressed,
        string foregroundDisabled,
        string borderBrush,
        string borderBrushPointerOver,
        string borderBrushPressed,
        string borderBrushDisabled)
    {
        var theme = ThemeResources.Current.GetThemeDictionary(themeName);
        AssertResourceReference(theme, "SubtleButtonBackground", background);
        AssertResourceReference(theme, "SubtleButtonBackgroundPointerOver", backgroundPointerOver);
        AssertResourceReference(theme, "SubtleButtonBackgroundPressed", backgroundPressed);
        AssertResourceReference(theme, "SubtleButtonBackgroundDisabled", backgroundDisabled);
        AssertResourceReference(theme, "SubtleButtonForeground", foreground);
        AssertResourceReference(theme, "SubtleButtonForegroundPointerOver", foregroundPointerOver);
        AssertResourceReference(theme, "SubtleButtonForegroundPressed", foregroundPressed);
        AssertResourceReference(theme, "SubtleButtonForegroundDisabled", foregroundDisabled);
        AssertResourceReference(theme, "SubtleButtonBorderBrush", borderBrush);
        AssertResourceReference(theme, "SubtleButtonBorderBrushPointerOver", borderBrushPointerOver);
        AssertResourceReference(theme, "SubtleButtonBorderBrushPressed", borderBrushPressed);
        AssertResourceReference(theme, "SubtleButtonBorderBrushDisabled", borderBrushDisabled);
    }

    private static void AssertResourceReference(ResourceDictionary theme, string key, object expectedResourceKey)
    {
        Assert.IsTrue(theme.Contains(expectedResourceKey), $"Theme is missing {expectedResourceKey}.");
        Assert.AreSame(theme[expectedResourceKey], theme[key], key);
    }
}
