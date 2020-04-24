using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;

namespace ModernWpf.Controls.Primitives
{
    public abstract class FlyoutBase : DependencyObject
    {
        protected FlyoutBase()
        {
        }

        #region Placement

        public static readonly DependencyProperty PlacementProperty =
            DependencyProperty.Register(
                nameof(Placement),
                typeof(FlyoutPlacementMode),
                typeof(FlyoutBase),
                new PropertyMetadata(FlyoutPlacementMode.Top));

        public FlyoutPlacementMode Placement
        {
            get => (FlyoutPlacementMode)GetValue(PlacementProperty);
            set => SetValue(PlacementProperty, value);
        }

        #endregion

        #region AreOpenCloseAnimationsEnabled

        public static readonly DependencyProperty AreOpenCloseAnimationsEnabledProperty =
            DependencyProperty.Register(
                nameof(AreOpenCloseAnimationsEnabled),
                typeof(bool),
                typeof(FlyoutBase),
                new PropertyMetadata(true, OnAreOpenCloseAnimationsEnabledChanged));

        public bool AreOpenCloseAnimationsEnabled
        {
            get => (bool)GetValue(AreOpenCloseAnimationsEnabledProperty);
            set => SetValue(AreOpenCloseAnimationsEnabledProperty, value);
        }

        private static void OnAreOpenCloseAnimationsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((FlyoutBase)d).OnAreOpenCloseAnimationsEnabledChanged(e);
        }

        internal virtual void OnAreOpenCloseAnimationsEnabledChanged(DependencyPropertyChangedEventArgs e)
        {
            UpdatePopupAnimation();
        }

        #endregion

        #region IsOpen

