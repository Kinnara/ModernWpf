using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using ModernWpf.Gallery.Models;
using Mux = ModernWpf.Controls;

namespace ModernWpf.Gallery.Pages
{
    internal static class CollectionsSampleFactory
    {
        private const string PullToRefreshBasicXaml =
@"<RefreshContainer x:Name=""rc"" RefreshRequested=""rc_RefreshRequested"">
    <ListView x:Name=""lv"" Width=""300"" Height=""300"" BorderThickness=""1"" BorderBrush=""Black""/>
</RefreshContainer>";

        private const string PullToRefreshBasicCSharp =
@"ObservableCollection<string> items = new ObservableCollection<string>();
listview.ItemsSource = items;

private void rc_RefreshRequested(RefreshContainer sender, RefreshRequestedEventArgs args)
{
    //Do some work to show new Content! Once the work is done, call RefreshCompletionDeferral.Complete()
    this.RefreshCompletionDeferral = args.GetDeferral();
    this.DoWork();
}

private void WorkCompleted()
{
    items.Insert(0, ""NewControl"");
    if (this.RefreshCompletionDeferral != null)
    {
        this.RefreshCompletionDeferral.Complete();
        this.RefreshCompletionDeferral.Dispose();
        this.RefreshCompletionDeferral = null;
    }
}";

        private const string PullToRefreshCustomIconXaml =
@"<RefreshContainer x:Name=""rc"" RefreshRequested=""rc_RefreshRequested"">
    <RefreshContainer.Visualizer>
        <RefreshVisualizer RefreshStateChanged=""rv2_RefreshStateChanged"">
            <RefreshVisualizer.Content>
                <SymbolIcon Symbol=""AddFriend""/>
            </RefreshVisualizer.Content>
        </RefreshVisualizer>
    </RefreshContainer.Visualizer>
    <ListView x:Name=""lv"" Width=""300"" Height=""300"" BorderThickness=""1"" BorderBrush=""Black""/>
</RefreshContainer>";

        private const string PullToRefreshCustomIconCSharp =
@"ObservableCollection<string> items = new ObservableCollection<string>();
listview.ItemsSource = items;

private void rc_RefreshRequested(RefreshContainer sender, RefreshRequestedEventArgs args)
{
    //Do some work to show new Content! Once the work is done, call RefreshCompletionDeferral.Complete()
    this.RefreshCompletionDeferral = args.GetDeferral();
    this.DoWork();
}

private void WorkCompleted()
{
    items.Insert(0, ""NewControl"");
    if (this.RefreshCompletionDeferral != null)
    {
        this.RefreshCompletionDeferral.Complete();
        this.RefreshCompletionDeferral.Dispose();
        this.RefreshCompletionDeferral = null;
    }
}
private void rv2_RefreshStateChanged()
{
    var visualizerContentVisual = ElementCompositionPreview.GetElementVisual(rv2.Content);
    visualizerContentVisual.StopAnimation(""RotationAngle"");
}";

        private const string GridViewLayoutCustomizationXaml =
@"<!-- The GridView used for this example is shown below. Setter properties are used to customize
some parts of the GridViewItems (i.e. the margins). -->
<GridView
    x:Name=""StyledGrid""
    ItemTemplate=""{StaticResource ImageOverlayTemplate}"">

    <GridView.ItemContainerStyle>
        <Style TargetType=""GridViewItem"" BasedOn=""{StaticResource DefaultGridViewItemStyle}"">
            <Setter Property=""Margin"" Value=""$(ColMargin), $(RowMargin), $(ColMargin), $(RowMargin)""/>
        </Style>
    </GridView.ItemContainerStyle>

    <!-- An extra property also needs to be added to the GridView's ItemsWrapGrid.
    The following sets the maximum # of items to show before wrapping.-->
    <GridView.ItemsPanel>
        <ItemsPanelTemplate>
            <ItemsWrapGrid x:Name=""MaxItemsWrapGrid""
                           MaximumRowsOrColumns=""$(MaxItems)""
                           Orientation=""Horizontal""/>
        </ItemsPanelTemplate>
    </GridView.ItemsPanel>
</GridView>

<!-- In this example, the GridView's ItemTemplate property is bound to a data template (shown below)
called ImageOverlayTemplate, defined in the Page.Resources section of the XAML file.

The data template is defined to display a CustomDataObject object (same type as in above sample). -->

<DataTemplate x:Name=""ImageOverlayTemplate"" x:DataType=""local:CustomDataObject"">
    <Grid Width=""100"">
        <Image Source=""{x:Bind ImageLocation}"" Stretch=""UniformToFill""/>
        <StackPanel Orientation=""Vertical"" Height=""40"" VerticalAlignment=""Bottom"" Padding=""5,1,5,1""
                    Background=""LightGray"" Opacity="".75"">
            <TextBlock Text=""{x:Bind Title}""/>
            <StackPanel Orientation=""Horizontal"">
                <TextBlock Text=""{x:Bind Likes}"" Style=""{ThemeResource CaptionTextBlockStyle}""
                           Foreground=""{ThemeResource SystemControlPageTextBaseMediumBrush}""/>
                <TextBlock Text="" Likes"" Style=""{ThemeResource CaptionTextBlockStyle}""
                           Foreground=""{ThemeResource SystemControlPageTextBaseMediumBrush}""/>
            </StackPanel>
        </StackPanel>
    </Grid>
</DataTemplate>";

