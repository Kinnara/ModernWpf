using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.CommonStyles;

[TestClass]
public class DataGridVisualStateTests
{
    [TestMethod]
    public void DefaultDataGridStyleUsesOfficialWpfFluentStyleSurface()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var defaultStyle = (Style)Application.Current.FindResource("DefaultDataGridStyle");
            var implicitStyle = (Style)Application.Current.FindResource(typeof(DataGrid));
            Assert.AreEqual(typeof(DataGrid), defaultStyle.TargetType);
            Assert.AreSame(defaultStyle, implicitStyle.BasedOn);

            AssertDynamicResourceSetter(defaultStyle, Control.BorderBrushProperty, "ControlElevationBorderBrush");
            AssertDynamicResourceSetter(defaultStyle, DataGrid.HorizontalGridLinesBrushProperty, "ControlElevationBorderBrush");
            AssertDynamicResourceSetter(defaultStyle, DataGrid.VerticalGridLinesBrushProperty, "ControlElevationBorderBrush");
            AssertDynamicResourceSetter(defaultStyle, System.Windows.Controls.Border.CornerRadiusProperty, "ControlCornerRadius");
            AssertStyleSetter(defaultStyle, DataGrid.RowStyleProperty, "DefaultDataGridRowStyle");
            AssertStyleSetter(defaultStyle, DataGrid.RowHeaderStyleProperty, "DefaultDataGridRowHeaderStyle");
            AssertStyleSetter(defaultStyle, DataGrid.CellStyleProperty, "DefaultDataGridCellStyle");
            AssertStyleSetter(defaultStyle, DataGrid.ColumnHeaderStyleProperty, "DefaultDataGridColumnHeaderStyle");
            AssertStyleSetter(defaultStyle, DataGrid.DropLocationIndicatorStyleProperty, "DefaultDataGridHeaderDropSeparatorStyle");
            AssertStyleSetter(defaultStyle, DataGrid.DragIndicatorStyleProperty, "DefaultDataGridColumnFloatingHeaderStyle");

