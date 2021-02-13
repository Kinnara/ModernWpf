using System;

namespace ModernWpf.Controls
{
    public sealed class TeachingTipClosedEventArgs : EventArgs
    {
        internal TeachingTipClosedEventArgs(TeachingTipCloseReason reason)
        {
            Reason = reason;
        }

        public TeachingTipCloseReason Reason { get; }
    }
}
