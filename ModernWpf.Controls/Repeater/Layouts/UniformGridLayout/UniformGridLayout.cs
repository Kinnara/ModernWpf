// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.Controls
{
    public class UniformGridLayout : VirtualizingLayout, IFlowLayoutAlgorithmDelegates
    {
        public UniformGridLayout()
        {
            LayoutId = "UniformGridLayout";
        }

        #region Properties

        public static readonly DependencyProperty ItemsJustificationProperty =
            DependencyProperty.Register(
                nameof(ItemsJustification),
                typeof(UniformGridLayoutItemsJustification),
                typeof(UniformGridLayout),
                new PropertyMetadata(UniformGridLayoutItemsJustification.Start, OnPropertyChanged));

        public UniformGridLayoutItemsJustification ItemsJustification
        {
            get => (UniformGridLayoutItemsJustification)GetValue(ItemsJustificationProperty);
            set => SetValue(ItemsJustificationProperty, value);
        }

        public static readonly DependencyProperty ItemsStretchProperty =
            DependencyProperty.Register(
                nameof(ItemsStretch),
                typeof(UniformGridLayoutItemsStretch),
                typeof(UniformGridLayout),
                new PropertyMetadata(UniformGridLayoutItemsStretch.None, OnPropertyChanged));

        public UniformGridLayoutItemsStretch ItemsStretch
        {
            get => (UniformGridLayoutItemsStretch)GetValue(ItemsStretchProperty);
            set => SetValue(ItemsStretchProperty, value);
        }

        public static readonly DependencyProperty MaximumRowsOrColumnsProperty =
            DependencyProperty.Register(
                nameof(MaximumRowsOrColumns),
                typeof(int),
                typeof(UniformGridLayout),
                new PropertyMetadata(-1, OnPropertyChanged));

        public int MaximumRowsOrColumns
        {
            get => (int)GetValue(MaximumRowsOrColumnsProperty);
            set => SetValue(MaximumRowsOrColumnsProperty, value);
        }

        public static readonly DependencyProperty MinColumnSpacingProperty =
            DependencyProperty.Register(
                nameof(MinColumnSpacing),
                typeof(double),
                typeof(UniformGridLayout),
                new PropertyMetadata(OnPropertyChanged));

        public double MinColumnSpacing
        {
            get => (double)GetValue(MinColumnSpacingProperty);
            set => SetValue(MinColumnSpacingProperty, value);
        }

        public static readonly DependencyProperty MinItemHeightProperty =
            DependencyProperty.Register(
                nameof(MinItemHeight),
                typeof(double),
                typeof(UniformGridLayout),
                new PropertyMetadata(OnPropertyChanged));

        public double MinItemHeight
        {
            get => (double)GetValue(MinItemHeightProperty);
            set => SetValue(MinItemHeightProperty, value);
        }

        public static readonly DependencyProperty MinItemWidthProperty =
            DependencyProperty.Register(
                nameof(MinItemWidth),
                typeof(double),
                typeof(UniformGridLayout),
                new PropertyMetadata(OnPropertyChanged));

        public double MinItemWidth
        {
            get => (double)GetValue(MinItemWidthProperty);
            set => SetValue(MinItemWidthProperty, value);
        }

        public static readonly DependencyProperty MinRowSpacingProperty =
            DependencyProperty.Register(
                nameof(MinRowSpacing),
                typeof(double),
                typeof(UniformGridLayout),
                new PropertyMetadata(OnPropertyChanged));

        public double MinRowSpacing
        {
            get => (double)GetValue(MinRowSpacingProperty);
            set => SetValue(MinRowSpacingProperty, value);
        }

        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register(
                nameof(Orientation),
                typeof(Orientation),
                typeof(UniformGridLayout),
                new PropertyMetadata(Orientation.Horizontal, OnPropertyChanged));

        public Orientation Orientation
        {
            get => (Orientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        private static void OnPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((UniformGridLayout)sender).PrivateOnPropertyChanged(args);
        }

        #endregion

        protected override void InitializeForContextCore(VirtualizingLayoutContext context)
        {
            var state = context.LayoutState;
            UniformGridLayoutState gridState = null;
            if (state != null)
            {
                gridState = GetAsGridState(state);
            }

            if (gridState == null)
            {
                if (state != null)
                {
                    throw new Exception("LayoutState must derive from UniformGridLayoutState.");
                }

                // Custom deriving layouts could potentially be stateful.
                // If that is the case, we will just create the base state required by UniformGridLayout ourselves.
                gridState = new UniformGridLayoutState();
            }

            gridState.InitializeForContext(context, this);
        }

        protected override void UninitializeForContextCore(VirtualizingLayoutContext context)
        {
            var gridState = GetAsGridState(context.LayoutState);
            gridState.UninitializeForContext(context);
        }

        protected override Size MeasureOverride(
            VirtualizingLayoutContext context,
            Size availableSize)
        {
            // Set the width and height on the grid state. If the user already set them then use the preset.
            // If not, we have to measure the first element and get back a size which we're going to be using for the rest of the items.
            var gridState = GetAsGridState(context.LayoutState);
            gridState.EnsureElementSize(availableSize, context, m_minItemWidth, m_minItemHeight, m_itemsStretch, Orientation, MinRowSpacing, MinColumnSpacing, m_maximumRowsOrColumns);

            var desiredSize = GetFlowAlgorithm(context).Measure(
                availableSize,
                context,
                true, /* isWrapping*/
                MinItemSpacing,
                LineSpacing,
                m_maximumRowsOrColumns /* maxItemsPerLine */,
                OM.ScrollOrientation,
                false /* disableVirtualization */,
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
                true /* isWrapping */,
                (FlowLayoutAlgorithm.LineAlignment)m_itemsJustification,
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

        #region IFlowLayoutAlgorithmDelegates

        Size IFlowLayoutAlgorithmDelegates.Algorithm_GetMeasureSize(
            int index,
            Size availableSize,
            VirtualizingLayoutContext context)
        {
            var gridState = GetAsGridState(context.LayoutState);
            return new Size(gridState.EffectiveItemWidth, gridState.EffectiveItemHeight);
        }

        Size IFlowLayoutAlgorithmDelegates.Algorithm_GetProvisionalArrangeSize(
            int index,
            Size measureSize,
            Size desiredSize,
            VirtualizingLayoutContext context)
        {
            var gridState = GetAsGridState(context.LayoutState);
            return new Size(gridState.EffectiveItemWidth, gridState.EffectiveItemHeight);
        }

        bool IFlowLayoutAlgorithmDelegates.Algorithm_ShouldBreakLine(
            int index,
            double remainingSpace)
        {
            return remainingSpace < 0;
        }

        FlowLayoutAnchorInfo IFlowLayoutAlgorithmDelegates.Algorithm_GetAnchorForRealizationRect(
            Size availableSize,
            VirtualizingLayoutContext context)
        {
            Rect bounds = new Rect(double.NaN, double.NaN, double.NaN, double.NaN);
            int anchorIndex = -1;

            int itemsCount = context.ItemCount;
            var realizationRect = context.RealizationRect;
            if (itemsCount > 0 && OM.MajorSize(realizationRect) > 0)
            {
                var gridState = GetAsGridState(context.LayoutState);
                var lastExtent = gridState.FlowAlgorithm.LastExtent;
                int itemsPerLine = (int)Math.Min( // note use of unsigned ints
                   Math.Max(1u, (uint)(OM.Minor(availableSize) / GetMinorSizeWithSpacing(context))),
                   Math.Max(1u, m_maximumRowsOrColumns));
                double majorSize = (itemsCount / itemsPerLine) * GetMajorSizeWithSpacing(context);
                double realizationWindowStartWithinExtent = OM.MajorStart(realizationRect) - OM.MajorStart(lastExtent);
                if ((realizationWindowStartWithinExtent + OM.MajorSize(realizationRect)) >= 0 && realizationWindowStartWithinExtent <= majorSize)
                {
                    double offset = Math.Max(0.0, OM.MajorStart(realizationRect) - OM.MajorStart(lastExtent));
                    int anchorRowIndex = (int)(offset / GetMajorSizeWithSpacing(context));

                    anchorIndex = Math.Max(0, Math.Min(itemsCount - 1, anchorRowIndex * itemsPerLine));
                    bounds = GetLayoutRectForDataIndex(availableSize, anchorIndex, lastExtent, context);
                }
            }

            return new FlowLayoutAnchorInfo
            {
                Index = anchorIndex,
                Offset = OM.MajorStart(bounds)
            };
        }

        FlowLayoutAnchorInfo IFlowLayoutAlgorithmDelegates.Algorithm_GetAnchorForTargetElement(
            int targetIndex,
            Size availableSize,
            VirtualizingLayoutContext context)
        {
            int index = -1;
            double offset = double.NaN;
            int count = context.ItemCount;
            if (targetIndex >= 0 && targetIndex < count)
            {
                int itemsPerLine = (int)Math.Min( // note use of unsigned ints
                    Math.Max(1u, (uint)OM.Minor(availableSize) / GetMinorSizeWithSpacing(context)),
                    Math.Max(1u, m_maximumRowsOrColumns));
                int indexOfFirstInLine = (targetIndex / itemsPerLine) * itemsPerLine;
                index = indexOfFirstInLine;
                var state = GetAsGridState(context.LayoutState);
                offset = OM.MajorStart(GetLayoutRectForDataIndex(availableSize, indexOfFirstInLine, state.FlowAlgorithm.LastExtent, context));
            }

            return new FlowLayoutAnchorInfo
            {
                Index = index,
                Offset = offset
            };
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
            //UNREFERENCED_PARAMETER(lastRealized);

            var extent = new Rect();


            // Constants
            int itemsCount = context.ItemCount;
            double availableSizeMinor = OM.Minor(availableSize);
            int itemsPerLine =
                (int)Math.Min( // note use of unsigned ints
                    Math.Max(1u, !double.IsInfinity(availableSizeMinor)
                        ? (uint)(availableSizeMinor / GetMinorSizeWithSpacing(context))
                        : (uint)itemsCount),
                Math.Max(1u, m_maximumRowsOrColumns));
            double lineSize = GetMajorSizeWithSpacing(context);

            if (itemsCount > 0)
            {
                // Only use all of the space if item stretch is fill, otherwise size layout according to items placed
                OM.SetMinorSize(ref extent,
                    !double.IsInfinity(availableSizeMinor) && m_itemsStretch == UniformGridLayoutItemsStretch.Fill ?
                    availableSizeMinor :
                    Math.Max(0.0, itemsPerLine * GetMinorSizeWithSpacing(context) - MinItemSpacing));
                OM.SetMajorSize(ref extent, Math.Max(0.0, (itemsCount / itemsPerLine) * lineSize - LineSpacing));

                if (firstRealized != null)
                {
                    Debug.Assert(lastRealized != null);

                    OM.SetMajorStart(ref extent, OM.MajorStart(firstRealizedLayoutBounds) - (firstRealizedItemIndex / itemsPerLine) * lineSize);
                    int remainingItems = itemsCount - lastRealizedItemIndex - 1;
                    OM.SetMajorSize(ref extent, OM.MajorEnd(lastRealizedLayoutBounds) - OM.MajorStart(extent) + (remainingItems / itemsPerLine) * lineSize);
                }
            }
            else
            {
                Debug.Assert(firstRealizedItemIndex == -1);
                Debug.Assert(lastRealizedItemIndex == -1);
            }

            return extent;
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

                //Note: For UniformGridLayout Vertical Orientation means we have a Horizontal ScrollOrientation. Horizontal Orientation means we have a Vertical ScrollOrientation.
                //i.e. the properties are the inverse of each other.
                ScrollOrientation scrollOrientation = (orientation == Orientation.Horizontal) ? ScrollOrientation.Vertical : ScrollOrientation.Horizontal;
                OM.ScrollOrientation = scrollOrientation;
            }
            else if (property == MinColumnSpacingProperty)
            {
                m_minColumnSpacing = (double)args.NewValue;
            }
            else if (property == MinRowSpacingProperty)
            {
                m_minRowSpacing = (double)args.NewValue;
            }
            else if (property == ItemsJustificationProperty)
            {
                m_itemsJustification = (UniformGridLayoutItemsJustification)args.NewValue;
            }
            else if (property == ItemsStretchProperty)
            {
                m_itemsStretch = (UniformGridLayoutItemsStretch)args.NewValue;
            }
            else if (property == MinItemWidthProperty)
            {
                m_minItemWidth = (double)args.NewValue;
            }
            else if (property == MinItemHeightProperty)
            {
                m_minItemHeight = (double)args.NewValue;
            }
            else if (property == MaximumRowsOrColumnsProperty)
            {
                m_maximumRowsOrColumns = (uint)(int)args.NewValue;
            }

            InvalidateLayout();
        }

        #endregion

        private double GetMinorSizeWithSpacing(VirtualizingLayoutContext context)
        {
            var minItemSpacing = MinItemSpacing;
            var gridState = GetAsGridState(context.LayoutState);
            return OM.ScrollOrientation == ScrollOrientation.Vertical ?
                (gridState.EffectiveItemWidth+ minItemSpacing) :
                (gridState.EffectiveItemHeight+ minItemSpacing);
        }

        private double GetMajorSizeWithSpacing(VirtualizingLayoutContext context)
        {
            var lineSpacing = LineSpacing;
            var gridState = GetAsGridState(context.LayoutState);
            return OM.ScrollOrientation == ScrollOrientation.Vertical ?
                (gridState.EffectiveItemHeight+ lineSpacing) :
                (gridState.EffectiveItemWidth+ lineSpacing);

        }

        private Rect GetLayoutRectForDataIndex(
            Size availableSize,
            int index,
            Rect lastExtent,
            VirtualizingLayoutContext context)
        {
            int itemsPerLine = (int)Math.Min( //note use of unsigned ints
                Math.Max(1u, (uint)(OM.Minor(availableSize) / GetMinorSizeWithSpacing(context))),
                Math.Max(1u, m_maximumRowsOrColumns));
            int rowIndex = index / itemsPerLine;
            int indexInRow = index - (rowIndex * itemsPerLine);

            var gridState = GetAsGridState(context.LayoutState);
            Rect bounds = OM.MinorMajorRect(
                indexInRow * GetMinorSizeWithSpacing(context) + OM.MinorStart(lastExtent),
                rowIndex * GetMajorSizeWithSpacing(context) + OM.MajorStart(lastExtent),
                OM.ScrollOrientation == ScrollOrientation.Vertical ? gridState.EffectiveItemWidth: gridState.EffectiveItemHeight,
                OM.ScrollOrientation == ScrollOrientation.Vertical ? gridState.EffectiveItemHeight: gridState.EffectiveItemWidth);

            return bounds;
        }

        private UniformGridLayoutState GetAsGridState(object state)
        {
            return state as UniformGridLayoutState;
        }

        private FlowLayoutAlgorithm GetFlowAlgorithm(VirtualizingLayoutContext context)
        {
            return GetAsGridState(context.LayoutState).FlowAlgorithm;
        }

        private void InvalidateLayout()
        {
            InvalidateMeasure();
        }

        private double LineSpacing => Orientation == Orientation.Horizontal ? m_minRowSpacing : m_minColumnSpacing;

        private double MinItemSpacing => Orientation == Orientation.Horizontal ? m_minColumnSpacing : m_minRowSpacing;

        private double m_minItemWidth = double.NaN;
        private double m_minItemHeight = double.NaN;
        private double m_minRowSpacing;
        private double m_minColumnSpacing;
        private UniformGridLayoutItemsJustification m_itemsJustification = UniformGridLayoutItemsJustification.Start;
        private UniformGridLayoutItemsStretch m_itemsStretch = UniformGridLayoutItemsStretch.None;
        private uint m_maximumRowsOrColumns = uint.MaxValue;

        private OrientationBasedMeasures OM { get; } = new OrientationBasedMeasures();
    }

    public enum UniformGridLayoutItemsJustification
    {
        Start = 0,
        Center = 1,
        End = 2,
        SpaceAround = 3,
        SpaceBetween = 4,
        SpaceEvenly = 5
    }

    public enum UniformGridLayoutItemsStretch
    {
        None = 0,
        Fill = 1,
        Uniform = 2
    }
}
