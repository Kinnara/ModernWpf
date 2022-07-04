using System.Windows.Controls;

namespace ModernWpf.Controls
{
    public class DateTimeComponentSelectorPanel : VirtualizingStackPanel
    {
        public override void MouseWheelUp()
        {
            LineUp();
        }

        public override void MouseWheelDown()
        {
            LineDown();
        }
    }
}