        private const string GridViewContentXaml =
@"<!-- The GridView used for this sample is shown below, with all of the necessary added properties. -->
<GridView
    x:Name=""ContentGridView""
    ItemsSource=""{x:Bind Items}""
    ItemTemplate=""{StaticResource $(ItemTemplate)}""
    IsItemClickEnabled=""$(IsItemClickEnabled)""
    CanDragItems=""$(CanDragItems)""
    AllowDrop=""$(CanDropItems)""
    CanReorderItems=""$(CanReorderItems)""
    SelectionMode=""$(SelectionMode)""
    SelectionChanged=""ContentGridView_SelectionChanged""
    ItemClick=""ContentGridView_ItemClick""
    FlowDirection=""$(FlowDirection)""/>

<!-- ContentGridView_SelectionChanged and ContentGridView_ItemClick are functions defined in the code-behind
to handle the events of when a selection changes on the GridView and when an item is clicked. -->

<!-- The data template bound to this GridView's ItemTemplate property is based on which one you
select from the options on the right. -->";

        public static IReadOnlyList<GalleryExample> CreateExamples(string uniqueId, IReadOnlyList<SampleSnippet> sampleSnippets)
        {
            switch (uniqueId)
            {
                case "GridView":
                    return CreateGridViewExamples(sampleSnippets);
                case "PullToRefresh":
                    return CreatePullToRefreshExamples();
                default:
                    return Array.Empty<GalleryExample>();
            }
        }

        public static UIElement Create(string uniqueId)
        {
            switch (uniqueId)
            {
                case "DataGrid":
                    return CreateDataGridSample();
                case "FlipView":
                    return CreateFlipViewSample();
                case "GridView":
                    return CreateGridViewSample();
                case "ItemsRepeater":
                    return CreateItemsRepeaterSample();
                case "ItemsView":
                    return CreateItemsViewSample();
                case "ListBox":
                    return CreateListBoxSample();
                case "ListView":
                    return CreateListViewSample();
                case "PullToRefresh":
                    return CreatePullToRefreshSample();
                case "TreeView":
                    return CreateTreeViewSample();
                default:
                    return null;
            }
        }

        private static UIElement CreateDataGridSample()
        {
            var panel = CreateSamplePanel("DataGrid presents editable rows and columns with selection, sorting, and generated or explicit columns.");
            var grid = new DataGrid
            {
                Width = 540,
                Height = 190,
                AutoGenerateColumns = false,
                CanUserAddRows = false,
                ItemsSource = CreatePeople()
            };
            grid.Columns.Add(new DataGridTextColumn { Header = "Name", Binding = new Binding("Name"), Width = 190 });
            grid.Columns.Add(new DataGridTextColumn { Header = "Role", Binding = new Binding("Role"), Width = 160 });
            grid.Columns.Add(new DataGridTextColumn { Header = "Status", Binding = new Binding("Status"), Width = 120 });
            panel.Children.Add(grid);
            return panel;
        }

        private static UIElement CreateFlipViewSample()
        {
            var panel = CreateSamplePanel("FlipView maps to explicit previous and next navigation because ModernWpf no longer carries the legacy MahApps FlipView adapter.");
            var items = new[] { "Featured", "Recent", "Recommended" };
            var index = 0;
            var content = CreateCollectionCard(items[index]);
            var output = CreateOutput("Item 1 of 3");

            var row = new StackPanel { Orientation = Orientation.Horizontal };
            var previous = CreateButton("Previous");
            var next = CreateButton("Next");
            previous.Click += delegate
            {
                index = (index + items.Length - 1) % items.Length;
                content.Child = CreateCardText(items[index]);
                output.Text = "Item " + (index + 1) + " of " + items.Length;
            };
            next.Click += delegate
            {
                index = (index + 1) % items.Length;
                content.Child = CreateCardText(items[index]);
                output.Text = "Item " + (index + 1) + " of " + items.Length;
            };
            row.Children.Add(previous);
            row.Children.Add(next);

            panel.Children.Add(content);
            panel.Children.Add(row);
            panel.Children.Add(output);
            return panel;
        }

        private static UIElement CreateGridViewSample()
        {
            var root = new GallerySamplePanel
            {
                Margin = new Thickness(0, 0, 0, 12)
            };
            GalleryAutomation.WithAutomationId(root, GalleryAutomation.SampleRootId("GridView"));
            root.Children.Add(CreateBasicGridViewExampleContent(assignRootAutomationId: false));
            return root;
        }

        private static IReadOnlyList<GalleryExample> CreateGridViewExamples(IReadOnlyList<SampleSnippet> sampleSnippets)
        {
            return new[]
            {
                new GalleryExample(
                    "Basic GridView with Simple DataTemplate",
                    CreateBasicGridViewExampleContent(assignRootAutomationId: true),
                    FindSampleCodeText(sampleSnippets, "GridView/GridViewSample1_xaml.txt"),
                    FindSampleCodeText(sampleSnippets, "GridView/GridViewSample1_cs.txt")),
                new GalleryExample(
                    "GridView with Layout Customization",
                    CreateStyledGridViewExampleContent(),
                    GridViewLayoutCustomizationXaml,
                    null),
                new GalleryExample(
                    "Content inside of a GridView.",
                    CreateContentGridViewExampleContent(),
                    GridViewContentXaml,
                    null)
            };
        }

        private static GallerySamplePanel CreateBasicGridViewExampleContent(bool assignRootAutomationId)
        {
            var root = CreateGridViewExampleRoot(assignRootAutomationId);
            root.Children.Add(new TextBlock
            {
                Text = "This is a basic GridView that has the full source code below.\r\nOther samples on this page display only the additional markup needed to customize the specific GridView.",
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 15)
            });

            var output = CreateOutput("");
            output.Name = "ClickOutput0";

