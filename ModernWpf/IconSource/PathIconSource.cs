using System.Windows;
using System.Windows.Media;

namespace ModernWpf.Controls
{
    public class PathIconSource : IconSource
    {
        public PathIconSource()
        {
        }

        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register(
                nameof(Data),
                typeof(Geometry),
                typeof(PathIconSource),
                new PropertyMetadata(null));

        public Geometry Data
        {
            get => (Geometry)GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }
    }
}
