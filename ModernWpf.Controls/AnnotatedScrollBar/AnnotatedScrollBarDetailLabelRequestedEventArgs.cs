namespace ModernWpf.Controls
{
    public sealed class AnnotatedScrollBarDetailLabelRequestedEventArgs
    {
        internal AnnotatedScrollBarDetailLabelRequestedEventArgs(double scrollOffset)
        {
            ScrollOffset = scrollOffset;
        }

        public object Content { get; set; }

        public double ScrollOffset { get; }
    }
}
