using MahApps.Metro.Controls;
using ModernWpf.MahApps.Controls;
using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.MahApps.Controls
{
    public class HamburgerMenuItemStyleSelectorEx : HamburgerMenuItemStyleSelector
    {
        public override Style SelectStyle(object item, DependencyObject container)
        {
            if (container != null)
            {
                var listBox = ItemsControl.ItemsControlFromItemContainer(container);
                var hamburgerMenu = listBox?.TryFindParent<HamburgerMenuEx>();
                if (hamburgerMenu != null)
                {
                    if (item is HamburgerMenuHeaderItem)
                    {
                        if (hamburgerMenu.HeaderItemContainerStyle != null)
                        {
                            return hamburgerMenu.HeaderItemContainerStyle;
                        }
                    }
                    else if (item is IHamburgerMenuSeparatorItem)
                    {
                        if (hamburgerMenu.SeparatorItemContainerStyle != null)
                        {
                            return hamburgerMenu.SeparatorItemContainerStyle;
                        }
                    }
                    else
                    {
                        var itemContainerStyle = this.IsItemOptions ? hamburgerMenu.OptionsItemContainerStyle : hamburgerMenu.ItemContainerStyle;
                        if (itemContainerStyle != null)
                        {
                            return itemContainerStyle;
                        }
                    }
                }
            }

            return base.SelectStyle(item, container);
        }
    }
}
