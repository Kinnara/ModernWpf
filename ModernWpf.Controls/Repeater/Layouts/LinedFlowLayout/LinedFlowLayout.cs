// Copyright (c) Kinnara. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;

namespace ModernWpf.Controls
{
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

    public partial class LinedFlowLayout : VirtualizingLayout
    {
        private const int ItemsInfoRequestBuffer = 32;

        public LinedFlowLayout()
        {
            LayoutId = nameof(LinedFlowLayout);
            SetIndexBasedLayoutOrientation(IndexBasedLayoutOrientation.TopToBottom);
        }

        public event TypedEventHandler<LinedFlowLayout, LinedFlowLayoutItemsInfoRequestedEventArgs> ItemsInfoRequested;

        public event TypedEventHandler<LinedFlowLayout, object> ItemsUnlocked;

        public int RequestedRangeStartIndex { get; private set; } = -1;

        public int RequestedRangeLength { get; private set; }

        public void InvalidateItemsInfo()
        {
            _state?.ClearItemsInfo();
            RequestedRangeStartIndex = -1;
            RequestedRangeLength = 0;
            InvalidateMeasure();
        }

        public int LockItemToLine(int itemIndex)
        {
            if (itemIndex < 0 || itemIndex >= _itemCount)
            {
                throw new ArgumentOutOfRangeException(nameof(itemIndex));
            }

            if (_lockedItemLines.TryGetValue(itemIndex, out int lockedLineIndex))
            {
                return lockedLineIndex;
            }

            if (_state == null || ActualLineHeight <= 0.0 ||
                !_state.ItemToLine.TryGetValue(itemIndex, out int lineIndex))
            {
                return -1;
            }

            _lockedItemLines[itemIndex] = lineIndex;
            return lineIndex;
        }

        protected override ItemCollectionTransitionProvider CreateDefaultItemTransitionProvider()
        {
            return new LinedFlowLayoutItemCollectionTransitionProvider();
        }

        protected override void InitializeForContextCore(VirtualizingLayoutContext context)
        {
            if (_isInitialized)
            {
                throw new InvalidOperationException("LinedFlowLayout cannot be shared by multiple layout contexts.");
            }

            if (context.LayoutState != null && !(context.LayoutState is LinedFlowLayoutState))
            {
                throw new InvalidOperationException("LayoutState must be a LinedFlowLayoutState.");
            }

            _state = context.LayoutState as LinedFlowLayoutState ?? new LinedFlowLayoutState();
            context.LayoutState = _state;
            _isInitialized = true;
        }

        protected override void UninitializeForContextCore(VirtualizingLayoutContext context)
        {
            if (context.LayoutState == _state)
            {
                context.LayoutState = null;
            }

            _state = null;
            _isInitialized = false;
            _itemCount = 0;
            RequestedRangeStartIndex = -1;
            RequestedRangeLength = 0;
            UnlockItems();
        }

