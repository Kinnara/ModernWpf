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
using System.Windows.Shapes;
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

        private const string ItemsRepeaterBasicXaml =
@"<!-- The ItemsRepeater and ScrollViewer used: -->
<ScrollViewer HorizontalScrollBarVisibility=""Auto""
              HorizontalScrollMode=""Auto""
              IsVerticalScrollChainingEnabled=""False""
              MaxHeight=""500"">
    <ItemsRepeater
               ItemsSource=""{x:Bind BarItems}""
               Layout=""{StaticResource $(Layout)}""
               ItemTemplate=""{StaticResource $(ElementGenerator)}"" />
</ScrollViewer>

<!-- The Layout specifications used: -->

$(SampleCodeLayout)

<!-- The DataTemplate used: $(ElementGenerator)-->

$(SampleCodeDT)";

        private const string ItemsRepeaterBasicCSharp =
@"// The ItemsSource used is a list of custom-class Bar objects called BarItems

public class Bar
{
    public Bar(double length, int max)
    {
        Length = length;
        MaxLength = max;

        Height = length / 4;
        MaxHeight = max / 4;

        Diameter = length / 6;
        MaxDiameter = max / 6;
    }
    public double Length { get; set; }
    public int MaxLength { get; set; }

    public double Height { get; set; }
    public double MaxHeight { get; set; }

    public double Diameter { get; set; }
    public double MaxDiameter { get; set; }
}

public ObservableCollection<Bar> BarItems;
private int MaxLength = 425;

