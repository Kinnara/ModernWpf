using System;
using System.Windows;
using System.Windows.Controls;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    public class GridView : ListViewBase
    {
        static GridView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GridView), new FrameworkPropertyMetadata(typeof(GridView)));
        }

        public GridView()
        {
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is GridViewItem;
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new GridViewItem();
        }
    }
}