            Assert.IsInstanceOfType(Application.Current.FindResource("FallbackBrushConverter"), typeof(FallbackBrushConverter));
            Assert.IsInstanceOfType(Application.Current.FindResource("DefaultDragIndicatorStyleStyle"), typeof(Style));
            Assert.IsInstanceOfType(Application.Current.FindResource("DataGridCheckBoxElementDefaultStyle"), typeof(Style));
            Assert.IsInstanceOfType(Application.Current.FindResource("DataGridCheckBoxEditingElementDefaultStyle"), typeof(Style));
        });
    }

    [TestMethod]
    public void DataGridTemplatesUseOfficialWpfPresenterSlots()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var cell = new DataGridCell
            {
                Style = (Style)Application.Current.FindResource("DefaultDataGridCellStyle"),
                Content = "Cell content"
            };
            var columnHeader = new DataGridColumnHeader
            {
                Style = (Style)Application.Current.FindResource("DefaultDataGridColumnHeaderStyle"),
                Content = "Column header"
            };
            var rowHeader = new DataGridRowHeader
            {
                Style = (Style)Application.Current.FindResource("DefaultDataGridRowHeaderStyle"),
                Content = "Row header"
            };
            var checkBox = new CheckBox
            {
                Style = (Style)Application.Current.FindResource("DataGridCheckBoxElementDefaultStyle"),
                Content = "CheckBox content"
            };

            using var host = new TestWindowHost(new StackPanel
            {
                Children = { cell, columnHeader, rowHeader, checkBox }
            }, width: 360, height: 220);
            host.UpdateLayout();

            AssertWpfPresenter(cell, cell.Content);
            AssertWpfPresenter(columnHeader, columnHeader.Content);
            AssertWpfPresenter(rowHeader, rowHeader.Content);
            AssertWpfPresenter(checkBox, checkBox.Content);
        });
    }

    [TestMethod]
    public void DataGridThemeResourcesExposeOfficialWpfFluentAliases()
    {
        foreach (var themeName in new[] { "Light", "Dark" })
        {
            AssertThemeResourceReference(themeName, "DataGridColumnFloatingHeaderBorderBrush", "ControlStrongStrokeColorDefaultBrush");
            AssertThemeResourceReference(themeName, "DataGridHeaderDropSeparatorBackground", "ControlStrongStrokeColorDefaultBrush");
            AssertThemeResourceReference(themeName, "DataGridHeaderSeparatorBrush", "ControlStrokeColorSecondaryBrush");
            AssertThemeResourceReference(themeName, "DataGridColumnHeaderForeground", "TextFillColorPrimaryBrush");
            AssertThemeResourceReference(themeName, "DataGridColumnHeaderBackground", "SubtleFillColorTransparentBrush");
            AssertThemeResourceReference(themeName, "DataGridHeaderBackground", "SubtleFillColorTertiaryBrush");
            AssertThemeResourceReference(themeName, "DataGridHeaderForegroundDisabled", "TextFillColorDisabledBrush");
            AssertThemeResourceReference(themeName, "DataGridRowSelectedForegroundThemeBrush", "TextOnAccentFillColorPrimaryBrush");
        }

        AssertThemeResourceReference("Light", "DataGridRowSelectedBackgroundThemeBrush", "SystemAccentColorDark1Brush");
        AssertThemeResourceReference("Dark", "DataGridRowSelectedBackgroundThemeBrush", "SystemAccentColorLight3Brush");
        AssertThemeResourceReference("HighContrast", "DataGridColumnFloatingHeaderBorderBrush", "SystemColorHighlightColorBrush");
        AssertThemeResourceReference("HighContrast", "DataGridHeaderSeparatorBrush", "SystemColorButtonTextColorBrush");
        AssertThemeResourceReference("HighContrast", "DataGridColumnHeaderBackground", "SystemControlTransparentBrush");
        AssertThemeResourceReference("HighContrast", "DataGridHeaderBackground", "SystemColorButtonFaceColorBrush");
        AssertThemeResourceReference("HighContrast", "DataGridHeaderForegroundDisabled", "SystemColorGrayTextColorBrush");
        AssertThemeResourceReference("HighContrast", "DataGridRowSelectedBackgroundThemeBrush", "SystemColorButtonTextColorBrush");
        AssertThemeResourceReference("HighContrast", "DataGridRowSelectedForegroundThemeBrush", "SystemColorButtonFaceColorBrush");
    }

    [TestMethod]
    public void DataGridFileDeletesModernWpfGuessedTemplateBranches()
    {
        var repoRoot = FindRepoRoot();
        var text = File.ReadAllText(Path.Combine(repoRoot, "ModernWpf", "Styles", "DataGrid.xaml"));

        Assert.IsTrue(text.Contains("DefaultDataGridColumnFloatingHeaderStyle", System.StringComparison.Ordinal));
        Assert.IsTrue(text.Contains("DataGridCheckBoxElementDefaultStyle", System.StringComparison.Ordinal));
        Assert.IsTrue(text.Contains("ResourceId=DataGridSelectAllButtonStyle", System.StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("ContentPresenterEx", System.StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("FontIconFallback", System.StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("DataGridHelper", System.StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("DataGridRowHelper", System.StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("DataGridCellPresenter", System.StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("DataGridCellExpanded", System.StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("DataGridRowGroupHeaderStyle", System.StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("Fluent.Controls", System.StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("System.Runtime", System.StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("ControlHelper.CornerRadius", System.StringComparison.Ordinal));
    }

    private static void AssertDynamicResourceSetter(Style style, DependencyProperty property, object expectedResourceKey)
    {
        var setter = style.Setters.OfType<Setter>().SingleOrDefault(setter => setter.Property == property);
        Assert.IsNotNull(setter, $"Expected local setter for {property.Name}.");
        Assert.IsInstanceOfType(setter!.Value, typeof(DynamicResourceExtension));
        Assert.AreEqual(expectedResourceKey, ((DynamicResourceExtension)setter.Value).ResourceKey);
    }

    private static void AssertStyleSetter(Style style, DependencyProperty property, string expectedResourceKey)
    {
        var setter = style.Setters.OfType<Setter>().SingleOrDefault(setter => setter.Property == property);
        Assert.IsNotNull(setter, $"Expected local setter for {property.Name}.");
        Assert.AreSame(Application.Current.FindResource(expectedResourceKey), setter!.Value);
    }

    private static void AssertThemeResourceReference(string themeName, string resourceKey, object expectedResourceKey)
    {
        var themeDictionary = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(themeDictionary.Contains(resourceKey), $"{themeName} is missing {resourceKey}.");
        Assert.IsTrue(themeDictionary.Contains(expectedResourceKey), $"{themeName} is missing {expectedResourceKey}.");
        Assert.AreSame(themeDictionary[expectedResourceKey], themeDictionary[resourceKey], $"{themeName}:{resourceKey}");
    }

    private static void AssertWpfPresenter(DependencyObject root, object expectedContent)
    {
        var presenter = VisualTreeTestHelper.EnumerateDescendants(root)
            .OfType<ContentPresenter>()
            .FirstOrDefault(item => Equals(item.Content, expectedContent))
            ?? throw new AssertFailedException($"Expected {root.GetType().Name} template to use WPF ContentPresenter.");

        Assert.AreEqual(typeof(ContentPresenter), presenter.GetType());
        Assert.IsNull(VisualTreeTestHelper.FindDescendant<ContentPresenterEx>(root));
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
