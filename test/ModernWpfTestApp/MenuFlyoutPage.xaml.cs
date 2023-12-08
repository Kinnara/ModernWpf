// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace MUXControlsTestApp
{

    [TopLevelTestPage(Name = "MenuFlyout", Icon = "MenuFlyout.png")]
    public sealed partial class MenuFlyoutPage : TestPage
    {
        public MenuFlyoutPage()
        {
            InitializeComponent();
        }

        private void TestMenuFlyoutItemClick(object sender, object e)
        {
            TestMenuFlyoutItemHeightTextBlock.Text = $"{TestMenuFlyoutItem.ActualHeight}";
            TestMenuFlyoutItemWidthTextBlock.Text = $"{TestMenuFlyoutItem.ActualWidth}";
        }
    }
}
