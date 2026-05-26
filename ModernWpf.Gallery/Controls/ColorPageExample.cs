using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace ModernWpf.Gallery.Controls
{
    [ContentProperty(nameof(ExampleContent))]
    public class ColorPageExample : UserControl
    {
        public string Description
        {
            get { return (string)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register(nameof(Description), typeof(string), typeof(ColorPageExample), new PropertyMetadata(string.Empty));

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(ColorPageExample), new PropertyMetadata(string.Empty));

        public UIElement ExampleContent
        {
            get { return (UIElement)GetValue(ExampleContentProperty); }
            set { SetValue(ExampleContentProperty, value); }
        }

        public static readonly DependencyProperty ExampleContentProperty =
            DependencyProperty.Register(nameof(ExampleContent), typeof(UIElement), typeof(ColorPageExample), new PropertyMetadata(null));
    }
}
