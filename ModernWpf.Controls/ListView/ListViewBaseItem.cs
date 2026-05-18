using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    public partial class ListViewBaseItem : ListBoxItem
    {
        protected ListViewBaseItem()
        {
            IsEnabledChanged += OnIsEnabledChanged;
        }

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
