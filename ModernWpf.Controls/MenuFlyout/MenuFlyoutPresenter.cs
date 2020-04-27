using System;
using System.Diagnostics;
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

        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);

            if (_parentPopup == null)
            {
                HookupParentPopup();
            }
        }

        internal void SetOwningFlyout(MenuFlyout owningFlyout)
        {
            m_owningFlyout = new WeakReference<MenuFlyout>(owningFlyout);
        }

        internal void UpdatePopupAnimation()
        {
            if (_parentPopup != null && m_owningFlyout.TryGetTarget(out var owningFlyout))
            {
                if (owningFlyout.AreOpenCloseAnimationsEnabled)
                {
                    _parentPopup.Resources.Remove(SystemParameters.MenuPopupAnimationKey);
                }
                else
                {
                    _parentPopup.Resources[SystemParameters.MenuPopupAnimationKey] = PopupAnimation.None;
                }
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
                if (_parentPopup == null)
                {
                    HookupParentPopup();
                }
            }
        }

        private void HookupParentPopup()
        {
            Debug.Assert(_parentPopup == null, "_parentPopup should be null");

            _parentPopup = Parent as Popup;

            if (_parentPopup != null)
            {
                _parentPopup.PreviewMouseLeftButtonDown += HandlePopupMouseButtonEvent;
                _parentPopup.PreviewMouseRightButtonDown += HandlePopupMouseButtonEvent;
                _parentPopup.PreviewMouseLeftButtonUp += HandlePopupMouseButtonEvent;
                _parentPopup.PreviewMouseRightButtonUp += HandlePopupMouseButtonEvent;

                UpdatePopupAnimation();
            }
        }

        private void HandlePopupMouseButtonEvent(object sender, MouseButtonEventArgs e)
        {
            if (!_parentPopup.IsOpen)
            {
                e.Handled = true;
            }
        }

        private Popup _parentPopup;
        private WeakReference<MenuFlyout> m_owningFlyout;
    }
}