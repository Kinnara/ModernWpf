// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using ModernWpf.Controls;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ItemsRepeaterTestApp.Samples.Selection
{
    public sealed partial class GroupedSample : Page
    {
        private SelectionModel selectionModel;

        private Random _rnd = new Random(12345);
        private StackLayout _layout = new StackLayout();
        private ObservableCollection<object> _groupedData = new ObservableCollection<object>(Enumerable.Range(0, 10).Select(i => Enumerable.Range(0, 20).Select(j => j.ToString()).ToList()));

        public GroupedSample()
        {
            this.InitializeComponent();
            selectionModel = (SelectionModel)Resources[nameof(selectionModel)];
            repeater.ItemsSource = _groupedData;
            selectionModel.Source = _groupedData;
        }

        private void OnMultipleSelectionClicked(object sender, RoutedEventArgs e)
        {
            selectionModel.SingleSelect = multipleSelection.IsChecked.Value ? false : true;
        }

        private void OnSelectTemplateKey(RecyclingElementFactory sender, SelectTemplateEventArgs args)
        {
            args.TemplateKey = args.DataContext is string ? "RepeaterItemTemplate" : "RepeaterGroupTemplate";
        }

        private void groupRepeater_ElementPrepared(ItemsRepeater sender, ItemsRepeaterElementPreparedEventArgs args)
        {
            if (args.Element is GroupedRepeaterItem)
            {
                (args.Element as GroupedRepeaterItem).RepeatedIndex = args.Index;
            }
        }

        private void OnBackClicked(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void OnTreeViewClicked(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new TreeViewSample());
        }

        private void groupRepeater_ElementIndexChanged(ItemsRepeater sender, ItemsRepeaterElementIndexChangedEventArgs args)
        {
            if (args.Element is GroupedRepeaterItem)
            {
                (args.Element as GroupedRepeaterItem).RepeatedIndex = args.NewIndex;
            }
        }
    }
}
