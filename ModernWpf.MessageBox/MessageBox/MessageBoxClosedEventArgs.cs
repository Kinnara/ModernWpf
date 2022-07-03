using System;
using System.Windows;

namespace ModernWpf.Controls
{
    public class MessageBoxClosedEventArgs : EventArgs
    {
        internal MessageBoxClosedEventArgs(MessageBoxResult result)
        {
            Result = result;
        }

        public MessageBoxResult Result { get; }
    }
}
