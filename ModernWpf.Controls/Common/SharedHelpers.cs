// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media;

namespace ModernWpf.Controls
{
    internal static class SharedHelpers
    {
        public static bool IsAnimationsEnabled => SystemParameters.ClientAreaAnimation &&
                                                  RenderCapability.Tier > 0;

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

        public static bool IsFrameworkElementLoaded(FrameworkElement frameworkElement)
        {
            return frameworkElement.IsLoaded;
        }

        // TODO
        internal static void ForwardCollectionChange<T>(
            ObservableCollection<T> source,
            ObservableCollection<T> destination,
            NotifyCollectionChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    destination.Insert(args.NewStartingIndex, (T)args.NewItems[0]);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    destination.RemoveAt(args.OldStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    destination[args.NewStartingIndex] = (T)args.NewItems[0];
                    break;
                case NotifyCollectionChangedAction.Move:
                    destination.Move(args.OldStartingIndex, args.NewStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    CopyList(source, destination);
                    break;
                default:
                    Debug.Assert(false);
                    break;
            }
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

        public static void CopyList<T>(
            IList<T> source,
            IList<T> destination)
        {
            destination.Clear();

            foreach (var element in source)
            {
                destination.Add(element);
            }
        }

        public static bool HasLocalValue(this DependencyObject d, DependencyProperty dp)
        {
            return d.ReadLocalValue(dp) != DependencyProperty.UnsetValue;
        }

        public static Window GetActiveWindow()
        {
            var active = UnsafeNativeMethods.GetActiveWindow();
            foreach (Window window in Application.Current.Windows)
            {
                if (new WindowInteropHelper(window).Handle == active)
                {
                    return window;
                }
            }
            return null;
        }

        public static string SafeSubstring(this string s, int startIndex)
        {
            if (s is null)
            {
                throw new ArgumentNullException(nameof(s));
            }

            if (startIndex > s.Length)
            {
                return string.Empty;
            }

            return s.Substring(startIndex);
        }

        public static bool IndexOf(this UIElementCollection collection, UIElement element, out int index)
        {
            int i = collection.IndexOf(element);
            if (i >= 0)
            {
                index = i;
                return true;
            }
            else
            {
                index = 0;
                return false;
            }
        }
    }
}
