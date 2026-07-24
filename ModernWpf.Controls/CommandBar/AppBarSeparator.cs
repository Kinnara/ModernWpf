using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.Controls
{
    public partial class AppBarSeparator : Control, ICommandBarElement, IAppBarElement
    {
        static AppBarSeparator()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AppBarSeparator),
                new FrameworkPropertyMetadata(typeof(AppBarSeparator)));

            FocusableProperty.OverrideMetadata(typeof(AppBarSeparator),
                new FrameworkPropertyMetadata(false));

            VisibilityProperty.OverrideMetadata(typeof(AppBarSeparator),
                new FrameworkPropertyMetadata(Visibility.Visible, OnVisibilityChanged));
        }

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

        private static void OnVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((AppBarSeparator)d).OnVisibilityChanged();
        }

        private void OnVisibilityChanged()
        {
            UpdateVisualState();
            CommandBar.OnCommandBarElementVisibilityChanged(this);
        }

        private void UpdateVisualState(bool useTransitions = true)
        {
            VisualStateManager.GoToState(this, GetApplicationViewState(), useTransitions);
        }
    }
}
