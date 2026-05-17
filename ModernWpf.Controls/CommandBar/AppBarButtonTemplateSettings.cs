using System.Windows;

namespace ModernWpf.Controls.Primitives
{
    public sealed class AppBarButtonTemplateSettings : DependencyObject
    {
        internal AppBarButtonTemplateSettings()
        {
        }

        public static readonly DependencyProperty KeyboardAcceleratorTextMinWidthProperty =
            DependencyProperty.Register(
                nameof(KeyboardAcceleratorTextMinWidth),
                typeof(double),
                typeof(AppBarButtonTemplateSettings),
                new PropertyMetadata(0.0));

        public double KeyboardAcceleratorTextMinWidth
        {
            get => (double)GetValue(KeyboardAcceleratorTextMinWidthProperty);
            internal set => SetValue(KeyboardAcceleratorTextMinWidthProperty, value);
        }
    }
}
