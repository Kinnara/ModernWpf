using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

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
            if (m_popup != null)
            {
                m_popup.PopupAnimation = (bool)e.NewValue ? PopupAnimation.Fade : PopupAnimation.None;
            }
        }

        #endregion

        #region IsOpen

        private static readonly DependencyPropertyKey IsOpenPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(IsOpen),
                typeof(bool),
                typeof(FlyoutBase),
                new PropertyMetadata(false));

        public static readonly DependencyProperty IsOpenProperty =
            IsOpenPropertyKey.DependencyProperty;

        public bool IsOpen
        {
            get => (bool)GetValue(IsOpenProperty);
            internal set => SetValue(IsOpenPropertyKey, value);
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

        internal PopupEx InternalPopup => m_popup;

        public event EventHandler<object> Opening;
        public event EventHandler<object> Opened;
        public event EventHandler<object> Closed;
        internal event EventHandler<object> Closing;

        public void ShowAt(FrameworkElement placementTarget)
        {
            if (placementTarget is null)
            {
                throw new ArgumentNullException(nameof(placementTarget));
            }

            ShowAtCore(placementTarget);
        }

        public void Hide()
        {
            HideCore();
        }

        protected abstract Control CreatePresenter();

        internal virtual void ShowAtCore(FrameworkElement placementTarget)
        {
            if (m_popup != null &&
                m_popup.IsOpen &&
                m_target == placementTarget)
            {
                return;
            }

            PreparePopup(placementTarget);

            Debug.Assert(m_popup.ReadLocalValue(Popup.PlacementProperty) != DependencyProperty.UnsetValue);
            Debug.Assert(m_popup.ReadLocalValue(Popup.PlacementTargetProperty) != DependencyProperty.UnsetValue);

            m_target = placementTarget;
            RaiseOpening();
            m_popup.IsOpen = true;
        }

        internal virtual void HideCore()
        {
            if (m_popup != null && m_popup.IsOpen)
            {
                m_popup.IsOpen = false;
            }
        }

        internal void RaiseOpening()
        {
            Opening?.Invoke(this, null);
        }

        internal void RaiseOpened()
        {
            Opened?.Invoke(this, null);
        }

        internal void RaiseClosed()
        {
            Closed?.Invoke(this, null);
        }

        internal void BindPlacement(Control presenter)
        {
            presenter.SetBinding(
                PopupPlacementHelper.PlacementProperty,
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
                    PopupAnimation = AreOpenCloseAnimationsEnabled ? PopupAnimation.Fade : PopupAnimation.None,
                    CustomPopupPlacementCallback = PositionPopup
                };
                m_popup.Opened += OnPopupOpened;
                m_popup.Closing += OnPopupClosing;
                m_popup.Closed += OnPopupClosed;
                m_popup.IsOpenChanged += OnPopupIsOpenChanged;
            }
        }

        private void PreparePopup(FrameworkElement placementTarget)
        {
            EnsurePopup();

            if (m_popup.IsOpen)
            {
                m_popup.IsOpen = false;
            }

            if (Placement == FlyoutPlacementMode.Full &&
                Window.GetWindow(placementTarget) is Window window)
            {
                m_popup.Placement = PlacementMode.Center;
                m_popup.PlacementTarget = window;

                m_popup.SetBinding(
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

                m_popup.SetBinding(
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
            else
            {
                m_popup.Placement = PlacementMode.Custom;
                m_popup.PlacementTarget = placementTarget;
                m_popup.ClearValue(FrameworkElement.WidthProperty);
                m_popup.ClearValue(FrameworkElement.HeightProperty);
            }
        }

        private void OnPopupOpened(object sender, EventArgs e)
        {
            RaiseOpened();
        }

        private void OnPopupClosing(object sender, EventArgs e)
        {
            Closing?.Invoke(this, null);
        }

        private void OnPopupClosed(object sender, EventArgs e)
        {
            RaiseClosed();

            if (!m_popup.IsOpen)
            {
                m_popup.ClearValue(Popup.PlacementProperty);
                m_popup.ClearValue(Popup.PlacementTargetProperty);
                m_popup.ClearValue(FrameworkElement.WidthProperty);
                m_popup.ClearValue(FrameworkElement.HeightProperty);
                m_target = null;
            }
        }

        private void OnPopupIsOpenChanged(object sender, EventArgs e)
        {
            UpdateIsOpen();
        }

        internal CustomPopupPlacement[] PositionPopup(Size popupSize, Size targetSize, Point offset)
        {
            return PositionPopup(Placement, popupSize, targetSize, s_offset);
        }

        internal static CustomPopupPlacement[] PositionPopup(FlyoutPlacementMode placement, Size popupSize, Size targetSize, double offset = 0)
        {
            CustomPopupPlacement preferredPlacement = CalculatePopupPlacement(placement, popupSize, targetSize, offset);

            CustomPopupPlacement? alternativePlacement = null;
            var alternativePlacementMode = GetAlternativePlacementMode(placement);
            if (alternativePlacementMode.HasValue)
            {
                alternativePlacement = CalculatePopupPlacement(alternativePlacementMode.Value, popupSize, targetSize, offset);
            }

            if (alternativePlacement.HasValue)
            {
                return new[] { preferredPlacement, alternativePlacement.Value };
            }
            else
            {
                return new[] { preferredPlacement };
            }
        }

        private static CustomPopupPlacement CalculatePopupPlacement(FlyoutPlacementMode placement, Size popupSize, Size targetSize, double offset)
        {
            Point point;
            PopupPrimaryAxis primaryAxis;

            switch (placement)
            {
                case FlyoutPlacementMode.Top:
                    point = new Point((targetSize.Width - popupSize.Width) / 2, -popupSize.Height);
                    point.Y -= offset;
                    primaryAxis = PopupPrimaryAxis.Vertical;
                    break;
                case FlyoutPlacementMode.Bottom:
                    point = new Point((targetSize.Width - popupSize.Width) / 2, targetSize.Height);
                    point.Y += offset;
                    primaryAxis = PopupPrimaryAxis.Vertical;
                    break;
                case FlyoutPlacementMode.Left:
                    point = new Point(-popupSize.Width, (targetSize.Height - popupSize.Height) / 2);
                    point.X -= offset;
                    primaryAxis = PopupPrimaryAxis.Horizontal;
                    break;
                case FlyoutPlacementMode.Right:
                    point = new Point(targetSize.Width, (targetSize.Height - popupSize.Height) / 2);
                    point.X += offset;
                    primaryAxis = PopupPrimaryAxis.Horizontal;
                    break;
                case FlyoutPlacementMode.Full:
                    point = new Point((targetSize.Width - popupSize.Width) / 2, (targetSize.Height - popupSize.Height) / 2);
                    primaryAxis = PopupPrimaryAxis.None;
                    break;
                case FlyoutPlacementMode.TopEdgeAlignedLeft:
                    point = new Point(0, -popupSize.Height);
                    point.Y -= offset;
                    primaryAxis = PopupPrimaryAxis.Vertical;
                    break;
                case FlyoutPlacementMode.TopEdgeAlignedRight:
                    point = new Point(targetSize.Width - popupSize.Width, -popupSize.Height);
                    point.Y -= offset;
                    primaryAxis = PopupPrimaryAxis.Vertical;
                    break;
                case FlyoutPlacementMode.BottomEdgeAlignedLeft:
                    point = new Point(0, targetSize.Height);
                    point.Y += offset;
                    primaryAxis = PopupPrimaryAxis.Vertical;
                    break;
                case FlyoutPlacementMode.BottomEdgeAlignedRight:
                    point = new Point(targetSize.Width - popupSize.Width, targetSize.Height);
                    point.Y += offset;
                    primaryAxis = PopupPrimaryAxis.Vertical;
                    break;
                case FlyoutPlacementMode.LeftEdgeAlignedTop:
                    point = new Point(-popupSize.Width, 0);
                    point.X -= offset;
                    primaryAxis = PopupPrimaryAxis.Horizontal;
                    break;
                case FlyoutPlacementMode.LeftEdgeAlignedBottom:
                    point = new Point(-popupSize.Width, targetSize.Height - popupSize.Height);
                    point.X -= offset;
                    primaryAxis = PopupPrimaryAxis.Horizontal;
                    break;
                case FlyoutPlacementMode.RightEdgeAlignedTop:
                    point = new Point(targetSize.Width, 0);
                    point.X += offset;
                    primaryAxis = PopupPrimaryAxis.Horizontal;
                    break;
                case FlyoutPlacementMode.RightEdgeAlignedBottom:
                    point = new Point(targetSize.Width, targetSize.Height - popupSize.Height);
                    point.X += offset;
                    primaryAxis = PopupPrimaryAxis.Horizontal;
                    break;
                //case FlyoutPlacementMode.Auto:
                default:
                    throw new ArgumentOutOfRangeException(nameof(placement));
            }

            return new CustomPopupPlacement(point, primaryAxis);
        }

        private static FlyoutPlacementMode? GetAlternativePlacementMode(FlyoutPlacementMode placement)
        {
            switch (placement)
            {
                case FlyoutPlacementMode.Top:
                    return FlyoutPlacementMode.Bottom;
                case FlyoutPlacementMode.Bottom:
                    return FlyoutPlacementMode.Top;
                case FlyoutPlacementMode.Left:
                    return FlyoutPlacementMode.Right;
                case FlyoutPlacementMode.Right:
                    return FlyoutPlacementMode.Left;
                case FlyoutPlacementMode.Full:
                    return null;
                case FlyoutPlacementMode.TopEdgeAlignedLeft:
                    return FlyoutPlacementMode.BottomEdgeAlignedLeft;
                case FlyoutPlacementMode.TopEdgeAlignedRight:
                    return FlyoutPlacementMode.BottomEdgeAlignedRight;
                case FlyoutPlacementMode.BottomEdgeAlignedLeft:
                    return FlyoutPlacementMode.TopEdgeAlignedLeft;
                case FlyoutPlacementMode.BottomEdgeAlignedRight:
                    return FlyoutPlacementMode.TopEdgeAlignedRight;
                case FlyoutPlacementMode.LeftEdgeAlignedTop:
                    return FlyoutPlacementMode.RightEdgeAlignedTop;
                case FlyoutPlacementMode.LeftEdgeAlignedBottom:
                    return FlyoutPlacementMode.RightEdgeAlignedBottom;
                case FlyoutPlacementMode.RightEdgeAlignedTop:
                    return FlyoutPlacementMode.RightEdgeAlignedTop;
                case FlyoutPlacementMode.RightEdgeAlignedBottom:
                    return FlyoutPlacementMode.LeftEdgeAlignedBottom;
                //case FlyoutPlacementMode.Auto:
                default:
                    return null;
            }
        }

        private static readonly IMultiValueConverter s_fullPlacementWidthConverter = new FullPlacementWidthConverter();
        private static readonly IMultiValueConverter s_fullPlacementHeightConverter = new FullPlacementHeightConverter();
        private static readonly IValueConverter s_placementConverter = new PlacementConverter();

        private const double s_offset = 4;

        private Control m_presenter;
        private PopupEx m_popup;
        private FrameworkElement m_target;

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
                double desiredHeight = windowHeight - border.Top - border.Bottom;
                double screenHeight = SystemParameters.PrimaryScreenHeight;
                return Math.Min(desiredHeight, screenHeight * 0.75);
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
                return (CustomPopupPlacementMode)value;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }
    }
}
