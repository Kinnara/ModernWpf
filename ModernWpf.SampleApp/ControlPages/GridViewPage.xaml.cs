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
        }

        private void ItemTemplate_Checked(object sender, RoutedEventArgs e)
        {
            var tag = (sender as FrameworkElement).Tag;
            if (tag != null)
            {
                string template = tag.ToString();
                ContentGridView.ItemTemplate = (DataTemplate)this.Resources[template];
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

        private void SelectionCheckBox_Click(object sender, RoutedEventArgs e)
        {
            SelectionOutput.Text = string.Empty;
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
                    case "Single":
                        ContentGridView.SelectionMode = SelectionMode.Single;
                        break;
                    case "Multiple":
                        ContentGridView.SelectionMode = SelectionMode.Multiple;
                        break;
                    case "Extended":
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

            // Only update either max-row value or margins
            /*if (sender == WrapItemCount)
            {
                StyledGridIWG.MaximumRowsOrColumns = (int)WrapItemCount.Value;
                return;
            }*/

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
    }
}
