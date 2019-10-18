using ModernWpf.Automation.Peers;
using System.ComponentModel;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;

namespace ModernWpf.Controls.Primitives
{
    public class RadioButtonsListViewItem : ListBoxItem
    {
        static RadioButtonsListViewItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RadioButtonsListViewItem),
                new FrameworkPropertyMetadata(typeof(RadioButtonsListViewItem)));
        }

        #region IsPressed

        private static readonly DependencyPropertyKey IsPressedPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(IsPressed),
                typeof(bool),
                typeof(RadioButtonsListViewItem),
                new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty IsPressedProperty =
            IsPressedPropertyKey.DependencyProperty;

        [Browsable(false), Category("Appearance")]
        public bool IsPressed
        {
            get => (bool)GetValue(IsPressedProperty);
            protected set => SetValue(IsPressedPropertyKey, value);
        }

        private void UpdateIsPressed()
        {
            Rect itemBounds = new Rect(new Point(), RenderSize);

            if ((Mouse.LeftButton == MouseButtonState.Pressed) &&
                IsMouseOver &&
                itemBounds.Contains(Mouse.GetPosition(this)))
            {
                IsPressed = true;
            }
            else
            {
                ClearValue(IsPressedPropertyKey);
            }
        }

        #endregion

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = true;
                UpdateIsPressed();
            }
            base.OnMouseLeftButtonDown(e);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = true;
                HandleMouseButtonUp(MouseButton.Left);
                UpdateIsPressed();
            }
            base.OnMouseLeftButtonUp(e);
        }

        private void HandleMouseButtonUp(MouseButton mouseButton)
        {
            if (SelectorHelper.UiGetIsSelectable(this) && Focus())
            {
                if (!IsSelected)
                {
                    SetCurrentValue(IsSelectedProperty, true);
                }
            }
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            UpdateIsPressed();
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            UpdateIsPressed();
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new RadioButtonsListViewItemAutomationPeer(this);
        }
    }
}
