using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ModernWpf.Automation.Peers;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    [TemplatePart(Name = IconVisualName, Type = typeof(ContentPresenterEx))]
    [TemplatePart(Name = TextVisualName, Type = typeof(TextBlock))]
    public partial class SelectorBarItem : Control
    {
        private const string IconVisualName = "PART_IconVisual";
        private const string TextVisualName = "PART_TextVisual";

        static SelectorBarItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SelectorBarItem), new FrameworkPropertyMetadata(typeof(SelectorBarItem)));
            IsEnabledProperty.OverrideMetadata(typeof(SelectorBarItem), new FrameworkPropertyMetadata(OnIsEnabledChanged));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _iconVisual = GetTemplateChild(IconVisualName) as ContentPresenterEx;
            _textVisual = GetTemplateChild(TextVisualName) as TextBlock;

            UpdatePartsVisibility(true, true);
            UpdateVisualState(false);
        }

        internal SelectorBar Owner { get; set; }

        internal void Select()
        {
            Owner?.SelectItem(this);
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new SelectorBarItemAutomationPeer(this);
        }

        protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnGotKeyboardFocus(e);
            UpdateVisualState();
        }

        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnLostKeyboardFocus(e);
            UpdateVisualState();
        }

        protected override void OnLostMouseCapture(MouseEventArgs e)
        {
            base.OnLostMouseCapture(e);

            if (_isPressed)
            {
                _isPressed = false;
                UpdateVisualState();
            }
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            UpdateVisualState();
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            UpdateVisualState();
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            if (!e.Handled && IsEnabled)
            {
                Focus();
                _isPressed = true;
                CaptureMouse();
                UpdateVisualState();
                e.Handled = true;
            }
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);

            if (_isPressed)
            {
                var shouldSelect = IsMouseOver || IsMouseCaptured;
                _isPressed = false;
                if (IsMouseCaptured)
                {
                    ReleaseMouseCapture();
                }

                if (shouldSelect)
                {
                    Select();
                }

                UpdateVisualState();
                e.Handled = true;
            }
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            if (e.Handled)
            {
                return;
            }

            if (e.Key == Key.Enter || e.Key == Key.Space)
            {
                Select();
                e.Handled = true;
                return;
            }

            if ((e.Key == Key.Left || e.Key == Key.Right) && Owner?.MoveFocusFrom(this, e.Key) == true)
            {
                e.Handled = true;
            }
        }

        private static void OnIsSelectedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var item = (SelectorBarItem)d;
            item.Owner?.OnItemIsSelectedChanged(item, (bool)e.NewValue);
            item.UpdateVisualState();
        }

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var item = (SelectorBarItem)d;
            if (!item.IsEnabled)
            {
                item._isPressed = false;
                if (item.IsMouseCaptured)
                {
                    item.ReleaseMouseCapture();
                }
            }

            item.UpdateVisualState();
        }

        private static void OnVisualPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var item = (SelectorBarItem)d;
            if (e.Property == IconProperty)
            {
                item.UpdatePartsVisibility(true, false);
            }
            else if (e.Property == TextProperty)
            {
                item.UpdatePartsVisibility(false, true);
            }
        }

        private void UpdatePartsVisibility(bool isForIcon, bool isForText)
        {
            UIElement iconParent = null;
            UIElement textParent = null;
            var hasIcon = false;
            var hasText = false;

            if (_iconVisual != null)
            {
                iconParent = VisualTreeHelper.GetParent(_iconVisual) as UIElement;
                hasIcon = Icon != null;
                if (isForIcon)
                {
                    _iconVisual.Visibility = hasIcon ? Visibility.Visible : Visibility.Collapsed;
                }
            }

            if (_textVisual != null)
            {
                textParent = VisualTreeHelper.GetParent(_textVisual) as UIElement;
                hasText = !string.IsNullOrEmpty(Text);
                if (isForText)
                {
                    _textVisual.Visibility = hasText ? Visibility.Visible : Visibility.Collapsed;
                }
            }

            if (iconParent != null && ReferenceEquals(iconParent, textParent))
            {
                iconParent.Visibility = hasIcon || hasText ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void UpdateVisualState(bool useTransitions = true)
        {
            var interactionState = _isPressed ? "Pressed" : IsMouseOver ? "PointerOver" : "Normal";
            var selectionState = IsSelected ? "Selected" : "Unselected";
            VisualStateManager.GoToState(this, selectionState + interactionState, useTransitions);
            VisualStateManager.GoToState(this, IsEnabled ? "Enabled" : "Disabled", useTransitions);
        }

        private ContentPresenterEx _iconVisual;
        private TextBlock _textVisual;
        private bool _isPressed;
    }
}
