﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Automation;

namespace MUXControlsTestApp
{
    public sealed partial class CommandBarFlyoutPage : TestPage
    {
        CommandBarFlyout Flyout1 => GetResource<CommandBarFlyout>();
        CommandBarFlyout Flyout2 => GetResource<CommandBarFlyout>();
        CommandBarFlyout Flyout3 => GetResource<CommandBarFlyout>();
        CommandBarFlyout Flyout4 => GetResource<CommandBarFlyout>();
        CommandBarFlyout Flyout5 => GetResource<CommandBarFlyout>();
        CommandBarFlyout Flyout6 => GetResource<CommandBarFlyout>();
        CommandBarFlyout Flyout7 => GetResource<CommandBarFlyout>();
        CommandBarFlyout Flyout8 => GetResource<CommandBarFlyout>();
        CommandBarFlyout Flyout9 => GetResource<CommandBarFlyout>();

        public CommandBarFlyoutPage()
        {
            InitializeComponent();

            /*if (ApiInformation.IsTypePresent("Windows.UI.Xaml.Input.KeyboardAccelerator"))
            {
                UndoButton1.KeyboardAccelerators.Add(new KeyboardAccelerator() { Key = VirtualKey.Z, Modifiers = VirtualKeyModifiers.Control });
                UndoButton2.KeyboardAccelerators.Add(new KeyboardAccelerator() { Key = VirtualKey.Z, Modifiers = VirtualKeyModifiers.Control });
                UndoButton3.KeyboardAccelerators.Add(new KeyboardAccelerator() { Key = VirtualKey.Z, Modifiers = VirtualKeyModifiers.Control });
                UndoButton4.KeyboardAccelerators.Add(new KeyboardAccelerator() { Key = VirtualKey.Z, Modifiers = VirtualKeyModifiers.Control });
                UndoButton5.KeyboardAccelerators.Add(new KeyboardAccelerator() { Key = VirtualKey.Z, Modifiers = VirtualKeyModifiers.Control });
                UndoButton6.KeyboardAccelerators.Add(new KeyboardAccelerator() { Key = VirtualKey.Z, Modifiers = VirtualKeyModifiers.Control });
                UndoButton7.KeyboardAccelerators.Add(new KeyboardAccelerator() { Key = VirtualKey.Z, Modifiers = VirtualKeyModifiers.Control });

                RedoButton1.KeyboardAccelerators.Add(new KeyboardAccelerator() { Key = VirtualKey.Y, Modifiers = VirtualKeyModifiers.Control });
                RedoButton2.KeyboardAccelerators.Add(new KeyboardAccelerator() { Key = VirtualKey.Y, Modifiers = VirtualKeyModifiers.Control });
                RedoButton3.KeyboardAccelerators.Add(new KeyboardAccelerator() { Key = VirtualKey.Y, Modifiers = VirtualKeyModifiers.Control });
                RedoButton4.KeyboardAccelerators.Add(new KeyboardAccelerator() { Key = VirtualKey.Y, Modifiers = VirtualKeyModifiers.Control });
                RedoButton5.KeyboardAccelerators.Add(new KeyboardAccelerator() { Key = VirtualKey.Y, Modifiers = VirtualKeyModifiers.Control });
                RedoButton6.KeyboardAccelerators.Add(new KeyboardAccelerator() { Key = VirtualKey.Y, Modifiers = VirtualKeyModifiers.Control });
                RedoButton7.KeyboardAccelerators.Add(new KeyboardAccelerator() { Key = VirtualKey.Y, Modifiers = VirtualKeyModifiers.Control });

                SelectAllButton1.KeyboardAccelerators.Add(new KeyboardAccelerator() { Key = VirtualKey.A, Modifiers = VirtualKeyModifiers.Control });
                SelectAllButton2.KeyboardAccelerators.Add(new KeyboardAccelerator() { Key = VirtualKey.A, Modifiers = VirtualKeyModifiers.Control });
                SelectAllButton3.KeyboardAccelerators.Add(new KeyboardAccelerator() { Key = VirtualKey.A, Modifiers = VirtualKeyModifiers.Control });
                SelectAllButton4.KeyboardAccelerators.Add(new KeyboardAccelerator() { Key = VirtualKey.A, Modifiers = VirtualKeyModifiers.Control });
                SelectAllButton5.KeyboardAccelerators.Add(new KeyboardAccelerator() { Key = VirtualKey.A, Modifiers = VirtualKeyModifiers.Control });
                SelectAllButton6.KeyboardAccelerators.Add(new KeyboardAccelerator() { Key = VirtualKey.A, Modifiers = VirtualKeyModifiers.Control });
                SelectAllButton7.KeyboardAccelerators.Add(new KeyboardAccelerator() { Key = VirtualKey.A, Modifiers = VirtualKeyModifiers.Control });
            }*/

            //if (ApiInformation.IsPropertyPresent("Windows.UI.Xaml.UIElement", "ContextFlyout"))
            {
                ContextFlyoutService.SetContextFlyout(FlyoutTarget1, Flyout1);
                ContextFlyoutService.SetContextFlyout(FlyoutTarget2, Flyout2);
                ContextFlyoutService.SetContextFlyout(FlyoutTarget3, Flyout3);
                ContextFlyoutService.SetContextFlyout(FlyoutTarget4, Flyout4);
                ContextFlyoutService.SetContextFlyout(FlyoutTarget5, Flyout5);
                ContextFlyoutService.SetContextFlyout(FlyoutTarget6, Flyout6);
                ContextFlyoutService.SetContextFlyout(FlyoutTarget7, Flyout7);
            }

            //if (ApiInformation.IsEnumNamedValuePresent("Windows.UI.Xaml.Controls.Primitives.FlyoutPlacementMode", "TopEdgeAlignedLeft"))
            {
                Flyout1.Placement = FlyoutPlacementMode.TopEdgeAlignedLeft;
                Flyout2.Placement = FlyoutPlacementMode.TopEdgeAlignedLeft;
                Flyout3.Placement = FlyoutPlacementMode.TopEdgeAlignedLeft;
                Flyout4.Placement = FlyoutPlacementMode.TopEdgeAlignedLeft;
                Flyout5.Placement = FlyoutPlacementMode.TopEdgeAlignedLeft;
                Flyout6.Placement = FlyoutPlacementMode.TopEdgeAlignedLeft;
                Flyout7.Placement = FlyoutPlacementMode.TopEdgeAlignedLeft;
            }
        }

