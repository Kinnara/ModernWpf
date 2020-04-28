// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using ModernWpf.Controls;
using System.Windows;
using System.Windows.Controls;

namespace MUXControlsTestApp.Samples
{
    public partial class StoreDemoPage
    {
        public StoreDemoPage()
        {
            InitializeComponent();

            var elementFactory = (RecyclingElementFactory)Resources["elementFactory"];
            elementFactory.Templates.Add("Category", (DataTemplate)Resources["Category"]);
            elementFactory.Templates.Add("Item", (DataTemplate)Resources["Item"]);

            var data = StoreMockData.Create(15, 10);
            outerRepeater.ItemsSource = data;
        }

        private void OnSelectTemplateKey(RecyclingElementFactory sender, SelectTemplateEventArgs args)
        {
            args.TemplateKey = args.DataContext is Category ? "Category" : "Item";
        }
    }
}
