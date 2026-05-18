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
                m_presenter.PlacementTarget == placementTarget &&
                m_presenter.Placement == placement &&
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
            m_presenter.SetCurrentValue(CustomPopupPlacementHelper.PlacementProperty, (CustomPlacementMode)effectivePlacement);

            if (m_presenter.IsOpen)
            {
                m_presenter.IsOpen = false;
            }

            m_presenter.Placement = placement;
            m_presenter.PlacementTarget = placementTarget;
            Target = placementTarget;

            if (placement == PlacementMode.Custom)
            {
                m_presenter.PlacementRectangle = GetPlacementRectangle(placementTarget, effectivePlacement);
            }
            else
            {
                m_presenter.ClearValue(Popup.PlacementRectangleProperty);
            }

            TrackPlacementTarget(placementTarget);
            OnOpening();
            SetOpenFlyout(this);
            m_presenter.IsOpen = true;
        }

        private CustomPopupPlacement[] PositionPopup(Size popupSize, Size targetSize, Point offset)
        {
            return PositionPopup(popupSize, targetSize, offset, m_presenter);
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
