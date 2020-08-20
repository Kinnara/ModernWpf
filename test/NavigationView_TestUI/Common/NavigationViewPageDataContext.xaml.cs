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
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

namespace MUXControlsTestApp
{
    public sealed partial class NavigationViewPageDataContext : TestPage, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private NavViewPageData[] _pages;

        public NavViewPageData[] Pages
        {
            get => _pages;
            set
            {
                _pages = value;
                NotifyPropertyChanged(nameof(Pages));
            }
        }

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

            Pages = new[]
            {
                new NavViewPageData("FirstInvokeChangeItem"),
                new NavViewPageData("SecondInvokeChangeItem"),
                new NavViewPageData("ThirdInvokeChangeItem"),
                new NavViewPageData("FourthInvokeChangeItem"),
                new NavViewPageData("FifthInvokeChangeItem"),
                new NavViewPageData("SixthInvokeChangeItem"),
                new NavViewPageData("SeventhInvokeChangeItem"),
            };
        }

        private void NavigationView_ItemInvoked(NavigationView sender, ModernWpf.Controls.NavigationViewItemInvokedEventArgs args)
        {
            if (_pages != null && _pages.Length == 0)
            {
                return;
            }

            var page = Pages[0];
            var newPages = new List<NavViewPageData>();
            newPages.Add(new NavViewPageData($"Clicked {page.Name}"));
            newPages.AddRange(Pages);

            Pages = newPages.ToArray();
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            var nvi = args.SelectedItem as NavigationViewItem;
            NavViewSelectedDataContext.Text = nvi.DataContext as string ?? "";
        }

        private void GetNavViewActiveVisualStates_Click(object sender, RoutedEventArgs e)
        {
            var visualstates = Utilities.VisualStateHelper.GetCurrentVisualStateName(NavView);
            NavViewActiveVisualStatesResult.Text = string.Join(",", visualstates);
        }

        public class NavViewPageData
        {
            public string Name { get; set; }

            public NavViewPageData(string name)
            {
                Name = name;
            }
        }
    }
}