        protected override Size MeasureOverride(VirtualizingLayoutContext context, Size availableSize)
        {
            var state = GetState(context);
            _itemCount = context.ItemCount;
            state.ItemCount = _itemCount;

            if (_itemCount == 0)
            {
                RecycleAll(context, state);
                state.ClearLayout();
                SetActualLineHeight(0.0);
                RequestedRangeStartIndex = -1;
                RequestedRangeLength = 0;
                context.LayoutOrigin = new Point();
                return new Size();
            }

            double availableWidth = NormalizeAvailableWidth(availableSize.Width);
            double actualLineHeight = UpdateActualLineHeight(context, availableSize, state);
            if (actualLineHeight <= 0.0 || double.IsNaN(actualLineHeight))
            {
                RecycleAll(context, state);
                state.ClearLayout();
                context.LayoutOrigin = new Point();
                return new Size(0.0, 0.0);
            }

            RequestItemsInfoForRealizationWindow(context, state, availableWidth, actualLineHeight);
            UpdateAverageItemsPerLine(state, availableWidth, actualLineHeight);
            BuildLines(state, availableWidth, actualLineHeight);

            var initialIndexes = GetRealizationIndexes(context, state, actualLineHeight);
            var preliminaryElements = RealizeAndMeasure(
                context,
                state,
                initialIndexes,
                actualLineHeight,
                measureForAspectRatio: true);

            UpdateAverageAspectRatio(state);
            UpdateAverageItemsPerLine(state, availableWidth, actualLineHeight);
            BuildLines(state, availableWidth, actualLineHeight);

            var finalIndexes = GetRealizationIndexes(context, state, actualLineHeight);
            RealizeAndMeasure(
                context,
                state,
                finalIndexes,
                actualLineHeight,
                measureForAspectRatio: false);

            foreach (var pair in preliminaryElements)
            {
                if (!finalIndexes.Contains(pair.Key))
                {
                    context.RecycleElement(pair.Value);
                    state.RealizedElements.Remove(pair.Key);
                }
            }

            RecycleOutsideRange(context, state, finalIndexes);
            ArrangeLines(state, availableWidth, actualLineHeight);

            context.LayoutOrigin = new Point();
            state.MeasuredWidth = availableWidth;

            double desiredWidth = state.Lines.Count == 0 ? 0.0 : state.Lines.Max(line => line.DesiredWidth);
            if (!double.IsPositiveInfinity(availableWidth))
            {
                desiredWidth = Math.Min(availableWidth, desiredWidth);
            }

            double desiredHeight = state.Lines.Count == 0
                ? 0.0
                : state.Lines.Count * actualLineHeight + (state.Lines.Count - 1) * LineSpacing;

            return new Size(Math.Max(0.0, desiredWidth), Math.Max(0.0, desiredHeight));
        }

        protected override Size ArrangeOverride(VirtualizingLayoutContext context, Size finalSize)
        {
            var state = GetState(context);
            double finalWidth = NormalizeAvailableWidth(finalSize.Width);
            ArrangeLines(state, finalWidth, ActualLineHeight);

            foreach (var pair in state.RealizedElements)
            {
                if (state.ArrangeBounds.TryGetValue(pair.Key, out Rect bounds))
                {
                    pair.Value.Arrange(bounds);
                }
            }

            return finalSize;
        }

        protected override void OnItemsChangedCore(
            VirtualizingLayoutContext context,
            object source,
            NotifyCollectionChangedEventArgs args)
        {
            if (context.LayoutState is LinedFlowLayoutState state)
            {
                state.ClearLayout();
                state.ClearItemsInfo();
            }

            _itemCount = context.ItemCount;
            RequestedRangeStartIndex = -1;
            RequestedRangeLength = 0;
            UnlockItems();
            InvalidateMeasure();
        }

        private static void OnPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var layout = (LinedFlowLayout)sender;
            if (args.Property != ActualLineHeightProperty)
            {
                layout._state?.ClearLayout();
                layout.InvalidateMeasure();
            }
        }

        private LinedFlowLayoutState GetState(VirtualizingLayoutContext context)
        {
            var state = context.LayoutState as LinedFlowLayoutState;
            if (state == null)
            {
                throw new InvalidOperationException("LinedFlowLayout has not been initialized for this context.");
            }

            return state;
        }

        private double UpdateActualLineHeight(
            VirtualizingLayoutContext context,
            Size availableSize,
            LinedFlowLayoutState state)
        {
            double value = LineHeight;
            if (double.IsNaN(value))
            {
                var element = context.GetOrCreateElementAt(0, ElementRealizationOptions.ForceCreate);
                double measureWidth = double.IsPositiveInfinity(availableSize.Width)
                    ? double.PositiveInfinity
                    : Math.Max(0.0, availableSize.Width);
                element.Measure(new Size(measureWidth, double.PositiveInfinity));
                value = element.DesiredSize.Height;

                if (value > 0.0 && element.DesiredSize.Width > 0.0)
                {
                    state.AspectRatios[0] = element.DesiredSize.Width / value;
                }

                state.RealizedElements[0] = element;
            }

            if (ActualLineHeight != value)
            {
                SetActualLineHeight(value);
            }

            return value;
        }