            var gridView = CreateSourceGridView("BasicGridView", CreateGridViewImageTemplate(), CreateGridViewItems());
            gridView.Width = 657;
            gridView.MaxHeight = double.PositiveInfinity;
            gridView.HorizontalAlignment = HorizontalAlignment.Left;
            gridView.ItemsPanel = CreateTopAlignedWrapItemsPanel();
            gridView.IsItemClickEnabled = true;
            gridView.SelectionMode = SelectionMode.Single;
            gridView.ItemClick += delegate(object sender, Mux.ItemClickEventArgs args)
            {
                var clickedItem = args.ClickedItem as GridViewDataItem;
                if (clickedItem != null)
                {
                    output.Text = "You clicked " + clickedItem.Title + ".";
                }
            };
            GalleryAutomation.WithAutomationId(gridView, GalleryAutomation.SampleElementId("GridView", "BasicGridView"));

            root.Children.Add(gridView);
            root.Children.Add(output);
            return root;
        }

        private static GallerySamplePanel CreateStyledGridViewExampleContent()
        {
            var root = CreateGridViewExampleRoot(assignRootAutomationId: false);
            root.Children.Add(new TextBlock
            {
                Text = "Use the options on the right to control different layout customizations to the GridView below.",
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 15)
            });

            var layout = CreateGridViewOptionsLayout();
            var sampleColumn = new StackPanel();
            layout.Children.Add(sampleColumn);

            Mux.ItemsWrapGrid styledGridWrapPanel = null;
            var styledGrid = CreateSourceGridView("StyledGrid", CreateGridViewImageOverlayTemplate(), CreateGridViewItems());
            styledGrid.ItemContainerStyle = CreateGridViewItemMarginStyle(5, 5);
            styledGrid.ItemsPanel = CreateGridViewItemsWrapPanel(3, delegate(Mux.ItemsWrapGrid panel)
            {
                styledGridWrapPanel = panel;
            });
            sampleColumn.Children.Add(styledGrid);

            var options = new StackPanel
            {
                Width = 250
            };
            Grid.SetColumn(options, 2);

            var columnSpace = CreateGridViewNumberBox("ColumnSpace", "Space between columns", 0, 100, 5);
            var rowSpace = CreateGridViewNumberBox("RowSpace", "Space between rows", 0, 100, 5);
            var wrapItemCount = CreateGridViewNumberBox("WrapItemCount", "Maximum number of items before wrapping", 1, 8, 3);

            ModernWpf.TypedEventHandler<Mux.NumberBox, Mux.NumberBoxValueChangedEventArgs> updateGridViewLayout =
                delegate(Mux.NumberBox sender, Mux.NumberBoxValueChangedEventArgs args)
                {
                    if (sender == wrapItemCount)
                    {
                        if (styledGridWrapPanel != null)
                        {
                            styledGridWrapPanel.MaximumRowsOrColumns = (int)wrapItemCount.Value;
                        }

                        return;
                    }

                    UpdateGridViewItemMargins(styledGrid, (int)columnSpace.Value, (int)rowSpace.Value);
                };

            columnSpace.ValueChanged += updateGridViewLayout;
            rowSpace.ValueChanged += updateGridViewLayout;
            wrapItemCount.ValueChanged += updateGridViewLayout;
            options.Children.Add(columnSpace);
            options.Children.Add(rowSpace);
            options.Children.Add(wrapItemCount);
            layout.Children.Add(options);

