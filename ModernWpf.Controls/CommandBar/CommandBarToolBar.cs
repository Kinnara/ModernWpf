using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;

namespace ModernWpf.Controls.Primitives
{
    [TemplatePart(Name = OverflowPopupName, Type = typeof(Popup))]
    public class CommandBarToolBar : ToolBar
    {
        static CommandBarToolBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CommandBarToolBar), new FrameworkPropertyMetadata(typeof(CommandBarToolBar)));

            IsOverflowOpenProperty.OverrideMetadata(typeof(CommandBarToolBar), new FrameworkPropertyMetadata(OnIsOverflowOpenChanged));

            KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(typeof(CommandBarToolBar),
                new FrameworkPropertyMetadata(KeyboardNavigationMode.Contained));
            KeyboardNavigation.TabNavigationProperty.OverrideMetadata(typeof(CommandBarToolBar),
                new FrameworkPropertyMetadata(KeyboardNavigationMode.Continue));
        }

        public CommandBarToolBar()
        {
            m_moreButtonClosedToolTip = new ToolTip { Content = Strings.AppBarMoreButtonClosedToolTip };
            m_moreButtonOpenToolTip = new ToolTip { Content = Strings.AppBarMoreButtonOpenToolTip };
        }

        #region CornerRadius

        public static readonly DependencyProperty CornerRadiusProperty =
            ControlHelper.CornerRadiusProperty.AddOwner(typeof(CommandBarToolBar));

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
                typeof(CommandBarToolBar),
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
                typeof(CommandBarToolBar),
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
                typeof(CommandBarToolBar),
                new PropertyMetadata(CommandBarOverflowButtonVisibility.Auto, OnOverflowButtonVisibilityChanged));

        public CommandBarOverflowButtonVisibility OverflowButtonVisibility
        {
            get => (CommandBarOverflowButtonVisibility)GetValue(OverflowButtonVisibilityProperty);
            set => SetValue(OverflowButtonVisibilityProperty, value);
        }

        private static void OnOverflowButtonVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CommandBarToolBar)d).UpdateEffectiveOverflowButtonVisibility();
        }

        #endregion

        #region OverflowPresenterStyle

        public static readonly DependencyProperty OverflowPresenterStyleProperty =
            DependencyProperty.Register(
                nameof(OverflowPresenterStyle),
                typeof(Style),
                typeof(CommandBarToolBar),
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
                typeof(CommandBarToolBar),
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
                typeof(CommandBarToolBar),
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
            ((CommandBarToolBar)d).OnEffectiveOverflowButtonVisibilityChanged();
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

        internal bool HasPrimaryCommands => m_toolBarPanel != null && m_toolBarPanel.HasChildren;

        internal event EventHandler OverflowOpened;

        internal event EventHandler OverflowClosed;

        internal event EventHandler HasPrimaryCommandsChanged;

        internal event EventHandler HasOverflowItemsChanged;

        public override void OnApplyTemplate()
        {
            if (m_moreButton != null)
            {
                m_moreButton.ClearValue(ToolTipProperty);
            }

            if (m_overflowPopup != null)
            {
                m_overflowPopup.ClearValue(Popup.CustomPopupPlacementCallbackProperty);
                m_overflowPopup.ClearValue(CustomPopupPlacementHelper.PlacementProperty);
                m_overflowPopup.Opened -= OnOverflowPopupOpened;
                m_overflowPopup.Closed -= OnOverflowPopupClosed;
            }

            if (m_toolBarPanel != null)
            {
                m_toolBarPanel.HasChildrenChanged -= OnToolBarPanelHasChildrenChanged;
            }

            base.OnApplyTemplate();

            m_layoutRoot = this.GetTemplateRoot();
            m_moreButton = GetTemplateChild("MoreButton") as ButtonBase;
            m_overflowPopup = GetTemplateChild(OverflowPopupName) as Popup;
            m_toolBarPanel = GetTemplateChild(ToolBarPanelName) as CommandBarPanel;
            m_toolBarOverflowPanel = GetTemplateChild(ToolBarOverflowPanelName) as CommandBarOverflowPanel;

            if (m_moreButton != null)
            {
                AutomationProperties.SetName(m_moreButton, Strings.AppBarMoreButtonName);
                UpdateMoreButtonTooTip();
            }

            if (m_overflowPopup != null)
            {
                m_overflowPopup.CustomPopupPlacementCallback = PositionOverflowPopup;
                m_overflowPopup.SetValue(CustomPopupPlacementHelper.PlacementProperty, CustomPlacementMode.BottomEdgeAlignedRight);
                m_overflowPopup.Opened += OnOverflowPopupOpened;
                m_overflowPopup.Closed += OnOverflowPopupClosed;
            }

            if (m_toolBarPanel != null)
            {
                m_toolBarPanel.HasChildrenChanged += OnToolBarPanelHasChildrenChanged;
            }

            if (TemplatedParent is CommandBar commandBar)
            {
                commandBar.UpdateVisualState(false);
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
                HasOverflowItemsChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    {
                        // If focus is within ToolBarOverflowPanel - move focus the the toggle button
                        ToolBarOverflowPanel overflow = m_toolBarOverflowPanel;
                        if (overflow != null && overflow.IsKeyboardFocusWithin)
                        {
                            MoveFocus(new TraversalRequest(FocusNavigationDirection.Last));
                        }
                        else
                        {
                            Keyboard.Focus(null);
                        }

                        // Close the overflow the Esc is pressed
                        SetCurrentValue(IsOverflowOpenProperty, false);
                    }
                    break;
            }

            base.OnKeyDown(e);
        }

        private static void OnIsOverflowOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CommandBarToolBar)d).UpdateMoreButtonTooTip();
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

        private void OnToolBarPanelHasChildrenChanged(object sender, EventArgs e)
        {
            HasPrimaryCommandsChanged?.Invoke(this, e);
        }

        private CustomPopupPlacement[] PositionOverflowPopup(Size popupSize, Size targetSize, Point offset)
        {
            return CustomPopupPlacementHelper.PositionPopup(
                CustomPlacementMode.BottomEdgeAlignedRight,
                popupSize,
                targetSize,
                offset,
                child: m_overflowPopup.Child as FrameworkElement);
        }

        private void UpdateMoreButtonTooTip()
        {
            if (m_moreButton != null)
            {
                m_moreButton.ToolTip = IsOverflowOpen ? m_moreButtonOpenToolTip : m_moreButtonClosedToolTip;
            }
        }

        private FrameworkElement m_layoutRoot;
        private ButtonBase m_moreButton;
        private Popup m_overflowPopup;
        private CommandBarPanel m_toolBarPanel;
        private CommandBarOverflowPanel m_toolBarOverflowPanel;

        private readonly ToolTip m_moreButtonClosedToolTip;
        private readonly ToolTip m_moreButtonOpenToolTip;

        private const string OverflowPopupName = "OverflowPopup";
        private const string ToolBarPanelName = "PART_ToolBarPanel";
        private const string ToolBarOverflowPanelName = "PART_ToolBarOverflowPanel";
    }
}
