using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace ModernWpf.Controls.Primitives
{
    public class PopupEx : Popup
    {
        internal bool SuppressFadeAnimation { get; set; }

        internal event EventHandler Closing;

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);

            if (PopupAnimation == PopupAnimation.Fade && SuppressFadeAnimation)
            {
                StopAnimation();
            }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == IsOpenProperty)
            {
                if (!IsOpen)
                {
                    if (PopupAnimation == PopupAnimation.Fade && SuppressFadeAnimation)
                    {
                        StopAnimation();
                    }

                    Closing?.Invoke(this, EventArgs.Empty);
                }
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

        private DependencyObject FindPopupRoot(DependencyObject child)
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
