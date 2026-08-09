using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using System.Windows.Media;
using Mux = ModernWpf.Controls;

namespace ModernWpf.Gallery.Pages
{
    internal static class ItemsViewFoundationsSampleFactory
    {
        private const string ItemContainerXaml =
@"<ItemContainer IsSelected=""{Binding IsSelected}"">
    <Grid Width=""260"" Height=""112"" Margin=""12"">
        <!-- ItemContainer.Child can be any UIElement. -->
        <TextBlock Text=""Quarterly report"" />
    </Grid>
</ItemContainer>";

        private const string ItemContainerCode =
@"// Standalone ItemContainer exposes a bindable selection state.
// ItemsView owns pointer/keyboard selection when it is used as the item host.
itemContainer.IsSelected = true;";

        private const string ItemsViewBasicXaml =
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

        private const string ItemsViewBasicCode =
@"private void ItemsView_ItemInvoked(
    ItemsView sender,
    ItemsViewItemInvokedEventArgs args)
{
    invocationResult.Text = $""Invoked: {args.InvokedItem}"";
}

// SelectionMode defaults to Single. Enter invokes the current item;
// Space updates selection; double-click invokes in selectable modes.";

        private const string ItemsViewLayoutsXaml =
@"<ItemsView ItemsSource=""{Binding Photos}""
           ItemTemplate=""{StaticResource PhotoItemTemplate}""
           Layout=""{Binding SelectedLayout}"" />";

        private const string ItemsViewLayoutsCode =
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

// WPF adaptation: ItemsView.ScrollView exposes the WPF ScrollViewer while
// ItemsRepeaterScrollHost retains realization-window and anchor behavior.";

        private const string ItemsViewSelectionXaml =
@"<ItemsView ItemsSource=""{Binding Reports}""
           SelectionMode=""Multiple""
           IsItemInvokedEnabled=""True""
           SelectionChanged=""ItemsView_SelectionChanged"" />";

        private const string ItemsViewSelectionCode =
@"itemsView.SelectAll();
itemsView.Deselect(0);
itemsView.InvertSelection();

// None disables user selection, Single selects one item, and Multiple and
// Extended expose multi-selection through UI Automation.";

        private const string LinedFlowLayoutXaml =
@"<ItemsRepeaterScrollHost>
    <ItemsRepeaterScrollHost.ScrollViewer>
        <ScrollViewer Height=""330"" VerticalScrollBarVisibility=""Auto"">
            <ItemsRepeater ItemsSource=""{Binding Photos}""
                           ItemTemplate=""{StaticResource PhotoTemplate}"">
                <ItemsRepeater.Layout>
                    <LinedFlowLayout LineHeight=""104""
                                     MinItemSpacing=""8""
                                     LineSpacing=""8""
                                     ItemsJustification=""Start""
                                     ItemsStretch=""Fill""
                                     ItemsInfoRequested=""Layout_ItemsInfoRequested"" />
                </ItemsRepeater.Layout>
            </ItemsRepeater>
        </ScrollViewer>
    </ItemsRepeaterScrollHost.ScrollViewer>
</ItemsRepeaterScrollHost>";

        private const string LinedFlowLayoutCode =
@"private void Layout_ItemsInfoRequested(
    LinedFlowLayout sender,
    LinedFlowLayoutItemsInfoRequestedEventArgs args)
{
    args.SetDesiredAspectRatios(Photos
        .Skip(args.ItemsRangeStartIndex)
        .Take(args.ItemsRangeRequestedLength)
        .Select(photo => photo.AspectRatio)
        .ToArray());
}

