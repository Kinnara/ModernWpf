using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls.Primitives;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.CommonStyles;

[TestClass]
public class CheckBoxVisualStateTests
{
    [TestMethod]
    public void DefaultCheckBoxStyleUsesOfficialWpfFluentTriggerShape()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var defaultStyle = (Style)Application.Current.FindResource("DefaultCheckBoxStyle");
            var implicitCheckBoxStyle = (Style)Application.Current.FindResource(typeof(CheckBox));
            Assert.AreEqual(typeof(CheckBox), defaultStyle.TargetType);
            Assert.AreEqual(typeof(CheckBox), implicitCheckBoxStyle.TargetType);
            Assert.AreSame(defaultStyle, implicitCheckBoxStyle.BasedOn);

            AssertDynamicResourceSetter(defaultStyle, Control.FocusVisualStyleProperty, SystemParameters.FocusVisualStyleKey);
            AssertDynamicResourceSetter(defaultStyle, Control.BackgroundProperty, "CheckBoxBackgroundUnchecked");
            AssertDynamicResourceSetter(defaultStyle, Control.ForegroundProperty, "CheckBoxForegroundUnchecked");
            AssertDynamicResourceSetter(defaultStyle, Control.BorderBrushProperty, "CheckBoxBorderBrushUnchecked");
            AssertResourceSetterValue(defaultStyle, Control.BorderThicknessProperty, "CheckBoxBorderThickness");
            AssertResourceSetterValue(defaultStyle, Control.PaddingProperty, "CheckBoxPadding");
            AssertSetterValue(defaultStyle, FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Left);
            AssertSetterValue(defaultStyle, FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
            AssertSetterValue(defaultStyle, Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Left);
            AssertSetterValue(defaultStyle, Control.VerticalContentAlignmentProperty, VerticalAlignment.Top);
            AssertSetterValue(defaultStyle, Control.FontWeightProperty, FontWeights.Normal);
            AssertSetterValue(defaultStyle, KeyboardNavigation.IsTabStopProperty, true);
            AssertSetterValue(defaultStyle, UIElement.FocusableProperty, true);
            AssertResourceSetterValue(defaultStyle, FrameworkElement.MinWidthProperty, "CheckBoxMinWidth");
            AssertResourceSetterValue(defaultStyle, FrameworkElement.MinHeightProperty, "CheckBoxHeight");
            AssertDynamicResourceSetter(defaultStyle, FocusVisualHelper.UseSystemFocusVisualsProperty, "UseSystemFocusVisuals");
            AssertResourceSetterValue(defaultStyle, FocusVisualHelper.FocusVisualMarginProperty, "CheckBoxFocusVisualMargin");
            AssertDynamicResourceSetter(defaultStyle, System.Windows.Controls.Border.CornerRadiusProperty, "ControlCornerRadius");
            AssertSetterValue(defaultStyle, UIElement.SnapsToDevicePixelsProperty, true);
            AssertSetterValue(defaultStyle, FrameworkElement.OverridesDefaultStyleProperty, true);
            AssertSetterValue(defaultStyle, Stylus.IsPressAndHoldEnabledProperty, false);
            Assert.IsInstanceOfType(FindSetter(defaultStyle, Control.TemplateProperty)?.Value, typeof(ControlTemplate));

            var dataGridStyle = (Style)Application.Current.FindResource("DataGridCheckBoxStyle");
            Assert.AreSame(defaultStyle, dataGridStyle.BasedOn);
            AssertSetterValue(dataGridStyle, FrameworkElement.MinWidthProperty, 0.0);
            AssertSetterValue(dataGridStyle, FrameworkElement.MinHeightProperty, 0.0);
            AssertSetterValue(dataGridStyle, FrameworkElement.MarginProperty, new Thickness(12, 0, 12, 0));
            AssertSetterValue(dataGridStyle, Control.FocusVisualStyleProperty, null);

            var readOnlyDataGridStyle = (Style)Application.Current.FindResource("DataGridReadOnlyCheckBoxStyle");
            Assert.AreSame(dataGridStyle, readOnlyDataGridStyle.BasedOn);
            AssertSetterValue(readOnlyDataGridStyle, UIElement.IsHitTestVisibleProperty, false);
            AssertSetterValue(readOnlyDataGridStyle, UIElement.FocusableProperty, false);

            var checkBox = CreateCheckBox();
            using var host = new TestWindowHost(checkBox, width: 180, height: 80);

            Assert.AreSame(checkBox.TryFindResource("CheckBoxBackgroundUnchecked"), checkBox.Background);
            Assert.AreSame(checkBox.TryFindResource("CheckBoxForegroundUnchecked"), checkBox.Foreground);
            Assert.AreSame(checkBox.TryFindResource("CheckBoxBorderBrushUnchecked"), checkBox.BorderBrush);
            Assert.AreEqual((Thickness)Application.Current.FindResource("CheckBoxPadding"), checkBox.Padding);
            Assert.AreEqual((Thickness)Application.Current.FindResource("CheckBoxBorderThickness"), checkBox.BorderThickness);
            Assert.AreEqual((double)Application.Current.FindResource("CheckBoxHeight"), checkBox.MinHeight);
            Assert.AreEqual((double)Application.Current.FindResource("CheckBoxMinWidth"), checkBox.MinWidth);
            Assert.AreEqual(HorizontalAlignment.Left, checkBox.HorizontalAlignment);
            Assert.AreEqual(VerticalAlignment.Center, checkBox.VerticalAlignment);
            Assert.AreEqual(HorizontalAlignment.Left, checkBox.HorizontalContentAlignment);
            Assert.AreEqual(VerticalAlignment.Top, checkBox.VerticalContentAlignment);
            Assert.AreEqual(FontWeights.Normal, checkBox.FontWeight);
            Assert.IsTrue(checkBox.Focusable);
            Assert.IsTrue(KeyboardNavigation.GetIsTabStop(checkBox));
            Assert.AreEqual(checkBox.TryFindResource("UseSystemFocusVisuals"), FocusVisualHelper.GetUseSystemFocusVisuals(checkBox));
            Assert.AreEqual(checkBox.TryFindResource("CheckBoxFocusVisualMargin"), FocusVisualHelper.GetFocusVisualMargin(checkBox));
            Assert.AreEqual(checkBox.TryFindResource("ControlCornerRadius"), checkBox.GetValue(System.Windows.Controls.Border.CornerRadiusProperty));
            Assert.IsTrue(checkBox.SnapsToDevicePixels);
            Assert.IsTrue(checkBox.OverridesDefaultStyle);
            AssertTemplateUsesOfficialWpfPresenter(checkBox);
            AssertOfficialTriggerShape(checkBox.Template);
            AssertUncheckedDisabledTriggerAppliesResources(checkBox);
        });
    }

    [TestMethod]
    public void CheckedAndIndeterminateStatesUseOfficialWpfFluentResources()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var checkBox = CreateCheckBox();
            checkBox.IsThreeState = true;
            checkBox.IsChecked = null;
            using var host = new TestWindowHost(checkBox, width: 180, height: 80);
            host.UpdateLayout();

            var rootBorder = GetTemplateChild<Border>(checkBox, "RootBorder");
            var iconPresenter = GetTemplateChild<Border>(checkBox, "ControlBorderIconPresenter");
            var strokeBorder = GetTemplateChild<Border>(checkBox, "StrokeBorder");
            var controlIcon = GetTemplateChild<TextBlock>(checkBox, "ControlIcon");

            Assert.AreEqual(Visibility.Visible, controlIcon.Visibility);
            Assert.AreEqual((string)Application.Current.FindResource("CheckBoxIndeterminateGlyph"), controlIcon.Text);
            Assert.AreSame(iconPresenter.TryFindResource("CheckBoxCheckBackgroundFillIndeterminate"), iconPresenter.Background);
            Assert.AreSame(strokeBorder.TryFindResource("CheckBoxCheckBackgroundStrokeIndeterminate"), strokeBorder.BorderBrush);
            Assert.AreSame(rootBorder.TryFindResource("CheckBoxBorderBrushIndeterminate"), rootBorder.BorderBrush);
            Assert.AreSame(rootBorder.TryFindResource("CheckBoxBackgroundIndeterminate"), rootBorder.Background);

            checkBox.IsChecked = true;
            host.UpdateLayout();

            Assert.AreEqual(Visibility.Visible, controlIcon.Visibility);
            Assert.AreEqual((string)Application.Current.FindResource("CheckBoxCheckedGlyph"), controlIcon.Text);
            Assert.AreSame(iconPresenter.TryFindResource("CheckBoxCheckBackgroundFillChecked"), iconPresenter.Background);
            Assert.AreSame(strokeBorder.TryFindResource("CheckBoxCheckBackgroundStrokeChecked"), strokeBorder.BorderBrush);
            Assert.AreSame(rootBorder.TryFindResource("CheckBoxBorderBrushChecked"), rootBorder.BorderBrush);
            Assert.AreSame(rootBorder.TryFindResource("CheckBoxBackgroundChecked"), rootBorder.Background);
        });
    }

    [TestMethod]
    public void RightToLeftCheckBoxKeepsCheckGlyphOrientation()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var checkBox = CreateCheckBox();
            checkBox.Content = "خيار";
            checkBox.IsChecked = true;
            checkBox.FlowDirection = FlowDirection.RightToLeft;
            using var host = new TestWindowHost(checkBox, width: 180, height: 80);
            host.UpdateLayout();

            var rootGrid = GetTemplateChild<Grid>(checkBox, "RootGrid");
            var controlIcon = GetTemplateChild<TextBlock>(checkBox, "ControlIcon");
            var contentPresenter = GetTemplateChild<ContentPresenter>(checkBox, "ContentPresenter");

            Assert.AreEqual(FlowDirection.RightToLeft, rootGrid.FlowDirection);
            Assert.AreEqual(FlowDirection.RightToLeft, contentPresenter.FlowDirection);
            Assert.AreEqual(
                FlowDirection.LeftToRight,
                controlIcon.FlowDirection,
                "Only the direction-neutral check glyph should opt out of the inherited right-to-left layout.");
        });
    }

    [TestMethod]
    public void ThemeDictionariesExposeOfficialCheckBoxGlyphResources()
    {
        WpfTestHost.Run(() =>
        {
            AssertThemeResourceReference("Light", "CheckBoxCheckGlyphForeground", "TextOnAccentFillColorPrimaryBrush");
            AssertThemeResourceReference("Light", "CheckBoxCheckGlyphForegroundPressed", "TextOnAccentFillColorSecondaryBrush");
            AssertThemeResourceReference("Light", "CheckBoxCheckGlyphForegroundDisabled", "TextFillColorDisabledBrush");

            AssertThemeResourceReference("Dark", "CheckBoxCheckGlyphForeground", "TextOnAccentFillColorPrimaryBrush");
            AssertThemeResourceReference("Dark", "CheckBoxCheckGlyphForegroundPressed", "TextOnAccentFillColorSecondaryBrush");
            AssertThemeResourceReference("Dark", "CheckBoxCheckGlyphForegroundDisabled", "TextFillColorDisabledBrush");

            AssertThemeResourceReference("HighContrast", "CheckBoxCheckGlyphForeground", "SystemColorButtonFaceColorBrush");
            AssertThemeResourceReference("HighContrast", "CheckBoxCheckGlyphForegroundPressed", "SystemColorHighlightTextColorBrush");
            AssertThemeResourceReference("HighContrast", "CheckBoxCheckGlyphForegroundDisabled", "SystemColorWindowColorBrush");
        });
    }

    [TestMethod]
    public void ThemeResourcesUseOfficialCheckBoxHighContrastTokens()
    {
        WpfTestHost.Run(() =>
        {
            foreach (var themeName in new[] { "Light", "Dark" })
            {
                AssertThemeResourceReferences(themeName,
                    ("CheckBoxForegroundUnchecked", "TextFillColorPrimaryBrush"),
                    ("CheckBoxForegroundUncheckedPointerOver", "TextFillColorPrimaryBrush"),
                    ("CheckBoxForegroundUncheckedPressed", "TextFillColorPrimaryBrush"),
                    ("CheckBoxForegroundUncheckedDisabled", "TextFillColorDisabledBrush"),
                    ("CheckBoxForegroundChecked", "TextFillColorPrimaryBrush"),
                    ("CheckBoxForegroundCheckedPointerOver", "TextFillColorPrimaryBrush"),
                    ("CheckBoxForegroundCheckedPressed", "TextFillColorPrimaryBrush"),
                    ("CheckBoxForegroundCheckedDisabled", "TextFillColorDisabledBrush"),
                    ("CheckBoxForegroundIndeterminate", "TextFillColorPrimaryBrush"),
                    ("CheckBoxForegroundIndeterminatePointerOver", "TextFillColorPrimaryBrush"),
                    ("CheckBoxForegroundIndeterminatePressed", "TextFillColorPrimaryBrush"),
                    ("CheckBoxForegroundIndeterminateDisabled", "TextFillColorDisabledBrush"),
                    ("CheckBoxBackgroundUnchecked", "SubtleFillColorTransparentBrush"),
                    ("CheckBoxBackgroundUncheckedPointerOver", "SubtleFillColorTransparentBrush"),
                    ("CheckBoxBackgroundUncheckedPressed", "SubtleFillColorTransparentBrush"),
                    ("CheckBoxBackgroundUncheckedDisabled", "SubtleFillColorTransparentBrush"),
                    ("CheckBoxBackgroundChecked", "SubtleFillColorTransparentBrush"),
                    ("CheckBoxBackgroundCheckedPointerOver", "SubtleFillColorTransparentBrush"),
                    ("CheckBoxBackgroundCheckedPressed", "SubtleFillColorTransparentBrush"),
                    ("CheckBoxBackgroundCheckedDisabled", "SubtleFillColorTransparentBrush"),
                    ("CheckBoxBackgroundIndeterminate", "SubtleFillColorTransparentBrush"),
                    ("CheckBoxBackgroundIndeterminatePointerOver", "SubtleFillColorTransparentBrush"),
                    ("CheckBoxBackgroundIndeterminatePressed", "SubtleFillColorTransparentBrush"),
                    ("CheckBoxBackgroundIndeterminateDisabled", "SubtleFillColorTransparentBrush"),
                    ("CheckBoxBorderBrushUnchecked", "SubtleFillColorTransparentBrush"),
                    ("CheckBoxBorderBrushUncheckedPointerOver", "SubtleFillColorTransparentBrush"),
                    ("CheckBoxBorderBrushUncheckedPressed", "SubtleFillColorTransparentBrush"),
                    ("CheckBoxBorderBrushUncheckedDisabled", "SubtleFillColorTransparentBrush"),
                    ("CheckBoxBorderBrushChecked", "SubtleFillColorTransparentBrush"),
                    ("CheckBoxBorderBrushCheckedPointerOver", "SubtleFillColorTransparentBrush"),
                    ("CheckBoxBorderBrushCheckedPressed", "SubtleFillColorTransparentBrush"),
                    ("CheckBoxBorderBrushCheckedDisabled", "SubtleFillColorTransparentBrush"),
                    ("CheckBoxBorderBrushIndeterminate", "SubtleFillColorTransparentBrush"),
                    ("CheckBoxBorderBrushIndeterminatePointerOver", "SubtleFillColorTransparentBrush"),
                    ("CheckBoxBorderBrushIndeterminatePressed", "SubtleFillColorTransparentBrush"),
                    ("CheckBoxBorderBrushIndeterminateDisabled", "SubtleFillColorTransparentBrush"),
                    ("CheckBoxCheckBackgroundStrokeUnchecked", "ControlStrongStrokeColorDefaultBrush"),
                    ("CheckBoxCheckBackgroundStrokeUncheckedPointerOver", "ControlStrongStrokeColorDefaultBrush"),
                    ("CheckBoxCheckBackgroundStrokeUncheckedPressed", "ControlStrongStrokeColorDisabledBrush"),
                    ("CheckBoxCheckBackgroundStrokeUncheckedDisabled", "ControlStrongStrokeColorDisabledBrush"),
                    ("CheckBoxCheckBackgroundStrokeChecked", "AccentFillColorDefaultBrush"),
                    ("CheckBoxCheckBackgroundStrokeCheckedPointerOver", "AccentFillColorSecondaryBrush"),
                    ("CheckBoxCheckBackgroundStrokeCheckedPressed", "AccentFillColorTertiaryBrush"),
                    ("CheckBoxCheckBackgroundStrokeCheckedDisabled", "ControlStrongStrokeColorDisabledBrush"),
                    ("CheckBoxCheckBackgroundStrokeIndeterminate", "AccentFillColorDefaultBrush"),
                    ("CheckBoxCheckBackgroundStrokeIndeterminatePointerOver", "AccentFillColorSecondaryBrush"),
                    ("CheckBoxCheckBackgroundStrokeIndeterminatePressed", "AccentFillColorTertiaryBrush"),
                    ("CheckBoxCheckBackgroundStrokeIndeterminateDisabled", "ControlStrongStrokeColorDisabledBrush"),
                    ("CheckBoxCheckBackgroundFillUnchecked", "ControlAltFillColorSecondaryBrush"),
                    ("CheckBoxCheckBackgroundFillUncheckedPointerOver", "ControlAltFillColorTertiaryBrush"),
                    ("CheckBoxCheckBackgroundFillUncheckedPressed", "ControlAltFillColorQuarternaryBrush"),
                    ("CheckBoxCheckBackgroundFillUncheckedDisabled", "ControlAltFillColorDisabledBrush"),
                    ("CheckBoxCheckBackgroundFillChecked", "AccentFillColorDefaultBrush"),
                    ("CheckBoxCheckBackgroundFillCheckedPointerOver", "AccentFillColorSecondaryBrush"),
                    ("CheckBoxCheckBackgroundFillCheckedPressed", "AccentFillColorTertiaryBrush"),
                    ("CheckBoxCheckBackgroundFillCheckedDisabled", "AccentFillColorDisabledBrush"),
                    ("CheckBoxCheckBackgroundFillIndeterminate", "AccentFillColorDefaultBrush"),
                    ("CheckBoxCheckBackgroundFillIndeterminatePointerOver", "AccentFillColorSecondaryBrush"),
                    ("CheckBoxCheckBackgroundFillIndeterminatePressed", "AccentFillColorTertiaryBrush"),
                    ("CheckBoxCheckBackgroundFillIndeterminateDisabled", "AccentFillColorDisabledBrush"),
                    ("CheckBoxCheckGlyphForeground", "TextOnAccentFillColorPrimaryBrush"),
                    ("CheckBoxCheckGlyphForegroundPressed", "TextOnAccentFillColorSecondaryBrush"),
                    ("CheckBoxCheckGlyphForegroundDisabled", "TextFillColorDisabledBrush"),
                    ("CheckBoxCheckGlyphForegroundUnchecked", "TextOnAccentFillColorPrimaryBrush"),
                    ("CheckBoxCheckGlyphForegroundUncheckedPointerOver", "TextOnAccentFillColorPrimaryBrush"),
                    ("CheckBoxCheckGlyphForegroundUncheckedPressed", "TextOnAccentFillColorPrimaryBrush"),
                    ("CheckBoxCheckGlyphForegroundUncheckedDisabled", "TextOnAccentFillColorDisabledBrush"),
                    ("CheckBoxCheckGlyphForegroundChecked", "TextOnAccentFillColorPrimaryBrush"),
                    ("CheckBoxCheckGlyphForegroundCheckedPointerOver", "TextOnAccentFillColorPrimaryBrush"),
                    ("CheckBoxCheckGlyphForegroundCheckedPressed", "TextOnAccentFillColorSecondaryBrush"),
                    ("CheckBoxCheckGlyphForegroundCheckedDisabled", "TextOnAccentFillColorDisabledBrush"),
                    ("CheckBoxCheckGlyphForegroundIndeterminate", "TextOnAccentFillColorPrimaryBrush"),
                    ("CheckBoxCheckGlyphForegroundIndeterminatePointerOver", "TextOnAccentFillColorPrimaryBrush"),
                    ("CheckBoxCheckGlyphForegroundIndeterminatePressed", "TextOnAccentFillColorSecondaryBrush"),
                    ("CheckBoxCheckGlyphForegroundIndeterminateDisabled", "TextOnAccentFillColorDisabledBrush"));
            }

            AssertThemeResourceReferences("HighContrast",
                ("CheckBoxForegroundUnchecked", "SystemColorButtonTextColorBrush"),
                ("CheckBoxForegroundUncheckedPointerOver", "SystemColorButtonTextColorBrush"),
                ("CheckBoxForegroundUncheckedPressed", "SystemColorButtonTextColorBrush"),
                ("CheckBoxForegroundUncheckedDisabled", "SystemColorGrayTextColorBrush"),
                ("CheckBoxForegroundChecked", "SystemColorButtonTextColorBrush"),
                ("CheckBoxForegroundCheckedPointerOver", "SystemColorButtonTextColorBrush"),
                ("CheckBoxForegroundCheckedPressed", "SystemColorButtonTextColorBrush"),
                ("CheckBoxForegroundCheckedDisabled", "SystemColorGrayTextColorBrush"),
                ("CheckBoxForegroundIndeterminate", "SystemColorWindowTextColorBrush"),
                ("CheckBoxForegroundIndeterminatePointerOver", "SystemColorWindowTextColorBrush"),
                ("CheckBoxForegroundIndeterminatePressed", "SystemColorWindowTextColorBrush"),
                ("CheckBoxForegroundIndeterminateDisabled", "SystemColorGrayTextColorBrush"),
                ("CheckBoxBackgroundUnchecked", "SystemColorWindowColorBrush"),
                ("CheckBoxBackgroundUncheckedPointerOver", "SystemColorWindowColorBrush"),
                ("CheckBoxBackgroundUncheckedPressed", "SystemColorWindowColorBrush"),
                ("CheckBoxBackgroundUncheckedDisabled", "SystemColorWindowColorBrush"),
                ("CheckBoxBackgroundChecked", "SystemColorWindowColorBrush"),
                ("CheckBoxBackgroundCheckedPointerOver", "SystemColorWindowColorBrush"),
                ("CheckBoxBackgroundCheckedPressed", "SystemColorWindowColorBrush"),
                ("CheckBoxBackgroundCheckedDisabled", "SystemColorWindowColorBrush"),
                ("CheckBoxBackgroundIndeterminate", "SystemColorWindowColorBrush"),
                ("CheckBoxBackgroundIndeterminatePointerOver", "SystemColorWindowColorBrush"),
                ("CheckBoxBackgroundIndeterminatePressed", "SystemColorWindowColorBrush"),
                ("CheckBoxBackgroundIndeterminateDisabled", "SystemColorWindowColorBrush"),
                ("CheckBoxBorderBrushUnchecked", "SystemColorWindowColorBrush"),
                ("CheckBoxBorderBrushUncheckedPointerOver", "SystemColorWindowColorBrush"),
                ("CheckBoxBorderBrushUncheckedPressed", "SystemColorWindowColorBrush"),
                ("CheckBoxBorderBrushUncheckedDisabled", "SystemColorWindowColorBrush"),
                ("CheckBoxBorderBrushChecked", "SystemColorWindowColorBrush"),
                ("CheckBoxBorderBrushCheckedPointerOver", "SystemColorWindowColorBrush"),
                ("CheckBoxBorderBrushCheckedPressed", "SystemColorWindowColorBrush"),
                ("CheckBoxBorderBrushCheckedDisabled", "SystemColorWindowColorBrush"),
                ("CheckBoxBorderBrushIndeterminate", "SystemColorWindowColorBrush"),
                ("CheckBoxBorderBrushIndeterminatePointerOver", "SystemColorWindowColorBrush"),
                ("CheckBoxBorderBrushIndeterminatePressed", "SystemColorWindowColorBrush"),
                ("CheckBoxBorderBrushIndeterminateDisabled", "SystemColorWindowColorBrush"),
                ("CheckBoxCheckBackgroundStrokeUnchecked", "SystemColorButtonTextColorBrush"),
                ("CheckBoxCheckBackgroundStrokeUncheckedPointerOver", "SystemColorHighlightColorBrush"),
                ("CheckBoxCheckBackgroundStrokeUncheckedPressed", "SystemColorHighlightColorBrush"),
                ("CheckBoxCheckBackgroundStrokeUncheckedDisabled", "SystemColorGrayTextColorBrush"),
                ("CheckBoxCheckBackgroundStrokeChecked", "SystemColorHighlightColorBrush"),
                ("CheckBoxCheckBackgroundStrokeCheckedPointerOver", "SystemColorButtonTextColorBrush"),
                ("CheckBoxCheckBackgroundStrokeCheckedPressed", "SystemColorButtonFaceColorBrush"),
                ("CheckBoxCheckBackgroundStrokeCheckedDisabled", "SystemColorGrayTextColorBrush"),
                ("CheckBoxCheckBackgroundStrokeIndeterminate", "SystemColorHighlightColorBrush"),
                ("CheckBoxCheckBackgroundStrokeIndeterminatePointerOver", "SystemColorHighlightColorBrush"),
                ("CheckBoxCheckBackgroundStrokeIndeterminatePressed", "SystemColorHighlightColorBrush"),
                ("CheckBoxCheckBackgroundStrokeIndeterminateDisabled", "SystemColorGrayTextColorBrush"),
                ("CheckBoxCheckBackgroundFillUnchecked", "SystemColorButtonFaceColorBrush"),
                ("CheckBoxCheckBackgroundFillUncheckedPointerOver", "SystemColorHighlightTextColorBrush"),
                ("CheckBoxCheckBackgroundFillUncheckedPressed", "SystemColorHighlightColorBrush"),
                ("CheckBoxCheckBackgroundFillUncheckedDisabled", "SystemColorWindowColorBrush"),
                ("CheckBoxCheckBackgroundFillChecked", "SystemColorHighlightColorBrush"),
                ("CheckBoxCheckBackgroundFillCheckedPointerOver", "SystemColorButtonTextColorBrush"),
                ("CheckBoxCheckBackgroundFillCheckedPressed", "SystemColorButtonFaceColorBrush"),
                ("CheckBoxCheckBackgroundFillCheckedDisabled", "SystemColorGrayTextColorBrush"),
                ("CheckBoxCheckBackgroundFillIndeterminate", "SystemColorHighlightColorBrush"),
                ("CheckBoxCheckBackgroundFillIndeterminatePointerOver", "SystemColorHighlightTextColorBrush"),
                ("CheckBoxCheckBackgroundFillIndeterminatePressed", "SystemColorHighlightColorBrush"),
                ("CheckBoxCheckBackgroundFillIndeterminateDisabled", "SystemColorGrayTextColorBrush"),
                ("CheckBoxCheckGlyphForeground", "SystemColorButtonFaceColorBrush"),
                ("CheckBoxCheckGlyphForegroundPressed", "SystemColorHighlightTextColorBrush"),
                ("CheckBoxCheckGlyphForegroundDisabled", "SystemColorWindowColorBrush"),
                ("CheckBoxCheckGlyphForegroundUnchecked", "SystemColorButtonFaceColorBrush"),
                ("CheckBoxCheckGlyphForegroundUncheckedPointerOver", "SystemColorHighlightColorBrush"),
                ("CheckBoxCheckGlyphForegroundUncheckedPressed", "SystemColorHighlightTextColorBrush"),
                ("CheckBoxCheckGlyphForegroundUncheckedDisabled", "SystemColorGrayTextColorBrush"),
                ("CheckBoxCheckGlyphForegroundChecked", "SystemColorHighlightTextColorBrush"),
                ("CheckBoxCheckGlyphForegroundCheckedPointerOver", "SystemColorButtonFaceColorBrush"),
                ("CheckBoxCheckGlyphForegroundCheckedPressed", "SystemColorButtonTextColorBrush"),
                ("CheckBoxCheckGlyphForegroundCheckedDisabled", "SystemColorWindowColorBrush"),
                ("CheckBoxCheckGlyphForegroundIndeterminate", "SystemColorHighlightTextColorBrush"),
                ("CheckBoxCheckGlyphForegroundIndeterminatePointerOver", "SystemColorHighlightColorBrush"),
                ("CheckBoxCheckGlyphForegroundIndeterminatePressed", "SystemColorHighlightTextColorBrush"),
                ("CheckBoxCheckGlyphForegroundIndeterminateDisabled", "SystemColorWindowColorBrush"));
        });
    }

    private static CheckBox CreateCheckBox()
    {
        return new CheckBox
        {
            Width = 150,
            Height = 48,
            Content = "Option"
        };
    }

    private static void AssertTemplateUsesOfficialWpfPresenter(CheckBox checkBox)
    {
        checkBox.ApplyTemplate();

        var rootBorder = GetTemplateChild<Border>(checkBox, "RootBorder");
        var rootGrid = GetTemplateChild<Grid>(checkBox, "RootGrid");
        var iconPresenter = GetTemplateChild<Border>(checkBox, "ControlBorderIconPresenter");
        var strokeBorder = GetTemplateChild<Border>(checkBox, "StrokeBorder");
        var controlIcon = GetTemplateChild<TextBlock>(checkBox, "ControlIcon");
        var contentPresenter = GetTemplateChild<ContentPresenter>(checkBox, "ContentPresenter");

        Assert.AreEqual(typeof(Grid), rootGrid.GetType());
        Assert.AreEqual(typeof(ContentPresenter), contentPresenter.GetType());
        Assert.AreEqual(checkBox.Content, contentPresenter.Content);
        Assert.AreEqual(checkBox.Padding, contentPresenter.Margin);
        Assert.AreEqual(checkBox.HorizontalContentAlignment, contentPresenter.HorizontalAlignment);
        Assert.AreEqual(checkBox.VerticalContentAlignment, contentPresenter.VerticalAlignment);
        Assert.IsTrue(contentPresenter.RecognizesAccessKey);
        Assert.AreSame(checkBox.Background, rootBorder.Background);
        Assert.AreSame(checkBox.BorderBrush, rootBorder.BorderBrush);
        Assert.AreEqual(checkBox.BorderThickness, rootBorder.BorderThickness);
        Assert.AreEqual(((CornerRadius)checkBox.GetValue(System.Windows.Controls.Border.CornerRadiusProperty)), rootBorder.CornerRadius);
        Assert.AreEqual(((CornerRadius)checkBox.GetValue(System.Windows.Controls.Border.CornerRadiusProperty)), iconPresenter.CornerRadius);
        Assert.AreEqual(((CornerRadius)checkBox.GetValue(System.Windows.Controls.Border.CornerRadiusProperty)), strokeBorder.CornerRadius);
        Assert.AreEqual((Thickness)Application.Current.FindResource("CheckBoxBorderThickness"), strokeBorder.BorderThickness);
        Assert.AreEqual((double)Application.Current.FindResource("CheckBoxSize"), iconPresenter.Width);
        Assert.AreEqual((double)Application.Current.FindResource("CheckBoxSize"), iconPresenter.Height);
        Assert.AreSame(iconPresenter.TryFindResource("CheckBoxCheckBackgroundFillUnchecked"), iconPresenter.Background);
        Assert.AreSame(strokeBorder.TryFindResource("CheckBoxCheckBackgroundStrokeUnchecked"), strokeBorder.BorderBrush);
        Assert.AreEqual((double)Application.Current.FindResource("CheckBoxIconSize"), controlIcon.FontSize);
        Assert.AreSame(controlIcon.TryFindResource("SymbolThemeFontFamily"), controlIcon.FontFamily);
        Assert.AreEqual(FontWeights.Bold, controlIcon.FontWeight);
        Assert.AreSame(controlIcon.TryFindResource("CheckBoxCheckGlyphForeground"), controlIcon.Foreground);
        Assert.AreEqual((string)Application.Current.FindResource("CheckBoxCheckedGlyph"), controlIcon.Text);
        Assert.AreEqual(Visibility.Collapsed, controlIcon.Visibility);
        Assert.AreEqual(0, VisualStateManager.GetVisualStateGroups(rootBorder).Count);
        Assert.AreEqual(0, VisualStateManager.GetVisualStateGroups(rootGrid).Count);
    }

    private static void AssertOfficialTriggerShape(ControlTemplate template)
    {
        var triggers = template.Triggers.OfType<Trigger>().ToArray();
        var multiTriggers = template.Triggers.OfType<MultiTrigger>().ToArray();

        Assert.AreEqual(5, triggers.Length);
        Assert.AreEqual(6, multiTriggers.Length);

        AssertContentNullTrigger(triggers);
        AssertContentEmptyTrigger(triggers);
        AssertIndeterminateTrigger(triggers);
        AssertCheckedTrigger(triggers);
        AssertDisabledTrigger(triggers);

        AssertTrigger(multiTriggers,
            new[] { ("IsMouseOver", (object?)true), ("IsChecked", (object?)false), ("IsPressed", (object?)false) },
            ("ControlBorderIconPresenter", "Background", "CheckBoxCheckBackgroundFillUncheckedPointerOver"),
            ("StrokeBorder", "BorderBrush", "CheckBoxCheckBackgroundStrokeUncheckedPointerOver"),
            ("RootBorder", "BorderBrush", "CheckBoxBorderBrushUncheckedPointerOver"),
            ("RootBorder", "Background", "CheckBoxBackgroundUncheckedPointerOver"));

        AssertTrigger(multiTriggers,
            new[] { ("IsMouseOver", (object?)true), ("IsChecked", (object?)false), ("IsPressed", (object?)true) },
            ("ControlBorderIconPresenter", "Background", "CheckBoxCheckBackgroundFillUncheckedPressed"),
            ("StrokeBorder", "BorderBrush", "CheckBoxCheckBackgroundStrokeUncheckedPressed"),
            ("RootBorder", "BorderBrush", "CheckBoxBorderBrushUncheckedPressed"),
            ("RootBorder", "Background", "CheckBoxBackgroundUncheckedPressed"));

        AssertTrigger(multiTriggers,
            new[] { ("IsMouseOver", (object)true), ("IsChecked", null), ("IsPressed", (object)false) },
            ("ControlBorderIconPresenter", "Background", "CheckBoxCheckBackgroundFillIndeterminatePointerOver"),
            ("StrokeBorder", "BorderBrush", "CheckBoxCheckBackgroundStrokeIndeterminatePointerOver"),
            ("RootBorder", "BorderBrush", "CheckBoxBorderBrushIndeterminatePointerOver"),
            ("RootBorder", "Background", "CheckBoxBackgroundIndeterminatePointerOver"));

        AssertTrigger(multiTriggers,
            new[] { ("IsMouseOver", (object)true), ("IsChecked", null), ("IsPressed", (object)true) },
            ("ControlBorderIconPresenter", "Background", "CheckBoxCheckBackgroundFillIndeterminatePressed"),
            ("StrokeBorder", "BorderBrush", "CheckBoxCheckBackgroundStrokeIndeterminatePressed"),
            ("RootBorder", "BorderBrush", "CheckBoxBorderBrushIndeterminatePressed"),
            ("RootBorder", "Background", "CheckBoxBackgroundIndeterminatePressed"),
            ("ControlIcon", "Foreground", "CheckBoxCheckGlyphForegroundPressed"));

        AssertTrigger(multiTriggers,
            new[] { ("IsMouseOver", (object?)true), ("IsChecked", (object?)true), ("IsPressed", (object?)false) },
            ("ControlBorderIconPresenter", "Background", "CheckBoxCheckBackgroundFillCheckedPointerOver"),
            ("StrokeBorder", "BorderBrush", "CheckBoxCheckBackgroundStrokeCheckedPointerOver"),
            ("RootBorder", "BorderBrush", "CheckBoxBorderBrushCheckedPointerOver"),
            ("RootBorder", "Background", "CheckBoxBackgroundCheckedPointerOver"));

        AssertTrigger(multiTriggers,
            new[] { ("IsMouseOver", (object?)true), ("IsChecked", (object?)true), ("IsPressed", (object?)true) },
            ("ControlBorderIconPresenter", "Background", "CheckBoxCheckBackgroundFillCheckedPressed"),
            ("StrokeBorder", "BorderBrush", "CheckBoxCheckBackgroundStrokeCheckedPressed"),
            ("RootBorder", "BorderBrush", "CheckBoxBorderBrushCheckedPressed"),
            ("RootBorder", "Background", "CheckBoxBackgroundCheckedPressed"),
            ("ControlIcon", "Foreground", "CheckBoxCheckGlyphForegroundPressed"));
    }

    private static void AssertContentNullTrigger(Trigger[] triggers)
    {
        var trigger = triggers.Single(item => item.Property.Name == "Content" && item.Value == null);
        AssertSetters(trigger.Setters.OfType<Setter>().ToArray(),
            ("ContentPresenter", "Margin", new Thickness(0)),
            ("", "MinWidth", 30.0));
    }

    private static void AssertContentEmptyTrigger(Trigger[] triggers)
    {
        var trigger = triggers.Single(item => item.Property.Name == "Content" && Equals(item.Value, string.Empty));
        AssertSetters(trigger.Setters.OfType<Setter>().ToArray(),
            ("ContentPresenter", "Margin", new Thickness(0)),
            ("", "MinWidth", 30.0));
    }

    private static void AssertIndeterminateTrigger(Trigger[] triggers)
    {
        var trigger = triggers.Single(item => item.Property.Name == "IsChecked" && item.Value == null);
        AssertSetters(trigger.Setters.OfType<Setter>().ToArray(),
            ("ControlIcon", "Text", "CheckBoxIndeterminateGlyph"),
            ("ControlIcon", "Visibility", Visibility.Visible),
            ("ControlBorderIconPresenter", "Background", "CheckBoxCheckBackgroundFillIndeterminate"),
            ("StrokeBorder", "BorderBrush", "CheckBoxCheckBackgroundStrokeIndeterminate"),
            ("RootBorder", "BorderBrush", "CheckBoxBorderBrushIndeterminate"),
            ("RootBorder", "Background", "CheckBoxBackgroundIndeterminate"));
    }

    private static void AssertCheckedTrigger(Trigger[] triggers)
    {
        var trigger = triggers.Single(item => item.Property.Name == "IsChecked" && Equals(item.Value, true));
        AssertSetters(trigger.Setters.OfType<Setter>().ToArray(),
            ("ControlIcon", "Visibility", Visibility.Visible),
            ("ControlBorderIconPresenter", "Background", "CheckBoxCheckBackgroundFillChecked"),
            ("StrokeBorder", "BorderBrush", "CheckBoxCheckBackgroundStrokeChecked"),
            ("RootBorder", "BorderBrush", "CheckBoxBorderBrushChecked"),
            ("RootBorder", "Background", "CheckBoxBackgroundChecked"));
    }

    private static void AssertDisabledTrigger(Trigger[] triggers)
    {
        var trigger = triggers.Single(item => item.Property.Name == "IsEnabled" && Equals(item.Value, false));
        AssertSetters(trigger.Setters.OfType<Setter>().ToArray(),
            ("ControlBorderIconPresenter", "Background", "CheckBoxCheckBackgroundFillUncheckedDisabled"),
            ("StrokeBorder", "BorderBrush", "CheckBoxCheckBackgroundStrokeUncheckedDisabled"),
            ("ControlIcon", "Foreground", "CheckBoxForegroundUncheckedDisabled"),
            ("", "Foreground", "TextFillColorDisabledBrush"),
            ("RootBorder", "BorderBrush", "CheckBoxBorderBrushUncheckedPressed"),
            ("RootBorder", "Background", "CheckBoxBackgroundUncheckedPressed"));
    }

    private static void AssertTrigger(
        MultiTrigger[] triggers,
        (string PropertyName, object? Value)[] expectedConditions,
        params (string TargetName, string PropertyName, object Value)[] expectedSetters)
    {
        var trigger = triggers.Single(item => expectedConditions.All(condition => HasCondition(item, condition.PropertyName, condition.Value)));
        AssertSetters(trigger.Setters.OfType<Setter>().ToArray(), expectedSetters);
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

    private static bool HasCondition(MultiTrigger trigger, string propertyName, object? value)
    {
        return trigger.Conditions.Cast<Condition>().Any(item =>
            item.Property.Name == propertyName &&
            Equals(item.Value, value));
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
            }
            else
            {
                var resolvedResource = Application.Current.TryFindResource(resourceKey);
                if (resolvedResource != null && Equals(setter.Value, resolvedResource))
                {
                    return;
                }

                Assert.IsInstanceOfType(setter.Value, typeof(StaticResourceExtension));
                var staticResource = (StaticResourceExtension)setter.Value;
                Assert.AreEqual(resourceKey, staticResource.ResourceKey);
            }
        }
        else
        {
            Assert.AreEqual(value, setter.Value);
        }
    }

    private static void AssertUncheckedDisabledTriggerAppliesResources(CheckBox checkBox)
    {
        var rootBorder = GetTemplateChild<Border>(checkBox, "RootBorder");
        var iconPresenter = GetTemplateChild<Border>(checkBox, "ControlBorderIconPresenter");
        var strokeBorder = GetTemplateChild<Border>(checkBox, "StrokeBorder");
        var controlIcon = GetTemplateChild<TextBlock>(checkBox, "ControlIcon");

        checkBox.IsEnabled = false;
        checkBox.UpdateLayout();

        Assert.AreSame(checkBox.TryFindResource("TextFillColorDisabledBrush"), checkBox.Foreground);
        Assert.AreSame(iconPresenter.TryFindResource("CheckBoxCheckBackgroundFillUncheckedDisabled"), iconPresenter.Background);
        Assert.AreSame(strokeBorder.TryFindResource("CheckBoxCheckBackgroundStrokeUncheckedDisabled"), strokeBorder.BorderBrush);
        Assert.AreSame(controlIcon.TryFindResource("CheckBoxForegroundUncheckedDisabled"), controlIcon.Foreground);
        Assert.AreSame(rootBorder.TryFindResource("CheckBoxBorderBrushUncheckedPressed"), rootBorder.BorderBrush);
        Assert.AreSame(rootBorder.TryFindResource("CheckBoxBackgroundUncheckedPressed"), rootBorder.Background);
    }

    private static void AssertThemeResourceReference(string themeName, string key, object expectedResourceKey)
    {
        var theme = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(theme.Contains(key), $"Theme is missing {key}.");
        Assert.IsTrue(theme.Contains(expectedResourceKey), $"Theme is missing {expectedResourceKey}.");
        Assert.AreSame(theme[expectedResourceKey], theme[key], key);
    }

    private static void AssertThemeResourceReferences(
        string themeName,
        params (string ResourceKey, object ExpectedResourceKey)[] references)
    {
        foreach (var reference in references)
        {
            AssertThemeResourceReference(themeName, reference.ResourceKey, reference.ExpectedResourceKey);
        }
    }

    private static T GetTemplateChild<T>(Control control, string name)
        where T : DependencyObject
    {
        return control.Template.FindName(name, control) as T
            ?? throw new AssertFailedException($"Expected {control.GetType().Name} template child '{name}' to be a {typeof(T).Name}.");
    }
}
