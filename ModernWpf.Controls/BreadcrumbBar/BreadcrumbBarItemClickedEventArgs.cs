namespace ModernWpf.Controls
{
    public sealed class BreadcrumbBarItemClickedEventArgs
    {
        internal BreadcrumbBarItemClickedEventArgs(object item, int index)
        {
            Item = item;
            Index = index;
        }

        public int Index { get; }

        public object Item { get; }
    }
}
