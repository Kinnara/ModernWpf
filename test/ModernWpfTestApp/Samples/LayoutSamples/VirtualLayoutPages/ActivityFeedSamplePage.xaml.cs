// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Linq;
using System.Windows.Controls;

namespace MUXControlsTestApp.Samples
{
    public sealed partial class ActivityFeedSamplePage
    {
        public ActivityFeedSamplePage()
        {
            this.InitializeComponent();
            repeater.ItemsSource = Enumerable.Range(0, 500);
        }
    }
}
