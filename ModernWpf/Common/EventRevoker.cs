using System;

namespace ModernWpf
{
    internal abstract class EventRevoker<TSource, TDelegate>
        where TSource : class
        where TDelegate : Delegate
    {
        private readonly WeakReference<TSource> _source;
        private readonly WeakReference<TDelegate> _handler;

        public EventRevoker(TSource source, TDelegate handler)
        {
            _source = new WeakReference<TSource>(source);
            AddHandler(source, handler);
        }

        protected abstract void AddHandler(TSource source, TDelegate handler);
        protected abstract void RemoveHandler(TSource source, TDelegate handler);

        public void Revoke()
        {
            if (_source.TryGetTarget(out var source) &&
                _handler.TryGetTarget(out var handler))
            {
                RemoveHandler(source, handler);
            }
        }
    }
}
