using System.Windows;

namespace ModernWpf.Controls
{
    public class TitleBarTemplateSettings : DependencyObject
    {
        public static readonly DependencyProperty IconElementProperty =
            DependencyProperty.Register(
                nameof(IconElement),
                typeof(IconElement),
                typeof(TitleBarTemplateSettings),
                new PropertyMetadata(null));

        public IconElement IconElement
        {
            get => (IconElement)GetValue(IconElementProperty);
            internal set => SetValue(IconElementProperty, value);
        }
    }
}
