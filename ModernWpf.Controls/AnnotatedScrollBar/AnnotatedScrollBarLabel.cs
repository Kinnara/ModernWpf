namespace ModernWpf.Controls
{
    public sealed class AnnotatedScrollBarLabel
    {
        public AnnotatedScrollBarLabel(object content, double scrollOffset)
        {
            Content = content;
            ScrollOffset = scrollOffset;
        }

        public object Content { get; }

        public double ScrollOffset { get; }

        public override string ToString()
        {
            return Content?.ToString() ?? string.Empty;
        }
    }
}
