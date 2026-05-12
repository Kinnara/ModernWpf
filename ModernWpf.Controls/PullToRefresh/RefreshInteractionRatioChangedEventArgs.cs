using System;

namespace ModernWpf.Controls
{
    public sealed class RefreshInteractionRatioChangedEventArgs : EventArgs
    {
        public RefreshInteractionRatioChangedEventArgs(double interactionRatio)
        {
            InteractionRatio = interactionRatio;
        }

        public double InteractionRatio { get; }
    }
}
