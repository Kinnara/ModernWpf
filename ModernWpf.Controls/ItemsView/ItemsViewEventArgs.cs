namespace ModernWpf.Controls
{
    public sealed class ItemsViewItemInvokedEventArgs
    {
        internal ItemsViewItemInvokedEventArgs(object invokedItem)
        {
            InvokedItem = invokedItem;
        }

        public object InvokedItem { get; }
    }

    public sealed class ItemsViewSelectionChangedEventArgs
    {
        internal ItemsViewSelectionChangedEventArgs()
        {
        }
    }

    public enum ItemsViewSelectionMode
    {
        None = 0,
        Single = 1,
        Multiple = 2,
        Extended = 3
    }
}
