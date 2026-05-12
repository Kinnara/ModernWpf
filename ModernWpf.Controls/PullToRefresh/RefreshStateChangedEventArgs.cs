using System;

namespace ModernWpf.Controls
{
    public sealed class RefreshStateChangedEventArgs : EventArgs
    {
        internal RefreshStateChangedEventArgs(RefreshVisualizerState oldState, RefreshVisualizerState newState)
        {
            OldState = oldState;
            NewState = newState;
        }

        public RefreshVisualizerState OldState { get; }

        public RefreshVisualizerState NewState { get; }
    }
}
