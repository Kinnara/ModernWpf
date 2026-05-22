using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace ModernWpf.Gallery.Controls
{
    [ContentProperty(nameof(ExampleContent))]
    public class ColorPageExample : UserControl
    {
        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register(nameof(Description), typeof(string), typeof(ColorPageExample), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty ExampleContentProperty =
            DependencyProperty.Register(nameof(ExampleContent), typeof(UIElement), typeof(ColorPageExample), new PropertyMetadata(null));

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(ColorPageExample), new PropertyMetadata(string.Empty));

        public string Description
        {
            get { return (string)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        public UIElement ExampleContent
        {
            get { return (UIElement)GetValue(ExampleContentProperty); }
            set { SetValue(ExampleContentProperty, value); }
        }

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
    }
}
