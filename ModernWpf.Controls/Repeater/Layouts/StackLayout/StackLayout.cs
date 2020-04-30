// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.Controls
{
    public class StackLayout : VirtualizingLayout, IFlowLayoutAlgorithmDelegates
    {
        public StackLayout()
        {
            LayoutId = "StackLayout";
        }

        #region Properties

        public static readonly DependencyProperty DisableVirtualizationProperty =
            DependencyProperty.Register(
                nameof(DisableVirtualization),
                typeof(bool),
                typeof(StackLayout),
                new PropertyMetadata(false, OnPropertyChanged));

        public bool DisableVirtualization
        {
            get => (bool)GetValue(DisableVirtualizationProperty);
            set => SetValue(DisableVirtualizationProperty, value);
        }

        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register(
                nameof(Orientation),
                typeof(Orientation),
                typeof(StackLayout),
                new PropertyMetadata(Orientation.Vertical, OnPropertyChanged));

        public Orientation Orientation
        {
            get => (Orientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        public static readonly DependencyProperty SpacingProperty =
            DependencyProperty.Register(
                nameof(Spacing),
                typeof(double),
                typeof(StackLayout),
                new PropertyMetadata(0.0, OnPropertyChanged));

        public double Spacing
        {
            get => (double)GetValue(SpacingProperty);
            set => SetValue(SpacingProperty, value);
        }

        private static void OnPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((StackLayout)sender).PrivateOnPropertyChanged(args);
        }

        #endregion

        protected override void InitializeForContextCore(VirtualizingLayoutContext context)
        {
            var state = context.LayoutState;
            StackLayoutState stackState = null;
            if (state != null)
            {
                stackState = GetAsStackState(state);
            }

            if (stackState == null)
            {
                if (state != null)
                {
                    throw new Exception("LayoutState must derive from StackLayoutState.");
                }

                // Custom deriving layouts could potentially be stateful.
                // If that is the case, we will just create the base state required by UniformGridLayout ourselves.
                stackState = new StackLayoutState();
            }

            stackState.InitializeForContext(context, this);
        }

        protected override void UninitializeForContextCore(VirtualizingLayoutContext context)
        {
            var stackState = GetAsStackState(context.LayoutState);
            stackState.UninitializeForContext(context);
        }

        protected override Size MeasureOverride(
            VirtualizingLayoutContext context,
            Size availableSize)
        {
            GetAsStackState(context.LayoutState).OnMeasureStart();

            var desiredSize = GetFlowAlgorithm(context).Measure(
                availableSize,
                context,
                false, /* isWrapping*/
                0 /* minItemSpacing */,
                m_itemSpacing,
                uint.MaxValue /* maxItemsPerLine */,
                OM.ScrollOrientation,
                DisableVirtualization,
                LayoutId);
            return desiredSize;
        }

        protected override Size ArrangeOverride(
            VirtualizingLayoutContext context,
            Size finalSize)
        {
            var value = GetFlowAlgorithm(context).Arrange(
                finalSize,
                context,
                false, /* isWraping */
                FlowLayoutAlgorithm.LineAlignment.Start,
                LayoutId);

            return value;
        }

        protected override void OnItemsChangedCore(
            VirtualizingLayoutContext context,
            object source,
            NotifyCollectionChangedEventArgs args)
        {
            GetFlowAlgorithm(context).OnItemsSourceChanged(source, args, context);
            // Always invalidate layout to keep the view accurate.
            InvalidateLayout();
        }

        private FlowLayoutAnchorInfo GetAnchorForRealizationRect(
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
                var state = GetAsStackState(context.LayoutState);
                var lastExtent = state.FlowAlgorithm.LastExtent;

                double averageElementSize = GetAverageElementSize(availableSize, context, state) + m_itemSpacing;
                double realizationWindowOffsetInExtent = OM.MajorStart(realizationRect) - OM.MajorStart(lastExtent);
                double majorSize = OM.MajorSize(lastExtent) == 0 ? Math.Max(0.0, averageElementSize * itemsCount - m_itemSpacing) : OM.MajorSize(lastExtent);
                if (itemsCount > 0 &&
                    OM.MajorSize(realizationRect) >= 0 &&
                    // MajorSize = 0 will account for when a nested repeater is outside the realization rect but still being measured. Also,
                    // note that if we are measuring this repeater, then we are already realizing an element to figure out the size, so we could
                    // just keep that element alive. It also helps in XYFocus scenarios to have an element realized for XYFocus to find a candidate
                    // in the navigating direction.
                    realizationWindowOffsetInExtent + OM.MajorSize(realizationRect) >= 0 && realizationWindowOffsetInExtent <= majorSize)
                {
                    anchorIndex = (int)(realizationWindowOffsetInExtent / averageElementSize);
                    offset = anchorIndex * averageElementSize + OM.MajorStart(lastExtent);
                    anchorIndex = Math.Max(0, Math.Min(itemsCount - 1, anchorIndex));
                }
            }

            return new FlowLayoutAnchorInfo { Index = anchorIndex, Offset = offset };
        }

        private Rect GetExtent(
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

            // Constants
            int itemsCount = context.ItemCount;
            var stackState = GetAsStackState(context.LayoutState);
            double averageElementSize = GetAverageElementSize(availableSize, context, stackState) + m_itemSpacing;

            OM.SetMinorSize(ref extent, stackState.MaxArrangeBounds);
            OM.SetMajorSize(ref extent, Math.Max(0.0, itemsCount * averageElementSize - m_itemSpacing));
            if (itemsCount > 0)
            {
                if (firstRealized != null)
                {
                    Debug.Assert(lastRealized != null);
                    OM.SetMajorStart(ref extent, OM.MajorStart(firstRealizedLayoutBounds) - firstRealizedItemIndex * averageElementSize);
                    var remainingItems = itemsCount - lastRealizedItemIndex - 1;
                    OM.SetMajorSize(ref extent, OM.MajorEnd(lastRealizedLayoutBounds) - OM.MajorStart(extent) + (remainingItems * averageElementSize));
                }
            }
            else
            {
                Debug.Assert(firstRealizedItemIndex == -1);
                Debug.Assert(lastRealizedItemIndex == -1);
            }

            return extent;
        }

        private void OnElementMeasured(
            UIElement element,
            int index,
            Size availableSize,
            Size measureSize,
            Size desiredSize,
            Size provisionalArrangeSize,
            VirtualizingLayoutContext context)
        {
            if (context is VirtualizingLayoutContext virtualContext)
            {
                var stackState = GetAsStackState(virtualContext.LayoutState);
                var provisionalArrangeSizeWinRt = provisionalArrangeSize;
                stackState.OnElementMeasured(
                    index,
                    OM.Major(provisionalArrangeSizeWinRt),
                    OM.Minor(provisionalArrangeSizeWinRt));
            }
        }

        Size IFlowLayoutAlgorithmDelegates.Algorithm_GetMeasureSize(int index, Size availableSize, VirtualizingLayoutContext context)
        {
            return availableSize;
        }

        Size IFlowLayoutAlgorithmDelegates.Algorithm_GetProvisionalArrangeSize(int index, Size measureSize, Size desiredSize, VirtualizingLayoutContext context)
        {
            var measureSizeMinor = OM.Minor(measureSize);
            return OM.MinorMajorSize(
                !double.IsInfinity(measureSizeMinor) ?
                    Math.Max(measureSizeMinor, OM.Minor(desiredSize)) :
                    OM.Minor(desiredSize),
                OM.Major(desiredSize));
        }

        bool IFlowLayoutAlgorithmDelegates.Algorithm_ShouldBreakLine(int index, double remainingSpace)
        {
            return true;
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
            double offset = double.NaN;
            int index = -1;
            int itemsCount = context.ItemCount;

            if (targetIndex >= 0 && targetIndex < itemsCount)
            {
                index = targetIndex;
                var state = GetAsStackState(context.LayoutState);
                double averageElementSize = GetAverageElementSize(availableSize, context, state) + m_itemSpacing;
                offset = index * averageElementSize + OM.MajorStart(state.FlowAlgorithm.LastExtent);
            }

            return new FlowLayoutAnchorInfo { Index = index, Offset = offset };
        }

        Rect IFlowLayoutAlgorithmDelegates.Algorithm_GetExtent(Size availableSize,
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
        }

        private void PrivateOnPropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            var property = args.Property;
            if (property == OrientationProperty)
            {
                var orientation = (Orientation)args.NewValue;

                //Note: For StackLayout Vertical Orientation means we have a Vertical ScrollOrientation.
                //Horizontal Orientation means we have a Horizontal ScrollOrientation.
                ScrollOrientation scrollOrientation = (orientation == Orientation.Horizontal) ? ScrollOrientation.Horizontal : ScrollOrientation.Vertical;
                OM.ScrollOrientation = scrollOrientation;
            }
            else if (property == SpacingProperty)
            {
                m_itemSpacing = (double)args.NewValue;
            }

            InvalidateLayout();
        }

        private double GetAverageElementSize(
            Size availableSize,
            VirtualizingLayoutContext context,
            StackLayoutState stackLayoutState)
        {
            double averageElementSize = 0;

            if (context.ItemCount > 0)
            {
                if (stackLayoutState.TotalElementsMeasured == 0)
                {
                    var tmpElement = context.GetOrCreateElementAt(0, ElementRealizationOptions.ForceCreate | ElementRealizationOptions.SuppressAutoRecycle);
                    stackLayoutState.FlowAlgorithm.MeasureElement(tmpElement, 0, availableSize, context);
                    context.RecycleElement(tmpElement);
                }

                Debug.Assert(stackLayoutState.TotalElementsMeasured > 0);
                averageElementSize = Math.Round(stackLayoutState.TotalElementSize / stackLayoutState.TotalElementsMeasured, MidpointRounding.AwayFromZero);
            }

            return averageElementSize;
        }

        private StackLayoutState GetAsStackState(object state)
        {
            return state as StackLayoutState;
        }

        private void InvalidateLayout()
        {
            InvalidateMeasure();
        }

        private FlowLayoutAlgorithm GetFlowAlgorithm(VirtualizingLayoutContext context)
        {
            return GetAsStackState(context.LayoutState).FlowAlgorithm;
        }

        private double m_itemSpacing;

        // !!! WARNING !!!
        // Any storage here needs to be related to layout configuration. 
        // layout specific state needs to be stored in StackLayoutState.

        private OrientationBasedMeasures OM { get; } = new OrientationBasedMeasures();
    }

    public struct FlowLayoutAnchorInfo
    {
        public int Index;
        public double Offset;
    };
}
