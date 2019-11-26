using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.Controls.Primitives
{
    public class CommandBarOverflowPresenter : ContentControl
    {
        static CommandBarOverflowPresenter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CommandBarOverflowPresenter),
                new FrameworkPropertyMetadata(typeof(CommandBarOverflowPresenter)));
        }

        public CommandBarOverflowPresenter()
        {
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        #region CornerRadius

        public static readonly DependencyProperty CornerRadiusProperty =
            ControlHelper.CornerRadiusProperty.AddOwner(typeof(CommandBarOverflowPresenter));

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
            if (TemplatedParent is CommandBarToolBar toolBar)
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
