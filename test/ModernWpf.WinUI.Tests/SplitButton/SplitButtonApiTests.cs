using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.SplitButton;

[TestClass]
public class SplitButtonApiTests
{
    [TestMethod]
    public void VerifyDefaultStyleAndWinUI2Resources()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var splitButton = new ModernWpf.Controls.SplitButton
            {
                Content = "Split",
                Width = 220,
                Height = 40
            };

            using var host = new TestWindowHost(splitButton, width: 360, height: 180);
            host.UpdateLayout();

            Assert.AreSame(splitButton.TryFindResource("SplitButtonBackground"), splitButton.Background);
            Assert.AreSame(splitButton.TryFindResource("SplitButtonForeground"), splitButton.Foreground);
            Assert.AreSame(splitButton.TryFindResource("SplitButtonBorderBrush"), splitButton.BorderBrush);
            Assert.AreEqual(splitButton.TryFindResource("SplitButtonBorderThemeThickness"), splitButton.BorderThickness);
            Assert.AreEqual(HorizontalAlignment.Left, splitButton.HorizontalAlignment);
            Assert.AreEqual(VerticalAlignment.Center, splitButton.VerticalAlignment);
            Assert.AreEqual(HorizontalAlignment.Center, splitButton.HorizontalContentAlignment);
            Assert.AreEqual(VerticalAlignment.Center, splitButton.VerticalContentAlignment);
            Assert.AreEqual(new Thickness(11, 6, 11, 7), splitButton.Padding);
            Assert.AreEqual(splitButton.TryFindResource("ControlCornerRadius"), splitButton.CornerRadius);
            Assert.IsTrue(splitButton.UseSystemFocusVisuals);
            Assert.AreEqual(new Thickness(-1), splitButton.FocusVisualMargin);
            Assert.IsTrue(splitButton.IsTabStop);
            var primaryButton = FindTemplatePart<System.Windows.Controls.Button>(splitButton, "PrimaryButton");
            var secondaryButton = FindTemplatePart<System.Windows.Controls.Button>(splitButton, "SecondaryButton");
            var primaryColumn = FindTemplatePart<System.Windows.Controls.ColumnDefinition>(splitButton, "PrimaryButtonColumn");
            var secondaryColumn = FindTemplatePart<System.Windows.Controls.ColumnDefinition>(splitButton, "SecondaryButtonColumn");
            var primaryBackground = FindTemplatePart<Grid>(splitButton, "PrimaryBackgroundGrid");
            var primaryButtonBorder = FindTemplatePart<GridEx>(splitButton, "PrimaryButtonBorder");
            var secondaryButtonBorder = FindTemplatePart<GridEx>(splitButton, "SecondaryButtonBorder");
            var rootGrid = VisualTreeTestHelper.FindDescendant<GridEx>(splitButton)
                ?? throw new AssertFailedException("Expected SplitButton template root to use GridEx chrome.");

            Assert.AreSame(splitButton.Background, primaryButton.Background);
            Assert.AreSame(splitButton.Foreground, primaryButton.Foreground);
            Assert.AreSame(splitButton.Background, secondaryButton.Background);
            Assert.AreSame(splitButton.TryFindResource("SplitButtonForegroundSecondary"), secondaryButton.Foreground);
            Assert.AreEqual(35d, primaryColumn.MinWidth);
            Assert.AreEqual(new GridLength(35d), secondaryColumn.Width);
            Assert.AreEqual(splitButton.CornerRadius, rootGrid.CornerRadius);
            Assert.AreEqual(new Thickness(1, 1, 0, 1), primaryButtonBorder.BorderThickness);
            Assert.AreEqual(new CornerRadius(4, 0, 0, 4), primaryButtonBorder.CornerRadius);
            Assert.AreEqual(new Thickness(0, 1, 1, 1), secondaryButtonBorder.BorderThickness);
            Assert.AreEqual(new CornerRadius(0, 4, 4, 0), secondaryButtonBorder.CornerRadius);
            Assert.AreEqual(2, Grid.GetColumnSpan(primaryBackground));
            Assert.IsNull(splitButton.Template?.FindName("Border", splitButton));

