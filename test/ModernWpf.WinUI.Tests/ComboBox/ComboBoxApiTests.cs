using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf;
using ModernWpf.Controls;
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
            comboBox.SelectedIndex = 0;

            using var host = new TestWindowHost(comboBox);
            host.UpdateLayout();

            Assert.AreEqual(HorizontalAlignment.Left, comboBox.HorizontalAlignment);
            Assert.AreEqual(VerticalAlignment.Top, comboBox.VerticalAlignment);
            Assert.IsTrue(ComboBoxHelper.GetKeepInteriorCornersSquare(comboBox));
            Assert.IsNotNull(ComboBoxHelper.GetTextBoxStyle(comboBox));
            Assert.AreEqual(new Thickness(0), comboBox.TryFindResource("ComboBoxDropdownBorderPadding"));

            ControlHelper.SetDescription(comboBox, "Pick one");
            host.UpdateLayout();

            var contentPresenter = FindTemplateChild<ContentPresenterEx>(comboBox, "ContentPresenter");
            Assert.AreEqual("Item 1", contentPresenter.Content);

            var descriptionPresenter = FindTemplateChild<ContentPresenterEx>(comboBox, "DescriptionPresenter");
            Assert.AreEqual("Pick one", descriptionPresenter.Content);
            Assert.AreEqual(Visibility.Visible, descriptionPresenter.Visibility);
            Assert.AreSame(
                descriptionPresenter.TryFindResource("SystemControlDescriptionTextForegroundBrush"),
                descriptionPresenter.Foreground);

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
    public void VerifyComboBoxItemTemplateUsesWinUIPresenterSlot()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var item = new ComboBoxItem
            {
                Content = "Item content"
            };

            using var host = new TestWindowHost(item);
            host.UpdateLayout();

            var presenter = FindTemplateChild<ContentPresenterEx>(item, "ContentPresenter");
            Assert.AreEqual("Item content", presenter.Content);
            Assert.AreSame(item.Foreground, presenter.Foreground);
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

    [TestMethod]
    public void EditableModeStatesUseVisualStateSetters()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var comboBox = CreateComboBox();
            comboBox.IsEditable = true;

            using var host = new TestWindowHost(comboBox);
            host.UpdateLayout();

            var layoutRoot = FindTemplateChild<FrameworkElement>(comboBox, "LayoutRoot");

            AssertStateSetter(layoutRoot, "TextBoxFocused", "DropDownGlyph.Foreground");
            AssertStateSetter(layoutRoot, "TextBoxFocusedOverlayPointerOver", "DropDownGlyph.Foreground");
            AssertStateSetter(layoutRoot, "TextBoxFocusedOverlayPointerOver", "DropDownOverlay.Background");
            AssertStateSetter(layoutRoot, "TextBoxFocusedOverlayPressed", "DropDownGlyph.Foreground");
            AssertStateSetter(layoutRoot, "TextBoxFocusedOverlayPressed", "DropDownOverlay.Background");
            AssertStateSetter(layoutRoot, "TextBoxOverlayPointerOver", "DropDownOverlay.Background");
            AssertStateSetter(layoutRoot, "TextBoxOverlayPressed", "DropDownOverlay.Background");
            Assert.AreEqual("TextBoxUnfocused", GetCurrentStateName(layoutRoot, "EditableModeStates"));
        });
    }

    [TestMethod]
    public void CommonStatesUseVisualStateSettersForDropDownGlyphAnimatedIconState()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var comboBox = CreateComboBox();

            using var host = new TestWindowHost(comboBox);
            host.UpdateLayout();

            var layoutRoot = FindTemplateChild<FrameworkElement>(comboBox, "LayoutRoot");
            var glyph = FindTemplateChild<FontIconFallback>(comboBox, "DropDownGlyph");

            Assert.IsTrue(ComboBoxHelper.GetVisualStateSettersEnabled(comboBox));
            AssertStateSetter(layoutRoot, "CommonStates", "PointerOver", "DropDownGlyph.(local:AnimatedIcon.State)");
            AssertStateSetter(layoutRoot, "CommonStates", "Pressed", "DropDownGlyph.(local:AnimatedIcon.State)");
            Assert.AreEqual("Normal", GetCurrentStateName(layoutRoot, "CommonStates"));
            Assert.AreEqual("Normal", AnimatedIcon.GetState(glyph));

            Assert.IsTrue(VisualStateManager.GoToState(comboBox, "PointerOver", false));
            Assert.AreEqual("PointerOver", AnimatedIcon.GetState(glyph));
            Assert.IsTrue(VisualStateManager.GoToState(comboBox, "Pressed", false));
            Assert.AreEqual("Pressed", AnimatedIcon.GetState(glyph));
            Assert.IsTrue(VisualStateManager.GoToState(comboBox, "Disabled", false));
            Assert.AreEqual("Normal", AnimatedIcon.GetState(glyph));
            Assert.IsTrue(VisualStateManager.GoToState(comboBox, "Normal", false));
            Assert.AreEqual("Normal", AnimatedIcon.GetState(glyph));
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

    private static void AssertStateSetter(
        FrameworkElement stateGroupsRoot,
        string stateName,
        string setterTarget)
    {
        AssertStateSetter(stateGroupsRoot, "EditableModeStates", stateName, setterTarget);
    }

    private static void AssertStateSetter(
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
    }

    private static string GetCurrentStateName(FrameworkElement stateGroupsRoot, string groupName)
    {
        var group = VisualStateManager.GetVisualStateGroups(stateGroupsRoot)
            .OfType<VisualStateGroup>()
            .Single(item => item.Name == groupName);
        Assert.IsNotNull(group.CurrentState);
        return group.CurrentState.Name;
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
