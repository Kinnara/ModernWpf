namespace System.Windows.Controls
{
    internal static class Extensions
    {
        public static void InitializeStyle(this FrameworkElement fe, object key)
        {
            if (fe.Style == null && fe.ReadLocalValue(FrameworkElement.StyleProperty) == DependencyProperty.UnsetValue)
            {
                fe.SetResourceReference(FrameworkElement.StyleProperty, key);
            }
        }
    }
}
