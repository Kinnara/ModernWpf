using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    [ContentProperty(nameof(Items))]
    public class MenuBarItem : MenuItem
    {
        public static readonly DependencyProperty CornerRadiusProperty =
            ControlHelper.CornerRadiusProperty.AddOwner(typeof(MenuBarItem));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

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

        protected override void OnInitialized(System.EventArgs e)
        {
            base.OnInitialized(e);
            MenuBarStyleHelper.InitializeStyle(this, typeof(MenuItem));
        }

        private static void OnTitlePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((MenuBarItem)d).Header = e.NewValue;
        }
    }
}
