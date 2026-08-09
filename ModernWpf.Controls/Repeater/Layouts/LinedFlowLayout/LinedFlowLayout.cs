// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;

namespace ModernWpf.Controls
{
    public class LinedFlowLayout : VirtualizingLayout, IFlowLayoutAlgorithmDelegates
    {
        private readonly List<WeakReference<LinedFlowLayoutState>> _states = new List<WeakReference<LinedFlowLayoutState>>();

        public LinedFlowLayout()
        {
            LayoutId = nameof(LinedFlowLayout);
            SetIndexBasedLayoutOrientation(IndexBasedLayoutOrientation.LeftToRight);
        }

        public static readonly DependencyProperty ItemsJustificationProperty =
            DependencyProperty.Register(
                nameof(ItemsJustification),
                typeof(LinedFlowLayoutItemsJustification),
                typeof(LinedFlowLayout),
                new FrameworkPropertyMetadata(
                    LinedFlowLayoutItemsJustification.Start,
                    OnLayoutPropertyChanged));

        public LinedFlowLayoutItemsJustification ItemsJustification
        {
            get => (LinedFlowLayoutItemsJustification)GetValue(ItemsJustificationProperty);
            set => SetValue(ItemsJustificationProperty, value);
        }

        public static readonly DependencyProperty ItemsStretchProperty =
            DependencyProperty.Register(
                nameof(ItemsStretch),
                typeof(LinedFlowLayoutItemsStretch),
                typeof(LinedFlowLayout),
                new FrameworkPropertyMetadata(
                    LinedFlowLayoutItemsStretch.None,
                    OnLayoutPropertyChanged));

        public LinedFlowLayoutItemsStretch ItemsStretch
        {
            get => (LinedFlowLayoutItemsStretch)GetValue(ItemsStretchProperty);
            set => SetValue(ItemsStretchProperty, value);
        }

        public static readonly DependencyProperty MinItemSpacingProperty =
            DependencyProperty.Register(
                nameof(MinItemSpacing),
                typeof(double),
                typeof(LinedFlowLayout),
                new FrameworkPropertyMetadata(0.0, OnLayoutPropertyChanged),
                IsNonNegativeFinite);

        public double MinItemSpacing
        {
            get => (double)GetValue(MinItemSpacingProperty);
            set => SetValue(MinItemSpacingProperty, value);
        }

        public static readonly DependencyProperty LineSpacingProperty =
            DependencyProperty.Register(
                nameof(LineSpacing),
                typeof(double),
                typeof(LinedFlowLayout),
                new FrameworkPropertyMetadata(0.0, OnLayoutPropertyChanged),
                IsNonNegativeFinite);

        public double LineSpacing
        {
            get => (double)GetValue(LineSpacingProperty);
            set => SetValue(LineSpacingProperty, value);
        }

        public static readonly DependencyProperty LineHeightProperty =
            DependencyProperty.Register(
                nameof(LineHeight),
                typeof(double),
                typeof(LinedFlowLayout),
                new FrameworkPropertyMetadata(double.NaN, OnLayoutPropertyChanged),
                IsValidLineHeight);

        public double LineHeight
        {
            get => (double)GetValue(LineHeightProperty);
            set => SetValue(LineHeightProperty, value);
        }

        private static readonly DependencyPropertyKey ActualLineHeightPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(ActualLineHeight),
                typeof(double),
                typeof(LinedFlowLayout),
                new FrameworkPropertyMetadata(0.0));

        public static readonly DependencyProperty ActualLineHeightProperty = ActualLineHeightPropertyKey.DependencyProperty;

        public double ActualLineHeight => (double)GetValue(ActualLineHeightProperty);

        public int RequestedRangeStartIndex { get; private set; } = -1;

        public int RequestedRangeLength { get; private set; }

        public event TypedEventHandler<LinedFlowLayout, LinedFlowLayoutItemsInfoRequestedEventArgs> ItemsInfoRequested;

