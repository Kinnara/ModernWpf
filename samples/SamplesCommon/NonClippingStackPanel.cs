using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SamplesCommon
{
    public class NonClippingStackPanel : StackPanel
    {
        protected override Geometry GetLayoutClip(Size layoutSlotSize)
        {
            return null;
        }
    }
}
