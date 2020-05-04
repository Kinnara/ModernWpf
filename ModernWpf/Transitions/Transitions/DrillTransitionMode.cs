using System;
using System.ComponentModel;

namespace ModernWpf.Controls
{
    [Obsolete]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public enum DrillTransitionMode
    {
        DrillInIncoming,
        DrillInOutgoing,
        DrillOutIncoming,
        DrillOutOutgoing
    }
}
