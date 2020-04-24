using System;

namespace ModernWpf.Controls
{
    internal sealed class FlyoutBaseClosingEventArgs
    {
        internal FlyoutBaseClosingEventArgs()
        {
        }

        public bool Cancel
        {
            get => false;
            set => throw new NotImplementedException();
        }
    }
}
