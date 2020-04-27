using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    [ContentProperty(nameof(Items))]
    public class MenuFlyout : FlyoutBase
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

        #region MenuFlyoutPresenterStyle

        public static readonly DependencyProperty MenuFlyoutPresenterStyleProperty =
            DependencyProperty.Register(
                nameof(MenuFlyoutPresenterStyle),
                typeof(Style),
                typeof(MenuFlyout),
                new PropertyMetadata(OnMenuFlyoutPresenterStyleChanged));

        public Style MenuFlyoutPresenterStyle
        {
            get => (Style)GetValue(MenuFlyoutPresenterStyleProperty);
            set => SetValue(MenuFlyoutPresenterStyleProperty, value);
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

        #endregion

        protected override Control CreatePresenter()
        {
            throw new InvalidOperationException();
        }

        internal override void ShowAtCore(FrameworkElement placementTarget, bool showAsContextFlyout = false)
        {
            if (showAsContextFlyout)
            {
                Show(placementTarget, PlacementMode.MousePoint);
            }
            else
            {
                Show(placementTarget);
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
        }

        internal override void UpdateIsOpen()
        {
            IsOpen = m_presenter != null && m_presenter.IsOpen;
        }

        internal override void OnAreOpenCloseAnimationsEnabledChanged(DependencyPropertyChangedEventArgs e)
        {
            m_presenter?.UpdatePopupAnimation();
        }

        private void Show(FrameworkElement placementTarget, PlacementMode placement = PlacementMode.Custom)
        {
            if (m_presenter != null &&
                m_presenter.IsOpen &&
                m_presenter.PlacementTarget == placementTarget &&
                m_presenter.Placement == placement &&
                m_currentPlacement == Placement)
            {
                return;
            }

            EnsurePresenter();

            if (m_presenter.IsOpen)
            {
                m_presenter.IsOpen = false;
            }

            m_presenter.Placement = placement;
            m_presenter.PlacementTarget = placementTarget;

            if (placement == PlacementMode.Custom)
            {
                m_presenter.PlacementRectangle = GetPlacementRectangle(placementTarget);
            }
            else
            {
                m_presenter.ClearValue(Popup.PlacementRectangleProperty);
            }

            m_currentPlacement = Placement;
            OnOpening();
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
                presenter.Closed += OnPresenterClosed;
                presenter.IsOpenChanged += OnPresenterIsOpenChanged;

                m_presenter = presenter;
            }
        }

        private void OnPresenterOpened(object sender, RoutedEventArgs e)
        {
            OnOpened();
        }

        private void OnPresenterClosed(object sender, RoutedEventArgs e)
        {
            if (!m_presenter.IsOpen)
            {
                m_presenter.ClearValue(ContextMenu.PlacementProperty);
                m_presenter.ClearValue(ContextMenu.PlacementTargetProperty);
                m_presenter.ClearValue(ContextMenu.PlacementRectangleProperty);
                m_currentPlacement = null;
            }

            OnClosed();
        }

        private void OnPresenterIsOpenChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UpdateIsOpen();
        }

        private MenuFlyoutPresenter m_presenter;
        private FlyoutPlacementMode? m_currentPlacement;
    }
}
