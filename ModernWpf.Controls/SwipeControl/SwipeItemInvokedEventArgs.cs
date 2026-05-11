namespace ModernWpf.Controls
{
    public sealed class SwipeItemInvokedEventArgs
    {
        internal SwipeItemInvokedEventArgs(SwipeControl swipeControl)
        {
            SwipeControl = swipeControl;
        }

        public SwipeControl SwipeControl { get; }
    }
}
