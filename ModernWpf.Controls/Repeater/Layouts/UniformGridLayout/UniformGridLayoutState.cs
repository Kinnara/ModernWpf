// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
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
            ((ILayoutContextOverrides)context).LayoutStateCore = this;
        }

        internal void UninitializeForContext(VirtualizingLayoutContext context)
        {
            FlowAlgorithm.UninitializeForContext(context);
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
                // If the first element is realized we don't need to get it from the context
                var realizedElement = FlowAlgorithm.GetElementIfRealized(0);
                if (realizedElement != null)
                {
                    realizedElement.Measure(availableSize);
                    SetSize(realizedElement.DesiredSize, layoutItemWidth, LayoutItemHeight, availableSize, stretch, orientation, minRowSpacing, minColumnSpacing, maxItemsPerLine);
                }
                else
                {
                    // Not realized by flowlayout, so do this now!
                    if (context.GetOrCreateElementAt(0, ElementRealizationOptions.ForceCreate) is { } firstElement)
                    {
                        firstElement.Measure(availableSize);
                        SetSize(firstElement.DesiredSize, layoutItemWidth, LayoutItemHeight, availableSize, stretch, orientation, minRowSpacing, minColumnSpacing, maxItemsPerLine);
                        context.RecycleElement(firstElement);
                    }
                }
            }
        }

        private void SetSize(Size desiredItemSize,
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

            EffectiveItemWidth = double.IsNaN(layoutItemWidth) ? desiredItemSize.Width : layoutItemWidth;
            EffectiveItemHeight = double.IsNaN(LayoutItemHeight) ? desiredItemSize.Height : LayoutItemHeight;

            var availableSizeMinor = orientation == Orientation.Horizontal ? availableSize.Width : availableSize.Height;
            var minorItemSpacing = orientation == Orientation.Vertical ? minRowSpacing : minColumnSpacing;

            var itemSizeMinor = orientation == Orientation.Horizontal ? EffectiveItemWidth : EffectiveItemHeight;

            double extraMinorPixelsForEachItem = 0.0;
            if (!double.IsInfinity(availableSizeMinor))
            {
                var numItemsPerColumn = Math.Min(
                    maxItemsPerLine,
                    (uint)Math.Max(1.0, availableSizeMinor / (itemSizeMinor + minorItemSpacing)));

                if (numItemsPerColumn == 0)
                {
                    numItemsPerColumn = 1;
                }

                var usedSpace = (numItemsPerColumn * (itemSizeMinor + minorItemSpacing)) - minorItemSpacing;
                var remainingSpace = (int)(availableSizeMinor - usedSpace);
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
    }
}
