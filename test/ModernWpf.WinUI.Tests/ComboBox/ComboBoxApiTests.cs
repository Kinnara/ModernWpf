using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

using WpfComboBox = System.Windows.Controls.ComboBox;
using Rectangle = System.Windows.Shapes.Rectangle;

namespace ModernWpf.WinUI.Tests.ComboBox;

[TestClass]
public class ComboBoxApiTests
{
    [TestMethod]
    public void VerifyComboBoxDefaultStyleMatchesOfficialWpfFluent()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var comboBox = CreateComboBox();
            comboBox.SelectedIndex = 0;
            comboBox.SetValue(System.Windows.Controls.Border.CornerRadiusProperty, new CornerRadius(6));

            using var host = new TestWindowHost(comboBox);
            host.UpdateLayout();

            var implicitStyle = comboBox.Style;
            var defaultStyle = AssertStyle(comboBox, "DefaultComboBoxStyle");
            Assert.AreSame(defaultStyle, implicitStyle.BasedOn);
            AssertComboBoxStyleSetters(defaultStyle);
            AssertComboBoxTemplateResourceSetters(comboBox.Template);

            Assert.AreSame(comboBox.TryFindResource("ComboBoxForeground"), comboBox.Foreground);
            Assert.AreSame(comboBox.TryFindResource("ComboBoxBackground"), comboBox.Background);
            Assert.AreSame(comboBox.TryFindResource("ComboBoxBorderBrush"), comboBox.BorderBrush);
            Assert.AreEqual(comboBox.TryFindResource("ComboBoxBorderThemeThickness"), comboBox.BorderThickness);
            Assert.AreEqual(HorizontalAlignment.Stretch, comboBox.HorizontalAlignment);
            Assert.AreEqual(VerticalAlignment.Center, comboBox.VerticalAlignment);
            Assert.AreEqual(comboBox.TryFindResource("TextControlThemeMinHeight"), comboBox.MinHeight);
            Assert.AreEqual(comboBox.TryFindResource("TextControlThemeMinWidth"), comboBox.MinWidth);
            Assert.AreEqual(comboBox.TryFindResource("ComboBoxPadding"), comboBox.Padding);
            Assert.AreSame(comboBox.TryFindResource("DefaultControlContextMenu"), comboBox.ContextMenu);
            Assert.IsNotNull(comboBox.FocusVisualStyle);

            var contentBorder = FindTemplateChild<Border>(comboBox, "ContentBorder");
            Assert.AreEqual(new CornerRadius(6), contentBorder.CornerRadius);

            var contentPresenter = FindTemplateChild<ContentPresenter>(comboBox, "PART_ContentPresenter");
            Assert.AreEqual(typeof(ContentPresenter), contentPresenter.GetType());
            Assert.AreEqual("Item 1", contentPresenter.Content);
            Assert.AreSame(comboBox.Foreground, TextElement.GetForeground(contentPresenter));

            var toggleButton = FindTemplateChild<ToggleButton>(comboBox, "ToggleButton");
            Assert.IsFalse(toggleButton.Focusable);
            var chevronIcon = FindTemplateChild<TextBlock>(comboBox, "ChevronIcon");
            Assert.AreEqual(comboBox.TryFindResource("SymbolThemeFontFamily"), chevronIcon.FontFamily);
            Assert.AreEqual(comboBox.TryFindResource("ComboBoxChevronDownGlyph"), chevronIcon.Text);
            Assert.AreEqual(comboBox.TryFindResource("ComboBoxChevronSize"), chevronIcon.FontSize);
            Assert.IsNotNull(FindTemplateChild<Popup>(comboBox, "PART_Popup"));
            var dropDownBorder = FindTemplateChild<Border>(comboBox, "DropDownBorder");
            Assert.AreSame(dropDownBorder.TryFindResource("ComboBoxDropDownBackground"), dropDownBorder.Background);
            Assert.AreSame(dropDownBorder.TryFindResource("ComboBoxDropDownBorderBrush"), dropDownBorder.BorderBrush);
            Assert.AreEqual(dropDownBorder.TryFindResource("PopupCornerRadius"), dropDownBorder.CornerRadius);
            Assert.IsTrue(comboBox.Template.Triggers.Count > 0);

            comboBox.IsEnabled = false;
            host.UpdateLayout();
            Assert.AreSame(contentBorder.TryFindResource("ComboBoxBackgroundDisabled"), contentBorder.Background);
            Assert.AreSame(contentBorder.TryFindResource("ComboBoxBorderBrushDisabled"), contentBorder.BorderBrush);
            Assert.AreSame(contentPresenter.TryFindResource("ComboBoxForegroundDisabled"), TextElement.GetForeground(contentPresenter));
            Assert.AreSame(chevronIcon.TryFindResource("ComboBoxDropDownGlyphForegroundDisabled"), chevronIcon.Foreground);

