using System;
using System.Windows;
using System.Windows.Input;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf;
using ModernWpf.Controls;
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
            Assert.AreEqual(splitButton.TryFindResource("ButtonPadding"), splitButton.Padding);
            Assert.AreEqual(splitButton.TryFindResource("ControlCornerRadius"), splitButton.CornerRadius);
            Assert.IsTrue(splitButton.UseSystemFocusVisuals);
            Assert.AreEqual(new Thickness(-1), splitButton.FocusVisualMargin);
            Assert.IsTrue(splitButton.IsTabStop);
            var primaryButton = FindTemplatePart<System.Windows.Controls.Button>(splitButton, "PrimaryButton");
            var secondaryButton = FindTemplatePart<System.Windows.Controls.Button>(splitButton, "SecondaryButton");
            var primaryColumn = FindTemplatePart<System.Windows.Controls.ColumnDefinition>(splitButton, "PrimaryButtonColumn");
            var secondaryColumn = FindTemplatePart<System.Windows.Controls.ColumnDefinition>(splitButton, "SecondaryButtonColumn");
            var rootGrid = VisualTreeTestHelper.FindDescendant<GridEx>(splitButton)
                ?? throw new AssertFailedException("Expected SplitButton template root to use GridEx chrome.");

            Assert.AreSame(splitButton.Background, primaryButton.Background);
            Assert.AreSame(splitButton.Foreground, primaryButton.Foreground);
            Assert.AreSame(splitButton.Background, secondaryButton.Background);
            Assert.AreSame(splitButton.TryFindResource("SplitButtonForegroundSecondary"), secondaryButton.Foreground);
            Assert.AreEqual(35d, primaryColumn.MinWidth);
            Assert.AreEqual(new GridLength(35d), secondaryColumn.Width);
            Assert.AreEqual(splitButton.CornerRadius, rootGrid.CornerRadius);

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
                AssertThemeResourceReference(themeName, "SplitButtonBorderBrushPressed", "ControlElevationBorderBrush");
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
            AssertThemeResourceReference("HighContrast", "SplitButtonBorderBrushPressed", "SystemColorButtonTextColorBrush");
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

            Assert.AreEqual("Split", presenter.Content);
            Assert.AreSame(transitions, presenter.ContentTransitions);
            Assert.IsNull(VisualTreeTestHelper.FindDescendant<ContentControlEx>(primaryButton));
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
