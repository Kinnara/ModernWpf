// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Automation;
using System;

using NavigationViewDisplayMode = ModernWpf.Controls.NavigationViewDisplayMode;
using NavigationView = ModernWpf.Controls.NavigationView;
using NavigationViewSelectionChangedEventArgs = ModernWpf.Controls.NavigationViewSelectionChangedEventArgs;
using NavigationViewItem = ModernWpf.Controls.NavigationViewItem;
using NavigationViewBackButtonVisible = ModernWpf.Controls.NavigationViewBackButtonVisible;
using NavigationViewItemSeparator = ModernWpf.Controls.NavigationViewItemSeparator;
using NavigationViewDisplayModeChangedEventArgs = ModernWpf.Controls.NavigationViewDisplayModeChangedEventArgs;
using ModernWpf.Controls;

namespace MUXControlsTestApp
{
    /// <summary>
    /// Verify ShouldPreserveNavigationViewRS3Behavior
    /// </summary>
    public sealed partial class NavigationViewRS3Page : TestPage
    {
         public NavigationViewRS3Page()
        {
            this.InitializeComponent();

            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ChangeTestFrameVisibility(Visibility.Collapsed);

            //CoreApplicationViewTitleBar titleBar = CoreApplication.GetCurrentView().TitleBar;
            //titleBar.ExtendViewIntoTitleBar = true;
            TitleBar.SetExtendViewIntoTitleBar(Application.Current.MainWindow, true);

            NavView.IsBackButtonVisible = NavigationViewBackButtonVisible.Visible;
            NavView.IsBackEnabled = true;
        }
        private void TestFrameCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            ChangeTestFrameVisibility(Visibility.Visible);
            // Show titlebar to reenable clicking the buttons in the test frame
            //CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = false;
            TitleBar.SetExtendViewIntoTitleBar(Application.Current.MainWindow, false);
        }

        private void TestFrameCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            ChangeTestFrameVisibility(Visibility.Collapsed);
            // Hide titlebar again in case we hid it before
            //CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            TitleBar.SetExtendViewIntoTitleBar(Application.Current.MainWindow, true);
        }

        private void ChangeTestFrameVisibility(Visibility visibility)
        {
            var testFrame = WindowEx.Current.Content as TestFrame;
            testFrame.ChangeBarVisibility(visibility);
        }

        private void GetTopPaddingHeight_Click(object sender, RoutedEventArgs e)
        {
            Grid rootGrid = VisualTreeHelper.GetChild(NavView, 0) as Grid;
            if (rootGrid != null)
            {
                Grid paneContentGrid = rootGrid.FindName("TogglePaneTopPadding") as Grid;
                TestResult.Text = paneContentGrid.Height.ToString();
            }           
        }

        private void GetToggleButtonRowHeight_Click(object sender, RoutedEventArgs e)
        {
            Grid rootGrid = VisualTreeHelper.GetChild(NavView, 0) as Grid;
            if (rootGrid != null)
            {
                Grid paneContentGrid = rootGrid.FindName("PaneContentGrid") as Grid;
                TestResult.Text = paneContentGrid.RowDefinitions[1].Height.ToString();
            }
        }

    }
}
