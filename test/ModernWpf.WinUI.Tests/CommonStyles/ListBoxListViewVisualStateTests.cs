using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;
using WpfListView = System.Windows.Controls.ListView;
using WpfListViewItem = System.Windows.Controls.ListViewItem;

namespace ModernWpf.WinUI.Tests.CommonStyles;

[TestClass]
public class ListBoxListViewVisualStateTests
{
    [TestMethod]
    public void DefaultListBoxStylesUseOfficialWpfFluentTemplateShape()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var defaultListBoxStyle = (Style)Application.Current.FindResource("DefaultListBoxStyle");
            var implicitListBoxStyle = (Style)Application.Current.FindResource(typeof(ListBox));
            Assert.AreEqual(typeof(ListBox), defaultListBoxStyle.TargetType);
            Assert.AreSame(defaultListBoxStyle, implicitListBoxStyle.BasedOn);

            var defaultItemStyle = (Style)Application.Current.FindResource("DefaultListBoxItemStyle");
            var implicitItemStyle = (Style)Application.Current.FindResource(typeof(ListBoxItem));
            Assert.AreEqual(typeof(ListBoxItem), defaultItemStyle.TargetType);
            Assert.AreSame(defaultItemStyle, implicitItemStyle.BasedOn);

            var itemSetters = defaultItemStyle.Setters.OfType<Setter>().ToArray();
            AssertDynamicResourceSetter(itemSetters, Control.ForegroundProperty, "ListBoxItemForeground");
            AssertDynamicResourceSetter(itemSetters, Control.FocusVisualStyleProperty, "DefaultCollectionFocusVisualStyle");
            AssertSetter(itemSetters, System.Windows.Controls.Border.CornerRadiusProperty, new CornerRadius(0));
            AssertNoSetter(itemSetters, FocusVisualHelper.UseSystemFocusVisualsProperty);

            var item = new ListBoxItem
            {
                Content = "ListBox content",
                IsSelected = true
            };

            using var host = new TestWindowHost(item);
            host.UpdateLayout();

            var presenter = VisualTreeTestHelper.FindDescendant<ContentPresenter>(item)
                ?? throw new AssertFailedException("Expected official WPF Fluent ListBoxItem template to use ContentPresenter.");
            Assert.IsNotInstanceOfType(presenter, typeof(ContentPresenterEx));
            Assert.AreEqual(item.Content, presenter.Content);
            Assert.IsNull(VisualTreeTestHelper.FindDescendant<ContentPresenterEx>(item));

