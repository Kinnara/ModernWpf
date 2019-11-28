using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    public class MenuFlyoutPresenter : ContextMenu
    {
        static MenuFlyoutPresenter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MenuFlyoutPresenter), new FrameworkPropertyMetadata(typeof(MenuFlyoutPresenter)));

            IsOpenProperty.OverrideMetadata(typeof(MenuFlyoutPresenter), new FrameworkPropertyMetadata(OnIsOpenChanged));
        }

        public MenuFlyoutPresenter()
        {
        }

        #region CornerRadius

        public static readonly DependencyProperty CornerRadiusProperty =
            ControlHelper.CornerRadiusProperty.AddOwner(typeof(MenuFlyoutPresenter));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        #endregion

        #region IsDefaultShadowEnabled

        public static readonly DependencyProperty IsDefaultShadowEnabledProperty =
            DependencyProperty.Register(
                nameof(IsDefaultShadowEnabled),
                typeof(bool),
                typeof(MenuFlyoutPresenter),
                new PropertyMetadata(true));

        public bool IsDefaultShadowEnabled
        {
            get => (bool)GetValue(IsDefaultShadowEnabledProperty);
            set => SetValue(IsDefaultShadowEnabledProperty, value);
        }

        #endregion

        internal event EventHandler<DependencyPropertyChangedEventArgs> IsOpenChanged;

        internal void UpdatePopupAnimation(bool areOpenCloseAnimationsEnabled)
        {
            if (areOpenCloseAnimationsEnabled)
            {
                Resources.Remove(SystemParameters.MenuPopupAnimationKey);
            }
            else
            {
                Resources[SystemParameters.MenuPopupAnimationKey] = PopupAnimation.None;
            }
        }

        private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((MenuFlyoutPresenter)d).OnIsOpenChanged(e);
        }

        private void OnIsOpenChanged(DependencyPropertyChangedEventArgs e)
        {
            IsOpenChanged?.Invoke(this, e);

            if ((bool)e.NewValue)
            {
                if (m_parentPopup == null)
                {
                    m_parentPopup = Parent as Popup;
                    if (m_parentPopup != null)
                    {
                        m_parentPopup.PreviewMouseLeftButtonDown += HandlePopupMouseButtonEvent;
                        m_parentPopup.PreviewMouseRightButtonDown += HandlePopupMouseButtonEvent;
                        m_parentPopup.PreviewMouseLeftButtonUp += HandlePopupMouseButtonEvent;
                        m_parentPopup.PreviewMouseRightButtonUp += HandlePopupMouseButtonEvent;
                    }
                }
            }
        }

        private void HandlePopupMouseButtonEvent(object sender, MouseButtonEventArgs e)
        {
            if (!m_parentPopup.IsOpen)
            {
                e.Handled = true;
            }
        }

        private Popup m_parentPopup;
    }
}