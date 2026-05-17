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
        }

        public AppBarSeparator()
        {
            IsVisibleChanged += OnIsVisibleChanged;
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

        private string GetApplicationViewState()
        {
            if (AppBarElementProperties.GetUseOverflowStyle(this))
            {
                return nameof(AppBarElementApplicationViewState.Overflow);
            }
            else if (IsCompact)
            {
                return nameof(AppBarElementApplicationViewState.Compact);
            }
            else
            {
                return nameof(AppBarElementApplicationViewState.FullSize);
            }
        }

        void IAppBarElement.UpdateApplicationViewState()
        {
            UpdateVisualState();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            UpdateVisualState(false);
        }

        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UpdateVisualState();
        }

        private void UpdateVisualState(bool useTransitions = true)
        {
            VisualStateManager.GoToState(this, GetApplicationViewState(), useTransitions);
        }
    }
}
