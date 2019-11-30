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

        internal override void ShowAtCore(FrameworkElement placementTarget)
        {
            Show(placementTarget);
        }

        internal override void ShowAsContextFlyoutCore(FrameworkElement placementTarget)
        {
            Show(placementTarget, PlacementMode.MousePoint);
        }

        internal override void HideCore()
        {
            if (m_presenter != null && m_presenter.IsOpen)
            {
                m_presenter.IsOpen = false;
            }
        }

        internal override void UpdateIsOpen()
        {
            IsOpen = m_presenter != null && m_presenter.IsOpen;
        }

        internal override void OnAreOpenCloseAnimationsEnabledChanged(DependencyPropertyChangedEventArgs e)
        {
            m_presenter?.UpdatePopupAnimation((bool)e.NewValue);
        }

        private void Show(FrameworkElement placementTarget, PlacementMode placement = PlacementMode.Custom)
        {
            if (m_presenter != null &&
                m_presenter.IsOpen &&
                m_presenter.PlacementTarget == placementTarget)
            {
                return;
            }

            EnsurePresenter();

            if (m_presenter.IsOpen)
            {
                m_presenter.IsOpen = false;
            }

            m_presenter.PlacementTarget = placementTarget;
            m_presenter.Placement = placement;
            RaiseOpening();
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
                BindPlacement(presenter);
                presenter.UpdatePopupAnimation(AreOpenCloseAnimationsEnabled);
                presenter.Opened += OnPresenterOpened;
                presenter.Closed += OnPresenterClosed;
                presenter.IsOpenChanged += OnPresenterIsOpenChanged;

                m_presenter = presenter;
            }
        }

        private void OnPresenterOpened(object sender, RoutedEventArgs e)
        {
            RaiseOpened();
        }

        private void OnPresenterClosed(object sender, RoutedEventArgs e)
        {
            RaiseClosed();
        }

        private void OnPresenterIsOpenChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UpdateIsOpen();
        }

        private MenuFlyoutPresenter m_presenter;
    }
}
