using System;

namespace ModernWpf.Controls
{
    public sealed class MessageBoxButtonClickDeferral
    {
        private readonly Action _handler;

        internal MessageBoxButtonClickDeferral(Action handler)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
        }

        public void Complete()
        {
            _handler();
        }
    }
}
