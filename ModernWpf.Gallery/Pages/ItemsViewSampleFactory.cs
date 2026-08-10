using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using Mux = ModernWpf.Controls;

namespace ModernWpf.Gallery.Pages
{
    internal static class ItemsViewSampleFactory
    {
        private const string BasicXaml =
@"<ItemsView ItemsSource=""{Binding Reports}""
           IsItemInvokedEnabled=""True""
           ItemInvoked=""ItemsView_ItemInvoked"">
    <ItemsView.ItemTemplate>
        <DataTemplate>
            <ItemContainer AutomationProperties.Name=""{Binding Label}"">
                <Border Height=""84"" Margin=""4"">
                    <TextBlock Text=""{Binding Label}"" />
                </Border>
            </ItemContainer>
        </DataTemplate>
    </ItemsView.ItemTemplate>
</ItemsView>";

        private const string BasicCode =
@"private void ItemsView_ItemInvoked(
    ItemsView sender,
    ItemsViewItemInvokedEventArgs args)
{
    invocationResult.Text = $""Invoked: {args.InvokedItem}"";
}

// SelectionMode defaults to Single. Enter invokes the current item;
// Space updates selection; double-click invokes in selectable modes.";

        private const string LayoutsXaml =
@"<ItemsView ItemsSource=""{Binding Photos}""
           ItemTemplate=""{StaticResource PhotoItemTemplate}""
           Layout=""{Binding SelectedLayout}"" />";

        private const string LayoutsCode =
@"itemsView.Layout = layoutName switch
{
    ""StackLayout"" => new StackLayout { Spacing = 8 },
    ""UniformGridLayout"" => new UniformGridLayout
    {
        MinItemWidth = 150,
        MinItemHeight = 92,
        MinRowSpacing = 8,
        MinColumnSpacing = 8
    },
    _ => CreateLinedFlowLayout()
};

// ItemsView forwards ItemTransitionProvider to its ItemsRepeater.
itemsView.ItemTransitionProvider = layoutName == ""LinedFlowLayout""
    ? new LinedFlowLayoutItemCollectionTransitionProvider()
    : null;

// WPF adaptation: ItemsView.ScrollView exposes the WPF ScrollViewer while
// ItemsRepeaterScrollHost retains realization-window and anchor behavior.";

        private const string SelectionXaml =
@"<ItemsView ItemsSource=""{Binding Reports}""
           SelectionMode=""Multiple""
           IsItemInvokedEnabled=""True""
           SelectionChanged=""ItemsView_SelectionChanged"" />";

        private const string SelectionCode =
@"itemsView.SelectAll();
itemsView.Deselect(0);
itemsView.InvertSelection();

// None disables user selection, Single selects one item, and Multiple and
// Extended expose multi-selection through UI Automation. Ctrl+A selects all
// items in Multiple and Extended modes.";

        public static UIElement Create(string uniqueId)
        {
            return uniqueId == "ItemsView"
                ? CreateBasicExample(out _)
                : null;
        }

        public static IReadOnlyList<GalleryExample> CreateExamples(string uniqueId)
        {
            if (uniqueId != "ItemsView")
            {
                return Array.Empty<GalleryExample>();
            }

            var basic = CreateBasicExample(out var basicOptions);
            var layouts = CreateLayoutsExample(out var layoutsOptions);
            var selection = CreateSelectionExample(out var selectionOptions);
            return new[]
            {
                new GalleryExample(
                    "A selectable collection with item invocation",
                    basic,
                    BasicXaml,
                    BasicCode,
                    basicOptions),
                new GalleryExample(
                    "Switch an ItemsView between virtualizing layouts",
                    layouts,
                    LayoutsXaml,
                    LayoutsCode,
                    layoutsOptions),
                new GalleryExample(
                    "Selection and invocation modes",
                    selection,
                    SelectionXaml,
                    SelectionCode,
                    selectionOptions)
            };
        }

