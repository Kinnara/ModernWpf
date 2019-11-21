using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    public class SimpleToolBar : ToolBar
    {
        static SimpleToolBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SimpleToolBar), new FrameworkPropertyMetadata(typeof(SimpleToolBar)));
        }

        #region DefaultLabelPosition

        public static readonly DependencyProperty DefaultLabelPositionProperty =
            DependencyProperty.RegisterAttached(
                nameof(DefaultLabelPosition),
                typeof(CommandBarDefaultLabelPosition),
                typeof(SimpleToolBar),
                new PropertyMetadata(CommandBarDefaultLabelPosition.Right));

        public CommandBarDefaultLabelPosition DefaultLabelPosition
        {
            get => (CommandBarDefaultLabelPosition)GetValue(DefaultLabelPositionProperty);
            set => SetValue(DefaultLabelPositionProperty, value);
        }

        #endregion

        #region OverflowButtonVisibility

        public static readonly DependencyProperty OverflowButtonVisibilityProperty =
            DependencyProperty.Register(
                nameof(OverflowButtonVisibility),
                typeof(Visibility),
                typeof(SimpleToolBar),
                new PropertyMetadata(Visibility.Visible));

        public Visibility OverflowButtonVisibility
        {
            get => (Visibility)GetValue(OverflowButtonVisibilityProperty);
            set => SetValue(OverflowButtonVisibilityProperty, value);
        }

        #endregion

        #region OverflowPresenterStyle

        public static readonly DependencyProperty OverflowPresenterStyleProperty =
            DependencyProperty.Register(
                nameof(OverflowPresenterStyle),
                typeof(Style),
                typeof(SimpleToolBar),
                null);

        public Style OverflowPresenterStyle
        {
            get => (Style)GetValue(OverflowPresenterStyleProperty);
            set => SetValue(OverflowPresenterStyleProperty, value);
        }

        #endregion

        #region OverflowContentMaxHeight

        private static readonly DependencyPropertyKey OverflowContentMaxHeightPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(OverflowContentMaxHeight),
                typeof(double),
                typeof(SimpleToolBar),
                new PropertyMetadata(CalculateOverflowContentMaxHeight()));

        public static readonly DependencyProperty OverflowContentMaxHeightProperty =
            OverflowContentMaxHeightPropertyKey.DependencyProperty;

        public double OverflowContentMaxHeight
        {
            get => (double)GetValue(OverflowContentMaxHeightProperty);
            private set => SetValue(OverflowContentMaxHeightPropertyKey, value);
        }

        private void UpdateOverflowContentMaxHeight()
        {
            OverflowContentMaxHeight = CalculateOverflowContentMaxHeight();
        }

        private static double CalculateOverflowContentMaxHeight()
        {
            return SystemParameters.PrimaryScreenHeight / 2 + 20;
        }

        #endregion

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (m_overflowPopup != null)
            {
                m_overflowPopup.ClearValue(Popup.CustomPopupPlacementCallbackProperty);
                m_overflowPopup.Opened -= OnOverflowPopupOpened;
            }

            m_overflowPopup = GetTemplateChild(OverflowPopupName) as Popup;
            m_overflowPanel = GetTemplateChild(ToolBarOverflowPanelName) as ToolBarOverflowPanel;

            if (m_overflowPopup != null)
            {
                m_overflowPopup.CustomPopupPlacementCallback = PositionPopup;
                m_overflowPopup.Placement = PlacementMode.Custom;
                m_overflowPopup.Opened += OnOverflowPopupOpened;
            }
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);

            if (element is AppBarButton appBarButton)
            {
                appBarButton.SetBinding(DefaultLabelPositionProperty, DefaultLabelPositionProperty, this);
            }
        }

        protected override void ClearContainerForItemOverride(DependencyObject element, object item)
        {
            if (element is AppBarButton appBarButton)
            {
                appBarButton.ClearValue(DefaultLabelPositionProperty);
            }

            base.ClearContainerForItemOverride(element, item);
        }

        private void OnOverflowPopupOpened(object sender, EventArgs e)
        {
            UpdateOverflowContentMaxHeight();
        }

        private CustomPopupPlacement[] PositionPopup(Size popupSize, Size targetSize, Point offset)
        {
            return FlyoutBase.PositionPopup(FlyoutPlacementMode.BottomEdgeAlignedRight, popupSize, targetSize);
        }

        private Popup m_overflowPopup;
        private ToolBarOverflowPanel m_overflowPanel;

        private const string OverflowPopupName = "OverflowPopup";
        private const string ToolBarOverflowPanelName = "PART_ToolBarOverflowPanel";
    }
}
