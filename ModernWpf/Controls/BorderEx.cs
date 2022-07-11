using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ModernWpf.Controls
{
    public class BorderEx : Border
    {
        protected override Geometry GetLayoutClip(Size layoutSlotSize)
        {
            if (ClipToBounds)
            {
                var radius = CornerRadius.TopLeft;
                var rect = new RectangleGeometry(new Rect(layoutSlotSize), radius, radius);
                rect.Freeze();
                return rect;
            }

            return base.GetLayoutClip(layoutSlotSize);
        }
    }
}
