using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace ModernWpf.Controls.Primitives
{
    public class PopupEx : Popup
    {
        static PopupEx()
        {
            IsOpenProperty.OverrideMetadata(typeof(PopupEx), new FrameworkPropertyMetadata(OnIsOpenPropertyChanged));
        }

        internal bool SuppressFadeAnimation { get; set; }

        internal event EventHandler<CancelEventArgs> Closing;

        internal event EventHandler IsOpenChanged;

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);

            if (PopupAnimation == PopupAnimation.Fade && SuppressFadeAnimation)
            {
                StopAnimation();
            }
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);

            if (!IsOpen)
            {
                e.Handled = true;
            }
        }

        protected override void OnPreviewMouseRightButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseRightButtonDown(e);

            if (!IsOpen)
            {
                e.Handled = true;
            }
        }

        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonUp(e);

            if (!IsOpen)
            {
                e.Handled = true;
            }
        }

        protected override void OnPreviewMouseRightButtonUp(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseRightButtonUp(e);

            if (!IsOpen)
            {
                e.Handled = true;
            }
        }

        private static void OnIsOpenPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((PopupEx)d).OnIsOpenChanged();
        }

        private void OnIsOpenChanged()
        {
            if (!IsOpen)
            {
                if (PopupAnimation == PopupAnimation.Fade && SuppressFadeAnimation)
                {
                    StopAnimation();
                }

                var args = new CancelEventArgs();
                Closing?.Invoke(this, args);

                if (args.Cancel)
                {
                    SetCurrentValue(IsOpenProperty, true);
                    return;
                }
            }

            IsOpenChanged?.Invoke(this, EventArgs.Empty);
        }

        private void StopAnimation()
        {
            if (Child is FrameworkElement child)
            {
                if (FindPopupRoot(child) is FrameworkElement popupRoot)
                {
                    popupRoot.BeginAnimation(OpacityProperty, null);
                }
            }
        }

        private static DependencyObject FindPopupRoot(DependencyObject child)
        {
            var parent = VisualTreeHelper.GetParent(child);

            while (parent == null)
            {
                return child;
            }

            return FindPopupRoot(parent);
        }
    }
}
