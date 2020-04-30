namespace System.Windows.Controls
{
    public sealed class ScrollViewerViewChangedEventArgs
    {
        public ScrollViewerViewChangedEventArgs()
        {
        }

        public bool IsIntermediate { get; }
    }

    public class ScrollViewerEx : ScrollViewer
    {
        public event EventHandler<ScrollViewerViewChangedEventArgs> ViewChanged;

        protected override void OnScrollChanged(ScrollChangedEventArgs e)
        {
            base.OnScrollChanged(e);

            if (e.HorizontalChange != 0 || e.VerticalChange != 0)
            {
                ViewChanged?.Invoke(this, new ScrollViewerViewChangedEventArgs());
            }
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            this.InitializeStyle(DefaultStyleKey);
        }
    }
}
