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
using MS.Win32;

namespace ModernWpf.Controls
{
    internal static class SharedHelpers
    {
        public static bool IsAnimationsEnabled => SystemParameters.ClientAreaAnimation &&
                                                  RenderCapability.Tier > 0;

        public static bool IsRS1OrHigher() => true;

        public static bool IsRS2OrHigher() => true;

        public static bool IsRS3OrHigher() => true;

        public static bool IsRS4OrHigher() => true;

        public static bool IsRS5OrHigher() => true;

        public static bool IsControlCornerRadiusAvailable() => true;

        public static bool IsThemeShadowAvailable() => false;

        public static bool IsOnXbox() => false;

        public static void QueueCallbackForCompositionRendering(Action callback)
        {
            CompositionTarget.Rendering += onRendering;

            void onRendering(object sender, EventArgs e)
            {
                // Detach event or Rendering will keep calling us back.
                CompositionTarget.Rendering -= onRendering;

                callback();
            }
        }

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

        public static object FindResource(string resource, ResourceDictionary resources, object defaultValue)
        {
            var boxedResource = resource;
            return resources.Contains(boxedResource) ? resources[boxedResource] : defaultValue;
        }

        public static object FindResource(string resource, FrameworkElement element, object defaultValue)
        {
            return element.TryFindResource(resource) ?? defaultValue;
        }

        public static object FindInApplicationResources(string resource, object defaultValue)
        {
            return SharedHelpers.FindResource(resource, Application.Current.Resources, defaultValue);
        }

        public static void SetBinding(
            string pathString,
            DependencyObject target,
            DependencyProperty targetProperty)
        {
            Binding binding = new Binding(pathString)
            {
                RelativeSource = RelativeSource.TemplatedParent
            };

            BindingOperations.SetBinding(target, targetProperty, binding);
        }

        public static bool IsFrameworkElementLoaded(FrameworkElement frameworkElement)
        {
            return frameworkElement.IsLoaded;
        }

        public static AncestorType GetAncestorOfType<AncestorType>(DependencyObject firstGuess) where AncestorType : DependencyObject
        {
            var obj = firstGuess;
            AncestorType matchedAncestor = null;
            while (obj != null && matchedAncestor == null)
            {
                matchedAncestor = obj as AncestorType;
                obj = VisualTreeHelper.GetParent(obj);
            }

            if (matchedAncestor != null)
            {
                return matchedAncestor;
            }
            else
            {
                return null;
            }
        }

        // TODO: WPF
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

        public static IconElement MakeIconElementFrom(IconSource iconSource)
        {
            if (iconSource is FontIconSource fontIconSource)
            {
                FontIcon fontIcon = new FontIcon();

                fontIcon.Glyph = fontIconSource.Glyph;
                fontIcon.FontSize = fontIconSource.FontSize;
                var newForeground = fontIconSource.Foreground;
                if (newForeground != null)
                {
                    fontIcon.Foreground = newForeground;
                }

                if (fontIconSource.FontFamily != null)
                {
                    fontIcon.FontFamily = fontIconSource.FontFamily;
                }

                fontIcon.FontWeight = fontIconSource.FontWeight;
                fontIcon.FontStyle = fontIconSource.FontStyle;
                //fontIcon.IsTextScaleFactorEnabled = fontIconSource.IsTextScaleFactorEnabled;
                //fontIcon.MirroredWhenRightToLeft = fontIconSource.MirroredWhenRightToLeft;

                return fontIcon;
            }
            else if (iconSource is SymbolIconSource symbolIconSource)
            {
                SymbolIcon symbolIcon = new SymbolIcon();
                symbolIcon.Symbol = symbolIconSource.Symbol;
                var newForeground = symbolIconSource.Foreground;
                if (newForeground != null)
                {
                    symbolIcon.Foreground = newForeground;
                }
                return symbolIcon;
            }
            else if (iconSource is BitmapIconSource bitmapIconSource)
            {
                BitmapIcon bitmapIcon = new BitmapIcon();

                if (bitmapIconSource.UriSource != null)
                {
                    bitmapIcon.UriSource = bitmapIconSource.UriSource;
                }

                bitmapIcon.ShowAsMonochrome = bitmapIconSource.ShowAsMonochrome;
                var newForeground = bitmapIconSource.Foreground;
                if (newForeground != null)
                {
                    bitmapIcon.Foreground = newForeground;
                }
                return bitmapIcon;
            }
            else if (iconSource is ImageIconSource imageIconSource)
            {
                ImageIcon imageIcon = new ImageIcon();
                var imageSource = imageIconSource.ImageSource;
                if (imageSource != null)
                {
                    imageIcon.Source = imageSource;
                }
                var newForeground = imageIconSource.Foreground;
                if (newForeground != null)
                {
                    imageIcon.Foreground = newForeground;
                }
                return imageIcon;
            }
            else if (iconSource is PathIconSource pathIconSource)
            {
                PathIcon pathIcon = new PathIcon();

                if (pathIconSource.Data != null)
                {
                    pathIcon.Data = pathIconSource.Data;
                }
                var newForeground = pathIconSource.Foreground;
                if (newForeground != null)
                {
                    pathIcon.Foreground = newForeground;
                }
                return pathIcon;
            }
            else if (iconSource is AnimatedIconSource animatedIconSource)
            {
                AnimatedIcon animatedIcon = new AnimatedIcon();
                var source = animatedIconSource.Source;
                if (source != null)
                {
                    animatedIcon.Source = source;
                }
                var fallbackIconSource = animatedIconSource.FallbackIconSource;
                if (fallbackIconSource != null)
                {
                    animatedIcon.FallbackIconSource = fallbackIconSource;
                }
                var newForeground = animatedIconSource.Foreground;
                if (newForeground != null)
                {
                    animatedIcon.Foreground = newForeground;
                }
                animatedIcon.FontSize = animatedIconSource.FontSize;
                return animatedIcon;
            }
            return null;
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

        public static Window GetActiveWindow()
        {
            var activeWindow = UnsafeNativeMethods.GetActiveWindow();
            if (activeWindow != IntPtr.Zero)
            {
                return HwndSource.FromHwnd(activeWindow)?.RootVisual as Window;
            }
            return null;
        }

        public static string SafeSubstring(this string s, int startIndex)
        {
            return s.SafeSubstring(startIndex, s.Length - startIndex);
        }

        public static string SafeSubstring(this string s, int startIndex, int length)
        {
            if (s is null)
            {
                throw new ArgumentNullException(nameof(s));
            }

            if (startIndex > s.Length)
            {
                return string.Empty;
            }

            if (length > s.Length - startIndex)
            {
                length = s.Length - startIndex;
            }

            return s.Substring(startIndex, length);
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

        public static string TryGetStringRepresentationFromObject(object obj)
        {
            return obj?.ToString() ?? string.Empty;
        }
    }
}
