using MahApps.Metro.Controls;
using ModernWpf.MahApps.Controls;
using SamplesCommon;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using SamplesCommon.SamplePages;

namespace MahAppsSample.ControlPages
{
    public partial class HamburgerMenuExPage
    {
        private DataTemplate _paneHeaderTemplate;

        public HamburgerMenuExPage()
        {
            InitializeComponent();

            _paneHeaderTemplate = nvSample.HamburgerMenuHeaderTemplate;
            panemc_Check_Click(panemc_Check, null);
            paneFooterCheck_Click(paneFooterCheck, null);
        }

        private void NavigationView_SelectionChanged(object sender, HamburgerMenuSelectionChangedEventArgs args)
        {
            var selectedItem = (HamburgerMenuItem)args.SelectedItem;
            if (selectedItem != null && selectedItem != downloadItem && selectedItem != favoriteItem)
            {
                if (selectedItem == settingsItem)
                {
                    contentFrame.NavigateToType(typeof(SampleSettingsPage));
                }
                else
                {
                    string selectedItemTag = ((string)selectedItem.Tag);
                    ((HamburgerMenuEx)sender).Header = "Sample Page " + selectedItemTag.Substring(selectedItemTag.Length - 1);
                    string pageName = "SamplesCommon.SamplePages." + selectedItemTag;
                    Type pageType = typeof(SamplePage1).Assembly.GetType(pageName);
                    contentFrame.NavigateToType(pageType);
                }
            }
        }

        private void settingsCheck_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as CheckBox).IsChecked == true)
            {
                if (!optionsItems.Contains(settingsItem))
                {
                    optionsItems.Add(settingsItem);
                }
            }
            else
            {
                optionsItems.Remove(settingsItem);
            }
        }

        private void autoSuggestCheck_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as CheckBox).IsChecked == true)
            {
                nvSample.AutoSuggestBox = asb;
            }
            else
            {
                nvSample.AutoSuggestBox = null;
            }
        }

        private void panemc_Check_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as CheckBox).IsChecked == true)
            {
                nvSample.HamburgerMenuHeaderTemplate = _paneHeaderTemplate;
            }
            else
            {
                nvSample.HamburgerMenuHeaderTemplate = null;
            }
        }

        private void paneFooterCheck_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as CheckBox).IsChecked == true)
            {
                if (!optionsItems.Contains(downloadItem))
                {
                    optionsItems.Insert(0, downloadItem);
                }
                if (!optionsItems.Contains(favoriteItem))
                {
                    optionsItems.Insert(1, favoriteItem);
                }
            }
            else
            {
                optionsItems.Remove(downloadItem);
                optionsItems.Remove(favoriteItem);
            }
        }
    }
}
