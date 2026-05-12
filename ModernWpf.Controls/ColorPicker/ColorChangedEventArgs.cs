using System;
using System.Windows.Media;

namespace ModernWpf.Controls
{
    public sealed class ColorChangedEventArgs : EventArgs
    {
        internal ColorChangedEventArgs(Color oldColor, Color newColor)
        {
            OldColor = oldColor;
            NewColor = newColor;
        }

        public Color OldColor { get; }

        public Color NewColor { get; }
    }
}
