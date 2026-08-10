using System.Windows;
using System.Windows.Media;

namespace ModernWpf.Controls
{
    public class TabViewItemTemplateSettings : DependencyObject
    {
        public static readonly DependencyProperty IconElementProperty =
            DependencyProperty.Register(
                nameof(IconElement),
                typeof(IconElement),
                typeof(TabViewItemTemplateSettings),
                new PropertyMetadata(null));

        public static readonly DependencyProperty TabGeometryProperty =
            DependencyProperty.Register(
                nameof(TabGeometry),
                typeof(Geometry),
                typeof(TabViewItemTemplateSettings),
                new PropertyMetadata(null));

        public IconElement IconElement
        {
            get => (IconElement)GetValue(IconElementProperty);
            set => SetValue(IconElementProperty, value);
        }

        public Geometry TabGeometry
        {
            get => (Geometry)GetValue(TabGeometryProperty);
            set => SetValue(TabGeometryProperty, value);
        }
    }
}