            foreach (var themeName in new[] { "Light", "Dark" })
            {
                AssertThemeResourceReference(themeName, "SplitButtonBackground", "ControlFillColorDefaultBrush");
                AssertThemeResourceReference(themeName, "SplitButtonBackgroundPointerOver", "ControlFillColorSecondaryBrush");
                AssertThemeResourceReference(themeName, "SplitButtonBackgroundPressed", "ControlFillColorTertiaryBrush");
                AssertThemeResourceReference(themeName, "SplitButtonBackgroundDisabled", "ControlFillColorDisabledBrush");
                AssertThemeResourceReference(themeName, "SplitButtonBackgroundChecked", "AccentFillColorDefaultBrush");
                AssertThemeResourceReference(themeName, "SplitButtonBackgroundCheckedPointerOver", "AccentFillColorSecondaryBrush");
                AssertThemeResourceReference(themeName, "SplitButtonBackgroundCheckedPressed", "AccentFillColorTertiaryBrush");
                AssertThemeResourceReference(themeName, "SplitButtonBackgroundCheckedDisabled", "AccentFillColorDisabledBrush");
                AssertThemeResourceReference(themeName, "SplitButtonForeground", "TextFillColorPrimaryBrush");
                AssertThemeResourceReference(themeName, "SplitButtonForegroundPointerOver", "TextFillColorPrimaryBrush");
                AssertThemeResourceReference(themeName, "SplitButtonForegroundPressed", "TextFillColorSecondaryBrush");
                AssertThemeResourceReference(themeName, "SplitButtonForegroundDisabled", "TextFillColorDisabledBrush");
                AssertThemeResourceReference(themeName, "SplitButtonForegroundChecked", "TextOnAccentFillColorPrimaryBrush");
                AssertThemeResourceReference(themeName, "SplitButtonForegroundCheckedPointerOver", "TextOnAccentFillColorPrimaryBrush");
                AssertThemeResourceReference(themeName, "SplitButtonForegroundCheckedPressed", "TextOnAccentFillColorSecondaryBrush");
                AssertThemeResourceReference(themeName, "SplitButtonForegroundCheckedDisabled", "TextFillColorDisabledBrush");
                AssertThemeResourceReference(themeName, "SplitButtonForegroundSecondary", "TextFillColorSecondaryBrush");
                AssertThemeResourceReference(themeName, "SplitButtonForegroundSecondaryPressed", "TextFillColorTertiaryBrush");
                AssertThemeResourceReference(themeName, "SplitButtonBorderBrush", "ControlElevationBorderBrush");
                AssertThemeResourceReference(themeName, "SplitButtonBorderBrushPointerOver", "ControlElevationBorderBrush");
                AssertThemeResourceReference(themeName, "SplitButtonBorderBrushPressed", "ControlStrokeColorDefaultBrush");
                AssertThemeResourceReference(themeName, "SplitButtonBorderBrushDisabled", "ControlStrokeColorDefaultBrush");
                AssertThemeResourceReference(themeName, "SplitButtonBorderBrushDivider", "ControlStrokeColorDefaultBrush");
                AssertThemeResourceReference(themeName, "SplitButtonBorderBrushChecked", "AccentControlElevationBorderBrush");
                AssertThemeResourceReference(themeName, "SplitButtonBorderBrushCheckedPointerOver", "AccentControlElevationBorderBrush");
                AssertThemeResourceReference(themeName, "SplitButtonBorderBrushCheckedPressed", "ControlFillColorTransparentBrush");
                AssertThemeResourceReference(themeName, "SplitButtonBorderBrushCheckedDisabled", "ControlFillColorTransparentBrush");
                AssertThemeResourceReference(themeName, "SplitButtonBorderBrushCheckedDivider", "ControlStrokeColorOnAccentTertiaryBrush");
                AssertThemeResourceReference(themeName, "SplitButtonInAppBarUnfocusedPointerOver", "SubtleFillColorTertiaryBrush");
                AssertThemeResourceValue(themeName, "SplitButtonBorderThemeThickness", new Thickness(1));
            }

