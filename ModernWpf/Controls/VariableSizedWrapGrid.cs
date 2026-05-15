using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using ModernWpf.Media.Animation;
using WpfOrientation = System.Windows.Controls.Orientation;

namespace ModernWpf.Controls
{
    [ContentProperty(nameof(Children))]
    public class VariableSizedWrapGrid : Panel
    {
        static VariableSizedWrapGrid()
        {
            BackgroundProperty.OverrideMetadata(
                typeof(VariableSizedWrapGrid),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    OnBackgroundPropertyChanged));
        }

        public static readonly DependencyProperty ItemHeightProperty =
            DependencyProperty.Register(
                nameof(ItemHeight),
                typeof(double),
                typeof(VariableSizedWrapGrid),
                new FrameworkPropertyMetadata(
                    double.NaN,
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.AffectsArrange),
                IsValidItemSize);

        public double ItemHeight
        {
            get => (double)GetValue(ItemHeightProperty);
            set => SetValue(ItemHeightProperty, value);
        }

        public static readonly DependencyProperty ItemWidthProperty =
            DependencyProperty.Register(
                nameof(ItemWidth),
                typeof(double),
                typeof(VariableSizedWrapGrid),
                new FrameworkPropertyMetadata(
                    double.NaN,
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.AffectsArrange),
                IsValidItemSize);

        public double ItemWidth
        {
            get => (double)GetValue(ItemWidthProperty);
            set => SetValue(ItemWidthProperty, value);
        }

        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register(
                nameof(Orientation),
                typeof(WpfOrientation),
                typeof(VariableSizedWrapGrid),
                new FrameworkPropertyMetadata(
                    WpfOrientation.Vertical,
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.AffectsArrange));

