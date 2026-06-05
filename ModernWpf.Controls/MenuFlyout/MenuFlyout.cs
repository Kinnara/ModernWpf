using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    [ContentProperty(nameof(Items))]
    public partial class MenuFlyout : FlyoutBase
    {
        public MenuFlyout()
        {
        }

        public ItemCollection Items
        {
            get
            {
                EnsurePresenter();
                return m_presenter.Items;
            }
        }

        private static void OnMenuFlyoutPresenterStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((MenuFlyout)d).OnMenuFlyoutPresenterStyleChanged(e);
        }

        private void OnMenuFlyoutPresenterStyleChanged(DependencyPropertyChangedEventArgs e)
        {
            if (m_presenter != null)
            {
                m_presenter.Style = (Style)e.NewValue;
            }
        }

        protected override Control CreatePresenter()
        {
            throw new InvalidOperationException();
        }

        internal override void ShowAtCore(FrameworkElement placementTarget, bool showAsContextFlyout = false, FlyoutShowOptions showOptions = null)
        {
            if (showAsContextFlyout)
            {
                Show(placementTarget, PlacementMode.MousePoint);
            }
            else
            {
                Show(placementTarget, PlacementMode.Custom, showOptions);
            }
        }

        internal MenuFlyoutPresenter Presenter
        {
            get
            {
                EnsurePresenter();
                return m_presenter;
            }
        }

        internal override void HideCore()
        {
            if (m_presenter != null && m_presenter.IsOpen)
            {
                m_presenter.IsOpen = false;
            }
        }

        internal override void OnIsOpenChanged()
        {
            base.OnIsOpenChanged();
        }

        internal override void UpdateIsOpen()
        {
            IsOpen = m_presenter != null && m_presenter.IsOpen;
        }

        internal override void OnAreOpenCloseAnimationsEnabledChanged(DependencyPropertyChangedEventArgs e)
        {
            m_presenter?.UpdatePopupAnimation();
        }

        protected override FrameworkElement PointerMoveAwayBoundsElement => m_presenter;

        protected override Control FocusTarget => m_presenter;

        private void Show(FrameworkElement placementTarget, PlacementMode placement = PlacementMode.Custom, FlyoutShowOptions showOptions = null)
        {
            showOptions = CloneShowOptions(showOptions);
            bool showAsContextFlyout = placement == PlacementMode.MousePoint;
            ApplyShowOptions(showOptions, showAsContextFlyout);

            if (m_presenter != null &&
                m_presenter.IsOpen &&
                IsPresenterPlacementTargetForShow(placementTarget) &&
                IsPresenterPlacementForShow(placement) &&
                IsSameTargetPosition(showOptions, showAsContextFlyout))
            {
                return;
            }

            if (TryStageLatestShowUntilOpenFlyoutCloses(placementTarget, showAsContextFlyout, showOptions))
            {
                return;
            }

            EnsurePresenter();
            var effectivePlacement = GetEffectivePlacement();
            var hasAbsolutePlacementPoint =
                placement == PlacementMode.Custom &&
                TryGetAbsolutePlacementPoint(placementTarget, effectivePlacement, out var absolutePlacementPoint);
            m_presenter.SetCurrentValue(CustomPopupPlacementHelper.PlacementProperty, (CustomPlacementMode)effectivePlacement);

            if (m_presenter.IsOpen)
            {
                m_presenter.IsOpen = false;
            }

            Target = placementTarget;

            if (hasAbsolutePlacementPoint)
            {
                m_presenter.SetAbsolutePlacementPoint(absolutePlacementPoint);
                m_presenter.Placement = PlacementMode.AbsolutePoint;
                m_presenter.HorizontalOffset = absolutePlacementPoint.X;
                m_presenter.VerticalOffset = absolutePlacementPoint.Y;
                m_presenter.ClearValue(ContextMenu.PlacementTargetProperty);
                m_presenter.ClearValue(ContextMenu.PlacementRectangleProperty);
            }
            else
            {
                m_presenter.SetAbsolutePlacementPoint(null);
                m_presenter.Placement = placement;
                m_presenter.PlacementTarget = placementTarget;
                m_presenter.HorizontalOffset = 0;
                m_presenter.VerticalOffset = 0;
            }

            if (!hasAbsolutePlacementPoint && placement == PlacementMode.Custom)
            {
                m_presenter.PlacementRectangle = GetPlacementRectangle(placementTarget, effectivePlacement);
            }
            else
            {
                m_presenter.ClearValue(ContextMenu.PlacementRectangleProperty);
            }

            TrackPlacementTarget(placementTarget);
            OnOpening();
            SetOpenFlyout(this);
            m_presenter.IsOpen = true;
        }

        private bool IsPresenterPlacementTargetForShow(FrameworkElement placementTarget)
        {
            return m_presenter.PlacementTarget == placementTarget ||
                (m_presenter.Placement == PlacementMode.AbsolutePoint && Target == placementTarget);
        }

        private bool IsPresenterPlacementForShow(PlacementMode placement)
        {
            if (m_presenter.Placement == placement)
            {
                return true;
            }

            return placement == PlacementMode.Custom &&
                m_presenter.Placement == PlacementMode.AbsolutePoint;
        }

        private CustomPopupPlacement[] PositionPopup(Size popupSize, Size targetSize, Point offset)
        {
            return PositionPopup(popupSize, targetSize, offset, null);
        }

        private bool TryGetAbsolutePlacementPoint(
            FrameworkElement placementTarget,
            FlyoutPlacementMode effectivePlacement,
            out Point point)
        {
            point = default;

            var placementRect = GetPlacementRectangle(placementTarget, effectivePlacement);
            if (placementRect.IsEmpty)
            {
                return false;
            }

            var topLeft = placementTarget.PointToScreen(placementRect.TopLeft);
            var bottomRight = placementTarget.PointToScreen(placementRect.BottomRight);
            var targetRect = new Rect(topLeft, bottomRight);
            var popupSize = GetPresenterDesiredScreenSize(placementTarget);

            switch (effectivePlacement)
            {
                case FlyoutPlacementMode.Top:
                case FlyoutPlacementMode.TopEdgeAlignedLeft:
                    point = new Point(targetRect.Left, targetRect.Top - popupSize.Height);
                    return true;
                case FlyoutPlacementMode.TopEdgeAlignedRight:
                    point = new Point(targetRect.Right - popupSize.Width, targetRect.Top - popupSize.Height);
                    return true;
                case FlyoutPlacementMode.Bottom:
                case FlyoutPlacementMode.BottomEdgeAlignedLeft:
                    point = new Point(targetRect.Left, targetRect.Bottom);
                    return true;
                case FlyoutPlacementMode.BottomEdgeAlignedRight:
                    point = new Point(targetRect.Right - popupSize.Width, targetRect.Bottom);
                    return true;
                case FlyoutPlacementMode.Left:
                case FlyoutPlacementMode.LeftEdgeAlignedTop:
                    point = new Point(targetRect.Left - popupSize.Width, targetRect.Top);
                    return true;
                case FlyoutPlacementMode.LeftEdgeAlignedBottom:
                    point = new Point(targetRect.Left - popupSize.Width, targetRect.Bottom - popupSize.Height);
                    return true;
                case FlyoutPlacementMode.Right:
                case FlyoutPlacementMode.RightEdgeAlignedTop:
                    point = new Point(targetRect.Right, targetRect.Top);
                    return true;
                case FlyoutPlacementMode.RightEdgeAlignedBottom:
                    point = new Point(targetRect.Right, targetRect.Bottom - popupSize.Height);
                    return true;
                default:
                    return false;
            }
        }

        private Size GetPresenterDesiredScreenSize(FrameworkElement placementTarget)
        {
            m_presenter.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            var desiredSize = m_presenter.DesiredSize;
            var source = PresentationSource.FromVisual(placementTarget);
            if (source?.CompositionTarget != null)
            {
                var transformed = source.CompositionTarget.TransformToDevice.Transform(
                    new Vector(desiredSize.Width, desiredSize.Height));
                return new Size(transformed.X, transformed.Y);
            }

            return desiredSize;
        }

        private void EnsurePresenter()
        {
            if (m_presenter == null)
            {
                var presenter = new MenuFlyoutPresenter
                {
                    Style = MenuFlyoutPresenterStyle,
                    Placement = PlacementMode.Custom,
                    CustomPopupPlacementCallback = PositionPopup,
                    StaysOpen = false
                };
                presenter.SetOwningFlyout(this);
                BindPlacement(presenter);
                presenter.UpdatePopupAnimation();
                presenter.Opened += OnPresenterOpened;
                presenter.Closing += OnPresenterClosing;
                presenter.Closed += OnPresenterClosed;
                presenter.IsOpenChanged += OnPresenterIsOpenChanged;

                m_presenter = presenter;
            }
        }

        private void OnPresenterOpened(object sender, RoutedEventArgs e)
        {
            m_closeCompleted = false;

            if (m_suppressNextOpened)
            {
                m_suppressNextOpened = false;
                return;
            }

            OnOpened();
        }

        private void OnPresenterClosing(object sender, CancelEventArgs e)
        {
            e.Cancel = OnClosing();
            if (e.Cancel)
            {
                m_suppressNextOpened = true;
            }
        }

        private void OnPresenterClosed(object sender, RoutedEventArgs e)
        {
            CompleteClose();
        }

        private void CompleteClose()
        {
            if (m_presenter.IsOpen || m_closeCompleted)
            {
                return;
            }

            m_closeCompleted = true;
            m_presenter.ClearValue(ContextMenu.PlacementProperty);
            m_presenter.ClearValue(ContextMenu.PlacementTargetProperty);
            m_presenter.ClearValue(ContextMenu.PlacementRectangleProperty);
            m_presenter.ClearValue(ContextMenu.HorizontalOffsetProperty);
            m_presenter.ClearValue(ContextMenu.VerticalOffsetProperty);
            m_presenter.SetAbsolutePlacementPoint(null);
            ClearPlacementTargetTracking();
            Target = null;
            UpdateStateToShowMode(ShowMode);

            OnClosed();
        }

        private void OnPresenterIsOpenChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UpdateIsOpen();

            if (!(bool)e.NewValue)
            {
                CompleteClose();
            }
        }

        private MenuFlyoutPresenter m_presenter;
        private bool m_suppressNextOpened;
        private bool m_closeCompleted;
    }
}
