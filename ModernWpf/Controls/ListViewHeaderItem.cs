using System.Windows;

namespace ModernWpf.Controls
{
    /// <summary>
    /// Represents items in the header for grouped data inside a ListView.
    /// </summary>
    public class ListViewHeaderItem : ListViewBaseHeaderItem
    {
        static ListViewHeaderItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ListViewHeaderItem), new FrameworkPropertyMetadata(typeof(ListViewHeaderItem)));
        }

        /// <summary>
        /// Initializes a new instance of the ListViewHeaderItem class.
        /// </summary>
        public ListViewHeaderItem()
        {
        }
    }
}
