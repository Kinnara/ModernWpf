namespace ModernWpf.Controls
{
    public sealed class AnnotatedScrollBarScrollingEventArgs
    {
        internal AnnotatedScrollBarScrollingEventArgs(
            double scrollOffset,
            AnnotatedScrollBarScrollingEventKind scrollingEventKind)
        {
            ScrollOffset = scrollOffset;
            ScrollingEventKind = scrollingEventKind;
        }

        public double ScrollOffset { get; }

        public AnnotatedScrollBarScrollingEventKind ScrollingEventKind { get; }

        public bool Cancel { get; set; }
    }
}
