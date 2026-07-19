using System;

namespace ModernWpf.Controls
{
    public enum CommandBarDynamicOverflowAction
    {
        AddingToOverflow = 0,
        RemovingFromOverflow = 1
    }

    public sealed class DynamicOverflowItemsChangingEventArgs : EventArgs
    {
        public CommandBarDynamicOverflowAction Action { get; internal set; }
    }

    public delegate void DynamicOverflowItemsChangingEventHandler(
        object sender,
        DynamicOverflowItemsChangingEventArgs e);
}
