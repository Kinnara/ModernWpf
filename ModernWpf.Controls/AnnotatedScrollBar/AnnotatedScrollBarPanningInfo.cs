using System.Windows;
using System.Windows.Controls;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    internal sealed class AnnotatedScrollBarPanningInfo : IScrollControllerPanningInfo
    {
        public bool IsRailEnabled => true;

        public Orientation PanOrientation => Orientation.Vertical;

        public UIElement PanningElementAncestor { get; private set; }

        public event TypedEventHandler<IScrollControllerPanningInfo, object> Changed;

        public event TypedEventHandler<IScrollControllerPanningInfo, ScrollControllerPanRequestedEventArgs> PanRequested;

        public void SetPanningElementExpressionAnimationSources(
            object propertySet,
            string minOffsetPropertyName,
            string maxOffsetPropertyName,
            string offsetPropertyName,
            string multiplierPropertyName)
        {
            // WinUI uses composition expression animations here. WPF updates the
            // thumb position from AnnotatedScrollBar.SetValues instead.
        }

        internal void PanningFrameworkElement(FrameworkElement value)
        {
            PanningElement = value;
        }

        internal void SetPanningElementAncestor(UIElement value)
        {
            if (!ReferenceEquals(PanningElementAncestor, value))
            {
                PanningElementAncestor = value;
                Changed?.Invoke(this, null);
            }
        }

        internal void PanningElementOffsetMultiplier(float value)
        {
            PanningElementOffsetMultiplierValue = value;
        }

        internal bool RaisePanRequested(object pointerPoint)
        {
            if (PanRequested == null)
            {
                return false;
            }

            var args = new ScrollControllerPanRequestedEventArgs(pointerPoint);
            PanRequested(this, args);
            return args.Handled;
        }

        internal FrameworkElement PanningElement { get; private set; }

        internal float PanningElementOffsetMultiplierValue { get; private set; } = 1.0f;
    }
}
