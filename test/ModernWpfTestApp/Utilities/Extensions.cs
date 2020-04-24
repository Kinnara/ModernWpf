using ModernWpf;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;

namespace MUXControlsTestApp.Utilities
{
    internal static class Extensions
    {
        private static readonly Action NoOpCallback = delegate { };

        public static void WaitForIdleDispatcher(this DependencyObject element)
        {
            element.Dispatcher.Invoke(NoOpCallback, DispatcherPriority.ApplicationIdle);
        }

        public static void UpdateLayout(this UIElement element, bool waitForIdleDispatcher)
        {
            element.UpdateLayout();
            if (waitForIdleDispatcher)
            {
                element.WaitForIdleDispatcher();
            }
        }

        public static void RegisterPropertyChangedCallback(this FrameworkElement element, DependencyProperty dp, DependencyPropertyChangedCallback callback)
        {
            if (element.IsVisible)
            {
                DependencyPropertyDescriptor.FromProperty(dp, element.GetType()).AddValueChanged(element, OnValueChanged);
            }

            element.IsVisibleChanged += OnIsVisibleChanged;

            void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
            {
                if ((bool)e.NewValue)
                {
                    DependencyPropertyDescriptor.FromProperty(dp, element.GetType()).AddValueChanged(element, OnValueChanged);
                }
                else
                {
                    DependencyPropertyDescriptor.FromProperty(dp, element.GetType()).RemoveValueChanged(element, OnValueChanged);
                }
            }

            void OnValueChanged(object sender, EventArgs e)
            {
                callback((DependencyObject)sender, dp);
            }
        }
    }
}
