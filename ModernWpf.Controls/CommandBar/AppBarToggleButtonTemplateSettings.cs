using System.Windows;

namespace ModernWpf.Controls.Primitives
{
    public sealed class AppBarToggleButtonTemplateSettings : DependencyObject
    {
        internal AppBarToggleButtonTemplateSettings()
        {
        }

        public static readonly DependencyProperty KeyboardAcceleratorTextMinWidthProperty =
            DependencyProperty.Register(
                nameof(KeyboardAcceleratorTextMinWidth),
                typeof(double),
                typeof(AppBarToggleButtonTemplateSettings),
                new PropertyMetadata(0.0));

        public double KeyboardAcceleratorTextMinWidth
        {
            get => (double)GetValue(KeyboardAcceleratorTextMinWidthProperty);
            internal set => SetValue(KeyboardAcceleratorTextMinWidthProperty, value);
        }
    }
}
