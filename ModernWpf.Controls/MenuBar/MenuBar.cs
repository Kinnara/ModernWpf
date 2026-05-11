using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    [ContentProperty(nameof(Items))]
    public class MenuBar : Menu
    {
        public static readonly DependencyProperty CornerRadiusProperty =
            ControlHelper.CornerRadiusProperty.AddOwner(typeof(MenuBar));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        protected override void OnInitialized(System.EventArgs e)
        {
            base.OnInitialized(e);
            MenuBarStyleHelper.InitializeStyle(this, typeof(Menu));
        }
    }
}
