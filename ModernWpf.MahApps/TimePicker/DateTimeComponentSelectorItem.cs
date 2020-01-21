using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.MahApps.Controls
{
    public class DateTimeComponentSelectorItem : ListBoxItem
    {
        static DateTimeComponentSelectorItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DateTimeComponentSelectorItem), new FrameworkPropertyMetadata(typeof(DateTimeComponentSelectorItem)));
        }
    }
}
