// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using ModernWpf.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ItemsRepeaterTestApp.Samples
{
    public partial class ItemTemplateDemo : Page
    {
        public List<int> Data { get; set; }
        public ItemTemplateDemo()
        {
            Data = Enumerable.Range(0, 1000).ToList();
            DataContext = this;
            InitializeComponent();
        }

        private void OnSelectTemplateKey(RecyclingElementFactory sender, SelectTemplateEventArgs args)
        {
            args.TemplateKey = (((int)args.DataContext) % 2 == 0) ? "even" : "odd";
        }
    }

    public class MySelector : DataTemplateSelector
    {
        public DataTemplate TemplateOdd { get; set; }

        public DataTemplate TemplateEven { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            return (((int)item) % 2 == 0) ? TemplateEven : TemplateOdd;
        }
    }
}