        private static GallerySamplePanel CreateBasicExample(out UIElement optionsContent)
        {
            var items = CreateLayoutItems(16);
            var itemsView = CreateItemsView(items, "PrimaryItemsView");
            itemsView.Layout = new Mux.StackLayout { Spacing = 8 };
            itemsView.IsItemInvokedEnabled = true;

            var invocationResult = new TextBlock
            {
                Margin = new Thickness(0, 10, 0, 0),
                Text = "Double-click an item or focus it and press Enter.",
                TextWrapping = TextWrapping.Wrap
            };
            AutomationProperties.SetName(invocationResult, "Invocation result");
            GalleryAutomation.WithAutomationId(
                invocationResult,
                GalleryAutomation.SampleElementId("ItemsView", "InvocationResult"));
            itemsView.ItemInvoked += delegate (Mux.ItemsView sender, Mux.ItemsViewItemInvokedEventArgs args)
            {
                var item = args.InvokedItem as LayoutItem;
                invocationResult.Text = "Invoked: " + (item?.Label ?? args.InvokedItem?.ToString());
            };

            var root = CreateRoot("BasicRoot");
            GalleryAutomation.WithAutomationId(root, GalleryAutomation.SampleRootId("ItemsView"));
            root.Children.Add(itemsView);
            root.Children.Add(invocationResult);

            optionsContent = new TextBlock
            {
                Text = "ItemsView defaults to single selection. Use arrow keys to move, Space to select, and Enter or double-click to invoke.",
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = 300
            };
            return root;
        }

        private static GallerySamplePanel CreateLayoutsExample(out UIElement optionsContent)
        {
            var items = CreateLayoutItems(80);
            var itemsView = CreateItemsView(items, "LayoutItemsView");
            SetLayout(itemsView, items, "UniformGridLayout");

            var layoutSelector = new ComboBox
            {
                ItemsSource = new[] { "StackLayout", "UniformGridLayout", "LinedFlowLayout" },
                SelectedItem = "UniformGridLayout",
                MinWidth = 200
            };
            AutomationProperties.SetName(layoutSelector, "Layout");
            GalleryAutomation.WithAutomationId(
                layoutSelector,
                GalleryAutomation.SampleElementId("ItemsView", "LayoutSelector"));
            layoutSelector.SelectionChanged += delegate
            {
                if (layoutSelector.SelectedItem is string layoutName)
                {
                    SetLayout(itemsView, items, layoutName);
                }
            };

            var add = new Button { Content = "Add item", Margin = new Thickness(0, 12, 0, 0) };
            var remove = new Button { Content = "Remove item", Margin = new Thickness(8, 12, 0, 0) };
            AutomationProperties.SetName(add, "Add an item");
            AutomationProperties.SetName(remove, "Remove the last item");
            GalleryAutomation.WithAutomationId(
                add,
                GalleryAutomation.SampleElementId("ItemsView", "AddItem"));
            GalleryAutomation.WithAutomationId(
                remove,
                GalleryAutomation.SampleElementId("ItemsView", "RemoveItem"));
            add.Click += delegate { items.Add(CreateLayoutItem(items.Count)); };
            remove.Click += delegate
            {
                if (items.Count > 1)
                {
                    items.RemoveAt(items.Count - 1);
                }
            };

            var root = CreateRoot("LayoutsRoot");
            root.Children.Add(itemsView);

            var buttons = new StackPanel { Orientation = Orientation.Horizontal };
            buttons.Children.Add(add);
            buttons.Children.Add(remove);

            var options = new StackPanel();
            options.Children.Add(new TextBlock
            {
                Text = "Layout",
                Margin = new Thickness(0, 0, 0, 4)
            });
            options.Children.Add(layoutSelector);
            options.Children.Add(buttons);
            options.Children.Add(new TextBlock
            {
                Margin = new Thickness(0, 12, 0, 0),
                Text = "LinedFlowLayout uses ItemTransitionProvider for add, remove, move, and layout transitions. All three layouts retain ItemsRepeater realization and recycling.",
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = 300
            });
            optionsContent = options;
            return root;
        }

