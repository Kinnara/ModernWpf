// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Data;

namespace ModernWpf.Controls
{
    internal static class SharedHelpers
    {
        public static bool DoRectsIntersect(
            Rect rect1,
            Rect rect2)
        {
            var doIntersect =
                !(rect1.Width <= 0 || rect1.Height <= 0 || rect2.Width <= 0 || rect2.Height <= 0) &&
                (rect2.X <= rect1.X + rect1.Width) &&
                (rect2.X + rect2.Width >= rect1.X) &&
                (rect2.Y <= rect1.Y + rect1.Height) &&
                (rect2.Y + rect2.Height >= rect1.Y);
            return doIntersect;
        }

        public static void RaiseAutomationPropertyChangedEvent(UIElement element, object oldValue, object newValue)
        {
            if (FrameworkElementAutomationPeer.FromElement(element) is AutomationPeer peer)
            {
                peer.RaisePropertyChangedEvent(
                    ExpandCollapsePatternIdentifiers.ExpandCollapseStateProperty,
                    oldValue,
                    newValue);
            }
        }

        public static BindingExpressionBase SetBinding(
            this FrameworkElement element,
            DependencyProperty dp,
            DependencyProperty sourceDP,
            DependencyObject source)
        {
            return element.SetBinding(dp, new Binding { Path = new PropertyPath(sourceDP), Source = source });
        }

        public static bool HasLocalValue(this DependencyObject d, DependencyProperty dp)
        {
            return d.ReadLocalValue(dp) != DependencyProperty.UnsetValue;
        }
    }
}
