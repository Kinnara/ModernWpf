using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

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

        public event EventHandler<object> Closed;
        public event EventHandler<object> Opened;
        public event EventHandler<object> Opening;

        public void ShowAt(FrameworkElement placementTarget)
        {
            if (placementTarget is null)
            {
                throw new ArgumentNullException(nameof(placementTarget));
            }

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
            Opening?.Invoke(this, s_sharedEventArgs);
            m_popup.IsOpen = true;
        }

        public void Hide()
        {
            if (m_popup != null)
            {
                m_popup.IsOpen = false;
            }
        }

        protected abstract Control CreatePresenter();

        private void EnsurePresenter()
        {
            if (m_presenter == null)
            {
                m_presenter = CreatePresenter();
            }
        }

        private void EnsureShadowChrome()
        {
            if (m_shadowChrome == null)
            {
                EnsurePresenter();

                m_shadowChrome = new ThemeShadowChrome();
                m_shadowChrome.BeginInit();
                m_shadowChrome.Child = m_presenter;
                m_shadowChrome.Margin = new Thickness();

                m_shadowChrome.SetBinding(ThemeShadowChrome.CornerRadiusProperty, ControlHelper.CornerRadiusProperty, m_presenter);
                m_shadowChrome.SetBinding(FrameworkElement.MaxWidthProperty, FrameworkElement.MaxWidthProperty, m_presenter);
                m_shadowChrome.SetBinding(FrameworkElement.MaxHeightProperty, FrameworkElement.MaxHeightProperty, m_presenter);

                if (m_presenter is FlyoutPresenter)
                {
                    m_shadowChrome.SetBinding(ThemeShadowChrome.IsShadowEnabledProperty, FlyoutPresenter.IsDefaultShadowEnabledProperty, m_presenter);
                }

                m_shadowChrome.EndInit();
            }
        }

        private void EnsurePopup()
        {
            if (m_popup == null)
            {
                EnsureShadowChrome();

                m_popup = new Popup
                {
                    Child = m_shadowChrome,
                    StaysOpen = false,
                    AllowsTransparency = true,
                    PopupAnimation = PopupAnimation.Fade,
                    CustomPopupPlacementCallback = PositionPopup,
                    HorizontalOffset = 0,
                    VerticalOffset = 0
                };
                m_popup.Opened += OnPopupOpened;
                m_popup.Closed += OnPopupClosed;
                m_popup.PreviewMouseLeftButtonDown += OnPopupPreviewMouseLeftButtonDown;
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
                m_popup.HorizontalOffset = 0;
                m_popup.VerticalOffset = 0;

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

                m_shadowChrome.Margin = new Thickness();
            }
            else
            {
                m_popup.Placement = PlacementMode.Custom;
                m_popup.PlacementTarget = placementTarget;
                m_popup.ClearValue(FrameworkElement.WidthProperty);
                m_popup.ClearValue(FrameworkElement.HeightProperty);

                if (m_popup.GetBindingExpression(Popup.HorizontalOffsetProperty) == null)
                {
                    m_popup.SetBinding(Popup.HorizontalOffsetProperty, ThemeShadowChrome.DesiredPopupHorizontalOffsetProperty, m_shadowChrome);
                    m_popup.SetBinding(Popup.VerticalOffsetProperty, ThemeShadowChrome.DesiredPopupVerticalOffsetProperty, m_shadowChrome);
                    m_shadowChrome.SetBinding(FrameworkElement.MarginProperty, ThemeShadowChrome.DesiredMarginProperty, m_shadowChrome);
                }
                else
                {
                    Debug.Assert(m_popup.GetBindingExpression(Popup.VerticalOffsetProperty) != null);
                    Debug.Assert(m_shadowChrome.GetBindingExpression(FrameworkElement.MarginProperty) != null);
                }
            }
        }

        private void OnPopupOpened(object sender, EventArgs e)
        {
            Opened?.Invoke(this, s_sharedEventArgs);
        }

        private void OnPopupClosed(object sender, EventArgs e)
        {
            Closed?.Invoke(this, s_sharedEventArgs);

            if (!m_popup.IsOpen)
            {
                m_popup.ClearValue(Popup.PlacementProperty);
                m_popup.ClearValue(Popup.PlacementTargetProperty);
                m_popup.ClearValue(FrameworkElement.WidthProperty);
                m_popup.ClearValue(FrameworkElement.HeightProperty);
                m_target = null;
            }
        }

        private void OnPopupPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!m_popup.IsOpen)
            {
                e.Handled = true;
            }
        }

        private CustomPopupPlacement[] PositionPopup(Size popupSize, Size targetSize, Point offset)
        {
            var placement = Placement;

            CustomPopupPlacement preferredPlacement = GetPopupPlacement(placement, popupSize, targetSize);

            CustomPopupPlacement? alternativePlacement = null;
            var alternativePlacementMode = GetAlternativePlacementMode(placement);
            if (alternativePlacementMode.HasValue)
            {
                alternativePlacement = GetPopupPlacement(alternativePlacementMode.Value, popupSize, targetSize);
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

        private static CustomPopupPlacement GetPopupPlacement(FlyoutPlacementMode placement, Size popupSize, Size targetSize)
        {
            Point point;
            PopupPrimaryAxis primaryAxis;

            switch (placement)
            {
                case FlyoutPlacementMode.Top:
                    point = new Point((targetSize.Width - popupSize.Width) / 2, -popupSize.Height);
                    point.Y -= s_offset;
                    primaryAxis = PopupPrimaryAxis.Vertical;
                    break;
                case FlyoutPlacementMode.Bottom:
                    point = new Point((targetSize.Width - popupSize.Width) / 2, targetSize.Height);
                    point.Y += s_offset;
                    primaryAxis = PopupPrimaryAxis.Vertical;
                    break;
                case FlyoutPlacementMode.Left:
                    point = new Point(-popupSize.Width, (targetSize.Height - popupSize.Height) / 2);
                    point.X -= s_offset;
                    primaryAxis = PopupPrimaryAxis.Horizontal;
                    break;
                case FlyoutPlacementMode.Right:
                    point = new Point(targetSize.Width, (targetSize.Height - popupSize.Height) / 2);
                    point.X += s_offset;
                    primaryAxis = PopupPrimaryAxis.Horizontal;
                    break;
                case FlyoutPlacementMode.Full:
                    point = new Point((targetSize.Width - popupSize.Width) / 2, (targetSize.Height - popupSize.Height) / 2);
                    primaryAxis = PopupPrimaryAxis.None;
                    break;
                case FlyoutPlacementMode.TopEdgeAlignedLeft:
                    point = new Point(0, -popupSize.Height);
                    point.Y -= s_offset;
                    primaryAxis = PopupPrimaryAxis.Vertical;
                    break;
                case FlyoutPlacementMode.TopEdgeAlignedRight:
                    point = new Point(targetSize.Width - popupSize.Width, -popupSize.Height);
                    point.Y -= s_offset;
                    primaryAxis = PopupPrimaryAxis.Vertical;
                    break;
                case FlyoutPlacementMode.BottomEdgeAlignedLeft:
                    point = new Point(0, targetSize.Height);
                    point.Y += s_offset;
                    primaryAxis = PopupPrimaryAxis.Vertical;
                    break;
                case FlyoutPlacementMode.BottomEdgeAlignedRight:
                    point = new Point(targetSize.Width - popupSize.Width, targetSize.Height);
                    point.Y += s_offset;
                    primaryAxis = PopupPrimaryAxis.Vertical;
                    break;
                case FlyoutPlacementMode.LeftEdgeAlignedTop:
                    point = new Point(-popupSize.Width, 0);
                    point.X -= s_offset;
                    primaryAxis = PopupPrimaryAxis.Horizontal;
                    break;
                case FlyoutPlacementMode.LeftEdgeAlignedBottom:
                    point = new Point(-popupSize.Width, targetSize.Height - popupSize.Height);
                    point.X -= s_offset;
                    primaryAxis = PopupPrimaryAxis.Horizontal;
                    break;
                case FlyoutPlacementMode.RightEdgeAlignedTop:
                    point = new Point(targetSize.Width, 0);
                    point.X += s_offset;
                    primaryAxis = PopupPrimaryAxis.Horizontal;
                    break;
                case FlyoutPlacementMode.RightEdgeAlignedBottom:
                    point = new Point(targetSize.Width, targetSize.Height - popupSize.Height);
                    point.X += s_offset;
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

        private static readonly object s_sharedEventArgs = new object();
        private static readonly IMultiValueConverter s_fullPlacementWidthConverter = new FullPlacementWidthConverter();
        private static readonly IMultiValueConverter s_fullPlacementHeightConverter = new FullPlacementHeightConverter();

        private const double s_offset = 4;

        private Control m_presenter;
        private Popup m_popup;
        private FrameworkElement m_target;
        private ThemeShadowChrome m_shadowChrome;

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
    }
}