        public WpfOrientation Orientation
        {
            get => (WpfOrientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        public static readonly DependencyProperty HorizontalChildrenAlignmentProperty =
            DependencyProperty.Register(
                nameof(HorizontalChildrenAlignment),
                typeof(HorizontalAlignment),
                typeof(VariableSizedWrapGrid),
                new FrameworkPropertyMetadata(
                    HorizontalAlignment.Left,
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.AffectsArrange));

        public HorizontalAlignment HorizontalChildrenAlignment
        {
            get => (HorizontalAlignment)GetValue(HorizontalChildrenAlignmentProperty);
            set => SetValue(HorizontalChildrenAlignmentProperty, value);
        }

        public static readonly DependencyProperty VerticalChildrenAlignmentProperty =
            DependencyProperty.Register(
                nameof(VerticalChildrenAlignment),
                typeof(VerticalAlignment),
                typeof(VariableSizedWrapGrid),
                new FrameworkPropertyMetadata(
                    VerticalAlignment.Top,
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.AffectsArrange));

        public VerticalAlignment VerticalChildrenAlignment
        {
            get => (VerticalAlignment)GetValue(VerticalChildrenAlignmentProperty);
            set => SetValue(VerticalChildrenAlignmentProperty, value);
        }

        public static readonly DependencyProperty MaximumRowsOrColumnsProperty =
            DependencyProperty.Register(
                nameof(MaximumRowsOrColumns),
                typeof(int),
                typeof(VariableSizedWrapGrid),
                new FrameworkPropertyMetadata(
                    -1,
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.AffectsArrange));

        public int MaximumRowsOrColumns
        {
            get => (int)GetValue(MaximumRowsOrColumnsProperty);
            set => SetValue(MaximumRowsOrColumnsProperty, value);
        }

        public static readonly DependencyProperty RowSpanProperty =
            DependencyProperty.RegisterAttached(
                "RowSpan",
                typeof(int),
                typeof(VariableSizedWrapGrid),
                new FrameworkPropertyMetadata(
                    1,
                    FrameworkPropertyMetadataOptions.AffectsParentMeasure |
                    FrameworkPropertyMetadataOptions.AffectsParentArrange,
                    OnChildLayoutPropertyChanged));

        public static int GetRowSpan(UIElement element) => (int)element.GetValue(RowSpanProperty);

        public static void SetRowSpan(UIElement element, int value) => element.SetValue(RowSpanProperty, value);

        public static readonly DependencyProperty ColumnSpanProperty =
            DependencyProperty.RegisterAttached(
                "ColumnSpan",
                typeof(int),
                typeof(VariableSizedWrapGrid),
                new FrameworkPropertyMetadata(
                    1,
                    FrameworkPropertyMetadataOptions.AffectsParentMeasure |
                    FrameworkPropertyMetadataOptions.AffectsParentArrange,
                    OnChildLayoutPropertyChanged));

        public static int GetColumnSpan(UIElement element) => (int)element.GetValue(ColumnSpanProperty);

        public static void SetColumnSpan(UIElement element, int value) => element.SetValue(ColumnSpanProperty, value);

        public static readonly DependencyProperty BackgroundTransitionProperty =
            DependencyProperty.Register(
                nameof(BackgroundTransition),
                typeof(BrushTransition),
                typeof(VariableSizedWrapGrid),
                new PropertyMetadata(null, OnBackgroundTransitionPropertyChanged));

        public BrushTransition BackgroundTransition
        {
            get => (BrushTransition)GetValue(BackgroundTransitionProperty);
            set => SetValue(BackgroundTransitionProperty, value);
        }

        public static readonly DependencyProperty ChildrenTransitionsProperty =
            DependencyProperty.Register(
                nameof(ChildrenTransitions),
                typeof(TransitionCollection),
                typeof(VariableSizedWrapGrid),
                new PropertyMetadata(null));

        public TransitionCollection ChildrenTransitions
        {
            get => (TransitionCollection)GetValue(ChildrenTransitionsProperty);
            set => SetValue(ChildrenTransitionsProperty, value);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var children = InternalChildren;
            if (children.Count == 0)
            {
                _layoutState = LayoutState.Empty;
                return new Size();
            }

            var itemSize = ComputeItemSize(availableSize);
            var layoutState = BuildLayout(itemSize, availableSize);

            for (int i = 0; i < children.Count; i++)
            {
                if (children[i] is UIElement child)
                {
                    child.Measure(GetChildMeasureSize(itemSize, child));
                }
            }

            _layoutState = layoutState;
            return ComputePanelSize(layoutState, availableSize);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var layoutState = _layoutState;
            if (layoutState == null || layoutState.Slots.Count != InternalChildren.Count)
            {
                layoutState = BuildLayout(ComputeItemSize(finalSize), finalSize);
                _layoutState = layoutState;
            }

            var alignment = ComputeAlignment(layoutState, finalSize);

            for (int i = 0; i < InternalChildren.Count && i < layoutState.Slots.Count; i++)
            {
                if (InternalChildren[i] is UIElement child)
                {
                    var slot = layoutState.Slots[i];
                    var column = layoutState.Columns[i];
                    var row = layoutState.Rows[i];
                    var arrangedSlot = new Rect(
                        slot.X + alignment.StartX + alignment.GapX * (column + 1),
                        slot.Y + alignment.StartY + alignment.GapY * (row + 1),
                        slot.Width + alignment.GapX * Math.Max(0, GetColumnSpan(child) - 1),
                        slot.Height + alignment.GapY * Math.Max(0, GetRowSpan(child) - 1));

                    child.Arrange(arrangedSlot);
                }
            }

            return finalSize;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            var background = EffectiveBackground;
            if (background != null)
            {
                drawingContext.DrawRectangle(background, null, new Rect(RenderSize));
            }
        }

        internal Brush EffectiveBackground => _backgroundTransitionHelper?.GetEffectiveBrush(Background) ?? Background;

        private static bool IsValidItemSize(object value)
        {
            return value is double size && (double.IsNaN(size) || (size >= 0.0 && !double.IsInfinity(size)));
        }

        private static void OnChildLayoutPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element &&
                (VisualTreeHelper.GetParent(element) ?? LogicalTreeHelper.GetParent(element)) is VariableSizedWrapGrid panel)
            {
                panel.InvalidateMeasure();
            }
        }