            root.Children.Add(layout);
            return root;
        }

        private static GallerySamplePanel CreateContentGridViewExampleContent()
        {
            var root = CreateGridViewExampleRoot(assignRootAutomationId: false);
            var layout = CreateGridViewOptionsLayout();

            var sampleGrid = new Grid();
            sampleGrid.RowDefinitions.Add(new RowDefinition());
            sampleGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var clickOutput = CreateOutput("");
            clickOutput.Name = "ClickOutput";
            var selectionOutput = CreateOutput("");
            selectionOutput.Name = "SelectionOutput";

            var contentGridView = CreateSourceGridView("ContentGridView", CreateGridViewImageTemplate(), CreateGridViewItems());
            contentGridView.FlowDirection = FlowDirection.LeftToRight;
            contentGridView.SelectionMode = SelectionMode.Single;
            contentGridView.SelectionChanged += delegate(object sender, SelectionChangedEventArgs args)
            {
                selectionOutput.Text = "You have selected " + contentGridView.SelectedItems.Count + " item(s).";
            };
            contentGridView.ItemClick += delegate(object sender, Mux.ItemClickEventArgs args)
            {
                var clickedItem = args.ClickedItem as GridViewDataItem;
                if (clickedItem != null)
                {
                    clickOutput.Text = "You clicked " + clickedItem.Title + ".";
                }
            };
            sampleGrid.Children.Add(contentGridView);

            var outputs = new StackPanel();
            outputs.Children.Add(clickOutput);
            outputs.Children.Add(selectionOutput);
            Grid.SetRow(outputs, 1);
            sampleGrid.Children.Add(outputs);
            layout.Children.Add(sampleGrid);

            var options = CreateContentGridViewOptions(contentGridView, clickOutput, selectionOutput);
            Grid.SetColumn(options, 2);
            layout.Children.Add(options);

            root.Children.Add(layout);
            return root;
        }

        private static GallerySamplePanel CreateGridViewExampleRoot(bool assignRootAutomationId)
        {
            var root = new GallerySamplePanel
            {
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            if (assignRootAutomationId)
            {
                GalleryAutomation.WithAutomationId(root, GalleryAutomation.SampleRootId("GridView"));
            }

            return root;
        }

        private static Grid CreateGridViewOptionsLayout()
        {
            var layout = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(250) });
            return layout;
        }

        private static Mux.GridView CreateSourceGridView(string name, DataTemplate itemTemplate, ObservableCollection<GridViewDataItem> items)
        {
            return new Mux.GridView
            {
                Name = name,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                MaxHeight = 340,
                ItemTemplate = itemTemplate,
                ItemsSource = items
            };
        }

        private static StackPanel CreateContentGridViewOptions(
            Mux.GridView contentGridView,
            TextBlock clickOutput,
            TextBlock selectionOutput)
        {
            var options = new StackPanel
            {
                Name = "Control2",
                Width = 250
            };

            var templateOptions = new Mux.RadioButtons
            {
                Header = "ItemTemplate",
                SelectedIndex = 0
            };
            templateOptions.Items.Add("Image");
            templateOptions.Items.Add("Icon/Text");
            templateOptions.Items.Add("Image/Text");
            templateOptions.Items.Add("Text");
            templateOptions.SelectionChanged += delegate
            {
                switch (templateOptions.SelectedIndex)
                {
                    case 1:
                        contentGridView.ItemTemplate = CreateGridViewIconTextTemplate();
                        break;
                    case 2:
                        contentGridView.ItemTemplate = CreateGridViewImageTextTemplate();
                        break;
                    case 3:
                        contentGridView.ItemTemplate = CreateGridViewTextTemplate();
                        break;
                    default:
                        contentGridView.ItemTemplate = CreateGridViewImageTemplate();
                        break;
                }
            };
            options.Children.Add(templateOptions);

            var reverseFlow = new ToggleButton
            {
                Content = "Reverse FlowDirection",
                Margin = new Thickness(0, 8, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            reverseFlow.Click += delegate
            {
                contentGridView.FlowDirection = contentGridView.FlowDirection == FlowDirection.LeftToRight
                    ? FlowDirection.RightToLeft
                    : FlowDirection.LeftToRight;
            };
            options.Children.Add(reverseFlow);

            options.Children.Add(new TextBlock
            {
                Text = "GridView Properties",
                Margin = new Thickness(0, 18, 0, 10)
            });
            options.Children.Add(new TextBlock
            {
                MaxWidth = 150,
                FontSize = 13,
                TextWrapping = TextWrapping.Wrap,
                Text = "In order to drag, drop, and reorder items within the GridView, make sure the last three boxes are checked below."
            });
            options.Children.Add(new TextBlock
            {
                MaxWidth = 150,
                FontSize = 13,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 8, 0, 0),
                Text = "Turning on IsItemClickEnabled will allow the user to click on an item (and print output below the GridView), regardless of selection mode."
            });

            var itemClickCheckBox = CreateGridViewOptionCheckBox("ItemClickCheckBox", "IsItemClickEnabled");
            itemClickCheckBox.Click += delegate
            {
                contentGridView.IsItemClickEnabled = itemClickCheckBox.IsChecked == true;
                clickOutput.Text = string.Empty;
            };
            options.Children.Add(itemClickCheckBox);

            options.Children.Add(CreateGridViewOptionCheckBox("DragCheckBox", "CanDragItems"));
            options.Children.Add(CreateGridViewOptionCheckBox("ReorderCheckBox", "CanReorderItems"));

            var dropCheckBox = CreateGridViewOptionCheckBox("DropCheckBox", "AllowDrop");
            dropCheckBox.Click += delegate { contentGridView.AllowDrop = dropCheckBox.IsChecked == true; };
            options.Children.Add(dropCheckBox);

            var selectionMode = new ComboBox
            {
                Name = "SelectionModeComboBox",
                Margin = new Thickness(0, 12, 0, 0)
            };
            selectionMode.Items.Add("None");
            selectionMode.Items.Add("Single");
            selectionMode.Items.Add("Multiple");
            selectionMode.Items.Add("Extended");
            selectionMode.SelectionChanged += delegate
            {
                if (contentGridView == null)
                {
                    return;
                }

                var selectedMode = selectionMode.SelectedItem as string;
                switch (selectedMode)
                {
                    case "None":
                        contentGridView.IsSelectionEnabled = false;
                        selectionOutput.Text = string.Empty;
                        break;
                    case "Multiple":
                        contentGridView.IsSelectionEnabled = true;
                        contentGridView.SelectionMode = SelectionMode.Multiple;
                        break;
                    case "Extended":
                        contentGridView.IsSelectionEnabled = true;
                        contentGridView.SelectionMode = SelectionMode.Extended;
                        break;
                    default:
                        contentGridView.IsSelectionEnabled = true;
                        contentGridView.SelectionMode = SelectionMode.Single;
                        break;
                }
            };
            selectionMode.SelectedIndex = 1;

            var selectionModeLabel = new TextBlock
            {
                Text = "SelectionMode",
                Margin = new Thickness(0, 12, 0, 4)
            };
            options.Children.Add(selectionModeLabel);
            options.Children.Add(selectionMode);
            return options;
        }

        private static CheckBox CreateGridViewOptionCheckBox(string name, string content)
        {
            return new CheckBox
            {
                Name = name,
                Content = content
            };
        }

        private static Mux.NumberBox CreateGridViewNumberBox(string name, string header, double minimum, double maximum, double value)
        {
            var numberBox = new Mux.NumberBox
            {
                Name = name,
                Header = header,
                Minimum = minimum,
                Maximum = maximum,
                Value = value,
                SmallChange = 1,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                MaxWidth = 250,
                Margin = new Thickness(0, 0, 0, 16),
                SpinButtonPlacementMode = Mux.NumberBoxSpinButtonPlacementMode.Inline
            };
            AutomationProperties.SetName(numberBox, header);
            return numberBox;
        }

        private static Style CreateGridViewItemMarginStyle(int columnSpace, int rowSpace)
        {
            var style = new Style(typeof(Mux.GridViewItem));
            style.Setters.Add(new Setter(FrameworkElement.MarginProperty, new Thickness(columnSpace, rowSpace, columnSpace, rowSpace)));
            return style;
        }

        private static ItemsPanelTemplate CreateGridViewItemsWrapPanel(int maximumRowsOrColumns, Action<Mux.ItemsWrapGrid> loaded)
        {
            var factory = new FrameworkElementFactory(typeof(Mux.ItemsWrapGrid));
            factory.SetValue(FrameworkElement.NameProperty, "MaxItemsWrapGrid");
            factory.SetValue(Mux.ItemsWrapGrid.MaximumRowsOrColumnsProperty, maximumRowsOrColumns);
            factory.SetValue(Mux.ItemsWrapGrid.OrientationProperty, Orientation.Horizontal);
            factory.AddHandler(FrameworkElement.LoadedEvent, new RoutedEventHandler(delegate(object sender, RoutedEventArgs args)
            {
                var panel = sender as Mux.ItemsWrapGrid;
                if (panel != null)
                {
                    loaded(panel);
                }
            }));
            return new ItemsPanelTemplate(factory);
        }

        private static void UpdateGridViewItemMargins(Mux.GridView gridView, int columnSpace, int rowSpace)
        {
            var margin = new Thickness(columnSpace, rowSpace, columnSpace, rowSpace);
            gridView.ItemContainerStyle = CreateGridViewItemMarginStyle(columnSpace, rowSpace);
            gridView.UpdateLayout();

            for (var i = 0; i < gridView.Items.Count; i++)
            {
                var item = gridView.ItemContainerGenerator.ContainerFromIndex(i) as Mux.GridViewItem;
                if (item != null)
                {
                    item.Margin = margin;
                }
            }
        }

        private static ObservableCollection<GridViewDataItem> CreateGridViewItems()
        {
            return new ObservableCollection<GridViewDataItem>(new[]
            {
                CreateGridViewItem(1, "125", "12"),
                CreateGridViewItem(2, "356", "45"),
                CreateGridViewItem(3, "267", "31"),
                CreateGridViewItem(4, "842", "68"),
                CreateGridViewItem(5, "421", "39"),
                CreateGridViewItem(6, "795", "72"),
                CreateGridViewItem(7, "642", "55"),
                CreateGridViewItem(8, "910", "84")
            });
        }

        private static GridViewDataItem CreateGridViewItem(int index, string views, string likes)
        {
            return new GridViewDataItem
            {
                Title = "Item " + index,
                ImageLocation = "/Assets/SampleMedia/LandscapeImage" + index + ".jpg",
                ImageSource = new BitmapImage(new Uri(ResourceUri("Assets/SampleMedia/LandscapeImage" + index + ".jpg"), UriKind.Absolute)),
                Views = views,
                Likes = likes,
                Description = GridViewDescriptions[(index - 1) % GridViewDescriptions.Length]
            };
        }

        private static DataTemplate CreateGridViewImageTemplate()
        {
            var image = new FrameworkElementFactory(typeof(Image));
            image.SetValue(FrameworkElement.WidthProperty, 190.0);
            image.SetValue(FrameworkElement.HeightProperty, 130.0);
            image.SetValue(Image.StretchProperty, Stretch.UniformToFill);
            image.SetBinding(Image.SourceProperty, new Binding("ImageSource"));
            image.SetBinding(AutomationProperties.NameProperty, new Binding("Title"));
            return new DataTemplate { VisualTree = image };
        }

        private static DataTemplate CreateGridViewIconTextTemplate()
        {
            var root = new FrameworkElementFactory(typeof(Grid));
            root.SetValue(FrameworkElement.WidthProperty, 280.0);
            root.SetValue(FrameworkElement.MinHeightProperty, 160.0);
            root.SetBinding(AutomationProperties.NameProperty, new Binding("Title"));

            var image = new FrameworkElementFactory(typeof(Image));
            image.SetValue(FrameworkElement.WidthProperty, 18.0);
            image.SetValue(FrameworkElement.HeightProperty, 18.0);
            image.SetValue(FrameworkElement.MarginProperty, new Thickness(0, 4, 0, 0));
            image.SetValue(Image.StretchProperty, Stretch.Uniform);
            image.SetBinding(Image.SourceProperty, new Binding("ImageSource"));
            root.AppendChild(image);

            var title = new FrameworkElementFactory(typeof(TextBlock));
            title.SetValue(FrameworkElement.MarginProperty, new Thickness(26, 0, 0, 0));
            title.SetResourceReference(FrameworkElement.StyleProperty, "BaseTextBlockStyle");
            title.SetBinding(TextBlock.TextProperty, new Binding("Title"));
            root.AppendChild(title);

            var description = new FrameworkElementFactory(typeof(TextBlock));
            description.SetValue(FrameworkElement.MarginProperty, new Thickness(0, 28, 8, 0));
            description.SetValue(TextBlock.TextWrappingProperty, TextWrapping.Wrap);
            description.SetValue(TextBlock.TextTrimmingProperty, TextTrimming.WordEllipsis);
            description.SetResourceReference(FrameworkElement.StyleProperty, "CaptionTextBlockStyle");
            description.SetBinding(TextBlock.TextProperty, new Binding("Description"));
            root.AppendChild(description);

            return new DataTemplate { VisualTree = root };
        }

        private static DataTemplate CreateGridViewImageTextTemplate()
        {
            var grid = new FrameworkElementFactory(typeof(Grid));
            grid.SetValue(FrameworkElement.WidthProperty, 280.0);
            grid.SetBinding(AutomationProperties.NameProperty, new Binding("Title"));

            var image = new FrameworkElementFactory(typeof(Image));
            image.SetValue(FrameworkElement.WidthProperty, 112.0);
            image.SetValue(FrameworkElement.HeightProperty, 100.0);
            image.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Top);
            image.SetValue(Image.StretchProperty, Stretch.Fill);
            image.SetBinding(Image.SourceProperty, new Binding("ImageSource"));
            grid.AppendChild(image);

            var textStack = new FrameworkElementFactory(typeof(StackPanel));
            textStack.SetValue(FrameworkElement.MarginProperty, new Thickness(120, 0, 0, 8));
            var title = new FrameworkElementFactory(typeof(TextBlock));
            title.SetValue(FrameworkElement.MarginProperty, new Thickness(0, 0, 0, 8));
            title.SetResourceReference(FrameworkElement.StyleProperty, "SubtitleTextBlockStyle");
            title.SetBinding(TextBlock.TextProperty, new Binding("Title"));
            textStack.AppendChild(title);
            textStack.AppendChild(CreateGridViewMetricRow("Views", " Views "));
            textStack.AppendChild(CreateGridViewMetricRow("Likes", " Likes"));
            grid.AppendChild(textStack);

            return new DataTemplate { VisualTree = grid };
        }

        private static FrameworkElementFactory CreateGridViewMetricRow(string bindingPath, string suffix)
        {
            var row = new FrameworkElementFactory(typeof(StackPanel));
            row.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);

            var value = new FrameworkElementFactory(typeof(TextBlock));
            value.SetResourceReference(FrameworkElement.StyleProperty, "CaptionTextBlockStyle");
            value.SetBinding(TextBlock.TextProperty, new Binding(bindingPath));
            row.AppendChild(value);

            var label = new FrameworkElementFactory(typeof(TextBlock));
            label.SetResourceReference(FrameworkElement.StyleProperty, "CaptionTextBlockStyle");
            label.SetValue(TextBlock.TextProperty, suffix);
            row.AppendChild(label);

            return row;
        }

        private static DataTemplate CreateGridViewTextTemplate()
        {
            var stack = new FrameworkElementFactory(typeof(StackPanel));
            stack.SetValue(FrameworkElement.WidthProperty, 240.0);
            stack.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);

            var title = new FrameworkElementFactory(typeof(TextBlock));
            title.SetValue(FrameworkElement.MarginProperty, new Thickness(8, 0, 0, 0));
            title.SetResourceReference(FrameworkElement.StyleProperty, "TitleTextBlockStyle");
            title.SetBinding(TextBlock.TextProperty, new Binding("Title"));
            stack.AppendChild(title);

            return new DataTemplate { VisualTree = stack };
        }

        private static DataTemplate CreateGridViewImageOverlayTemplate()
        {
            var grid = new FrameworkElementFactory(typeof(Grid));
            grid.SetValue(FrameworkElement.WidthProperty, 100.0);
            grid.SetBinding(AutomationProperties.NameProperty, new Binding("Title"));

            var image = new FrameworkElementFactory(typeof(Image));
            image.SetValue(Image.StretchProperty, Stretch.UniformToFill);
            image.SetBinding(Image.SourceProperty, new Binding("ImageSource"));
            grid.AppendChild(image);

            var overlay = new FrameworkElementFactory(typeof(Border));
            overlay.SetValue(FrameworkElement.HeightProperty, 40.0);
            overlay.SetValue(Border.PaddingProperty, new Thickness(5, 1, 5, 1));
            overlay.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Bottom);
            overlay.SetValue(UIElement.OpacityProperty, 0.75);
            overlay.SetResourceReference(Border.BackgroundProperty, "SystemControlBackgroundBaseMediumBrush");

            var overlayContent = new FrameworkElementFactory(typeof(StackPanel));
            overlayContent.SetValue(StackPanel.OrientationProperty, Orientation.Vertical);

            var title = new FrameworkElementFactory(typeof(TextBlock));
            title.SetBinding(TextBlock.TextProperty, new Binding("Title"));
            title.SetResourceReference(TextBlock.ForegroundProperty, "SystemControlForegroundAltHighBrush");
            overlayContent.AppendChild(title);
            overlayContent.AppendChild(CreateGridViewMetricRow("Likes", " Likes"));
            overlay.AppendChild(overlayContent);
            grid.AppendChild(overlay);

            return new DataTemplate { VisualTree = grid };
        }

        private static readonly string[] GridViewDescriptions =
        {
            "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Integer id facilisis lectus.",
            "Nullam eget mattis metus. Donec pharetra, tellus in mattis tincidunt, magna ipsum gravida nibh.",
            "Quisque accumsan pretium ligula in faucibus. Mauris sollicitudin augue vitae lorem cursus.",
            "Aenean in nisl at elit venenatis blandit ut vitae lectus. Praesent in sollicitudin nunc.",
            "Ut consequat magna luctus justo egestas vehicula. Integer pharetra risus libero.",
            "Proin malesuada, libero vitae aliquam venenatis, diam est faucibus felis.",
            "Aenean vulputate, turpis non tincidunt ornare, metus est sagittis erat.",
            "Duis facilisis, quam ut laoreet commodo, elit ex aliquet massa."
        };

        private sealed class GridViewDataItem
        {
            public string Title { get; set; }
            public string ImageLocation { get; set; }
            public BitmapImage ImageSource { get; set; }
            public string Views { get; set; }
            public string Likes { get; set; }
            public string Description { get; set; }
        }

        private static UIElement CreateItemsRepeaterSample()
        {
            var panel = CreateSamplePanel("ItemsRepeater renders repeated content from a data source with a reusable item template.");
            var repeater = new Mux.ItemsRepeater
            {
                ItemsSource = new[] { "Inbox", "Archive", "Drafts", "Sent", "Deleted" },
                ItemTemplate = CreateRepeaterTemplate()
            };
            panel.Children.Add(new Mux.ItemsRepeaterScrollHost
            {
                Height = 190,
                Width = 320,
                ScrollViewer = new ScrollViewer
                {
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    Content = repeater
                }
            });
            return panel;
        }

        private static UIElement CreateItemsViewSample()
        {
            var panel = CreateSamplePanel("ItemsView maps to a selectable WPF ListBox with tile-like items because ModernWpf does not currently expose ItemsView.");
            var listBox = new ListBox
            {
                Width = 420,
                Height = 170,
                ItemsPanel = CreateWrapItemsPanel(),
                ItemTemplate = CreateTileTemplate(),
                ItemsSource = new[] { "Contoso", "Fabrikam", "Northwind", "Tailspin", "AdventureWorks" }
            };
            panel.Children.Add(listBox);
            return panel;
        }

        private static UIElement CreateListBoxSample()
        {
            var panel = CreateSamplePanel("ListBox lets users choose one or more values from a simple list.");
            panel.Children.Add(new ListBox
            {
                Width = 260,
                Height = 150,
                SelectionMode = SelectionMode.Multiple,
                ItemsSource = new[] { "Red", "Green", "Blue", "Yellow" }
            });
            return panel;
        }

        private static UIElement CreateListViewSample()
        {
            var panel = CreateSamplePanel("ListView presents rich rows that can include multiple pieces of data.");
            var listView = new ListView
            {
                Width = 360,
                Height = 170,
                ItemsSource = CreatePeople(),
                ItemTemplate = CreatePersonTemplate()
            };
            panel.Children.Add(listView);
            return panel;
        }

        private static UIElement CreatePullToRefreshSample()
        {
            var root = new GallerySamplePanel
            {
                Margin = new Thickness(0, 0, 0, 12)
            };
            GalleryAutomation.WithAutomationId(root, GalleryAutomation.SampleRootId("PullToRefresh"));
            root.Children.Add(CreateBasicPullToRefreshExampleContent(assignRootAutomationId: false));
            return root;
        }

        private static IReadOnlyList<GalleryExample> CreatePullToRefreshExamples()
        {
            return new[]
            {
                new GalleryExample(
                    "Basic PullToRefresh",
                    CreateBasicPullToRefreshExampleContent(assignRootAutomationId: true),
                    PullToRefreshBasicXaml,
                    PullToRefreshBasicCSharp),
                new GalleryExample(
                    "Custom Icon PullToRefresh",
                    CreateCustomIconPullToRefreshExampleContent(),
                    PullToRefreshCustomIconXaml,
                    PullToRefreshCustomIconCSharp)
            };
        }

        private static GallerySamplePanel CreateBasicPullToRefreshExampleContent(bool assignRootAutomationId)
        {
            var root = CreatePullToRefreshExampleRoot(assignRootAutomationId);
            var items = new ObservableCollection<string>(new[]
            {
                "AcrylicBrush",
                "ColorPicker",
                "NavigationView",
                "ParallaxView",
                "PersonPicture",
                "PullToRefreshPage",
                "RatingsControl",
                "RevealBrush",
                "TreeView"
            });

            var listView = CreatePullToRefreshListView("lv", items);
            var host = CreatePullToRefreshHostGrid();
            var refreshContainer = new Mux.RefreshContainer
            {
                Name = "rc",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Content = listView
            };
            GalleryAutomation.WithAutomationId(refreshContainer, GalleryAutomation.SampleElementId("PullToRefresh", "RefreshContainer"));
            AttachRefreshHandler(refreshContainer, TimeSpan.FromMilliseconds(500), () => items.Insert(0, "NewControl"));
            host.Children.Add(refreshContainer);
            root.Children.Add(host);
            return root;
        }

        private static GallerySamplePanel CreateCustomIconPullToRefreshExampleContent()
        {
            var root = CreatePullToRefreshExampleRoot(assignRootAutomationId: false);
            var grid = CreatePullToRefreshHostGrid();
            grid.Name = "Ex2Grid";
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition());

            var items = new ObservableCollection<string>(new[]
            {
                "Mike",
                "Ben",
                "Barbra",
                "Claire",
                "Justin",
                "Shawn",
                "Drew",
                "Lili"
            });

            var refreshContainer = new Mux.RefreshContainer
            {
                Name = "rc2",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Content = CreatePullToRefreshListView("lv2", items),
                Visualizer = new Mux.RefreshVisualizer
                {
                    Name = "rv2",
                    Content = CreatePullToRefreshSunImage()
                }
            };
            refreshContainer.Visualizer.RefreshStateChanged += delegate { };
            AttachRefreshHandler(refreshContainer, TimeSpan.FromMilliseconds(800), () => items.Insert(0, "New Friend"));

            grid.Children.Add(refreshContainer);
            Grid.SetRow(refreshContainer, 1);
            root.Children.Add(grid);
            return root;
        }

        private static GallerySamplePanel CreatePullToRefreshExampleRoot(bool assignRootAutomationId)
        {
            var root = new GallerySamplePanel
            {
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            if (assignRootAutomationId)
            {
                GalleryAutomation.WithAutomationId(root, GalleryAutomation.SampleRootId("PullToRefresh"));
            }

            return root;
        }

        private static Grid CreatePullToRefreshHostGrid()
        {
            return new Grid
            {
                Height = 220,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
        }

        private static ListView CreatePullToRefreshListView(string name, ObservableCollection<string> items)
        {
            var listView = new ListView
            {
                Name = name,
                Height = 200,
                MinWidth = 200,
                ItemsSource = items
            };
            listView.SetResourceReference(Control.BorderBrushProperty, "TextControlBorderBrush");
            listView.BorderThickness = new Thickness(1);
            return listView;
        }

        private static Image CreatePullToRefreshSunImage()
        {
            var image = new Image
            {
                Width = 35,
                Height = 35
            };
            image.Loaded += delegate
            {
                var theme = ModernWpf.ThemeManager.GetActualTheme(image);
                var fileName = theme == ModernWpf.ElementTheme.Light ? "SunBlack.png" : "SunWhite.png";
                image.Source = new BitmapImage(new Uri(ResourceUri("Assets/SampleMedia/" + fileName), UriKind.Absolute));
            };
            return image;
        }

        private static void AttachRefreshHandler(Mux.RefreshContainer refreshContainer, TimeSpan delay, Action completeWork)
        {
            var timer = new DispatcherTimer { Interval = delay };
            Mux.RefreshDeferral refreshCompletionDeferral = null;

            timer.Tick += delegate
            {
                timer.Stop();
                completeWork();
                if (refreshCompletionDeferral != null)
                {
                    refreshCompletionDeferral.Complete();
                    refreshCompletionDeferral = null;
                }
            };

            refreshContainer.RefreshRequested += delegate(Mux.RefreshContainer sender, Mux.RefreshRequestedEventArgs args)
            {
                refreshCompletionDeferral = args.GetDeferral();
                timer.Start();
            };

            refreshContainer.Unloaded += delegate { timer.Stop(); };
        }

        private static UIElement CreateTreeViewSample()
        {
            var panel = CreateSamplePanel("TreeView displays hierarchical data with expandable nodes.");
            var tree = new TreeView { Width = 320, Height = 180 };
            var controls = new TreeViewItem { Header = "Controls", IsExpanded = true };
            controls.Items.Add(new TreeViewItem { Header = "Button" });
            controls.Items.Add(new TreeViewItem { Header = "ListView" });
            controls.Items.Add(new TreeViewItem { Header = "NavigationView" });
            var design = new TreeViewItem { Header = "Design", IsExpanded = true };
            design.Items.Add(new TreeViewItem { Header = "Color" });
            design.Items.Add(new TreeViewItem { Header = "Typography" });
            tree.Items.Add(controls);
            tree.Items.Add(design);
            panel.Children.Add(tree);
            return panel;
        }

        private static GridViewColumn CreateGridViewColumn(string header, string bindingPath, double width)
        {
            return new GridViewColumn
            {
                Header = header,
                Width = width,
                DisplayMemberBinding = new Binding(bindingPath)
            };
        }

        private static ItemsPanelTemplate CreateWrapItemsPanel()
        {
            var factory = new FrameworkElementFactory(typeof(WrapPanel));
            return new ItemsPanelTemplate(factory);
        }

        private static ItemsPanelTemplate CreateTopAlignedWrapItemsPanel()
        {
            var factory = new FrameworkElementFactory(typeof(WrapPanel));
            factory.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Top);
            return new ItemsPanelTemplate(factory);
        }

        private static Border CreateCollectionCard(string text)
        {
            return new Border
            {
                Width = 260,
                Height = 120,
                Padding = new Thickness(16),
                BorderThickness = new Thickness(1),
                Margin = new Thickness(0, 0, 0, 12),
                Child = CreateCardText(text)
            };
        }

        private static TextBlock CreateCardText(string text)
        {
            return new TextBlock
            {
                Text = text,
                FontSize = 24,
                FontWeight = FontWeights.SemiBold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
        }

        private static DataTemplate CreateRepeaterTemplate()
        {
            return (DataTemplate)XamlReader.Parse(
                "<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>" +
                "<Border Padding='10' Margin='0,0,0,6' BorderThickness='1' BorderBrush='#D8D8D8'>" +
                "<TextBlock Text='{Binding}' />" +
                "</Border>" +
                "</DataTemplate>");
        }

        private static DataTemplate CreateTileTemplate()
        {
            return (DataTemplate)XamlReader.Parse(
                "<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>" +
                "<Border Width='120' Height='72' Margin='0,0,8,8' Padding='10' BorderThickness='1' BorderBrush='#D8D8D8'>" +
                "<TextBlock Text='{Binding}' TextWrapping='Wrap' VerticalAlignment='Center' />" +
                "</Border>" +
                "</DataTemplate>");
        }

        private static DataTemplate CreatePersonTemplate()
        {
            return (DataTemplate)XamlReader.Parse(
                "<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>" +
                "<StackPanel Margin='0,4,0,4'>" +
                "<TextBlock Text='{Binding Name}' FontWeight='SemiBold' />" +
                "<TextBlock Text='{Binding Role}' Opacity='0.72' />" +
                "</StackPanel>" +
                "</DataTemplate>");
        }

        private static object[] CreatePeople()
        {
            return new object[]
            {
                new { Name = "Avery Howard", Role = "Designer", Status = "Online" },
                new { Name = "Kai Martin", Role = "Engineer", Status = "Busy" },
                new { Name = "Mina Patel", Role = "PM", Status = "Away" }
            };
        }

        private static Button CreateButton(string text)
        {
            return new Button
            {
                Content = text,
                Padding = new Thickness(16, 6, 16, 6),
                Margin = new Thickness(0, 0, 8, 0),
                HorizontalAlignment = HorizontalAlignment.Left
            };
        }

        private static StackPanel CreateSamplePanel(string description)
        {
            var panel = new StackPanel
            {
                Margin = new Thickness(0, 0, 0, 12)
            };
            panel.Children.Add(new TextBlock
            {
                Text = description,
                TextWrapping = TextWrapping.Wrap,
                Opacity = 0.72,
                Margin = new Thickness(0, 0, 0, 12)
            });
            return panel;
        }

        private static TextBlock CreateOutput(string text)
        {
            return new TextBlock
            {
                Text = text,
                Margin = new Thickness(0, 12, 0, 0),
                TextWrapping = TextWrapping.Wrap
            };
        }

        private static string FindSampleCodeText(IReadOnlyList<SampleSnippet> snippets, string relativePath)
        {
            var fileName = System.IO.Path.GetFileName(relativePath);
            for (var i = 0; i < snippets.Count; i++)
            {
                if (string.Equals(snippets[i].Title, fileName, StringComparison.Ordinal) ||
                    string.Equals(snippets[i].Title, relativePath, StringComparison.Ordinal))
                {
                    return snippets[i].Text;
                }
            }

            var path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Samples", "SampleCode", relativePath);
            return System.IO.File.Exists(path) ? System.IO.File.ReadAllText(path) : null;
        }

        private static string ResourceUri(string path)
        {
            return "pack://application:,,,/ModernWpf.Gallery;component/" + path;
        }
    }
}
