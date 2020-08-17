// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Automation;
using Windows.ApplicationModel.Core;

using NavigationView = ModernWpf.Controls.NavigationView;
using NavigationViewSelectionChangedEventArgs = ModernWpf.Controls.NavigationViewSelectionChangedEventArgs;
using NavigationViewItem = ModernWpf.Controls.NavigationViewItem;

namespace MUXControlsTestApp
{
    public sealed partial class NavigationViewPageDataContext : TestPage
    {
        public NavigationViewPageDataContext()
        {
            this.InitializeComponent();

            for (int i = 0; i < 8; i++)
            {
                var nvi = new NavigationViewItem();
                var itemString = "Item #" + i;
                nvi.Content = itemString;
                nvi.DataContext = itemString + "_DataContext";
                NavView.MenuItems.Add(nvi);
            }

            NavView.SelectedItem = NavView.MenuItems[0];
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            var nvi = args.SelectedItem as NavigationViewItem;
            NavViewSelectedDataContext.Text = nvi.DataContext as string;
        }

        private void GetNavViewActiveVisualStates_Click(object sender, RoutedEventArgs e)
        {
            var visualstates = Utilities.VisualStateHelper.GetCurrentVisualStateName(NavView);
            NavViewActiveVisualStatesResult.Text = string.Join(",", visualstates);
        }
    }
}
