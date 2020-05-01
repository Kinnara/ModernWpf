using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

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
                scrollBar.IsVisibleChanged += OnIsVisibleChanged;
                scrollBar.MouseEnter += OnIsMouseOverChanged;
                scrollBar.MouseLeave += OnIsMouseOverChanged;
                scrollBar.IsEnabledChanged += OnIsEnabledChanged;

                if (scrollBar.IsLoaded)
                {
                    UpdateVisualState(scrollBar);
                }
            }
            else
            {
                scrollBar.IsVisibleChanged -= OnIsVisibleChanged;
                scrollBar.MouseEnter -= OnIsMouseOverChanged;
                scrollBar.MouseLeave -= OnIsMouseOverChanged;
                scrollBar.IsEnabledChanged -= OnIsEnabledChanged;
            }
        }

        #endregion

        #region IndicatorMode

        public static readonly DependencyProperty IndicatorModeProperty =
            DependencyProperty.RegisterAttached(
                "IndicatorMode",
                typeof(ScrollingIndicatorMode),
                typeof(ScrollBarHelper),
                new PropertyMetadata(ScrollingIndicatorMode.MouseIndicator, OnIndicatorModeChanged));

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

        #region CollapsedThumbBackgroundColor

        public static readonly DependencyProperty CollapsedThumbBackgroundColorProperty =
            DependencyProperty.RegisterAttached(
                "CollapsedThumbBackgroundColor",
                typeof(Color?),
                typeof(ScrollBarHelper),
                new PropertyMetadata((Color?)null));

        public static Color? GetCollapsedThumbBackgroundColor(ScrollBar scrollBar)
        {
            return (Color?)scrollBar.GetValue(CollapsedThumbBackgroundColorProperty);
        }

        public static void SetCollapsedThumbBackgroundColor(ScrollBar scrollBar, Color? value)
        {
            scrollBar.SetValue(CollapsedThumbBackgroundColorProperty, value);
        }

        #endregion

        #region ExpandedThumbBackgroundColor

        public static readonly DependencyProperty ExpandedThumbBackgroundColorProperty =
            DependencyProperty.RegisterAttached(
                "ExpandedThumbBackgroundColor",
                typeof(Color?),
                typeof(ScrollBarHelper),
                new PropertyMetadata((Color?)null));

        public static Color? GetExpandedThumbBackgroundColor(ScrollBar scrollBar)
        {
            return (Color?)scrollBar.GetValue(ExpandedThumbBackgroundColorProperty);
        }

        public static void SetExpandedThumbBackgroundColor(ScrollBar scrollBar, Color? value)
        {
            scrollBar.SetValue(ExpandedThumbBackgroundColorProperty, value);
        }

        #endregion

        private static void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                var scrollBar = (ScrollBar)sender;
                scrollBar.ApplyTemplate();
                UpdateVisualState(scrollBar, false);
            }
        }

        private static void OnIsMouseOverChanged(object sender, MouseEventArgs e)
        {
            var scrollBar = (ScrollBar)sender;
            if (scrollBar.IsEnabled)
            {
                UpdateVisualState(scrollBar);
            }
        }

        private static void OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var scrollBar = (ScrollBar)sender;
            UpdateVisualState(scrollBar);
        }

        private static void UpdateVisualState(ScrollBar scrollBar, bool useTransitions = true)
        {
            string stateName;

            if (scrollBar.IsEnabled)
            {
                bool autoHide = GetIndicatorMode(scrollBar) != ScrollingIndicatorMode.MouseIndicator;
                if (autoHide)
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

            VisualStateManager.GoToState(scrollBar, stateName, useTransitions);
        }
    }
}
