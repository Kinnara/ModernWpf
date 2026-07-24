using ModernWpf.Controls;
using System.Windows;
using System.Windows.Media;

namespace SamplesCommon
{
    public class NonClippingStackPanel : StackPanelEx
    {
        protected override Geometry GetLayoutClip(Size layoutSlotSize)
        {
            return null;
        }
    }
}
