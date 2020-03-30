using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;

namespace MUXControlsTestApp.Utilities
{
    public static class Extensions
    {
        private static readonly Action NoOpCallback = delegate { };

        public static void UpdateLayoutAndWaitForIdleDispatcher(this UIElement element)
        {
            element.UpdateLayout();
            element.Dispatcher.Invoke(NoOpCallback, DispatcherPriority.ApplicationIdle);
        }

        public static void RegisterPropertyChangedCallback(this FrameworkElement element, DependencyProperty dp, EventHandler callback)
        {
            if (element.IsVisible)
            {
                DependencyPropertyDescriptor.FromProperty(dp, element.GetType()).AddValueChanged(element, callback);
            }

            element.IsVisibleChanged += OnIsVisibleChanged;

            void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
            {
                if ((bool)e.NewValue)
                {
                    DependencyPropertyDescriptor.FromProperty(dp, element.GetType()).AddValueChanged(element, callback);
                }
                else
                {
                    DependencyPropertyDescriptor.FromProperty(dp, element.GetType()).RemoveValueChanged(element, callback);
                }
            }
        }
    }
}
