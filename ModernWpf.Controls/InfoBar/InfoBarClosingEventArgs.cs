using System;

namespace ModernWpf.Controls
{
    public class InfoBarClosingEventArgs : EventArgs
    {
        internal InfoBarClosingEventArgs(InfoBarCloseReason reason)
        {
            Reason = reason;
        }

        public InfoBarCloseReason Reason { get; }

        public bool Cancel { get; set; }
    }
}
