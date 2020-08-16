// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using ModernWpf.Controls;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MUXControlsTestApp.Samples
{
    public partial class ItemTemplateDemo
    {
        public List<int> Data { get; set; }
        public List<MyData> Numbers { get; } = new List<MyData>();

        public ItemTemplateDemo()
        {
            Data = Enumerable.Range(0, 1000).ToList();

            for(int i=0;i<10;i++)
            {
                Numbers.Add(new MyData(i));
            }

            DataContext = this;
            InitializeComponent();
        }

        private void OnSelectTemplateKey(RecyclingElementFactory sender, SelectTemplateEventArgs args)
        {
            args.TemplateKey = (((int)args.DataContext) % 2 == 0) ? "even" : "odd";
        }
    }

    public class MyData
    {
        public int number;

        public MyData(int number)
        {
            this.number = number;
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
