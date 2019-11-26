using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    public class AppBarToggleButton : ToggleButton, ICommandBarElement
    {
        static AppBarToggleButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AppBarToggleButton),
                new FrameworkPropertyMetadata(typeof(AppBarToggleButton)));

            IsCheckedProperty.OverrideMetadata(typeof(AppBarToggleButton),
                new FrameworkPropertyMetadata(OnIsCheckedChanged));

            ToolBar.OverflowModeProperty.OverrideMetadata(typeof(AppBarToggleButton),
                new FrameworkPropertyMetadata(OnOverflowModePropertyChanged));

            CommandBarToolBar.DefaultLabelPositionProperty.OverrideMetadata(typeof(AppBarToggleButton),
                new FrameworkPropertyMetadata(OnDefaultLabelPositionPropertyChanged));
        }

        public AppBarToggleButton()
        {
            UpdateIsInOverflow();
            UpdateIsCheckedOrIndeterminate();
            UpdateApplicationViewState();
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
            DependencyProperty.Register(
                nameof(Icon),
                typeof(IconElement),
                typeof(AppBarToggleButton),
                new PropertyMetadata(OnIconChanged));

        [TypeConverter(typeof(IconElementConverter))]
        public IconElement Icon
        {
            get => (IconElement)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        private static void OnIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((AppBarToggleButton)d).UpdateApplicationViewState();
        }

        #endregion

        #region Label

        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register(
                nameof(Label),
                typeof(string),
                typeof(AppBarToggleButton),
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
                typeof(AppBarToggleButton),
                new PropertyMetadata(CommandBarLabelPosition.Default, OnLabelPositionChanged));

        public CommandBarLabelPosition LabelPosition
        {
            get => (CommandBarLabelPosition)GetValue(LabelPositionProperty);
            set => SetValue(LabelPositionProperty, value);
        }

        private static void OnLabelPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((AppBarToggleButton)d).UpdateApplicationViewState();
        }

        #endregion

        #region IsCompact

        public static readonly DependencyProperty IsCompactProperty =
            DependencyProperty.Register(
                nameof(IsCompact),
                typeof(bool),
                typeof(AppBarToggleButton),
                new PropertyMetadata(false, OnIsCompactChanged));

        public bool IsCompact
        {
            get => (bool)GetValue(IsCompactProperty);
            set => SetValue(IsCompactProperty, value);
        }

        private static void OnIsCompactChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((AppBarToggleButton)d).UpdateApplicationViewState();
        }

        #endregion

        #region IsInOverflow

        private static readonly DependencyPropertyKey IsInOverflowPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(IsInOverflow),
                typeof(bool),
                typeof(AppBarToggleButton),
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
            ((AppBarToggleButton)d).UpdateApplicationViewState();
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
                typeof(AppBarToggleButton),
                new PropertyMetadata(AppBarElementApplicationViewState.FullSize));

        public static readonly DependencyProperty ApplicationViewStateProperty =
            ApplicationViewStatePropertyKey.DependencyProperty;

        public AppBarElementApplicationViewState ApplicationViewState
        {
            get => (AppBarElementApplicationViewState)GetValue(ApplicationViewStateProperty);
            private set => SetValue(ApplicationViewStatePropertyKey, value);
        }

        internal void UpdateApplicationViewStateInOverflow(bool hasMenuIcon)
        {
            ApplicationViewState = CalculateApplicationViewStateInOverflow(hasMenuIcon);
        }

        private AppBarElementApplicationViewState CalculateApplicationViewStateInOverflow(bool hasMenuIcon)
        {
            AppBarElementApplicationViewState value;

            if (hasMenuIcon)
            {
                value = AppBarElementApplicationViewState.OverflowWithMenuIcons;
            }
            else
            {
                value = AppBarElementApplicationViewState.Overflow;
            }

            return value;
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
                        value = CalculateApplicationViewStateInOverflow(Icon != null);
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

        private static void OnIsCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((AppBarToggleButton)d).UpdateIsCheckedOrIndeterminate();
        }

        private static void OnOverflowModePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((AppBarToggleButton)d).UpdateIsInOverflow();
        }

        private static void OnDefaultLabelPositionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((AppBarToggleButton)d).UpdateApplicationViewState();
        }
    }
}
