using System.Windows;

namespace ModernWpf.Controls
{
    public class InfoBarTemplateSettings : DependencyObject
    {
        public static readonly DependencyProperty IconElementProperty =
            DependencyProperty.Register(
                nameof(IconElement),
                typeof(IconElement),
                typeof(InfoBarTemplateSettings));

        public IconElement IconElement
        {
            get => (IconElement)GetValue(IconElementProperty);
            internal set => SetValue(IconElementProperty, value);
        }
    }
}
