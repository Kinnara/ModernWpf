using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    public class ListViewBaseItem : ListBoxItem
    {
        protected ListViewBaseItem()
        {
        }

        #region UseSystemFocusVisuals

        public static readonly DependencyProperty UseSystemFocusVisualsProperty =
            FocusVisualHelper.UseSystemFocusVisualsProperty.AddOwner(typeof(ListViewBaseItem));

        public bool UseSystemFocusVisuals
        {
            get => (bool)GetValue(UseSystemFocusVisualsProperty);
            set => SetValue(UseSystemFocusVisualsProperty, value);
        }

        #endregion

        #region FocusVisualMargin

        public static readonly DependencyProperty FocusVisualMarginProperty =
            FocusVisualHelper.FocusVisualMarginProperty.AddOwner(typeof(ListViewBaseItem));

        public Thickness FocusVisualMargin
        {
            get => (Thickness)GetValue(FocusVisualMarginProperty);
            set => SetValue(FocusVisualMarginProperty, value);
        }

        #endregion

        #region FocusVisualPrimaryBrush

        public static readonly DependencyProperty FocusVisualPrimaryBrushProperty =
            FocusVisualHelper.FocusVisualPrimaryBrushProperty.AddOwner(typeof(ListViewBaseItem));

        public Thickness FocusVisualPrimaryBrush
        {
            get => (Thickness)GetValue(FocusVisualPrimaryBrushProperty);
            set => SetValue(FocusVisualPrimaryBrushProperty, value);
        }

        #endregion

        #region FocusVisualPrimaryThickness

        public static readonly DependencyProperty FocusVisualPrimaryThicknessProperty =
            FocusVisualHelper.FocusVisualPrimaryThicknessProperty.AddOwner(typeof(ListViewBaseItem));

        public Thickness FocusVisualPrimaryThickness
        {
            get => (Thickness)GetValue(FocusVisualPrimaryThicknessProperty);
            set => SetValue(FocusVisualPrimaryThicknessProperty, value);
        }

        #endregion

        #region FocusVisualSecondaryBrush

        public static readonly DependencyProperty FocusVisualSecondaryBrushProperty =
            FocusVisualHelper.FocusVisualSecondaryBrushProperty.AddOwner(typeof(ListViewBaseItem));

        public Thickness FocusVisualSecondaryBrush
        {
            get => (Thickness)GetValue(FocusVisualSecondaryBrushProperty);
            set => SetValue(FocusVisualSecondaryBrushProperty, value);
        }

        #endregion

        #region FocusVisualSecondaryThickness

        public static readonly DependencyProperty FocusVisualSecondaryThicknessProperty =
            FocusVisualHelper.FocusVisualSecondaryThicknessProperty.AddOwner(typeof(ListViewBaseItem));

        public Thickness FocusVisualSecondaryThickness
        {
            get => (Thickness)GetValue(FocusVisualSecondaryThicknessProperty);
            set => SetValue(FocusVisualSecondaryThicknessProperty, value);
        }

        #endregion

        #region CornerRadius

        public static readonly DependencyProperty CornerRadiusProperty =
            ControlHelper.CornerRadiusProperty.AddOwner(typeof(ListViewBaseItem));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        #endregion

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            UpdateMultiSelectStates(ParentListViewBase, false);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (!e.Handled)
            {
                m_isPressed = true;
            }
            base.OnMouseLeftButtonDown(e);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (!e.Handled)
            {
                HandleMouseUp(e);
                m_isPressed = false;
            }
            base.OnMouseLeftButtonUp(e);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            if (!e.Handled)
            {
                m_isPressed = false;
            }
            base.OnMouseLeave(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.Key == Key.Enter)
            {
                OnClick();
                e.Handled = true;
            }
        }

        internal void SubscribeToMultiSelectEnabledChanged(ListViewBase parent)
        {
            parent.MultiSelectEnabledChanged += OnMultiSelectEnabledChanged;
            UpdateMultiSelectStates(parent);
        }

        internal void UnsubscribeFromMultiSelectEnabledChanged(ListViewBase parent)
        {
            parent.MultiSelectEnabledChanged -= OnMultiSelectEnabledChanged;
            UpdateMultiSelectStates(parent);
        }

        private void OnMultiSelectEnabledChanged(object sender, EventArgs e)
        {
            UpdateMultiSelectStates((ListViewBase)sender);
        }

        private void UpdateMultiSelectStates(ListViewBase parent, bool useTransitions = true)
        {
            if (parent != null)
            {
                bool enabled = parent.MultiSelectEnabled && parent.IsMultiSelectCheckBoxEnabled;
                VisualStateManager.GoToState(this, enabled ? "MultiSelectEnabled" : "MultiSelectDisabled", useTransitions);
            }
        }

        private void HandleMouseUp(MouseButtonEventArgs e)
        {
            if (m_isPressed)
            {
                Rect r = new Rect(new Point(), RenderSize);

                if (r.Contains(e.GetPosition(this)))
                {
                    OnClick();
                }
            }
        }

        private void OnClick()
        {
            ParentListViewBase?.NotifyListItemClicked(this);
        }

        private ListViewBase ParentListViewBase => ItemsControl.ItemsControlFromItemContainer(this) as ListViewBase;

        private bool m_isPressed;
    }
}
