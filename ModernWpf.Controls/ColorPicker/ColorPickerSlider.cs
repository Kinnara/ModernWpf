using System.Windows;
using System.Windows.Controls;
using ModernWpf.Controls;

namespace ModernWpf.Controls.Primitives
{
    public class ColorPickerSlider : Slider
    {
        public static readonly DependencyProperty ColorChannelProperty =
            DependencyProperty.Register(
                nameof(ColorChannel),
                typeof(ColorPickerHsvChannel),
                typeof(ColorPickerSlider),
                new PropertyMetadata(ColorPickerHsvChannel.Value));

        public ColorPickerHsvChannel ColorChannel
        {
            get => (ColorPickerHsvChannel)GetValue(ColorChannelProperty);
            set => SetValue(ColorChannelProperty, value);
        }
    }
}
