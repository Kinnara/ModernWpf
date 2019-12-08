using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    public class AppBarToggleButton : ToggleButton, ICommandBarElement, IAppBarElement
    {
        static AppBarToggleButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AppBarToggleButton),
                new FrameworkPropertyMetadata(typeof(AppBarToggleButton)));

            CommandProperty.OverrideMetadata(typeof(AppBarToggleButton),
                new FrameworkPropertyMetadata(OnCommandPropertyChanged));

            IsCheckedProperty.OverrideMetadata(typeof(AppBarToggleButton),
                new FrameworkPropertyMetadata(OnIsCheckedChanged));

            ToolBar.OverflowModeProperty.OverrideMetadata(typeof(AppBarToggleButton),
                new FrameworkPropertyMetadata(OnOverflowModePropertyChanged));

            CommandBarToolBar.DefaultLabelPositionProperty.OverrideMetadata(typeof(AppBarToggleButton),
                new FrameworkPropertyMetadata(OnDefaultLabelPositionPropertyChanged));
        }

        public AppBarToggleButton()
        {
        }

        #region CornerRadius

        public static readonly DependencyProperty CornerRadiusProperty =
            ControlHelper.CornerRadiusProperty.AddOwner(typeof(AppBarToggleButton));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
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

        #endregion

        #region ApplicationViewState

        public static readonly DependencyProperty ApplicationViewStateProperty =
            AppBarElementProperties.ApplicationViewStateProperty.AddOwner(typeof(AppBarToggleButton));

        public AppBarElementApplicationViewState ApplicationViewState
        {
            get => (AppBarElementApplicationViewState)GetValue(ApplicationViewStateProperty);
        }

        void IAppBarElement.UpdateApplicationViewState()
        {
            UpdateApplicationViewState();
        }

        private void UpdateApplicationViewState()
        {
            AppBarElementApplicationViewState value;

            if (IsInOverflow)
            {
                if (VisualParent is CommandBarOverflowPanel overflow)
                {
                    value = ComputeApplicationViewStateInOverflow(overflow.HasMenuIcon);
                }
                else
                {
                    value = ComputeApplicationViewStateInOverflow(Icon != null);
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

            SetValue(AppBarElementProperties.ApplicationViewStatePropertyKey, value);
        }

        private AppBarElementApplicationViewState ComputeApplicationViewStateInOverflow(bool hasMenuIcon)
        {
            return hasMenuIcon ? AppBarElementApplicationViewState.OverflowWithMenuIcons :
                                 AppBarElementApplicationViewState.Overflow;
        }

        #endregion

        #region IsCheckedOrIndeterminate

        private static readonly DependencyPropertyKey IsCheckedOrIndeterminatePropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(IsCheckedOrIndeterminate),
                typeof(bool),
                typeof(AppBarToggleButton),
                new PropertyMetadata(false));

        public static readonly DependencyProperty IsCheckedOrIndeterminateProperty =
            IsCheckedOrIndeterminatePropertyKey.DependencyProperty;

        public bool IsCheckedOrIndeterminate
        {
            get => (bool)GetValue(IsCheckedOrIndeterminateProperty);
            private set => SetValue(IsCheckedOrIndeterminatePropertyKey, value);
        }

        private void UpdateIsCheckedOrIndeterminate()
        {
            IsCheckedOrIndeterminate = IsChecked != false;
        }

        #endregion

        #region InputGestureText

        public static readonly DependencyProperty InputGestureTextProperty =
            AppBarElementProperties.InputGestureTextProperty.AddOwner(typeof(AppBarToggleButton));

        public string InputGestureText
        {
            get => (string)GetValue(InputGestureTextProperty);
            set => SetValue(InputGestureTextProperty, value);
        }

        #endregion

        #region HasInputGestureText

        public static readonly DependencyProperty HasInputGestureTextProperty =
            AppBarElementProperties.HasInputGestureTextProperty.AddOwner(typeof(AppBarToggleButton));

        public bool HasInputGestureText
        {
            get => (bool)GetValue(HasInputGestureTextProperty);
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
                AppBarElementProperties.UpdateIsInOverflow(this);
            }
        }

        private static void OnCommandPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var button = (AppBarToggleButton)d;
            button.CoerceValue(LabelProperty);
            button.CoerceValue(InputGestureTextProperty);
        }

        private static void OnIsCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((AppBarToggleButton)d).UpdateIsCheckedOrIndeterminate();
        }

        private static void OnOverflowModePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AppBarElementProperties.UpdateIsInOverflow(d);
        }

        private static void OnDefaultLabelPositionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((AppBarToggleButton)d).UpdateApplicationViewState();
        }
    }
}
