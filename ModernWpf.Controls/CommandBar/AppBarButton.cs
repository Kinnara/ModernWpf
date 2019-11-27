using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    public class AppBarButton : Button, ICommandBarElement
    {
        static AppBarButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AppBarButton),
                new FrameworkPropertyMetadata(typeof(AppBarButton)));

            ToolBar.OverflowModeProperty.OverrideMetadata(typeof(AppBarButton),
                new FrameworkPropertyMetadata(OnOverflowModePropertyChanged));

            CommandBarToolBar.DefaultLabelPositionProperty.OverrideMetadata(typeof(AppBarButton),
                new FrameworkPropertyMetadata(OnDefaultLabelPositionPropertyChanged));
        }

        public AppBarButton()
        {
            UpdateIsInOverflow();
            UpdateApplicationViewState();
        }

        #region CornerRadius

        public static readonly DependencyProperty CornerRadiusProperty =
            ControlHelper.CornerRadiusProperty.AddOwner(typeof(AppBarButton));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        #endregion

        #region Flyout

        public static readonly DependencyProperty FlyoutProperty =
            FlyoutService.FlyoutProperty.AddOwner(
                typeof(AppBarButton),
                new FrameworkPropertyMetadata(OnFlyoutChanged));

        public FlyoutBase Flyout
        {
            get => (FlyoutBase)GetValue(FlyoutProperty);
            set => SetValue(FlyoutProperty, value);
        }

        private static void OnFlyoutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((AppBarButton)d).OnFlyoutChanged();
        }

        private void OnFlyoutChanged()
        {
            //UpdateHasFlyout();
        }

        #endregion

        #region Icon

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register(
                nameof(Icon),
                typeof(IconElement),
                typeof(AppBarButton),
                new PropertyMetadata(OnIconChanged));

        [TypeConverter(typeof(IconElementConverter))]
        public IconElement Icon
        {
            get => (IconElement)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        private static void OnIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((AppBarButton)d).UpdateApplicationViewState();
        }

        #endregion

        #region Label

        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register(
                nameof(Label),
                typeof(string),
                typeof(AppBarButton),
                new PropertyMetadata(string.Empty));

        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        #endregion

        #region LabelPosition

        public static readonly DependencyProperty LabelPositionProperty =
            DependencyProperty.Register(
                nameof(LabelPosition),
                typeof(CommandBarLabelPosition),
                typeof(AppBarButton),
                new PropertyMetadata(CommandBarLabelPosition.Default, OnLabelPositionChanged));

        public CommandBarLabelPosition LabelPosition
        {
            get => (CommandBarLabelPosition)GetValue(LabelPositionProperty);
            set => SetValue(LabelPositionProperty, value);
        }

        private static void OnLabelPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((AppBarButton)d).UpdateApplicationViewState();
        }

        #endregion

        #region IsCompact

        public static readonly DependencyProperty IsCompactProperty =
            DependencyProperty.Register(
                nameof(IsCompact),
                typeof(bool),
                typeof(AppBarButton),
                new PropertyMetadata(false, OnIsCompactChanged));

        public bool IsCompact
        {
            get => (bool)GetValue(IsCompactProperty);
            set => SetValue(IsCompactProperty, value);
        }

        private static void OnIsCompactChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((AppBarButton)d).UpdateApplicationViewState();
        }

        #endregion

        #region IsInOverflow

        private static readonly DependencyPropertyKey IsInOverflowPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(IsInOverflow),
                typeof(bool),
                typeof(AppBarButton),
                new PropertyMetadata(false, OnIsInOverflowChanged));

        public static readonly DependencyProperty IsInOverflowProperty =
            IsInOverflowPropertyKey.DependencyProperty;

        public bool IsInOverflow
        {
            get => (bool)GetValue(IsInOverflowProperty);
            private set => SetValue(IsInOverflowPropertyKey, value);
        }

        private static void OnIsInOverflowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((AppBarButton)d).UpdateApplicationViewState();
        }

        private void UpdateIsInOverflow()
        {
            IsInOverflow = ToolBar.GetIsOverflowItem(this) || ToolBar.GetOverflowMode(this) == OverflowMode.Always;
        }

        #endregion

        #region ApplicationViewState

        private static readonly DependencyPropertyKey ApplicationViewStatePropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(ApplicationViewState),
                typeof(AppBarElementApplicationViewState),
                typeof(AppBarButton),
                new PropertyMetadata(AppBarElementApplicationViewState.FullSize));

        public static readonly DependencyProperty ApplicationViewStateProperty =
            ApplicationViewStatePropertyKey.DependencyProperty;

        public AppBarElementApplicationViewState ApplicationViewState
        {
            get => (AppBarElementApplicationViewState)GetValue(ApplicationViewStateProperty);
            private set => SetValue(ApplicationViewStatePropertyKey, value);
        }

        internal void UpdateApplicationViewStateInOverflow(bool hasToggleButton, bool hasMenuIcon)
        {
            ApplicationViewState = CalculateApplicationViewStateInOverflow(hasToggleButton, hasMenuIcon);
        }

        private AppBarElementApplicationViewState CalculateApplicationViewStateInOverflow(bool hasToggleButton, bool hasMenuIcon)
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

        private void UpdateApplicationViewState()
        {
            AppBarElementApplicationViewState value;

            if (IsInOverflow)
            {
                switch (ApplicationViewState)
                {
                    case AppBarElementApplicationViewState.FullSize:
                    case AppBarElementApplicationViewState.Compact:
                    case AppBarElementApplicationViewState.LabelOnRight:
                    case AppBarElementApplicationViewState.LabelCollapsed:
                        value = CalculateApplicationViewStateInOverflow(false, Icon != null);
                        break;
                    default:
                        return;
                }
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

            ApplicationViewState = value;
        }

        #endregion

        //#region HasFlyout

        //private static readonly DependencyPropertyKey HasFlyoutPropertyKey =
        //    DependencyProperty.RegisterReadOnly(
        //        nameof(HasFlyout),
        //        typeof(bool),
        //        typeof(AppBarButton),
        //        new PropertyMetadata(false));

        //public static readonly DependencyProperty HasFlyoutProperty =
        //    HasFlyoutPropertyKey.DependencyProperty;

        //public bool HasFlyout
        //{
        //    get => (bool)GetValue(HasFlyoutProperty);
        //    private set => SetValue(HasFlyoutPropertyKey, value);
        //}

        //private void UpdateHasFlyout()
        //{
        //    HasFlyout = Flyout != null;
        //}

        //#endregion

        #region InputGestureText

        public static readonly DependencyProperty InputGestureTextProperty =
            DependencyProperty.Register(
                nameof(InputGestureText),
                typeof(string),
                typeof(AppBarButton),
                new PropertyMetadata(string.Empty, OnInputGestureTextChanged));

        public string InputGestureText
        {
            get => (string)GetValue(InputGestureTextProperty);
            set => SetValue(InputGestureTextProperty, value);
        }

        private static void OnInputGestureTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((AppBarButton)d).UpdateHasInputGestureText();
        }

        #endregion

        #region HasInputGestureText

        private static readonly DependencyPropertyKey HasInputGestureTextPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(HasInputGestureText),
                typeof(bool),
                typeof(AppBarButton),
                new PropertyMetadata(false));

        public static readonly DependencyProperty HasInputGestureTextProperty =
            HasInputGestureTextPropertyKey.DependencyProperty;

        public bool HasInputGestureText
        {
            get => (bool)GetValue(HasInputGestureTextProperty);
            private set => SetValue(HasInputGestureTextPropertyKey, value);
        }

        private void UpdateHasInputGestureText()
        {
            HasInputGestureText = !string.IsNullOrEmpty(InputGestureText);
        }

        #endregion

        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);

            UpdateApplicationViewState();
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == ToolBar.IsOverflowItemProperty)
            {
                UpdateIsInOverflow();
            }
        }

        private static void OnOverflowModePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((AppBarButton)d).UpdateIsInOverflow();
        }

        private static void OnDefaultLabelPositionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((AppBarButton)d).UpdateApplicationViewState();
        }
    }
}
