using System.Windows.Controls;

namespace ModernWpf.MahApps.Controls
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