// WPF adaptation: ItemsRepeaterScrollHost supplies the visible and
// realization windows while the nested WPF ScrollViewer performs scrolling.";

        public static UIElement Create(string uniqueId)
        {
            switch (uniqueId)
            {
                case "ItemContainer":
                    return CreateItemContainerExample(out _);
                case "ItemsView":
                    return CreateItemsViewBasicExample(out _);
                case "LinedFlowLayout":
                    return CreateLinedFlowLayoutExample(out _);
                default:
                    return null;
            }
        }

        public static IReadOnlyList<GalleryExample> CreateExamples(string uniqueId)
        {
            switch (uniqueId)
            {
                case "ItemContainer":
                    var itemContainer = CreateItemContainerExample(out var itemContainerOptions);
                    return new[]
                    {
                        new GalleryExample(
                            "An ItemContainer presenting arbitrary content and selection state",
                            itemContainer,
                            ItemContainerXaml,
                            ItemContainerCode,
                            itemContainerOptions)
                    };
                case "ItemsView":
                    var basicItemsView = CreateItemsViewBasicExample(out var basicItemsViewOptions);
                    var layoutsItemsView = CreateItemsViewLayoutsExample(out var layoutsItemsViewOptions);
                    var selectionItemsView = CreateItemsViewSelectionExample(out var selectionItemsViewOptions);
                    return new[]
                    {
                        new GalleryExample(
                            "A selectable collection with item invocation",
                            basicItemsView,
                            ItemsViewBasicXaml,
                            ItemsViewBasicCode,
                            basicItemsViewOptions),
                        new GalleryExample(
                            "Switch an ItemsView between virtualizing layouts",
                            layoutsItemsView,
                            ItemsViewLayoutsXaml,
                            ItemsViewLayoutsCode,
                            layoutsItemsViewOptions),
                        new GalleryExample(
                            "Selection and invocation modes",
                            selectionItemsView,
                            ItemsViewSelectionXaml,
                            ItemsViewSelectionCode,
                            selectionItemsViewOptions)
                    };
                case "LinedFlowLayout":
                    var linedFlowLayout = CreateLinedFlowLayoutExample(out var linedFlowLayoutOptions);
                    return new[]
                    {
                        new GalleryExample(
                            "A virtualized collection arranged in equal-height lines",
                            linedFlowLayout,
                            LinedFlowLayoutXaml,
                            LinedFlowLayoutCode,
                            linedFlowLayoutOptions)
                    };
                default:
                    return Array.Empty<GalleryExample>();
            }
        }

        private static GallerySamplePanel CreateItemContainerExample(out UIElement optionsContent)
        {
            var root = new GallerySamplePanel
            {
                Margin = new Thickness(0, 0, 0, 12)
            };
            GalleryAutomation.WithAutomationId(root, GalleryAutomation.SampleRootId("ItemContainer"));

            var itemContainer = new Mux.ItemContainer
            {
                Width = 340,
                HorizontalAlignment = HorizontalAlignment.Left,
                Child = CreateItemContainerChild(),
                IsSelected = true
            };
            AutomationProperties.SetName(itemContainer, "Quarterly report item");
            GalleryAutomation.WithAutomationId(
                itemContainer,
                GalleryAutomation.SampleElementId("ItemContainer", "PrimaryItem"));
            root.Children.Add(itemContainer);

            var explanation = new TextBlock
            {
                Margin = new Thickness(0, 12, 0, 0),
                Text = "ItemContainer supplies the item chrome and automation semantics used by ItemsView. " +
                    "When used alone, bind or set IsSelected explicitly.",
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = 620,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            root.Children.Add(explanation);

            var selected = new CheckBox
            {
                Content = "Selected",
                IsChecked = itemContainer.IsSelected
            };
            AutomationProperties.SetName(selected, "Selected");
            GalleryAutomation.WithAutomationId(
                selected,
                GalleryAutomation.SampleElementId("ItemContainer", "SelectedOption"));
            selected.Checked += delegate { itemContainer.IsSelected = true; };
            selected.Unchecked += delegate { itemContainer.IsSelected = false; };

            var radiusLabel = new TextBlock
            {
                Text = "Corner radius",
                Margin = new Thickness(0, 16, 0, 4)
            };
            var radius = new Slider
            {
                Minimum = 0,
                Maximum = 16,
                Value = itemContainer.CornerRadius.TopLeft,
                TickFrequency = 2,
                IsSnapToTickEnabled = true,
                Width = 180,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            AutomationProperties.SetName(radius, "Corner radius");
            radius.ValueChanged += delegate
            {
                itemContainer.CornerRadius = new CornerRadius(radius.Value);
            };

            var options = new StackPanel();
            options.Children.Add(selected);
            options.Children.Add(radiusLabel);
            options.Children.Add(radius);
            optionsContent = options;
            return root;
        }

        private static UIElement CreateItemContainerChild()
        {
            var grid = new Grid
            {
                Height = 112,
                Margin = new Thickness(14)
            };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(72) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var preview = new Border
            {
                Width = 64,
                Height = 80,
                CornerRadius = new CornerRadius(4),
                Background = new SolidColorBrush(Color.FromRgb(87, 55, 200)),
                Child = new TextBlock
                {
                    Text = "Q3",
                    Foreground = Brushes.White,
                    FontSize = 22,
                    FontWeight = FontWeights.SemiBold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                }
            };
            grid.Children.Add(preview);

            var text = new StackPanel
            {
                Margin = new Thickness(12, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            text.Children.Add(new TextBlock
            {
                Text = "Quarterly report",
                FontSize = 16,
                FontWeight = FontWeights.SemiBold
            });
            text.Children.Add(new TextBlock
            {
                Text = "Modified today",
                Margin = new Thickness(0, 4, 0, 0),
                Opacity = 0.72
            });
            Grid.SetColumn(text, 1);
            grid.Children.Add(text);
            return grid;
        }

        private static GallerySamplePanel CreateItemsViewBasicExample(out UIElement optionsContent)
        {
            var items = CreateLayoutItems().Take(16).ToArray();
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

            var root = CreateItemsViewRoot("BasicRoot");
            GalleryAutomation.WithAutomationId(root, GalleryAutomation.SampleRootId("ItemsView"));
            root.Children.Add(itemsView);
            root.Children.Add(invocationResult);

            optionsContent = new TextBlock
            {
                Text = "ItemsView defaults to single selection. Use Space to select and Enter or double-click to invoke.",
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = 300
            };
            return root;
        }

        private static GallerySamplePanel CreateItemsViewLayoutsExample(out UIElement optionsContent)
        {
            var items = CreateLayoutItems().Take(80).ToArray();
            var itemsView = CreateItemsView(items, "LayoutItemsView");
            SetItemsViewLayout(itemsView, items, "UniformGridLayout");

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
                    SetItemsViewLayout(itemsView, items, layoutName);
                }
            };

            var root = CreateItemsViewRoot("LayoutsRoot");
            root.Children.Add(itemsView);

            var options = new StackPanel();
            options.Children.Add(new TextBlock
            {
                Text = "Layout",
                Margin = new Thickness(0, 0, 0, 4)
            });
            options.Children.Add(layoutSelector);
            options.Children.Add(new TextBlock
            {
                Margin = new Thickness(0, 12, 0, 0),
                Text = "All three layouts retain ItemsRepeater realization and recycling.",
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = 300
            });
            optionsContent = options;
            return root;
        }

        private static GallerySamplePanel CreateItemsViewSelectionExample(out UIElement optionsContent)
        {
            var items = CreateLayoutItems().Take(18).ToArray();
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

            var root = CreateItemsViewRoot("SelectionRoot");
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

        private static GallerySamplePanel CreateItemsViewRoot(string elementName)
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
                ItemTemplate = CreateItemsViewItemTemplate(),
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            AutomationProperties.SetName(itemsView, "ItemsView sample collection");
            GalleryAutomation.WithAutomationId(
                itemsView,
                GalleryAutomation.SampleElementId("ItemsView", elementName));
            return itemsView;
        }

        private static DataTemplate CreateItemsViewItemTemplate()
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

        private static void SetItemsViewLayout(
            Mux.ItemsView itemsView,
            IReadOnlyList<LayoutItem> items,
            string layoutName)
        {
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

        private static GallerySamplePanel CreateLinedFlowLayoutExample(out UIElement optionsContent)
        {
            var items = CreateLayoutItems();
            var layout = new Mux.LinedFlowLayout
            {
                LineHeight = 104,
                MinItemSpacing = 8,
                LineSpacing = 8,
                ItemsStretch = Mux.LinedFlowLayoutItemsStretch.Fill
            };
            layout.ItemsInfoRequested += delegate (Mux.LinedFlowLayout sender, Mux.LinedFlowLayoutItemsInfoRequestedEventArgs args)
            {
                args.SetDesiredAspectRatios(items
                    .Skip(args.ItemsRangeStartIndex)
                    .Take(args.ItemsRangeRequestedLength)
                    .Select(item => item.AspectRatio)
                    .ToArray());
            };

            var repeater = new Mux.ItemsRepeater
            {
                ItemsSource = items,
                ItemTemplate = CreateLayoutItemTemplate(),
                Layout = layout,
                VerticalCacheLength = 2
            };
            GalleryAutomation.WithAutomationId(
                repeater,
                GalleryAutomation.SampleElementId("LinedFlowLayout", "ItemsRepeater"));

            var scrollViewer = new ScrollViewer
            {
                Height = 330,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Content = repeater
            };
            var scrollHost = new Mux.ItemsRepeaterScrollHost
            {
                ScrollViewer = scrollViewer
            };
            GalleryAutomation.WithAutomationId(
                scrollHost,
                GalleryAutomation.SampleElementId("LinedFlowLayout", "ScrollHost"));

            var root = new GallerySamplePanel
            {
                MinWidth = 600,
                Margin = new Thickness(0, 0, 0, 12)
            };
            GalleryAutomation.WithAutomationId(root, GalleryAutomation.SampleRootId("LinedFlowLayout"));
            root.Children.Add(scrollHost);

            var justification = new ComboBox
            {
                ItemsSource = Enum.GetValues(typeof(Mux.LinedFlowLayoutItemsJustification)),
                SelectedItem = layout.ItemsJustification,
                MinWidth = 180
            };
            AutomationProperties.SetName(justification, "Items justification");
            justification.SelectionChanged += delegate
            {
                if (justification.SelectedItem is Mux.LinedFlowLayoutItemsJustification value)
                {
                    layout.ItemsJustification = value;
                }
            };

            var fill = new CheckBox
            {
                Content = "Stretch items to fill each line",
                IsChecked = true,
                Margin = new Thickness(0, 14, 0, 0)
            };
            AutomationProperties.SetName(fill, "Stretch items to fill each line");
            fill.Checked += delegate { layout.ItemsStretch = Mux.LinedFlowLayoutItemsStretch.Fill; };
            fill.Unchecked += delegate { layout.ItemsStretch = Mux.LinedFlowLayoutItemsStretch.None; };

            var lineHeightLabel = new TextBlock
            {
                Text = "Line height",
                Margin = new Thickness(0, 14, 0, 4)
            };
            var lineHeight = new Slider
            {
                Minimum = 72,
                Maximum = 148,
                Value = layout.LineHeight,
                TickFrequency = 4,
                IsSnapToTickEnabled = true,
                Width = 200,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            AutomationProperties.SetName(lineHeight, "Line height");
            lineHeight.ValueChanged += delegate { layout.LineHeight = lineHeight.Value; };

            var options = new StackPanel();
            options.Children.Add(new TextBlock
            {
                Text = "Items justification",
                Margin = new Thickness(0, 0, 0, 4)
            });
            options.Children.Add(justification);
            options.Children.Add(fill);
            options.Children.Add(lineHeightLabel);
            options.Children.Add(lineHeight);
            optionsContent = options;
            return root;
        }

        private static IReadOnlyList<LayoutItem> CreateLayoutItems()
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
            return Enumerable.Range(0, 80)
                .Select(index => new LayoutItem(
                    "Item " + (index + 1),
                    ratios[index % ratios.Length],
                    new SolidColorBrush(colors[index % colors.Length])))
                .ToArray();
        }

        private static DataTemplate CreateLayoutItemTemplate()
        {
            return (DataTemplate)XamlReader.Parse(
                @"<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>
                      <Border Margin='0' Background='{Binding Brush}' CornerRadius='4'>
                          <TextBlock Margin='10' Text='{Binding Label}' Foreground='White'
                                     FontWeight='SemiBold' VerticalAlignment='Bottom' />
                      </Border>
                  </DataTemplate>");
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
