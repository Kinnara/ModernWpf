using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
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
                new PropertyMetadata(default(bool), OnIsEnabledChanged));

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
                scrollBar.Loaded += OnLoaded;
                scrollBar.MouseEnter += OnMouseEnter;
                scrollBar.MouseLeave += OnMouseLeave;
                scrollBar.IsEnabledChanged += OnIsEnabledChanged;

                if (scrollBar.IsLoaded)
                {
                    UpdateVisualState(scrollBar);
                }
            }
            else
            {
                scrollBar.Loaded -= OnLoaded;
                scrollBar.MouseEnter -= OnMouseEnter;
                scrollBar.MouseLeave -= OnMouseLeave;
                scrollBar.IsEnabledChanged -= OnIsEnabledChanged;
            }
        }

        #endregion

        #region CollapsedThumbBackgroundColor

        public static readonly DependencyProperty CollapsedThumbBackgroundColorProperty =
            DependencyProperty.RegisterAttached(
                "CollapsedThumbBackgroundColor",
                typeof(Color?),
                typeof(ScrollBarHelper),
                new PropertyMetadata(default(Color?)));

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
                new PropertyMetadata(default(Color?)));

        public static Color? GetExpandedThumbBackgroundColor(ScrollBar scrollBar)
        {
            return (Color?)scrollBar.GetValue(ExpandedThumbBackgroundColorProperty);
        }

        public static void SetExpandedThumbBackgroundColor(ScrollBar scrollBar, Color? value)
        {
            scrollBar.SetValue(ExpandedThumbBackgroundColorProperty, value);
        }

        #endregion

        private static void OnLoaded(object sender, RoutedEventArgs e)
        {
            var scrollBar = (ScrollBar)sender;
            UpdateVisualState(scrollBar);
        }

        private static void OnMouseEnter(object sender, MouseEventArgs e)
        {
            var scrollBar = (ScrollBar)sender;
            Debug.Assert(scrollBar.IsMouseOver);
            if (scrollBar.IsEnabled)
            {
                UpdateVisualState(scrollBar);
            }
        }

        private static void OnMouseLeave(object sender, MouseEventArgs e)
        {
            var scrollBar = (ScrollBar)sender;
            Debug.Assert(!scrollBar.IsMouseOver);
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

        private static void UpdateVisualState(ScrollBar scrollBar)
        {
            string stateName;
            bool useTransitions;

            if (scrollBar.IsEnabled)
            {
                bool autoHide = scrollBar.TemplatedParent is ScrollViewer sv && ScrollViewerHelper.GetAutoHideScrollBars(sv);
                if (autoHide)
                {
                    stateName = scrollBar.IsMouseOver ? StateExpanded : StateCollapsed;
                }
                else
                {
                    stateName = StateExpanded;
                }

                useTransitions = true;
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