        private void RequestItemsInfoForRealizationWindow(
            VirtualizingLayoutContext context,
            LinedFlowLayoutState state,
            double availableWidth,
            double lineHeight)
        {
            int estimatedItemsPerLine = Math.Max(
                1,
                double.IsPositiveInfinity(availableWidth)
                    ? _itemCount
                    : (int)Math.Floor((availableWidth + MinItemSpacing) /
                        Math.Max(1.0, lineHeight * state.AverageAspectRatio + MinItemSpacing)));

            var realizationRect = context.RealizationRect;
            bool hasUnboundedRealizationHeight = IsUnboundedRealizationLength(realizationRect.Height);
            int firstLine = realizationRect.IsEmpty
                ? 0
                : Math.Max(0, (int)Math.Floor(realizationRect.Top / Math.Max(1.0, lineHeight + LineSpacing)) - 1);
            int visibleLineCount = realizationRect.IsEmpty || hasUnboundedRealizationHeight
                ? 1
                : Math.Max(1, (int)Math.Ceiling(realizationRect.Height / Math.Max(1.0, lineHeight + LineSpacing)) + 2);

            int start = hasUnboundedRealizationHeight
                ? 0
                : Math.Max(0, firstLine * estimatedItemsPerLine - ItemsInfoRequestBuffer);
            int length = hasUnboundedRealizationHeight
                ? _itemCount
                : Math.Min(
                    _itemCount - start,
                    Math.Max(ItemsInfoRequestBuffer * 2, visibleLineCount * estimatedItemsPerLine + ItemsInfoRequestBuffer * 2));

            if (length <= 0 || ItemsInfoRequested == null)
            {
                return;
            }

            var args = new LinedFlowLayoutItemsInfoRequestedEventArgs(start, length);
            ItemsInfoRequested(this, args);
            ApplyItemsInfo(state, args);

            if (args.DesiredAspectRatios != null)
            {
                RequestedRangeStartIndex = args.ItemsRangeStartIndex;
                RequestedRangeLength = args.DesiredAspectRatios.Length;
            }
            else
            {
                RequestedRangeStartIndex = -1;
                RequestedRangeLength = 0;
            }
        }

        private static void ApplyItemsInfo(
            LinedFlowLayoutState state,
            LinedFlowLayoutItemsInfoRequestedEventArgs args)
        {
            int length = args.EstablishedLength > 0 ? args.EstablishedLength : args.ItemsRangeRequestedLength;
            for (int offset = 0; offset < length; offset++)
            {
                int index = args.ItemsRangeStartIndex + offset;
                if (index < 0 || index >= state.ItemCount)
                {
                    continue;
                }

                if (args.DesiredAspectRatios != null && offset < args.DesiredAspectRatios.Length)
                {
                    double ratio = args.DesiredAspectRatios[offset];
                    if (ratio > 0.0 && !double.IsNaN(ratio) && !double.IsInfinity(ratio))
                    {
                        state.AspectRatios[index] = ratio;
                    }
                }

                double minWidth = args.MinWidths != null && offset < args.MinWidths.Length
                    ? args.MinWidths[offset]
                    : args.MinWidth;
                double maxWidth = args.MaxWidths != null && offset < args.MaxWidths.Length
                    ? args.MaxWidths[offset]
                    : args.MaxWidth;

                if (minWidth >= 0.0)
                {
                    state.MinWidths[index] = minWidth;
                }

                if (maxWidth >= 0.0)
                {
                    state.MaxWidths[index] = maxWidth;
                }
            }
        }

