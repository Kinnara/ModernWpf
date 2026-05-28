using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;
using WpfGridView = System.Windows.Controls.GridView;
using WpfListView = System.Windows.Controls.ListView;
using WpfListViewItem = System.Windows.Controls.ListViewItem;
using WpfRectangle = System.Windows.Shapes.Rectangle;

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

            AssertDynamicResourceSetter(defaultListBoxStyle, Control.BackgroundProperty, "ListBoxBackground");
            AssertSetterValue(defaultListBoxStyle, FrameworkElement.MarginProperty, new Thickness(0));
            AssertSetterValue(defaultListBoxStyle, Control.PaddingProperty, new Thickness(0));
            AssertSetterValue(defaultListBoxStyle, Control.BorderThicknessProperty, new Thickness(0));
            AssertSetterValue(defaultListBoxStyle, ScrollViewer.VerticalScrollBarVisibilityProperty, ScrollBarVisibility.Auto);
            AssertSetterValue(defaultListBoxStyle, ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Auto);
            AssertSetterValue(defaultListBoxStyle, ScrollViewer.CanContentScrollProperty, true);
            AssertSetterValue(defaultListBoxStyle, Control.VerticalContentAlignmentProperty, VerticalAlignment.Center);
            AssertSetterValue(defaultListBoxStyle, VirtualizingPanel.IsVirtualizingProperty, true);
            AssertSetterValue(defaultListBoxStyle, VirtualizingPanel.VirtualizationModeProperty, VirtualizationMode.Standard);
            AssertSetterValue(defaultListBoxStyle, UIElement.SnapsToDevicePixelsProperty, true);
            AssertSetterValue(defaultListBoxStyle, FrameworkElement.OverridesDefaultStyleProperty, true);
            Assert.IsInstanceOfType(FindSetter(defaultListBoxStyle, ItemsControl.ItemsPanelProperty)?.Value, typeof(ItemsPanelTemplate));
            Assert.IsInstanceOfType(FindSetter(defaultListBoxStyle, Control.TemplateProperty)?.Value, typeof(ControlTemplate));

            var listBox = new ListBox
            {
                Width = 180,
                Height = 80
            };
            listBox.Items.Add("First");
            using (var listBoxHost = new TestWindowHost(listBox, width: 220, height: 120))
            {
                listBoxHost.UpdateLayout();

                Assert.AreSame(listBox.TryFindResource("ListBoxBackground"), listBox.Background);
                Assert.AreEqual(new Thickness(0), listBox.Margin);
                Assert.AreEqual(new Thickness(0), listBox.Padding);
                Assert.AreEqual(new Thickness(0), listBox.BorderThickness);
                Assert.AreEqual(ScrollBarVisibility.Auto, ScrollViewer.GetVerticalScrollBarVisibility(listBox));
                Assert.AreEqual(ScrollBarVisibility.Auto, ScrollViewer.GetHorizontalScrollBarVisibility(listBox));
                Assert.IsTrue(ScrollViewer.GetCanContentScroll(listBox));
                Assert.AreEqual(VerticalAlignment.Center, listBox.VerticalContentAlignment);
                Assert.IsTrue(VirtualizingPanel.GetIsVirtualizing(listBox));
                Assert.AreEqual(VirtualizationMode.Standard, VirtualizingPanel.GetVirtualizationMode(listBox));
                Assert.IsTrue(listBox.SnapsToDevicePixels);
                Assert.IsTrue(listBox.OverridesDefaultStyle);

                var rootBorder = FindTemplateChild<Border>(listBox, "Bd");
                Assert.AreSame(listBox.Background, rootBorder.Background);
                Assert.AreSame(listBox.BorderBrush, rootBorder.BorderBrush);
                Assert.AreEqual(listBox.BorderThickness, rootBorder.BorderThickness);

                var scrollViewer = FindTemplateChild<ScrollViewer>(listBox, "PART_ContentHost");
                Assert.AreEqual(listBox.Padding, scrollViewer.Padding);
                Assert.AreEqual(ScrollViewer.GetCanContentScroll(listBox), scrollViewer.CanContentScroll);
                Assert.AreEqual(ScrollViewer.GetHorizontalScrollBarVisibility(listBox), scrollViewer.HorizontalScrollBarVisibility);
                Assert.AreEqual(ScrollViewer.GetVerticalScrollBarVisibility(listBox), scrollViewer.VerticalScrollBarVisibility);

                var itemsPresenter = VisualTreeTestHelper.FindDescendant<ItemsPresenter>(listBox)
                    ?? throw new AssertFailedException("Expected ListBox template ItemsPresenter.");
                Assert.AreEqual(listBox.SnapsToDevicePixels, itemsPresenter.SnapsToDevicePixels);
            }

            var defaultItemStyle = (Style)Application.Current.FindResource("DefaultListBoxItemStyle");
            var implicitItemStyle = (Style)Application.Current.FindResource(typeof(ListBoxItem));
            Assert.AreEqual(typeof(ListBoxItem), defaultItemStyle.TargetType);
            Assert.AreSame(defaultItemStyle, implicitItemStyle.BasedOn);

            var itemSetters = defaultItemStyle.Setters.OfType<Setter>().ToArray();
            AssertDynamicResourceSetter(itemSetters, Control.ForegroundProperty, "ListBoxItemForeground");
            AssertBrushSetter(defaultItemStyle, Control.BackgroundProperty, Brushes.Transparent);
            AssertBrushSetter(defaultItemStyle, Control.BorderBrushProperty, Brushes.Transparent);
            Assert.IsInstanceOfType(FindSetter(defaultItemStyle, Control.HorizontalContentAlignmentProperty)?.Value, typeof(BindingBase));
            Assert.IsInstanceOfType(FindSetter(defaultItemStyle, Control.VerticalContentAlignmentProperty)?.Value, typeof(BindingBase));
            AssertDynamicResourceSetter(itemSetters, Control.FocusVisualStyleProperty, "DefaultCollectionFocusVisualStyle");
            AssertDynamicResourceSetter(defaultItemStyle, FrameworkElement.MarginProperty, "ListBoxItemMargin");
            AssertSetterValue(defaultItemStyle, Control.BorderThicknessProperty, new Thickness(1));
            AssertDynamicResourceSetter(defaultItemStyle, Control.PaddingProperty, "ListBoxItemPadding");
            AssertSetter(itemSetters, System.Windows.Controls.Border.CornerRadiusProperty, new CornerRadius(0));
            AssertSetterValue(defaultItemStyle, UIElement.SnapsToDevicePixelsProperty, true);
            AssertSetterValue(defaultItemStyle, FrameworkElement.OverridesDefaultStyleProperty, true);
            Assert.IsInstanceOfType(FindSetter(defaultItemStyle, Control.TemplateProperty)?.Value, typeof(ControlTemplate));
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
            Assert.AreSame(item.BorderBrush, border.BorderBrush);
            Assert.AreEqual(item.BorderThickness, border.BorderThickness);
            Assert.AreEqual(item.Padding, border.Padding);
            Assert.AreEqual(item.GetValue(System.Windows.Controls.Border.CornerRadiusProperty), border.CornerRadius);
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

            var baseListViewStyle = (Style)Application.Current.FindResource("BaseListViewStyle");
            Assert.AreSame(baseListViewStyle, defaultListViewStyle.BasedOn);
            AssertSetterValue(baseListViewStyle, FrameworkElement.MarginProperty, new Thickness(0));
            AssertSetterValue(baseListViewStyle, Control.PaddingProperty, new Thickness(0));
            AssertDynamicResourceSetter(baseListViewStyle, Control.BackgroundProperty, "ListViewBackground");
            AssertDynamicResourceSetter(baseListViewStyle, Control.BorderBrushProperty, "ListViewBorderBrush");
            AssertSetterValue(baseListViewStyle, Control.BorderThicknessProperty, new Thickness(1));
            AssertSetterValue(baseListViewStyle, ScrollViewer.VerticalScrollBarVisibilityProperty, ScrollBarVisibility.Auto);
            AssertSetterValue(baseListViewStyle, ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Auto);
            AssertSetterValue(baseListViewStyle, ScrollViewer.CanContentScrollProperty, true);
            AssertSetterValue(baseListViewStyle, Control.VerticalContentAlignmentProperty, VerticalAlignment.Center);
            AssertSetterValue(baseListViewStyle, VirtualizingPanel.IsVirtualizingProperty, true);
            AssertSetterValue(baseListViewStyle, VirtualizingPanel.VirtualizationModeProperty, VirtualizationMode.Standard);
            AssertSetterValue(baseListViewStyle, UIElement.SnapsToDevicePixelsProperty, true);
            AssertSetterValue(baseListViewStyle, FrameworkElement.OverridesDefaultStyleProperty, true);
            Assert.IsInstanceOfType(FindSetter(baseListViewStyle, ItemsControl.ItemsPanelProperty)?.Value, typeof(ItemsPanelTemplate));
            AssertSetterValue(defaultListViewStyle, Control.TemplateProperty, Application.Current.FindResource("ListViewTemplate"));
            AssertGridViewDataTrigger(defaultListViewStyle);

            var listView = new WpfListView
            {
                Width = 180,
                Height = 80
            };
            listView.Items.Add("First");
            using (var listViewHost = new TestWindowHost(listView, width: 220, height: 120))
            {
                listViewHost.UpdateLayout();

                Assert.AreSame(listView.TryFindResource("ListViewBackground"), listView.Background);
                Assert.AreSame(listView.TryFindResource("ListViewBorderBrush"), listView.BorderBrush);
                Assert.AreEqual(new Thickness(1), listView.BorderThickness);
                Assert.AreEqual(new Thickness(0), listView.Margin);
                Assert.AreEqual(new Thickness(0), listView.Padding);
                Assert.AreEqual(ScrollBarVisibility.Auto, ScrollViewer.GetVerticalScrollBarVisibility(listView));
                Assert.AreEqual(ScrollBarVisibility.Auto, ScrollViewer.GetHorizontalScrollBarVisibility(listView));
                Assert.IsTrue(ScrollViewer.GetCanContentScroll(listView));
                Assert.IsTrue(VirtualizingPanel.GetIsVirtualizing(listView));
                Assert.AreEqual(VirtualizationMode.Standard, VirtualizingPanel.GetVirtualizationMode(listView));
                Assert.IsTrue(listView.SnapsToDevicePixels);
                Assert.IsTrue(listView.OverridesDefaultStyle);

                var rootBorder = VisualTreeTestHelper.FindDescendant<Border>(listView)
                    ?? throw new AssertFailedException("Expected ListView template root Border.");
                Assert.AreSame(listView.Background, rootBorder.Background);
                Assert.AreSame(listView.BorderBrush, rootBorder.BorderBrush);
                Assert.AreEqual(listView.BorderThickness, rootBorder.BorderThickness);

                var scrollViewer = FindTemplateChild<ScrollViewer>(listView, "PART_ContentHost");
                Assert.AreEqual(listView.Padding, scrollViewer.Padding);
                Assert.AreEqual(ScrollViewer.GetCanContentScroll(listView), scrollViewer.CanContentScroll);

                listView.IsEnabled = false;
                listViewHost.UpdateLayout();
                var disabledVisual = FindTemplateChild<WpfRectangle>(listView, "PART_DisabledVisual");
                Assert.AreEqual(Visibility.Visible, disabledVisual.Visibility);
            }

            var gridViewStyle = (Style)Application.Current.FindResource(WpfGridView.GridViewStyleKey);
            Assert.AreSame(baseListViewStyle, gridViewStyle.BasedOn);
            AssertSetterValue(gridViewStyle, ItemsControl.ItemContainerStyleProperty, Application.Current.FindResource(WpfGridView.GridViewItemContainerStyleKey));
            AssertSetterValue(gridViewStyle, Control.TemplateProperty, Application.Current.FindResource("GridViewTemplate"));
            AssertSetterValue(gridViewStyle, WpfGridView.ColumnHeaderContainerStyleProperty, Application.Current.FindResource("DefaultGridViewColumnHeaderStyle"));

            var defaultItemStyle = (Style)Application.Current.FindResource("DefaultListViewItemStyle");
            var implicitItemStyle = (Style)Application.Current.FindResource(typeof(WpfListViewItem));
            Assert.AreEqual(typeof(WpfListViewItem), defaultItemStyle.TargetType);
            Assert.AreSame(defaultItemStyle, implicitItemStyle.BasedOn);

            var itemSetters = defaultItemStyle.Setters.OfType<Setter>().ToArray();
            AssertDynamicResourceSetter(itemSetters, Control.ForegroundProperty, "ListViewItemForeground");
            AssertBrushSetter(defaultItemStyle, Control.BackgroundProperty, Brushes.Transparent);
            AssertDynamicResourceSetter(itemSetters, System.Windows.Controls.Border.CornerRadiusProperty, "ControlCornerRadius");
            AssertSetterValue(defaultItemStyle, FrameworkElement.MinHeightProperty, 40.0);
            AssertSetterValue(defaultItemStyle, FrameworkElement.MinWidthProperty, 88.0);
            AssertSetterValue(defaultItemStyle, Control.BorderThicknessProperty, new Thickness(1));
            AssertDynamicResourceSetter(defaultItemStyle, FrameworkElement.MarginProperty, "ListViewItemMargin");
            AssertDynamicResourceSetter(defaultItemStyle, Control.PaddingProperty, "ListViewItemPadding");
            AssertSetterValue(defaultItemStyle, Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch);
            AssertSetterValue(defaultItemStyle, Control.VerticalContentAlignmentProperty, VerticalAlignment.Center);
            AssertDynamicResourceSetter(itemSetters, Control.FocusVisualStyleProperty, "DefaultCollectionFocusVisualStyle");
            AssertSetter(itemSetters, Control.OverridesDefaultStyleProperty, true);
            Assert.IsInstanceOfType(FindSetter(defaultItemStyle, Control.TemplateProperty)?.Value, typeof(ControlTemplate));
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
            var itemBorder = item.Template.FindName("Border", item) as Border
                ?? throw new AssertFailedException("Expected ListViewItem template Border.");
            Assert.AreSame(item.TryFindResource("ListViewItemBackgroundPointerOver"), itemBorder.Background);
            Assert.AreEqual(item.GetValue(System.Windows.Controls.Border.CornerRadiusProperty), itemBorder.CornerRadius);

            var gridViewItemStyle = (Style)Application.Current.FindResource(WpfGridView.GridViewItemContainerStyleKey);
            Assert.AreSame(defaultItemStyle, gridViewItemStyle.BasedOn);
            AssertSetterValue(gridViewItemStyle, Control.PaddingProperty, new Thickness(6, 0, 6, 0));
            AssertDynamicResourceSetter(gridViewItemStyle, FrameworkElement.MinHeightProperty, "GridViewItemContainerMinHeight");
            Assert.IsInstanceOfType(FindSetter(gridViewItemStyle, Control.TemplateProperty)?.Value, typeof(ControlTemplate));

            var gridViewItem = new WpfListViewItem
            {
                Style = gridViewItemStyle,
                Content = "GridView content",
                IsSelected = true
            };
            using var gridViewItemHost = new TestWindowHost(gridViewItem);
            gridViewItemHost.UpdateLayout();

            var gridViewItemRoot = FindTemplateChild<Border>(gridViewItem, "RootBorder");
            Assert.AreSame(gridViewItem.TryFindResource("ListViewItemBackgroundPointerOver"), gridViewItemRoot.Background);
            Assert.AreEqual(gridViewItem.GetValue(System.Windows.Controls.Border.CornerRadiusProperty), gridViewItemRoot.CornerRadius);
            var gridViewActiveRectangle = FindTemplateChild<WpfRectangle>(gridViewItem, "ActiveRectangle");
            Assert.AreEqual(Visibility.Visible, gridViewActiveRectangle.Visibility);
            Assert.AreSame(gridViewItem.TryFindResource("ListViewItemPillFillBrush"), gridViewActiveRectangle.Fill);
            Assert.IsNotNull(VisualTreeTestHelper.FindDescendant<GridViewRowPresenter>(gridViewItem));
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
            AssertDynamicResourceSetter(defaultHeaderStyle, Control.BackgroundProperty, "GridViewColumnHeaderBackground");
            AssertDynamicResourceSetter(defaultHeaderStyle, Control.BorderBrushProperty, "GridViewColumnHeaderBorderBrush");
            AssertDynamicResourceSetter(defaultHeaderStyle, Control.ForegroundProperty, "GridViewColumnHeaderForeground");
            AssertSetterValue(defaultHeaderStyle, Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch);
            AssertSetterValue(defaultHeaderStyle, Control.VerticalContentAlignmentProperty, VerticalAlignment.Center);
            AssertSetterValue(defaultHeaderStyle, Control.BorderThicknessProperty, new Thickness(0, 2, 0, 2));
            AssertSetterValue(defaultHeaderStyle, Control.PaddingProperty, new Thickness(12, 0, 12, 0));
            AssertSetterValue(defaultHeaderStyle, FrameworkElement.OverridesDefaultStyleProperty, true);
            Assert.IsInstanceOfType(FindSetter(defaultHeaderStyle, Control.TemplateProperty)?.Value, typeof(ControlTemplate));
            AssertGridViewColumnHeaderRoleTriggers(defaultHeaderStyle);

            var gripperStyle = (Style)Application.Current.FindResource("DefaultGridViewColumnHeaderGripper");
            AssertSetterValue(gripperStyle, Canvas.RightProperty, -4.0);
            AssertSetterValue(gripperStyle, FrameworkElement.WidthProperty, 8.0);
            Assert.IsInstanceOfType(FindSetter(gripperStyle, FrameworkElement.HeightProperty)?.Value, typeof(BindingBase));
            AssertDynamicResourceSetter(gripperStyle, System.Windows.Controls.Border.CornerRadiusProperty, "ControlCornerRadius");
            Assert.IsInstanceOfType(FindSetter(gripperStyle, Control.TemplateProperty)?.Value, typeof(ControlTemplate));

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

            Assert.AreSame(header.TryFindResource("GridViewColumnHeaderBackground"), header.Background);
            Assert.AreSame(header.TryFindResource("GridViewColumnHeaderBorderBrush"), header.BorderBrush);
            Assert.AreSame(header.TryFindResource("GridViewColumnHeaderForeground"), header.Foreground);
            Assert.AreEqual(new Thickness(0, 2, 0, 2), header.BorderThickness);
            Assert.AreEqual(new Thickness(12, 0, 12, 0), header.Padding);
            Assert.IsTrue(header.OverridesDefaultStyle);

            var headerBorder = FindTemplateChild<Border>(header, "HeaderBorder");
            Assert.AreSame(header.Background, headerBorder.Background);
            Assert.AreSame(header.BorderBrush, headerBorder.BorderBrush);
            Assert.AreEqual(header.BorderThickness, headerBorder.BorderThickness);
            Assert.AreEqual(new CornerRadius(4, 4, 0, 0), headerBorder.CornerRadius);
            Assert.AreEqual(header.Padding, presenter.Margin);
            Assert.AreEqual(header.HorizontalContentAlignment, presenter.HorizontalAlignment);
            Assert.AreEqual(header.VerticalContentAlignment, presenter.VerticalAlignment);
            Assert.IsTrue(presenter.RecognizesAccessKey);
            Assert.AreEqual(header.SnapsToDevicePixels, presenter.SnapsToDevicePixels);

            var gripper = FindTemplateChild<Thumb>(header, "PART_HeaderGripper");
            Assert.AreSame(gripperStyle, gripper.Style);
            Assert.AreEqual(-4.0, Canvas.GetRight(gripper));
            Assert.AreEqual(8.0, gripper.Width);
            gripper.ApplyTemplate();
            var gripperThumb = FindTemplateChild<WpfRectangle>(gripper, "PART_Thumb");
            Assert.AreEqual(2.0, gripperThumb.Width);
            Assert.AreEqual(16.0, gripperThumb.Height);
            Assert.AreSame(gripper.TryFindResource("GridViewColumnHeaderGripperThumbFill"), gripperThumb.Fill);
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
                .Select(file => File.ReadAllText(System.IO.Path.Combine(repoRoot, "ModernWpf", "Styles", file))));

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

    private static void AssertSetterValue(Style style, DependencyProperty property, object expectedValue)
    {
        var setter = FindSetter(style, property);
        Assert.IsNotNull(setter, $"Expected setter for {property.Name}.");
        Assert.AreEqual(expectedValue, setter!.Value);
    }

    private static void AssertDynamicResourceSetter(Style style, DependencyProperty property, string resourceKey)
    {
        var setter = FindSetter(style, property);
        Assert.IsNotNull(setter, $"Expected setter for {property.Name}.");
        Assert.IsInstanceOfType(setter!.Value, typeof(DynamicResourceExtension));
        Assert.AreEqual(resourceKey, ((DynamicResourceExtension)setter.Value).ResourceKey);
    }

    private static void AssertBrushSetter(Style style, DependencyProperty property, Brush expectedBrush)
    {
        var setter = FindSetter(style, property);
        Assert.IsNotNull(setter, $"Expected setter for {property.Name}.");
        Assert.IsInstanceOfType(setter!.Value, typeof(Brush));
        AssertBrushEquals(expectedBrush, (Brush)setter.Value);
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

    private static T FindTemplateChild<T>(Control control, string name)
        where T : FrameworkElement
    {
        control.ApplyTemplate();
        return control.Template?.FindName(name, control) as T
            ?? throw new AssertFailedException($"Expected template child '{name}' on {control.GetType().Name}.");
    }

    private static void AssertGridViewDataTrigger(Style style)
    {
        var trigger = style.Triggers
            .OfType<DataTrigger>()
            .Single();
        Assert.AreEqual("True", trigger.Value?.ToString());
        Assert.IsInstanceOfType(trigger.Binding, typeof(Binding));

        var binding = (Binding)trigger.Binding;
        Assert.AreEqual("View", binding.Path.Path);
        Assert.AreSame(Application.Current.FindResource("ViewIsGridViewConverter"), binding.Converter);

        AssertTriggerSetter(trigger, Control.TemplateProperty, Application.Current.FindResource("GridViewTemplate"));
        AssertTriggerSetter(trigger, ItemsControl.ItemContainerStyleProperty, Application.Current.FindResource(WpfGridView.GridViewItemContainerStyleKey));
        AssertTriggerSetter(trigger, WpfGridView.ColumnHeaderContainerStyleProperty, Application.Current.FindResource("DefaultGridViewColumnHeaderStyle"));
    }

    private static void AssertGridViewColumnHeaderRoleTriggers(Style style)
    {
        var paddingTrigger = style.Triggers
            .OfType<Trigger>()
            .Single(item => item.Property == GridViewColumnHeader.RoleProperty && Equals(item.Value, GridViewColumnHeaderRole.Padding));
        AssertTriggerSetter(paddingTrigger, Control.BorderThicknessProperty, new Thickness(0, 2, 0, 2));
        Assert.IsInstanceOfType(FindTriggerSetter(paddingTrigger, Control.TemplateProperty)?.Value, typeof(ControlTemplate));

        var floatingTrigger = style.Triggers
            .OfType<Trigger>()
            .Single(item => item.Property == GridViewColumnHeader.RoleProperty && Equals(item.Value, GridViewColumnHeaderRole.Floating));
        AssertTriggerSetter(floatingTrigger, UIElement.OpacityProperty, 0.6);
        Assert.IsInstanceOfType(FindTriggerSetter(floatingTrigger, Control.TemplateProperty)?.Value, typeof(ControlTemplate));
    }

    private static void AssertTriggerSetter(TriggerBase trigger, DependencyProperty property, object expectedValue)
    {
        var setter = FindTriggerSetter(trigger, property);
        Assert.IsNotNull(setter, $"Expected trigger setter for {property.Name}.");
        Assert.AreEqual(expectedValue, setter!.Value);
    }

    private static Setter? FindTriggerSetter(TriggerBase trigger, DependencyProperty property)
    {
        var setters = trigger switch
        {
            Trigger typedTrigger => typedTrigger.Setters,
            DataTrigger typedTrigger => typedTrigger.Setters,
            MultiTrigger typedTrigger => typedTrigger.Setters,
            _ => throw new AssertFailedException($"Unsupported trigger type {trigger.GetType().Name}.")
        };

        return setters
            .OfType<Setter>()
            .SingleOrDefault(item => item.Property == property);
    }

    private static void AssertBrushEquals(Brush expected, Brush actual)
    {
        Assert.IsNotNull(expected);
        Assert.IsNotNull(actual);

        if (expected is SolidColorBrush expectedSolid && actual is SolidColorBrush actualSolid)
        {
            Assert.AreEqual(expectedSolid.Color, actualSolid.Color);
            Assert.AreEqual(expectedSolid.Opacity, actualSolid.Opacity);
            return;
        }

        Assert.AreEqual(expected.ToString(), actual.ToString());
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
            if (File.Exists(System.IO.Path.Combine(directory.FullName, "ModernWpf.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        Assert.Fail("Could not locate ModernWpf.sln from the test output directory.");
        return string.Empty;
    }
}
