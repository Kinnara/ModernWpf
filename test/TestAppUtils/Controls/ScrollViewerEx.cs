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

        public bool ChangeView(
            double? horizontalOffset,
            double? verticalOffset,
            float? zoomFactor)
        {
            return ChangeView(horizontalOffset, verticalOffset, zoomFactor, false);
        }

        public bool ChangeView(
            double? horizontalOffset,
            double? verticalOffset,
            float? zoomFactor,
            bool disableAnimation)
        {
            bool changed = false;

            if (horizontalOffset.HasValue)
            {
                if (horizontalOffset.Value != HorizontalOffset)
                {
                    changed = true;
                }

                ScrollToHorizontalOffset(horizontalOffset.Value);
            }

            if (verticalOffset.HasValue)
            {
                if (verticalOffset.Value != VerticalOffset)
                {
                    changed = true;
                }

                ScrollToVerticalOffset(verticalOffset.Value);
            }

            return changed;
        }

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
