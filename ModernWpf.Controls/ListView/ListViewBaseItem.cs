using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    public class ListViewBaseItem : ListBoxItem
    {
        protected ListViewBaseItem()
        {
            IsEnabledChanged += OnIsEnabledChanged;
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

        public Brush FocusVisualPrimaryBrush
        {
            get => (Brush)GetValue(FocusVisualPrimaryBrushProperty);
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

        public Brush FocusVisualSecondaryBrush
        {
            get => (Brush)GetValue(FocusVisualSecondaryBrushProperty);
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

            UpdateVisualStates(false);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (!e.Handled)
            {
                m_isPressed = true;
                UpdateVisualStates();
            }
            base.OnMouseLeftButtonDown(e);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (!e.Handled)
            {
                HandleMouseUp(e);
                m_isPressed = false;
                UpdateVisualStates();
            }
            base.OnMouseLeftButtonUp(e);
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            UpdateVisualStates();
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            if (!e.Handled)
            {
                m_isPressed = false;
                UpdateVisualStates();
            }
            base.OnMouseLeave(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (IsPrimaryInteractionKey(e))
            {
                m_isKeyboardPressed = true;
                UpdateVisualStates();
                OnClick();
                e.Handled = true;
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            if (m_isKeyboardPressed && (e.Key == Key.Space || e.Key == Key.Enter))
            {
                m_isKeyboardPressed = false;
                UpdateVisualStates();
            }
        }

        protected override void OnSelected(RoutedEventArgs e)
        {
            base.OnSelected(e);
            UpdateVisualStates();
        }

        protected override void OnUnselected(RoutedEventArgs e)
        {
            base.OnUnselected(e);
            UpdateVisualStates();
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == IsEnabledProperty)
            {
                UpdateVisualStates();
            }
        }

        private void OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UpdateVisualStates();
        }

        internal void SubscribeToMultiSelectEnabledChanged(ListViewBase parent)
        {
            parent.MultiSelectEnabledChanged += OnMultiSelectEnabledChanged;
            UpdateVisualStates();
        }

        internal void UnsubscribeFromMultiSelectEnabledChanged(ListViewBase parent)
        {
            parent.MultiSelectEnabledChanged -= OnMultiSelectEnabledChanged;
            UpdateVisualStates();
        }

        private void OnMultiSelectEnabledChanged(object sender, EventArgs e)
        {
            UpdateVisualStates();
        }

        private void UpdateVisualStates(bool useTransitions = true)
        {
            VisualStateManager.GoToState(this, GetCommonState(), useTransitions);
            VisualStateManager.GoToState(this, GetMultiSelectState(), useTransitions);
        }

        private string GetCommonState()
        {
            if (!IsEnabled)
            {
                return IsSelected ? "SelectedDisabled" : "Disabled";
            }

            if (IsPressed)
            {
                return IsSelected ? "PressedSelected" : "Pressed";
            }

            if (IsMouseOver)
            {
                return IsSelected ? "PointerOverSelected" : "PointerOver";
            }

            return IsSelected ? "Selected" : "Normal";
        }

        private string GetMultiSelectState()
        {
            var parent = ParentListViewBase;
            if (parent == null || !parent.MultiSelectEnabled || !parent.IsMultiSelectCheckBoxEnabled)
            {
                return "NoMultiSelect";
            }

            return this is GridViewItem ? "GridMultiSelect" : "ListMultiSelect";
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

        private static bool IsPrimaryInteractionKey(KeyEventArgs e)
        {
            if (e.Key == Key.Space && (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
            {
                return false;
            }

            return e.Key == Key.Space || e.Key == Key.Enter;
        }

        private bool IsPressed => m_isPressed || m_isKeyboardPressed;

        private ListViewBase ParentListViewBase => ItemsControl.ItemsControlFromItemContainer(this) as ListViewBase;

        private bool m_isPressed;
        private bool m_isKeyboardPressed;
    }
}
