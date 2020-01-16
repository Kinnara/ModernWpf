// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Linq;
using System.Windows.Controls;

namespace ItemsRepeaterTestApp.Samples
{
    public sealed partial class ActivityFeedSamplePage : Page
    {
        public ActivityFeedSamplePage()
        {
            this.InitializeComponent();
            repeater.ItemsSource = Enumerable.Range(0, 500);
        }
    }
}
