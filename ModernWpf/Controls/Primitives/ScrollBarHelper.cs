using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace ModernWpf.Controls.Primitives
{
    public static class ScrollBarHelper
    {
        private const string StateExpanded = "Expanded";
        private const string StateCollapsed = "Collapsed";

        #region IsEnabled

        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsEnabled",
                typeof(bool),
                typeof(ScrollBarHelper),
                new PropertyMetadata(false, OnIsEnabledChanged));

        public static bool GetIsEnabled(ScrollBar scrollBar)
        {
            return (bool)scrollBar.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(ScrollBar scrollBar, bool value)
        {
            scrollBar.SetValue(IsEnabledProperty, value);
        }

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var scrollBar = (ScrollBar)d;
            var oldValue = (bool)e.OldValue;
            var newValue = (bool)e.NewValue;

            if (newValue)
            {
                scrollBar.IsVisibleChanged += OnScrollBarIsVisibleChanged;
                scrollBar.MouseEnter += OnScrollBarIsMouseOverChanged;
                scrollBar.MouseLeave += OnScrollBarIsMouseOverChanged;
                scrollBar.IsEnabledChanged += OnScrollBarIsEnabledChanged;

                if (scrollBar.IsLoaded)
                {
                    UpdateVisualState(scrollBar);
                }
            }
            else
            {
                scrollBar.IsVisibleChanged -= OnScrollBarIsVisibleChanged;
                scrollBar.MouseEnter -= OnScrollBarIsMouseOverChanged;
                scrollBar.MouseLeave -= OnScrollBarIsMouseOverChanged;
                scrollBar.IsEnabledChanged -= OnScrollBarIsEnabledChanged;
            }
        }

        #endregion

        #region IndicatorMode

        public static readonly DependencyProperty IndicatorModeProperty =
            DependencyProperty.RegisterAttached(
                "IndicatorMode",
                typeof(ScrollingIndicatorMode),
                typeof(ScrollBarHelper),
                new PropertyMetadata(ScrollingIndicatorMode.None, OnIndicatorModeChanged));

        public static ScrollingIndicatorMode GetIndicatorMode(ScrollBar scrollBar)
        {
            return (ScrollingIndicatorMode)scrollBar.GetValue(IndicatorModeProperty);
        }

        public static void SetIndicatorMode(ScrollBar scrollBar, ScrollingIndicatorMode value)
        {
            scrollBar.SetValue(IndicatorModeProperty, value);
        }

        private static void OnIndicatorModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UpdateVisualState((ScrollBar)d);
        }

        #endregion

        #region AutoHide

        public static readonly DependencyProperty AutoHideProperty =
            DependencyProperty.RegisterAttached(
                "AutoHide",
                typeof(bool),
                typeof(ScrollBarHelper),
                new PropertyMetadata(false, OnAutoHideChanged));

        public static bool GetAutoHide(ScrollBar scrollBar)
        {
            return (bool)scrollBar.GetValue(AutoHideProperty);
        }

        public static void SetAutoHide(ScrollBar scrollBar, bool value)
        {
            scrollBar.SetValue(AutoHideProperty, value);
        }

        private static void OnAutoHideChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var scrollBar = (ScrollBar)d;
            UpdateVisualState(scrollBar);
        }

        #endregion

        private static void OnScrollBarIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                var scrollBar = (ScrollBar)sender;
                scrollBar.ApplyTemplate();
                UpdateVisualState(scrollBar, false);
            }
        }

        private static void OnScrollBarIsMouseOverChanged(object sender, MouseEventArgs e)
        {
            var scrollBar = (ScrollBar)sender;
            if (scrollBar.IsEnabled)
            {
                UpdateVisualState(scrollBar);
            }
        }

        private static void OnScrollBarIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var scrollBar = (ScrollBar)sender;
            UpdateVisualState(scrollBar);
        }

        private static void UpdateVisualState(ScrollBar scrollBar, bool useTransitions = true)
        {
            string stateName;

            if (scrollBar.IsEnabled)
            {
                if (GetAutoHide(scrollBar))
                {
                    stateName = scrollBar.IsMouseOver ? StateExpanded : StateCollapsed;
                }
                else
                {
                    stateName = StateExpanded;
                }
            }
            else
            {
                stateName = StateCollapsed;
                useTransitions = false;
            }

            if (!Helper.IsAnimationsEnabled)
            {
                stateName += "WithoutAnimation";
            }

            VisualStateManager.GoToState(scrollBar, stateName, useTransitions);
        }
    }
}
