namespace ModernWpf.Controls.Primitives
{
    internal class FlyoutBaseClosingRevoker : EventRevoker<FlyoutBase, TypedEventHandler<FlyoutBase, FlyoutBaseClosingEventArgs>>
    {
        public FlyoutBaseClosingRevoker(FlyoutBase source, TypedEventHandler<FlyoutBase, FlyoutBaseClosingEventArgs> handler) : base(source, handler)
        {
        }

        protected override void AddHandler(FlyoutBase source, TypedEventHandler<FlyoutBase, FlyoutBaseClosingEventArgs> handler)
        {
            source.Closing += handler;
        }

        protected override void RemoveHandler(FlyoutBase source, TypedEventHandler<FlyoutBase, FlyoutBaseClosingEventArgs> handler)
        {
            source.Closing -= handler;
        }
    }
}
