using System;

namespace ModernWpf.Controls
{
    public class InfoBarClosedEventArgs : EventArgs
    {
        internal InfoBarClosedEventArgs(InfoBarCloseReason reason)
        {
            Reason = reason;
        }

        public InfoBarCloseReason Reason { get; }
    }
}
