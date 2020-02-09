// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using MUXControlsTestApp.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace ModernWpf.Tests.MUXControls.ApiTests.RepeaterTests.Common
{
    public enum ScrollOrientation
    {
        Vertical,
        Horizontal
    }

    public static class SharedHelpers
    {
        public static DataTemplate GetDataTemplate(string content)
        {
            return (DataTemplate)XamlReader.Parse(
                       string.Format(@"<DataTemplate  
                            xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>
                           {0}
                        </DataTemplate>", content));
        }

        public static void RunActionsWithWait(Action[] actions)
        {
            foreach (var action in actions)
            {
                RunOnUIThread.Execute(() =>
                {
                    action();
                });

                IdleSynchronizer.Wait();
            }
        }

        public static Orientation ToLayoutOrientation(this ScrollOrientation scrollOrientation)
        {
            return scrollOrientation == ScrollOrientation.Horizontal ? Orientation.Horizontal : Orientation.Vertical;
        }

        public static Orientation ToOrthogonalLayoutOrientation(this ScrollOrientation scrollOrientation)
        {
            return scrollOrientation == ScrollOrientation.Horizontal ? Orientation.Vertical : Orientation.Horizontal;
        }

        public static UIElement ElementAt(this UIElementCollection collection, int index) => collection[index];
    }
}
