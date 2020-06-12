namespace ModernWpf.Controls
{
    internal class SplitViewIsPaneOpenChangedRevoker : EventRevoker<SplitView, DependencyPropertyChangedCallback>
    {
        public SplitViewIsPaneOpenChangedRevoker(SplitView source, DependencyPropertyChangedCallback handler) : base(source, handler)
        {
        }

        protected override void AddHandler(SplitView source, DependencyPropertyChangedCallback handler)
        {
            source.IsPaneOpenChanged += handler;
        }

        protected override void RemoveHandler(SplitView source, DependencyPropertyChangedCallback handler)
        {
            source.IsPaneOpenChanged -= handler;
        }
    }

    internal class SplitViewDisplayModeChangedRevoker : EventRevoker<SplitView, DependencyPropertyChangedCallback>
    {
        public SplitViewDisplayModeChangedRevoker(SplitView source, DependencyPropertyChangedCallback handler) : base(source, handler)
        {
        }

        protected override void AddHandler(SplitView source, DependencyPropertyChangedCallback handler)
        {
            source.DisplayModeChanged += handler;
        }

        protected override void RemoveHandler(SplitView source, DependencyPropertyChangedCallback handler)
        {
            source.DisplayModeChanged -= handler;
        }
    }

    internal class SplitViewCompactPaneLengthChangedRevoker : EventRevoker<SplitView, DependencyPropertyChangedCallback>
    {
        public SplitViewCompactPaneLengthChangedRevoker(SplitView source, DependencyPropertyChangedCallback handler) : base(source, handler)
        {
        }

        protected override void AddHandler(SplitView source, DependencyPropertyChangedCallback handler)
        {
            source.CompactPaneLengthChanged += handler;
        }

        protected override void RemoveHandler(SplitView source, DependencyPropertyChangedCallback handler)
        {
            source.CompactPaneLengthChanged -= handler;
        }
    }
}