        private void BuildLines(LinedFlowLayoutState state, double availableWidth, double lineHeight)
        {
            state.Lines.Clear();
            state.ItemToLine.Clear();

            var line = CreateLine(0, lineHeight);
            double usedWidth = 0.0;
            var locks = _lockedItemLines.OrderBy(pair => pair.Key).ToArray();
            int nextLockOffset = 0;

            for (int index = 0; index < _itemCount; index++)
            {
                while (nextLockOffset < locks.Length && locks[nextLockOffset].Key < index)
                {
                    nextLockOffset++;
                }

                double width = GetDesiredWidth(state, index, lineHeight);
                bool hasItems = line.ItemIndexes.Count > 0;
                bool wouldOverflow = !double.IsPositiveInfinity(availableWidth) &&
                    hasItems &&
                    usedWidth + MinItemSpacing + width > availableWidth;
                bool mustWrapForLock = false;
                bool canWrapBeforeNextLock = true;
                if (nextLockOffset < locks.Length)
                {
                    var nextLock = locks[nextLockOffset];
                    int lineIncrementsNeeded = nextLock.Value - state.Lines.Count;
                    int itemsRemainingThroughLock = nextLock.Key - index + 1;
                    mustWrapForLock = hasItems &&
                        lineIncrementsNeeded > 0 &&
                        itemsRemainingThroughLock <= lineIncrementsNeeded;
                    canWrapBeforeNextLock = state.Lines.Count < nextLock.Value;
                }

                if (hasItems && (mustWrapForLock || (wouldOverflow && canWrapBeforeNextLock)))
                {
                    FinalizeLine(line, usedWidth);
                    state.Lines.Add(line);
                    line = CreateLine(state.Lines.Count, lineHeight);
                    usedWidth = 0.0;
                }

                if (line.ItemIndexes.Count > 0)
                {
                    usedWidth += MinItemSpacing;
                }

                line.ItemIndexes.Add(index);
                line.ItemWidths.Add(width);
                state.ItemToLine[index] = state.Lines.Count;
                usedWidth += width;
            }

            if (line.ItemIndexes.Count > 0)
            {
                FinalizeLine(line, usedWidth);
                state.Lines.Add(line);
            }
        }

        private LinedFlowLayoutLine CreateLine(int lineIndex, double lineHeight)
        {
            return new LinedFlowLayoutLine
            {
                Y = lineIndex * (lineHeight + LineSpacing)
            };
        }

        private static void FinalizeLine(LinedFlowLayoutLine line, double desiredWidth)
        {
            line.DesiredWidth = desiredWidth;
        }

        private double GetDesiredWidth(LinedFlowLayoutState state, int index, double lineHeight)
        {
            double ratio = state.AspectRatios.TryGetValue(index, out double knownRatio)
                ? knownRatio
                : state.AverageAspectRatio;
            double width = Math.Max(0.0, ratio * lineHeight);

            if (state.MinWidths.TryGetValue(index, out double minWidth))
            {
                width = Math.Max(width, minWidth);
            }

            if (state.MaxWidths.TryGetValue(index, out double maxWidth))
            {
                width = Math.Min(width, maxWidth);
            }

            return width;
        }

        private HashSet<int> GetRealizationIndexes(
            VirtualizingLayoutContext context,
            LinedFlowLayoutState state,
            double lineHeight)
        {
            var result = new HashSet<int>();
            if (state.Lines.Count == 0)
            {
                return result;
            }

            var realizationRect = context.RealizationRect;
            int firstLine;
            int lastLine;

            if (realizationRect.IsEmpty)
            {
                firstLine = 0;
                lastLine = 0;
            }
            else if (IsUnboundedRealizationLength(realizationRect.Height))
            {
                firstLine = 0;
                lastLine = state.Lines.Count - 1;
            }
            else
            {
                double linePitch = Math.Max(1.0, lineHeight + LineSpacing);
                firstLine = Math.Max(0, (int)Math.Floor(realizationRect.Top / linePitch) - 1);
                lastLine = Math.Min(state.Lines.Count - 1, (int)Math.Ceiling(realizationRect.Bottom / linePitch) + 1);
            }

            if (context.RecommendedAnchorIndex >= 0 &&
                state.ItemToLine.TryGetValue(context.RecommendedAnchorIndex, out int anchorLine))
            {
                firstLine = Math.Min(firstLine, anchorLine);
                lastLine = Math.Max(lastLine, anchorLine);
            }

            for (int lineIndex = firstLine; lineIndex <= lastLine; lineIndex++)
            {
                foreach (int index in state.Lines[lineIndex].ItemIndexes)
                {
                    result.Add(index);
                }
            }

            return result;
        }

