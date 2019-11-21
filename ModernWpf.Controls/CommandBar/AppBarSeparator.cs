using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.Controls
{
    public class AppBarSeparator : Control, ICommandBarElement
    {
        static AppBarSeparator()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AppBarSeparator),
                new FrameworkPropertyMetadata(typeof(AppBarSeparator)));

            ToolBar.OverflowModeProperty.OverrideMetadata(typeof(AppBarSeparator),
                new FrameworkPropertyMetadata(OnOverflowModePropertyChanged));
        }

        public AppBarSeparator()
        {
            this.SetBinding(PrivateIsOverflowItemProperty, ToolBar.IsOverflowItemProperty, this);
            UpdateApplicationViewState();
        }

        #region IsCompact

        public static readonly DependencyProperty IsCompactProperty =
            DependencyProperty.Register(
                nameof(IsCompact),
                typeof(bool),
                typeof(AppBarSeparator),
                new PropertyMetadata(false, OnIsCompactChanged));

        public bool IsCompact
        {
            get => (bool)GetValue(IsCompactProperty);
            set => SetValue(IsCompactProperty, value);
        }

        private static void OnIsCompactChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((AppBarSeparator)d).UpdateApplicationViewState();
        }

        #endregion

        #region IsInOverflow

        private static readonly DependencyPropertyKey IsInOverflowPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(IsInOverflow),
                typeof(bool),
                typeof(AppBarSeparator),
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
            ((AppBarSeparator)d).UpdateApplicationViewState();
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
                typeof(AppBarSeparator),
                new PropertyMetadata(AppBarElementApplicationViewState.FullSize));

        public static readonly DependencyProperty ApplicationViewStateProperty =
            ApplicationViewStatePropertyKey.DependencyProperty;

        public AppBarElementApplicationViewState ApplicationViewState
        {
            get => (AppBarElementApplicationViewState)GetValue(ApplicationViewStateProperty);
            private set => SetValue(ApplicationViewStatePropertyKey, value);
        }

        private void UpdateApplicationViewState()
        {
            AppBarElementApplicationViewState value;

            if (IsInOverflow)
            {
                value = AppBarElementApplicationViewState.Overflow;
            }
            else if (IsCompact)
            {
                value = AppBarElementApplicationViewState.Compact;
            }
            else
            {
                value = AppBarElementApplicationViewState.FullSize;
            }

            ApplicationViewState = value;
        }

        #endregion

        #region PrivateIsOverflowItem

        private static readonly DependencyProperty PrivateIsOverflowItemProperty =
            DependencyProperty.Register(
                "PrivateIsOverflowItem",
                typeof(bool),
                typeof(AppBarSeparator),
                new PropertyMetadata(false, OnPrivateIsOverflowItemChanged));

        private static void OnPrivateIsOverflowItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((AppBarSeparator)d).UpdateIsInOverflow();
        }

        #endregion

        private static void OnOverflowModePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((AppBarSeparator)d).UpdateIsInOverflow();
        }
    }
}
