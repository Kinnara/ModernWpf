using System.Windows.Controls;
using System.Windows.Input;

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

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                LineUp();
                e.Handled = true;
            }
            else if (e.Key == Key.Down)
            {
                LineDown();
                e.Handled = true;
            }

            base.OnKeyDown(e);
        }
    }
}
