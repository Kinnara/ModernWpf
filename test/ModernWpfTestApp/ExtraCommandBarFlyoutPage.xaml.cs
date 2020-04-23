// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;

namespace MUXControlsTestApp
{
    public sealed partial class ExtraCommandBarFlyoutPage : TestPage
    {
        public ExtraCommandBarFlyoutPage()
        {
            InitializeComponent();
        }

        private void OnClearClipboardContentsClicked(object sender, object args)
        {
            Clipboard.Clear();
        }

        private void OnCountPopupsClicked(object sender, object args)
        {
            PopupCountTextBox.Text = VisualTreeHelperEx.GetOpenPopups(WindowEx.Current).Count.ToString();
        }
    }
}