        public void OnElementClicked(object sender, object args)
        {
            RecordEvent(sender, "clicked");
        }

        public void OnElementChecked(object sender, object args)
        {
            RecordEvent(sender, "checked");
        }

        public void OnElementUnchecked(object sender, object args)
        {
            RecordEvent(sender, "unchecked");
        }

        public void OnFlyoutOpened(object sender, object args)
        {
            IsFlyoutOpenCheckBox.IsChecked = true;
        }

        public void OnFlyoutClosed(object sender, object args)
        {
            IsFlyoutOpenCheckBox.IsChecked = false;
        }

        private void RecordEvent(string eventString)
        {
            StatusReportingTextBox.Text = eventString;
        }

        private void RecordEvent(object sender, string eventString)
        {
            DependencyObject senderAsDO = sender as DependencyObject;

            if (senderAsDO != null)
            {
                RecordEvent(AutomationProperties.GetAutomationId(senderAsDO) + " " + eventString);
            }
        }

        private void OnFlyoutTarget1Click(object sender, RoutedEventArgs e)
        {
            ShowFlyoutAt(Flyout1, FlyoutTarget1);
        }

        private void OnFlyoutTarget2Click(object sender, RoutedEventArgs e)
        {
            ShowFlyoutAt(Flyout2, FlyoutTarget2);
        }

        private void OnFlyoutTarget3Click(object sender, RoutedEventArgs e)
        {
            ShowFlyoutAt(Flyout3, FlyoutTarget3);
        }

        private void OnFlyoutTarget4Click(object sender, RoutedEventArgs e)
        {
            ShowFlyoutAt(Flyout4, FlyoutTarget4);
        }

        private void OnFlyoutTarget5Click(object sender, RoutedEventArgs e)
        {
            ShowFlyoutAt(Flyout5, FlyoutTarget5);
        }

        private void OnFlyoutTarget6Click(object sender, RoutedEventArgs e)
        {
            ShowFlyoutAt(Flyout6, FlyoutTarget6, FlyoutShowMode.Standard);
        }

        private void OnFlyoutTarget7Click(object sender, RoutedEventArgs e)
        {
            ShowFlyoutAt(Flyout7, FlyoutTarget7);
        }

        private void OnFlyoutTarget8Click(object sender, RoutedEventArgs e)
        {
            ShowFlyoutAt(Flyout8, FlyoutTarget8);
        }

        private void OnFlyoutTarget9Click(object sender, RoutedEventArgs e)
        {
            ShowFlyoutAt(Flyout9, FlyoutTarget9);
        }

        private void ShowFlyoutAt(FlyoutBase flyout, FrameworkElement targetElement, FlyoutShowMode showMode = FlyoutShowMode.Transient)
        {
            //if (PlatformConfiguration.IsOsVersionGreaterThanOrEqual(OSVersion.Redstone5))
            //{
            //    flyout.ShowAt(targetElement, new FlyoutShowOptions { Placement = FlyoutPlacementMode.TopEdgeAlignedLeft, ShowMode = showMode });
            //}
            //else
            {
                flyout.Placement = FlyoutPlacementMode.Top;
                flyout.ShowAt(targetElement);
            }
        }

        private void IsRTLCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            FlowDirection = FlowDirection.RightToLeft;
        }

        private void IsRTLCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            FlowDirection = FlowDirection.LeftToRight;
        }

        private T GetResource<T>([CallerMemberName] string key = null)
        {
            return (T)((FrameworkElement)Content).Resources[key];
        }
    }
}