            Assert.IsNull(VisualTreeTestHelper.FindDescendant<ContentPresenterEx>(comboBox));
            Assert.IsNull(VisualTreeTestHelper.FindDescendant<FontIconFallback>(comboBox));
        });
    }

    [TestMethod]
    public void VerifyComboBoxItemTemplateUsesOfficialWpfPresenter()
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

            var implicitStyle = item.Style;
            var defaultStyle = AssertStyle(item, "DefaultComboBoxItemStyle");
            Assert.AreSame(defaultStyle, implicitStyle.BasedOn);
            AssertComboBoxItemStyleSetters(defaultStyle);
            Assert.AreSame(item.TryFindResource("ComboBoxForeground"), item.Foreground);
            Assert.AreEqual(Brushes.Transparent, item.Background);
            Assert.AreEqual(item.TryFindResource("ComboBoxItemMargin"), item.Margin);
            Assert.AreEqual(item.TryFindResource("ComboBoxItemContentMargin"), item.Padding);
            Assert.AreEqual(item.TryFindResource("ControlCornerRadius"), item.GetValue(System.Windows.Controls.Border.CornerRadiusProperty));

            var presenter = FindTemplateChild<ContentPresenter>(item, "PART_ContentPresenter");
            Assert.AreEqual(typeof(ContentPresenter), presenter.GetType());
            Assert.AreEqual("Item content", presenter.Content);

            var activeRectangle = FindTemplateChild<Rectangle>(item, "ActiveRectangle");
            Assert.AreSame(activeRectangle.TryFindResource("ComboBoxItemPillFillBrush"), activeRectangle.Fill);
            Assert.AreEqual(Visibility.Collapsed, activeRectangle.Visibility);

            item.IsSelected = true;
            host.UpdateLayout();
            Assert.AreEqual(Visibility.Visible, activeRectangle.Visibility);
            Assert.AreSame(presenter.TryFindResource("ComboBoxItemForegroundSelected"), TextElement.GetForeground(presenter));

            item.IsEnabled = false;
            host.UpdateLayout();
            Assert.AreSame(item.TryFindResource("ComboBoxForegroundDisabled"), item.Foreground);

            Assert.IsNull(VisualTreeTestHelper.FindDescendant<ContentPresenterEx>(item));
        });
    }

    [TestMethod]
    public void ComboBoxPlaceholderTextIsVisibleOnlyWithoutASelection()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var comboBox = CreateComboBox();
            ControlHelper.SetPlaceholderText(comboBox, "Pick a color");

            using var host = new TestWindowHost(comboBox);
            host.UpdateLayout();

            var placeholder = FindTemplateChild<TextBlock>(comboBox, "PlaceholderTextContentPresenter");
            Assert.AreEqual("Pick a color", placeholder.Text);
            Assert.AreEqual(Visibility.Visible, placeholder.Visibility);
            Assert.AreSame(
                placeholder.TryFindResource("ComboBoxPlaceHolderForeground"),
                placeholder.Foreground);

            comboBox.SelectedIndex = 0;
            host.UpdateLayout();
            Assert.AreEqual(Visibility.Collapsed, placeholder.Visibility);

            comboBox.SelectedIndex = -1;
            host.UpdateLayout();
            Assert.AreEqual(Visibility.Visible, placeholder.Visibility);

            ControlHelper.SetPlaceholderText(comboBox, string.Empty);
            host.UpdateLayout();
            Assert.AreEqual(Visibility.Collapsed, placeholder.Visibility);
        });
    }

    [TestMethod]
    public void VerifyEditableComboBoxUsesOfficialTextBoxTemplate()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var comboBox = CreateComboBox();
            comboBox.IsEditable = true;
            comboBox.SetValue(System.Windows.Controls.Border.CornerRadiusProperty, new CornerRadius(4));

            using var host = new TestWindowHost(comboBox);
            host.UpdateLayout();

            var editableTextBox = FindTemplateChild<TextBox>(comboBox, "PART_EditableTextBox");
            Assert.AreSame(comboBox.TryFindResource("DefaultComboBoxTextBoxStyle"), editableTextBox.Style);
            AssertComboBoxTextBoxStyleSetters(editableTextBox.Style);
            AssertComboBoxEditableTemplateResourceSetters(comboBox.Template);
            Assert.AreSame(comboBox.Foreground, editableTextBox.Foreground);
            Assert.AreEqual(new Thickness(11, 5, 38, 6), editableTextBox.Padding);
            Assert.AreEqual(new CornerRadius(4), ((CornerRadius)editableTextBox.GetValue(System.Windows.Controls.Border.CornerRadiusProperty)));

            editableTextBox.ApplyTemplate();
            var editableContentBorder = FindTemplateChild<Border>(editableTextBox, "ContentBorder");
            Assert.AreEqual(new CornerRadius(4), editableContentBorder.CornerRadius);
            var editableContentHost = FindTemplateChild<ScrollViewer>(editableTextBox, "PART_ContentHost");
            Assert.AreSame(editableTextBox.Foreground, TextElement.GetForeground(editableContentHost));

            Assert.IsNotNull(FindTemplateChild<Border>(comboBox, "DropDownOverlay"));
            Assert.IsNotNull(FindTemplateChild<ToggleButton>(comboBox, "ToggleButton"));
            var chevronIcon = FindTemplateChild<TextBlock>(comboBox, "ChevronIcon");
            Assert.AreEqual(comboBox.TryFindResource("SymbolThemeFontFamily"), chevronIcon.FontFamily);
            Assert.AreEqual(comboBox.TryFindResource("ComboBoxChevronSize"), chevronIcon.FontSize);
            Assert.AreEqual(comboBox.TryFindResource("ComboBoxChevronDownGlyph"), chevronIcon.Text);
            Assert.IsNotNull(FindTemplateChild<Popup>(comboBox, "PART_Popup"));
            Assert.IsTrue(comboBox.Template.Triggers.Count > 0);

            comboBox.IsEnabled = false;
            host.UpdateLayout();
            var contentBorder = FindTemplateChild<Border>(comboBox, "ContentBorder");
            Assert.AreSame(contentBorder.TryFindResource("ComboBoxBackgroundDisabled"), contentBorder.Background);
            Assert.AreSame(contentBorder.TryFindResource("ComboBoxBorderBrushDisabled"), contentBorder.BorderBrush);
            Assert.AreSame(editableTextBox.TryFindResource("ComboBoxForegroundDisabled"), editableTextBox.Foreground);
            Assert.AreSame(chevronIcon.TryFindResource("ComboBoxDropDownGlyphForegroundDisabled"), chevronIcon.Foreground);

            Assert.IsNull(VisualTreeTestHelper.FindDescendant<ContentPresenterEx>(comboBox));
            Assert.IsNull(VisualTreeTestHelper.FindDescendant<FontIconFallback>(comboBox));
        });
    }

    [TestMethod]
    public void OfficialComboBoxStyleDeletesWinUILayer()
    {
        var repoRoot = FindRepoRoot();
        var text = File.ReadAllText(Path.Combine(repoRoot, "ModernWpf", "Styles", "ComboBox.xaml"));

        Assert.IsFalse(text.Contains("ComboBoxHelper", StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("VisualStateEx", StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("ContentPresenterEx", StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("FontIconFallback", StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("ControlHelper.CornerRadius", StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("System.Runtime", StringComparison.Ordinal));
        StringAssert.Contains(text, "DefaultComboBoxTextBoxStyle");
        StringAssert.Contains(text, "DefaultComboBoxToggleButtonStyle");
    }

    [TestMethod]
    public void ThemeResourcesUseOfficialComboBoxHighContrastTokens()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            foreach (var themeName in new[] { "Light", "Dark" })
            {
                AssertThemeResourceReferences(
                    themeName,
                    ("ComboBoxItemForeground", "TextFillColorPrimaryBrush"),
                    ("ComboBoxItemForegroundPressed", "TextFillColorSecondaryBrush"),
                    ("ComboBoxItemForegroundPointerOver", "TextFillColorPrimaryBrush"),
                    ("ComboBoxItemForegroundDisabled", "TextFillColorDisabledBrush"),
                    ("ComboBoxItemForegroundSelected", "TextFillColorPrimaryBrush"),
                    ("ComboBoxItemForegroundSelectedUnfocused", "TextFillColorPrimaryBrush"),
                    ("ComboBoxItemForegroundSelectedPressed", "TextFillColorSecondaryBrush"),
                    ("ComboBoxItemForegroundSelectedPointerOver", "TextFillColorPrimaryBrush"),
                    ("ComboBoxItemForegroundSelectedDisabled", "TextFillColorDisabledBrush"),
                    ("ComboBoxItemBackground", "SubtleFillColorTransparentBrush"),
                    ("ComboBoxItemBackgroundPressed", "SubtleFillColorTertiaryBrush"),
                    ("ComboBoxItemBackgroundPointerOver", "SubtleFillColorSecondaryBrush"),
                    ("ComboBoxItemBackgroundDisabled", "SubtleFillColorDisabledBrush"),
                    ("ComboBoxItemBackgroundSelected", "SubtleFillColorSecondaryBrush"),
                    ("ComboBoxItemBackgroundSelectedUnfocused", "SubtleFillColorTertiaryBrush"),
                    ("ComboBoxItemBackgroundSelectedPressed", "SubtleFillColorSecondaryBrush"),
                    ("ComboBoxItemBackgroundSelectedPointerOver", "SubtleFillColorTertiaryBrush"),
                    ("ComboBoxItemBackgroundSelectedDisabled", "SubtleFillColorSecondaryBrush"),
                    ("ComboBoxItemBorderBrush", "SubtleFillColorTransparentBrush"),
                    ("ComboBoxItemBorderBrushPressed", "SubtleFillColorTertiaryBrush"),
                    ("ComboBoxItemBorderBrushPointerOver", "SubtleFillColorSecondaryBrush"),
                    ("ComboBoxItemBorderBrushDisabled", "SubtleFillColorDisabledBrush"),
                    ("ComboBoxItemBorderBrushSelected", "SubtleFillColorSecondaryBrush"),
                    ("ComboBoxItemBorderBrushSelectedUnfocused", "SubtleFillColorTransparentBrush"),
                    ("ComboBoxItemBorderBrushSelectedPressed", "SubtleFillColorSecondaryBrush"),
                    ("ComboBoxItemBorderBrushSelectedPointerOver", "SubtleFillColorTertiaryBrush"),
                    ("ComboBoxItemBorderBrushSelectedDisabled", "SubtleFillColorSecondaryBrush"),
                    ("ComboBoxBackground", "ControlFillColorDefaultBrush"),
                    ("ComboBoxBackgroundPointerOver", "ControlFillColorSecondaryBrush"),
                    ("ComboBoxBackgroundPressed", "ControlFillColorTertiaryBrush"),
                    ("ComboBoxBackgroundDisabled", "ControlFillColorDisabledBrush"),
                    ("ComboBoxBackgroundUnfocused", "ControlFillColorDefaultBrush"),
                    ("ComboBoxBackgroundFocused", "ControlFillColorDefaultBrush"),
                    ("ComboBoxBackgroundBorderBrushFocused", "FocusStrokeColorOuterBrush"),
                    ("ComboBoxBackgroundBorderBrushUnfocused", "ControlStrokeColorDefaultBrush"),
                    ("ComboBoxForeground", "TextFillColorPrimaryBrush"),
                    ("ComboBoxForegroundPointerOver", "TextFillColorPrimaryBrush"),
                    ("ComboBoxForegroundPressed", "TextFillColorSecondaryBrush"),
                    ("ComboBoxForegroundDisabled", "TextFillColorDisabledBrush"),
                    ("ComboBoxForegroundFocused", "TextFillColorPrimaryBrush"),
                    ("ComboBoxForegroundFocusedPressed", "TextFillColorPrimaryBrush"),
                    ("ComboBoxHeaderForeground", "TextFillColorPrimaryBrush"),
                    ("ComboBoxHeaderForegroundDisabled", "TextFillColorDisabledBrush"),
                    ("ComboBoxPlaceHolderForeground", "TextFillColorSecondaryBrush"),
                    ("ComboBoxPlaceHolderForegroundPointerOver", "TextFillColorSecondaryBrush"),
                    ("ComboBoxPlaceHolderForegroundPressed", "TextFillColorTertiaryBrush"),
                    ("ComboBoxPlaceHolderForegroundDisabled", "TextFillColorDisabledBrush"),
                    ("ComboBoxPlaceHolderForegroundFocused", "TextFillColorSecondaryBrush"),
                    ("ComboBoxPlaceHolderForegroundFocusedPressed", "TextFillColorSecondaryBrush"),
                    ("ComboBoxBorderBrush", "ControlElevationBorderBrush"),
                    ("ComboBoxBorderBrushPointerOver", "ControlElevationBorderBrush"),
                    ("ComboBoxBorderBrushPressed", "ControlStrokeColorDefaultBrush"),
                    ("ComboBoxBorderBrushDisabled", "ControlStrokeColorDefaultBrush"),
                    ("ComboBoxDropDownGlyphForeground", "TextFillColorSecondaryBrush"),
                    ("ComboBoxDropDownGlyphForegroundDisabled", "TextFillColorDisabledBrush"),
                    ("ComboBoxDropDownGlyphForegroundFocused", "TextFillColorSecondaryBrush"),
                    ("ComboBoxDropDownGlyphForegroundFocusedPressed", "SystemControlHighlightAltBaseMediumHighBrush"),
                    ("ComboBoxDropDownForeground", "TextFillColorPrimaryBrush"),
                    ("ComboBoxDropDownBackground", "AcrylicInAppFillColorDefaultBrush"),
                    ("ComboBoxDropDownBorderBrush", "SurfaceStrokeColorFlyoutBrush"),
                    ("ComboBoxLightDismissOverlayBackground", "SystemControlPageBackgroundMediumAltMediumBrush"),
                    ("ComboBoxDropDownBackgroundPointerOver", "SubtleFillColorSecondaryBrush"),
                    ("ComboBoxDropDownBackgroundPointerPressed", "SubtleFillColorTertiaryBrush"),
                    ("ComboBoxFocusedDropDownBackgroundPointerOver", "ControlFillColorTertiaryBrush"),
                    ("ComboBoxFocusedDropDownBackgroundPointerPressed", "ControlAltFillColorQuarternaryBrush"),
                    ("ComboBoxEditableDropDownGlyphForeground", "TextFillColorSecondaryBrush"),
                    ("ComboBoxItemPillFillBrush", "AccentFillColorDefaultBrush"));
                AssertThemeResourceValue(themeName, "ComboBoxDropdownBorderPadding", new Thickness(0));
            }

            AssertThemeResourceReferences(
                "HighContrast",
                ("ComboBoxItemForeground", "SystemControlForegroundBaseHighBrush"),
                ("ComboBoxItemForegroundPressed", "SystemControlHighlightAltBaseHighBrush"),
                ("ComboBoxItemForegroundPointerOver", "SystemControlHighlightAltBaseHighBrush"),
                ("ComboBoxItemForegroundDisabled", "SystemControlDisabledBaseMediumLowBrush"),
                ("ComboBoxItemForegroundSelected", "SystemControlHighlightAltBaseHighBrush"),
                ("ComboBoxItemForegroundSelectedUnfocused", "SystemControlHighlightAltBaseHighBrush"),
                ("ComboBoxItemForegroundSelectedPressed", "SystemControlHighlightAltBaseHighBrush"),
                ("ComboBoxItemForegroundSelectedPointerOver", "SystemControlHighlightAltBaseHighBrush"),
                ("ComboBoxItemForegroundSelectedDisabled", "SystemControlDisabledBaseMediumLowBrush"),
                ("ComboBoxItemBackground", "SystemControlTransparentBrush"),
                ("ComboBoxItemBackgroundPressed", "SystemControlHighlightListMediumBrush"),
                ("ComboBoxItemBackgroundPointerOver", "SystemControlHighlightListLowBrush"),
                ("ComboBoxItemBackgroundDisabled", "SystemControlTransparentBrush"),
                ("ComboBoxItemBackgroundSelected", "SystemControlHighlightListAccentLowBrush"),
                ("ComboBoxItemBackgroundSelectedUnfocused", "SystemControlHighlightListAccentLowBrush"),
                ("ComboBoxItemBackgroundSelectedPressed", "SystemControlHighlightListAccentHighBrush"),
                ("ComboBoxItemBackgroundSelectedPointerOver", "SystemControlHighlightListAccentMediumBrush"),
                ("ComboBoxItemBackgroundSelectedDisabled", "SystemControlTransparentBrush"),
                ("ComboBoxItemBorderBrush", "SystemControlTransparentBrush"),
                ("ComboBoxItemBorderBrushPressed", "SystemControlTransparentBrush"),
                ("ComboBoxItemBorderBrushPointerOver", "SystemControlTransparentBrush"),
                ("ComboBoxItemBorderBrushDisabled", "SystemControlTransparentBrush"),
                ("ComboBoxItemBorderBrushSelected", "SystemControlTransparentBrush"),
                ("ComboBoxItemBorderBrushSelectedUnfocused", "SystemControlTransparentBrush"),
                ("ComboBoxItemBorderBrushSelectedPressed", "SystemControlTransparentBrush"),
                ("ComboBoxItemBorderBrushSelectedPointerOver", "SystemControlTransparentBrush"),
                ("ComboBoxItemBorderBrushSelectedDisabled", "SystemControlTransparentBrush"),
                ("ComboBoxBackground", "SystemControlBackgroundAltMediumLowBrush"),
                ("ComboBoxBackgroundPointerOver", "SystemControlPageBackgroundAltMediumBrush"),
                ("ComboBoxBackgroundPressed", "SystemControlBackgroundListMediumBrush"),
                ("ComboBoxBackgroundDisabled", "SystemControlBackgroundBaseLowBrush"),
                ("ComboBoxBackgroundUnfocused", "SystemControlHighlightListAccentLowBrush"),
                ("ComboBoxBackgroundFocused", "ComboBoxBackgroundUnfocused"),
                ("ComboBoxBackgroundBorderBrushFocused", "SystemControlHighlightTransparentBrush"),
                ("ComboBoxBackgroundBorderBrushUnfocused", "SystemControlHighlightBaseMediumLowBrush"),
                ("ComboBoxForeground", "SystemControlForegroundBaseHighBrush"),
                ("ComboBoxForegroundPointerOver", "SystemControlForegroundBaseHighBrush"),
                ("ComboBoxForegroundPressed", "SystemControlForegroundBaseHighBrush"),
                ("ComboBoxForegroundDisabled", "SystemControlDisabledBaseMediumLowBrush"),
                ("ComboBoxForegroundFocused", "SystemControlForegroundBaseHighBrush"),
                ("ComboBoxForegroundFocusedPressed", "SystemControlForegroundBaseHighBrush"),
                ("ComboBoxHeaderForeground", "SystemControlForegroundBaseHighBrush"),
                ("ComboBoxHeaderForegroundDisabled", "SystemControlDisabledBaseMediumLowBrush"),
                ("ComboBoxPlaceHolderForeground", "SystemControlPageTextBaseMediumBrush"),
                ("ComboBoxPlaceHolderForegroundPointerOver", "SystemControlPageTextBaseMediumBrush"),
                ("ComboBoxPlaceHolderForegroundPressed", "SystemControlPageTextBaseMediumBrush"),
                ("ComboBoxPlaceHolderForegroundDisabled", "SystemControlDisabledChromeDisabledLowBrush"),
                ("ComboBoxPlaceHolderForegroundFocused", "SystemControlPageTextBaseMediumBrush"),
                ("ComboBoxPlaceHolderForegroundFocusedPressed", "SystemControlHighlightAltBaseHighBrush"),
                ("ComboBoxBorderBrush", "SystemControlForegroundBaseMediumBrush"),
                ("ComboBoxBorderBrushPointerOver", "SystemControlHighlightBaseMediumHighBrush"),
                ("ComboBoxBorderBrushPressed", "SystemControlHighlightBaseMediumBrush"),
                ("ComboBoxBorderBrushDisabled", "SystemControlDisabledBaseLowBrush"),
                ("ComboBoxDropDownGlyphForeground", "SystemControlForegroundBaseMediumHighBrush"),
                ("ComboBoxDropDownGlyphForegroundDisabled", "SystemControlDisabledBaseMediumLowBrush"),
                ("ComboBoxDropDownGlyphForegroundFocused", "SystemControlForegroundBaseHighBrush"),
                ("ComboBoxDropDownGlyphForegroundFocusedPressed", "SystemControlForegroundBaseHighBrush"),
                ("ComboBoxDropDownForeground", "SystemControlForegroundBaseHighBrush"),
                ("ComboBoxDropDownBackground", "SystemControlBackgroundChromeMediumLowBrush"),
                ("ComboBoxDropDownBorderBrush", "SystemControlForegroundChromeHighBrush"),
                ("ComboBoxLightDismissOverlayBackground", "SystemControlPageBackgroundMediumAltMediumBrush"),
                ("ComboBoxDropDownBackgroundPointerOver", "SystemControlPageBackgroundAltMediumBrush"),
                ("ComboBoxDropDownBackgroundPointerPressed", "SystemControlBackgroundListMediumBrush"),
                ("ComboBoxEditableDropDownGlyphForeground", "SystemControlForegroundBaseMediumHighBrush"),
                ("ComboBoxItemPillFillBrush", "SystemControlHighlightListAccentLowBrush"));
            AssertThemeResourceValue("HighContrast", "ComboBoxDropdownBorderPadding", new Thickness(0));
        });
    }

    [TestMethod]
    public void DataGridComboBoxAdapterStylesRemainResolvable()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var comboBox = new WpfComboBox();
            var defaultStyle = AssertStyle(comboBox, "DefaultComboBoxStyle");
            var editingStyle = AssertStyle(comboBox, "DataGridComboBoxStyle");
            var elementStyle = AssertStyle(comboBox, "DataGridTextBlockComboBoxStyle");

            Assert.AreSame(defaultStyle, editingStyle.BasedOn);
            Assert.AreSame(defaultStyle, elementStyle.BasedOn);

            AssertSetterValue(editingStyle, Control.PaddingProperty, new Thickness(12, 0, 0, 0));
            AssertSetterValue(editingStyle, Control.VerticalContentAlignmentProperty, VerticalAlignment.Center);
            AssertSetterValue(editingStyle, FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Stretch);
            AssertSetterValue(editingStyle, FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Stretch);
            AssertSetterValue(editingStyle, System.Windows.Controls.Border.CornerRadiusProperty, new CornerRadius());
            AssertDynamicResourceSetter(editingStyle, Validation.ErrorTemplateProperty, "DataGridTextControlValidationErrorTemplate");

            AssertSetterValue(elementStyle, Control.PaddingProperty, new Thickness(12, 0, 0, 0));
            AssertSetterValue(elementStyle, FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Stretch);
            Assert.IsInstanceOfType(FindSetter(elementStyle, Control.TemplateProperty)?.Value, typeof(ControlTemplate));
        });
    }

    private static WpfComboBox CreateComboBox()
    {
        var comboBox = new WpfComboBox();
        comboBox.Items.Add("Item 1");
        comboBox.Items.Add("Item 2");
        comboBox.Items.Add("Item 3");
        return comboBox;
    }

    private static T FindTemplateChild<T>(Control control, string name)
        where T : FrameworkElement
    {
        control.ApplyTemplate();
        return control.Template?.FindName(name, control) as T
            ?? throw new AssertFailedException($"Expected template child '{name}' on {control.GetType().Name}.");
    }

    private static Style AssertStyle(FrameworkElement element, string resourceKey)
    {
        return element.TryFindResource(resourceKey) as Style
            ?? throw new AssertFailedException($"Expected style resource '{resourceKey}'.");
    }

    private static void AssertComboBoxStyleSetters(Style style)
    {
        AssertDynamicResourceSetter(style, Control.FocusVisualStyleProperty, "DefaultControlFocusVisualStyle");
        AssertDynamicResourceSetter(style, Control.ContextMenuProperty, "DefaultControlContextMenu");
        AssertDynamicResourceSetter(style, Control.ForegroundProperty, "ComboBoxForeground");
        AssertDynamicResourceSetter(style, Control.BackgroundProperty, "ComboBoxBackground");
        AssertDynamicResourceSetter(style, Control.BorderBrushProperty, "ComboBoxBorderBrush");
        AssertResourceSetterValue(style, Control.BorderThicknessProperty, "ComboBoxBorderThemeThickness");
        AssertSetterValue(style, ScrollViewer.CanContentScrollProperty, false);
        AssertSetterValue(style, ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Auto);
        AssertSetterValue(style, ScrollViewer.VerticalScrollBarVisibilityProperty, ScrollBarVisibility.Auto);
        AssertSetterValue(style, ScrollViewer.IsDeferredScrollingEnabledProperty, false);
        AssertSetterValue(style, FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Stretch);
        AssertSetterValue(style, FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
        AssertSetterValue(style, Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch);
        AssertSetterValue(style, Control.VerticalContentAlignmentProperty, VerticalAlignment.Center);
        AssertDynamicResourceSetter(style, FrameworkElement.MinHeightProperty, "TextControlThemeMinHeight");
        AssertDynamicResourceSetter(style, FrameworkElement.MinWidthProperty, "TextControlThemeMinWidth");
        AssertDynamicResourceSetter(style, Control.PaddingProperty, "ComboBoxPadding");
        AssertDynamicResourceSetter(style, System.Windows.Controls.Border.CornerRadiusProperty, "ControlCornerRadius");
        AssertSetterValue(style, Popup.PopupAnimationProperty, PopupAnimation.None);
        AssertSetterValue(style, Popup.PlacementProperty, PlacementMode.Bottom);
        AssertSetterValue(style, UIElement.SnapsToDevicePixelsProperty, true);
        AssertSetterValue(style, FrameworkElement.OverridesDefaultStyleProperty, true);
        AssertResourceSetterValue(style, Control.TemplateProperty, "DefaultComboBoxTemplate");

        var trigger = style.Triggers.OfType<Trigger>().Single(item =>
            item.Property == WpfComboBox.IsEditableProperty &&
            Equals(item.Value, true));
        AssertSetters(trigger.Setters.OfType<Setter>().ToArray(),
            ("", "Template", "EditableComboBoxTemplate"));
    }

    private static void AssertComboBoxTextBoxStyleSetters(Style style)
    {
        AssertSetterValue(style, Control.FocusVisualStyleProperty, null);
        AssertDynamicResourceSetter(style, Control.ContextMenuProperty, "DefaultControlContextMenu");
        AssertDynamicResourceSetter(style, Control.ForegroundProperty, "TextControlForeground");
        AssertDynamicResourceSetter(style, TextBoxBase.CaretBrushProperty, "TextControlForeground");
        AssertDynamicResourceSetter(style, Control.BorderBrushProperty, "TextControlElevationBorderBrush");
        AssertDynamicResourceSetter(style, Control.BorderThicknessProperty, "TextControlBorderThemeThickness");
        AssertSetterValue(style, ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Hidden);
        AssertSetterValue(style, ScrollViewer.VerticalScrollBarVisibilityProperty, ScrollBarVisibility.Hidden);
        AssertSetterValue(style, Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Left);
        AssertSetterValue(style, Control.VerticalContentAlignmentProperty, VerticalAlignment.Top);
        AssertSetterValue(style, FrameworkElement.CursorProperty, Cursors.IBeam);
        AssertDynamicResourceSetter(style, FrameworkElement.MinHeightProperty, "TextControlThemeMinHeight");
        AssertDynamicResourceSetter(style, FrameworkElement.MinWidthProperty, "TextControlThemeMinWidth");
        AssertDynamicResourceSetter(style, Control.PaddingProperty, "TextControlThemePadding");
        AssertDynamicResourceSetter(style, System.Windows.Controls.Border.CornerRadiusProperty, "ControlCornerRadius");
        AssertSetterValue(style, UIElement.AllowDropProperty, true);
        AssertSetterValue(style, ScrollViewer.PanningModeProperty, PanningMode.VerticalFirst);
        AssertSetterValue(style, Stylus.IsFlicksEnabledProperty, false);
        AssertSetterValue(style, FrameworkElement.OverridesDefaultStyleProperty, true);
        AssertDynamicResourceSetter(style, TextBoxBase.SelectionBrushProperty, "TextControlSelectionHighlightColor");
        Assert.IsInstanceOfType(FindSetter(style, Control.TemplateProperty)?.Value, typeof(ControlTemplate));
    }

    private static void AssertComboBoxItemStyleSetters(Style style)
    {
        AssertDynamicResourceSetter(style, Control.FocusVisualStyleProperty, "DefaultControlFocusVisualStyle");
        AssertDynamicResourceSetter(style, Control.ForegroundProperty, "ComboBoxForeground");
        AssertSetterValue(style, Control.BackgroundProperty, Brushes.Transparent);
        AssertResourceSetterValue(style, FrameworkElement.MarginProperty, "ComboBoxItemMargin");
        AssertResourceSetterValue(style, Control.PaddingProperty, "ComboBoxItemContentMargin");
        AssertDynamicResourceSetter(style, System.Windows.Controls.Border.CornerRadiusProperty, "ControlCornerRadius");
        AssertSetterValue(style, UIElement.SnapsToDevicePixelsProperty, true);
        AssertSetterValue(style, FrameworkElement.OverridesDefaultStyleProperty, true);
        Assert.IsInstanceOfType(FindSetter(style, Control.TemplateProperty)?.Value, typeof(ControlTemplate));
    }

    private static void AssertComboBoxTemplateResourceSetters(ControlTemplate template)
    {
        var triggers = template.Triggers.OfType<Trigger>().ToArray();
        AssertTrigger(triggers, "HasItems", false,
            ("DropDownBorder", "MinHeight", "ComboBoxPopupMinHeight"));
        AssertTrigger(triggers, "PART_Popup", "AllowsTransparency", false,
            ("DropDownBorder", "CornerRadius", new CornerRadius()));
        AssertTrigger(triggers, "IsGrouping", true,
            ("", "CanContentScroll", false));
        AssertTrigger(triggers, "IsMouseOver", true,
            ("ContentBorder", "Background", "ComboBoxBackgroundPointerOver"),
            ("ContentBorder", "BorderBrush", "ComboBoxBorderBrushPointerOver"),
            ("PART_ContentPresenter", "Foreground", "ComboBoxForegroundPointerOver"),
            ("PlaceholderTextContentPresenter", "Foreground", "ComboBoxPlaceHolderForegroundPointerOver"));
        AssertTrigger(triggers, "ToggleButton", "IsPressed", true,
            ("ContentBorder", "Background", "ComboBoxBackgroundPressed"),
            ("ContentBorder", "BorderBrush", "ComboBoxBorderBrushPressed"),
            ("PART_ContentPresenter", "Foreground", "ComboBoxForegroundPressed"),
            ("PlaceholderTextContentPresenter", "Foreground", "ComboBoxPlaceHolderForegroundPressed"));
        AssertTrigger(triggers, "IsFocused", true,
            ("PART_ContentPresenter", "Foreground", "ComboBoxForegroundFocused"),
            ("PlaceholderTextContentPresenter", "Foreground", "ComboBoxPlaceHolderForegroundFocused"),
            ("ChevronIcon", "Foreground", "ComboBoxDropDownGlyphForegroundFocused"));
        AssertTrigger(triggers, "IsEnabled", false,
            ("ContentBorder", "Background", "ComboBoxBackgroundDisabled"),
            ("ContentBorder", "BorderBrush", "ComboBoxBorderBrushDisabled"),
            ("PART_ContentPresenter", "Foreground", "ComboBoxForegroundDisabled"),
            ("PlaceholderTextContentPresenter", "Foreground", "ComboBoxPlaceHolderForegroundDisabled"),
            ("ChevronIcon", "Foreground", "ComboBoxDropDownGlyphForegroundDisabled"));

        var multiTriggers = template.Triggers.OfType<MultiTrigger>().ToArray();
        AssertTrigger(multiTriggers,
            new[] { ("", "SelectedIndex", (object)(-1)), ("", "PlaceholderTextVisibility", (object)Visibility.Visible) },
            ("PlaceholderTextContentPresenter", "Visibility", Visibility.Visible));
        AssertTrigger(multiTriggers,
            new[] { ("", "IsFocused", (object)true), ("ToggleButton", "IsPressed", (object)true) },
            ("PART_ContentPresenter", "Foreground", "ComboBoxForegroundFocusedPressed"),
            ("PlaceholderTextContentPresenter", "Foreground", "ComboBoxPlaceHolderForegroundFocusedPressed"),
            ("ChevronIcon", "Foreground", "ComboBoxDropDownGlyphForegroundFocusedPressed"));
    }

    private static void AssertComboBoxEditableTemplateResourceSetters(ControlTemplate template)
    {
        var triggers = template.Triggers.OfType<Trigger>().ToArray();
        AssertTrigger(triggers, "HasItems", false,
            ("DropDownBorder", "MinHeight", "ComboBoxPopupMinHeight"));
        AssertTrigger(triggers, "PART_Popup", "AllowsTransparency", false,
            ("DropDownBorder", "CornerRadius", new CornerRadius()));
        AssertTrigger(triggers, "IsGrouping", true,
            ("", "CanContentScroll", false));
        AssertTrigger(triggers, "IsMouseOver", true,
            ("ContentBorder", "Background", "ComboBoxBackgroundPointerOver"),
            ("ContentBorder", "BorderBrush", "ComboBoxBorderBrushPointerOver"),
            ("PART_EditableTextBox", "Foreground", "ComboBoxForegroundPointerOver"));
        AssertTrigger(triggers, "IsFocused", true,
            ("PART_EditableTextBox", "Foreground", "ComboBoxForegroundFocused"),
            ("ChevronIcon", "Foreground", "ComboBoxEditableDropDownGlyphForeground"));
        AssertTrigger(triggers, "ToggleButton", "IsMouseOver", true,
            ("DropDownOverlay", "Background", "ComboBoxDropDownBackgroundPointerOver"));
        AssertTrigger(triggers, "ToggleButton", "IsPressed", true,
            ("DropDownOverlay", "Background", "ComboBoxDropDownBackgroundPointerPressed"));
        AssertTrigger(triggers, "IsEnabled", false,
            ("ContentBorder", "Background", "ComboBoxBackgroundDisabled"),
            ("ContentBorder", "BorderBrush", "ComboBoxBorderBrushDisabled"),
            ("PART_EditableTextBox", "Foreground", "ComboBoxForegroundDisabled"),
            ("ChevronIcon", "Foreground", "ComboBoxDropDownGlyphForegroundDisabled"));

        var multiTriggers = template.Triggers.OfType<MultiTrigger>().ToArray();
        AssertTrigger(multiTriggers,
            new[] { ("", "IsFocused", (object)true), ("ToggleButton", "IsMouseOver", (object)true) },
            ("DropDownOverlay", "Background", "ComboBoxDropDownBackgroundPointerOver"),
            ("ChevronIcon", "Foreground", "ComboBoxEditableDropDownGlyphForeground"));
        AssertTrigger(multiTriggers,
            new[] { ("", "IsFocused", (object)true), ("ToggleButton", "IsPressed", (object)true) },
            ("ChevronIcon", "Foreground", "ComboBoxEditableDropDownGlyphForeground"),
            ("PART_EditableTextBox", "Foreground", "ComboBoxForegroundFocusedPressed"),
            ("DropDownOverlay", "Background", "ComboBoxFocusedDropDownBackgroundPointerPressed"));
    }

    private static void AssertTrigger(
        Trigger[] triggers,
        string propertyName,
        object value,
        params (string TargetName, string PropertyName, object Value)[] expectedSetters)
    {
        AssertTrigger(triggers, string.Empty, propertyName, value, expectedSetters);
    }

    private static void AssertTrigger(
        Trigger[] triggers,
        string sourceName,
        string propertyName,
        object value,
        params (string TargetName, string PropertyName, object Value)[] expectedSetters)
    {
        var trigger = triggers.Single(item =>
            (item.SourceName ?? string.Empty) == sourceName &&
            item.Property.Name == propertyName &&
            Equals(item.Value, value));
        AssertSetters(trigger.Setters.OfType<Setter>().ToArray(), expectedSetters);
    }

    private static void AssertTrigger(
        MultiTrigger[] triggers,
        (string SourceName, string PropertyName, object Value)[] expectedConditions,
        params (string TargetName, string PropertyName, object Value)[] expectedSetters)
    {
        var trigger = triggers.Single(item => expectedConditions.All(condition => HasCondition(item, condition.SourceName, condition.PropertyName, condition.Value)));
        AssertSetters(trigger.Setters.OfType<Setter>().ToArray(), expectedSetters);
    }

    private static bool HasCondition(MultiTrigger trigger, string sourceName, string propertyName, object value)
    {
        return trigger.Conditions.Cast<Condition>().Any(item =>
            (item.SourceName ?? string.Empty) == sourceName &&
            item.Property.Name == propertyName &&
            Equals(item.Value, value));
    }

    private static void AssertSetters(
        Setter[] setters,
        params (string TargetName, string PropertyName, object Value)[] expectedSetters)
    {
        Assert.AreEqual(expectedSetters.Length, setters.Length);

        foreach (var expectedSetter in expectedSetters)
        {
            AssertSetter(setters, expectedSetter.TargetName, expectedSetter.PropertyName, expectedSetter.Value);
        }
    }

    private static void AssertDynamicResourceSetter(Style style, DependencyProperty property, object resourceKey)
    {
        var setter = FindSetter(style, property);
        Assert.IsNotNull(setter, $"Expected setter for {property.Name}.");
        Assert.IsInstanceOfType(setter!.Value, typeof(DynamicResourceExtension));

        var dynamicResource = (DynamicResourceExtension)setter.Value;
        Assert.AreEqual(resourceKey, dynamicResource.ResourceKey);
    }

    private static void AssertResourceSetterValue(Style style, DependencyProperty property, object resourceKey)
    {
        var setter = FindSetter(style, property);
        Assert.IsNotNull(setter, $"Expected setter for {property.Name}.");

        if (setter!.Value is StaticResourceExtension staticResource)
        {
            Assert.AreEqual(resourceKey, staticResource.ResourceKey);
            return;
        }

        Assert.AreEqual(Application.Current.FindResource(resourceKey), setter.Value);
    }

    private static void AssertSetterValue(Style style, DependencyProperty property, object? value)
    {
        var setter = FindSetter(style, property);
        Assert.IsNotNull(setter, $"Expected setter for {property.Name}.");
        Assert.AreEqual(value, setter!.Value);
    }

    private static Setter? FindSetter(Style style, DependencyProperty property)
    {
        for (var current = style; current != null; current = current.BasedOn)
        {
            var setter = current.Setters
                .OfType<Setter>()
                .SingleOrDefault(item => item.Property == property);

            if (setter != null)
            {
                return setter;
            }
        }

        return null;
    }

    private static void AssertSetter(Setter[] setters, string targetName, string propertyName, object value)
    {
        var setter = setters.Single(item =>
            (item.TargetName ?? string.Empty) == targetName &&
            item.Property.Name == propertyName);

        if (value is string resourceKey)
        {
            if (setter.Value is DynamicResourceExtension dynamicResource)
            {
                Assert.AreEqual(resourceKey, dynamicResource.ResourceKey);
                return;
            }

            if (setter.Value is StaticResourceExtension staticResource)
            {
                Assert.AreEqual(resourceKey, staticResource.ResourceKey);
                return;
            }

            Assert.AreEqual(Application.Current.FindResource(resourceKey), setter.Value);
            return;
        }

        Assert.AreEqual(value, setter.Value);
    }

    private static void AssertThemeResourceReference(string themeName, string resourceKey, string expectedResourceKey)
    {
        var themeDictionary = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(themeDictionary.Contains(resourceKey), $"{themeName} is missing {resourceKey}.");
        Assert.IsTrue(themeDictionary.Contains(expectedResourceKey), $"{themeName} is missing {expectedResourceKey}.");
        Assert.AreSame(themeDictionary[expectedResourceKey], themeDictionary[resourceKey], $"{themeName}:{resourceKey}");
    }

    private static void AssertThemeResourceReferences(
        string themeName,
        params (string ResourceKey, string ExpectedResourceKey)[] expectedResources)
    {
        foreach (var expectedResource in expectedResources)
        {
            AssertThemeResourceReference(themeName, expectedResource.ResourceKey, expectedResource.ExpectedResourceKey);
        }
    }

    private static void AssertThemeResourceValue(string themeName, string resourceKey, object expectedValue)
    {
        var themeDictionary = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(themeDictionary.Contains(resourceKey), $"{themeName} is missing {resourceKey}.");
        Assert.AreEqual(expectedValue, themeDictionary[resourceKey], $"{themeName}:{resourceKey}");
    }

    private static string FindRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory != null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "ModernWpf.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        Assert.Fail("Could not locate ModernWpf.sln from the test output directory.");
        return string.Empty;
    }
}
