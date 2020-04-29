// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;
using System.Windows.Controls;
using Frame = ModernWpf.Controls.Frame;

namespace MUXControlsTestApp
{
    [TopLevelTestPage(Name = "CommandBarFlyout", Icon = "CommandBarFlyout.png")]
    public sealed partial class CommandBarFlyoutMainPage : TestPage
    {
        public CommandBarFlyoutMainPage()
        {
            this.InitializeComponent();
        }

        public void OnCommandBarFlyoutTestsClicked(object sender, object args)
        {
            var rootFrame = Application.Current.MainWindow.Content as Frame;
            rootFrame.NavigateWithoutAnimation(typeof(CommandBarFlyoutPage), "CommandBarFlyout Tests");
        }

        public void OnExtraCommandBarFlyoutTestsClicked(object sender, object args)
        {
            var rootFrame = Application.Current.MainWindow.Content as Frame;
            rootFrame.NavigateWithoutAnimation(typeof(ExtraCommandBarFlyoutPage), "Extra CommandBarFlyout Tests");
        }
    }
}
