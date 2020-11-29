using System;

namespace ModernWpf
{
    internal abstract class EventRevoker<TSource, TDelegate>
        where TSource : class
        where TDelegate : Delegate
    {
        private WeakReference<TSource> _source;
        private WeakReference<TDelegate> _handler;

        protected EventRevoker(TSource source, TDelegate handler)
        {
            _source = new WeakReference<TSource>(source);
            _handler = new WeakReference<TDelegate>(handler);
            AddHandler(source, handler);
        }

        protected abstract void AddHandler(TSource source, TDelegate handler);
        protected abstract void RemoveHandler(TSource source, TDelegate handler);

        public void Revoke()
        {
            if (_source != null && _handler != null &&
                _source.TryGetTarget(out var source) &&
                _handler.TryGetTarget(out var handler))
            {
                RemoveHandler(source, handler);
            }

            _source = null;
            _handler = null;
        }
    }
}
