using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