        public event TypedEventHandler<LinedFlowLayout, object> ItemsUnlocked;

        public void InvalidateItemsInfo()
        {
            ForEachState(state => state.ClearItemsInfo());
            RequestedRangeStartIndex = -1;
            RequestedRangeLength = 0;
            UnlockItems();
            InvalidateMeasure();
        }

        public int LockItemToLine(int itemIndex)
        {
            if (itemIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(itemIndex));
            }

            int lineIndex = -1;
            ForEachState(state =>
            {
                if (lineIndex < 0 && itemIndex < state.LineIndices.Length)
                {
                    lineIndex = state.LineIndices[itemIndex];
                    if (lineIndex >= 0)
                    {
                        state.LockedLines[itemIndex] = lineIndex;
                    }
                }
            });

            return lineIndex;
        }

        protected override void InitializeForContextCore(VirtualizingLayoutContext context)
        {
            if (context.LayoutState != null && !(context.LayoutState is LinedFlowLayoutState))
            {
                throw new InvalidOperationException("LayoutState must be a LinedFlowLayoutState.");
            }

            var state = context.LayoutState as LinedFlowLayoutState ?? new LinedFlowLayoutState();
            state.InitializeForContext(context, this);
            _states.Add(new WeakReference<LinedFlowLayoutState>(state));
        }

        protected override void UninitializeForContextCore(VirtualizingLayoutContext context)
        {
            if (context.LayoutState is LinedFlowLayoutState state)
            {
                state.UninitializeForContext(context);
            }
        }

        protected override Size MeasureOverride(VirtualizingLayoutContext context, Size availableSize)
        {
            var state = GetState(context);
            int itemCount = context.ItemCount;
            if (itemCount == 0)
            {
                state.PlannedWidths = new double[0];
                state.LineIndices = new int[0];
                state.LineStartIndices.Clear();
                state.TotalHeight = 0.0;
                SetValue(ActualLineHeightPropertyKey, 0.0);
                return default;
            }

            double actualLineHeight = ResolveLineHeight(context);
            if (!AreClose(ActualLineHeight, actualLineHeight))
            {
                SetValue(ActualLineHeightPropertyKey, actualLineHeight);
                UnlockItems();
            }

            double availableWidth = ResolveAvailableWidth(availableSize, context, actualLineHeight);
            RequestItemsInfo(context, state, itemCount, availableWidth, actualLineHeight);
            BuildPlan(state, itemCount, availableWidth, actualLineHeight);

            var measured = state.FlowAlgorithm.Measure(
                new Size(availableWidth, availableSize.Height),
                context,
                true,
                MinItemSpacing,
                LineSpacing,
                uint.MaxValue,
                ScrollOrientation.Vertical,
                false,
                LayoutId);

            if (state.NeedsRemeasure)
            {
                state.NeedsRemeasure = false;
                InvalidateMeasure();
            }

            return new Size(Math.Max(measured.Width, availableWidth), state.TotalHeight);
        }

        protected override Size ArrangeOverride(VirtualizingLayoutContext context, Size finalSize)
        {
            var state = GetState(context);
            var alignment = ItemsStretch == LinedFlowLayoutItemsStretch.Fill
                ? FlowLayoutAlgorithm.LineAlignment.Start
                : (FlowLayoutAlgorithm.LineAlignment)ItemsJustification;
            state.FlowAlgorithm.Arrange(finalSize, context, true, alignment, LayoutId);
            return new Size(finalSize.Width, state.TotalHeight);
        }

        protected override void OnItemsChangedCore(
            VirtualizingLayoutContext context,
            object source,
            NotifyCollectionChangedEventArgs args)
        {
            if (context.LayoutState is LinedFlowLayoutState state)
            {
                state.FlowAlgorithm.OnItemsSourceChanged(source, args, context);
                state.ClearItemsInfo();
                state.MeasuredAspectRatios.Clear();
            }

            UnlockItems();
            InvalidateMeasure();
        }

        Size IFlowLayoutAlgorithmDelegates.Algorithm_GetMeasureSize(
            int index,
            Size availableSize,
            VirtualizingLayoutContext context)
        {
            return new Size(double.PositiveInfinity, ActualLineHeight);
        }

        Size IFlowLayoutAlgorithmDelegates.Algorithm_GetProvisionalArrangeSize(
            int index,
            Size measureSize,
            Size desiredSize,
            VirtualizingLayoutContext context)
        {
            var state = GetState(context);
            return new Size(GetPlannedWidth(state, index), ActualLineHeight);
        }

        bool IFlowLayoutAlgorithmDelegates.Algorithm_ShouldBreakLine(int index, double remainingSpace)
        {
            return remainingSpace < 0.0;
        }

        FlowLayoutAnchorInfo IFlowLayoutAlgorithmDelegates.Algorithm_GetAnchorForRealizationRect(
            Size availableSize,
            VirtualizingLayoutContext context)
        {
            var state = GetState(context);
            Rect rect = context.RealizationRect;
            int lineIndex = GetLineIndexForOffset(state, rect.Y);
            return new FlowLayoutAnchorInfo
            {
                Index = GetLineStartIndex(state, lineIndex),
                Offset = Math.Max(0, lineIndex) * (ActualLineHeight + LineSpacing)
            };
        }

        FlowLayoutAnchorInfo IFlowLayoutAlgorithmDelegates.Algorithm_GetAnchorForTargetElement(
            int targetIndex,
            Size availableSize,
            VirtualizingLayoutContext context)
        {
            var state = GetState(context);
            int lineIndex = targetIndex >= 0 && targetIndex < state.LineIndices.Length
                ? state.LineIndices[targetIndex]
                : -1;
            return new FlowLayoutAnchorInfo
            {
                Index = lineIndex >= 0 ? GetLineStartIndex(state, lineIndex) : -1,
                Offset = lineIndex >= 0 ? lineIndex * (ActualLineHeight + LineSpacing) : double.NaN
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
            var state = GetState(context);
            return new Rect(0.0, 0.0, state.AvailableWidth, state.TotalHeight);
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
            if (desiredSize.Width <= 0.0 || desiredSize.Height <= 0.0 ||
                double.IsInfinity(desiredSize.Width) || double.IsNaN(desiredSize.Width))
            {
                return;
            }

            var state = GetState(context);
            double ratio = desiredSize.Width / desiredSize.Height;
            if (!state.MeasuredAspectRatios.TryGetValue(index, out double oldRatio) || Math.Abs(oldRatio - ratio) > 0.02)
            {
                state.MeasuredAspectRatios[index] = ratio;
                if (!state.AspectRatios.ContainsKey(index))
                {
                    state.NeedsRemeasure = true;
                }
            }
        }

        void IFlowLayoutAlgorithmDelegates.Algorithm_OnLineArranged(
            int startIndex,
            int countInLine,
            double lineSize,
            VirtualizingLayoutContext context)
        {
        }

        private static bool IsNonNegativeFinite(object value)
        {
            double number = (double)value;
            return number >= 0.0 && !double.IsInfinity(number) && !double.IsNaN(number);
        }

        private static bool IsValidLineHeight(object value)
        {
            double number = (double)value;
            return double.IsNaN(number) || (number > 0.0 && !double.IsInfinity(number));
        }

        private static bool AreClose(double first, double second)
        {
            if (first == second)
            {
                return true;
            }

            double tolerance = (Math.Abs(first) + Math.Abs(second) + 10.0) * 1.1102230246251565E-16;
            double difference = first - second;
            return -tolerance < difference && difference < tolerance;
        }

        private static void OnLayoutPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var owner = (LinedFlowLayout)sender;
            owner.UnlockItems();
            owner.InvalidateMeasure();
        }

        private double ResolveLineHeight(VirtualizingLayoutContext context)
        {
            if (!double.IsNaN(LineHeight))
            {
                return LineHeight;
            }

            UIElement element = context.GetOrCreateElementAt(0);
            element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            double height = element.DesiredSize.Height;
            return height > 0.0 && !double.IsInfinity(height) && !double.IsNaN(height) ? height : 100.0;
        }

        private static double ResolveAvailableWidth(Size availableSize, VirtualizingLayoutContext context, double lineHeight)
        {
            double width = availableSize.Width;
            if (double.IsInfinity(width) || double.IsNaN(width) || width <= 0.0)
            {
                width = context.VisibleRect.Width;
            }

            if (double.IsInfinity(width) || double.IsNaN(width) || width <= 0.0)
            {
                width = context.RealizationRect.Width;
            }

            return width > 0.0 && !double.IsInfinity(width) && !double.IsNaN(width)
                ? width
                : Math.Max(1.0, lineHeight);
        }

        private void RequestItemsInfo(
            VirtualizingLayoutContext context,
            LinedFlowLayoutState state,
            int itemCount,
            double availableWidth,
            double lineHeight)
        {
            var handler = ItemsInfoRequested;
            if (handler == null)
            {
                RequestedRangeStartIndex = -1;
                RequestedRangeLength = 0;
                return;
            }

            GetRequestedRange(context, state, itemCount, availableWidth, lineHeight, out int startIndex, out int length);
            if (state.InfoStartIndex >= 0 &&
                startIndex >= state.InfoStartIndex &&
                startIndex + length <= state.InfoStartIndex + state.InfoLength)
            {
                RequestedRangeStartIndex = state.InfoStartIndex;
                RequestedRangeLength = state.InfoLength;
                return;
            }

            var args = new LinedFlowLayoutItemsInfoRequestedEventArgs(startIndex, length);
            handler(this, args);

            state.UniformMinWidth = args.MinWidth;
            state.UniformMaxWidth = args.MaxWidth;
            if (args.EstablishedLength <= 0)
            {
                RequestedRangeStartIndex = -1;
                RequestedRangeLength = 0;
                return;
            }

            int suppliedStart = args.ItemsRangeStartIndex;
            int suppliedLength = Math.Min(args.EstablishedLength, itemCount - suppliedStart);
            ApplyItemsInfo(state.AspectRatios, suppliedStart, suppliedLength, args.DesiredAspectRatios, true);
            ApplyItemsInfo(state.MinWidths, suppliedStart, suppliedLength, args.MinWidths, false);
            ApplyItemsInfo(state.MaxWidths, suppliedStart, suppliedLength, args.MaxWidths, false);
            state.InfoStartIndex = suppliedStart;
            state.InfoLength = suppliedLength;
            RequestedRangeStartIndex = suppliedStart;
            RequestedRangeLength = suppliedLength;
        }

        private void GetRequestedRange(
            VirtualizingLayoutContext context,
            LinedFlowLayoutState state,
            int itemCount,
            double availableWidth,
            double lineHeight,
            out int startIndex,
            out int length)
        {
            Rect viewport = context.VisibleRect.IsEmpty ? context.RealizationRect : context.VisibleRect;
            double stride = Math.Max(1.0, lineHeight + LineSpacing);
            int firstLine = Math.Max(0, (int)Math.Floor(Math.Max(0.0, viewport.Y) / stride));
            int viewportLines = Math.Max(1, (int)Math.Ceiling(Math.Max(stride, viewport.Height) / stride));
            int requestedFirstLine = Math.Max(0, firstLine - viewportLines * 2);
            int requestedLastLine = firstLine + viewportLines * 3;

            if (state.LineStartIndices.Count > 0)
            {
                requestedFirstLine = Math.Min(requestedFirstLine, state.LineStartIndices.Count - 1);
                requestedLastLine = Math.Min(requestedLastLine, state.LineStartIndices.Count - 1);
                startIndex = state.LineStartIndices[requestedFirstLine];
                int endIndex = requestedLastLine + 1 < state.LineStartIndices.Count
                    ? state.LineStartIndices[requestedLastLine + 1]
                    : itemCount;
                length = Math.Max(1, endIndex - startIndex);
                return;
            }

            int estimatedItemsPerLine = Math.Max(
                1,
                (int)Math.Floor((availableWidth + MinItemSpacing) / Math.Max(1.0, lineHeight + MinItemSpacing)));
            startIndex = Math.Min(itemCount - 1, requestedFirstLine * estimatedItemsPerLine);
            length = Math.Min(itemCount - startIndex, Math.Max(1, (requestedLastLine - requestedFirstLine + 1) * estimatedItemsPerLine));
        }

        private static void ApplyItemsInfo(
            Dictionary<int, double> target,
            int startIndex,
            int length,
            double[] values,
            bool positiveOnly)
        {
            if (values == null)
            {
                return;
            }

            for (int index = 0; index < length; index++)
            {
                double value = values[index];
                if (!double.IsNaN(value) && !double.IsInfinity(value) && (!positiveOnly || value > 0.0))
                {
                    target[startIndex + index] = value;
                }
            }
        }

        private void BuildPlan(LinedFlowLayoutState state, int itemCount, double availableWidth, double lineHeight)
        {
            state.AvailableWidth = availableWidth;
            state.PlannedWidths = new double[itemCount];
            state.LineIndices = new int[itemCount];
            state.LineStartIndices.Clear();

            var lineItems = new List<int>();
            double occupied = 0.0;
            int lineIndex = 0;
            for (int itemIndex = 0; itemIndex < itemCount; itemIndex++)
            {
                double width = GetNaturalWidth(state, itemIndex, lineHeight);
                bool startsNewLine = lineItems.Count > 0 && occupied + MinItemSpacing + width > availableWidth;
                if (startsNewLine)
                {
                    CompleteLine(state, lineItems, occupied, availableWidth, lineIndex++);
                    lineItems.Clear();
                    occupied = 0.0;
                }

                if (lineItems.Count == 0)
                {
                    state.LineStartIndices.Add(itemIndex);
                }
                else
                {
                    occupied += MinItemSpacing;
                }

                state.PlannedWidths[itemIndex] = width;
                lineItems.Add(itemIndex);
                occupied += width;
            }

            if (lineItems.Count > 0)
            {
                CompleteLine(state, lineItems, occupied, availableWidth, lineIndex++);
            }

            state.TotalHeight = lineIndex == 0
                ? 0.0
                : lineIndex * lineHeight + Math.Max(0, lineIndex - 1) * LineSpacing;

            bool unlocked = false;
            foreach (var pair in state.LockedLines)
            {
                if (pair.Key >= state.LineIndices.Length || state.LineIndices[pair.Key] != pair.Value)
                {
                    unlocked = true;
                    break;
                }
            }

            if (unlocked)
            {
                state.LockedLines.Clear();
                ItemsUnlocked?.Invoke(this, null);
            }
        }

        private void CompleteLine(
            LinedFlowLayoutState state,
            List<int> lineItems,
            double occupied,
            double availableWidth,
            int lineIndex)
        {
            if (ItemsStretch == LinedFlowLayoutItemsStretch.Fill && occupied < availableWidth)
            {
                StretchLine(state, lineItems, availableWidth - occupied);
            }

            foreach (int itemIndex in lineItems)
            {
                state.LineIndices[itemIndex] = lineIndex;
            }
        }

        private void StretchLine(LinedFlowLayoutState state, List<int> lineItems, double remaining)
        {
            var growable = new List<int>(lineItems);
            while (remaining > 0.01 && growable.Count > 0)
            {
                double share = remaining / growable.Count;
                bool consumedAny = false;
                for (int index = growable.Count - 1; index >= 0; index--)
                {
                    int itemIndex = growable[index];
                    double maximum = GetMaximumWidth(state, itemIndex);
                    double growth = Math.Min(share, maximum - state.PlannedWidths[itemIndex]);
                    if (growth > 0.0)
                    {
                        state.PlannedWidths[itemIndex] += growth;
                        remaining -= growth;
                        consumedAny = true;
                    }

                    if (state.PlannedWidths[itemIndex] >= maximum - 0.01)
                    {
                        growable.RemoveAt(index);
                    }
                }

                if (!consumedAny)
                {
                    break;
                }
            }
        }

        private double GetNaturalWidth(LinedFlowLayoutState state, int index, double lineHeight)
        {
            double ratio;
            if (!state.AspectRatios.TryGetValue(index, out ratio) &&
                !state.MeasuredAspectRatios.TryGetValue(index, out ratio))
            {
                ratio = 1.0;
            }

            double width = Math.Max(0.0, lineHeight * Math.Max(0.01, ratio));
            double minimum = state.MinWidths.TryGetValue(index, out double itemMin)
                ? Math.Max(0.0, itemMin)
                : Math.Max(0.0, state.UniformMinWidth);
            double maximum = GetMaximumWidth(state, index);
            return Math.Max(minimum, Math.Min(maximum, width));
        }

        private static double GetMaximumWidth(LinedFlowLayoutState state, int index)
        {
            double maximum = state.MaxWidths.TryGetValue(index, out double itemMax)
                ? itemMax
                : state.UniformMaxWidth;
            return maximum >= 0.0 && !double.IsInfinity(maximum) && !double.IsNaN(maximum)
                ? maximum
                : double.MaxValue;
        }

        private static double GetPlannedWidth(LinedFlowLayoutState state, int index)
        {
            return index >= 0 && index < state.PlannedWidths.Length
                ? state.PlannedWidths[index]
                : 0.0;
        }

        private int GetLineIndexForOffset(LinedFlowLayoutState state, double offset)
        {
            if (state.LineStartIndices.Count == 0)
            {
                return -1;
            }

            int line = (int)Math.Floor(Math.Max(0.0, offset) / Math.Max(1.0, ActualLineHeight + LineSpacing));
            return Math.Max(0, Math.Min(state.LineStartIndices.Count - 1, line));
        }

        private static int GetLineStartIndex(LinedFlowLayoutState state, int lineIndex)
        {
            return lineIndex >= 0 && lineIndex < state.LineStartIndices.Count
                ? state.LineStartIndices[lineIndex]
                : -1;
        }

        private static LinedFlowLayoutState GetState(VirtualizingLayoutContext context)
        {
            return (LinedFlowLayoutState)context.LayoutState;
        }

        private void UnlockItems()
        {
            bool hadLocks = false;
            ForEachState(state =>
            {
                hadLocks |= state.LockedLines.Count > 0;
                state.LockedLines.Clear();
            });

            if (hadLocks)
            {
                ItemsUnlocked?.Invoke(this, null);
            }
        }

        private void ForEachState(Action<LinedFlowLayoutState> action)
        {
            for (int index = _states.Count - 1; index >= 0; index--)
            {
                if (_states[index].TryGetTarget(out LinedFlowLayoutState state))
                {
                    action(state);
                }
                else
                {
                    _states.RemoveAt(index);
                }
            }
        }
    }

    public enum LinedFlowLayoutItemsJustification
    {
        Start = 0,
        Center = 1,
        End = 2,
        SpaceAround = 3,
        SpaceBetween = 4,
        SpaceEvenly = 5
    }

    public enum LinedFlowLayoutItemsStretch
    {
        None = 0,
        Fill = 1
    }
}
