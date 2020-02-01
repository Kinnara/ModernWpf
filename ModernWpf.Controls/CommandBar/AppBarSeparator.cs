using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.Controls
{
    public class AppBarSeparator : Control, ICommandBarElement, IAppBarElement
    {
        static AppBarSeparator()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AppBarSeparator),
                new FrameworkPropertyMetadata(typeof(AppBarSeparator)));

            FocusableProperty.OverrideMetadata(typeof(AppBarSeparator),
                new FrameworkPropertyMetadata(false));

            ToolBar.OverflowModeProperty.OverrideMetadata(typeof(AppBarSeparator),
                new FrameworkPropertyMetadata(OnOverflowModePropertyChanged));
        }

        public AppBarSeparator()
        {
        }

        #region IsCompact

        public static readonly DependencyProperty IsCompactProperty =
            AppBarElementProperties.IsCompactProperty.AddOwner(typeof(AppBarSeparator));

        public bool IsCompact
        {
            get => (bool)GetValue(IsCompactProperty);
            set => SetValue(IsCompactProperty, value);
        }

        #endregion

        #region IsInOverflow

        public static readonly DependencyProperty IsInOverflowProperty =
            AppBarElementProperties.IsInOverflowProperty.AddOwner(typeof(AppBarSeparator));

        public bool IsInOverflow
        {
            get => (bool)GetValue(IsInOverflowProperty);
        }

        #endregion

        #region ApplicationViewState

        public static readonly DependencyProperty ApplicationViewStateProperty =
            AppBarElementProperties.ApplicationViewStateProperty.AddOwner(typeof(AppBarSeparator));

        public AppBarElementApplicationViewState ApplicationViewState
        {
            get => (AppBarElementApplicationViewState)GetValue(ApplicationViewStateProperty);
        }

        void IAppBarElement.UpdateApplicationViewState()
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

            SetValue(AppBarElementProperties.ApplicationViewStatePropertyKey, value);
        }

        #endregion

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == ToolBar.IsOverflowItemProperty)
            {
                AppBarElementProperties.UpdateIsInOverflow(this);
            }
        }

        private static void OnOverflowModePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AppBarElementProperties.UpdateIsInOverflow(d);
        }
    }
}