        private static GallerySamplePanel CreateSelectionExample(out UIElement optionsContent)
        {
            var items = CreateLayoutItems(18);
            var itemsView = CreateItemsView(items, "SelectionItemsView");
            itemsView.Layout = new Mux.UniformGridLayout
            {
                MinItemWidth = 150,
                MinItemHeight = 92,
                MinRowSpacing = 8,
                MinColumnSpacing = 8
            };
            itemsView.SelectionMode = Mux.ItemsViewSelectionMode.Multiple;
            itemsView.IsItemInvokedEnabled = true;

            var status = new TextBlock
            {
                Margin = new Thickness(0, 10, 0, 0),
                TextWrapping = TextWrapping.Wrap
            };
            AutomationProperties.SetName(status, "Selection status");
            GalleryAutomation.WithAutomationId(
                status,
                GalleryAutomation.SampleElementId("ItemsView", "SelectionStatus"));

            Action updateStatus = delegate
            {
                var labels = itemsView.SelectedItems
                    .OfType<LayoutItem>()
                    .Select(item => item.Label)
                    .ToArray();
                status.Text = labels.Length == 0
                    ? "No items selected."
                    : "Selected: " + string.Join(", ", labels);
            };
            itemsView.SelectionChanged += delegate { updateStatus(); };
            itemsView.ItemInvoked += delegate (Mux.ItemsView sender, Mux.ItemsViewItemInvokedEventArgs args)
            {
                var item = args.InvokedItem as LayoutItem;
                status.Text = "Invoked: " + (item?.Label ?? args.InvokedItem?.ToString());
            };
            updateStatus();

            var root = CreateRoot("SelectionRoot");
            root.Children.Add(itemsView);
            root.Children.Add(status);

            var selectionMode = new ComboBox
            {
                ItemsSource = Enum.GetValues(typeof(Mux.ItemsViewSelectionMode)),
                SelectedItem = itemsView.SelectionMode,
                MinWidth = 180
            };
            AutomationProperties.SetName(selectionMode, "Selection mode");
            GalleryAutomation.WithAutomationId(
                selectionMode,
                GalleryAutomation.SampleElementId("ItemsView", "SelectionMode"));
            selectionMode.SelectionChanged += delegate
            {
                if (selectionMode.SelectedItem is Mux.ItemsViewSelectionMode value)
                {
                    itemsView.SelectionMode = value;
                    updateStatus();
                }
            };

            var invocationEnabled = new CheckBox
            {
                Content = "Enable item invocation",
                IsChecked = itemsView.IsItemInvokedEnabled,
                Margin = new Thickness(0, 12, 0, 0)
            };
            AutomationProperties.SetName(invocationEnabled, "Enable item invocation");
            GalleryAutomation.WithAutomationId(
                invocationEnabled,
                GalleryAutomation.SampleElementId("ItemsView", "InvocationEnabled"));
            invocationEnabled.Checked += delegate { itemsView.IsItemInvokedEnabled = true; };
            invocationEnabled.Unchecked += delegate { itemsView.IsItemInvokedEnabled = false; };

            var selectAll = new Button { Content = "Select all", Margin = new Thickness(0, 12, 0, 0) };
            var clear = new Button { Content = "Clear", Margin = new Thickness(8, 12, 0, 0) };
            var invert = new Button { Content = "Invert", Margin = new Thickness(8, 12, 0, 0) };
            selectAll.Click += delegate { itemsView.SelectAll(); };
            clear.Click += delegate { itemsView.DeselectAll(); };
            invert.Click += delegate { itemsView.InvertSelection(); };
            AutomationProperties.SetName(selectAll, "Select all items");
            AutomationProperties.SetName(clear, "Clear selection");
            AutomationProperties.SetName(invert, "Invert selection");
            GalleryAutomation.WithAutomationId(
                selectAll,
                GalleryAutomation.SampleElementId("ItemsView", "SelectAll"));
            GalleryAutomation.WithAutomationId(
                clear,
                GalleryAutomation.SampleElementId("ItemsView", "ClearSelection"));
            GalleryAutomation.WithAutomationId(
                invert,
                GalleryAutomation.SampleElementId("ItemsView", "InvertSelection"));

            var buttons = new StackPanel { Orientation = Orientation.Horizontal };
            buttons.Children.Add(selectAll);
            buttons.Children.Add(clear);
            buttons.Children.Add(invert);

            var options = new StackPanel();
            options.Children.Add(new TextBlock
            {
                Text = "Selection mode",
                Margin = new Thickness(0, 0, 0, 4)
            });
            options.Children.Add(selectionMode);
            options.Children.Add(invocationEnabled);
            options.Children.Add(buttons);
            optionsContent = options;
            return root;
        }

