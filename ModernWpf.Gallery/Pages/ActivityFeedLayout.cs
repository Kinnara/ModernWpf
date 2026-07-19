using System;
using System.Collections.Generic;
using System.Windows;
using ModernWpf.Controls;

namespace ModernWpf.Gallery.Pages
{
    /// <summary>
    /// WPF port of the custom virtualizing layout used by the WinUI Gallery
    /// ItemsRepeater activity-feed sample.
    /// </summary>
    internal sealed class ActivityFeedLayout : VirtualizingLayout
    {
        private double _rowSpacing;
        private double _columnSpacing;
        private Size _minItemSize = Size.Empty;

        public static readonly DependencyProperty RowSpacingProperty = DependencyProperty.Register(
            nameof(RowSpacing),
            typeof(double),
            typeof(ActivityFeedLayout),
            new PropertyMetadata(0.0, OnLayoutPropertyChanged));

        public static readonly DependencyProperty ColumnSpacingProperty = DependencyProperty.Register(
            nameof(ColumnSpacing),
            typeof(double),
            typeof(ActivityFeedLayout),
            new PropertyMetadata(0.0, OnLayoutPropertyChanged));

        public static readonly DependencyProperty MinItemSizeProperty = DependencyProperty.Register(
            nameof(MinItemSize),
            typeof(Size),
            typeof(ActivityFeedLayout),
            new PropertyMetadata(Size.Empty, OnLayoutPropertyChanged));

        public double RowSpacing
        {
            get => (double)GetValue(RowSpacingProperty);
            set => SetValue(RowSpacingProperty, value);
        }

        public double ColumnSpacing
        {
            get => (double)GetValue(ColumnSpacingProperty);
            set => SetValue(ColumnSpacingProperty, value);
        }

        public Size MinItemSize
        {
            get => (Size)GetValue(MinItemSizeProperty);
            set => SetValue(MinItemSizeProperty, value);
        }

        protected override void InitializeForContextCore(VirtualizingLayoutContext context)
        {
            base.InitializeForContextCore(context);
            if (!(context.LayoutState is ActivityFeedLayoutState))
            {
                context.LayoutState = new ActivityFeedLayoutState();
            }
        }

        protected override void UninitializeForContextCore(VirtualizingLayoutContext context)
        {
            base.UninitializeForContextCore(context);
            context.LayoutState = null;
        }

        protected override Size MeasureOverride(VirtualizingLayoutContext context, Size availableSize)
        {
            if (context.ItemCount == 0)
            {
                return default;
            }

            if (_minItemSize == Size.Empty)
            {
                var firstElement = context.GetOrCreateElementAt(0);
                firstElement.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                _minItemSize = firstElement.DesiredSize;
            }

            var rowHeight = _minItemSize.Height + _rowSpacing;
            var rowCount = context.ItemCount / 3;
            var firstRowIndex = Math.Max((int)(context.RealizationRect.Y / rowHeight) - 1, 0);
            var lastRowIndex = Math.Min(
                (int)(context.RealizationRect.Bottom / rowHeight) + 1,
                rowCount);

            if (!(context.LayoutState is ActivityFeedLayoutState state))
            {
                throw new InvalidOperationException("LayoutState is not an ActivityFeedLayoutState.");
            }

            state.LayoutRects.Clear();
            state.FirstRealizedIndex = firstRowIndex * 3;

            var desiredItemWidth = Math.Max(
                _minItemSize.Width,
                (availableSize.Width - (_columnSpacing * 3)) / 4);

            for (var rowIndex = firstRowIndex; rowIndex < lastRowIndex; rowIndex++)
            {
                var firstItemIndex = rowIndex * 3;
                var boundsForCurrentRow = CalculateLayoutBoundsForRow(rowIndex, desiredItemWidth);

                for (var columnIndex = 0; columnIndex < 3; columnIndex++)
                {
                    var index = firstItemIndex + columnIndex;
                    var container = context.GetOrCreateElementAt(index);
                    var bounds = boundsForCurrentRow[columnIndex];
                    container.Measure(new Size(bounds.Width, bounds.Height));
                    state.LayoutRects.Add(bounds);
                }
            }

            var extentHeight = rowCount == 0
                ? 0
                : ((rowCount - 1) * rowHeight) + _minItemSize.Height;
            return new Size((desiredItemWidth * 4) + (_columnSpacing * 2), extentHeight);
        }

        protected override Size ArrangeOverride(VirtualizingLayoutContext context, Size finalSize)
        {
            if (!(context.LayoutState is ActivityFeedLayoutState state))
            {
                throw new InvalidOperationException("LayoutState is not an ActivityFeedLayoutState.");
            }

            var currentIndex = state.FirstRealizedIndex;
            foreach (var arrangeRect in state.LayoutRects)
            {
                context.GetOrCreateElementAt(currentIndex).Arrange(arrangeRect);
                currentIndex++;
            }

            return finalSize;
        }

        private static void OnLayoutPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var layout = (ActivityFeedLayout)sender;
            if (args.Property == RowSpacingProperty)
            {
                layout._rowSpacing = (double)args.NewValue;
            }
            else if (args.Property == ColumnSpacingProperty)
            {
                layout._columnSpacing = (double)args.NewValue;
            }
            else if (args.Property == MinItemSizeProperty)
            {
                layout._minItemSize = (Size)args.NewValue;
            }

            layout.InvalidateMeasure();
        }

        private Rect[] CalculateLayoutBoundsForRow(int rowIndex, double desiredItemWidth)
        {
            var boundsForRow = new Rect[3];
            var yOffset = rowIndex * (_minItemSize.Height + _rowSpacing);

            for (var index = 0; index < boundsForRow.Length; index++)
            {
                boundsForRow[index].Y = yOffset;
                boundsForRow[index].Height = _minItemSize.Height;
            }

            if (rowIndex % 2 == 0)
            {
                boundsForRow[0].X = 0;
                boundsForRow[0].Width = desiredItemWidth;
                boundsForRow[1].X = boundsForRow[0].Right + _columnSpacing;
                boundsForRow[1].Width = desiredItemWidth;
                boundsForRow[2].X = boundsForRow[1].Right + _columnSpacing;
                boundsForRow[2].Width = (desiredItemWidth * 2) + _columnSpacing;
            }
            else
            {
                boundsForRow[0].X = 0;
                boundsForRow[0].Width = (desiredItemWidth * 2) + _columnSpacing;
                boundsForRow[1].X = boundsForRow[0].Right + _columnSpacing;
                boundsForRow[1].Width = desiredItemWidth;
                boundsForRow[2].X = boundsForRow[1].Right + _columnSpacing;
                boundsForRow[2].Width = desiredItemWidth;
            }

            return boundsForRow;
        }

        private sealed class ActivityFeedLayoutState
        {
            public int FirstRealizedIndex { get; set; }

            public List<Rect> LayoutRects { get; } = new List<Rect>();
        }
    }
}
