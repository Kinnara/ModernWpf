using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.CommonStyles;

[TestClass]
public class TextBoxPasswordBoxVisualStateTests
{
    [TestMethod]
    public void DefaultTextBoxStyleUsesOfficialWpfFluentTemplateShape()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var defaultStyle = (Style)Application.Current.FindResource("DefaultTextBoxStyle");
            var implicitStyle = (Style)Application.Current.FindResource(typeof(TextBox));
            Assert.AreEqual(typeof(TextBox), defaultStyle.TargetType);
            Assert.AreEqual(typeof(TextBox), implicitStyle.TargetType);
            Assert.AreSame(defaultStyle, implicitStyle.BasedOn);
            AssertTextBoxStyleSetters(defaultStyle);

            var textBox = new TextBox
            {
                Text = "Text value",
                Width = 240
            };

            using var host = new TestWindowHost(textBox, width: 320, height: 120);
            host.UpdateLayout();

            AssertTextBoxStyleSetters(textBox);
            AssertTextBoxTemplateShape(textBox);
            AssertTextBoxTriggerShape(textBox.Template);
            AssertTextBoxClearButtonSubstitutionClearsText(textBox);
            AssertDisabledTextControlTemplateResources(textBox);
        });
    }

    [TestMethod]
    public void InitialTextBoxValidationErrorAdornerClearsWhenValueBecomesValid()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            AssertInitialTextBoxValidationErrorClears();
            AssertInitialTextBoxValidationErrorClears(
                (ControlTemplate)Application.Current.FindResource("DataGridTextControlValidationErrorTemplate"));
        });
    }

    [TestMethod]
    public void DefaultTextBoxBaseStyleUsesOfficialWpfFluentTemplateShape()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var baseStyle = (Style)Application.Current.FindResource("DefaultTextBoxBaseStyle");
            Assert.AreEqual(typeof(TextBoxBase), baseStyle.TargetType);

            var setters = baseStyle.Setters.OfType<Setter>().ToArray();
            AssertSetter(setters, Control.FocusVisualStyleProperty, null);
            AssertDynamicResourceSetter(setters, Control.ContextMenuProperty, "TextControlContextMenu");
            AssertDynamicResourceSetter(setters, Control.ForegroundProperty, "TextControlForeground");
            AssertDynamicResourceSetter(setters, TextBoxBase.CaretBrushProperty, "TextControlForeground");
            AssertDynamicResourceSetter(setters, Control.BackgroundProperty, "TextControlBackground");
            AssertDynamicResourceSetter(setters, Control.BorderBrushProperty, "TextControlElevationBorderBrush");
            AssertDynamicResourceSetter(setters, Control.BorderThicknessProperty, "TextControlBorderThemeThickness");
            AssertSetter(setters, ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Hidden);
            AssertSetter(setters, ScrollViewer.VerticalScrollBarVisibilityProperty, ScrollBarVisibility.Hidden);
            AssertSetter(setters, Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Left);
            AssertSetter(setters, Control.VerticalContentAlignmentProperty, VerticalAlignment.Top);
            AssertSetter(setters, FrameworkElement.CursorProperty, Cursors.IBeam);
            AssertDynamicResourceSetter(setters, FrameworkElement.MinHeightProperty, "TextControlThemeMinHeight");
            AssertDynamicResourceSetter(setters, FrameworkElement.MinWidthProperty, "TextControlThemeMinWidth");
            AssertDynamicResourceSetter(setters, Control.PaddingProperty, "TextControlThemePadding");
            AssertDynamicResourceSetter(setters, System.Windows.Controls.Border.CornerRadiusProperty, "ControlCornerRadius");
            AssertSetter(setters, UIElement.AllowDropProperty, true);
            AssertSetter(setters, ScrollViewer.PanningModeProperty, PanningMode.VerticalFirst);
            AssertSetter(setters, Stylus.IsFlicksEnabledProperty, false);
            AssertSetter(setters, TextContextMenu.UsingTextContextMenuProperty, true);
            AssertSetter(setters, Control.OverridesDefaultStyleProperty, true);
            AssertDynamicResourceSetter(setters, TextBoxBase.SelectionBrushProperty, "TextControlSelectionHighlightColor");
            Assert.IsInstanceOfType(setters.Single(item => item.Property == Control.TemplateProperty).Value, typeof(ControlTemplate));
        });
    }

    [TestMethod]
    public void DefaultPasswordBoxStyleUsesOfficialWpfFluentTemplateShape()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var defaultStyle = (Style)Application.Current.FindResource("DefaultPasswordBoxStyle");
            var implicitStyle = (Style)Application.Current.FindResource(typeof(PasswordBox));
            Assert.AreEqual(typeof(PasswordBox), defaultStyle.TargetType);
            Assert.AreEqual(typeof(PasswordBox), implicitStyle.TargetType);
            Assert.AreSame(defaultStyle, implicitStyle.BasedOn);
            AssertPasswordBoxStyleSetters(defaultStyle);

            var passwordBox = new PasswordBox
            {
                Width = 240
            };
            passwordBox.Password = "secret";

            using var host = new TestWindowHost(passwordBox, width: 320, height: 120);
            host.UpdateLayout();

            AssertPasswordBoxStyleSetters(passwordBox);
            AssertPasswordBoxTemplateShape(passwordBox);
            AssertTextControlTriggerShape(passwordBox.Template);
            AssertDisabledTextControlTemplateResources(passwordBox);
        });
    }

    [TestMethod]
    public void DataGridTextBoxStyleRetainsModernWpfEditingSubstitution()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var dataGridTextBoxStyle = (Style)Application.Current.FindResource("DataGridTextBoxStyle");
            var defaultTextBoxStyle = (Style)Application.Current.FindResource("DefaultTextBoxStyle");
            Assert.AreEqual(typeof(TextBox), dataGridTextBoxStyle.TargetType);
            Assert.AreSame(defaultTextBoxStyle, dataGridTextBoxStyle.BasedOn);

            var setters = dataGridTextBoxStyle.Setters.OfType<Setter>().ToArray();
            AssertSetter(setters, FrameworkElement.MinWidthProperty, 0.0);
            AssertSetter(setters, Control.PaddingProperty, new Thickness(11, 0, 6, 0));
            AssertSetter(setters, FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Stretch);
            AssertSetter(setters, Control.VerticalContentAlignmentProperty, VerticalAlignment.Center);
            AssertSetter(setters, System.Windows.Controls.Border.CornerRadiusProperty, new CornerRadius(0));
            AssertDynamicResourceSetter(setters, Validation.ErrorTemplateProperty, "DataGridTextControlValidationErrorTemplate");

            var errorTemplate = Application.Current.FindResource("DataGridTextControlValidationErrorTemplate");
            Assert.IsInstanceOfType(errorTemplate, typeof(ControlTemplate));

            var textBox = new TextBox
            {
                Style = dataGridTextBoxStyle
            };

            using var host = new TestWindowHost(textBox, width: 180, height: 80);
            host.UpdateLayout();

            Assert.AreSame(errorTemplate, Validation.GetErrorTemplate(textBox));
        });
    }

    [TestMethod]
    public void TextEntryStylesDeleteWinUIGuessedTemplateBranches()
    {
        var repoRoot = FindRepoRoot();
        var text = string.Join(
            "\n",
            new[] { "TextBox.xaml", "PasswordBox.xaml" }
                .Select(file => System.IO.File.ReadAllText(System.IO.Path.Combine(repoRoot, "ModernWpf", "Styles", file))));

        Assert.IsFalse(text.Contains("VisualStateEx", System.StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("VisualStateManager", System.StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("ContentPresenterEx", System.StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("HeaderContentPresenter", System.StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("DescriptionPresenter", System.StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("PlaceholderTextContentPresenter", System.StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("PasswordBoxHelper", System.StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("RevealButton", System.StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("OrConverter", System.StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("FontIconFallback", System.StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("TextBoxHelper.HasText", System.StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("TextBoxHelper.IsEnabled", System.StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("TextBoxHelper.IsDeleteButtonVisible", System.StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("TemplateButtonCommand", System.StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("ControlHelper.CornerRadius", System.StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("DefaultControlContextMenu", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ThemeResourcesUseOfficialTextControlHighContrastTokens()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            foreach (var themeName in new[] { "Light", "Dark" })
            {
                AssertThemeResourceReferences(
                    themeName,
                    ("TextControlBackground", "ControlFillColorDefaultBrush"),
                    ("TextControlBackgroundPointerOver", "ControlFillColorSecondaryBrush"),
                    ("TextControlBackgroundFocused", "ControlFillColorInputActiveBrush"),
                    ("TextControlBackgroundDisabled", "ControlFillColorDisabledBrush"),
                    ("TextControlBorderBrush", "TextControlElevationBorderBrush"),
                    ("TextControlBorderBrushPointerOver", "TextControlElevationBorderBrush"),
                    ("TextControlBorderBrushFocused", "TextControlElevationBorderFocusedBrush"),
                    ("TextControlBorderBrushDisabled", "ControlStrokeColorDefaultBrush"),
                    ("TextControlForeground", "TextFillColorPrimaryBrush"),
                    ("TextControlForegroundPointerOver", "TextFillColorPrimaryBrush"),
                    ("TextControlForegroundFocused", "TextFillColorPrimaryBrush"),
                    ("TextControlPlaceholderForeground", "TextFillColorSecondaryBrush"),
                    ("TextControlPlaceholderForegroundPointerOver", "TextFillColorSecondaryBrush"),
                    ("TextControlPlaceholderForegroundFocused", "TextFillColorTertiaryBrush"),
                    ("TextControlPlaceholderForegroundDisabled", "TextFillColorDisabledBrush"),
                    ("TextControlSelectionHighlightColor", "AccentFillColorSelectedTextBackgroundBrush"),
                    ("TextControlButtonBackground", "SubtleFillColorTransparentBrush"),
                    ("TextControlButtonBackgroundPointerOver", "SubtleFillColorSecondaryBrush"),
                    ("TextControlButtonBackgroundPressed", "SubtleFillColorTertiaryBrush"),
                    ("TextControlButtonBorderBrush", "ControlFillColorTransparentBrush"),
                    ("TextControlButtonBorderBrushPointerOver", "ControlFillColorTransparentBrush"),
                    ("TextControlButtonBorderBrushPressed", "ControlFillColorTransparentBrush"),
                    ("TextControlButtonForeground", "TextFillColorSecondaryBrush"),
                    ("TextControlButtonForegroundPointerOver", "TextFillColorSecondaryBrush"),
                    ("TextControlButtonForegroundPressed", "TextFillColorTertiaryBrush"));
                AssertThemeSolidColorBrushColorReference(themeName, "TextControlForegroundDisabled", "TemporaryTextFillColorDisabled");
                AssertTextControlMetricResources(themeName, new Thickness(1, 1, 1, 2));
            }

            AssertThemeResourceReferences(
                "HighContrast",
                ("TextControlForeground", "SystemControlForegroundBaseHighBrush"),
                ("TextControlForegroundPointerOver", "SystemControlForegroundBaseHighBrush"),
                ("TextControlForegroundFocused", "SystemControlForegroundBaseHighBrush"),
                ("TextControlForegroundDisabled", "SystemControlDisabledChromeDisabledLowBrush"),
                ("TextControlBackground", "SystemControlBackgroundAltMediumLowBrush"),
                ("TextControlBackgroundPointerOver", "SystemControlBackgroundAltMediumBrush"),
                ("TextControlBackgroundFocused", "SystemControlBackgroundAltHighBrush"),
                ("TextControlBackgroundDisabled", "SystemControlBackgroundBaseLowBrush"),
                ("TextControlBorderBrush", "SystemControlForegroundBaseMediumBrush"),
                ("TextControlBorderBrushPointerOver", "SystemControlHighlightBaseMediumHighBrush"),
                ("TextControlBorderBrushFocused", "SystemControlHighlightAccentBrush"),
                ("TextControlBorderBrushDisabled", "SystemControlDisabledBaseLowBrush"),
                ("TextControlPlaceholderForeground", "SystemControlPageTextBaseMediumBrush"),
                ("TextControlPlaceholderForegroundPointerOver", "SystemControlPageTextBaseMediumBrush"),
                ("TextControlPlaceholderForegroundFocused", "SystemControlForegroundBaseMediumLowBrush"),
                ("TextControlPlaceholderForegroundDisabled", "SystemControlDisabledChromeDisabledLowBrush"),
                ("TextControlHeaderForeground", "SystemControlForegroundBaseHighBrush"),
                ("TextControlHeaderForegroundDisabled", "SystemControlDisabledBaseMediumLowBrush"),
                ("TextControlSelectionHighlightColor", "SystemControlHighlightAccentBrush"),
                ("TextControlButtonBackground", "SystemControlTransparentBrush"),
                ("TextControlButtonBackgroundPointerOver", "SystemControlTransparentBrush"),
                ("TextControlButtonBackgroundPressed", "SystemControlHighlightAccentBrush"),
                ("TextControlButtonBorderBrush", "SystemControlTransparentBrush"),
                ("TextControlButtonBorderBrushPointerOver", "SystemControlTransparentBrush"),
                ("TextControlButtonBorderBrushPressed", "SystemControlTransparentBrush"),
                ("TextControlButtonForeground", "SystemControlForegroundBaseMediumHighBrush"),
                ("TextControlButtonForegroundPointerOver", "SystemControlHighlightAccentBrush"),
                ("TextControlButtonForegroundPressed", "SystemControlHighlightAltChromeWhiteBrush"));
            AssertTextControlMetricResources("HighContrast", new Thickness(2));
        });
    }

    private static void AssertTextBoxStyleSetters(TextBox textBox)
    {
        Assert.IsNull(textBox.FocusVisualStyle);
        Assert.IsInstanceOfType(Validation.GetErrorTemplate(textBox), typeof(ControlTemplate));
        Assert.AreSame(textBox.TryFindResource("TextControlForeground"), textBox.Foreground);
        Assert.AreSame(textBox.TryFindResource("TextControlForeground"), textBox.CaretBrush);
        Assert.AreSame(textBox.TryFindResource("TextControlBackground"), textBox.Background);
        Assert.AreSame(textBox.TryFindResource("TextControlElevationBorderBrush"), textBox.BorderBrush);
        Assert.AreEqual((Thickness)textBox.TryFindResource("TextControlBorderThemeThickness"), textBox.BorderThickness);
        Assert.AreEqual(ScrollBarVisibility.Hidden, ScrollViewer.GetHorizontalScrollBarVisibility(textBox));
        Assert.AreEqual(ScrollBarVisibility.Hidden, ScrollViewer.GetVerticalScrollBarVisibility(textBox));
        Assert.AreEqual(HorizontalAlignment.Left, textBox.HorizontalContentAlignment);
        Assert.AreEqual(VerticalAlignment.Top, textBox.VerticalContentAlignment);
        Assert.AreEqual((double)textBox.TryFindResource("TextControlThemeMinHeight"), textBox.MinHeight);
        Assert.AreEqual((double)textBox.TryFindResource("TextControlThemeMinWidth"), textBox.MinWidth);
        Assert.AreEqual((Thickness)textBox.TryFindResource("TextControlThemePadding"), textBox.Padding);
        Assert.AreEqual((CornerRadius)textBox.TryFindResource("ControlCornerRadius"), ((CornerRadius)textBox.GetValue(System.Windows.Controls.Border.CornerRadiusProperty)));
        Assert.IsTrue(textBox.OverridesDefaultStyle);
        Assert.AreEqual(Cursors.IBeam, textBox.Cursor);
        Assert.IsTrue(textBox.AllowDrop);
        Assert.AreEqual(PanningMode.VerticalFirst, ScrollViewer.GetPanningMode(textBox));
        Assert.IsFalse(Stylus.GetIsFlicksEnabled(textBox));
        Assert.AreSame(textBox.TryFindResource("TextControlSelectionHighlightColor"), textBox.SelectionBrush);
        Assert.IsTrue(TextContextMenu.GetUsingTextContextMenu(textBox));
        Assert.AreEqual(new Thickness(0, 0, 0, 8), (Thickness)textBox.TryFindResource("TextBoxTopHeaderMargin"));
    }

    private static void AssertTextBoxStyleSetters(Style style)
    {
        var setters = style.Setters.OfType<Setter>().ToArray();
        AssertSetter(setters, Control.FocusVisualStyleProperty, null);
        AssertDynamicResourceSetter(setters, Validation.ErrorTemplateProperty, "TextControlValidationErrorTemplate");
        AssertDynamicResourceSetter(setters, Control.ContextMenuProperty, "TextControlContextMenu");
        AssertDynamicResourceSetter(setters, Control.ForegroundProperty, "TextControlForeground");
        AssertDynamicResourceSetter(setters, TextBoxBase.CaretBrushProperty, "TextControlForeground");
        AssertDynamicResourceSetter(setters, Control.BackgroundProperty, "TextControlBackground");
        AssertDynamicResourceSetter(setters, Control.BorderBrushProperty, "TextControlElevationBorderBrush");
        AssertDynamicResourceSetter(setters, Control.BorderThicknessProperty, "TextControlBorderThemeThickness");
        AssertSetter(setters, ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Hidden);
        AssertSetter(setters, ScrollViewer.VerticalScrollBarVisibilityProperty, ScrollBarVisibility.Hidden);
        AssertSetter(setters, Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Left);
        AssertSetter(setters, Control.VerticalContentAlignmentProperty, VerticalAlignment.Top);
        AssertDynamicResourceSetter(setters, FrameworkElement.MinHeightProperty, "TextControlThemeMinHeight");
        AssertDynamicResourceSetter(setters, FrameworkElement.MinWidthProperty, "TextControlThemeMinWidth");
        AssertDynamicResourceSetter(setters, Control.PaddingProperty, "TextControlThemePadding");
        AssertDynamicResourceSetter(setters, System.Windows.Controls.Border.CornerRadiusProperty, "ControlCornerRadius");
        AssertSetter(setters, Control.OverridesDefaultStyleProperty, true);
        AssertSetter(setters, FrameworkElement.CursorProperty, Cursors.IBeam);
        AssertSetter(setters, UIElement.AllowDropProperty, true);
        AssertSetter(setters, ScrollViewer.PanningModeProperty, PanningMode.VerticalFirst);
        AssertSetter(setters, Stylus.IsFlicksEnabledProperty, false);
        AssertDynamicResourceSetter(setters, TextBoxBase.SelectionBrushProperty, "TextControlSelectionHighlightColor");
        AssertSetter(setters, TextContextMenu.UsingTextContextMenuProperty, true);
        Assert.AreSame(Application.Current.FindResource("DefaultTextBoxControlTemplate"), setters.Single(item => item.Property == Control.TemplateProperty).Value);
    }

    private static void AssertPasswordBoxStyleSetters(PasswordBox passwordBox)
    {
        Assert.IsNull(passwordBox.FocusVisualStyle);
        Assert.AreSame(passwordBox.TryFindResource("TextControlForeground"), passwordBox.Foreground);
        Assert.AreSame(passwordBox.TryFindResource("TextControlForeground"), passwordBox.CaretBrush);
        Assert.AreSame(passwordBox.TryFindResource("TextControlBackground"), passwordBox.Background);
        Assert.AreSame(passwordBox.TryFindResource("TextControlBorderBrush"), passwordBox.BorderBrush);
        Assert.AreEqual((Thickness)passwordBox.TryFindResource("PasswordBoxBorderThemeThickness"), passwordBox.BorderThickness);
        Assert.AreEqual(ScrollBarVisibility.Hidden, ScrollViewer.GetHorizontalScrollBarVisibility(passwordBox));
        Assert.AreEqual(ScrollBarVisibility.Hidden, ScrollViewer.GetVerticalScrollBarVisibility(passwordBox));
        Assert.AreEqual(HorizontalAlignment.Left, passwordBox.HorizontalContentAlignment);
        Assert.AreEqual(VerticalAlignment.Top, passwordBox.VerticalContentAlignment);
        Assert.AreEqual(Cursors.IBeam, passwordBox.Cursor);
        Assert.AreEqual((double)passwordBox.TryFindResource("TextControlThemeMinHeight"), passwordBox.MinHeight);
        Assert.AreEqual((double)passwordBox.TryFindResource("TextControlThemeMinWidth"), passwordBox.MinWidth);
        Assert.AreEqual((Thickness)passwordBox.TryFindResource("TextControlThemePadding"), passwordBox.Padding);
        Assert.AreEqual((CornerRadius)passwordBox.TryFindResource("ControlCornerRadius"), ((CornerRadius)passwordBox.GetValue(System.Windows.Controls.Border.CornerRadiusProperty)));
        Assert.IsTrue(passwordBox.AllowDrop);
        Assert.AreEqual(PanningMode.VerticalFirst, ScrollViewer.GetPanningMode(passwordBox));
        Assert.AreEqual('\u25CF', passwordBox.PasswordChar);
        Assert.IsFalse(Stylus.GetIsFlicksEnabled(passwordBox));
        Assert.IsTrue(passwordBox.OverridesDefaultStyle);
        Assert.AreSame(passwordBox.TryFindResource("TextControlSelectionHighlightColor"), passwordBox.SelectionBrush);
        Assert.IsTrue(TextContextMenu.GetUsingTextContextMenu(passwordBox));
        Assert.AreEqual(new Thickness(0, 0, 0, 8), (Thickness)passwordBox.TryFindResource("PasswordBoxTopHeaderMargin"));
    }

    private static void AssertPasswordBoxStyleSetters(Style style)
    {
        var setters = style.Setters.OfType<Setter>().ToArray();
        AssertSetter(setters, Control.FocusVisualStyleProperty, null);
        AssertDynamicResourceSetter(setters, Control.ContextMenuProperty, "TextControlContextMenu");
        AssertDynamicResourceSetter(setters, Control.ForegroundProperty, "TextControlForeground");
        AssertDynamicResourceSetter(setters, PasswordBox.CaretBrushProperty, "TextControlForeground");
        AssertDynamicResourceSetter(setters, Control.BackgroundProperty, "TextControlBackground");
        AssertDynamicResourceSetter(setters, Control.BorderBrushProperty, "TextControlBorderBrush");
        AssertSetter(setters, Control.BorderThicknessProperty, Application.Current.FindResource("PasswordBoxBorderThemeThickness"));
        AssertSetter(setters, ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Hidden);
        AssertSetter(setters, ScrollViewer.VerticalScrollBarVisibilityProperty, ScrollBarVisibility.Hidden);
        AssertSetter(setters, Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Left);
        AssertSetter(setters, Control.VerticalContentAlignmentProperty, VerticalAlignment.Top);
        AssertSetter(setters, FrameworkElement.CursorProperty, Cursors.IBeam);
        AssertDynamicResourceSetter(setters, FrameworkElement.MinHeightProperty, "TextControlThemeMinHeight");
        AssertDynamicResourceSetter(setters, FrameworkElement.MinWidthProperty, "TextControlThemeMinWidth");
        AssertDynamicResourceSetter(setters, Control.PaddingProperty, "TextControlThemePadding");
        AssertDynamicResourceSetter(setters, System.Windows.Controls.Border.CornerRadiusProperty, "ControlCornerRadius");
        AssertSetter(setters, UIElement.AllowDropProperty, true);
        AssertSetter(setters, ScrollViewer.PanningModeProperty, PanningMode.VerticalFirst);
        AssertSetter(setters, PasswordBox.PasswordCharProperty, '\u25CF');
        AssertSetter(setters, Stylus.IsFlicksEnabledProperty, false);
        AssertSetter(setters, Control.OverridesDefaultStyleProperty, true);
        AssertDynamicResourceSetter(setters, PasswordBox.SelectionBrushProperty, "TextControlSelectionHighlightColor");
        AssertSetter(setters, TextContextMenu.UsingTextContextMenuProperty, true);
        Assert.IsInstanceOfType(setters.Single(item => item.Property == Control.TemplateProperty).Value, typeof(ControlTemplate));
    }

    private static void AssertTextBoxTemplateShape(TextBox textBox)
    {
        var contentBorder = GetTemplateChild<Border>(textBox, "ContentBorder");
        var contentHost = GetTemplateChild<ScrollViewer>(textBox, "PART_ContentHost");
        var deleteButton = GetTemplateChild<Button>(textBox, "DeleteButton");

        Assert.AreSame(textBox.Background, contentBorder.Background);
        Assert.AreSame(textBox.BorderBrush, contentBorder.BorderBrush);
        Assert.AreEqual(textBox.BorderThickness, contentBorder.BorderThickness);
        Assert.AreEqual(textBox.MinHeight, contentBorder.MinHeight);
        Assert.AreEqual(((CornerRadius)textBox.GetValue(System.Windows.Controls.Border.CornerRadiusProperty)), contentBorder.CornerRadius);
        Assert.IsTrue(ValidationHelper.GetIsTemplateValidationAdornerSite(contentBorder));

        Assert.AreEqual(textBox.BorderThickness, contentHost.Margin);
        Assert.AreEqual(textBox.Padding, contentHost.Padding);
        Assert.AreEqual(ScrollViewer.GetHorizontalScrollBarVisibility(textBox), contentHost.HorizontalScrollBarVisibility);
        Assert.AreEqual(ScrollViewer.GetVerticalScrollBarVisibility(textBox), contentHost.VerticalScrollBarVisibility);
        Assert.AreSame(textBox.Foreground, TextElement.GetForeground(contentHost));

        Assert.IsTrue(TextBoxHelper.GetIsDeleteButton(deleteButton));
        Assert.IsNull(textBox.Template.FindName("HeaderContentPresenter", textBox));
        Assert.IsNull(textBox.Template.FindName("DescriptionPresenter", textBox));
        Assert.IsNull(textBox.Template.FindName("PlaceholderTextContentPresenter", textBox));
        Assert.IsNull(VisualTreeTestHelper.FindDescendant<ContentPresenterEx>(textBox));
    }

    private static void AssertPasswordBoxTemplateShape(PasswordBox passwordBox)
    {
        var contentBorder = GetTemplateChild<Border>(passwordBox, "ContentBorder");
        var contentHost = GetTemplateChild<ScrollViewer>(passwordBox, "PART_ContentHost");

        Assert.AreSame(passwordBox.Background, contentBorder.Background);
        Assert.AreSame(passwordBox.BorderBrush, contentBorder.BorderBrush);
        Assert.AreEqual(passwordBox.BorderThickness, contentBorder.BorderThickness);
        Assert.AreEqual(passwordBox.MinWidth, contentBorder.MinWidth);
        Assert.AreEqual(passwordBox.MinHeight, contentBorder.MinHeight);
        Assert.AreEqual(((CornerRadius)passwordBox.GetValue(System.Windows.Controls.Border.CornerRadiusProperty)), contentBorder.CornerRadius);

        Assert.AreEqual(passwordBox.BorderThickness, contentHost.Margin);
        Assert.AreEqual(passwordBox.Padding, contentHost.Padding);
        Assert.AreEqual(ScrollViewer.GetHorizontalScrollBarVisibility(passwordBox), contentHost.HorizontalScrollBarVisibility);
        Assert.AreEqual(ScrollBarVisibility.Disabled, contentHost.VerticalScrollBarVisibility);
        Assert.AreSame(passwordBox.Foreground, TextElement.GetForeground(contentHost));

        Assert.IsNull(passwordBox.Template.FindName("HeaderContentPresenter", passwordBox));
        Assert.IsNull(passwordBox.Template.FindName("DescriptionPresenter", passwordBox));
        Assert.IsNull(passwordBox.Template.FindName("PlaceholderTextContentPresenter", passwordBox));
        Assert.IsNull(passwordBox.Template.FindName("RevealButton", passwordBox));
        Assert.IsNull(VisualTreeTestHelper.FindDescendant<ContentPresenterEx>(passwordBox));
    }

    private static void AssertDisabledTextControlTemplateResources(Control control)
    {
        var contentBorder = GetTemplateChild<Border>(control, "ContentBorder");
        var contentHost = GetTemplateChild<ScrollViewer>(control, "PART_ContentHost");

        control.IsEnabled = false;
        control.UpdateLayout();

        Assert.AreSame(contentBorder.TryFindResource("TextControlBackgroundDisabled"), contentBorder.Background);
        Assert.AreSame(contentBorder.TryFindResource("TextControlBorderBrushDisabled"), contentBorder.BorderBrush);
        Assert.AreSame(contentHost.TryFindResource("TextControlForegroundDisabled"), TextElement.GetForeground(contentHost));
    }

    private static void AssertTextBoxTriggerShape(ControlTemplate template)
    {
        var triggers = template.Triggers.OfType<Trigger>().ToArray();

        AssertTrigger(triggers, "IsMouseOver", true,
            ("ContentBorder", "Background", "TextControlBackgroundPointerOver"),
            ("ContentBorder", "BorderBrush", "TextControlBorderBrushPointerOver"),
            ("PART_ContentHost", "Foreground", "TextControlForegroundPointerOver"));

        AssertTrigger(triggers, "IsFocused", true,
            ("ContentBorder", "BorderThickness", "TextControlBorderThemeThicknessFocused"),
            ("ContentBorder", "Background", "TextControlBackgroundFocused"),
            ("ContentBorder", "BorderBrush", "TextControlBorderBrushFocused"),
            ("PART_ContentHost", "Foreground", "TextControlForegroundFocused"));

        AssertTrigger(triggers, "IsEnabled", false,
            ("ContentBorder", "Background", "TextControlBackgroundDisabled"),
            ("ContentBorder", "BorderBrush", "TextControlBorderBrushDisabled"),
            ("PART_ContentHost", "Foreground", "TextControlForegroundDisabled"));

        Assert.IsTrue(triggers.Any(item => item.Property == TextBox.TextProperty && Equals(item.Value, string.Empty)));
        Assert.IsTrue(triggers.Any(item => item.Property == TextBoxBase.IsReadOnlyProperty && Equals(item.Value, true)));
        Assert.IsTrue(triggers.Any(item => item.Property == TextBox.AcceptsReturnProperty && Equals(item.Value, true)));
        Assert.IsTrue(triggers.Any(item => item.Property == TextBox.TextWrappingProperty && Equals(item.Value, TextWrapping.Wrap)));
        Assert.IsTrue(triggers.Any(item => item.Property == TextBox.TextWrappingProperty && Equals(item.Value, TextWrapping.WrapWithOverflow)));
    }

    private static void AssertTextControlTriggerShape(ControlTemplate template)
    {
        var triggers = template.Triggers.OfType<Trigger>().ToArray();
        Assert.AreEqual(3, triggers.Length);

        AssertTrigger(triggers, "IsMouseOver", true,
            ("ContentBorder", "Background", "TextControlBackgroundPointerOver"),
            ("ContentBorder", "BorderBrush", "TextControlBorderBrushPointerOver"),
            ("PART_ContentHost", "Foreground", "TextControlForegroundPointerOver"));

        AssertTrigger(triggers, "IsFocused", true,
            ("ContentBorder", "BorderThickness", "TextControlBorderThemeThicknessFocused"),
            ("ContentBorder", "Background", "TextControlBackgroundFocused"),
            ("ContentBorder", "BorderBrush", "TextControlBorderBrushFocused"),
            ("PART_ContentHost", "Foreground", "TextControlForegroundFocused"));

        AssertTrigger(triggers, "IsEnabled", false,
            ("ContentBorder", "Background", "TextControlBackgroundDisabled"),
            ("ContentBorder", "BorderBrush", "TextControlBorderBrushDisabled"),
            ("PART_ContentHost", "Foreground", "TextControlForegroundDisabled"));
    }

    private static void AssertTextBoxClearButtonSubstitutionClearsText(TextBox textBox)
    {
        var deleteButton = GetTemplateChild<Button>(textBox, "DeleteButton");
        Assert.AreEqual("Text value", textBox.Text);

        deleteButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, deleteButton));

        Assert.AreEqual(string.Empty, textBox.Text);
    }

    private static void AssertTrigger(
        Trigger[] triggers,
        string propertyName,
        object value,
        params (string TargetName, string PropertyName, string ResourceKey)[] expectedSetters)
    {
        var trigger = triggers.Single(item => item.Property.Name == propertyName && Equals(item.Value, value));
        var setters = trigger.Setters.OfType<Setter>().ToArray();

        foreach (var expectedSetter in expectedSetters)
        {
            AssertSetter(setters, expectedSetter.TargetName, expectedSetter.PropertyName, expectedSetter.ResourceKey);
        }
    }

    private static void AssertSetter(Setter[] setters, string targetName, string propertyName, string resourceKey)
    {
        var setter = setters.Single(item =>
            item.TargetName == targetName &&
            item.Property.Name == propertyName);

        Assert.IsInstanceOfType(setter.Value, typeof(DynamicResourceExtension));
        var resource = (DynamicResourceExtension)setter.Value;
        Assert.AreEqual(resourceKey, resource.ResourceKey);
    }

    private static void AssertSetter(Setter[] setters, DependencyProperty property, object? value)
    {
        var setter = setters.Single(item => item.Property == property);
        Assert.AreEqual(value, setter.Value);
    }

    private static void AssertDynamicResourceSetter(Setter[] setters, DependencyProperty property, object resourceKey)
    {
        var setter = setters.Single(item => item.Property == property);
        Assert.IsInstanceOfType(setter.Value, typeof(DynamicResourceExtension));
        var dynamicResource = (DynamicResourceExtension)setter.Value;
        Assert.AreEqual(resourceKey, dynamicResource.ResourceKey);
    }

    private static void AssertTextControlMetricResources(string themeName, Thickness focusedBorderThickness)
    {
        AssertThemeResourceValue(themeName, "TextControlBorderThemeThickness", new Thickness(1));
        AssertThemeResourceValue(themeName, "TextControlBorderThemeThicknessFocused", focusedBorderThickness);
        AssertThemeResourceValue(themeName, "TextControlThemePadding", new Thickness(10, 5, 6, 6));
        AssertThemeResourceValue(themeName, "TextControlThemeMinWidth", 64.0);
        AssertThemeResourceValue(themeName, "TextControlMarginThemeThickness", new Thickness(0, 9.5, 0, 9.5));
        AssertThemeResourceValue(themeName, "HelperButtonThemePadding", new Thickness(0, 0, -2, 0));
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

    private static void AssertThemeResourceReference(string themeName, string resourceKey, string expectedResourceKey)
    {
        var themeDictionary = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(themeDictionary.Contains(resourceKey), $"{themeName} is missing {resourceKey}.");
        Assert.IsTrue(themeDictionary.Contains(expectedResourceKey), $"{themeName} is missing {expectedResourceKey}.");
        Assert.AreSame(themeDictionary[expectedResourceKey], themeDictionary[resourceKey], $"{themeName}:{resourceKey}");
    }

    private static void AssertThemeResourceValue(string themeName, string resourceKey, object expectedValue)
    {
        var themeDictionary = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(themeDictionary.Contains(resourceKey), $"{themeName} is missing {resourceKey}.");
        Assert.AreEqual(expectedValue, themeDictionary[resourceKey], $"{themeName}:{resourceKey}");
    }

    private static void AssertThemeSolidColorBrushColorReference(string themeName, string resourceKey, string expectedColorResourceKey)
    {
        var themeDictionary = ThemeResources.Current.GetThemeDictionary(themeName);

        Assert.IsTrue(themeDictionary.Contains(resourceKey), $"{themeName} is missing {resourceKey}.");
        Assert.IsTrue(themeDictionary.Contains(expectedColorResourceKey), $"{themeName} is missing {expectedColorResourceKey}.");
        Assert.IsInstanceOfType(themeDictionary[resourceKey], typeof(SolidColorBrush), $"{themeName}:{resourceKey}");
        Assert.AreEqual(
            themeDictionary[expectedColorResourceKey],
            ((SolidColorBrush)themeDictionary[resourceKey]).Color,
            $"{themeName}:{resourceKey}");
    }

    private static T GetTemplateChild<T>(Control control, string name)
        where T : DependencyObject
    {
        return control.Template.FindName(name, control) as T
            ?? throw new AssertFailedException($"Expected {control.GetType().Name} template child '{name}' to be a {typeof(T).Name}.");
    }

    private static void AssertInitialTextBoxValidationErrorClears(ControlTemplate? localErrorTemplate = null)
    {
        var model = new RequiredTextModel();
        var textBox = new TextBox
        {
            DataContext = model,
            Width = 240
        };
        if (localErrorTemplate != null)
        {
            Validation.SetErrorTemplate(textBox, localErrorTemplate);
        }

        textBox.SetBinding(
            TextBox.TextProperty,
            new Binding(nameof(RequiredTextModel.Value))
            {
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                ValidatesOnDataErrors = true
            });
        WpfTestHost.DoEvents();

        Assert.IsTrue(
            Validation.GetHasError(textBox),
            "The binding must be invalid before the TextBox template is applied.");

        using var host = new TestWindowHost(textBox, width: 320, height: 120);
        host.UpdateLayout();

        var contentBorder = GetTemplateChild<Border>(textBox, "ContentBorder");
        var adornerLayer = AdornerLayer.GetAdornerLayer(textBox)
            ?? throw new AssertFailedException("Expected the hosted TextBox to have an AdornerLayer.");
        Assert.AreSame(
            localErrorTemplate ?? textBox.TryFindResource("TextControlValidationErrorTemplate"),
            Validation.GetErrorTemplate(textBox),
            "Redirecting the validation site must preserve the effective error template.");
        Assert.IsTrue(
            GetValidationAdornerCount(adornerLayer, textBox, contentBorder) > 0,
            "Expected the initial validation error to display an adorner.");

        textBox.Text = "valid";
        WpfTestHost.DoEvents();
        host.UpdateLayout();

        Assert.IsFalse(Validation.GetHasError(textBox));
        Assert.AreEqual(
            0,
            GetValidationAdornerCount(adornerLayer, textBox, contentBorder),
            "The validation adorner must be removed after the initial error is corrected.");
    }

    private static int GetValidationAdornerCount(
        AdornerLayer adornerLayer,
        TextBox textBox,
        Border contentBorder)
    {
        return (adornerLayer.GetAdorners(textBox)?.Length ?? 0)
            + (adornerLayer.GetAdorners(contentBorder)?.Length ?? 0);
    }

    private static string FindRepoRoot()
    {
        var directory = new System.IO.DirectoryInfo(System.AppContext.BaseDirectory);

        while (directory != null)
        {
            if (System.IO.File.Exists(System.IO.Path.Combine(directory.FullName, "ModernWpf.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        Assert.Fail("Could not locate ModernWpf.sln from the test output directory.");
        return string.Empty;
    }

    private sealed class RequiredTextModel : System.ComponentModel.IDataErrorInfo
    {
        public string Value { get; set; } = string.Empty;

        public string Error => string.Empty;

        public string this[string columnName] =>
            columnName == nameof(Value) && string.IsNullOrEmpty(Value)
                ? "Value is required."
                : string.Empty;
    }
}
