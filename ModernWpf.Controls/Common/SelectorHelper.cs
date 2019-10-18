using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.Controls.Primitives
{
    internal static class SelectorHelper
    {
        internal static bool ItemGetIsSelectable(object item)
        {
            if (item != null)
            {
                return !(item is Separator);
            }

            return false;
        }

        internal static bool UiGetIsSelectable(DependencyObject o)
        {
            if (o != null)
            {
                if (!ItemGetIsSelectable(o))
                {
                    return false;
                }
                else
                {
                    // Check the data item
                    ItemsControl itemsControl = ItemsControl.ItemsControlFromItemContainer(o);
                    if (itemsControl != null)
                    {
                        object data = itemsControl.ItemContainerGenerator.ItemFromContainer(o);
                        if (data != o)
                        {
                            return ItemGetIsSelectable(data);
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}
