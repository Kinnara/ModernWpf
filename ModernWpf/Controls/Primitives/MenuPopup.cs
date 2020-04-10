using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace ModernWpf.Controls.Primitives
{
    public class MenuPopup : Popup
    {
        #region IsSuspendingAnimation

        private static readonly DependencyPropertyKey IsSuspendingAnimationPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(IsSuspendingAnimation),
                typeof(bool),
                typeof(MenuPopup),
                null);

        public static readonly DependencyProperty IsSuspendingAnimationProperty =
            IsSuspendingAnimationPropertyKey.DependencyProperty;

        public bool IsSuspendingAnimation
        {
            get => (bool)GetValue(IsSuspendingAnimationProperty);
            private set => SetValue(IsSuspendingAnimationPropertyKey, value);
        }

        #endregion

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            ClearValue(IsSuspendingAnimationPropertyKey);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            ClearValue(IsSuspendingAnimationPropertyKey);
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == IsOpenProperty)
            {
                OnIsOpenChanged(e);
            }

            base.OnPropertyChanged(e);
        }

        private void OnIsOpenChanged(DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                var window = Window.GetWindow(this);
                if (window != null)
                {
                    var focusedElement = FocusManager.GetFocusedElement(window);
                    if (focusedElement is TextBoxBase || focusedElement is PasswordBox)
                    {
                        IsSuspendingAnimation = true;
                    }
                }
            }
        }
    }
}
