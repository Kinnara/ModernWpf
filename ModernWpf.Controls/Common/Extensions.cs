using System.Windows;
using System;
using System.Windows.Controls;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    internal static class Extensions
    {
        public static FlyoutBase Flyout(this Button button)
        {
            return FlyoutService.GetFlyout(button);
        }

        public static string DefaultIfNullOrEmpty(this string s, string defaultValue)
        {
            return !string.IsNullOrEmpty(s) ? s : defaultValue;
        }

        /// <summary> 
        /// Executes the specified action if the element is loaded or at the loaded event if it's not loaded.
        /// </summary>
        /// <param name="element">The element where the action should be run.</param>
        /// <param name="invokeAction">An action that takes no parameters.</param>
        public static void ExecuteWhenLoaded(this FrameworkElement element, Action invokeAction)
        {
            if (element.IsLoaded)
            {
                element.RunOnUIThread(invokeAction);
            }
            else
            {
                void ElementLoaded(object o, RoutedEventArgs a)
                {
                    element.Loaded -= ElementLoaded;
                    element.RunOnUIThread(invokeAction);
                }

                element.Loaded += ElementLoaded;
            }
        }
    }
}
