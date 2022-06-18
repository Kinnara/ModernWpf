//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************
using ModernWpf.Controls;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Navigation;
using GridView = ModernWpf.Controls.GridView;

namespace ModernWpf.SampleApp.ControlPages
{
    public sealed partial class GridViewPage : ItemsPageBase
    {
        WrapPanel StyledGridIWG;

        public GridViewPage()
        {
            this.InitializeComponent();
            this.DataContext = this;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Get data objects and place them into an ObservableCollection
            List<CustomDataObject> tempList = CustomDataObject.GetDataObjects();
            ObservableCollection<CustomDataObject> Items = new ObservableCollection<CustomDataObject>(tempList);
            ObservableCollection<CustomDataObject> Items2 = new ObservableCollection<CustomDataObject>(tempList);
            BasicGridView.ItemsSource = Items2;
            ContentGridView.ItemsSource = Items;
            StyledGrid.ItemsSource = Items;

            Tag = @"<!-- ImageTemplate: -->
<DataTemplate x:Key='ImageTemplate' x:DataType='local1: CustomDataObject'>
    <Image Stretch = 'UniformToFill' Source = '{x:Bind ImageLocation}' 
           AutomationProperties.Name = '{x:Bind Title}' Width = '190' Height = '130' 
           AutomationProperties.AccessibilityView = 'Raw'/>
</DataTemplate> ";

        }

        private void ItemTemplate_Checked(object sender, RoutedEventArgs e)
        {
            var tag = (sender as FrameworkElement).Tag;
            if (tag != null)
            {
                string template = tag.ToString();
                ContentGridView.ItemTemplate = (DataTemplate)this.Resources[template];
                Control2.Tag = template;

                if (template == "ImageTemplate")
                {
                    Tag = @"<!-- ImageTemplate: -->
<DataTemplate x:Key='ImageTemplate' x:DataType='local1: CustomDataObject'>
    <Image Stretch = 'UniformToFill' Source = '{x:Bind ImageLocation}' 
           AutomationProperties.Name = '{x:Bind Title}' Width = '190' Height = '130' 
           AutomationProperties.AccessibilityView = 'Raw'/>
</DataTemplate> ";
                }

                else if (template == "IconTextTemplate")
                {
                    Tag = @"<!-- IconTextTemplate: -->
<DataTemplate x:Key='IconTextTemplate' x:DataType='local1:CustomDataObject'>
    <RelativePanel AutomationProperties.Name='{x:Bind Title}' Width='280' MinHeight='160'>
        <Image x:Name='image'
               Width='18'
               Margin='0,4,0,0'
               RelativePanel.AlignLeftWithPanel='True'
               RelativePanel.AlignTopWithPanel='True'
               Source='{x:Bind ImageLocation}'
               Stretch='Uniform' />
        <TextBlock x:Name='title' Style='{StaticResource BaseTextBlockStyle}' Margin='8,0,0,0' 
                   Text='{x:Bind Title}' RelativePanel.RightOf='image' RelativePanel.AlignTopWithPanel='True'/>
        <TextBlock Text='{x:Bind Description}' Style='{StaticResource CaptionTextBlockStyle}' 
                   TextWrapping='Wrap' Margin='0,4,8,0' RelativePanel.Below='title' TextTrimming='WordEllipsis'/>
    </RelativePanel>
</DataTemplate>";
                }

                else if (template == "ImageTextTemplate")
                {
                    Tag = @"<!-- ImageTextTemplate: -->
<DataTemplate x: Key = 'ImageTextTemplate' x: DataType = 'local1:CustomDataObject'>
    <Grid AutomationProperties.Name = '{x:Bind Title}' Width = '280'>
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width = 'Auto'/>
                <ColumnDefinition Width = '*'/>
        </Grid.ColumnDefinitions>
        <Image Source = '{x:Bind ImageLocation}' Height = '100' Stretch = 'Fill' VerticalAlignment = 'Top'/>
        <StackPanel Grid.Column = '1' Margin = '8,0,0,8'>
            <TextBlock Text = '{x:Bind Title}' Style = '{ThemeResource SubtitleTextBlockStyle}' Margin = '0,0,0,8'/>
            <StackPanel Orientation = 'Horizontal'>
                <TextBlock Text = '{x:Bind Views}' Style = '{ThemeResource CaptionTextBlockStyle}'/>
                    <TextBlock Text = ' Views ' Style = '{ThemeResource CaptionTextBlockStyle}'/>
            </StackPanel>
            <StackPanel Orientation = 'Horizontal'>
                    <TextBlock Text = '{x:Bind Likes}' Style = '{ThemeResource CaptionTextBlockStyle}'/> 
                    <TextBlock Text = ' Likes' Style = '{ThemeResource CaptionTextBlockStyle}'/>
            </StackPanel>
        </StackPanel>
     </Grid>
</DataTemplate>";
                }

                else
                {
                    Tag = @"<!-- TextTemplate: -->
<DataTemplate x:Key='TextTemplate' x:DataType='local1: CustomDataObject'>
    <StackPanel Width = '240' Orientation = 'Horizontal'>
        <TextBlock Style = '{StaticResource TitleTextBlockStyle}' Margin = '8,0,0,0' Text = '{x:Bind Title}'/>
            </StackPanel>
</DataTemplate>";
                }
            }
        }

