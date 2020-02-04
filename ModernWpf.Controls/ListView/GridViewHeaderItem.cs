using System.Windows;

namespace ModernWpf.Controls
{
    public class GridViewHeaderItem : ListViewBaseHeaderItem
    {
        static GridViewHeaderItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GridViewHeaderItem), new FrameworkPropertyMetadata(typeof(GridViewHeaderItem)));
        }

        public GridViewHeaderItem()
        {
        }
    }
}
