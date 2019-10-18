// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.Controls
{
    public class UniformGridLayoutState
    {
        internal void InitializeForContext(
            VirtualizingLayoutContext context,
            IFlowLayoutAlgorithmDelegates callbacks)
        {
            FlowAlgorithm.InitializeForContext(context, callbacks);
            context.LayoutStateCore = this;
        }

        internal void UninitializeForContext(VirtualizingLayoutContext context)
        {
            FlowAlgorithm.UninitializeForContext(context);

            if (m_cachedFirstElement != null)
            {
                context.RecycleElement(m_cachedFirstElement);
            }
        }

        internal FlowLayoutAlgorithm FlowAlgorithm { get; } = new FlowLayoutAlgorithm();
        internal double EffectiveItemWidth { get; private set; } = 0.0;
        internal double EffectiveItemHeight { get; private set; } = 0.0;

        internal void EnsureElementSize(
            Size availableSize,
            VirtualizingLayoutContext context,
            double layoutItemWidth,
            double LayoutItemHeight,
            UniformGridLayoutItemsStretch stretch,
            Orientation orientation,
            double minRowSpacing,
            double minColumnSpacing,
            uint maxItemsPerLine)
        {
            if (maxItemsPerLine == 0)
            {
                maxItemsPerLine = 1;
            }

            if (context.ItemCount > 0)
            {
                // If the first element is realized we don't need to cache it or to get it from the context
                var realizedElement = FlowAlgorithm.GetElementIfRealized(0);
                if (realizedElement != null)
                {
                    realizedElement.Measure(availableSize);
                    SetSize(realizedElement, layoutItemWidth, LayoutItemHeight, availableSize, stretch, orientation, minRowSpacing, minColumnSpacing, maxItemsPerLine);
                    m_cachedFirstElement = null;
                }
                else
                {
                    if (m_cachedFirstElement == null)
                    {
                        // we only cache if we aren't realizing it
                        m_cachedFirstElement = context.GetOrCreateElementAt(0, ElementRealizationOptions.ForceCreate | ElementRealizationOptions.SuppressAutoRecycle); // expensive
                    }

                    m_cachedFirstElement.Measure(availableSize);
                    SetSize(m_cachedFirstElement, layoutItemWidth, LayoutItemHeight, availableSize, stretch, orientation, minRowSpacing, minColumnSpacing, maxItemsPerLine);

                    // See if we can move ownership to the flow algorithm. If we can, we do not need a local cache.
                    bool added = FlowAlgorithm.TryAddElement0(m_cachedFirstElement);
                    if (added)
                    {
                        m_cachedFirstElement = null;
                    }
                }
            }
        }

        private void SetSize(UIElement UIElement,
            double layoutItemWidth,
            double LayoutItemHeight,
            Size availableSize,
            UniformGridLayoutItemsStretch stretch,
            Orientation orientation,
            double minRowSpacing,
            double minColumnSpacing,
            uint maxItemsPerLine)
        {
            if (maxItemsPerLine == 0)
            {
                maxItemsPerLine = 1;
            }

            EffectiveItemWidth = double.IsNaN(layoutItemWidth) ? UIElement.DesiredSize.Width : layoutItemWidth;
            EffectiveItemHeight = double.IsNaN(LayoutItemHeight) ? UIElement.DesiredSize.Height : LayoutItemHeight;

            var availableSizeMinor = orientation == Orientation.Horizontal ? availableSize.Width : availableSize.Height;
            var minorItemSpacing = orientation == Orientation.Vertical ? minRowSpacing : minColumnSpacing;

            var itemSizeMinor = orientation == Orientation.Horizontal ? EffectiveItemWidth : EffectiveItemHeight;

            double extraMinorPixelsForEachItem = 0.0;
            if (!double.IsInfinity(availableSizeMinor))
            {
                var numItemsPerColumn = Math.Min(
                    maxItemsPerLine,
                    (uint)Math.Max(1.0, availableSizeMinor / (itemSizeMinor + minorItemSpacing)));
                var usedSpace = (numItemsPerColumn * (itemSizeMinor + minorItemSpacing)) - minorItemSpacing;
                var remainingSpace = ((int)(availableSizeMinor - usedSpace));
                extraMinorPixelsForEachItem = remainingSpace / ((int)numItemsPerColumn);
            }

            if (stretch == UniformGridLayoutItemsStretch.Fill)
            {
                if (orientation == Orientation.Horizontal)
                {
                    EffectiveItemWidth += extraMinorPixelsForEachItem;
                }
                else
                {
                    EffectiveItemHeight += extraMinorPixelsForEachItem;
                }
            }
            else if (stretch == UniformGridLayoutItemsStretch.Uniform)
            {
                var itemSizeMajor = orientation == Orientation.Horizontal ? EffectiveItemHeight : EffectiveItemWidth;
                var extraMajorPixelsForEachItem = itemSizeMajor * (extraMinorPixelsForEachItem / itemSizeMinor);
                if (orientation == Orientation.Horizontal)
                {
                    EffectiveItemWidth += extraMinorPixelsForEachItem;
                    EffectiveItemHeight += extraMajorPixelsForEachItem;
                }
                else
                {
                    EffectiveItemHeight += extraMinorPixelsForEachItem;
                    EffectiveItemWidth += extraMajorPixelsForEachItem;
                }
            }
        }

        // If it's realized then we shouldn't be caching it
        internal void EnsureFirstElementOwnership(VirtualizingLayoutContext context)
        {
            if (m_cachedFirstElement != null && FlowAlgorithm.GetElementIfRealized(0) != null)
            {
                // We created the element, but then flowlayout algorithm took ownership, so we can clear it and
                // let flowlayout algorithm do its thing.
                context.RecycleElement(m_cachedFirstElement);
                m_cachedFirstElement = null;
            }
        }

        internal void ClearElementOnDataSourceChange(VirtualizingLayoutContext context, NotifyCollectionChangedEventArgs args)
        {
            if (m_cachedFirstElement != null)
            {
                bool shouldClear = false;
                switch (args.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        shouldClear = args.NewStartingIndex == 0;
                        break;

                    case NotifyCollectionChangedAction.Replace:
                        shouldClear = args.NewStartingIndex == 0 || args.OldStartingIndex == 0;
                        break;

                    case NotifyCollectionChangedAction.Remove:
                        shouldClear = args.OldStartingIndex == 0;
                        break;

                    case NotifyCollectionChangedAction.Reset:
                        shouldClear = true;
                        break;

                    case NotifyCollectionChangedAction.Move:
                        throw new NotImplementedException();
                }

                if (shouldClear)
                {
                    context.RecycleElement(m_cachedFirstElement);
                    m_cachedFirstElement = null;
                }
            }
        }

        // We need to measure the element at index 0 to know what size to measure all other items.
        // If FlowlayoutAlgorithm has already realized element 0 then we can use that.
        // If it does not, then we need to do context.GetElement(0) at which point we have requested an element and are on point to clear it.
        // If we are responsible for clearing element 0 we keep m_cachedFirstElement valid.
        // If we are not (because FlowLayoutAlgorithm is holding it for us) then we just null out this field and use the one from FlowLayoutAlgorithm.
        private UIElement m_cachedFirstElement = null;
    }
}
