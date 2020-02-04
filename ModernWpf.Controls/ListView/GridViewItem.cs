using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    public class GridViewItem : ListViewBaseItem
    {
        static GridViewItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GridViewItem), new FrameworkPropertyMetadata(typeof(GridViewItem)));
        }
    }
}
