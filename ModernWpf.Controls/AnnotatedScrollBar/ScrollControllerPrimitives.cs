using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.Controls.Primitives
{
    public enum ScrollingAnimationMode
    {
        Disabled = 0,
        Enabled = 1,
        Auto = 2
    }

    public enum ScrollingSnapPointsMode
    {
        Default = 0,
        Ignore = 1
    }

    public sealed class ScrollingScrollOptions
    {
        public ScrollingScrollOptions(ScrollingAnimationMode animationMode)
            : this(animationMode, ScrollingSnapPointsMode.Default)
        {
        }

        public ScrollingScrollOptions(ScrollingAnimationMode animationMode, ScrollingSnapPointsMode snapPointsMode)
        {
            AnimationMode = animationMode;
            SnapPointsMode = snapPointsMode;
        }

        public ScrollingAnimationMode AnimationMode { get; set; }

        public ScrollingSnapPointsMode SnapPointsMode { get; set; }
    }

    public sealed class ScrollControllerScrollToRequestedEventArgs
    {
        public ScrollControllerScrollToRequestedEventArgs(double offset, ScrollingScrollOptions options)
        {
            Offset = offset;
            Options = options;
        }

        public double Offset { get; }

        public ScrollingScrollOptions Options { get; }

        public int CorrelationId { get; set; } = -1;
    }

    public sealed class ScrollControllerScrollByRequestedEventArgs
    {
        public ScrollControllerScrollByRequestedEventArgs(double offsetDelta, ScrollingScrollOptions options)
        {
            OffsetDelta = offsetDelta;
            Options = options;
        }

        public double OffsetDelta { get; }

        public ScrollingScrollOptions Options { get; }

        public int CorrelationId { get; set; } = -1;
    }

    public sealed class ScrollControllerAddScrollVelocityRequestedEventArgs
    {
        public ScrollControllerAddScrollVelocityRequestedEventArgs(float offsetVelocity, float? inertiaDecayRate)
        {
            OffsetVelocity = offsetVelocity;
            InertiaDecayRate = inertiaDecayRate;
        }

        public float OffsetVelocity { get; }

        public float? InertiaDecayRate { get; }

        public int CorrelationId { get; set; } = -1;
    }

    public sealed class ScrollControllerPanRequestedEventArgs
    {
        public ScrollControllerPanRequestedEventArgs(object pointerPoint)
        {
            PointerPoint = pointerPoint;
        }

        public object PointerPoint { get; }

        public bool Handled { get; set; }
    }

    public interface IScrollControllerPanningInfo
    {
        bool IsRailEnabled { get; }

        Orientation PanOrientation { get; }

        UIElement PanningElementAncestor { get; }

        void SetPanningElementExpressionAnimationSources(
            object propertySet,
            string minOffsetPropertyName,
            string maxOffsetPropertyName,
            string offsetPropertyName,
            string multiplierPropertyName);

        event TypedEventHandler<IScrollControllerPanningInfo, object> Changed;

        event TypedEventHandler<IScrollControllerPanningInfo, ScrollControllerPanRequestedEventArgs> PanRequested;
    }

    public interface IScrollController
    {
        IScrollControllerPanningInfo PanningInfo { get; }

        bool CanScroll { get; }

        bool IsScrollingWithMouse { get; }

        void SetIsScrollable(bool isScrollable);

        void SetValues(double minOffset, double maxOffset, double offset, double viewportLength);

        object GetScrollAnimation(int correlationId, Point startPosition, Point endPosition, object defaultAnimation);

        void NotifyRequestedScrollCompleted(int correlationId);

        event TypedEventHandler<IScrollController, object> CanScrollChanged;

        event TypedEventHandler<IScrollController, object> IsScrollingWithMouseChanged;

        event TypedEventHandler<IScrollController, ScrollControllerScrollToRequestedEventArgs> ScrollToRequested;

        event TypedEventHandler<IScrollController, ScrollControllerScrollByRequestedEventArgs> ScrollByRequested;

        event TypedEventHandler<IScrollController, ScrollControllerAddScrollVelocityRequestedEventArgs> AddScrollVelocityRequested;
    }
}