            AssertThemeResourceReference("HighContrast", "SplitButtonBackground", "SystemColorButtonFaceColorBrush");
            AssertThemeResourceReference("HighContrast", "SplitButtonBackgroundPointerOver", "SystemColorHighlightTextColorBrush");
            AssertThemeResourceReference("HighContrast", "SplitButtonBackgroundPressed", "SystemColorHighlightTextColorBrush");
            AssertThemeResourceReference("HighContrast", "SplitButtonBackgroundDisabled", "SystemColorWindowColorBrush");
            AssertThemeResourceReference("HighContrast", "SplitButtonBackgroundChecked", "SystemColorHighlightColorBrush");
            AssertThemeResourceReference("HighContrast", "SplitButtonBackgroundCheckedPointerOver", "SystemColorButtonTextColorBrush");
            AssertThemeResourceReference("HighContrast", "SplitButtonBackgroundCheckedPressed", "SystemColorButtonFaceColorBrush");
            AssertThemeResourceReference("HighContrast", "SplitButtonBackgroundCheckedDisabled", "SystemColorGrayTextColorBrush");
            AssertThemeResourceReference("HighContrast", "SplitButtonForeground", "SystemColorButtonTextColorBrush");
            AssertThemeResourceReference("HighContrast", "SplitButtonForegroundPointerOver", "SystemColorHighlightColorBrush");
            AssertThemeResourceReference("HighContrast", "SplitButtonForegroundPressed", "SystemColorHighlightColorBrush");
            AssertThemeResourceReference("HighContrast", "SplitButtonForegroundDisabled", "SystemColorGrayTextColorBrush");
            AssertThemeResourceReference("HighContrast", "SplitButtonForegroundChecked", "SystemColorHighlightTextColorBrush");
            AssertThemeResourceReference("HighContrast", "SplitButtonForegroundCheckedPointerOver", "SystemColorButtonFaceColorBrush");
            AssertThemeResourceReference("HighContrast", "SplitButtonForegroundCheckedPressed", "SystemColorButtonTextColorBrush");
            AssertThemeResourceReference("HighContrast", "SplitButtonForegroundCheckedDisabled", "SystemColorWindowColorBrush");
            AssertThemeResourceReference("HighContrast", "SplitButtonForegroundSecondary", "SystemColorButtonTextColorBrush");
            AssertThemeResourceReference("HighContrast", "SplitButtonForegroundSecondaryPressed", "SystemColorHighlightColorBrush");
            AssertThemeResourceReference("HighContrast", "SplitButtonBorderBrush", "SystemControlForegroundTransparentBrush");
            AssertThemeResourceReference("HighContrast", "SplitButtonBorderBrushPointerOver", "SystemColorHighlightColorBrush");
            AssertThemeResourceReference("HighContrast", "SplitButtonBorderBrushPressed", "SystemColorHighlightTextColorBrush");
            AssertThemeResourceReference("HighContrast", "SplitButtonBorderBrushDisabled", "SystemControlDisabledTransparentBrush");
            AssertThemeResourceReference("HighContrast", "SplitButtonBorderBrushDivider", "SystemControlDisabledBaseMediumLowBrush");
            AssertThemeResourceReference("HighContrast", "SplitButtonBorderBrushChecked", "SystemColorButtonTextColorBrush");
            AssertThemeResourceReference("HighContrast", "SplitButtonBorderBrushCheckedPointerOver", "SystemColorButtonTextColorBrush");
            AssertThemeResourceReference("HighContrast", "SplitButtonBorderBrushCheckedPressed", "SystemColorButtonFaceColorBrush");
            AssertThemeResourceReference("HighContrast", "SplitButtonBorderBrushCheckedDisabled", "SystemColorGrayTextColorBrush");
            AssertThemeResourceReference("HighContrast", "SplitButtonBorderBrushCheckedDivider", "SystemColorHighlightTextColorBrush");
            AssertThemeResourceReference("HighContrast", "SplitButtonInAppBarUnfocusedPointerOver", "SystemColorButtonFaceColorBrush");
            AssertThemeResourceValue("HighContrast", "SplitButtonBorderThemeThickness", new Thickness(1));
        });
    }

    [TestMethod]
    public void VerifyDefaultsAndBasicSetting()
    {
        WpfTestHost.Run(() =>
        {
            var flyout = new Flyout();
            var command = new TestCommand();
            var transitions = new ModernWpf.Media.Animation.TransitionCollection();
            const int parameter = 0;

            var splitButton = new ModernWpf.Controls.SplitButton();
            Assert.IsNotNull(splitButton);

            Assert.IsNull(splitButton.Flyout);
            Assert.IsNull(splitButton.Command);
            Assert.IsNull(splitButton.CommandParameter);
            Assert.IsNull(splitButton.ContentTransitions);

            splitButton.Flyout = flyout;
            splitButton.Command = command;
            splitButton.CommandParameter = parameter;
            splitButton.ContentTransitions = transitions;

            WpfTestHost.DoEvents();

            Assert.AreSame(flyout, splitButton.Flyout);
            Assert.AreSame(command, splitButton.Command);
            Assert.AreEqual(parameter, splitButton.CommandParameter);
            Assert.AreSame(transitions, splitButton.ContentTransitions);
        });
    }

    [TestMethod]
    public void VerifySplitButtonTemplateUsesWinUIContentPresenterShape()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var transitions = new ModernWpf.Media.Animation.TransitionCollection();
            var splitButton = new ModernWpf.Controls.SplitButton
            {
                Content = "Split",
                ContentTransitions = transitions,
                Width = 220,
                Height = 40
            };

            using var host = new TestWindowHost(splitButton, width: 360, height: 180);
            host.UpdateLayout();

            var primaryButton = FindTemplatePart<System.Windows.Controls.Button>(splitButton, "PrimaryButton");
            var presenter = VisualTreeTestHelper.FindDescendant<ContentPresenterEx>(primaryButton)
                ?? throw new AssertFailedException("Expected SplitButton primary button template to use ContentPresenterEx.");
            var primaryButtonRoot = GetButtonTemplateRoot(primaryButton);

            Assert.AreEqual("Split", presenter.Content);
            Assert.AreSame(transitions, presenter.ContentTransitions);
            Assert.IsNull(VisualTreeTestHelper.FindDescendant<ContentControlEx>(primaryButton));
            Assert.IsTrue(ButtonHelper.GetVisualStateSettersEnabled(primaryButton));
            AssertAnimatedIconStateSetters(primaryButtonRoot);
            AssertAnimatedIconStateTransitions(primaryButton, presenter);

            var secondaryButton = FindTemplatePart<System.Windows.Controls.Button>(splitButton, "SecondaryButton");
            var secondaryPresenter = VisualTreeTestHelper.FindDescendant<ContentPresenterEx>(secondaryButton)
                ?? throw new AssertFailedException("Expected SplitButton secondary button template to use ContentPresenterEx.");
            var secondaryButtonRoot = GetButtonTemplateRoot(secondaryButton);

            Assert.IsTrue(ButtonHelper.GetVisualStateSettersEnabled(secondaryButton));
            AssertAnimatedIconStateSetters(secondaryButtonRoot);
            AssertAnimatedIconStateTransitions(secondaryButton, secondaryPresenter);
        });
    }

    [TestMethod]
    public void TemplateUsesVisualStateSettersForWinUIStateParity()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var splitButton = new ToggleSplitButton
            {
                Content = "Split",
                Width = 220,
                Height = 40,
                IsChecked = true
            };

            using var host = new TestWindowHost(splitButton, width: 360, height: 180);
            host.UpdateLayout();

            var root = FindTemplatePart<GridEx>(splitButton, "RootGrid");

            AssertStateSetter(root, "CommonStates", "Disabled", "PrimaryButtonBorder.BorderBrush");
            AssertStateSetter(root, "CommonStates", "Disabled", "SecondaryButtonBorder.BorderBrush");
            AssertStateSetter(root, "CommonStates", "FlyoutOpen", "PrimaryBackgroundGrid.Background");
            AssertStateSetter(root, "CommonStates", "FlyoutOpen", "SecondaryButtonBorder.BorderBrush");
            AssertStateSetter(root, "CommonStates", "TouchPressed", "SecondaryButton.Foreground");
            AssertStateSetter(root, "CommonStates", "TouchPressed", "PrimaryButtonBorder.BorderBrush");
            AssertStateSetter(root, "CommonStates", "PrimaryPointerOver", "PrimaryButton.Foreground");
            AssertStateSetter(root, "CommonStates", "PrimaryPressed", "PrimaryBackgroundGrid.Background");
            AssertStateSetter(root, "CommonStates", "PrimaryPressed", "PrimaryButtonBorder.BorderBrush");
            AssertStateSetter(root, "CommonStates", "SecondaryPointerOver", "SecondaryButton.BorderBrush");
            AssertStateSetter(root, "CommonStates", "SecondaryPressed", "SecondaryButton.Foreground");
            AssertStateSetter(root, "CommonStates", "SecondaryPressed", "SecondaryButtonBorder.BorderBrush");
            AssertStateSetter(root, "CommonStates", "Checked", "DividerBackgroundGrid.Background");
            AssertStateSetter(root, "CommonStates", "Checked", "PrimaryButtonBorder.BorderBrush");
            AssertStateSetter(root, "CommonStates", "CheckedFlyoutOpen", "SecondaryButtonBorder.BorderBrush");
            AssertStateSetter(root, "CommonStates", "CheckedTouchPressed", "PrimaryButtonBorder.BorderBrush");
            AssertStateSetter(root, "CommonStates", "CheckedPrimaryPointerOver", "PrimaryBackgroundGrid.Background");
            AssertStateSetter(root, "CommonStates", "CheckedPrimaryPointerOver", "SecondaryButtonBorder.BorderBrush");
            AssertStateSetter(root, "CommonStates", "CheckedPrimaryPressed", "PrimaryButton.Foreground");
            AssertStateSetter(root, "CommonStates", "CheckedPrimaryPressed", "PrimaryButtonBorder.BorderBrush");
            AssertStateSetter(root, "CommonStates", "CheckedSecondaryPointerOver", "SecondaryButton.Foreground");
            AssertStateSetter(root, "CommonStates", "CheckedSecondaryPointerOver", "SecondaryButtonBorder.BorderBrush");
            AssertStateSetter(root, "CommonStates", "CheckedSecondaryPressed", "SecondaryBackgroundGrid.Background");
            AssertStateSetter(root, "CommonStates", "CheckedSecondaryPressed", "SecondaryButtonBorder.BorderBrush");
            AssertStateSetter(root, "SecondaryButtonPlacementStates", "SecondaryButtonSpan", "SecondaryButton.(Grid.Column)");
            AssertStateSetter(root, "SecondaryButtonPlacementStates", "SecondaryButtonSpan", "SecondaryButton.(Grid.ColumnSpan)");

            var primaryButton = FindTemplatePart<System.Windows.Controls.Button>(splitButton, "PrimaryButton");
            primaryButton.ApplyTemplate();
            var primaryButtonRoot = GetButtonTemplateRoot(primaryButton);

            AssertStateSetter(primaryButtonRoot, "CommonStates", "PointerOver", "ContentPresenter.(ui:AnimatedIcon.State)");
            AssertStateSetter(primaryButtonRoot, "CommonStates", "Pressed", "ContentPresenter.(ui:AnimatedIcon.State)");
            AssertStateSetter(primaryButtonRoot, "CommonStates", "Disabled", "ContentPresenter.Foreground");
            AssertStateSetter(primaryButtonRoot, "CommonStates", "Disabled", "RootGrid.Background");
            AssertStateSetter(primaryButtonRoot, "CommonStates", "Disabled", "ContentPresenter.BorderBrush");
        });
    }

    [TestMethod]
    public void VerifyIsCheckedProperty()
    {
        WpfTestHost.Run(() =>
        {
            var toggleSplitButton = new ToggleSplitButton();

            Assert.IsFalse(toggleSplitButton.IsChecked, "ToggleSplitButton is not unchecked");

            toggleSplitButton.SetValue(ToggleSplitButton.IsCheckedProperty, true);

            Assert.IsTrue((bool)toggleSplitButton.GetValue(ToggleSplitButton.IsCheckedProperty), "ToggleSplitButton is not checked");
        });
    }

    private static T FindTemplatePart<T>(ModernWpf.Controls.SplitButton splitButton, string name)
        where T : class
    {
        splitButton.ApplyTemplate();

        return splitButton.Template?.FindName(name, splitButton) as T
            ?? throw new AssertFailedException($"Could not find SplitButton template part '{name}'.");
    }

    private static void AssertStateSetter(
        FrameworkElement stateGroupsRoot,
        string groupName,
        string stateName,
        string? expectedTarget,
        string? expectedProperty = null)
    {
        var group = FindVisualStateGroup(stateGroupsRoot, groupName);
        Assert.IsNotNull(group, $"Expected visual state group '{groupName}'.");

        var state = FindVisualState(group!, stateName);
        Assert.IsNotNull(state, $"Expected visual state '{groupName}.{stateName}'.");
        Assert.IsInstanceOfType(state, typeof(VisualStateEx));

        var stateEx = (VisualStateEx)state!;
        foreach (VisualStateSetter setter in stateEx.Setters)
        {
            bool targetMatches = expectedTarget is null ?
                string.IsNullOrEmpty(setter.Target) :
                setter.Target == expectedTarget;
            bool propertyMatches = expectedProperty is null ?
                string.IsNullOrEmpty(setter.Property) :
                setter.Property == expectedProperty;

            if (targetMatches && propertyMatches)
            {
                return;
            }
        }

        Assert.Fail(
            $"Expected visual state '{groupName}.{stateName}' to contain setter '{expectedTarget ?? expectedProperty}'.");
    }

    private static VisualStateGroup? FindVisualStateGroup(FrameworkElement stateGroupsRoot, string groupName)
    {
        foreach (VisualStateGroup group in VisualStateManager.GetVisualStateGroups(stateGroupsRoot))
        {
            if (group.Name == groupName)
            {
                return group;
            }
        }

        return null;
    }

    private static VisualState? FindVisualState(VisualStateGroup group, string stateName)
    {
        foreach (VisualState state in group.States)
        {
            if (state.Name == stateName)
            {
                return state;
            }
        }

        return null;
    }

    private static FrameworkElement GetButtonTemplateRoot(System.Windows.Controls.Button button)
    {
        button.ApplyTemplate();

        return button.Template?.FindName("RootGrid", button) as FrameworkElement
            ?? throw new AssertFailedException("Expected SplitButton inner button template root.");
    }

    private static void AssertAnimatedIconStateSetters(FrameworkElement stateGroupsRoot)
    {
        AssertAnimatedIconStateSetter(stateGroupsRoot, "PointerOver", "PointerOver");
        AssertAnimatedIconStateSetter(stateGroupsRoot, "Pressed", "Pressed");
    }

    private static void AssertAnimatedIconStateSetter(
        FrameworkElement stateGroupsRoot,
        string stateName,
        string expectedValue)
    {
        var group = FindVisualStateGroup(stateGroupsRoot, "CommonStates");
        Assert.IsNotNull(group, "Expected visual state group 'CommonStates'.");

        var state = FindVisualState(group!, stateName);
        Assert.IsNotNull(state, $"Expected visual state 'CommonStates.{stateName}'.");
        Assert.IsInstanceOfType(state, typeof(VisualStateEx));

        var stateEx = (VisualStateEx)state!;
        var setter = stateEx.Setters.Single(item => item.Target == "ContentPresenter.(ui:AnimatedIcon.State)");

        Assert.AreEqual(expectedValue, setter.Value);
    }

    private static void AssertAnimatedIconStateTransitions(ButtonBase button, DependencyObject stateTarget)
    {
        Assert.AreEqual("Normal", AnimatedIcon.GetState(stateTarget));
        Assert.IsTrue(VisualStateManager.GoToState(button, "PointerOver", false));
        Assert.AreEqual("PointerOver", AnimatedIcon.GetState(stateTarget));
        Assert.IsTrue(VisualStateManager.GoToState(button, "Pressed", false));
        Assert.AreEqual("Pressed", AnimatedIcon.GetState(stateTarget));
        Assert.IsTrue(VisualStateManager.GoToState(button, "Disabled", false));
        Assert.AreEqual("Normal", AnimatedIcon.GetState(stateTarget));
        Assert.IsTrue(VisualStateManager.GoToState(button, "Normal", false));
        Assert.AreEqual("Normal", AnimatedIcon.GetState(stateTarget));
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

#pragma warning disable CS0067
    private sealed class TestCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
        }
    }
#pragma warning restore CS0067
}
