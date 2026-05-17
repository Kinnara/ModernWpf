using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    public class AppBarToggleButton : ToggleButton, ICommandBarElement, IAppBarButtonElement
    {
        static AppBarToggleButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AppBarToggleButton),
                new FrameworkPropertyMetadata(typeof(AppBarToggleButton)));

            IsEnabledProperty.OverrideMetadata(typeof(AppBarToggleButton),
                new FrameworkPropertyMetadata(OnIsEnabledChanged));

            CommandProperty.OverrideMetadata(typeof(AppBarToggleButton),
                new FrameworkPropertyMetadata(OnCommandPropertyChanged));

            IsCheckedProperty.OverrideMetadata(typeof(AppBarToggleButton),
                new FrameworkPropertyMetadata(OnIsCheckedChanged));

            ToolTipProperty.OverrideMetadata(typeof(AppBarToggleButton),
                new FrameworkPropertyMetadata { CoerceValueCallback = AppBarElementProperties.CoerceToolTip });

            AppBarElementProperties.DefaultLabelPositionProperty.OverrideMetadata(typeof(AppBarToggleButton),
                new FrameworkPropertyMetadata(OnDefaultLabelPositionPropertyChanged));

            AppBarElementProperties.IsInOverflowPropertyKey.OverrideMetadata(typeof(AppBarToggleButton),
                new FrameworkPropertyMetadata(OnIsInOverflowChanged));

            AppBarElementProperties.ShowKeyboardAcceleratorTextProperty.OverrideMetadata(typeof(AppBarToggleButton),
                new FrameworkPropertyMetadata(OnShowKeyboardAcceleratorTextPropertyChanged));
        }

        public AppBarToggleButton()
        {
            SetValue(TemplateSettingsPropertyKey, new AppBarToggleButtonTemplateSettings());
            IsVisibleChanged += OnIsVisibleChanged;
        }

        #region UseSystemFocusVisuals

        public static readonly DependencyProperty UseSystemFocusVisualsProperty =
            FocusVisualHelper.UseSystemFocusVisualsProperty.AddOwner(typeof(AppBarToggleButton));

        public bool UseSystemFocusVisuals
        {
            get => (bool)GetValue(UseSystemFocusVisualsProperty);
            set => SetValue(UseSystemFocusVisualsProperty, value);
        }

        #endregion

        #region FocusVisualMargin

        public static readonly DependencyProperty FocusVisualMarginProperty =
            FocusVisualHelper.FocusVisualMarginProperty.AddOwner(typeof(AppBarToggleButton));

        public Thickness FocusVisualMargin
        {
            get => (Thickness)GetValue(FocusVisualMarginProperty);
            set => SetValue(FocusVisualMarginProperty, value);
        }

        #endregion

        #region CornerRadius

        public static readonly DependencyProperty CornerRadiusProperty =
            ControlHelper.CornerRadiusProperty.AddOwner(typeof(AppBarToggleButton));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        #endregion

        #region BackgroundSizing

        public static readonly DependencyProperty BackgroundSizingProperty =
            ControlHelper.BackgroundSizingProperty.AddOwner(typeof(AppBarToggleButton));

        public BackgroundSizing BackgroundSizing
        {
            get => (BackgroundSizing)GetValue(BackgroundSizingProperty);
            set => SetValue(BackgroundSizingProperty, value);
        }

        #endregion

        #region Icon

        public static readonly DependencyProperty IconProperty =
            AppBarElementProperties.IconProperty.AddOwner(typeof(AppBarToggleButton));

        public IconElement Icon
        {
            get => (IconElement)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        #endregion

        #region Label

        public static readonly DependencyProperty LabelProperty =
            AppBarElementProperties.LabelProperty.AddOwner(typeof(AppBarToggleButton));

        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        #endregion

        #region LabelPosition

        public static readonly DependencyProperty LabelPositionProperty =
            AppBarElementProperties.LabelPositionProperty.AddOwner(typeof(AppBarToggleButton));

        public CommandBarLabelPosition LabelPosition
        {
            get => (CommandBarLabelPosition)GetValue(LabelPositionProperty);
            set => SetValue(LabelPositionProperty, value);
        }

        #endregion

        #region IsCompact

        public static readonly DependencyProperty IsCompactProperty =
            AppBarElementProperties.IsCompactProperty.AddOwner(typeof(AppBarToggleButton));

        public bool IsCompact
        {
            get => (bool)GetValue(IsCompactProperty);
            set => SetValue(IsCompactProperty, value);
        }

        #endregion

        #region IsInOverflow

        public static readonly DependencyProperty IsInOverflowProperty =
            AppBarElementProperties.IsInOverflowProperty.AddOwner(typeof(AppBarToggleButton));

        public bool IsInOverflow
        {
            get => (bool)GetValue(IsInOverflowProperty);
        }

        private static void OnIsInOverflowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var button = (AppBarToggleButton)d;
            button.UpdateVisualState();
        }

        #endregion

        #region ApplicationViewState

        private void UpdateApplicationViewState()
        {
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
            return m_isWithIcons ? AppBarElementApplicationViewState.OverflowWithMenuIcons :
                                   AppBarElementApplicationViewState.Overflow;
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

            if (ReadLocalValue(AppBarElementProperties.DefaultLabelPositionProperty) != DependencyProperty.UnsetValue)
            {
                return (CommandBarDefaultLabelPosition)GetValue(AppBarElementProperties.DefaultLabelPositionProperty);
            }

            return CommandBarDefaultLabelPosition.Bottom;
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

        public static readonly DependencyProperty KeyboardAcceleratorTextOverrideProperty =
            AppBarElementProperties.KeyboardAcceleratorTextOverrideProperty.AddOwner(typeof(AppBarToggleButton));

        public static readonly DependencyProperty InputGestureTextProperty =
            KeyboardAcceleratorTextOverrideProperty;

        public string InputGestureText
        {
            get => KeyboardAcceleratorTextOverride;
            set => KeyboardAcceleratorTextOverride = value;
        }

        public string KeyboardAcceleratorTextOverride
        {
            get => (string)GetValue(KeyboardAcceleratorTextOverrideProperty);
            set => SetValue(KeyboardAcceleratorTextOverrideProperty, value);
        }

        #endregion

        #region TemplateSettings

        private static readonly DependencyPropertyKey TemplateSettingsPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(TemplateSettings),
                typeof(AppBarToggleButtonTemplateSettings),
                typeof(AppBarToggleButton),
                null);

        public static readonly DependencyProperty TemplateSettingsProperty =
            TemplateSettingsPropertyKey.DependencyProperty;

        public AppBarToggleButtonTemplateSettings TemplateSettings =>
            (AppBarToggleButtonTemplateSettings)GetValue(TemplateSettingsProperty);

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
            var button = (AppBarToggleButton)d;
            button.UpdateCommonState();
        }

        private static void OnCommandPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var button = (AppBarToggleButton)d;
            button.CoerceValue(LabelProperty);
            button.CoerceValue(InputGestureTextProperty);
        }

        private static void OnIsCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var button = (AppBarToggleButton)d;
            button.UpdateCommonState();
        }

        private static void OnDefaultLabelPositionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((AppBarToggleButton)d).UpdateInternalStyles();
        }

        private static void OnShowKeyboardAcceleratorTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((AppBarToggleButton)d).UpdateKeyboardAcceleratorTextVisibility();
        }

        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UpdateApplicationViewState();
        }

        private void UpdateVisualState(bool useTransitions = true)
        {
            ApplyApplicationViewState(useTransitions);
            UpdateInputModeState(useTransitions);
            UpdateCommonState(useTransitions);
            UpdateKeyboardAcceleratorTextVisibility(useTransitions);
        }

        private void UpdateCommonState(bool useTransitions = true)
        {
            if (_vsm is null)
            {
                return;
            }

            string stateName;
            bool isEnabled = IsEnabled;
            bool isChecked = IsChecked != false;

            if (!isEnabled)
            {
                stateName = "Disabled";
            }
            else if (UseOverflowStyle)
            {
                if (isChecked)
                {
                    if (IsPressed)
                    {
                        stateName = "OverflowCheckedPressed";
                    }
                    else if (IsMouseOver)
                    {
                        stateName = "OverflowCheckedPointerOver";
                    }
                    else
                    {
                        stateName = "OverflowChecked";
                    }
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
            else if (IsPressed)
            {
                stateName = "Pressed";
            }
            else if (IsMouseOver)
            {
                stateName = "PointerOver";
            }
            else if (!isChecked)
            {
                stateName = "Normal";
            }
            else
            {
                stateName = string.Empty;
            }

            if (isChecked && (!isEnabled || !UseOverflowStyle))
            {
                stateName = "Checked" + stateName;
            }

            _vsm.CanChangeCommonState = true;
            VisualStateManager.GoToState(this, stateName, useTransitions);
            _vsm.CanChangeCommonState = false;
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
        private bool m_isWithIcons;
        private bool m_isWithKeyboardAcceleratorText;
        private double m_maxKeyboardAcceleratorTextWidth;
    }
}
