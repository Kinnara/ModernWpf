// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using ModernWpf.Controls;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace MUXControlsTestApp
{
    [TopLevelTestPage(Name = "ContentDialog")]
    public sealed partial class ContentDialogPage : TestPage
    {
        public ContentDialogPage()
        {
            this.InitializeComponent();
        }

        private async void ShowDialog_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new TestDialog
            {
                Title = "Title",
                Content = "Content",
                IsPrimaryButtonEnabled = true,
                PrimaryButtonText = "PrimaryButton",
                SecondaryButtonText = "SecondaryButton",
                CloseButtonText = "CloseButton"
            };
            dialog.Opened += Dialog_Opened;
            dialog.Closing += Dialog_Closing;
            dialog.Closed += Dialog_Closed;
            var result = await dialog.ShowAsync();
            Debug.WriteLine(result);
        }

        private void Dialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            Debug.WriteLine("Opened");
        }

        private void Dialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            Debug.WriteLine("Closing");
        }

        private void Dialog_Closed(ContentDialog sender, ContentDialogClosedEventArgs args)
        {
            Debug.WriteLine("Closed");
        }

        private class TestDialog : ContentDialog
        {
            public override void OnApplyTemplate()
            {
                base.OnApplyTemplate();

                var dialogShowingStates = GetTemplateChild("DialogShowingStates") as VisualStateGroup;
                var backgroundElement = GetTemplateChild("BackgroundElement") as FrameworkElement;
                Debug.Assert(dialogShowingStates != null && backgroundElement != null);
                dialogShowingStates.CurrentStateChanged += (s, e) =>
                {
                    //Debug.WriteLine($"OldState: {e.OldState?.Name}, NewState: {e.NewState.Name}");
                    if (e.NewState.Name == "DialogShowing")
                    {
                        Dispatcher.BeginInvoke(() =>
                        {
                            Debug.Assert(KeyboardNavigation.GetTabNavigation(backgroundElement) == KeyboardNavigationMode.Cycle);
                        });
                    }
                };
            }
        }
    }
}