        private void ContentGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is GridView gridView)
            {
                SelectionOutput.Text = string.Format("You have selected {0} item(s).", gridView.SelectedItems.Count);
            }
        }

        private void ContentGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            ClickOutput.Text = "You clicked " + (e.ClickedItem as CustomDataObject).Title + ".";
        }

        private void BasicGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            ClickOutput0.Text = "You clicked " + (e.ClickedItem as CustomDataObject).Title + ".";
        }

        private void ItemClickCheckBox_Click(object sender, RoutedEventArgs e)
        {
            ClickOutput.Text = string.Empty;
        }

        private void FlowDirectionCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (ContentGridView.FlowDirection == FlowDirection.LeftToRight)
            {
                ContentGridView.FlowDirection = FlowDirection.RightToLeft;
            }
            else
            {
                ContentGridView.FlowDirection = FlowDirection.LeftToRight;
            }
        }

        private void SelectionModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ContentGridView != null)
            {
                string colorName = e.AddedItems[0].ToString();
                switch (colorName)
                {
                    case "None":
                        ContentGridView.IsSelectionEnabled = false;
                        SelectionOutput.Text = string.Empty;
                        break;
                    case "Single":
                        ContentGridView.IsSelectionEnabled = true;
                        ContentGridView.SelectionMode = SelectionMode.Single;
                        break;
                    case "Multiple":
                        ContentGridView.IsSelectionEnabled = true;
                        ContentGridView.SelectionMode = SelectionMode.Multiple;
                        break;
                    case "Extended":
                        ContentGridView.IsSelectionEnabled = true;
                        ContentGridView.SelectionMode = SelectionMode.Extended;
                        break;
                }
            }
        }

        private void StyledGrid_InitWrapGrid(object sender, RoutedEventArgs e)
        {
            // Update ItemsWrapGrid object created on page load by assigning it to StyledGrid's ItemWrapGrid
            StyledGridIWG = sender as WrapPanel;

            // Now we can change StyledGrid's MaximumRowsorColumns property within its ItemsPanel>ItemsPanelTemplate>ItemsWrapGrid.
            //StyledGridIWG.MaximumRowsOrColumns = 3;
        }

        private void NumberBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (StyledGridIWG == null) { return; }

            int rowSpace = (int)RowSpace.Value;
            int columnSpace = (int)ColumnSpace.Value;
            for (int i = 0; i < StyledGrid.Items.Count; i++)
            {
                GridViewItem item = StyledGrid.ItemContainerGenerator.ContainerFromIndex(i) as GridViewItem;

                Thickness NewMargin = item.Margin;
                NewMargin.Left = columnSpace;
                NewMargin.Top = rowSpace;
                NewMargin.Right = columnSpace;
                NewMargin.Bottom = rowSpace;

                item.Margin = NewMargin;
            }
        }

        private void ControlExample_Loaded(object sender, RoutedEventArgs e)
        {

            ControlExampleSubstitution Substitution1 = new ControlExampleSubstitution
            {
                Key = "ColMargin",
            };
            BindingOperations.SetBinding(Substitution1, ControlExampleSubstitution.ValueProperty, new Binding
            {
                Source = ColumnSpace,
                Path = new PropertyPath("Value"),
            });

            ControlExampleSubstitution Substitution2 = new ControlExampleSubstitution
            {
                Key = "RowMargin",
            };
            BindingOperations.SetBinding(Substitution2, ControlExampleSubstitution.ValueProperty, new Binding
            {
                Source = RowSpace,
                Path = new PropertyPath("Value"),
            });

            ObservableCollection<ControlExampleSubstitution> Substitutions = new ObservableCollection<ControlExampleSubstitution>() { Substitution1, Substitution2 };
            (sender as ControlExample).Substitutions = Substitutions;
        }

        private void ControlExample2_Loaded(object sender, RoutedEventArgs e)
        {
            ControlExampleSubstitution Substitution1 = new ControlExampleSubstitution
            {
                Key = "ItemTemplate",
            };
            BindingOperations.SetBinding(Substitution1, ControlExampleSubstitution.ValueProperty, new Binding
            {
                Source = Control2,
                Path = new PropertyPath("Tag"),
            });

            ControlExampleSubstitution Substitution2 = new ControlExampleSubstitution
            {
                Key = "IsItemClickEnabled",
            };
            BindingOperations.SetBinding(Substitution2, ControlExampleSubstitution.ValueProperty, new Binding
            {
                Source = ContentGridView,
                Path = new PropertyPath("IsItemClickEnabled"),
            });

            ControlExampleSubstitution Substitution3 = new ControlExampleSubstitution
            {
                Key = "SelectionMode",
            };
            BindingOperations.SetBinding(Substitution3, ControlExampleSubstitution.ValueProperty, new Binding
            {
                Source = ContentGridView,
                Path = new PropertyPath("SelectionMode"),
            });

            ControlExampleSubstitution Substitution4 = new ControlExampleSubstitution
            {
                Key = "FlowDirection",
            };
            BindingOperations.SetBinding(Substitution4, ControlExampleSubstitution.ValueProperty, new Binding
            {
                Source = ContentGridView,
                Path = new PropertyPath("FlowDirection"),
            });

            ControlExampleSubstitution Substitution5 = new ControlExampleSubstitution
            {
                Key = "DisplayDT",
            };
            BindingOperations.SetBinding(Substitution5, ControlExampleSubstitution.ValueProperty, new Binding
            {
                Source = this,
                Path = new PropertyPath("Tag"),
            });

            ControlExampleSubstitution Substitution6 = new ControlExampleSubstitution
            {
                Key = "CanDropItems",
            };
            BindingOperations.SetBinding(Substitution6, ControlExampleSubstitution.ValueProperty, new Binding
            {
                Source = ContentGridView,
                Path = new PropertyPath("AllowDrop"),
            });

            ObservableCollection<ControlExampleSubstitution> Substitutions = new ObservableCollection<ControlExampleSubstitution>() { Substitution1, Substitution2, Substitution3, Substitution4, Substitution5 };
            (sender as ControlExample).Substitutions = Substitutions;
        }
    }
}
