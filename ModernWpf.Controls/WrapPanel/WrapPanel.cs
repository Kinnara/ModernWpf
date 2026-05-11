using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.Controls
{
    public enum WrapPanelItemsStretch
    {
        None = 0,
        Last = 1,
    }

    public class WrapPanel : Panel
    {
        public static readonly DependencyProperty PaddingProperty =
            DependencyProperty.Register(
                nameof(Padding),
                typeof(Thickness),
                typeof(WrapPanel),
                new FrameworkPropertyMetadata(
                    default(Thickness),
                    FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

        public Thickness Padding
        {
            get => (Thickness)GetValue(PaddingProperty);
            set => SetValue(PaddingProperty, value);
        }

        public static readonly DependencyProperty ItemSpacingProperty =
            DependencyProperty.Register(
                nameof(ItemSpacing),
                typeof(double),
                typeof(WrapPanel),
                new FrameworkPropertyMetadata(
                    0.0,
                    FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

        public double ItemSpacing
        {
            get => (double)GetValue(ItemSpacingProperty);
            set => SetValue(ItemSpacingProperty, value);
        }

        public static readonly DependencyProperty LineSpacingProperty =
            DependencyProperty.Register(
                nameof(LineSpacing),
                typeof(double),
                typeof(WrapPanel),
                new FrameworkPropertyMetadata(
                    0.0,
                    FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

        public double LineSpacing
        {
            get => (double)GetValue(LineSpacingProperty);
            set => SetValue(LineSpacingProperty, value);
        }

        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register(
                nameof(Orientation),
                typeof(Orientation),
                typeof(WrapPanel),
                new FrameworkPropertyMetadata(
                    Orientation.Horizontal,
                    FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

        public Orientation Orientation
        {
            get => (Orientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        public static readonly DependencyProperty ItemsStretchProperty =
            DependencyProperty.Register(
                nameof(ItemsStretch),
                typeof(WrapPanelItemsStretch),
                typeof(WrapPanel),
                new FrameworkPropertyMetadata(
                    WrapPanelItemsStretch.None,
                    FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

        public WrapPanelItemsStretch ItemsStretch
        {
            get => (WrapPanelItemsStretch)GetValue(ItemsStretchProperty);
            set => SetValue(ItemsStretchProperty, value);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var padding = Padding;
            var childAvailableSize = new Size(
                Math.Max(0, availableSize.Width - padding.Left - padding.Right),
                Math.Max(0, availableSize.Height - padding.Top - padding.Bottom));

            foreach (UIElement child in InternalChildren)
            {
                child.Measure(childAvailableSize);
            }

            return UpdateRows(availableSize);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            UpdateRows(finalSize);

            var orientation = Orientation;
            var childIndex = 0;

            foreach (var row in _rows)
            {
                foreach (var uvRect in row.ChildrenRects)
                {
                    var child = GetNextVisibleChild(ref childIndex);
                    if (child == null)
                    {
                        break;
                    }

                    var arrangeRect = new UvRect
                    {
                        Position = uvRect.Position,
                        Size = new UvMeasure(uvRect.Size.U, row.Size.V)
                    };

                    child.Arrange(arrangeRect.ToRect(orientation));
                }
            }

            while (childIndex < InternalChildren.Count)
            {
                var child = InternalChildren[childIndex++];
                if (child.Visibility == Visibility.Collapsed)
                {
                    child.Arrange(Rect.Empty);
                }
            }

            return finalSize;
        }

        private Size UpdateRows(Size availableSize)
        {
            _rows.Clear();

            var orientation = Orientation;
            var padding = Padding;
            var paddingStart = new UvMeasure(orientation, padding.Left, padding.Top);
            var paddingEnd = new UvMeasure(orientation, padding.Right, padding.Bottom);

            if (InternalChildren.Count == 0)
            {
                return paddingStart.Add(paddingEnd.U, paddingEnd.V).ToSize(orientation);
            }

            var parentMeasure = new UvMeasure(orientation, availableSize.Width, availableSize.Height);
            var spacingMeasure = new UvMeasure(ItemSpacing, LineSpacing);
            var position = new UvMeasure(orientation, padding.Left, padding.Top);
            var currentRow = new Row();
            var finalMeasure = new UvMeasure();

            void ArrangeChild(UIElement child, bool isLast)
            {
                if (child.Visibility == Visibility.Collapsed)
                {
                    return;
                }

                var desiredMeasure = new UvMeasure(orientation, child.DesiredSize);
                if (desiredMeasure.U + position.U + paddingEnd.U > parentMeasure.U)
                {
                    if (!currentRow.IsEmpty)
                    {
                        position.U = paddingStart.U;
                        position.V += currentRow.Size.V + spacingMeasure.V;
                        _rows.Add(currentRow);
                        currentRow = new Row();
                    }
                }

                if (isLast && ItemsStretch == WrapPanelItemsStretch.Last)
                {
                    desiredMeasure.U = Math.Max(0, parentMeasure.U - position.U);
                }

                currentRow.Add(position, desiredMeasure);
                position.U += desiredMeasure.U + spacingMeasure.U;
                finalMeasure.U = Math.Max(finalMeasure.U, position.U);
            }

            var lastIndex = InternalChildren.Count - 1;
            for (var i = 0; i < lastIndex; i++)
            {
                ArrangeChild(InternalChildren[i], false);
            }

            ArrangeChild(InternalChildren[lastIndex], true);

            if (!currentRow.IsEmpty)
            {
                _rows.Add(currentRow);
            }

            if (_rows.Count == 0)
            {
                return paddingStart.Add(paddingEnd.U, paddingEnd.V).ToSize(orientation);
            }

            var lastRowRect = _rows[_rows.Count - 1].Rect();
            finalMeasure.V = lastRowRect.Position.V + lastRowRect.Size.V;
            return finalMeasure.Add(paddingEnd.U, paddingEnd.V).ToSize(orientation);
        }

        private UIElement GetNextVisibleChild(ref int childIndex)
        {
            while (childIndex < InternalChildren.Count)
            {
                var child = InternalChildren[childIndex++];
                if (child.Visibility != Visibility.Collapsed)
                {
                    return child;
                }

                child.Arrange(Rect.Empty);
            }

            return null;
        }

        private struct UvMeasure
        {
            public double U;
            public double V;

            public UvMeasure(double u, double v)
            {
                U = u;
                V = v;
            }

            public UvMeasure(Orientation orientation, Size size)
                : this(orientation, size.Width, size.Height)
            {
            }

            public UvMeasure(Orientation orientation, double width, double height)
            {
                if (orientation == Orientation.Horizontal)
                {
                    U = width;
                    V = height;
                }
                else
                {
                    U = height;
                    V = width;
                }
            }

            public UvMeasure Add(double u, double v)
            {
                return new UvMeasure(U + u, V + v);
            }

            public Size ToSize(Orientation orientation)
            {
                return orientation == Orientation.Horizontal
                    ? new Size(U, V)
                    : new Size(V, U);
            }
        }

        private struct UvRect
        {
            public UvMeasure Position;
            public UvMeasure Size;

            public Rect ToRect(Orientation orientation)
            {
                return orientation == Orientation.Horizontal
                    ? new Rect(Position.U, Position.V, Size.U, Size.V)
                    : new Rect(Position.V, Position.U, Size.V, Size.U);
            }
        }

        private struct Row
        {
            public List<UvRect> ChildrenRects;
            public UvMeasure Size;

            public bool IsEmpty => ChildrenRects == null || ChildrenRects.Count == 0;

            public UvRect Rect()
            {
                return !IsEmpty
                    ? new UvRect { Position = ChildrenRects[0].Position, Size = Size }
                    : new UvRect { Position = new UvMeasure(), Size = Size };
            }

            public void Add(UvMeasure position, UvMeasure size)
            {
                ChildrenRects ??= new List<UvRect>();
                ChildrenRects.Add(new UvRect { Position = position, Size = size });

                var newU = position.U + size.U;
                var newV = Math.Max(Size.V, size.V);
                Size = new UvMeasure(newU, newV);
            }
        }

        private readonly List<Row> _rows = new();
    }
}
