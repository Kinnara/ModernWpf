// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using ModernWpf.Media.Animation;
using System;
using System.Windows;
using Frame = ModernWpf.Controls.Frame;

namespace MUXControlsTestApp
{
    public class NavigateToTestCommand : System.Windows.Input.ICommand
    {
        public event EventHandler CanExecuteChanged;

        public Frame Frame { get; set; }

        public NavigateToTestCommand()
        {
            if (CanExecuteChanged != null)
            {
                CanExecuteChanged(this, null);
            }
        }


        public bool CanExecute(object parameter)
        {
            return parameter != null && parameter is TestDeclaration;
        }

        public void Execute(object parameter)
        {
            var testDeclaration = parameter as TestDeclaration;
            var rootFrame = Frame != null ? Frame : Application.Current.MainWindow.Content as Frame;
            rootFrame.NavigateWithoutAnimation(testDeclaration.PageType, testDeclaration.Name);
        }
    }

    static class FrameExtensions
    {
        public static void NavigateWithoutAnimation(this Frame frame, Type sourcePageType)
        {
            //if (ApiInformation.IsTypePresent("Windows.UI.Xaml.Media.Animation.SuppressNavigationTransitionInfo"))
            {
                frame.Navigate(sourcePageType, null, new SuppressNavigationTransitionInfo());
            }
            //else
            //{
            //    frame.Navigate(sourcePageType);
            //}
        }

        public static void NavigateWithoutAnimation(this Frame frame, Type sourcePageType, object parameter)
        {
            //if (ApiInformation.IsTypePresent("Windows.UI.Xaml.Media.Animation.SuppressNavigationTransitionInfo"))
            {
                frame.Navigate(sourcePageType, parameter, new SuppressNavigationTransitionInfo());
            }
            //else
            //{
            //    frame.Navigate(sourcePageType, parameter);
            //}
        }
    }
}
