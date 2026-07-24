using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using ModernWpf.Controls;

namespace ModernWpf.Gallery.Pages
{
    /// <summary>
    /// WPF port of the masonry-style virtualizing layout used by the WinUI
    /// Gallery's content-heavy ItemsRepeater sample.
    /// </summary>
    internal sealed class VariedImageSizeLayout : VirtualizingLayout
    {
        private int _firstIndex;
        private int _lastIndex;
        private double _lastAvailableWidth;
        private readonly List<double> _columnOffsets = new List<double>();
        private readonly List<Rect> _cachedBounds = new List<Rect>();
        private bool _cachedBoundsInvalid;

        public double Width { get; set; } = 150;

        protected override void OnItemsChangedCore(
            VirtualizingLayoutContext context,
            object source,
            NotifyCollectionChangedEventArgs args)
        {
            _cachedBounds.Clear();
            _firstIndex = 0;
            _lastIndex = 0;
            _cachedBoundsInvalid = true;
            InvalidateMeasure();
        }

        protected override Size MeasureOverride(VirtualizingLayoutContext context, Size availableSize)
        {
            var viewport = context.RealizationRect;
            if (availableSize.Width != _lastAvailableWidth || _cachedBoundsInvalid)
            {
                UpdateCachedBounds(availableSize);
                _lastAvailableWidth = availableSize.Width;
            }

            var columnCount = Math.Max(1, (int)(availableSize.Width / Width));
            if (_columnOffsets.Count == 0)
            {
                for (var index = 0; index < columnCount; index++)
                {
                    _columnOffsets.Add(0);
                }
            }

            _firstIndex = GetStartIndex(viewport);
            var currentIndex = _firstIndex;
            var nextOffset = -1.0;

            while (currentIndex < context.ItemCount && nextOffset < viewport.Bottom)
            {
                var child = context.GetOrCreateElementAt(currentIndex);
                child.Measure(new Size(Width, availableSize.Height));

                if (currentIndex >= _cachedBounds.Count)
                {
                    var columnIndex = GetIndexOfLowestColumn(_columnOffsets, out nextOffset);
                    _cachedBounds.Add(new Rect(
                        columnIndex * Width,
                        nextOffset,
                        Width,
                        child.DesiredSize.Height));
                    _columnOffsets[columnIndex] += child.DesiredSize.Height;
                }
                else if (currentIndex + 1 == _cachedBounds.Count)
                {
                    GetIndexOfLowestColumn(_columnOffsets, out nextOffset);
                }
                else
                {
                    nextOffset = _cachedBounds[currentIndex + 1].Top;
                }

                _lastIndex = currentIndex;
                currentIndex++;
            }

            return GetExtentSize(availableSize);
        }

        protected override Size ArrangeOverride(VirtualizingLayoutContext context, Size finalSize)
        {
            if (_cachedBounds.Count > 0)
            {
                for (var index = _firstIndex; index <= _lastIndex; index++)
                {
                    context.GetOrCreateElementAt(index).Arrange(_cachedBounds[index]);
                }
            }

            return finalSize;
        }

        private void UpdateCachedBounds(Size availableSize)
        {
            var columnCount = Math.Max(1, (int)(availableSize.Width / Width));
            _columnOffsets.Clear();
            for (var index = 0; index < columnCount; index++)
            {
                _columnOffsets.Add(0);
            }

            for (var index = 0; index < _cachedBounds.Count; index++)
            {
                var columnIndex = GetIndexOfLowestColumn(_columnOffsets, out var nextOffset);
                var oldHeight = _cachedBounds[index].Height;
                _cachedBounds[index] = new Rect(columnIndex * Width, nextOffset, Width, oldHeight);
                _columnOffsets[columnIndex] += oldHeight;
            }

            _cachedBoundsInvalid = false;
        }

        private int GetStartIndex(Rect viewport)
        {
            if (_cachedBounds.Count == 0)
            {
                return 0;
            }

            for (var index = 0; index < _cachedBounds.Count; index++)
            {
                var bounds = _cachedBounds[index];
                if (bounds.Y < viewport.Bottom && bounds.Bottom > viewport.Top)
                {
                    return index;
                }
            }

            return 0;
        }

        private static int GetIndexOfLowestColumn(List<double> columnOffsets, out double lowestOffset)
        {
            var lowestIndex = 0;
            lowestOffset = columnOffsets[0];
            for (var index = 0; index < columnOffsets.Count; index++)
            {
                var currentOffset = columnOffsets[index];
                if (lowestOffset > currentOffset)
                {
                    lowestOffset = currentOffset;
                    lowestIndex = index;
                }
            }

            return lowestIndex;
        }

        private Size GetExtentSize(Size availableSize)
        {
            var largestColumnOffset = _columnOffsets.Count == 0 ? 0 : _columnOffsets[0];
            for (var index = 0; index < _columnOffsets.Count; index++)
            {
                largestColumnOffset = Math.Max(largestColumnOffset, _columnOffsets[index]);
            }

            return new Size(availableSize.Width, largestColumnOffset);
        }
    }
}