        private Dictionary<int, UIElement> RealizeAndMeasure(
            VirtualizingLayoutContext context,
            LinedFlowLayoutState state,
            HashSet<int> indexes,
            double lineHeight,
            bool measureForAspectRatio)
        {
            var result = new Dictionary<int, UIElement>();
            foreach (int index in indexes.OrderBy(value => value))
            {
                var element = context.GetOrCreateElementAt(index);
                result[index] = element;
                state.RealizedElements[index] = element;

                if (measureForAspectRatio && !state.AspectRatios.ContainsKey(index))
                {
                    element.Measure(new Size(double.PositiveInfinity, lineHeight));
                    if (lineHeight > 0.0 && element.DesiredSize.Width > 0.0 &&
                        !double.IsInfinity(element.DesiredSize.Width))
                    {
                        state.AspectRatios[index] = element.DesiredSize.Width / lineHeight;
                    }
                }

                double width = GetDesiredWidth(state, index, lineHeight);
                element.Measure(new Size(width, lineHeight));
            }

            return result;
        }

        private static void UpdateAverageAspectRatio(LinedFlowLayoutState state)
        {
            if (state.AspectRatios.Count > 0)
            {
                state.AverageAspectRatio = state.AspectRatios.Values.Average();
            }
        }

        private void ArrangeLines(LinedFlowLayoutState state, double availableWidth, double lineHeight)
        {
            state.ArrangeBounds.Clear();
            foreach (var line in state.Lines)
            {
                var widths = new List<double>(line.ItemWidths);
                double spacing = MinItemSpacing;
                double desiredWidth = widths.Sum() + Math.Max(0, widths.Count - 1) * spacing;
                double remaining = double.IsPositiveInfinity(availableWidth)
                    ? 0.0
                    : Math.Max(0.0, availableWidth - desiredWidth);

                if (ItemsStretch == LinedFlowLayoutItemsStretch.Fill && remaining > 0.0)
                {
                    StretchLine(state, line, widths, remaining);
                    desiredWidth = widths.Sum() + Math.Max(0, widths.Count - 1) * spacing;
                    remaining = Math.Max(0.0, availableWidth - desiredWidth);
                }

                double x = 0.0;
                double extraGap = 0.0;
                switch (ItemsJustification)
                {
                    case LinedFlowLayoutItemsJustification.Center:
                        x = remaining / 2.0;
                        break;
                    case LinedFlowLayoutItemsJustification.End:
                        x = remaining;
                        break;
                    case LinedFlowLayoutItemsJustification.SpaceAround:
                        extraGap = widths.Count > 0 ? remaining / widths.Count : 0.0;
                        x = extraGap / 2.0;
                        break;
                    case LinedFlowLayoutItemsJustification.SpaceBetween:
                        extraGap = widths.Count > 1 ? remaining / (widths.Count - 1) : 0.0;
                        break;
                    case LinedFlowLayoutItemsJustification.SpaceEvenly:
                        extraGap = widths.Count > 0 ? remaining / (widths.Count + 1) : 0.0;
                        x = extraGap;
                        break;
                }

                for (int itemOffset = 0; itemOffset < line.ItemIndexes.Count; itemOffset++)
                {
                    int index = line.ItemIndexes[itemOffset];
                    state.ArrangeBounds[index] = new Rect(x, line.Y, widths[itemOffset], lineHeight);
                    x += widths[itemOffset] + spacing + extraGap;
                }

                line.DesiredWidth = desiredWidth;
            }
        }

