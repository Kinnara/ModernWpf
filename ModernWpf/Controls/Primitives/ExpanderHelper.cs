using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ModernWpf.Controls.Primitives
{
    public static class ExpanderHelper
    {
        #region IsEnabled

        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsEnabled",
                typeof(bool),
                typeof(ExpanderHelper),
                new PropertyMetadata(false, OnIsEnabledChanged));

        public static bool GetIsEnabled(Expander expander)
        {
            return (bool)expander.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(Expander expander, bool value)
        {
            expander.SetValue(IsEnabledProperty, value);
        }

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ex = (Expander)d;
            if ((bool)e.NewValue)
            {
                ex.Loaded += OnLoaded;
                ex.Expanded += OnExpanded;
                ex.Collapsed += OnCollapsed;
                BindingOperations.SetBinding(ex, ExpandDirectionProperty, new Binding() { Path = new PropertyPath(Expander.ExpandDirectionProperty), Source = ex });
            }
            else
            {
                ex.Loaded -= OnLoaded;
                ex.Expanded -= OnExpanded;
                ex.Collapsed -= OnCollapsed;
                BindingOperations.ClearBinding(ex, ExpandDirectionProperty);
            }
        }

        #endregion

        #region ExpandDirection

        internal static readonly DependencyProperty ExpandDirectionProperty =
            DependencyProperty.RegisterAttached(
                "ExpandDirection",
                typeof(ExpandDirection),
                typeof(ExpanderHelper),
                new PropertyMetadata(ExpandDirection.Down, OnExpandDirectionChanged));

        private static void OnExpandDirectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UpdateVisualState((Expander)d);
        }

        #endregion

        private static void OnLoaded(object sender, RoutedEventArgs e)
        {
            var ex = (Expander)sender;
            ex.ApplyTemplate();
            UpdateVisualState(ex, false);
        }

        private static void OnExpanded(object sender, RoutedEventArgs e)
        {
            UpdateVisualState((Expander)sender);
        }

        private static void OnCollapsed(object sender, RoutedEventArgs e)
        {
            UpdateVisualState((Expander)sender);
        }

        private static void UpdateVisualState(Expander ex, bool useTransitions = true)
        {
            string visualState = null;

            switch (ex.ExpandDirection)
            {
                case ExpandDirection.Left:
                    visualState = ex.IsExpanded ? "VisibleLeft" : "CollapsedLeft";
                    break;
                case ExpandDirection.Down:
                    visualState = ex.IsExpanded ? "VisibleDown" : "CollapsedDown";
                    break;
                case ExpandDirection.Right:
                    visualState = ex.IsExpanded ? "VisibleRight" : "CollapsedRight";
                    break;
                case ExpandDirection.Up:
                    visualState = ex.IsExpanded ? "VisibleUp" : "CollapsedUp";
                    break;
            }

            if (!string.IsNullOrWhiteSpace(visualState))
            {
                VisualStateManager.GoToState(ex, visualState, useTransitions);
            }
        }
    }
}
