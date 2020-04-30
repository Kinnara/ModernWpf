using ModernWpf.Controls;
using System;
using System.Windows;
using System.Windows.Controls;
using ToggleSwitch = ModernWpf.Controls.ToggleSwitch;

namespace MUXControlsTestApp
{
    [TopLevelTestPage(Name = "SplitView", Icon="SplitView.png")]
    public sealed partial class SplitViewPage : TestPage
    {
        public SplitViewPage()
        {
            this.InitializeComponent();
        }

        private void DisplayModeCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            splitView.DisplayMode = (SplitViewDisplayMode)Enum.Parse(typeof(SplitViewDisplayMode), (e.AddedItems[0] as ComboBoxItem).Content.ToString());
        }

        private void Pane_Opening(SplitView sender, object args)
        {

        }

        private void Pane_Opened(SplitView sender, object e)
        {
            paneOpenCheckbox.IsChecked = true;
        }

        private void Pane_Closing(SplitView sender, SplitViewPaneClosingEventArgs args)
        {
        }

        private void Pane_Closed(SplitView sender, object e)
        {
            paneOpenCheckbox.IsChecked = false;
        }

        private void PaneOpenCheckbox_CheckChanged(object sender, RoutedEventArgs e)
        {
            splitView.IsPaneOpen = paneOpenCheckbox.IsChecked.GetValueOrDefault();
        }

        private void PanePlacement_Toggled(object sender, RoutedEventArgs e)
        {
            var ts = sender as ToggleSwitch;
            if (ts.IsOn)
            {
                splitView.PanePlacement = SplitViewPanePlacement.Right;
            }
            else
            {
                splitView.PanePlacement = SplitViewPanePlacement.Left;
            }
        }
    }
}
