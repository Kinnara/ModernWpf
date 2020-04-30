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

    public class MenuFlyoutSeparator : Separator
    {
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            this.InitializeStyle(DefaultStyleKey);
        }
    }
}
