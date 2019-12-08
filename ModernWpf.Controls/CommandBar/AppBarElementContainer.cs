using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.Controls
{
    public class AppBarElementContainer : ContentControl, ICommandBarElement
    {
        static AppBarElementContainer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AppBarElementContainer),
                new FrameworkPropertyMetadata(typeof(AppBarElementContainer)));

            ToolBar.OverflowModeProperty.OverrideMetadata(typeof(AppBarElementContainer),
                new FrameworkPropertyMetadata(OnOverflowModePropertyChanged));
        }

        public AppBarElementContainer()
        {
        }

        #region IsCompact

        public static readonly DependencyProperty IsCompactProperty =
            AppBarElementProperties.IsCompactProperty.AddOwner(typeof(AppBarElementContainer));

        public bool IsCompact
        {
            get => (bool)GetValue(IsCompactProperty);
            set => SetValue(IsCompactProperty, value);
        }

        #endregion

        #region IsInOverflow

        public static readonly DependencyProperty IsInOverflowProperty =
            AppBarElementProperties.IsInOverflowProperty.AddOwner(typeof(AppBarElementContainer));

        public bool IsInOverflow
        {
            get => (bool)GetValue(IsInOverflowProperty);
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