        private static void OnBackgroundPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = (VariableSizedWrapGrid)d;
            if (grid.BackgroundTransition != null || grid._backgroundTransitionHelper?.IsTransitioning == true)
            {
                grid.BackgroundTransitionHelper.OnBrushChanged(
                    (Brush)e.OldValue,
                    (Brush)e.NewValue,
                    grid.BackgroundTransition);
            }
        }

        private static void OnBackgroundTransitionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((VariableSizedWrapGrid)d)._backgroundTransitionHelper?.OnTransitionChanged((BrushTransition)e.NewValue);
        }

        private Size ComputeItemSize(Size availableSize)
        {
            var hasFixedWidth = !double.IsNaN(ItemWidth);
            var hasFixedHeight = !double.IsNaN(ItemHeight);
            var itemWidth = hasFixedWidth ? ItemWidth : availableSize.Width;
            var itemHeight = hasFixedHeight ? ItemHeight : availableSize.Height;

            if (InternalChildren.Count > 0 && InternalChildren[0] is UIElement firstChild &&
                (!hasFixedWidth || !hasFixedHeight))
            {
                var measureSize = new Size(
                    hasFixedWidth ? itemWidth * GetColumnSpan(firstChild) : itemWidth,
                    hasFixedHeight ? itemHeight * GetRowSpan(firstChild) : itemHeight);

                firstChild.Measure(measureSize);

                if (!hasFixedWidth)
                {
                    itemWidth = firstChild.DesiredSize.Width;
                }

                if (!hasFixedHeight)
                {
                    itemHeight = firstChild.DesiredSize.Height;
                }
            }

            if (double.IsNaN(itemWidth) || double.IsInfinity(itemWidth))
            {
                itemWidth = 0.0;
            }

            if (double.IsNaN(itemHeight) || double.IsInfinity(itemHeight))
            {
                itemHeight = 0.0;
            }

            return new Size(Math.Max(0.0, itemWidth), Math.Max(0.0, itemHeight));
        }

        private LayoutState BuildLayout(Size itemSize, Size availableSize)
        {
            var children = InternalChildren;
            var spans = new Span[children.Count];
            var totalRowSpan = 0;
            var totalColumnSpan = 0;

            for (int i = 0; i < children.Count; i++)
            {
                var child = children[i] as UIElement;
                var span = new Span(
                    child != null ? GetPositiveSpan(GetRowSpan(child)) : 1,
                    child != null ? GetPositiveSpan(GetColumnSpan(child)) : 1);

                spans[i] = span;
                totalRowSpan += span.RowSpan;
                totalColumnSpan += span.ColumnSpan;
            }

            var itemsPerLine = DetermineItemsPerLine(itemSize, availableSize, totalRowSpan, totalColumnSpan);
            if (itemsPerLine <= 0)
            {
                return LayoutState.Empty;
            }

            var occupied = new HashSet<Cell>();
            var slots = new List<Rect>(children.Count);
            var rows = new List<int>(children.Count);
            var columns = new List<int>(children.Count);
            var lineCount = 0;
            var isHorizontal = Orientation == WpfOrientation.Horizontal;

            for (int i = 0; i < children.Count; i++)
            {
                var span = spans[i];
                var cell = FindNextAvailableCell(occupied, itemsPerLine, span, isHorizontal);
                MarkOccupied(occupied, itemsPerLine, cell, span, isHorizontal);

                var slot = new Rect(
                    cell.Column * itemSize.Width,
                    cell.Row * itemSize.Height,
                    span.ColumnSpan * itemSize.Width,
                    span.RowSpan * itemSize.Height);

                slots.Add(slot);
                rows.Add(cell.Row);
                columns.Add(cell.Column);
                lineCount = Math.Max(
                    lineCount,
                    isHorizontal ? cell.Row + span.RowSpan : cell.Column + span.ColumnSpan);
            }

            return new LayoutState(itemSize, itemsPerLine, lineCount, slots, rows, columns);
        }

        private int DetermineItemsPerLine(Size itemSize, Size availableSize, int totalRowSpan, int totalColumnSpan)
        {
            var isHorizontal = Orientation == WpfOrientation.Horizontal;
            var directAvailable = isHorizontal ? availableSize.Width : availableSize.Height;
            var directItemSize = isHorizontal ? itemSize.Width : itemSize.Height;
            var directTileCount = isHorizontal ? totalColumnSpan : totalRowSpan;
            int itemsPerLine;

            if (directTileCount <= 0)
            {
                return 0;
            }

            if (double.IsInfinity(directAvailable) || directItemSize <= 0.0)
            {
                itemsPerLine = directTileCount;
            }
            else
            {
                itemsPerLine = (int)Math.Floor(directAvailable / directItemSize);
            }

            itemsPerLine = Math.Max(1, itemsPerLine);

            if (MaximumRowsOrColumns > 0)
            {
                itemsPerLine = Math.Min(itemsPerLine, MaximumRowsOrColumns);
            }

            return itemsPerLine;
        }

        private Cell FindNextAvailableCell(HashSet<Cell> occupied, int itemsPerLine, Span span, bool isHorizontal)
        {
            for (int indirect = 0; ; indirect++)
            {
                for (int direct = 0; direct < itemsPerLine; direct++)
                {
                    var cell = isHorizontal
                        ? new Cell(indirect, direct)
                        : new Cell(direct, indirect);

                    if (CanPlace(occupied, itemsPerLine, cell, span, isHorizontal))
                    {
                        return cell;
                    }
                }
            }
        }

        private static bool CanPlace(HashSet<Cell> occupied, int itemsPerLine, Cell cell, Span span, bool isHorizontal)
        {
            var directStart = isHorizontal ? cell.Column : cell.Row;
            var directSpan = isHorizontal ? span.ColumnSpan : span.RowSpan;

            if (directSpan > itemsPerLine)
            {
                if (directStart != 0)
                {
                    return false;
                }
            }
            else if (directStart + directSpan > itemsPerLine)
            {
                return false;
            }

            foreach (var candidate in EnumerateOccupiedCells(itemsPerLine, cell, span, isHorizontal))
            {
                if (occupied.Contains(candidate))
                {
                    return false;
                }
            }

            return true;
        }

        private static void MarkOccupied(HashSet<Cell> occupied, int itemsPerLine, Cell cell, Span span, bool isHorizontal)
        {
            foreach (var candidate in EnumerateOccupiedCells(itemsPerLine, cell, span, isHorizontal))
            {
                occupied.Add(candidate);
            }
        }

        private static IEnumerable<Cell> EnumerateOccupiedCells(int itemsPerLine, Cell cell, Span span, bool isHorizontal)
        {
            var rowSpan = span.RowSpan;
            var columnSpan = span.ColumnSpan;

            if (isHorizontal)
            {
                columnSpan = Math.Min(columnSpan, itemsPerLine - cell.Column);
            }
            else
            {
                rowSpan = Math.Min(rowSpan, itemsPerLine - cell.Row);
            }

            for (int row = cell.Row; row < cell.Row + rowSpan; row++)
            {
                for (int column = cell.Column; column < cell.Column + columnSpan; column++)
                {
                    yield return new Cell(row, column);
                }
            }
        }

        private Size GetChildMeasureSize(Size itemSize, UIElement child)
        {
            return new Size(
                itemSize.Width * GetPositiveSpan(GetColumnSpan(child)),
                itemSize.Height * GetPositiveSpan(GetRowSpan(child)));
        }

        private Size ComputePanelSize(LayoutState layoutState, Size availableSize)
        {
            var alignment = ComputeAlignment(layoutState, availableSize);
            var isHorizontal = Orientation == WpfOrientation.Horizontal;
            var columnCount = isHorizontal ? layoutState.ItemsPerLine : layoutState.LineCount;
            var rowCount = isHorizontal ? layoutState.LineCount : layoutState.ItemsPerLine;

            return new Size(
                layoutState.ItemSize.Width * columnCount +
                alignment.GapX * (columnCount + 1) +
                alignment.StartX * 2.0,
                layoutState.ItemSize.Height * rowCount +
                alignment.GapY * (rowCount + 1) +
                alignment.StartY * 2.0);
        }

        private AlignmentOffsets ComputeAlignment(LayoutState layoutState, Size availableSize)
        {
            var isHorizontal = Orientation == WpfOrientation.Horizontal;
            var columnCount = isHorizontal ? layoutState.ItemsPerLine : layoutState.LineCount;
            var rowCount = isHorizontal ? layoutState.LineCount : layoutState.ItemsPerLine;
            var requiredWidth = layoutState.ItemSize.Width * columnCount;
            var requiredHeight = layoutState.ItemSize.Height * rowCount;

            ComputeAlignmentOffsets(
                HorizontalChildrenAlignment,
                availableSize.Width,
                requiredWidth,
                columnCount,
                out var startX,
                out var gapX);
            ComputeAlignmentOffsets(
                VerticalChildrenAlignment,
                availableSize.Height,
                requiredHeight,
                rowCount,
                out var startY,
                out var gapY);

            return new AlignmentOffsets(startX, startY, gapX, gapY);
        }

        private static void ComputeAlignmentOffsets(
            HorizontalAlignment alignment,
            double availableSize,
            double requiredSize,
            int totalLines,
            out double startingOffset,
            out double justificationOffset)
        {
            ComputeAlignmentOffsets((int)alignment, availableSize, requiredSize, totalLines, out startingOffset, out justificationOffset);
        }

        private static void ComputeAlignmentOffsets(
            VerticalAlignment alignment,
            double availableSize,
            double requiredSize,
            int totalLines,
            out double startingOffset,
            out double justificationOffset)
        {
            ComputeAlignmentOffsets((int)alignment, availableSize, requiredSize, totalLines, out startingOffset, out justificationOffset);
        }

        private static void ComputeAlignmentOffsets(
            int alignment,
            double availableSize,
            double requiredSize,
            int totalLines,
            out double startingOffset,
            out double justificationOffset)
        {
            startingOffset = 0.0;
            justificationOffset = 0.0;

            if (double.IsInfinity(availableSize) || totalLines <= 0)
            {
                return;
            }

            if (alignment == (int)VerticalAlignment.Center)
            {
                startingOffset = Math.Max((availableSize - requiredSize) / 2.0, 0.0);
            }
            else if (alignment == (int)VerticalAlignment.Bottom)
            {
                startingOffset = Math.Max(availableSize - requiredSize, 0.0);
            }
            else if (alignment == (int)VerticalAlignment.Stretch)
            {
                justificationOffset = Math.Max((availableSize - requiredSize) / (totalLines + 1), 0.0);
            }
        }

        private static int GetPositiveSpan(int span)
        {
            return span <= 0 ? 1 : span;
        }

        private BrushTransitionHelper BackgroundTransitionHelper =>
            _backgroundTransitionHelper ?? (_backgroundTransitionHelper = new BrushTransitionHelper(InvalidateVisual));

        private BrushTransitionHelper _backgroundTransitionHelper;
        private LayoutState _layoutState;

        private sealed class LayoutState
        {
            public static readonly LayoutState Empty =
                new LayoutState(new Size(), 0, 0, new List<Rect>(), new List<int>(), new List<int>());

            public LayoutState(Size itemSize, int itemsPerLine, int lineCount, List<Rect> slots, List<int> rows, List<int> columns)
            {
                ItemSize = itemSize;
                ItemsPerLine = itemsPerLine;
                LineCount = lineCount;
                Slots = slots;
                Rows = rows;
                Columns = columns;
            }

            public Size ItemSize { get; }
            public int ItemsPerLine { get; }
            public int LineCount { get; }
            public List<Rect> Slots { get; }
            public List<int> Rows { get; }
            public List<int> Columns { get; }
        }

        private struct Span
        {
            public Span(int rowSpan, int columnSpan)
            {
                RowSpan = rowSpan;
                ColumnSpan = columnSpan;
            }

            public int RowSpan { get; }
            public int ColumnSpan { get; }
        }

        private struct Cell : IEquatable<Cell>
        {
            public Cell(int row, int column)
            {
                Row = row;
                Column = column;
            }

            public int Row { get; }
            public int Column { get; }

            public bool Equals(Cell other)
            {
                return Row == other.Row && Column == other.Column;
            }

            public override bool Equals(object obj)
            {
                return obj is Cell other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (Row * 397) ^ Column;
                }
            }
        }

        private struct AlignmentOffsets
        {
            public AlignmentOffsets(double startX, double startY, double gapX, double gapY)
            {
                StartX = startX;
                StartY = startY;
                GapX = gapX;
                GapY = gapY;
            }

            public double StartX { get; }
            public double StartY { get; }
            public double GapX { get; }
            public double GapY { get; }
        }
    }
}
