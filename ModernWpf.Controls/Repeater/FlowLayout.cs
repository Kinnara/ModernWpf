using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.Controls
{
    public class FlowLayout : VirtualizingLayout, IFlowLayoutAlgorithmDelegates
    {
        public FlowLayout()
        {
            LayoutId = "FlowLayout";
        }

        #region Properties

        public static readonly DependencyProperty LineAlignmentProperty =
            DependencyProperty.Register(
                nameof(LineAlignment),
                typeof(FlowLayoutLineAlignment),
                typeof(FlowLayout),
                new PropertyMetadata(FlowLayoutLineAlignment.Start, OnPropertyChanged));

        public FlowLayoutLineAlignment LineAlignment
        {
            get => (FlowLayoutLineAlignment)GetValue(LineAlignmentProperty);
            set => SetValue(LineAlignmentProperty, value);
        }

        public static readonly DependencyProperty MinColumnSpacingProperty =
            DependencyProperty.Register(
                nameof(MinColumnSpacing),
                typeof(double),
                typeof(FlowLayout),
                new PropertyMetadata(0.0, OnPropertyChanged));

        public double MinColumnSpacing
        {
            get => (double)GetValue(MinColumnSpacingProperty);
            set => SetValue(MinColumnSpacingProperty, value);
        }

        public static readonly DependencyProperty MinRowSpacingProperty =
            DependencyProperty.Register(
                nameof(MinRowSpacing),
                typeof(double),
                typeof(FlowLayout),
                new PropertyMetadata(0.0, OnPropertyChanged));

        public double MinRowSpacing
        {
            get => (double)GetValue(MinRowSpacingProperty);
            set => SetValue(MinRowSpacingProperty, value);
        }

        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register(
                nameof(Orientation),
                typeof(Orientation),
                typeof(FlowLayout),
                new PropertyMetadata(Orientation.Horizontal, OnPropertyChanged));

        public Orientation Orientation
        {
            get => (Orientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        private static void OnPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((FlowLayout)sender).PrivateOnPropertyChanged(args);
        }

        #endregion

        #region IVirtualizingLayoutOverrides

        protected internal override void InitializeForContextCore(VirtualizingLayoutContext context)
        {
            var state = context.LayoutState;
            FlowLayoutState flowState = null;
            if (state != null)
            {
                flowState = GetAsFlowState(state);
            }

            if (flowState == null)
            {
                if (state != null)
                {
                    throw new Exception("LayoutState must derive from FlowLayoutState.");
                }

                // Custom deriving layouts could potentially be stateful.
                // If that is the case, we will just create the base state required by FlowLayout ourselves.
                flowState = new FlowLayoutState();
            }

            flowState.InitializeForContext(context, this);
        }

        protected internal override void UninitializeForContextCore(VirtualizingLayoutContext context)
        {
            var flowState = GetAsFlowState(context.LayoutState);
            flowState.UninitializeForContext(context);
        }

        protected internal override Size MeasureOverride(
            VirtualizingLayoutContext context,
            Size availableSize)
        {
            var desiredSize = GetFlowAlgorithm(context).Measure(
                availableSize,
                context,
                true, /* isWrapping*/
                MinItemSpacing,
                LineSpacing,
                uint.MaxValue /* maxItemsPerLine */,
                OrientationBasedMeasures.ScrollOrientation,
                false /* disableVirtualization */,
                LayoutId);
            return desiredSize;
        }

        protected internal override Size ArrangeOverride(
            VirtualizingLayoutContext context,
            Size finalSize)
        {
            var value = GetFlowAlgorithm(context).Arrange(
                finalSize,
                context,
                true, /* isWrapping */
                (FlowLayoutAlgorithm.LineAlignment)m_lineAlignment,
                LayoutId);
            return value;
        }

        protected internal override void OnItemsChangedCore(
            VirtualizingLayoutContext context,
            object source,
            NotifyCollectionChangedEventArgs args)
        {
            GetFlowAlgorithm(context).OnItemsSourceChanged(source, args, context);
            // Always invalidate layout to keep the view accurate.
            InvalidateLayout();
        }

        #endregion

        #region IFlowLayoutOverrides

        protected virtual Size GetMeasureSize(
            int index,
            Size availableSize)
        {
            return availableSize;
        }

        protected virtual Size GetProvisionalArrangeSize(
             int index,
            Size measureSize,
            Size desiredSize)
        {
            return desiredSize;
        }

        protected virtual bool ShouldBreakLine(
             int index,
             double remainingSpace)
        {
            return remainingSpace < 0;
        }

        protected virtual FlowLayoutAnchorInfo GetAnchorForRealizationRect(
            Size availableSize,
            VirtualizingLayoutContext context)
        {
            int anchorIndex = -1;
            double offset = double.NaN;

            // Constants
            int itemsCount = context.ItemCount;
            if (itemsCount > 0)
            {
                var realizationRect = context.RealizationRect;
                var state = context.LayoutState;
                var flowState = GetAsFlowState(state);
                var lastExtent = flowState.FlowAlgorithm.LastExtent;

                double averageItemsPerLine = 0;
                double averageLineSize = GetAverageLineInfo(availableSize, context, flowState, ref averageItemsPerLine) + LineSpacing;
                Debug.Assert(averageItemsPerLine != 0);

                double extentMajorSize = OrientationBasedMeasures.MajorSize(lastExtent) == 0 ? (itemsCount / averageItemsPerLine) * averageLineSize : OrientationBasedMeasures.MajorSize(lastExtent);
                if (itemsCount > 0 &&
                    OrientationBasedMeasures.MajorSize(realizationRect) > 0 &&
                    DoesRealizationWindowOverlapExtent(realizationRect, OrientationBasedMeasures.MinorMajorRect(OrientationBasedMeasures.MinorStart(lastExtent), OrientationBasedMeasures.MajorStart(lastExtent), OrientationBasedMeasures.Minor(availableSize), extentMajorSize)))
                {
                    double realizationWindowStartWithinExtent = OrientationBasedMeasures.MajorStart(realizationRect) - OrientationBasedMeasures.MajorStart(lastExtent);
                    int lineIndex = Math.Max(0, (int)(realizationWindowStartWithinExtent / averageLineSize));
                    anchorIndex = (int)(lineIndex * averageItemsPerLine);

                    // Clamp it to be within valid range
                    anchorIndex = Math.Max(0, Math.Min(itemsCount - 1, anchorIndex));
                    offset = lineIndex * averageLineSize + OrientationBasedMeasures.MajorStart(lastExtent);
                }
            }

            return new FlowLayoutAnchorInfo { Index = anchorIndex, Offset = offset };
        }

        protected virtual FlowLayoutAnchorInfo GetAnchorForTargetElement(
             int targetIndex,
            Size availableSize,
            VirtualizingLayoutContext context)
        {
            double offset = double.NaN;
            int index = -1;
            int itemsCount = context.ItemCount;

            if (targetIndex >= 0 && targetIndex < itemsCount)
            {
                index = targetIndex;
                var state = context.LayoutState;
                var flowState = GetAsFlowState(state);
                double averageItemsPerLine = 0;
                double averageLineSize = GetAverageLineInfo(availableSize, context, flowState, ref averageItemsPerLine) + LineSpacing;
                int lineIndex = (int)(targetIndex / averageItemsPerLine);
                offset = lineIndex * averageLineSize + OrientationBasedMeasures.MajorStart(flowState.FlowAlgorithm.LastExtent);
            }

            return new FlowLayoutAnchorInfo { Index = index, Offset = offset };
        }

        protected virtual Rect GetExtent(
            Size availableSize,
            VirtualizingLayoutContext context,
            UIElement firstRealized,
             int firstRealizedItemIndex,
            Rect firstRealizedLayoutBounds,
            UIElement lastRealized,
             int lastRealizedItemIndex,
            Rect lastRealizedLayoutBounds)
        {
            //UNREFERENCED_PARAMETER(lastRealized);

            var extent = new Rect();

            int itemsCount = context.ItemCount;

            if (itemsCount > 0)
            {
                double availableSizeMinor = OrientationBasedMeasures.Minor(availableSize);
                var state = context.LayoutState;
                var flowState = GetAsFlowState(state);
                double averageItemsPerLine = 0;
                double averageLineSize = GetAverageLineInfo(availableSize, context, flowState, ref averageItemsPerLine) + LineSpacing;

                Debug.Assert(averageItemsPerLine != 0);
                if (firstRealized != null)
                {
                    Debug.Assert(lastRealized != null);
                    int linesBeforeFirst = (int)(firstRealizedItemIndex / averageItemsPerLine);
                    double extentMajorStart = OrientationBasedMeasures.MajorStart(firstRealizedLayoutBounds) - linesBeforeFirst * averageLineSize;
                    OrientationBasedMeasures.SetMajorStart(ref extent, extentMajorStart);
                    int remainingItems = itemsCount - lastRealizedItemIndex - 1;
                    int remainingLinesAfterLast = (int)(remainingItems / averageItemsPerLine);
                    double extentMajorSize = OrientationBasedMeasures.MajorEnd(lastRealizedLayoutBounds) - OrientationBasedMeasures.MajorStart(extent) + remainingLinesAfterLast * averageLineSize;
                    OrientationBasedMeasures.SetMajorSize(ref extent, extentMajorSize);

                    // If the available size is infinite, we will have realized all the items in one line.
                    // In that case, the extent in the non virtualizing direction should be based on the
                    // right/bottom of the last realized element.
                    OrientationBasedMeasures.SetMinorSize(ref extent,
                        !double.IsInfinity(availableSizeMinor) ?
                        availableSizeMinor :
                        Math.Max(0.0, OrientationBasedMeasures.MinorEnd(lastRealizedLayoutBounds)));
                }
                else
                {
                    var lineSpacing = LineSpacing;
                    var minItemSpacing = MinItemSpacing;
                    // We dont have anything realized. make an educated guess.
                    int numLines = (int)Math.Ceiling(itemsCount / averageItemsPerLine);
                    extent =
                        !double.IsInfinity(availableSizeMinor) ?
                        OrientationBasedMeasures.MinorMajorRect(0, 0, availableSizeMinor, Math.Max(0.0, numLines * averageLineSize - lineSpacing)) :
                        OrientationBasedMeasures.MinorMajorRect(
                            0,
                            0,
                            Math.Max(0.0, (OrientationBasedMeasures.Minor(flowState.SpecialElementDesiredSize) + minItemSpacing) * itemsCount - minItemSpacing),
                            Math.Max(0.0, (averageLineSize - lineSpacing)));
                }
            }
            else
            {
                Debug.Assert(firstRealizedItemIndex == -1);
                Debug.Assert(lastRealizedItemIndex == -1);
            }

            return extent;
        }

        protected virtual void OnElementMeasured(
            UIElement element,
            int index,
            Size availableSize,
            Size measureSize,
            Size desiredSize,
            Size provisionalArrangeSize,
            VirtualizingLayoutContext context)
        {
        }

        protected virtual void OnLineArranged(
             int startIndex,
             int countInLine,
             double lineSize,
            VirtualizingLayoutContext context)
        {
            var flowState = GetAsFlowState(context.LayoutState);
            flowState.OnLineArranged(startIndex, countInLine, lineSize, context);
        }

        #endregion

        #region IFlowLayoutAlgorithmDelegates

        Size IFlowLayoutAlgorithmDelegates.Algorithm_GetMeasureSize(int index, Size availableSize, VirtualizingLayoutContext context)
        {
            return GetMeasureSize(index, availableSize);
        }

        Size IFlowLayoutAlgorithmDelegates.Algorithm_GetProvisionalArrangeSize(int index, Size measureSize, Size desiredSize, VirtualizingLayoutContext context)
        {
            return GetProvisionalArrangeSize(index, measureSize, desiredSize);
        }

        bool IFlowLayoutAlgorithmDelegates.Algorithm_ShouldBreakLine(int index, double remainingSpace)
        {
            return ShouldBreakLine(index, remainingSpace);
        }

        FlowLayoutAnchorInfo IFlowLayoutAlgorithmDelegates.Algorithm_GetAnchorForRealizationRect(
            Size availableSize,
            VirtualizingLayoutContext context)
        {
            return GetAnchorForRealizationRect(availableSize, context);
        }

        FlowLayoutAnchorInfo IFlowLayoutAlgorithmDelegates.Algorithm_GetAnchorForTargetElement(
            int targetIndex,
            Size availableSize,
            VirtualizingLayoutContext context)
        {
            return GetAnchorForTargetElement(targetIndex, availableSize, context);
        }

        Rect IFlowLayoutAlgorithmDelegates.Algorithm_GetExtent(
            Size availableSize,
            VirtualizingLayoutContext context,
            UIElement firstRealized,
            int firstRealizedItemIndex,
            Rect firstRealizedLayoutBounds,
            UIElement lastRealized,
            int lastRealizedItemIndex,
            Rect lastRealizedLayoutBounds)
        {
            return GetExtent(
                availableSize,
                context,
                firstRealized,
                firstRealizedItemIndex,
                firstRealizedLayoutBounds,
                lastRealized,
                lastRealizedItemIndex,
                lastRealizedLayoutBounds);
        }

        void IFlowLayoutAlgorithmDelegates.Algorithm_OnElementMeasured(
            UIElement element,
            int index,
            Size availableSize,
            Size measureSize,
            Size desiredSize,
            Size provisionalArrangeSize,
            VirtualizingLayoutContext context)
        {
            OnElementMeasured(
                element,
                index,
                availableSize,
                measureSize,
                desiredSize,
                provisionalArrangeSize,
                context);
        }

        void IFlowLayoutAlgorithmDelegates.Algorithm_OnLineArranged(
            int startIndex,
            int countInLine,
            double lineSize,
            VirtualizingLayoutContext context)
        {
            OnLineArranged(
                startIndex,
                countInLine,
                lineSize,
                context);
        }

        #endregion

        private void PrivateOnPropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            var property = args.Property;
            if (property == OrientationProperty)
            {
                var orientation = (Orientation)args.NewValue;

                //Note: For FlowLayout Vertical Orientation means we have a Horizontal ScrollOrientation. Horizontal Orientation means we have a Vertical ScrollOrientation.
                //i.e. the properties are the inverse of each other.
                ScrollOrientation scrollOrientation = (orientation == Orientation.Horizontal) ? ScrollOrientation.Vertical : ScrollOrientation.Horizontal;
                OrientationBasedMeasures.ScrollOrientation = scrollOrientation;
            }
            else if (property == MinColumnSpacingProperty)
            {
                m_minColumnSpacing = (double)args.NewValue;
            }
            else if (property == MinRowSpacingProperty)
            {
                m_minRowSpacing = (double)args.NewValue;
            }
            else if (property == LineAlignmentProperty)
            {
                m_lineAlignment = (FlowLayoutLineAlignment)args.NewValue;
            }

            InvalidateLayout();
        }

        private double GetAverageLineInfo(
            Size availableSize,
            VirtualizingLayoutContext context,
            FlowLayoutState flowState,
            ref double avgCountInLine)
        {
            // default to 1 item per line with 0 size
            double avgLineSize = 0;
            avgCountInLine = 1;

            Debug.Assert(context.ItemCountCore() > 0);
            if (flowState.TotalLinesMeasured == 0)
            {
                var tmpElement = context.GetOrCreateElementAt(0, ElementRealizationOptions.ForceCreate | ElementRealizationOptions.SuppressAutoRecycle);
                var desiredSize = flowState.FlowAlgorithm.MeasureElement(tmpElement, 0, availableSize, context);
                context.RecycleElement(tmpElement);

                int estimatedCountInLine = Math.Max(1, (int)(OrientationBasedMeasures.Minor(availableSize) / OrientationBasedMeasures.Minor(desiredSize)));
                flowState.OnLineArranged(0, estimatedCountInLine, OrientationBasedMeasures.Major(desiredSize), context);
                flowState.SpecialElementDesiredSize = desiredSize;
            }

            avgCountInLine = Math.Max(1.0, flowState.TotalItemsPerLine / flowState.TotalLinesMeasured);
            avgLineSize = Math.Round(flowState.TotalLineSize / flowState.TotalLinesMeasured);

            return avgLineSize;
        }

        private FlowLayoutState GetAsFlowState(object state)
        {
            return state as FlowLayoutState;
        }

        private void InvalidateLayout()
        {
            InvalidateMeasure();
        }

        private FlowLayoutAlgorithm GetFlowAlgorithm(VirtualizingLayoutContext context)
        {
            return GetAsFlowState(context.LayoutState).FlowAlgorithm;
        }

        private bool DoesRealizationWindowOverlapExtent(Rect realizationWindow, Rect extent)
        {
            return OrientationBasedMeasures.MajorEnd(realizationWindow) >= OrientationBasedMeasures.MajorStart(extent) && OrientationBasedMeasures.MajorStart(realizationWindow) <= OrientationBasedMeasures.MajorEnd(extent);
        }

        private double LineSpacing => OrientationBasedMeasures.ScrollOrientation == ScrollOrientation.Vertical ? m_minColumnSpacing : m_minRowSpacing;

        private double MinItemSpacing => OrientationBasedMeasures.ScrollOrientation == ScrollOrientation.Vertical ? m_minRowSpacing : m_minColumnSpacing;

        // Fields
        private double m_minRowSpacing;
        private double m_minColumnSpacing;
        private FlowLayoutLineAlignment m_lineAlignment = FlowLayoutLineAlignment.Start;

        // !!! WARNING !!!
        // Any storage here needs to be related to layout configuration. 
        // layout specific state needs to be stored in FlowLayoutState.

        private OrientationBasedMeasures OrientationBasedMeasures { get; } = new OrientationBasedMeasures();
    }

    public enum FlowLayoutLineAlignment
    {
        Start = 0,
        Center = 1,
        End = 2,
        SpaceAround = 3,
        SpaceBetween = 4,
        SpaceEvenly = 5
    }
}
