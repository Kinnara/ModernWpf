// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using ModernWpf.Controls;

namespace ModernWpfTestApp
{
    [TopLevelTestPage(Name = "AutoSuggestBox", Icon = "AutoSuggestBox.png")]
    public sealed partial class AutoSuggestBoxPage : TestPage
    {
        string[] suggestions =
        {
            "Lorem",
            "ipsum",
            "dolor",
            "sit",
            "amet"
        };

        public AutoSuggestBoxPage()
        {
            this.InitializeComponent();
        }

        private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            sender.ItemsSource = string.IsNullOrWhiteSpace(args.QueryText) ? null : suggestions;
        }
    }
}
