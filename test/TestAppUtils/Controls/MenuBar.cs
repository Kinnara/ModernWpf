using ModernWpf.Controls.Primitives;

namespace System.Windows.Controls
{
    public class MenuBar : Menu
    {
        #region CornerRadius

        public static readonly DependencyProperty CornerRadiusProperty = ControlHelper.CornerRadiusProperty.AddOwner(typeof(MenuBar));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        #endregion

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            this.InitializeStyle(DefaultStyleKey);
        }
    }

    public class MenuBarItem : MenuItem
    {
        #region Title

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
                nameof(Title),
                typeof(string),
                typeof(MenuBarItem),
                new PropertyMetadata(string.Empty, OnTitlePropertyChanged));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        private static void OnTitlePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((MenuBarItem)sender).OnTitlePropertyChanged(args);
        }

        private void OnTitlePropertyChanged(DependencyPropertyChangedEventArgs args)
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
}
