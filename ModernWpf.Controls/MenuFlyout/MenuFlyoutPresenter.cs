using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    public partial class MenuFlyoutPresenter : ContextMenu
    {
        static MenuFlyoutPresenter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MenuFlyoutPresenter), new FrameworkPropertyMetadata(typeof(MenuFlyoutPresenter)));

            IsOpenProperty.OverrideMetadata(typeof(MenuFlyoutPresenter), new FrameworkPropertyMetadata(OnIsOpenChanged));
        }

        public MenuFlyoutPresenter()
        {
        }

        internal event EventHandler<DependencyPropertyChangedEventArgs> IsOpenChanged;
        internal event EventHandler<CancelEventArgs> Closing;

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

        internal void SetAbsolutePlacementPoint(Point? point)
        {
            m_absolutePlacementPoint = point;
            ApplyAbsolutePlacementPoint();
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
            if (!(bool)e.NewValue)
            {
                var args = new CancelEventArgs();
                Closing?.Invoke(this, args);

                if (args.Cancel)
                {
                    SetCurrentValue(IsOpenProperty, true);
                    return;
                }
            }

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
                ApplyAbsolutePlacementPoint();
            }
        }

        private void ApplyAbsolutePlacementPoint()
        {
            if (_parentPopup == null)
            {
                return;
            }

            if (m_absolutePlacementPoint.HasValue)
            {
                _parentPopup.Placement = PlacementMode.AbsolutePoint;
                _parentPopup.HorizontalOffset = m_absolutePlacementPoint.Value.X;
                _parentPopup.VerticalOffset = m_absolutePlacementPoint.Value.Y;
                _parentPopup.ClearValue(Popup.PlacementTargetProperty);
                _parentPopup.ClearValue(Popup.PlacementRectangleProperty);
                MovePopupWindowToAbsolutePlacementPoint();
                Dispatcher.BeginInvoke(new Action(MovePopupWindowToAbsolutePlacementPoint), DispatcherPriority.Loaded);
                Dispatcher.BeginInvoke(new Action(MovePopupWindowToAbsolutePlacementPoint), DispatcherPriority.ApplicationIdle);
            }
            else
            {
                _parentPopup.ClearValue(Popup.HorizontalOffsetProperty);
                _parentPopup.ClearValue(Popup.VerticalOffsetProperty);
            }
        }

        private void MovePopupWindowToAbsolutePlacementPoint()
        {
            if (!m_absolutePlacementPoint.HasValue)
            {
                return;
            }

            if (PresentationSource.FromVisual(this) is HwndSource source && source.Handle != IntPtr.Zero)
            {
                var point = m_absolutePlacementPoint.Value;
                SetWindowPos(
                    source.Handle,
                    IntPtr.Zero,
                    (int)Math.Round(point.X),
                    (int)Math.Round(point.Y),
                    0,
                    0,
                    SWP_NOSIZE | SWP_NOZORDER | SWP_NOACTIVATE | SWP_SHOWWINDOW);
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
        private Point? m_absolutePlacementPoint;

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr insertAfter, int x, int y, int cx, int cy, uint flags);

        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOZORDER = 0x0004;
        private const uint SWP_NOACTIVATE = 0x0010;
        private const uint SWP_SHOWWINDOW = 0x0040;
    }
}
