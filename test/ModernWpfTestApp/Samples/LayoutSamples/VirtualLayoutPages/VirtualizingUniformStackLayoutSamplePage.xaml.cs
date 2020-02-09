// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using ModernWpf.Controls;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MUXControlsTestApp.Samples
{
    public sealed partial class VirtualizingUniformStackLayoutSamplePage : Page
    {
        private RecyclingElementFactory elementFactory;

        ObservableCollection<int> data = new ObservableCollection<int>(Enumerable.Range(0, 1000));
        public VirtualizingUniformStackLayoutSamplePage()
        {
            this.InitializeComponent();
            elementFactory = (RecyclingElementFactory)Resources[nameof(elementFactory)];

            repeater.ItemTemplate = elementFactory;
            repeater.ItemsSource = data;
            bringIntoView.Click += BringIntoView_Click;
            insert.Click += Insert_Click;
        }

        private void BringIntoView_Click(object sender, RoutedEventArgs e)
        {
            int index = 0;
            if (int.TryParse(tb.Text, out index))
            {
                var anchor = repeater.GetOrCreateElement(index);
                ((FrameworkElement)anchor).BringIntoView();
            }
        }

        private void Insert_Click(object sender, RoutedEventArgs e)
        {
            int index = 0;
            if (!int.TryParse(indexTb.Text, out index))
            {
                index = 0;
            }

            int count = 1;
            if (!int.TryParse(countTb.Text, out count))
            {
                count = 1;
            }

            for (int i = 0; i < count; i++)
            {
                data.Insert(index, 50000 + i);
            }

        }
    }
}