        private static void StretchLine(
            LinedFlowLayoutState state,
            LinedFlowLayoutLine line,
            IList<double> widths,
            double remaining)
        {
            var expandable = new HashSet<int>(Enumerable.Range(0, widths.Count));
            while (remaining > 0.01 && expandable.Count > 0)
            {
                double share = remaining / expandable.Count;
                double consumed = 0.0;
                foreach (int offset in expandable.ToArray())
                {
                    int itemIndex = line.ItemIndexes[offset];
                    double proposed = widths[offset] + share;
                    if (state.MaxWidths.TryGetValue(itemIndex, out double maxWidth) && proposed >= maxWidth)
                    {
                        consumed += Math.Max(0.0, maxWidth - widths[offset]);
                        widths[offset] = maxWidth;
                        expandable.Remove(offset);
                    }
                    else
                    {
                        widths[offset] = proposed;
                        consumed += share;
                    }
                }

                if (consumed <= 0.0)
                {
                    break;
                }

                remaining -= consumed;
            }
        }

        private void UpdateAverageItemsPerLine(
            LinedFlowLayoutState state,
            double availableWidth,
            double lineHeight)
        {
            var lineItemCounts = new List<int>();
            int currentLineItemCount = 0;
            double usedWidth = 0.0;
            for (int index = 0; index < _itemCount; index++)
            {
                double width = GetDesiredWidth(state, index, lineHeight);
                bool wouldOverflow = !double.IsPositiveInfinity(availableWidth) &&
                    currentLineItemCount > 0 &&
                    usedWidth + MinItemSpacing + width > availableWidth;
                if (wouldOverflow)
                {
                    lineItemCounts.Add(currentLineItemCount);
                    currentLineItemCount = 0;
                    usedWidth = 0.0;
                }

                if (currentLineItemCount > 0)
                {
                    usedWidth += MinItemSpacing;
                }

                usedWidth += width;
                currentLineItemCount++;
            }

            if (currentLineItemCount > 0)
            {
                lineItemCounts.Add(currentLineItemCount);
            }

            double unlockedAverage = GetAverageItemsPerLine(lineItemCounts);
            if (_lockedItemLines.Count > 0 &&
                state.AverageItemsPerLine > 0.0 &&
                Math.Abs(state.AverageItemsPerLine - unlockedAverage) > 0.1)
            {
                UnlockItems();
            }

            state.AverageItemsPerLine = unlockedAverage;
        }

        private static double GetAverageItemsPerLine(IList<int> lineItemCounts)
        {
            return lineItemCounts.Count == 0
                ? 0.0
                : lineItemCounts.Count == 1
                    ? lineItemCounts[0]
                    : (double)lineItemCounts.Take(lineItemCounts.Count - 1).Sum() /
                        (lineItemCounts.Count - 1);
        }

        private static void RecycleAll(VirtualizingLayoutContext context, LinedFlowLayoutState state)
        {
            foreach (var element in state.RealizedElements.Values.ToArray())
            {
                context.RecycleElement(element);
            }

            state.RealizedElements.Clear();
        }

        private static void RecycleOutsideRange(
            VirtualizingLayoutContext context,
            LinedFlowLayoutState state,
            HashSet<int> indexes)
        {
            foreach (var pair in state.RealizedElements.ToArray())
            {
                if (!indexes.Contains(pair.Key))
                {
                    context.RecycleElement(pair.Value);
                    state.RealizedElements.Remove(pair.Key);
                }
            }
        }

        private void UnlockItems()
        {
            if (_lockedItemLines.Count > 0)
            {
                _lockedItemLines.Clear();
                ItemsUnlocked?.Invoke(this, null);
            }
        }

        private static double NormalizeAvailableWidth(double value)
        {
            if (double.IsNaN(value))
            {
                return 0.0;
            }

            return double.IsPositiveInfinity(value) ? value : Math.Max(0.0, value);
        }

        private static bool IsUnboundedRealizationLength(double value)
        {
            // ItemsRepeater uses double.MaxValue, rather than positive infinity,
            // when no scrolling surface constrains the realization window.
            return double.IsPositiveInfinity(value) || value >= int.MaxValue;
        }

        private readonly Dictionary<int, int> _lockedItemLines = new Dictionary<int, int>();
        private LinedFlowLayoutState _state;
        private int _itemCount;
        private bool _isInitialized;
    }
}
