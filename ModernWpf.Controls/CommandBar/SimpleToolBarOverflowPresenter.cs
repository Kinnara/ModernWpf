using System.Windows;
using System.Windows.Controls;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    public class SimpleToolBarOverflowPresenter : ContentControl
    {
        static SimpleToolBarOverflowPresenter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SimpleToolBarOverflowPresenter),
                new FrameworkPropertyMetadata(typeof(SimpleToolBarOverflowPresenter)));
        }

        public SimpleToolBarOverflowPresenter()
        {
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        #region CornerRadius

        public static readonly DependencyProperty CornerRadiusProperty =
            ControlHelper.CornerRadiusProperty.AddOwner(typeof(SimpleToolBarOverflowPresenter));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        #endregion

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            UpdateVisualState(false);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (IsVisible)
            {
                UpdateVisualState(true);
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            UpdateVisualState(false);
        }

        private void UpdateVisualState(bool useTransitions)
        {
            string stateName;

            //if (IsLoaded && IsVisible)
            //{
            //    stateName = IsPopupOpenDown() ? "FullWidthOpenDown" : "FullWidthOpenUp";
            //}
            //else
            {
                stateName = "DisplayModeDefault";
            }

            VisualStateManager.GoToState(this, stateName, useTransitions);
        }

        private bool IsPopupOpenDown()
        {
            if (TemplatedParent is SimpleToolBar toolBar)
            {
                var popupPoint = PointToScreen(new Point());
                var toolBarPoint = toolBar.PointToScreen(new Point());
                var verticalOffset = popupPoint.Y - toolBarPoint.Y;
                return verticalOffset > 0;
            }
            return true;
        }
    }
}
