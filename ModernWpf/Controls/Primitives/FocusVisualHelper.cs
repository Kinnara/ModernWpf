using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace ModernWpf.Controls.Primitives
{
    public static class FocusVisualHelper
    {
        #region IsSystemFocusVisual

        public static bool GetIsSystemFocusVisual(Control control)
        {
            return (bool)control.GetValue(IsSystemFocusVisualProperty);
        }

        public static void SetIsSystemFocusVisual(Control control, bool value)
        {
            control.SetValue(IsSystemFocusVisualProperty, value);
        }

        public static readonly DependencyProperty IsSystemFocusVisualProperty =
            DependencyProperty.RegisterAttached(
                "IsSystemFocusVisual",
                typeof(bool),
                typeof(FocusVisualHelper),
                new PropertyMetadata(OnIsSystemFocusVisualChanged));

        private static void OnIsSystemFocusVisualChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (Control)d;
            if ((bool)e.NewValue)
            {
                control.IsVisibleChanged += OnControlIsVisibleChanged;
            }
            else
            {
                control.IsVisibleChanged -= OnControlIsVisibleChanged;
                control.ClearValue(FrameworkElement.MarginProperty);
            }
        }

        #endregion

        #region IsSystemFocusVisualVisible

        public static bool GetIsSystemFocusVisualVisible(FrameworkElement element)
        {
            return (bool)element.GetValue(IsSystemFocusVisualVisibleProperty);
        }

        private static void SetIsSystemFocusVisualVisible(FrameworkElement element, bool value)
        {
            element.SetValue(IsSystemFocusVisualVisiblePropertyKey, value);
        }

        private static readonly DependencyPropertyKey IsSystemFocusVisualVisiblePropertyKey =
            DependencyProperty.RegisterAttachedReadOnly(
                "IsSystemFocusVisualVisible",
                typeof(bool),
                typeof(FocusVisualHelper),
                null);

        public static readonly DependencyProperty IsSystemFocusVisualVisibleProperty =
            IsSystemFocusVisualVisiblePropertyKey.DependencyProperty;

        #endregion

        #region FocusedElement

        private static FrameworkElement GetFocusedElement(Control control)
        {
            return (FrameworkElement)control.GetValue(FocusedElementProperty);
        }

        private static void SetFocusedElement(Control control, FrameworkElement value)
        {
            control.SetValue(FocusedElementProperty, value);
        }

        private static readonly DependencyProperty FocusedElementProperty =
            DependencyProperty.RegisterAttached(
                "FocusedElement",
                typeof(FrameworkElement),
                typeof(FocusVisualHelper));

        #endregion

        private static void OnControlIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var control = (Control)sender;
            if ((bool)e.NewValue)
            {
                if ((VisualTreeHelper.GetParent(control) as Adorner)?.AdornedElement is FrameworkElement focusedElement)
                {
                    SetIsSystemFocusVisualVisible(focusedElement, true);
                    control.Margin = FrameworkElementHelper.GetFocusVisualMargin(focusedElement);
                    SetFocusedElement(control, focusedElement);
                }
            }
            else
            {
                FrameworkElement focusedElement = GetFocusedElement(control);
                if (focusedElement != null)
                {
                    focusedElement.ClearValue(IsSystemFocusVisualVisiblePropertyKey);
                    control.ClearValue(FrameworkElement.MarginProperty);
                    control.ClearValue(FocusedElementProperty);
                }
            }
        }
    }
}
