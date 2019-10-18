using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.Controls
{
    internal static class UIElementCollectionExtensions
    {
        public static bool IndexOf(this UIElementCollection collection, UIElement value, out int index)
        {
            int i = collection.IndexOf(value);
            if (i >= 0)
            {
                index = i;
                return true;
            }
            else
            {
                index = 0;
                return false;
            }
        }
    }
}