            var border = VisualTreeTestHelper.FindDescendant<Border>(item)
                ?? throw new AssertFailedException("Expected official WPF Fluent ListBoxItem template root Border.");
            Assert.AreSame(item.TryFindResource("ListBoxItemSelectedBackgroundThemeBrush"), border.Background);
            Assert.AreSame(item.TryFindResource("ListBoxItemSelectedForegroundThemeBrush"), item.Foreground);
        });
    }

    [TestMethod]
    public void DefaultListViewStylesUseOfficialWpfFluentTemplateShape()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var defaultListViewStyle = (Style)Application.Current.FindResource("DefaultListViewStyle");
            var implicitListViewStyle = (Style)Application.Current.FindResource(typeof(WpfListView));
            Assert.AreEqual(typeof(WpfListView), defaultListViewStyle.TargetType);
            Assert.AreSame(defaultListViewStyle, implicitListViewStyle.BasedOn);
            Assert.IsInstanceOfType(Application.Current.FindResource("ViewIsGridViewConverter"), typeof(IsGridViewConverter));
            Assert.IsInstanceOfType(Application.Current.FindResource("GridViewTemplate"), typeof(ControlTemplate));

            var defaultItemStyle = (Style)Application.Current.FindResource("DefaultListViewItemStyle");
            var implicitItemStyle = (Style)Application.Current.FindResource(typeof(WpfListViewItem));
            Assert.AreEqual(typeof(WpfListViewItem), defaultItemStyle.TargetType);
            Assert.AreSame(defaultItemStyle, implicitItemStyle.BasedOn);

            var itemSetters = defaultItemStyle.Setters.OfType<Setter>().ToArray();
            AssertDynamicResourceSetter(itemSetters, Control.ForegroundProperty, "ListViewItemForeground");
            AssertDynamicResourceSetter(itemSetters, System.Windows.Controls.Border.CornerRadiusProperty, "ControlCornerRadius");
            AssertDynamicResourceSetter(itemSetters, Control.FocusVisualStyleProperty, "DefaultCollectionFocusVisualStyle");
            AssertSetter(itemSetters, Control.OverridesDefaultStyleProperty, true);
            AssertNoSetter(itemSetters, FocusVisualHelper.UseSystemFocusVisualsProperty);

            var item = new WpfListViewItem
            {
                Content = "ListView content",
                IsSelected = true
            };

            using var host = new TestWindowHost(item);
            host.UpdateLayout();

            var presenter = VisualTreeTestHelper.FindDescendant<ContentPresenter>(item)
                ?? throw new AssertFailedException("Expected official WPF Fluent ListViewItem template to use ContentPresenter.");
            Assert.IsNotInstanceOfType(presenter, typeof(ContentPresenterEx));
            Assert.AreEqual(item.Content, presenter.Content);
            Assert.IsNull(VisualTreeTestHelper.FindDescendant<ContentPresenterEx>(item));

            var activeRectangle = item.Template.FindName("ActiveRectangle", item) as System.Windows.Shapes.Rectangle
                ?? throw new AssertFailedException("Expected official WPF Fluent ListViewItem selection indicator.");
            Assert.AreEqual(Visibility.Visible, activeRectangle.Visibility);
            Assert.AreSame(item.TryFindResource("ListViewItemPillFillBrush"), activeRectangle.Fill);
        });
    }

    [TestMethod]
    public void DefaultGridViewColumnHeaderStyleUsesOfficialWpfFluentTemplateShape()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var defaultHeaderStyle = (Style)Application.Current.FindResource("DefaultGridViewColumnHeaderStyle");
            Assert.AreEqual(typeof(GridViewColumnHeader), defaultHeaderStyle.TargetType);

            var header = new GridViewColumnHeader
            {
                Style = defaultHeaderStyle,
                Content = "Header content"
            };

            using var host = new TestWindowHost(header);
            host.UpdateLayout();

            var presenter = VisualTreeTestHelper.FindDescendant<ContentPresenter>(header)
                ?? throw new AssertFailedException("Expected official WPF Fluent GridViewColumnHeader template to use ContentPresenter.");
            Assert.IsNotInstanceOfType(presenter, typeof(ContentPresenterEx));
            Assert.AreEqual(header.Content, presenter.Content);
            Assert.IsNull(VisualTreeTestHelper.FindDescendant<ContentPresenterEx>(header));
            Assert.IsInstanceOfType(header.Template.FindName("PART_HeaderGripper", header), typeof(Thumb));
        });
    }

    [TestMethod]
    public void ThemeResourcesUseOfficialListBoxListViewHighContrastTokens()
    {
        WpfTestHost.Run(() =>
        {
            AssertDefaultThemeListBoxListViewResources(
                "Light",
                "SystemControlBackgroundChromeMediumLowBrush",
                "AccentFillColorSecondaryBrush",
                selectedBackgroundOpacity: 0.4,
                selectedBackgroundPointerOverOpacity: 0.6);

            AssertDefaultThemeListBoxListViewResources(
                "Dark",
                "CardBackgroundFillColorDefaultBrush",
                "SubtleFillColorTertiaryBrush",
                selectedBackgroundOpacity: 0.6,
                selectedBackgroundPointerOverOpacity: 0.8);

            AssertHighContrastListBoxListViewResources();
        });
    }

    [TestMethod]
    public void ListBoxListViewFilesDeleteWinUIGuessedTemplateBranches()
    {
        var repoRoot = FindRepoRoot();
        var text = string.Join(
            "\n",
            new[] { "ListBox.xaml", "ListBoxItem.xaml", "GridView.xaml", "ListView.xaml", "ListViewItem.xaml" }
                .Select(file => File.ReadAllText(Path.Combine(repoRoot, "ModernWpf", "Styles", file))));

        Assert.IsFalse(text.Contains("VisualStateEx", System.StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("ContentPresenterEx", System.StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("ScrollViewerEx", System.StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("FocusVisualHelper", System.StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("ListViewBaseItemRoundedChromeEnabled", System.StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("PressedBackground", System.StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("Selector.IsSelectionActive", System.StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("ControlHelper.CornerRadius", System.StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("System.Runtime", System.StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("Fluent.Controls", System.StringComparison.Ordinal));
    }

    private static void AssertSetter(Setter[] setters, DependencyProperty property, object value)
    {
        var setter = setters.Single(item => item.Property == property);
        Assert.AreEqual(value, setter.Value);
    }

    private static void AssertDynamicResourceSetter(Setter[] setters, DependencyProperty property, string resourceKey)
    {
        var setter = setters.Single(item => item.Property == property);
        Assert.IsInstanceOfType(setter.Value, typeof(DynamicResourceExtension));
        Assert.AreEqual(resourceKey, ((DynamicResourceExtension)setter.Value).ResourceKey);
    }

    private static void AssertNoSetter(Setter[] setters, DependencyProperty property)
    {
        Assert.IsFalse(setters.Any(item => item.Property == property), $"Unexpected setter for {property.Name}.");
    }

    private static void AssertDefaultThemeListBoxListViewResources(
        string themeName,
        string listBoxBackgroundResourceKey,
        string selectedPointerOverResourceKey,
        double selectedBackgroundOpacity,
        double selectedBackgroundPointerOverOpacity)
    {
        AssertThemeResourceValue(themeName, "ListBoxBorderThemeThickness", new Thickness(0));
        AssertThemeResourceReferences(
            themeName,
            ("ListBoxForeground", "TextFillColorPrimaryBrush"),
            ("ListBoxBackground", listBoxBackgroundResourceKey),
            ("ListBoxBorder", "TextFillColorPrimaryBrush"),
            ("ListBoxItemForeground", "TextFillColorPrimaryBrush"),
            ("ListBoxItemForegroundDisabled", "TextFillColorDisabledBrush"),
            ("ListBoxItemBackgroundPointerOver", "SubtleFillColorSecondaryBrush"),
            ("ListBoxItemBackgroundPressed", "SubtleFillColorTertiaryBrush"),
            ("ListBoxItemBackgroundSelected", "SystemControlHighlightListAccentLowBrush"),
            ("ListBoxItemBackgroundSelectedPointerOver", "SystemControlHighlightListAccentMediumBrush"),
            ("ListBoxItemBackgroundSelectedPressed", "SystemControlHighlightListAccentHighBrush"),
            ("ListBoxItemSelectedForegroundThemeBrush", "TextFillColorPrimaryBrush"),
            ("ListBoxItemUnselectedBackgroundPointerOverThemeBrush", "ControlAltFillColorTertiaryBrush"));
        AssertThemeSolidColorBrushOpacity(themeName, "ListBoxItemSelectedBackgroundThemeBrush", selectedBackgroundOpacity);
        AssertThemeSolidColorBrushOpacity(themeName, "ListBoxItemSelectedBackgroundPointerOverThemeBrush", selectedBackgroundPointerOverOpacity);

        AssertThemeResourceValue(themeName, "ListViewHeaderItemMinHeight", 44.0);
        AssertThemeResourceValue(themeName, "GridViewHeaderItemMinHeight", 44.0);
        AssertThemeResourceValue(themeName, "ListViewHeaderItemThemeFontSize", 20.0);
        AssertThemeResourceValue(themeName, "GridViewHeaderItemThemeFontSize", 20.0);
        AssertThemeResourceReferences(
            themeName,
            ("ListViewHeaderItemBackground", "SystemControlTransparentBrush"),
            ("GridViewHeaderItemBackground", "SystemControlTransparentBrush"),
            ("ListViewHeaderItemDividerStroke", "SystemControlForegroundBaseLowBrush"),
            ("GridViewHeaderItemDividerStroke", "SystemControlForegroundBaseLowBrush"));

        AssertThemeResourceValue(themeName, "ListViewItemContentOffsetX", -40.5);
        AssertThemeResourceValue(themeName, "ListViewItemDisabledThemeOpacity", 0.55);
        AssertThemeResourceValue(themeName, "ListViewItemDragThemeOpacity", 0.80);
        AssertThemeResourceValue(themeName, "ListViewItemReorderThemeOpacity", 0.80);
        AssertThemeResourceValue(themeName, "ListViewItemReorderTargetThemeOpacity", 0.50);
        AssertThemeResourceValue(themeName, "ListViewItemReorderTargetThemeScale", 0.95);
        AssertThemeResourceValue(themeName, "ListViewItemReorderHintThemeOffset", 10.0);
        AssertThemeResourceValue(themeName, "ListViewItemSelectedBorderThemeThickness", 4.0);
        AssertThemeResourceValue(themeName, "ListViewItemSelectionCheckMarkVisualEnabled", true);
        AssertThemeResourceReferences(
            themeName,
            ("ListViewItemPillFillBrush", "AccentFillColorDefaultBrush"),
            ("ListViewItemBorderBackground", "SubtleFillColorTertiaryBrush"),
            ("ListViewBackground", "SubtleFillColorTransparentBrush"),
            ("ListViewBorderBrush", "SubtleFillColorTransparentBrush"),
            ("GridViewColumnHeaderBackground", "SubtleFillColorTransparentBrush"),
            ("GridViewColumnHeaderBackgroundPointerOver", "SubtleFillColorSecondaryBrush"),
            ("GridViewColumnHeaderBackgroundPressed", "SubtleFillColorTertiaryBrush"),
            ("GridViewColumnHeaderBackgroundDisabled", "SubtleFillColorDisabledBrush"),
            ("GridViewColumnHeaderBorderBrush", "SubtleFillColorTransparentBrush"),
            ("GridViewColumnHeaderBorderBrushPointerOver", "SubtleFillColorTransparentBrush"),
            ("GridViewColumnHeaderBorderBrushPressed", "SubtleFillColorTransparentBrush"),
            ("GridViewColumnHeaderBorderBrushDisabled", "SubtleFillColorTransparentBrush"),
            ("GridViewColumnHeaderForeground", "TextFillColorPrimaryBrush"),
            ("GridViewColumnHeaderForegroundDisabled", "TextFillColorDisabledBrush"),
            ("GridViewColumnHeaderGripperThumbFill", "ControlStrongFillColorDefaultBrush"),
            ("ListViewItemBackground", "SubtleFillColorTransparentBrush"),
            ("ListViewItemBackgroundPointerOver", "SubtleFillColorSecondaryBrush"),
            ("ListViewItemBackgroundPressed", "SubtleFillColorTertiaryBrush"),
            ("ListViewItemBackgroundSelected", "SubtleFillColorSecondaryBrush"),
            ("ListViewItemBackgroundSelectedPointerOver", selectedPointerOverResourceKey),
            ("ListViewItemBackgroundSelectedPressed", "SubtleFillColorSecondaryBrush"),
            ("ListViewItemBackgroundSelectedDisabled", "SubtleFillColorSecondaryBrush"),
            ("ListViewItemForeground", "TextFillColorPrimaryBrush"),
            ("ListViewItemForegroundPointerOver", "TextFillColorPrimaryBrush"),
            ("ListViewItemForegroundPressed", "TextFillColorPrimaryBrush"),
            ("ListViewItemForegroundSelected", "TextFillColorPrimaryBrush"),
            ("ListViewItemForegroundSelectedPressed", "TextFillColorPrimaryBrush"),
            ("ListViewItemForegroundSelectedPointerOver", "TextFillColorPrimaryBrush"),
            ("ListViewItemFocusVisualPrimaryBrush", "FocusStrokeColorOuterBrush"),
            ("ListViewItemFocusVisualSecondaryBrush", "FocusStrokeColorInnerBrush"),
            ("ListViewItemFocusBorderBrush", "FocusStrokeColorOuterBrush"),
            ("ListViewItemFocusSecondaryBorderBrush", "TextFillColorPrimaryBrush"),
            ("ListViewItemCheckBrush", "TextFillColorSecondaryBrush"),
            ("ListViewItemCheckBoxBrush", "TextFillColorSecondaryBrush"),
            ("ListViewItemDragBackground", "SubtleFillColorTransparentBrush"),
            ("ListViewItemDragForeground", "TextOnAccentFillColorPrimaryBrush"),
            ("ListViewItemPlaceholderBackground", "ControlStrongFillColorDisabledBrush"),
            ("ListViewItemMultiArrangeOverlayTextBorder", "TextOnAccentFillColorPrimaryBrush"),
            ("ListViewItemMultiArrangeOverlayTextBackground", "AccentFillColorDefaultBrush"));

        AssertThemeResourceReferences(
            themeName,
            ("GridViewItemBackground", "SubtleFillColorTransparentBrush"),
            ("GridViewItemBackgroundPointerOver", "SubtleFillColorSecondaryBrush"),
            ("GridViewItemBackgroundPressed", "SubtleFillColorTertiaryBrush"),
            ("GridViewItemBackgroundSelected", "SubtleFillColorSecondaryBrush"),
            ("GridViewItemBackgroundSelectedPointerOver", selectedPointerOverResourceKey),
            ("GridViewItemBackgroundSelectedPressed", "SubtleFillColorSecondaryBrush"),
            ("GridViewItemBackgroundSelectedDisabled", "SubtleFillColorSecondaryBrush"),
            ("GridViewItemForeground", "TextFillColorPrimaryBrush"),
            ("GridViewItemForegroundPointerOver", "TextFillColorPrimaryBrush"),
            ("GridViewItemForegroundSelected", "TextFillColorPrimaryBrush"),
            ("GridViewItemFocusVisualPrimaryBrush", "FocusStrokeColorOuterBrush"),
            ("GridViewItemFocusVisualSecondaryBrush", "FocusStrokeColorInnerBrush"),
            ("GridViewItemFocusBorderBrush", "FocusStrokeColorOuterBrush"),
            ("GridViewItemFocusSecondaryBorderBrush", "TextFillColorPrimaryBrush"),
            ("GridViewItemCheckBrush", "TextFillColorSecondaryBrush"),
            ("GridViewItemCheckBoxBrush", "TextFillColorSecondaryBrush"),
            ("GridViewItemDragBackground", "SubtleFillColorTransparentBrush"),
            ("GridViewItemDragForeground", "TextOnAccentFillColorPrimaryBrush"),
            ("GridViewItemPlaceholderBackground", "ControlStrongFillColorDisabledBrush"));
    }

    private static void AssertHighContrastListBoxListViewResources()
    {
        AssertThemeResourceValue("HighContrast", "ListBoxBorderThemeThickness", new Thickness(2));
        AssertThemeResourceReferences(
            "HighContrast",
            ("ListBoxForeground", "SystemControlForegroundBaseHighBrush"),
            ("ListBoxBackground", "SystemControlBackgroundChromeMediumLowBrush"),
            ("ListBoxBorder", "SystemControlForegroundBaseHighBrush"),
            ("ListBoxItemForeground", "SystemControlForegroundBaseHighBrush"),
            ("ListBoxItemForegroundDisabled", "SystemControlDisabledBaseMediumLowBrush"),
            ("ListBoxItemBackgroundPointerOver", "SystemControlHighlightListLowBrush"),
            ("ListBoxItemBackgroundPressed", "SystemControlHighlightListMediumBrush"),
            ("ListBoxItemBackgroundSelected", "SystemControlHighlightListAccentMediumLowBrush"),
            ("ListBoxItemBackgroundSelectedPointerOver", "SystemControlHighlightListAccentLowBrush"),
            ("ListBoxItemBackgroundSelectedPressed", "SystemControlHighlightListAccentVeryHighBrush"));
        AssertThemeSolidColorBrushReference("HighContrast", "ListBoxItemSelectedBackgroundThemeBrush", "SystemColorHighlightColorBrush");
        AssertThemeSolidColorBrushReference("HighContrast", "ListBoxItemSelectedBackgroundPointerOverThemeBrush", "SystemColorButtonTextColorBrush");
        AssertThemeSolidColorBrushReference("HighContrast", "ListBoxItemSelectedForegroundThemeBrush", "SystemColorButtonFaceColorBrush");
        AssertThemeSolidColorBrushReference("HighContrast", "ListBoxItemUnselectedBackgroundPointerOverThemeBrush", "SystemColorHighlightTextColorBrush");

        AssertThemeResourceValue("HighContrast", "ListViewHeaderItemMinHeight", 44.0);
        AssertThemeResourceValue("HighContrast", "GridViewHeaderItemMinHeight", 44.0);
        AssertThemeResourceValue("HighContrast", "ListViewHeaderItemThemeFontSize", 20.0);
        AssertThemeResourceValue("HighContrast", "GridViewHeaderItemThemeFontSize", 20.0);
        AssertThemeResourceReferences(
            "HighContrast",
            ("ListViewHeaderItemBackground", "SystemControlTransparentBrush"),
            ("GridViewHeaderItemBackground", "SystemControlTransparentBrush"),
            ("ListViewHeaderItemDividerStroke", "SystemControlForegroundBaseLowBrush"),
            ("GridViewHeaderItemDividerStroke", "SystemControlForegroundBaseLowBrush"),
            ("ListViewItemPillFillBrush", "SystemControlHighlightListAccentLowBrush"));

        AssertThemeResourceValue("HighContrast", "ListViewItemContentOffsetX", -40.5);
        AssertThemeResourceValue("HighContrast", "ListViewItemDisabledThemeOpacity", 0.55);
        AssertThemeResourceValue("HighContrast", "ListViewItemDragThemeOpacity", 0.80);
        AssertThemeResourceValue("HighContrast", "ListViewItemReorderThemeOpacity", 0.80);
        AssertThemeResourceValue("HighContrast", "ListViewItemReorderTargetThemeOpacity", 0.50);
        AssertThemeResourceValue("HighContrast", "ListViewItemReorderTargetThemeScale", 0.95);
        AssertThemeResourceValue("HighContrast", "ListViewItemReorderHintThemeOffset", 10.0);
        AssertThemeResourceValue("HighContrast", "ListViewItemSelectedBorderThemeThickness", 4.0);
        AssertThemeResourceValue("HighContrast", "ListViewItemSelectionCheckMarkVisualEnabled", true);
        AssertThemeSolidColorBrushReference("HighContrast", "ListViewItemBorderBackground", "SystemColorButtonFaceColorBrush");
        AssertThemeSolidColorBrushReference("HighContrast", "ListViewBackground", "SystemControlTransparentBrush");
        AssertThemeSolidColorBrushReference("HighContrast", "ListViewBorderBrush", "SystemControlTransparentBrush");
        AssertThemeSolidColorBrushReference("HighContrast", "GridViewColumnHeaderBackground", "SystemControlTransparentBrush");
        AssertThemeSolidColorBrushReference("HighContrast", "GridViewColumnHeaderBackgroundPointerOver", "SystemColorHighlightTextColorBrush");
        AssertThemeSolidColorBrushReference("HighContrast", "GridViewColumnHeaderBackgroundPressed", "SystemColorHighlightColorBrush");
        AssertThemeSolidColorBrushReference("HighContrast", "GridViewColumnHeaderBackgroundDisabled", "SystemColorWindowColorBrush");
        AssertThemeSolidColorBrushReference("HighContrast", "GridViewColumnHeaderBorderBrush", "SystemColorButtonTextColorBrush");
        AssertThemeSolidColorBrushReference("HighContrast", "GridViewColumnHeaderBorderBrushPointerOver", "SystemColorHighlightColorBrush");
        AssertThemeSolidColorBrushReference("HighContrast", "GridViewColumnHeaderBorderBrushPressed", "SystemColorHighlightTextColorBrush");
        AssertThemeSolidColorBrushReference("HighContrast", "GridViewColumnHeaderBorderBrushDisabled", "SystemColorWindowColorBrush");
        AssertThemeSolidColorBrushReference("HighContrast", "GridViewColumnHeaderForeground", "SystemColorButtonTextColorBrush");
        AssertThemeSolidColorBrushReference("HighContrast", "GridViewColumnHeaderForegroundDisabled", "SystemColorGrayTextColorBrush");
        AssertThemeSolidColorBrushReference("HighContrast", "GridViewColumnHeaderGripperThumbFill", "SystemColorButtonTextColorBrush");
        AssertThemeSolidColorBrushReference("HighContrast", "ListViewItemBackground", "SystemColorWindowColorBrush");
        AssertThemeSolidColorBrushReference("HighContrast", "ListViewItemBackgroundPointerOver", "SystemColorHighlightColorBrush");
        AssertThemeSolidColorBrushReference("HighContrast", "ListViewItemBackgroundPressed", "SystemColorHighlightColorBrush");
        AssertThemeSolidColorBrushReference("HighContrast", "ListViewItemBackgroundSelected", "SystemColorHighlightColorBrush");
        AssertThemeSolidColorBrushReference("HighContrast", "ListViewItemBackgroundSelectedPointerOver", "SystemColorHighlightColorBrush");
        AssertThemeSolidColorBrushReference("HighContrast", "ListViewItemBackgroundSelectedPressed", "SystemColorHighlightColorBrush");
        AssertThemeSolidColorBrushReference("HighContrast", "ListViewItemBackgroundSelectedDisabled", "SystemColorWindowColorBrush");
        AssertThemeSolidColorBrushReference("HighContrast", "ListViewItemForeground", "SystemColorButtonTextColorBrush");
        AssertThemeSolidColorBrushReference("HighContrast", "ListViewItemForegroundPointerOver", "SystemColorHighlightTextColorBrush");
        AssertThemeSolidColorBrushReference("HighContrast", "ListViewItemForegroundPressed", "SystemColorHighlightTextColorBrush");
        AssertThemeSolidColorBrushReference("HighContrast", "ListViewItemForegroundSelected", "SystemColorHighlightTextColorBrush");
        AssertThemeSolidColorBrushReference("HighContrast", "ListViewItemForegroundSelectedPointerOver", "SystemColorHighlightTextColorBrush");
        AssertThemeSolidColorBrushReference("HighContrast", "ListViewItemForegroundSelectedPressed", "SystemColorHighlightTextColorBrush");
        AssertThemeSolidColorBrushReference("HighContrast", "ListViewItemFocusVisualPrimaryBrush", "SystemColorWindowTextColorBrush");
        AssertThemeSolidColorBrushReference("HighContrast", "ListViewItemFocusVisualSecondaryBrush", "SystemColorWindowColorBrush");
        AssertThemeSolidColorBrushReference("HighContrast", "ListViewItemFocusBorderBrush", "SystemColorWindowTextColorBrush");
        AssertThemeSolidColorBrushReference("HighContrast", "ListViewItemFocusSecondaryBorderBrush", "SystemColorWindowTextColorBrush");
        AssertThemeSolidColorBrushReference("HighContrast", "ListViewItemCheckBrush", "SystemColorWindowTextColorBrush");
        AssertThemeSolidColorBrushReference("HighContrast", "ListViewItemCheckBoxBrush", "SystemColorButtonFaceColorBrush");
        AssertThemeSolidColorBrushReference("HighContrast", "ListViewItemDragBackground", "SystemControlTransparentBrush");
        AssertThemeSolidColorBrushReference("HighContrast", "ListViewItemDragForeground", "SystemColorWindowTextColorBrush");
        AssertThemeSolidColorBrushReference("HighContrast", "ListViewItemPlaceholderBackground", "SystemColorButtonFaceColorBrush");
        AssertThemeSolidColorBrushReference("HighContrast", "ListViewItemMultiArrangeOverlayTextBorder", "SystemColorWindowTextColorBrush");
        AssertThemeSolidColorBrushReference("HighContrast", "ListViewItemMultiArrangeOverlayTextBackground", "SystemColorWindowColorBrush");

        AssertThemeResourceReferences(
            "HighContrast",
            ("GridViewItemBackground", "SystemControlTransparentBrush"),
            ("GridViewItemBackgroundPointerOver", "SystemControlHighlightListLowBrush"),
            ("GridViewItemBackgroundPressed", "SystemControlHighlightListMediumBrush"),
            ("GridViewItemBackgroundSelected", "SystemControlHighlightAccentBrush"),
            ("GridViewItemBackgroundSelectedPointerOver", "SystemControlHighlightListAccentMediumBrush"),
            ("GridViewItemBackgroundSelectedPressed", "SystemControlHighlightListAccentHighBrush"),
            ("GridViewItemBackgroundSelectedDisabled", "SystemColorWindowColorBrush"),
            ("GridViewItemForeground", "SystemControlForegroundBaseHighBrush"),
            ("GridViewItemForegroundPointerOver", "SystemControlForegroundBaseHighBrush"),
            ("GridViewItemForegroundSelected", "SystemControlForegroundBaseHighBrush"),
            ("GridViewItemFocusVisualPrimaryBrush", "SystemControlFocusVisualPrimaryBrush"),
            ("GridViewItemFocusVisualSecondaryBrush", "SystemControlFocusVisualSecondaryBrush"),
            ("GridViewItemFocusBorderBrush", "SystemControlForegroundAltHighBrush"),
            ("GridViewItemFocusSecondaryBorderBrush", "SystemControlForegroundBaseHighBrush"),
            ("GridViewItemCheckBrush", "SystemControlForegroundBaseMediumHighBrush"),
            ("GridViewItemCheckBoxBrush", "SystemControlBackgroundChromeMediumBrush"),
            ("GridViewItemDragBackground", "SystemControlTransparentBrush"),
            ("GridViewItemDragForeground", "SystemControlHighlightAltChromeWhiteBrush"),
            ("GridViewItemPlaceholderBackground", "SystemControlDisabledChromeDisabledHighBrush"));
    }

    private static void AssertThemeResourceReferences(
        string themeName,
        params (string ResourceKey, string ExpectedResourceKey)[] references)
    {
        foreach (var reference in references)
        {
            AssertThemeResourceReference(themeName, reference.ResourceKey, reference.ExpectedResourceKey);
        }
    }

    private static void AssertThemeResourceReference(string themeName, string resourceKey, object expectedResourceKey)
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
        Assert.AreEqual(expectedValue, themeDictionary[resourceKey], $"{themeName}:{resourceKey}");
    }

    private static void AssertThemeSolidColorBrushReference(string themeName, string resourceKey, object expectedBrushKey)
    {
        var themeDictionary = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(themeDictionary.Contains(resourceKey), $"{themeName} is missing {resourceKey}.");
        Assert.IsTrue(themeDictionary.Contains(expectedBrushKey), $"{themeName} is missing {expectedBrushKey}.");
        var brush = (SolidColorBrush)themeDictionary[resourceKey];
        var expectedBrush = (SolidColorBrush)themeDictionary[expectedBrushKey];
        Assert.AreEqual(expectedBrush.Color, brush.Color, $"{themeName}:{resourceKey}");
        Assert.AreEqual(expectedBrush.Opacity, brush.Opacity, $"{themeName}:{resourceKey}");
    }

    private static void AssertThemeSolidColorBrushOpacity(string themeName, string resourceKey, double expectedOpacity)
    {
        var themeDictionary = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(themeDictionary.Contains(resourceKey), $"{themeName} is missing {resourceKey}.");
        Assert.IsInstanceOfType(themeDictionary[resourceKey], typeof(SolidColorBrush), $"{themeName}:{resourceKey}");
        Assert.AreEqual(expectedOpacity, ((SolidColorBrush)themeDictionary[resourceKey]).Opacity, $"{themeName}:{resourceKey}");
    }

    private static string FindRepoRoot()
    {
        var directory = new DirectoryInfo(System.AppContext.BaseDirectory);

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
