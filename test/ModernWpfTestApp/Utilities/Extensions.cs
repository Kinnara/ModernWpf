using ModernWpf;
using ModernWpf.Controls.Primitives;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace MUXControlsTestApp.Utilities
{
    internal static class Extensions
    {
        public static void UpdateLayout(this UIElement element, bool waitForIdleDispatcher)
        {
            element.UpdateLayout();
            if (waitForIdleDispatcher)
            {
                DispatcherHelper.DoEvents(DispatcherPriority.ApplicationIdle);
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

        public static CornerRadius GetCornerRadius(this Control control)
        {
            return ControlHelper.GetCornerRadius(control);
        }

        public static void SetCornerRadius(this Control control, CornerRadius value)
        {
            ControlHelper.SetCornerRadius(control, value);
        }
    }
}
