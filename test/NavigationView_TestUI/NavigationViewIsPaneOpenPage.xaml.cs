// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;

namespace MUXControlsTestApp
{
    public sealed partial class NavigationViewIsPaneOpenPage : TestPage
    {
        public NavigationViewIsPaneOpenPage()
        {
            this.InitializeComponent();
        }

        // This NavigationViewItem can be used to check whether selecting an item right after showing it still shows the selection indicator
        // See https://github.com/microsoft/microsoft-ui-xaml/issues/2941 for context
        private void NavigationView_ItemInvoked(ModernWpf.Controls.NavigationView sender, ModernWpf.Controls.NavigationViewItemInvokedEventArgs args)
        {
            if((args.InvokedItem as string) == "Apps")
            {
                CollapsedItem.Visibility = Visibility.Visible;

                NavView.SelectedItem = CollapsedItem;
            }
        }
    }
}
