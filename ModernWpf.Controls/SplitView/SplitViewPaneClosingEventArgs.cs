namespace ModernWpf.Controls
{
    public sealed class SplitViewPaneClosingEventArgs
    {
        internal SplitViewPaneClosingEventArgs()
        {
        }

        public bool Cancel { get; set; }
    }
}