        private static GallerySamplePanel CreateRoot(string elementName)
        {
            var root = new GallerySamplePanel
            {
                MinWidth = 600,
                Margin = new Thickness(0, 0, 0, 12)
            };
            GalleryAutomation.WithAutomationId(
                root,
                GalleryAutomation.SampleElementId("ItemsView", elementName));
            return root;
        }

        private static Mux.ItemsView CreateItemsView(IReadOnlyList<LayoutItem> items, string elementName)
        {
            var itemsView = new Mux.ItemsView
            {
                Height = 330,
                ItemsSource = items,
                ItemTemplate = CreateItemTemplate(),
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            AutomationProperties.SetName(itemsView, "ItemsView sample collection");
            GalleryAutomation.WithAutomationId(
                itemsView,
                GalleryAutomation.SampleElementId("ItemsView", elementName));
            return itemsView;
        }

        private static DataTemplate CreateItemTemplate()
        {
            return (DataTemplate)XamlReader.Parse(
                @"<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
                                  xmlns:ui='clr-namespace:ModernWpf.Controls;assembly=ModernWpf.Controls'>
                      <ui:ItemContainer HorizontalContentAlignment='Stretch'
                                        AutomationProperties.Name='{Binding Label}'>
                          <Border Height='84' Margin='4' Padding='12,8'
                                  Background='{Binding Brush}' CornerRadius='4'>
                              <TextBlock Text='{Binding Label}' Foreground='White'
                                         FontWeight='SemiBold' VerticalAlignment='Bottom' />
                          </Border>
                      </ui:ItemContainer>
                  </DataTemplate>");
        }

        private static void SetLayout(
            Mux.ItemsView itemsView,
            IReadOnlyList<LayoutItem> items,
            string layoutName)
        {
            itemsView.ItemTransitionProvider = null;
            switch (layoutName)
            {
                case "StackLayout":
                    itemsView.Layout = new Mux.StackLayout { Spacing = 8 };
                    break;
                case "LinedFlowLayout":
                    var linedFlowLayout = new Mux.LinedFlowLayout
                    {
                        LineHeight = 100,
                        MinItemSpacing = 8,
                        LineSpacing = 8,
                        ItemsStretch = Mux.LinedFlowLayoutItemsStretch.Fill
                    };
                    linedFlowLayout.ItemsInfoRequested += delegate (
                        Mux.LinedFlowLayout sender,
                        Mux.LinedFlowLayoutItemsInfoRequestedEventArgs args)
                    {
                        args.SetDesiredAspectRatios(items
                            .Skip(args.ItemsRangeStartIndex)
                            .Take(args.ItemsRangeRequestedLength)
                            .Select(item => item.AspectRatio)
                            .ToArray());
                    };
                    itemsView.Layout = linedFlowLayout;
                    itemsView.ItemTransitionProvider = new Mux.LinedFlowLayoutItemCollectionTransitionProvider();
                    break;
                default:
                    itemsView.Layout = new Mux.UniformGridLayout
                    {
                        MinItemWidth = 150,
                        MinItemHeight = 92,
                        MinRowSpacing = 8,
                        MinColumnSpacing = 8
                    };
                    break;
            }
        }

        private static ObservableCollection<LayoutItem> CreateLayoutItems(int count)
        {
            return new ObservableCollection<LayoutItem>(
                Enumerable.Range(0, count).Select(CreateLayoutItem));
        }

        private static LayoutItem CreateLayoutItem(int index)
        {
            var colors = new[]
            {
                Color.FromRgb(86, 55, 200),
                Color.FromRgb(0, 120, 212),
                Color.FromRgb(16, 124, 16),
                Color.FromRgb(202, 80, 16),
                Color.FromRgb(136, 23, 152),
                Color.FromRgb(0, 134, 117)
            };
            var ratios = new[] { 1.5, 1.0, 1.8, 0.8, 1.25, 1.65, 0.9, 1.4 };
            return new LayoutItem(
                "Item " + (index + 1),
                ratios[index % ratios.Length],
                new SolidColorBrush(colors[index % colors.Length]));
        }

        private sealed class LayoutItem
        {
            internal LayoutItem(string label, double aspectRatio, Brush brush)
            {
                Label = label;
                AspectRatio = aspectRatio;
                Brush = brush;
            }

            public string Label { get; }

            public double AspectRatio { get; }

            public Brush Brush { get; }
        }
    }
}
