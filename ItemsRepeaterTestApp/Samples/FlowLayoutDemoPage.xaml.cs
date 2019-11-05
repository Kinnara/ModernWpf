// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Linq;
using System.Windows.Controls;

namespace ItemsRepeaterTestApp.Samples
{
    public partial class FlowLayoutDemoPage : Page
    {
        public FlowLayoutDemoPage()
        {
            InitializeComponent();

            repeater.ItemsSource = (from i in Enumerable.Range(0, 1000)
                                    select new MyItem()
                                    {
                                        Label = i.ToString(),
                                        Height = 50 + i % 2 * 50
                                    }).ToList();
        }
    }

    public class MyItem
    {
        public string Label { get; set; }
        public float Height { get; set; }
    }
}
