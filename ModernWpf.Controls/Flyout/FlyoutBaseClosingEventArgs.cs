using System;

namespace ModernWpf.Controls.Primitives
{
    public sealed class FlyoutBaseClosingEventArgs : EventArgs
    {
        internal FlyoutBaseClosingEventArgs()
        {
        }

        public bool Cancel { get; set; }
    }
}
