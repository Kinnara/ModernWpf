using System;
using System.Windows;

namespace ModernWpf
{
    internal static class FrameworkElementExtensions
    {
        public static void InvokeOnInitialized<T>(this T element, Action a) where T : FrameworkElement
        {
            if (element.IsInitialized)
            {
                a();
            }
            else
            {
                element.Initialized += onInitialized;
            }

            void onInitialized(object sender, EventArgs e)
            {
                element.Initialized -= onInitialized;
                a();
            }
        }

        public static void InvokeOnInitialized<T>(this T element, Action<T> a) where T : FrameworkElement
        {
            if (element.IsInitialized)
            {
                a(element);
            }
            else
            {
                element.Initialized += onInitialized;
            }

            void onInitialized(object sender, EventArgs e)
            {
                element.Initialized -= onInitialized;
                a(element);
            }
        }

        public static void InvokeOnLoaded<T>(this T element, Action a) where T : FrameworkElement
        {
            if (element.IsLoaded)
            {
                a();
            }
            else
            {
                element.Loaded += onLoaded;
            }

            void onLoaded(object sender, RoutedEventArgs e)
            {
                element.Loaded -= onLoaded;
                a();
            }
        }

        public static void InvokeOnLoaded<T>(this T element, Action<T> a) where T : FrameworkElement
        {
            if (element.IsLoaded)
            {
                a(element);
            }
            else
            {
                element.Loaded += onLoaded;
            }

            void onLoaded(object sender, RoutedEventArgs e)
            {
                element.Loaded -= onLoaded;
                a(element);
            }
        }
    }
}
