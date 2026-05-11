using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf;
using ModernWpf.Controls.Primitives;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

using WpfComboBox = System.Windows.Controls.ComboBox;

namespace ModernWpf.WinUI.Tests.ComboBox;

[TestClass]
public class ComboBoxApiTests
{
    [TestMethod]
    public void VerifyComboBoxDefaultStyleAndWinUI2Resources()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var comboBox = CreateComboBox();

            using var host = new TestWindowHost(comboBox);
            host.UpdateLayout();

            Assert.AreEqual(HorizontalAlignment.Left, comboBox.HorizontalAlignment);
            Assert.AreEqual(VerticalAlignment.Top, comboBox.VerticalAlignment);
            Assert.IsTrue(ComboBoxHelper.GetKeepInteriorCornersSquare(comboBox));
            Assert.IsNotNull(ComboBoxHelper.GetTextBoxStyle(comboBox));
            Assert.AreEqual(new Thickness(0), comboBox.TryFindResource("ComboBoxDropdownBorderPadding"));

            comboBox.IsEditable = true;
            host.UpdateLayout();

            var editableTextBox = FindTemplateChild<TextBox>(comboBox, "PART_EditableTextBox");
            Assert.AreSame(ComboBoxHelper.GetTextBoxStyle(comboBox), editableTextBox.Style);

            AssertThemeResourceReference("Light", "ComboBoxDropDownBackground", "AcrylicInAppFillColorDefaultBrush");
            AssertThemeResourceReference("Dark", "ComboBoxDropDownBackground", "AcrylicInAppFillColorDefaultBrush");
            AssertThemeResourceReference("Light", "ComboBoxDropDownBorderBrush", "SurfaceStrokeColorFlyoutBrush");
            AssertThemeResourceReference("Dark", "ComboBoxDropDownBorderBrush", "SurfaceStrokeColorFlyoutBrush");
            AssertThemeResourceReference("HighContrast", "ComboBoxDropDownBackground", "SystemControlBackgroundChromeMediumLowBrush");
            AssertThemeResourceReference("HighContrast", "ComboBoxDropDownBorderBrush", "SystemControlForegroundChromeHighBrush");
            AssertThemeResourceReference("Light", "ComboBoxLightDismissOverlayBackground", "SystemControlPageBackgroundMediumAltMediumBrush");
            AssertThemeResourceReference("Dark", "ComboBoxLightDismissOverlayBackground", "SystemControlPageBackgroundMediumAltMediumBrush");
            AssertThemeResourceReference("HighContrast", "ComboBoxLightDismissOverlayBackground", "SystemControlPageBackgroundMediumAltMediumBrush");
        });
    }

    [TestMethod]
    public void VerifyComboBoxOverlayCornerRadius()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var comboBox = CreateComboBox();
            ControlHelper.SetCornerRadius(comboBox, new CornerRadius(2));

            using var host = new TestWindowHost(comboBox);

            comboBox.IsDropDownOpen = true;
            FlushLayout(host);

            var background = FindTemplateChild<Border>(comboBox, "Background");
            AssertCornerRadiusMatchesOpenDirection(
                background.CornerRadius,
                new CornerRadius(2, 2, 0, 0),
                new CornerRadius(0, 0, 2, 2));

            var overlayCornerRadius = GetOverlayCornerRadius(comboBox);
            var popupBorder = FindTemplateChild<Border>(comboBox, "PopupBorder");
            AssertCornerRadiusMatchesOpenDirection(
                popupBorder.CornerRadius,
                new CornerRadius(0, 0, overlayCornerRadius.BottomRight, overlayCornerRadius.BottomLeft),
                new CornerRadius(overlayCornerRadius.TopRight, overlayCornerRadius.TopLeft, 0, 0));
        });
    }

    [TestMethod]
    public void VerifyComboBoxEditModeCornerRadius()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var comboBox = CreateComboBox();
            ControlHelper.SetCornerRadius(comboBox, new CornerRadius(2));
            comboBox.IsEditable = true;

            using var host = new TestWindowHost(comboBox);

            comboBox.IsDropDownOpen = true;
            FlushLayout(host);

            var editableText = FindTemplateChild<TextBox>(comboBox, "PART_EditableTextBox");
            AssertCornerRadiusMatchesOpenDirection(
                ControlHelper.GetCornerRadius(editableText),
                new CornerRadius(2, 2, 0, 0),
                new CornerRadius(0, 0, 2, 2));

            var overlayCornerRadius = GetOverlayCornerRadius(comboBox);
            var popupBorder = FindTemplateChild<Border>(comboBox, "PopupBorder");
            AssertCornerRadiusMatchesOpenDirection(
                popupBorder.CornerRadius,
                new CornerRadius(0, 0, overlayCornerRadius.BottomRight, overlayCornerRadius.BottomLeft),
                new CornerRadius(overlayCornerRadius.TopRight, overlayCornerRadius.TopLeft, 0, 0));
        });
    }

    private static WpfComboBox CreateComboBox()
    {
        var comboBox = new WpfComboBox();
        comboBox.Items.Add("Item 1");
        comboBox.Items.Add("Item 2");
        comboBox.Items.Add("Item 3");
        comboBox.Items.Add("Item 4");
        comboBox.Items.Add("Item 5");
        comboBox.Items.Add("Item 6");
        return comboBox;
    }

    private static void FlushLayout(TestWindowHost host)
    {
        host.UpdateLayout();
        WpfTestHost.DoEvents();
        host.UpdateLayout();
    }

    private static T FindTemplateChild<T>(Control control, string name)
        where T : FrameworkElement
    {
        return control.Template?.FindName(name, control) as T
            ?? throw new InvalidOperationException($"Could not find template child '{name}'.");
    }

    private static void AssertThemeResourceReference(string themeName, string resourceKey, string expectedResourceKey)
    {
        var themeDictionary = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(themeDictionary.Contains(resourceKey), $"{themeName} is missing {resourceKey}.");
        Assert.IsTrue(themeDictionary.Contains(expectedResourceKey), $"{themeName} is missing {expectedResourceKey}.");
        Assert.AreSame(themeDictionary[expectedResourceKey], themeDictionary[resourceKey], $"{themeName}:{resourceKey}");
    }

    private static CornerRadius GetOverlayCornerRadius(FrameworkElement element)
    {
        return element.TryFindResource("OverlayCornerRadius") is CornerRadius radius
            ? radius
            : default;
    }

    private static void AssertCornerRadiusMatchesOpenDirection(
        CornerRadius actual,
        CornerRadius openDownExpected,
        CornerRadius openUpExpected)
    {
        Assert.IsTrue(
            actual == openDownExpected || actual == openUpExpected,
            $"Expected {openDownExpected} or {openUpExpected}, got {actual}.");
    }
}
