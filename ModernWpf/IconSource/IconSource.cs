using System.Windows;
using System.Windows.Media;

namespace ModernWpf.Controls
{
    public class IconSource : DependencyObject
    {
        public static readonly DependencyProperty ForegroundProperty =
            DependencyProperty.Register(
                nameof(Foreground),
                typeof(Brush),
                typeof(IconSource),
                new PropertyMetadata(null));

        public Brush Foreground
        {
            get => (Brush)GetValue(ForegroundProperty);
            set => SetValue(ForegroundProperty, value);
        }
    }
}
