using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    public class AppBarRepeatButton : RepeatButton, ICommandBarElement, IAppBarElement
    {
        static AppBarRepeatButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AppBarRepeatButton),
                new FrameworkPropertyMetadata(typeof(AppBarRepeatButton)));

            IsEnabledProperty.OverrideMetadata(typeof(AppBarRepeatButton),
                new FrameworkPropertyMetadata(OnIsEnabledChanged));

            CommandProperty.OverrideMetadata(typeof(AppBarRepeatButton),
                new FrameworkPropertyMetadata(OnCommandPropertyChanged));

            ToolTipProperty.OverrideMetadata(typeof(AppBarRepeatButton),
                new FrameworkPropertyMetadata { CoerceValueCallback = AppBarElementProperties.CoerceToolTip });

            ToolBar.OverflowModeProperty.OverrideMetadata(typeof(AppBarRepeatButton),
                new FrameworkPropertyMetadata(OnOverflowModePropertyChanged));

            CommandBarToolBar.DefaultLabelPositionProperty.OverrideMetadata(typeof(AppBarRepeatButton),
                new FrameworkPropertyMetadata(OnDefaultLabelPositionPropertyChanged));

            AppBarElementProperties.IsInOverflowPropertyKey.OverrideMetadata(typeof(AppBarRepeatButton),
                new FrameworkPropertyMetadata(OnIsInOverflowChanged));

            AppBarElementProperties.ShowKeyboardAcceleratorTextProperty.OverrideMetadata(typeof(AppBarRepeatButton),
                new FrameworkPropertyMetadata(OnShowKeyboardAcceleratorTextPropertyChanged));
        }

        public AppBarRepeatButton()
        {
            IsVisibleChanged += OnIsVisibleChanged;
        }

        #region UseSystemFocusVisuals

        public static readonly DependencyProperty UseSystemFocusVisualsProperty =
            FocusVisualHelper.UseSystemFocusVisualsProperty.AddOwner(typeof(AppBarRepeatButton));

        public bool UseSystemFocusVisuals
        {
            get => (bool)GetValue(UseSystemFocusVisualsProperty);
            set => SetValue(UseSystemFocusVisualsProperty, value);
        }

        #endregion

        #region FocusVisualMargin

        public static readonly DependencyProperty FocusVisualMarginProperty =
            FocusVisualHelper.FocusVisualMarginProperty.AddOwner(typeof(AppBarRepeatButton));

        public Thickness FocusVisualMargin
        {
            get => (Thickness)GetValue(FocusVisualMarginProperty);
            set => SetValue(FocusVisualMarginProperty, value);
        }

        #endregion

        #region CornerRadius

        public static readonly DependencyProperty CornerRadiusProperty =
            ControlHelper.CornerRadiusProperty.AddOwner(typeof(AppBarRepeatButton));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        #endregion

        #region Flyout

        public static readonly DependencyProperty FlyoutProperty =
            FlyoutService.FlyoutProperty.AddOwner(
                typeof(AppBarRepeatButton),
                new FrameworkPropertyMetadata(OnFlyoutChanged));

        public FlyoutBase Flyout
        {
            get => (FlyoutBase)GetValue(FlyoutProperty);
            set => SetValue(FlyoutProperty, value);
        }

        private static void OnFlyoutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((AppBarRepeatButton)d).OnFlyoutChanged();
        }

        private void OnFlyoutChanged()
        {
            UpdateVisualState();
        }

        #endregion

        #region Icon

        public static readonly DependencyProperty IconProperty =
            AppBarElementProperties.IconProperty.AddOwner(typeof(AppBarRepeatButton));

        public IconElement Icon
        {
            get => (IconElement)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        #endregion

        #region Label

        public static readonly DependencyProperty LabelProperty =
            AppBarElementProperties.LabelProperty.AddOwner(typeof(AppBarRepeatButton));

        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        #endregion

        #region LabelPosition

        public static readonly DependencyProperty LabelPositionProperty =
            AppBarElementProperties.LabelPositionProperty.AddOwner(typeof(AppBarRepeatButton));

        public CommandBarLabelPosition LabelPosition
        {
            get => (CommandBarLabelPosition)GetValue(LabelPositionProperty);
            set => SetValue(LabelPositionProperty, value);
        }

        #endregion

        #region IsCompact

        public static readonly DependencyProperty IsCompactProperty =
            AppBarElementProperties.IsCompactProperty.AddOwner(typeof(AppBarRepeatButton));

        public bool IsCompact
        {
            get => (bool)GetValue(IsCompactProperty);
            set => SetValue(IsCompactProperty, value);
        }

        #endregion

        #region IsInOverflow

        public static readonly DependencyProperty IsInOverflowProperty =
            AppBarElementProperties.IsInOverflowProperty.AddOwner(typeof(AppBarRepeatButton));

        public bool IsInOverflow
        {
            get => (bool)GetValue(IsInOverflowProperty);
        }

        private static void OnIsInOverflowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var button = (AppBarRepeatButton)d;
            button.UpdateCommonState();
        }

        #endregion

        #region ApplicationViewState

        private static readonly DependencyProperty ApplicationViewStateProperty =
            AppBarElementProperties.ApplicationViewStateProperty.AddOwner(typeof(AppBarRepeatButton));

        private AppBarElementApplicationViewState ApplicationViewState
        {
            get => (AppBarElementApplicationViewState)GetValue(ApplicationViewStateProperty);
        }

        private void UpdateApplicationViewState()
        {
            AppBarElementApplicationViewState value;

            if (IsInOverflow && IsVisible && VisualParent is CommandBarOverflowPanel overflow)
            {
                value = ComputeApplicationViewStateInOverflow(overflow.HasToggleButton, overflow.HasMenuIcon);
            }
            else
            {
                CommandBarDefaultLabelPosition defaultLabelPosition;

                if (VisualParent is ToolBarPanel)
                {
                    defaultLabelPosition = (CommandBarDefaultLabelPosition)GetValue(CommandBarToolBar.DefaultLabelPositionProperty);
                }
                else
                {
                    defaultLabelPosition = CommandBarDefaultLabelPosition.Bottom;
                }

                if (LabelPosition == CommandBarLabelPosition.Collapsed ||
                    defaultLabelPosition == CommandBarDefaultLabelPosition.Collapsed)
                {
                    value = AppBarElementApplicationViewState.LabelCollapsed;
                }
                else if (defaultLabelPosition == CommandBarDefaultLabelPosition.Right)
                {
                    value = AppBarElementApplicationViewState.LabelOnRight;
                }
                else if (IsCompact)
                {
                    value = AppBarElementApplicationViewState.Compact;
                }
                else
                {
                    value = AppBarElementApplicationViewState.FullSize;
                }
            }

            SetValue(AppBarElementProperties.ApplicationViewStatePropertyKey, value);
        }

        private AppBarElementApplicationViewState ComputeApplicationViewStateInOverflow(bool hasToggleButton, bool hasMenuIcon)
        {
            if (hasToggleButton && hasMenuIcon)
            {
                return AppBarElementApplicationViewState.OverflowWithToggleButtonsAndMenuIcons;
            }
            else if (hasToggleButton)
            {
                return AppBarElementApplicationViewState.OverflowWithToggleButtons;
            }
            else if (hasMenuIcon)
            {
                return AppBarElementApplicationViewState.OverflowWithMenuIcons;
            }
            else
            {
                return AppBarElementApplicationViewState.Overflow;
            }
        }

        private void ApplyApplicationViewState(bool useTransitions = true)
        {
            VisualStateManager.GoToState(this, ApplicationViewState.ToString(), useTransitions);
        }

        void IAppBarElement.UpdateApplicationViewState()
        {
            UpdateApplicationViewState();
        }

        void IAppBarElement.ApplyApplicationViewState()
        {
            ApplyApplicationViewState();
        }

        #endregion

        #region InputGestureText

        public static readonly DependencyProperty InputGestureTextProperty =
            AppBarElementProperties.InputGestureTextProperty.AddOwner(typeof(AppBarRepeatButton));

        public string InputGestureText
        {
            get => (string)GetValue(InputGestureTextProperty);
            set => SetValue(InputGestureTextProperty, value);
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

            UpdateVisualState(false);
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
            else if (e.Property == ToolBar.IsOverflowItemProperty)
            {
                AppBarElementProperties.UpdateIsInOverflow(this);
            }
        }

        protected override void OnIsPressedChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnIsPressedChanged(e);
            UpdateCommonState();
        }

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var button = (AppBarRepeatButton)d;
            button.UpdateCommonState();
        }

        private static void OnCommandPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var button = (AppBarRepeatButton)d;
            button.CoerceValue(LabelProperty);
            button.CoerceValue(InputGestureTextProperty);
        }

        private static void OnOverflowModePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AppBarElementProperties.UpdateIsInOverflow(d);
        }

        private static void OnDefaultLabelPositionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((AppBarRepeatButton)d).UpdateApplicationViewState();
        }

        private static void OnShowKeyboardAcceleratorTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((AppBarRepeatButton)d).UpdateKeyboardAcceleratorTextVisibility();
        }

        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UpdateApplicationViewState();
        }

        private void UpdateVisualState(bool useTransitions = true)
        {
            ApplyApplicationViewState(useTransitions);
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

                if (IsInOverflow)
                {
                    stateName = "Overflow" + stateName;
                }
            }

            _vsm.CanChangeCommonState = true;
            VisualStateManager.GoToState(this, stateName, useTransitions);
            _vsm.CanChangeCommonState = false;
        }

        private void UpdateKeyboardAcceleratorTextVisibility(bool useTransitions = true)
        {
            string stateName = AppBarElementProperties.GetShowKeyboardAcceleratorText(this) ?
                "KeyboardAcceleratorTextVisible" :
                "KeyboardAcceleratorTextCollapsed";
            VisualStateManager.GoToState(this, stateName, useTransitions);
        }

        private void UpdateFlyoutState(bool useTransitions = true)
        {
            bool hasFlyout = Flyout != null && !ToolBar.GetIsOverflowItem(this);
            VisualStateManager.GoToState(this, hasFlyout ? "HasFlyout" : "NoFlyout", useTransitions);
        }

        private AppBarElementVisualStateManager _vsm;
    }
}
