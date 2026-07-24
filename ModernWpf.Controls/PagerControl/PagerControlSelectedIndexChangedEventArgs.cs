namespace ModernWpf.Controls
{
    public sealed class PagerControlSelectedIndexChangedEventArgs
    {
        internal PagerControlSelectedIndexChangedEventArgs(int previousPageIndex, int newPageIndex)
        {
            PreviousPageIndex = previousPageIndex;
            NewPageIndex = newPageIndex;
        }

        public int NewPageIndex { get; }

        public int PreviousPageIndex { get; }
    }
}