        private static readonly DependencyPropertyKey IsOpenPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(IsOpen),
                typeof(bool),
                typeof(FlyoutBase),
                new PropertyMetadata(false, OnIsOpenChanged));

        public static readonly DependencyProperty IsOpenProperty =
            IsOpenPropertyKey.DependencyProperty;

        public bool IsOpen
        {
            get => (bool)GetValue(IsOpenProperty);
            internal set => SetValue(IsOpenPropertyKey, value);
        }

        private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((FlyoutBase)d).OnIsOpenChanged();
        }

        internal virtual void OnIsOpenChanged()
        {
            if (IsOpen)
            {
                if (Keyboard.FocusedElement != null)
                {
                    m_weakRefToPreviousFocus = new WeakReference<IInputElement>(Keyboard.FocusedElement);
                }

                m_presenter?.Focus();
            }
            else
            {
                if (m_weakRefToPreviousFocus != null)
                {
                    if (m_weakRefToPreviousFocus.TryGetTarget(out IInputElement previousFocus))
                    {
                        previousFocus.Focus();
                    }

                    m_weakRefToPreviousFocus = null;
                }
            }
        }

        internal virtual void UpdateIsOpen()
        {
            IsOpen = m_popup != null && m_popup.IsOpen;
        }

        #endregion

        #region ShowMode

        public static readonly DependencyProperty ShowModeProperty =
            DependencyProperty.Register(
                nameof(ShowMode),
                typeof(FlyoutShowMode),
                typeof(FlyoutBase),
                new PropertyMetadata(FlyoutShowMode.Standard));

        public FlyoutShowMode ShowMode
        {
            get => (FlyoutShowMode)GetValue(ShowModeProperty);
            set => SetValue(ShowModeProperty, value);
        }

        #endregion

        #region AttachedFlyout

        public static readonly DependencyProperty AttachedFlyoutProperty =
            DependencyProperty.RegisterAttached(
                "AttachedFlyout",
                typeof(FlyoutBase),
                typeof(FlyoutBase));

        public static FlyoutBase GetAttachedFlyout(FrameworkElement element)
        {
            return (FlyoutBase)element.GetValue(AttachedFlyoutProperty);
        }

        public static void SetAttachedFlyout(FrameworkElement element, FlyoutBase value)
        {
            element.SetValue(AttachedFlyoutProperty, value);
        }

        public static void ShowAttachedFlyout(FrameworkElement flyoutOwner)
        {
            var flyout = GetAttachedFlyout(flyoutOwner);
            if (flyout != null)
            {
                flyout.ShowAt(flyoutOwner);
            }
        }

        #endregion

        internal virtual PopupAnimation DesiredPopupAnimation => PopupAnimation.Fade;

        internal PopupEx InternalPopup => m_popup;

        internal double Offset { get; set; } = s_offset;

        public event EventHandler<object> Opening;
        public event EventHandler<object> Opened;
        public event EventHandler<object> Closed;
        internal event TypedEventHandler<FlyoutBase, FlyoutBaseClosingEventArgs> Closing;

        public void ShowAt(FrameworkElement placementTarget)
        {
            if (placementTarget is null)
            {
                throw new ArgumentNullException(nameof(placementTarget));
            }

            ShowAtCore(placementTarget, false);
        }

        public void Hide()
        {
            CancelAsyncShow();
            HideCore();
        }

        protected abstract Control CreatePresenter();

        internal void ShowAsContextFlyout(FrameworkElement placementTarget)
        {
            if (placementTarget is null)
            {
                throw new ArgumentNullException(nameof(placementTarget));
            }

            ShowAtCore(placementTarget, true);
        }

        internal virtual void ShowAtCore(FrameworkElement placementTarget, bool showAsContextFlyout)
        {
            CancelAsyncShow();

            if (m_popup != null &&
                m_popup.IsOpen &&
                m_target == placementTarget &&
                m_showingAsContextFlyout == showAsContextFlyout)
            {
                return;
            }

            if (m_closing)
            {
                m_pendingShow = () => ShowAtCore(placementTarget, showAsContextFlyout);
                return;
            }

            PreparePopup(placementTarget, showAsContextFlyout);
            Debug.Assert(m_popup.HasLocalValue(Popup.PlacementProperty));
            Debug.Assert(m_popup.HasLocalValue(Popup.PlacementTargetProperty));

            m_target = placementTarget;
            m_showingAsContextFlyout = showAsContextFlyout;
            OnOpening();
            m_popup.IsOpen = true;
        }

        internal virtual void HideCore()
        {
            if (m_popup != null && m_popup.IsOpen)
            {
                m_popup.IsOpen = false;
            }
        }

        internal virtual void OnOpening()
        {
            Opening?.Invoke(this, null);
        }

        internal virtual void OnOpened()
        {
            Opened?.Invoke(this, null);
        }

        internal virtual void OnClosed()
        {
            Closed?.Invoke(this, null);

            var pendingShow = m_pendingShow;
            CancelAsyncShow();
            if (pendingShow != null)
            {
                m_asyncShow = Dispatcher.BeginInvoke(pendingShow);
            }
        }

        internal void BindPlacement(Control presenter)
        {
            presenter.SetBinding(
                CustomPopupPlacementHelper.PlacementProperty,
                new Binding
                {
                    Path = new PropertyPath(PlacementProperty),
                    Source = this,
                    Converter = s_placementConverter
                });
        }

        private void EnsurePresenter()
        {
            if (m_presenter == null)
            {
                m_presenter = CreatePresenter();
                BindPlacement(m_presenter);
            }
        }

        private void EnsurePopup()
        {
            if (m_popup == null)
            {
                EnsurePresenter();

                m_popup = new PopupEx
                {
                    Child = m_presenter,
                    StaysOpen = false,
                    AllowsTransparency = true,
                    CustomPopupPlacementCallback = PositionPopup
                };
                m_popup.Opened += OnPopupOpened;
                m_popup.Closing += OnPopupClosing;
                m_popup.Closed += OnPopupClosed;
                m_popup.IsOpenChanged += OnPopupIsOpenChanged;
            }
        }

        private void PreparePopup(FrameworkElement placementTarget, bool showAsContextFlyout)
        {
            EnsurePopup();

            if (m_popup.IsOpen)
            {
                m_popup.IsOpen = false;
            }

            UpdatePopupAnimation();

            if (showAsContextFlyout)
            {
                m_presenter.ClearValue(FrameworkElement.WidthProperty);
                m_presenter.ClearValue(FrameworkElement.HeightProperty);
                m_popup.Placement = PlacementMode.MousePoint;
                m_popup.PlacementTarget = placementTarget;
                m_popup.ClearValue(Popup.PlacementRectangleProperty);
            }
            else if (Placement == FlyoutPlacementMode.Full &&
                Window.GetWindow(placementTarget) is Window window)
            {
                var adornerDecorator = window.FindDescendant<AdornerDecorator>();
                if (adornerDecorator != null)
                {
                    placementTarget = adornerDecorator;

                    m_presenter.SetBinding(
                        FrameworkElement.WidthProperty,
                        new Binding
                        {
                            Path = new PropertyPath(FrameworkElement.ActualWidthProperty),
                            Source = adornerDecorator
                        });

                    m_presenter.SetBinding(
                        FrameworkElement.HeightProperty,
                        new Binding
                        {
                            Path = new PropertyPath(FrameworkElement.ActualHeightProperty),
                            Source = adornerDecorator
                        });
                }
                else
                {
                    placementTarget = window;

                    m_presenter.SetBinding(
                        FrameworkElement.WidthProperty,
                        new MultiBinding
                        {
                            Converter = s_fullPlacementWidthConverter,
                            Bindings =
                            {
                                new Binding { Path = new PropertyPath(FrameworkElement.ActualWidthProperty), Source = window },
                                new Binding { Path = new PropertyPath(Control.BorderThicknessProperty), Source = window },
                            }
                        });

                    m_presenter.SetBinding(
                        FrameworkElement.HeightProperty,
                        new MultiBinding
                        {
                            Converter = s_fullPlacementHeightConverter,
                            Bindings =
                            {
                                new Binding { Path = new PropertyPath(FrameworkElement.ActualHeightProperty), Source = window },
                                new Binding { Path = new PropertyPath(Control.BorderThicknessProperty), Source = window },
                            }
                        });
                }

                m_popup.Placement = PlacementMode.Center;
                m_popup.PlacementTarget = placementTarget;
                m_popup.ClearValue(Popup.PlacementRectangleProperty);
            }
            else
            {
                m_presenter.ClearValue(FrameworkElement.WidthProperty);
                m_presenter.ClearValue(FrameworkElement.HeightProperty);
                m_popup.Placement = PlacementMode.Custom;
                m_popup.PlacementTarget = placementTarget;
                m_popup.PlacementRectangle = GetPlacementRectangle(placementTarget);
            }
        }

        private void UpdatePopupAnimation()
        {
            if (m_popup != null)
            {
                m_popup.PopupAnimation = AreOpenCloseAnimationsEnabled && SharedHelpers.IsAnimationsEnabled ?
                    DesiredPopupAnimation : PopupAnimation.None;
            }
        }

        internal Rect GetPlacementRectangle(UIElement target)
        {
            Rect value = Rect.Empty;

            if (target != null)
            {
                Size targetSize = target.RenderSize;

                switch (Placement)
                {
                    case FlyoutPlacementMode.Top:
                    case FlyoutPlacementMode.Bottom:
                    case FlyoutPlacementMode.TopEdgeAlignedLeft:
                    case FlyoutPlacementMode.TopEdgeAlignedRight:
                    case FlyoutPlacementMode.BottomEdgeAlignedLeft:
                    case FlyoutPlacementMode.BottomEdgeAlignedRight:
                        value = new Rect(
                            new Point(0, -Offset),
                            new Point(targetSize.Width, targetSize.Height + Offset));
                        break;
                    case FlyoutPlacementMode.Left:
                    case FlyoutPlacementMode.Right:
                    case FlyoutPlacementMode.LeftEdgeAlignedTop:
                    case FlyoutPlacementMode.LeftEdgeAlignedBottom:
                    case FlyoutPlacementMode.RightEdgeAlignedTop:
                    case FlyoutPlacementMode.RightEdgeAlignedBottom:
                        value = new Rect(
                            new Point(-Offset, 0),
                            new Point(targetSize.Width + Offset, targetSize.Height));
                        break;
                }
            }

            return value;
        }

        private void OnPopupOpened(object sender, EventArgs e)
        {
            OnOpened();
        }

        private void OnPopupClosing(object sender, EventArgs e)
        {
            Closing?.Invoke(this, new FlyoutBaseClosingEventArgs()); // TODO: Cancel
            m_closing = true;
        }

        private void OnPopupClosed(object sender, EventArgs e)
        {
            m_closing = false;

            if (!m_popup.IsOpen)
            {
                m_popup.ClearValue(Popup.PlacementProperty);
                m_popup.ClearValue(Popup.PlacementTargetProperty);
                m_popup.ClearValue(Popup.PlacementRectangleProperty);
                m_popup.ClearValue(FrameworkElement.WidthProperty);
                m_popup.ClearValue(FrameworkElement.HeightProperty);
                m_target = null;
                m_showingAsContextFlyout = false;
            }

            OnClosed();
        }

        private void OnPopupIsOpenChanged(object sender, EventArgs e)
        {
            UpdateIsOpen();
        }

        private CustomPopupPlacement[] PositionPopup(Size popupSize, Size targetSize, Point offset)
        {
            return PositionPopup(popupSize, targetSize, offset, m_presenter);
        }

        internal CustomPopupPlacement[] PositionPopup(Size popupSize, Size targetSize, Point offset, FrameworkElement child)
        {
            return CustomPopupPlacementHelper.PositionPopup((CustomPlacementMode)Placement, popupSize, targetSize, offset, child);
        }

        private void CancelAsyncShow()
        {
            m_pendingShow = null;

            if (m_asyncShow != null)
            {
                m_asyncShow.Abort();
                m_asyncShow = null;
            }
        }

        private static readonly IMultiValueConverter s_fullPlacementWidthConverter = new FullPlacementWidthConverter();
        private static readonly IMultiValueConverter s_fullPlacementHeightConverter = new FullPlacementHeightConverter();
        private static readonly IValueConverter s_placementConverter = new PlacementConverter();

        private const double s_offset = 4;

        private Control m_presenter;
        private PopupEx m_popup;
        private FrameworkElement m_target;
        private bool m_showingAsContextFlyout;
        private WeakReference<IInputElement> m_weakRefToPreviousFocus;
        private bool m_closing;
        private Action m_pendingShow;
        private DispatcherOperation m_asyncShow;

        private class FullPlacementWidthConverter : IMultiValueConverter
        {
            public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
            {
                double windowWidth = (double)values[0];
                Thickness border = (Thickness)values[1];
                return windowWidth - border.Left - border.Right;
            }

            public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        private class FullPlacementHeightConverter : IMultiValueConverter
        {
            public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
            {
                double windowHeight = (double)values[0];
                Thickness border = (Thickness)values[1];
                return windowHeight - border.Top - border.Bottom;
            }

            public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        private class PlacementConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return (CustomPlacementMode)value;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }
    }
}
