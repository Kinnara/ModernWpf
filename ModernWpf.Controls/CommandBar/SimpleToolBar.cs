using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    public class SimpleToolBar : ToolBar
    {
        static SimpleToolBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SimpleToolBar), new FrameworkPropertyMetadata(typeof(SimpleToolBar)));
        }

        public SimpleToolBar()
        {
        }

        #region CornerRadius

        public static readonly DependencyProperty CornerRadiusProperty =
            ControlHelper.CornerRadiusProperty.AddOwner(typeof(SimpleToolBar));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        #endregion

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

        #region IsDynamicOverflowEnabled

        public static readonly DependencyProperty IsDynamicOverflowEnabledProperty =
            DependencyProperty.Register(
                nameof(IsDynamicOverflowEnabled),
                typeof(bool),
                typeof(SimpleToolBar),
                new PropertyMetadata(true));

        public bool IsDynamicOverflowEnabled
        {
            get => (bool)GetValue(IsDynamicOverflowEnabledProperty);
            set => SetValue(IsDynamicOverflowEnabledProperty, value);
        }

        #endregion

        #region OverflowButtonVisibility

        public static readonly DependencyProperty OverflowButtonVisibilityProperty =
            DependencyProperty.Register(
                nameof(OverflowButtonVisibility),
                typeof(CommandBarOverflowButtonVisibility),
                typeof(SimpleToolBar),
                new PropertyMetadata(CommandBarOverflowButtonVisibility.Auto, OnOverflowButtonVisibilityChanged));

        public CommandBarOverflowButtonVisibility OverflowButtonVisibility
        {
            get => (CommandBarOverflowButtonVisibility)GetValue(OverflowButtonVisibilityProperty);
            set => SetValue(OverflowButtonVisibilityProperty, value);
        }

        private static void OnOverflowButtonVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SimpleToolBar)d).UpdateEffectiveOverflowButtonVisibility();
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

        #region EffectiveOverflowButtonVisibility

        private static readonly DependencyPropertyKey EffectiveOverflowButtonVisibilityPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(EffectiveOverflowButtonVisibility),
                typeof(Visibility),
                typeof(SimpleToolBar),
                new PropertyMetadata(Visibility.Collapsed, OnEffectiveOverflowButtonVisibilityChanged));

        public static readonly DependencyProperty EffectiveOverflowButtonVisibilityProperty =
            EffectiveOverflowButtonVisibilityPropertyKey.DependencyProperty;

        public Visibility EffectiveOverflowButtonVisibility
        {
            get => (Visibility)GetValue(EffectiveOverflowButtonVisibilityProperty);
            private set => SetValue(EffectiveOverflowButtonVisibilityPropertyKey, value);
        }

        private static void OnEffectiveOverflowButtonVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SimpleToolBar)d).OnEffectiveOverflowButtonVisibilityChanged();
        }

        private void OnEffectiveOverflowButtonVisibilityChanged()
        {
            InvalidateLayout();
        }

        private void UpdateEffectiveOverflowButtonVisibility()
        {
            bool visible = true;

            switch (OverflowButtonVisibility)
            {
                case CommandBarOverflowButtonVisibility.Auto:
                    visible = HasOverflowItems;
                    break;
                case CommandBarOverflowButtonVisibility.Collapsed:
                    visible = false;
                    break;
            }

            EffectiveOverflowButtonVisibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }

        #endregion

        internal Popup OverflowPopup => m_overflowPopup;

        internal event EventHandler OverflowOpened;

        internal event EventHandler OverflowClosed;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (m_overflowPopup != null)
            {
                m_overflowPopup.ClearValue(Popup.CustomPopupPlacementCallbackProperty);
                m_overflowPopup.Opened -= OnOverflowPopupOpened;
                m_overflowPopup.Closed -= OnOverflowPopupClosed;
            }

            m_layoutRoot = this.GetTemplateRoot();
            m_overflowPopup = GetTemplateChild(OverflowPopupName) as Popup;
            m_toolBarPanel = GetTemplateChild(ToolBarPanelName) as ToolBarPanel;
            m_toolBarOverflowPanel = GetTemplateChild(ToolBarOverflowPanelName) as SimpleToolBarOverflowPanel;

            if (m_overflowPopup != null)
            {
                m_overflowPopup.CustomPopupPlacementCallback = PositionPopup;
                m_overflowPopup.Opened += OnOverflowPopupOpened;
                m_overflowPopup.Closed += OnOverflowPopupClosed;
            }

            InvalidateLayout();
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);

            if (element is AppBarButton ||
                element is AppBarToggleButton)
            {
                var appBarElement = (FrameworkElement)element;
                appBarElement.SetBinding(DefaultLabelPositionProperty, DefaultLabelPositionProperty, this);
            }
        }

        protected override void ClearContainerForItemOverride(DependencyObject element, object item)
        {
            if (element is AppBarButton ||
                element is AppBarToggleButton)
            {
                element.ClearValue(DefaultLabelPositionProperty);
            }

            base.ClearContainerForItemOverride(element, item);
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == HasOverflowItemsProperty)
            {
                UpdateEffectiveOverflowButtonVisibility();
            }
        }

        private void InvalidateLayout()
        {
            var layoutRoot = m_layoutRoot;
            if (layoutRoot != null)
            {
                Dispatcher.BeginInvoke(() =>
                {
                    if (layoutRoot.ActualHeight > 0)
                    {
                        layoutRoot.Height = layoutRoot.ActualHeight;
                        layoutRoot.UpdateLayout();
                        layoutRoot.ClearValue(HeightProperty);
                    }
                });
            }
        }

        private void OnOverflowPopupOpened(object sender, EventArgs e)
        {
            UpdateOverflowContentMaxHeight();
            OverflowOpened?.Invoke(this, EventArgs.Empty);
        }

        private void OnOverflowPopupClosed(object sender, EventArgs e)
        {
            OverflowClosed?.Invoke(this, EventArgs.Empty);
        }

        private CustomPopupPlacement[] PositionPopup(Size popupSize, Size targetSize, Point offset)
        {
            return FlyoutBase.PositionPopup(FlyoutPlacementMode.BottomEdgeAlignedRight, popupSize, targetSize);
        }

        private FrameworkElement m_layoutRoot;
        private Popup m_overflowPopup;
        private ToolBarPanel m_toolBarPanel;
        private SimpleToolBarOverflowPanel m_toolBarOverflowPanel;

        private const string OverflowPopupName = "OverflowPopup";
        private const string ToolBarPanelName = "PART_ToolBarPanel";
        private const string ToolBarOverflowPanelName = "PART_ToolBarOverflowPanel";
    }
}
