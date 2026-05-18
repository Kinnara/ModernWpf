using System;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using ModernWpf.Automation.Peers;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    public partial class AppBarButton : Button, ICommandBarElement, IAppBarButtonElement
    {
        static AppBarButton()
        {
            InputGestureTextProperty = KeyboardAcceleratorTextOverrideProperty;

            DefaultStyleKeyProperty.OverrideMetadata(typeof(AppBarButton),
                new FrameworkPropertyMetadata(typeof(AppBarButton)));

            VisibilityProperty.OverrideMetadata(typeof(AppBarButton),
                new FrameworkPropertyMetadata(Visibility.Visible, OnVisibilityChanged));

            IsEnabledProperty.OverrideMetadata(typeof(AppBarButton),
                new FrameworkPropertyMetadata(OnIsEnabledChanged));

            CommandProperty.OverrideMetadata(typeof(AppBarButton),
                new FrameworkPropertyMetadata(OnCommandPropertyChanged));

            ToolTipProperty.OverrideMetadata(typeof(AppBarButton),
                new FrameworkPropertyMetadata { CoerceValueCallback = AppBarElementProperties.CoerceToolTip });

            WidthProperty.OverrideMetadata(typeof(AppBarButton),
                new FrameworkPropertyMetadata(
                    double.NaN,
                    FrameworkPropertyMetadataOptions.AffectsMeasure,
                    null,
                    CoerceWidthForLabelOnRightStyle));

            AppBarElementProperties.DefaultLabelPositionProperty.OverrideMetadata(typeof(AppBarButton),
                new FrameworkPropertyMetadata(OnDefaultLabelPositionPropertyChanged));

            AppBarElementProperties.IsInOverflowPropertyKey.OverrideMetadata(typeof(AppBarButton),
                new FrameworkPropertyMetadata(OnIsInOverflowChanged));

            AppBarElementProperties.ShowKeyboardAcceleratorTextProperty.OverrideMetadata(typeof(AppBarButton),
                new FrameworkPropertyMetadata(OnShowKeyboardAcceleratorTextPropertyChanged));
        }

        public AppBarButton()
        {
            SetValue(TemplateSettingsPropertyKey, new AppBarButtonTemplateSettings());
        }

        private static void OnFlyoutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((AppBarButton)d).OnFlyoutChanged(e);
        }

        private void OnFlyoutChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is FlyoutBase oldFlyout)
            {
                oldFlyout.Opened -= OnFlyoutOpened;
                oldFlyout.Closed -= OnFlyoutClosed;
            }

            if (e.NewValue is FlyoutBase newFlyout)
            {
                newFlyout.Opened += OnFlyoutOpened;
                newFlyout.Closed += OnFlyoutClosed;
                m_isFlyoutOpen = newFlyout.IsOpen;
            }
            else
            {
                m_isFlyoutOpen = false;
            }

            UpdateCommonState();
            UpdateVisualState();
        }

        private static void OnIsInOverflowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var button = (AppBarButton)d;
            button.UpdateVisualState();
        }

        #region ApplicationViewState

        private void UpdateApplicationViewState()
        {
            CoerceValue(WidthProperty);

            string stateName;

            if (UseOverflowStyle && IsVisible)
            {
                stateName = ComputeOverflowApplicationViewState().ToString();
            }
            else
            {
                stateName = ComputePrimaryApplicationViewState().ToString();
            }

            VisualStateManager.GoToState(this, stateName, true);
        }

        private AppBarElementApplicationViewState ComputeOverflowApplicationViewState()
        {
            if (m_isWithToggleButtons && m_isWithIcons)
            {
                return AppBarElementApplicationViewState.OverflowWithToggleButtonsAndMenuIcons;
            }
            else if (m_isWithToggleButtons)
            {
                return AppBarElementApplicationViewState.OverflowWithToggleButtons;
            }
            else if (m_isWithIcons)
            {
                return AppBarElementApplicationViewState.OverflowWithMenuIcons;
            }
            else
            {
                return AppBarElementApplicationViewState.Overflow;
            }
        }

        private AppBarElementApplicationViewState ComputePrimaryApplicationViewState()
        {
            CommandBarDefaultLabelPosition defaultLabelPosition = GetEffectiveLabelPosition();

            if (defaultLabelPosition == CommandBarDefaultLabelPosition.Collapsed)
            {
                return AppBarElementApplicationViewState.LabelCollapsed;
            }
            else if (defaultLabelPosition == CommandBarDefaultLabelPosition.Right)
            {
                return AppBarElementApplicationViewState.LabelOnRight;
            }
            else if (IsCompact)
            {
                return AppBarElementApplicationViewState.Compact;
            }
            else
            {
                return AppBarElementApplicationViewState.FullSize;
            }
        }

        private CommandBarDefaultLabelPosition GetEffectiveLabelPosition()
        {
            if (LabelPosition == CommandBarLabelPosition.Collapsed)
            {
                return CommandBarDefaultLabelPosition.Collapsed;
            }

            return m_defaultLabelPosition;
        }

        private void ApplyApplicationViewState(bool useTransitions = true)
        {
            string stateName = UseOverflowStyle && IsVisible ?
                ComputeOverflowApplicationViewState().ToString() :
                ComputePrimaryApplicationViewState().ToString();
            VisualStateManager.GoToState(this, stateName, useTransitions);
        }

        void IAppBarElement.UpdateApplicationViewState()
        {
            UpdateApplicationViewState();
        }

        #endregion

        #region InputGestureText

        public static readonly DependencyProperty InputGestureTextProperty;

        public string InputGestureText
        {
            get => KeyboardAcceleratorTextOverride;
            set => KeyboardAcceleratorTextOverride = value;
        }

        #endregion

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (this.GetTemplateRoot() is { } templateRoot)
            {
                _vsm = new AppBarElementVisualStateManager();
                VisualStateManager.SetCustomVisualStateManager(templateRoot, _vsm);
            }

            _keyboardAcceleratorTextLabel = GetTemplateChild("KeyboardAcceleratorTextLabel") as TextBlock;
            UpdateTemplateSettings(m_maxKeyboardAcceleratorTextWidth);
            UpdateVisualState(false);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            m_isPointerFocusSuppressed = true;
            try
            {
                base.OnMouseLeftButtonDown(e);
            }
            finally
            {
                m_isPointerFocusSuppressed = false;
            }
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);

            if (IsInOverflow)
            {
                CommandBar.ClosePeerSubMenusOnPointerEntered(this, this);
            }
        }

        protected override void OnPreviewGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            if (m_isPointerFocusSuppressed && ReferenceEquals(e.NewFocus, this))
            {
                e.Handled = true;
                return;
            }

            base.OnPreviewGotKeyboardFocus(e);
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new AppBarButtonAutomationPeer(this);
        }

        protected override void OnClick()
        {
            if (Flyout == null)
            {
                CommandBar.OnCommandExecutionStatic(this);
            }

            base.OnClick();

            OpenAssociatedFlyout();
        }

        internal void OpenAssociatedFlyout()
        {
            if (Flyout is not { } flyout)
            {
                return;
            }

            if (IsInOverflow)
            {
                flyout.ShowAt(this, CreateOverflowFlyoutShowOptions(GetOverflowFlyoutPosition()));
            }
            else
            {
                flyout.ShowAt(this);
            }
        }

        private Point GetOverflowFlyoutPosition()
        {
            // WinUI gets this point from CascadingMenuHelper; WPF has no equivalent.
            return new Point(Math.Max(0, ActualWidth), 0);
        }

        private FlyoutShowOptions CreateOverflowFlyoutShowOptions(Point position)
        {
            double itemWidth = Math.Max(0, ActualWidth);
            double itemHeight = Math.Max(0, ActualHeight);
            double overlap = itemWidth - position.X;

            return new FlyoutShowOptions
            {
                Placement = FlyoutPlacementMode.RightEdgeAlignedTop,
                ExclusionRect = new Rect(overlap, 0, Math.Max(0, position.X - overlap), itemHeight),
                Position = position
            };
        }

        internal void CloseSubMenuTree()
        {
            if (Flyout?.IsOpen == true)
            {
                Flyout.Hide();
            }
        }

        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);

            UpdateApplicationViewState();
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == IsMouseOverProperty)
            {
                UpdateCommonState();
            }
        }

        protected override void OnIsPressedChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnIsPressedChanged(e);
            UpdateCommonState();
        }

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var button = (AppBarButton)d;
            button.UpdateCommonState();
        }

        private static void OnCommandPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var button = (AppBarButton)d;
            button.CoerceValue(LabelProperty);
            button.CoerceValue(InputGestureTextProperty);
        }

        private static void OnDefaultLabelPositionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((AppBarButton)d).SetDefaultLabelPosition((CommandBarDefaultLabelPosition)e.NewValue);
        }

        private static void OnShowKeyboardAcceleratorTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((AppBarButton)d).UpdateKeyboardAcceleratorTextVisibility();
        }

        private static void OnVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((AppBarButton)d).OnVisibilityChanged();
        }

        private static object CoerceWidthForLabelOnRightStyle(DependencyObject d, object baseValue)
        {
            var button = (AppBarButton)d;
            if (button.ShouldApplyLabelOnRightWidthAdjustment() &&
                button.ReadLocalValue(WidthProperty) == DependencyProperty.UnsetValue)
            {
                return double.NaN;
            }

            return baseValue;
        }

        private bool ShouldApplyLabelOnRightWidthAdjustment()
        {
            return GetEffectiveLabelPosition() == CommandBarDefaultLabelPosition.Right &&
                   !UseOverflowStyle;
        }

        private void OnVisibilityChanged()
        {
            UpdateApplicationViewState();
            CommandBar.OnCommandBarElementVisibilityChanged(this);
        }

        private void UpdateVisualState(bool useTransitions = true)
        {
            ApplyApplicationViewState(useTransitions);
            UpdateInputModeState(useTransitions);
            UpdateCommonState(useTransitions);
            UpdateKeyboardAcceleratorTextVisibility(useTransitions);
            UpdateFlyoutState(useTransitions);
        }

        private void UpdateCommonState(bool useTransitions = true)
        {
            if (_vsm is null)
            {
                return;
            }

            string stateName;

            if (!IsEnabled)
            {
                stateName = "Disabled";
            }
            else if (UseOverflowStyle && m_isWithIcons)
            {
                if (m_isFlyoutOpen)
                {
                    stateName = "OverflowSubMenuOpened";
                }
                else if (IsPressed)
                {
                    stateName = "OverflowPressed";
                }
                else if (IsMouseOver)
                {
                    stateName = "OverflowPointerOver";
                }
                else
                {
                    stateName = "OverflowNormal";
                }
            }
            else
            {
                if (IsPressed)
                {
                    stateName = "Pressed";
                }
                else if (IsMouseOver)
                {
                    stateName = "PointerOver";
                }
                else
                {
                    stateName = "Normal";
                }

            }

            _vsm.CanChangeCommonState = true;
            VisualStateManager.GoToState(this, stateName, useTransitions);
            _vsm.CanChangeCommonState = false;
        }

        private void OnFlyoutOpened(object sender, object e)
        {
            m_isFlyoutOpen = true;
            UpdateCommonState();
        }

        private void OnFlyoutClosed(object sender, object e)
        {
            m_isFlyoutOpen = false;
            UpdateCommonState();
        }

        private void UpdateKeyboardAcceleratorTextVisibility(bool useTransitions = true)
        {
            string stateName = m_isWithKeyboardAcceleratorText && UseOverflowStyle ?
                "KeyboardAcceleratorTextVisible" :
                "KeyboardAcceleratorTextCollapsed";
            VisualStateManager.GoToState(this, stateName, useTransitions);
        }

        private void UpdateInputModeState(bool useTransitions = true)
        {
            VisualStateManager.GoToState(this, "InputModeDefault", useTransitions);

            string stateName = m_inputMode switch
            {
                AppBarButtonInputMode.Touch => "TouchInputMode",
                AppBarButtonInputMode.GameController => "GameControllerInputMode",
                _ => null
            };

            if (stateName != null)
            {
                VisualStateManager.GoToState(this, stateName, useTransitions);
            }
        }

        private void UpdateFlyoutState(bool useTransitions = true)
        {
            bool hasFlyout = Flyout != null;
            VisualStateManager.GoToState(this, hasFlyout ? "HasFlyout" : "NoFlyout", useTransitions);
        }

        private bool UseOverflowStyle => AppBarElementProperties.GetUseOverflowStyle(this);

        internal void SetOverflowStyleParams(bool hasIcons, bool hasToggleButtons, bool hasKeyboardAcceleratorText)
        {
            bool updateState = false;

            if (m_isWithIcons != hasIcons)
            {
                m_isWithIcons = hasIcons;
                updateState = true;
            }

            if (m_isWithToggleButtons != hasToggleButtons)
            {
                m_isWithToggleButtons = hasToggleButtons;
                updateState = true;
            }

            if (m_isWithKeyboardAcceleratorText != hasKeyboardAcceleratorText)
            {
                m_isWithKeyboardAcceleratorText = hasKeyboardAcceleratorText;
                updateState = true;
            }

            if (updateState)
            {
                UpdateVisualState();
            }
        }

        void IAppBarButtonElement.SetOverflowStyleParams(bool hasIcons, bool hasToggleButtons, bool hasKeyboardAcceleratorText)
        {
            SetOverflowStyleParams(hasIcons, hasToggleButtons, hasKeyboardAcceleratorText);
        }

        private void SetInputMode(AppBarButtonInputMode inputMode)
        {
            if (m_inputMode != inputMode)
            {
                m_inputMode = inputMode;
                UpdateInputModeState();
            }
        }

        void IAppBarButtonElement.SetInputMode(AppBarButtonInputMode inputMode)
        {
            SetInputMode(inputMode);
        }

        private void SetDefaultLabelPosition(CommandBarDefaultLabelPosition defaultLabelPosition)
        {
            if (m_defaultLabelPosition != defaultLabelPosition)
            {
                m_defaultLabelPosition = defaultLabelPosition;
                UpdateInternalStyles();
            }
        }

        void IAppBarButtonElement.SetDefaultLabelPosition(CommandBarDefaultLabelPosition defaultLabelPosition)
        {
            SetDefaultLabelPosition(defaultLabelPosition);
        }

        bool IAppBarButtonElement.GetHasBottomLabel()
        {
            return GetHasLabelAtPosition(CommandBarDefaultLabelPosition.Bottom);
        }

        bool IAppBarButtonElement.GetHasRightLabel()
        {
            return GetHasLabelAtPosition(CommandBarDefaultLabelPosition.Right);
        }

        private bool GetHasLabelAtPosition(CommandBarDefaultLabelPosition labelPosition)
        {
            return GetEffectiveLabelPosition() == labelPosition &&
                   Label != null;
        }

        double IAppBarButtonElement.GetKeyboardAcceleratorTextDesiredWidth()
        {
            if (_keyboardAcceleratorTextLabel == null)
            {
                return 0;
            }

            _keyboardAcceleratorTextLabel.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Thickness margin = _keyboardAcceleratorTextLabel.Margin;
            return Math.Max(0, _keyboardAcceleratorTextLabel.DesiredSize.Width - margin.Left - margin.Right);
        }

        void IAppBarButtonElement.UpdateTemplateSettings(double maxKeyboardAcceleratorTextWidth)
        {
            UpdateTemplateSettings(maxKeyboardAcceleratorTextWidth);
        }

        private void UpdateTemplateSettings(double maxKeyboardAcceleratorTextWidth)
        {
            m_maxKeyboardAcceleratorTextWidth = maxKeyboardAcceleratorTextWidth;

            if (TemplateSettings != null)
            {
                TemplateSettings.KeyboardAcceleratorTextMinWidth = maxKeyboardAcceleratorTextWidth;
            }
        }

        private void UpdateInternalStyles()
        {
            UpdateApplicationViewState();
            CoerceValue(ToolTipProperty);
            UpdateVisualState();
        }

        private AppBarElementVisualStateManager _vsm;
        private TextBlock _keyboardAcceleratorTextLabel;
        private bool m_isFlyoutOpen;
        private bool m_isWithIcons;
        private bool m_isWithToggleButtons;
        private bool m_isWithKeyboardAcceleratorText;
        private bool m_isPointerFocusSuppressed;
        private AppBarButtonInputMode m_inputMode;
        private double m_maxKeyboardAcceleratorTextWidth;
        private CommandBarDefaultLabelPosition m_defaultLabelPosition = CommandBarDefaultLabelPosition.Bottom;
    }
}