private void InitializeData()
{
    if (BarItems == null)
    {
        BarItems = new ObservableCollection<Bar>();
    }
    BarItems.Add(new Bar(300, this.MaxLength));
    BarItems.Add(new Bar(25, this.MaxLength));
    BarItems.Add(new Bar(175, this.MaxLength));
}";

        public static IReadOnlyList<GalleryExample> CreateExamples(string uniqueId, IReadOnlyList<SampleSnippet> sampleSnippets)
        {
            switch (uniqueId)
            {
                case "GridView":
                    return CreateGridViewExamples(sampleSnippets);
                case "ItemsRepeater":
                    return CreateItemsRepeaterExamples(sampleSnippets);
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
                case "GridView":
                    return CreateGridViewSample();
                case "ItemsRepeater":
                    return CreateItemsRepeaterSample();
                case "PullToRefresh":
                    return CreatePullToRefreshSample();
                default:
                    return null;
            }
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
            GalleryAutomation.WithAutomationId(output, GalleryAutomation.SampleElementId("GridView", "ClickOutput0"));

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
            image.SetValue(RenderOptions.BitmapScalingModeProperty, BitmapScalingMode.HighQuality);
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
            var root = new GallerySamplePanel
            {
                Margin = new Thickness(0, 0, 0, 12)
            };
            GalleryAutomation.WithAutomationId(root, GalleryAutomation.SampleRootId("ItemsRepeater"));
            root.Children.Add(CreateBasicItemsRepeaterExampleContent(assignRootAutomationId: false));
            return root;
        }

        private static IReadOnlyList<GalleryExample> CreateItemsRepeaterExamples(IReadOnlyList<SampleSnippet> sampleSnippets)
        {
            return new[]
            {
                new GalleryExample(
                    "Basic, non-interactive items laid out by ItemsRepeater",
                    CreateBasicItemsRepeaterExampleContent(assignRootAutomationId: true),
                    ItemsRepeaterBasicXaml,
                    ItemsRepeaterBasicCSharp),
                new GalleryExample(
                    "Virtualizing, scrollable list of items laid out by ItemsRepeater",
                    CreateVirtualizingItemsRepeaterExampleContent(),
                    FindSampleCodeText(sampleSnippets, "ItemsRepeater/ItemsRepeaterSample2_xaml.txt"),
                    FindSampleCodeText(sampleSnippets, "ItemsRepeater/ItemsRepeaterSample2_cs.txt")),
                new GalleryExample(
                    "ItemsRepeater with mixed-type collection",
                    CreateMixedItemsRepeaterExampleContent(),
                    FindSampleCodeText(sampleSnippets, "ItemsRepeater/ItemsRepeaterSample1_xaml.txt"),
                    FindSampleCodeText(sampleSnippets, "ItemsRepeater/ItemsRepeaterSample1_cs.txt")),
                new GalleryExample(
                    "Laying out nested ItemsRepeaters",
                    CreateNestedItemsRepeaterExampleContent(),
                    FindSampleCodeText(sampleSnippets, "ItemsRepeater/ItemsRepeaterNestedSample_xaml.txt"),
                    null),
                new GalleryExample(
                    "Animated Scrolling and Content Display",
                    CreateAnimatedItemsRepeaterExampleContent(),
                    FindSampleCodeText(sampleSnippets, "ItemsRepeater/ItemsRepeaterSample3_xaml.txt"),
                    FindSampleCodeText(sampleSnippets, "ItemsRepeater/ItemsRepeaterSample3_cs.txt")),
                new GalleryExample(
                    "Virtualized, Content-Heavy Layout with Filtering and Sorting",
                    CreateRecipeItemsRepeaterExampleContent(),
                    FindSampleCodeText(sampleSnippets, "ItemsRepeater/ItemsRepeaterSample4_xaml.txt"),
                    FindSampleCodeText(sampleSnippets, "ItemsRepeater/ItemsRepeaterSample4_cs.txt"))
            };
        }

        private static GallerySamplePanel CreateBasicItemsRepeaterExampleContent(bool assignRootAutomationId)
        {
            var root = CreateItemsRepeaterExampleRoot(assignRootAutomationId);
            var layout = CreateRepeaterOptionsLayout();
            var maxLength = 425;
            var random = new Random(1);
            var barItems = CreateRepeaterBars(maxLength);

            var repeater = new Mux.ItemsRepeater
            {
                Name = "repeater",
                ItemsSource = barItems,
                Layout = CreateVerticalStackLayout(),
                ItemTemplate = CreateHorizontalBarTemplate(),
                MaxWidth = maxLength + 12
            };
            GalleryAutomation.WithAutomationId(repeater, GalleryAutomation.SampleElementId("ItemsRepeater", "ItemsRepeater"));

            var host = new Mux.ItemsRepeaterScrollHost
            {
                ScrollViewer = new ScrollViewer
                {
                    MaxHeight = 500,
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    Content = repeater
                }
            };
            layout.Children.Add(host);

            var options = new StackPanel
            {
                Width = 250
            };
            Grid.SetColumn(options, 2);

            Button deleteButton = null;
            var addButton = CreateButton("Add Item");
            addButton.Name = "AddBtn";
            addButton.MinWidth = 150;
            addButton.Click += delegate
            {
                barItems.Add(new RepeaterBar(random.Next(maxLength), maxLength));
                if (deleteButton != null)
                {
                    deleteButton.IsEnabled = true;
                }
            };

            deleteButton = CreateButton("Remove Item");
            deleteButton.Name = "DeleteBtn";
            deleteButton.MinWidth = 150;
            deleteButton.Click += delegate
            {
                if (barItems.Count > 0)
                {
                    barItems.RemoveAt(0);
                }

                deleteButton.IsEnabled = barItems.Count > 0;
            };
            options.Children.Add(addButton);
            options.Children.Add(deleteButton);

            options.Children.Add(new TextBlock
            {
                Text = "Layout",
                Margin = new Thickness(0, 12, 0, 4)
            });

            Action<string> applyLayout = delegate(string layoutKey)
            {
                switch (layoutKey)
                {
                    case "HorizontalStackLayout":
                        repeater.Layout = CreateHorizontalStackLayout();
                        repeater.ItemTemplate = CreateVerticalBarTemplate();
                        repeater.MaxWidth = 6000;
                        break;
                    case "UniformGridLayout":
                        repeater.Layout = CreateBasicUniformGridLayout();
                        repeater.ItemTemplate = CreateCircularBarTemplate();
                        repeater.MaxWidth = 540;
                        break;
                    default:
                        repeater.Layout = CreateVerticalStackLayout();
                        repeater.ItemTemplate = CreateHorizontalBarTemplate();
                        repeater.MaxWidth = maxLength + 12;
                        break;
                }
            };

            options.Children.Add(CreateLayoutRadioButton("VStackBtn", "StackLayout - Vertical", "VerticalStackLayout", true, applyLayout));
            options.Children.Add(CreateLayoutRadioButton("HStackBtn", "StackLayout - Horizontal", "HorizontalStackLayout", false, applyLayout));
            options.Children.Add(CreateLayoutRadioButton("HGridBtn", "UniformGridLayout", "UniformGridLayout", false, applyLayout));

            layout.Children.Add(options);
            root.Children.Add(layout);
            return root;
        }

        private static GallerySamplePanel CreateVirtualizingItemsRepeaterExampleContent()
        {
            var root = CreateItemsRepeaterExampleRoot(assignRootAutomationId: false);
            var layout = CreateRepeaterOptionsLayout();

            var repeater = new Mux.ItemsRepeater
            {
                Name = "repeater2",
                Margin = new Thickness(0, 0, 12, 0),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                ItemsSource = CreateNumberedItems(500),
                ItemTemplate = new NumberTemplateSelector
                {
                    Normal = CreateNumberItemTemplate(accent: false),
                    Accent = CreateNumberItemTemplate(accent: true)
                },
                Layout = CreateActivityFeedLayoutApproximation()
            };

            var scrollViewer = new ScrollViewer
            {
                Name = "scrollViewer",
                Height = 400,
                Padding = new Thickness(0, 0, 16, 0),
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Content = repeater
            };
            layout.Children.Add(new Mux.ItemsRepeaterScrollHost { ScrollViewer = scrollViewer });

            var options = new StackPanel
            {
                Width = 250
            };
            Grid.SetColumn(options, 2);
            options.Children.Add(CreateLayoutRadioButton(null, "Uniform grid", "UniformGridLayout2", false, delegate
            {
                repeater.Layout = CreateUniformGridLayout2();
            }));
            options.Children.Add(CreateLayoutRadioButton(null, "Custom virtualizing layout", "MyFeedLayout", true, delegate
            {
                repeater.Layout = CreateActivityFeedLayoutApproximation();
            }));
            layout.Children.Add(options);

            root.Children.Add(layout);
            return root;
        }

        private static GallerySamplePanel CreateMixedItemsRepeaterExampleContent()
        {
            var root = CreateItemsRepeaterExampleRoot(assignRootAutomationId: false);
            root.Children.Add(new TextBlock
            {
                Text = "This is an ItemsRepeater that displays both integer and string items. It uses a DataTemplateSelector to choose the correct layout for each of its items.",
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 12)
            });

            root.Children.Add(new Mux.ItemsRepeater
            {
                Name = "MixedTypeRepeater",
                Margin = new Thickness(0, 0, 12, 0),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                ItemsSource = CreateMixedRepeaterItems(),
                ItemTemplate = new StringOrIntTemplateSelector
                {
                    StringTemplate = CreateStringItemTemplate(),
                    IntTemplate = CreateIntItemTemplate()
                },
                Layout = new Mux.UniformGridLayout
                {
                    MinItemWidth = 200,
                    MinItemHeight = 200
                }
            });
            return root;
        }

        private static GallerySamplePanel CreateNestedItemsRepeaterExampleContent()
        {
            var root = CreateItemsRepeaterExampleRoot(assignRootAutomationId: false);
            var outerRepeater = new Mux.ItemsRepeater
            {
                Name = "outerRepeater",
                VerticalAlignment = VerticalAlignment.Top,
                ItemsSource = CreateNestedCategories(),
                ItemTemplate = CreateCategoryTemplate(),
                Layout = CreateVerticalStackLayout()
            };
            root.Children.Add(new ScrollViewer
            {
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Content = outerRepeater
            });
            return root;
        }

        private static GallerySamplePanel CreateAnimatedItemsRepeaterExampleContent()
        {
            var root = CreateItemsRepeaterExampleRoot(assignRootAutomationId: false);
            var layout = new Grid();
            layout.ColumnDefinitions.Add(new ColumnDefinition());
            layout.ColumnDefinitions.Add(new ColumnDefinition());

            Rectangle colorRectangle = null;
            Action<RepeaterColorItem> selectColor = delegate(RepeaterColorItem item)
            {
                if (colorRectangle != null)
                {
                    colorRectangle.Fill = item.Brush;
                }
            };
            var repeater = new Mux.ItemsRepeater
            {
                Name = "animatedScrollRepeater",
                ItemsSource = CreateColorItems(selectColor),
                ItemTemplate = CreateColorButtonTemplate(),
                Layout = CreateVerticalStackLayout()
            };
            repeater.ElementPrepared += delegate(Mux.ItemsRepeater sender, Mux.ItemsRepeaterElementPreparedEventArgs args)
            {
                var element = args.Element as FrameworkElement;
                if (element != null)
                {
                    element.Margin = new Thickness(0, 0, 0, 4);
                }
            };

            var scrollViewer = new ScrollViewer
            {
                Name = "Animated_ScrollViewer",
                Width = 250,
                Height = 175,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Content = repeater
            };
            layout.Children.Add(scrollViewer);

            colorRectangle = new Rectangle
            {
                Name = "colorRectangle",
                Width = 150,
                Height = 150,
                Margin = new Thickness(10, 0, 0, 0),
                StrokeThickness = 1
            };
            colorRectangle.SetResourceReference(Shape.StrokeProperty, "SystemControlForegroundBaseHighBrush");
            AutomationProperties.SetName(colorRectangle, "ColorRectangle");
            Grid.SetColumn(colorRectangle, 1);
            layout.Children.Add(colorRectangle);

            root.Children.Add(layout);
            return root;
        }

        private static GallerySamplePanel CreateRecipeItemsRepeaterExampleContent()
        {
            var root = CreateItemsRepeaterExampleRoot(assignRootAutomationId: false);
            var layout = new Grid
            {
                Height = 600
            };
            layout.ColumnDefinitions.Add(new ColumnDefinition());
            layout.ColumnDefinitions.Add(new ColumnDefinition());

            var sourceRecipes = CreateRecipeList();
            var filteredRecipes = new ObservableCollection<RepeaterRecipe>(sourceRecipes);
            var sortDescending = false;

            var repeater = new Mux.ItemsRepeater
            {
                Name = "VariedImageSizeRepeater",
                ItemsSource = filteredRecipes,
                ItemTemplate = CreateRecipeTemplate(),
                Layout = new Mux.UniformGridLayout
                {
                    MinItemWidth = 200,
                    MinColumnSpacing = 12,
                    MinRowSpacing = 12
                }
            };
            var tracker = new Mux.ItemsRepeaterScrollHost
            {
                Name = "tracker",
                ScrollViewer = new ScrollViewer
                {
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    Content = repeater
                }
            };
            layout.Children.Add(tracker);

            var options = new StackPanel
            {
                Margin = new Thickness(10, 0, 0, 0)
            };
            Grid.SetColumn(options, 1);

            options.Children.Add(new TextBlock
            {
                Text = "Filter by ingredient...",
                Margin = new Thickness(0, 0, 0, 4)
            });
            var filter = new TextBox
            {
                Name = "FilterRecipes",
                Width = 200,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 0, 0, 20)
            };
            options.Children.Add(filter);
            options.Children.Add(new TextBlock
            {
                Text = "Sort by number of ingredients",
                Margin = new Thickness(0, 0, 0, 10)
            });
            var leastToMost = CreateButton("Least to most");
            var mostToLeast = CreateButton("Most to least");
            options.Children.Add(leastToMost);
            options.Children.Add(mostToLeast);

            Action updateSortAndFilter = delegate
            {
                var filterText = filter.Text ?? string.Empty;
                var next = new List<RepeaterRecipe>();
                foreach (var recipe in sourceRecipes)
                {
                    if (recipe.Ingredients.IndexOf(filterText, StringComparison.InvariantCultureIgnoreCase) >= 0)
                    {
                        next.Add(recipe);
                    }
                }

                next.Sort(delegate(RepeaterRecipe left, RepeaterRecipe right)
                {
                    var result = left.NumIngredients.CompareTo(right.NumIngredients);
                    return sortDescending ? -result : result;
                });

                filteredRecipes.Clear();
                foreach (var recipe in next)
                {
                    filteredRecipes.Add(recipe);
                }
            };

            filter.TextChanged += delegate { updateSortAndFilter(); };
            leastToMost.Click += delegate
            {
                sortDescending = false;
                updateSortAndFilter();
            };
            mostToLeast.Click += delegate
            {
                sortDescending = true;
                updateSortAndFilter();
            };

            layout.Children.Add(options);
            root.Children.Add(layout);
            return root;
        }

        private static GallerySamplePanel CreateItemsRepeaterExampleRoot(bool assignRootAutomationId)
        {
            var root = new GallerySamplePanel
            {
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            if (assignRootAutomationId)
            {
                GalleryAutomation.WithAutomationId(root, GalleryAutomation.SampleRootId("ItemsRepeater"));
            }

            return root;
        }

        private static Grid CreateRepeaterOptionsLayout()
        {
            var layout = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            layout.ColumnDefinitions.Add(new ColumnDefinition());
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(250) });
            return layout;
        }

        private static RadioButton CreateLayoutRadioButton(
            string name,
            string content,
            string tag,
            bool isChecked,
            Action<string> checkedAction)
        {
            var radioButton = new RadioButton
            {
                Content = content,
                Tag = tag,
                IsChecked = isChecked,
                Margin = new Thickness(0, 2, 0, 2)
            };
            if (name != null)
            {
                radioButton.Name = name;
            }

            radioButton.Checked += delegate { checkedAction(tag); };
            return radioButton;
        }

        private static Mux.StackLayout CreateVerticalStackLayout()
        {
            return new Mux.StackLayout
            {
                Orientation = Orientation.Vertical,
                Spacing = 8
            };
        }

        private static Mux.StackLayout CreateHorizontalStackLayout()
        {
            return new Mux.StackLayout
            {
                Orientation = Orientation.Horizontal,
                Spacing = 8
            };
        }

        private static Mux.UniformGridLayout CreateBasicUniformGridLayout()
        {
            return new Mux.UniformGridLayout
            {
                MinColumnSpacing = 8,
                MinRowSpacing = 8
            };
        }

        private static Mux.UniformGridLayout CreateUniformGridLayout2()
        {
            return new Mux.UniformGridLayout
            {
                MinItemWidth = 108,
                MinItemHeight = 108,
                MinColumnSpacing = 12,
                MinRowSpacing = 12
            };
        }

        private static Mux.UniformGridLayout CreateActivityFeedLayoutApproximation()
        {
            return new Mux.UniformGridLayout
            {
                MinItemWidth = 80,
                MinItemHeight = 108,
                MinColumnSpacing = 12,
                MinRowSpacing = 12
            };
        }

        private static ObservableCollection<RepeaterBar> CreateRepeaterBars(int maxLength)
        {
            return new ObservableCollection<RepeaterBar>(new[]
            {
                new RepeaterBar(300, maxLength),
                new RepeaterBar(25, maxLength),
                new RepeaterBar(175, maxLength)
            });
        }

        private static int[] CreateNumberedItems(int count)
        {
            var items = new int[count];
            for (var i = 0; i < count; i++)
            {
                items[i] = i;
            }

            return items;
        }

        private static List<object> CreateMixedRepeaterItems()
        {
            return new List<object>
            {
                64,
                "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.",
                128,
                "Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat.",
                256,
                "Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur.",
                512,
                "Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.",
                1024
            };
        }

        private static ObservableCollection<RepeaterNestedCategory> CreateNestedCategories()
        {
            return new ObservableCollection<RepeaterNestedCategory>(new[]
            {
                new RepeaterNestedCategory("Fruits", new[] { "Apricots", "Bananas", "Grapes", "Strawberries", "Watermelon", "Plums", "Blueberries" }),
                new RepeaterNestedCategory("Vegetables", new[] { "Broccoli", "Spinach", "Sweet potato", "Cauliflower", "Onion", "Brussels sprouts", "Carrots" }),
                new RepeaterNestedCategory("Grains", new[] { "Rice", "Quinoa", "Pasta", "Bread", "Farro", "Oats", "Barley" }),
                new RepeaterNestedCategory("Proteins", new[] { "Steak", "Chicken", "Tofu", "Salmon", "Pork", "Chickpeas", "Eggs" })
            });
        }

        private static List<RepeaterColorItem> CreateColorItems(Action<RepeaterColorItem> selectAction = null)
        {
            var colors = new[]
            {
                "Blue",
                "BlueViolet",
                "Crimson",
                "DarkCyan",
                "DarkGoldenrod",
                "DarkMagenta",
                "DarkOliveGreen",
                "DarkRed",
                "DarkSlateBlue",
                "DeepPink",
                "IndianRed",
                "MediumSlateBlue",
                "Maroon",
                "MidnightBlue",
                "Peru",
                "SaddleBrown",
                "SteelBlue",
                "OrangeRed",
                "Firebrick",
                "DarkKhaki"
            };

            var items = new List<RepeaterColorItem>();
            foreach (var color in colors)
            {
                items.Add(new RepeaterColorItem(color, selectAction));
            }

            return items;
        }

        private static List<RepeaterRecipe> CreateRecipeList()
        {
            var random = new Random(1);
            var colors = CreateColorItems();
            var fruits = new[] { "Apricots", "Bananas", "Grapes", "Strawberries", "Watermelon", "Plums", "Blueberries" };
            var vegetables = new[] { "Broccoli", "Spinach", "Sweet potato", "Cauliflower", "Onion", "Brussels sprouts", "Carrots" };
            var grains = new[] { "Rice", "Quinoa", "Pasta", "Bread", "Farro", "Oats", "Barley" };
            var proteins = new[] { "Steak", "Chicken", "Tofu", "Salmon", "Pork", "Chickpeas", "Eggs" };
            var extras = new[] { "Garlic", "Lemon", "Butter", "Lime", "Feta Cheese", "Parmesan Cheese", "Breadcrumbs" };
            var recipes = new List<RepeaterRecipe>();

            for (var i = 0; i < 120; i++)
            {
                var ingredients = new List<string>
                {
                    fruits[random.Next(fruits.Length)],
                    vegetables[random.Next(vegetables.Length)],
                    grains[random.Next(grains.Length)],
                    proteins[random.Next(proteins.Length)]
                };

                var extraCount = random.Next(0, 4);
                for (var j = 0; j < extraCount; j++)
                {
                    var extra = extras[random.Next(extras.Length)];
                    if (!ingredients.Contains(extra))
                    {
                        ingredients.Add(extra);
                    }
                }

                recipes.Add(new RepeaterRecipe
                {
                    Num = i,
                    Name = "Recipe " + i,
                    ColorBrush = colors[random.Next(colors.Count)].Brush,
                    IngredientList = ingredients
                });
            }

            return recipes;
        }

        private static DataTemplate CreateHorizontalBarTemplate()
        {
            return ParseRepeaterTemplate(
                "<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>" +
                "<Border Width='{Binding MaxLength}' Background='{DynamicResource SystemControlBackgroundChromeMediumBrush}'>" +
                "<Rectangle Width='{Binding Length}' Height='24' HorizontalAlignment='Left' Fill='{DynamicResource SystemControlBackgroundAccentBrush}'/>" +
                "</Border>" +
                "</DataTemplate>");
        }

        private static DataTemplate CreateVerticalBarTemplate()
        {
            return ParseRepeaterTemplate(
                "<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>" +
                "<Border Height='{Binding MaxHeight}' Background='{DynamicResource SystemControlBackgroundChromeMediumBrush}'>" +
                "<Rectangle Width='48' Height='{Binding Height}' VerticalAlignment='Top' Fill='{DynamicResource SystemControlBackgroundAccentBrush}'/>" +
                "</Border>" +
                "</DataTemplate>");
        }

        private static DataTemplate CreateCircularBarTemplate()
        {
            return ParseRepeaterTemplate(
                "<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>" +
                "<Grid>" +
                "<Ellipse Width='{Binding MaxDiameter}' Height='{Binding MaxDiameter}' HorizontalAlignment='Center' VerticalAlignment='Center' Fill='{DynamicResource SystemControlBackgroundChromeMediumBrush}'/>" +
                "<Ellipse Width='{Binding Diameter}' Height='{Binding Diameter}' HorizontalAlignment='Center' VerticalAlignment='Center' Fill='{DynamicResource SystemControlBackgroundAccentBrush}'/>" +
                "</Grid>" +
                "</DataTemplate>");
        }

        private static DataTemplate CreateNumberItemTemplate(bool accent)
        {
            var backgroundResource = accent ? "SystemControlBackgroundAccentBrush" : "SystemControlBackgroundChromeMediumBrush";
            var foreground = accent ? " Foreground='{DynamicResource SystemControlForegroundChromeWhiteBrush}'" : string.Empty;
            return ParseRepeaterTemplate(
                "<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>" +
                "<Button HorizontalAlignment='Stretch' VerticalAlignment='Stretch' Content='{Binding}' Background='{DynamicResource " + backgroundResource + "}'" + foreground + "/>" +
                "</DataTemplate>");
        }

        private static DataTemplate CreateStringItemTemplate()
        {
            return ParseRepeaterTemplate(
                "<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>" +
                "<Grid Margin='10' Background='{DynamicResource SystemControlBackgroundAccentBrush}'>" +
                "<TextBlock Padding='10' Text='{Binding}' Foreground='{DynamicResource SystemControlForegroundChromeWhiteBrush}' HorizontalAlignment='Center' TextWrapping='Wrap' VerticalAlignment='Center'/>" +
                "</Grid>" +
                "</DataTemplate>");
        }

        private static DataTemplate CreateIntItemTemplate()
        {
            return ParseRepeaterTemplate(
                "<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>" +
                "<Grid Margin='10' Background='{DynamicResource SystemControlBackgroundChromeMediumBrush}'>" +
                "<TextBlock Padding='10' Text='{Binding}' Style='{DynamicResource HeaderTextBlockStyle}' HorizontalAlignment='Center' VerticalAlignment='Center'/>" +
                "</Grid>" +
                "</DataTemplate>");
        }

        private static DataTemplate CreateCategoryTemplate()
        {
            return ParseRepeaterTemplate(
                "<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' " +
                "xmlns:mux='clr-namespace:ModernWpf.Controls;assembly=ModernWpf.Controls'>" +
                "<StackPanel>" +
                "<TextBlock Padding='8' Style='{DynamicResource TitleTextBlockStyle}' Text='{Binding CategoryName}'/>" +
                "<mux:ItemsRepeater Name='innerRepeater' ItemsSource='{Binding CategoryItems}'>" +
                "<mux:ItemsRepeater.ItemTemplate>" +
                "<DataTemplate><Grid Margin='10' Background='{DynamicResource SystemControlBackgroundAccentBrush}'><TextBlock Padding='10' Text='{Binding}' Foreground='{DynamicResource SystemControlForegroundChromeWhiteBrush}' HorizontalAlignment='Center' TextWrapping='Wrap' VerticalAlignment='Center'/></Grid></DataTemplate>" +
                "</mux:ItemsRepeater.ItemTemplate>" +
                "<mux:ItemsRepeater.Layout><mux:StackLayout Orientation='Horizontal' Spacing='8'/></mux:ItemsRepeater.Layout>" +
                "</mux:ItemsRepeater>" +
                "</StackPanel>" +
                "</DataTemplate>");
        }

        private static DataTemplate CreateColorButtonTemplate()
        {
            return ParseRepeaterTemplate(
                "<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>" +
                "<Button HorizontalAlignment='Stretch' Content='{Binding Name}' Background='{Binding Brush}' Foreground='{DynamicResource TextFillColorInverseBrush}' Command='{Binding SelectCommand}' CommandParameter='{Binding}'/>" +
                "</DataTemplate>");
        }

        private static DataTemplate CreateRecipeTemplate()
        {
            return ParseRepeaterTemplate(
                "<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>" +
                "<Border BorderThickness='1' Background='{DynamicResource SystemControlBackgroundBaseLowBrush}' Margin='5'>" +
                "<StackPanel>" +
                "<StackPanel Background='{Binding ColorBrush}' Margin='8' Height='75' Opacity='.8'>" +
                "<TextBlock Text='{Binding Num}' FontSize='35' TextAlignment='Center' Padding='12' Foreground='{DynamicResource SystemControlForegroundAltHighBrush}'/>" +
                "</StackPanel>" +
                "<TextBlock Name='recipeName' Text='{Binding Name}' TextWrapping='Wrap' Margin='15,0,10,0' Style='{DynamicResource TitleTextBlockStyle}'/>" +
                "<TextBlock Text='{Binding Ingredients}' Style='{DynamicResource BodyTextBlockStyle}' Margin='15,0,15,15'/>" +
                "</StackPanel>" +
                "</Border>" +
                "</DataTemplate>");
        }

        private static DataTemplate ParseRepeaterTemplate(string xaml)
        {
            return (DataTemplate)XamlReader.Parse(xaml);
        }

        private sealed class NumberTemplateSelector : DataTemplateSelector
        {
            public DataTemplate Normal { get; set; }
            public DataTemplate Accent { get; set; }

            public override DataTemplate SelectTemplate(object item, DependencyObject container)
            {
                return item is int value && value % 2 != 0 ? Accent : Normal;
            }
        }

        private sealed class StringOrIntTemplateSelector : DataTemplateSelector
        {
            public DataTemplate StringTemplate { get; set; }
            public DataTemplate IntTemplate { get; set; }

            public override DataTemplate SelectTemplate(object item, DependencyObject container)
            {
                return item is string ? StringTemplate : IntTemplate;
            }
        }

        private sealed class RepeaterBar
        {
            public RepeaterBar(double length, int max)
            {
                Length = length;
                MaxLength = max;
                Height = length / 4;
                MaxHeight = max / 4;
                Diameter = length / 6;
                MaxDiameter = max / 6;
            }

            public double Length { get; }
            public int MaxLength { get; }
            public double Height { get; }
            public double MaxHeight { get; }
            public double Diameter { get; }
            public double MaxDiameter { get; }
        }

        private sealed class RepeaterNestedCategory
        {
            public RepeaterNestedCategory(string categoryName, IEnumerable<string> items)
            {
                CategoryName = categoryName;
                CategoryItems = new ObservableCollection<string>(items);
            }

            public string CategoryName { get; }
            public ObservableCollection<string> CategoryItems { get; }
        }

        private sealed class RepeaterColorItem
        {
            public RepeaterColorItem(string name, Action<RepeaterColorItem> selectAction)
            {
                Name = name;
                Brush = (Brush)new BrushConverter().ConvertFromString(name);
                if (selectAction != null)
                {
                    SelectCommand = new GalleryCommand(delegate(object parameter)
                    {
                        selectAction((RepeaterColorItem)parameter);
                    });
                }
            }

            public string Name { get; }
            public Brush Brush { get; }
            public GalleryCommand SelectCommand { get; }
        }

        private sealed class RepeaterRecipe
        {
            public int Num { get; set; }
            public string Name { get; set; }
            public Brush ColorBrush { get; set; }
            public List<string> IngredientList { get; set; }

            public string Ingredients
            {
                get { return "\n" + string.Join("\n", IngredientList); }
            }

            public int NumIngredients
            {
                get { return IngredientList.Count; }
            }
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
                "AutoSuggestBox",
                "ColorPicker",
                "NavigationView",
                "ParallaxView",
                "PersonPicture",
                "PullToRefresh",
                "RatingControl",
                "TeachingTip",
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
