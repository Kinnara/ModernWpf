using ModernWpf.Controls;

namespace System.Windows.Controls
{
    public class MenuFlyout : ModernWpf.Controls.MenuFlyout { }

    public class MenuFlyoutItem : MenuItem
    {
        #region Text

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
                nameof(Text),
                typeof(string),
                typeof(MenuFlyoutItem),
                new PropertyMetadata(string.Empty, OnTextPropertyChanged));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        private static void OnTextPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((MenuFlyoutItem)sender).OnTextPropertyChanged(args);
        }

        private void OnTextPropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            Header = (string)args.NewValue;
        }

        #endregion

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            this.InitializeStyle(DefaultStyleKey);
        }
    }

    public class ToggleMenuFlyoutItem : MenuFlyoutItem
    {
        static ToggleMenuFlyoutItem()
        {
            IsCheckableProperty.OverrideMetadata(typeof(ToggleMenuFlyoutItem), new FrameworkPropertyMetadata(true, null, CoerceIsCheckable));
        }

        private static object CoerceIsCheckable(DependencyObject d, object baseValue)
        {
            return true;
        }
    }

    public class RadioMenuFlyoutItem : RadioMenuItem
    {
        #region Text

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
                nameof(Text),
                typeof(string),
                typeof(RadioMenuFlyoutItem),
                new PropertyMetadata(string.Empty, OnTextPropertyChanged));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        private static void OnTextPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((RadioMenuFlyoutItem)sender).OnTextPropertyChanged(args);
        }

        private void OnTextPropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            Header = (string)args.NewValue;
        }

        #endregion

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            this.InitializeStyle(DefaultStyleKey);
        }
    }

    public class MenuFlyoutSeparator : Separator
    {
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            this.InitializeStyle(DefaultStyleKey);
        }
    }
}
