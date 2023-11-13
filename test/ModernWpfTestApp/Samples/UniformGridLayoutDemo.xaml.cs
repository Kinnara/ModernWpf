// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace MUXControlsTestApp.Samples
{
    public sealed partial class UniformGridLayoutDemo
    {
        public IEnumerable<int> collection;

        public UniformGridLayoutDemo()
        {
            collection = Enumerable.Range(0, 40);
            this.InitializeComponent();
            UniformGridRepeater.ItemsSource = collection;
        }

        public void GetRepeaterActualHeightButtonClick(object sender, RoutedEventArgs e)
        {
            RepeaterActualHeightLabel.Text = UniformGridRepeater.ActualHeight.ToString();
        }
    }
}
