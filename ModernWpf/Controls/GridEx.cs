using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using ModernWpf.Media.Animation;

namespace ModernWpf.Controls
{
    [ContentProperty(nameof(Children))]
    public class GridEx : Panel
    {
        static GridEx()
        {
            BackgroundProperty.OverrideMetadata(
                typeof(GridEx),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    OnBackgroundPropertyChanged));
        }

        public GridEx()
        {
            _definitionsHost = new DefinitionsHost(this);
            _itemsHost = new ItemsHost(this);
            _border = new LayoutChromeDecorator { Child = _itemsHost };
            UpdateBorder();
            AddVisualChild(_border);
        }

        public static readonly DependencyProperty BackgroundSizingProperty =
            DependencyProperty.Register(
                nameof(BackgroundSizing),
                typeof(ModernWpf.Controls.BackgroundSizing),
                typeof(GridEx),
                new FrameworkPropertyMetadata(
                    ModernWpf.Controls.BackgroundSizing.InnerBorderEdge,
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    OnBorderPropertyChanged));

        public BackgroundSizing BackgroundSizing
        {
            get => (BackgroundSizing)GetValue(BackgroundSizingProperty);
            set => SetValue(BackgroundSizingProperty, value);
        }

        public static readonly DependencyProperty BackgroundTransitionProperty =
            DependencyProperty.Register(
                nameof(BackgroundTransition),
                typeof(BrushTransition),
                typeof(GridEx),
                new PropertyMetadata(null, OnBorderPropertyChanged));

        public BrushTransition BackgroundTransition
        {
            get => (BrushTransition)GetValue(BackgroundTransitionProperty);
            set => SetValue(BackgroundTransitionProperty, value);
        }

        public static readonly DependencyProperty BorderBrushProperty =
            Border.BorderBrushProperty.AddOwner(
                typeof(GridEx),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    OnBorderPropertyChanged));

        public Brush BorderBrush
        {
            get => (Brush)GetValue(BorderBrushProperty);
            set => SetValue(BorderBrushProperty, value);
        }

        public static readonly DependencyProperty BorderThicknessProperty =
            Border.BorderThicknessProperty.AddOwner(
                typeof(GridEx),
                new FrameworkPropertyMetadata(
                    new Thickness(),
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.AffectsArrange |
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    OnBorderPropertyChanged));

        public Thickness BorderThickness
        {
            get => (Thickness)GetValue(BorderThicknessProperty);
            set => SetValue(BorderThicknessProperty, value);
        }

        public static readonly DependencyProperty ChildrenTransitionsProperty =
            DependencyProperty.Register(
                nameof(ChildrenTransitions),
                typeof(TransitionCollection),
                typeof(GridEx),
                new PropertyMetadata(null));

        public TransitionCollection ChildrenTransitions
        {
            get => (TransitionCollection)GetValue(ChildrenTransitionsProperty);
            set => SetValue(ChildrenTransitionsProperty, value);
        }

        public static readonly DependencyProperty ColumnSpacingProperty =
            DependencyProperty.Register(
                nameof(ColumnSpacing),
                typeof(double),
                typeof(GridEx),
                new FrameworkPropertyMetadata(
                    0.0,
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.AffectsArrange,
                    OnSpacingPropertyChanged),
                IsValidSpacing);

        public double ColumnSpacing
        {
            get => (double)GetValue(ColumnSpacingProperty);
            set => SetValue(ColumnSpacingProperty, value);
        }

        public static readonly DependencyProperty CornerRadiusProperty =
            Border.CornerRadiusProperty.AddOwner(
                typeof(GridEx),
                new FrameworkPropertyMetadata(
                    new CornerRadius(),
                    FrameworkPropertyMetadataOptions.AffectsArrange |
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    OnBorderPropertyChanged));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public static readonly DependencyProperty PaddingProperty =
            Control.PaddingProperty.AddOwner(
                typeof(GridEx),
                new FrameworkPropertyMetadata(
                    new Thickness(),
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.AffectsArrange,
                    OnBorderPropertyChanged));

        public Thickness Padding
        {
            get => (Thickness)GetValue(PaddingProperty);
            set => SetValue(PaddingProperty, value);
        }

        public static readonly DependencyProperty RowSpacingProperty =
            DependencyProperty.Register(
                nameof(RowSpacing),
                typeof(double),
                typeof(GridEx),
                new FrameworkPropertyMetadata(
                    0.0,
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.AffectsArrange,
                    OnSpacingPropertyChanged),
                IsValidSpacing);

        public double RowSpacing
        {
            get => (double)GetValue(RowSpacingProperty);
            set => SetValue(RowSpacingProperty, value);
        }

        public ColumnDefinitionCollection ColumnDefinitions => _definitionsHost.ColumnDefinitions;

        public RowDefinitionCollection RowDefinitions => _definitionsHost.RowDefinitions;

        protected override int VisualChildrenCount => 1;

        protected override Visual GetVisualChild(int index)
        {
            if (index != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return _border;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            _border.Measure(availableSize);
            ValidateDefinitionsHost();
            return _border.DesiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            _border.Arrange(new Rect(finalSize));
            return finalSize;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
        }

        protected override Geometry GetLayoutClip(Size layoutSlotSize)
        {
            return LayoutChromeHelper.CreateRoundedLayoutClip(
                layoutSlotSize,
                CornerRadius,
                base.GetLayoutClip(layoutSlotSize));
        }

        protected override UIElementCollection CreateUIElementCollection(FrameworkElement logicalParent)
        {
            return _itemsHost.Children;
        }

        internal Brush EffectiveBackground => _border?.EffectiveBackground ?? Background;

        private static bool IsValidSpacing(object value)
        {
            return value is double spacing && !double.IsNaN(spacing);
        }

        private static void OnBackgroundPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((GridEx)d).UpdateBorder();
        }

        private static void OnBorderPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((GridEx)d).UpdateBorder();
        }

        private static void OnSpacingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = (GridEx)d;
            grid._itemsHost?.InvalidateMeasure();
        }

        private void UpdateBorder()
        {
            if (_border == null)
            {
                return;
            }

            _border.BackgroundTransition = BackgroundTransition;
            _border.Background = Background;
            _border.BackgroundSizing = BackgroundSizing;
            _border.BorderBrush = BorderBrush;
            _border.BorderThickness = BorderThickness;
            _border.CornerRadius = CornerRadius;
            _border.Padding = Padding;
        }

        private void OnDefinitionsHostMeasureInvalidated()
        {
            _itemsHost?.InvalidateMeasure();
            InvalidateMeasure();
        }

        private void ValidateDefinitionsHost()
        {
            if (!_definitionsHost.IsMeasureValid)
            {
                _definitionsHost.Measure(new Size());
            }
        }

        private readonly DefinitionsHost _definitionsHost;
        private readonly LayoutChromeDecorator _border;
        private readonly ItemsHost _itemsHost;

        private sealed class DefinitionsHost : Grid
        {
            public DefinitionsHost(GridEx owner)
            {
                _owner = owner;
            }

            protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
            {
                base.OnPropertyChanged(e);

                if (e.Property.Name == nameof(IsMeasureValid) &&
                    e.NewValue is bool isMeasureValid &&
                    !isMeasureValid)
                {
                    _owner.OnDefinitionsHostMeasureInvalidated();
                }
            }

            private readonly GridEx _owner;
        }

        private class ItemsHost : Grid
        {
            public ItemsHost(GridEx owner)
            {
                _owner = owner;
            }

            protected override Size MeasureOverride(Size constraint)
            {
                var layoutScope = ApplySpacingLayout();
                try
                {
                    var desiredSize = base.MeasureOverride(GetEffectiveNegativeSpacingSize(constraint));
                    return GetDesiredSizeWithNegativeSpacing(desiredSize);
                }
                finally
                {
                    RestoreLayout(layoutScope);
                }
            }

            protected override Size ArrangeOverride(Size arrangeSize)
            {
                var layoutScope = ApplySpacingLayout();
                try
                {
                    var effectiveArrangeSize = GetEffectiveNegativeSpacingSize(arrangeSize);
                    base.ArrangeOverride(effectiveArrangeSize);
                    if (HasCustomSpacingArrange())
                    {
                        RestoreLayout(layoutScope);
                        layoutScope = null;
                        ArrangeSpacingChildren(GetEffectiveSpacingDefinitionSize(arrangeSize));
                    }

                    return arrangeSize;
                }
                finally
                {
                    RestoreLayout(layoutScope);
                }
            }

            protected override UIElementCollection CreateUIElementCollection(FrameworkElement logicalParent)
            {
                return new UIElementCollection(this, _owner);
            }

            private List<LayoutScopeEntry> ApplySpacingLayout()
            {
                var useColumnGaps = _owner.ColumnDefinitions.Count > 0 && _owner.ColumnSpacing >= 0;
                var useRowGaps = _owner.RowDefinitions.Count > 0 && _owner.RowSpacing >= 0;
                SyncDefinitions(useColumnGaps, useRowGaps);

                var columnSpacing = _owner.ColumnSpacing;
                var rowSpacing = _owner.RowSpacing;
                if ((!useColumnGaps && columnSpacing >= 0) && (!useRowGaps && rowSpacing >= 0))
                {
                    return null;
                }

                List<LayoutScopeEntry> scope = null;
                foreach (UIElement child in InternalChildren)
                {
                    var originalColumn = Grid.GetColumn(child);
                    var originalColumnSpan = Grid.GetColumnSpan(child);
                    var originalRow = Grid.GetRow(child);
                    var originalRowSpan = Grid.GetRowSpan(child);
                    var originalMargin = child is FrameworkElement element ? element.Margin : new Thickness();
                    var newColumn = useColumnGaps ? originalColumn * 2 : originalColumn;
                    var newColumnSpan = useColumnGaps ? Math.Max(1, (Math.Max(1, originalColumnSpan) * 2) - 1) : originalColumnSpan;
                    var newRow = useRowGaps ? originalRow * 2 : originalRow;
                    var newRowSpan = useRowGaps ? Math.Max(1, (Math.Max(1, originalRowSpan) * 2) - 1) : originalRowSpan;
                    var newMargin = originalMargin;

                    if (newColumn != originalColumn ||
                        newColumnSpan != originalColumnSpan ||
                        newRow != originalRow ||
                        newRowSpan != originalRowSpan ||
                        !newMargin.Equals(originalMargin))
                    {
                        scope ??= new List<LayoutScopeEntry>();
                        scope.Add(new LayoutScopeEntry(
                            child,
                            originalColumn,
                            originalColumnSpan,
                            originalRow,
                            originalRowSpan,
                            originalMargin));

                        Grid.SetColumn(child, newColumn);
                        Grid.SetColumnSpan(child, newColumnSpan);
                        Grid.SetRow(child, newRow);
                        Grid.SetRowSpan(child, newRowSpan);
                        if (child is FrameworkElement frameworkElement)
                        {
                            frameworkElement.Margin = newMargin;
                        }
                    }
                }

                return scope;
            }

            private Size GetEffectiveNegativeSpacingSize(Size size)
            {
                var result = size;

                if (_owner.ColumnDefinitions.Count > 0 && _owner.ColumnSpacing < 0 && !double.IsInfinity(result.Width))
                {
                    result.Width -= GetCombinedColumnSpacing();
                }

                if (_owner.RowDefinitions.Count > 0 && _owner.RowSpacing < 0 && !double.IsInfinity(result.Height))
                {
                    result.Height -= GetCombinedRowSpacing();
                }

                return result;
            }

            private Size GetDesiredSizeWithNegativeSpacing(Size desiredSize)
            {
                var result = desiredSize;

                if (_owner.ColumnDefinitions.Count > 0 && _owner.ColumnSpacing < 0)
                {
                    result.Width = Math.Max(0, result.Width + GetCombinedColumnSpacing());
                }

                if (_owner.RowDefinitions.Count > 0 && _owner.RowSpacing < 0)
                {
                    result.Height = Math.Max(0, result.Height + GetCombinedRowSpacing());
                }

                return result;
            }

            private double GetCombinedColumnSpacing()
            {
                return _owner.ColumnSpacing * Math.Max(0, _owner.ColumnDefinitions.Count - 1);
            }

            private double GetCombinedRowSpacing()
            {
                return _owner.RowSpacing * Math.Max(0, _owner.RowDefinitions.Count - 1);
            }

            private bool HasCustomSpacingArrange()
            {
                return (_owner.ColumnDefinitions.Count > 0 && _owner.ColumnSpacing != 0) ||
                    (_owner.RowDefinitions.Count > 0 && _owner.RowSpacing != 0);
            }

            private Size GetEffectiveSpacingDefinitionSize(Size size)
            {
                var result = size;

                if (_owner.ColumnDefinitions.Count > 0 && _owner.ColumnSpacing != 0 && !double.IsInfinity(result.Width))
                {
                    result.Width = Math.Max(0, result.Width - GetCombinedColumnSpacing());
                }

                if (_owner.RowDefinitions.Count > 0 && _owner.RowSpacing != 0 && !double.IsInfinity(result.Height))
                {
                    result.Height = Math.Max(0, result.Height - GetCombinedRowSpacing());
                }

                return result;
            }

            private void ArrangeSpacingChildren(Size definitionArrangeSize)
            {
                var useColumnSpacing = _owner.ColumnDefinitions.Count > 0 && _owner.ColumnSpacing != 0;
                var useRowSpacing = _owner.RowDefinitions.Count > 0 && _owner.RowSpacing != 0;

                var columnSizes = useColumnSpacing ? GetColumnArrangeSizes(definitionArrangeSize.Width) : null;
                var rowSizes = useRowSpacing ? GetRowArrangeSizes(definitionArrangeSize.Height) : null;

                foreach (UIElement child in InternalChildren)
                {
                    var origin = child.TranslatePoint(new Point(), this);
                    var arrangeRect = new Rect(origin, child.RenderSize);

                    if (useColumnSpacing)
                    {
                        var column = GetClampedColumn(child);
                        var columnSpan = GetClampedColumnSpan(child, column);
                        arrangeRect.X = GetOffset(columnSizes, column) + (_owner.ColumnSpacing * column);
                        arrangeRect.Width = GetRangeSize(columnSizes, column, columnSpan, _owner.ColumnSpacing);
                    }

                    if (useRowSpacing)
                    {
                        var row = GetClampedRow(child);
                        var rowSpan = GetClampedRowSpan(child, row);
                        arrangeRect.Y = GetOffset(rowSizes, row) + (_owner.RowSpacing * row);
                        arrangeRect.Height = GetRangeSize(rowSizes, row, rowSpan, _owner.RowSpacing);
                    }

                    child.Arrange(arrangeRect);
                }
            }

            private int GetClampedColumn(UIElement child)
            {
                return Math.Min(Math.Max(0, Grid.GetColumn(child)), _owner.ColumnDefinitions.Count - 1);
            }

            private int GetClampedColumnSpan(UIElement child, int column)
            {
                return Math.Max(1, Math.Min(Math.Max(1, Grid.GetColumnSpan(child)), _owner.ColumnDefinitions.Count - column));
            }

            private int GetClampedRow(UIElement child)
            {
                return Math.Min(Math.Max(0, Grid.GetRow(child)), _owner.RowDefinitions.Count - 1);
            }

            private int GetClampedRowSpan(UIElement child, int row)
            {
                return Math.Max(1, Math.Min(Math.Max(1, Grid.GetRowSpan(child)), _owner.RowDefinitions.Count - row));
            }

            private double[] GetColumnArrangeSizes(double effectiveWidth)
            {
                var sizes = new double[_owner.ColumnDefinitions.Count];
                var starWeight = 0.0;

                for (int i = 0; i < sizes.Length; i++)
                {
                    var source = _owner.ColumnDefinitions[i];
                    if (source.Width.IsAbsolute)
                    {
                        sizes[i] = Math.Min(source.MaxWidth, Math.Max(source.MinWidth, source.Width.Value));
                    }
                    else if (source.Width.IsStar)
                    {
                        starWeight += Math.Max(0, source.Width.Value);
                    }
                }

                foreach (UIElement child in InternalChildren)
                {
                    var column = GetClampedColumn(child);
                    if (_owner.ColumnDefinitions[column].Width.IsAuto &&
                        GetClampedColumnSpan(child, column) == 1)
                    {
                        var source = _owner.ColumnDefinitions[column];
                        sizes[column] = Math.Max(
                            sizes[column],
                            Math.Min(source.MaxWidth, Math.Max(source.MinWidth, child.DesiredSize.Width)));
                    }
                }

                ApplyColumnSpanDesiredSizes(sizes);
                ResolveStarSizes(effectiveWidth, sizes, starWeight, _owner.ColumnDefinitions);
                return sizes;
            }

            private double[] GetRowArrangeSizes(double effectiveHeight)
            {
                var sizes = new double[_owner.RowDefinitions.Count];
                var starWeight = 0.0;

                for (int i = 0; i < sizes.Length; i++)
                {
                    var source = _owner.RowDefinitions[i];
                    if (source.Height.IsAbsolute)
                    {
                        sizes[i] = Math.Min(source.MaxHeight, Math.Max(source.MinHeight, source.Height.Value));
                    }
                    else if (source.Height.IsStar)
                    {
                        starWeight += Math.Max(0, source.Height.Value);
                    }
                }

                foreach (UIElement child in InternalChildren)
                {
                    var row = GetClampedRow(child);
                    if (_owner.RowDefinitions[row].Height.IsAuto &&
                        GetClampedRowSpan(child, row) == 1)
                    {
                        var source = _owner.RowDefinitions[row];
                        sizes[row] = Math.Max(
                            sizes[row],
                            Math.Min(source.MaxHeight, Math.Max(source.MinHeight, child.DesiredSize.Height)));
                    }
                }

                ApplyRowSpanDesiredSizes(sizes);
                ResolveStarSizes(effectiveHeight, sizes, starWeight, _owner.RowDefinitions);
                return sizes;
            }

            private void ApplyColumnSpanDesiredSizes(double[] sizes)
            {
                foreach (UIElement child in InternalChildren)
                {
                    var column = GetClampedColumn(child);
                    var columnSpan = GetClampedColumnSpan(child, column);
                    if (columnSpan > 1)
                    {
                        EnsureColumnAutoRangeSize(
                            sizes,
                            column,
                            columnSpan,
                            child.DesiredSize.Width,
                            _owner.ColumnSpacing,
                            _owner.ColumnDefinitions);
                    }
                }
            }

            private void ApplyRowSpanDesiredSizes(double[] sizes)
            {
                foreach (UIElement child in InternalChildren)
                {
                    var row = GetClampedRow(child);
                    var rowSpan = GetClampedRowSpan(child, row);
                    if (rowSpan > 1)
                    {
                        EnsureRowAutoRangeSize(
                            sizes,
                            row,
                            rowSpan,
                            child.DesiredSize.Height,
                            _owner.RowSpacing,
                            _owner.RowDefinitions);
                    }
                }
            }

            private static void ResolveStarSizes(
                double effectiveSize,
                double[] sizes,
                double starWeight,
                ColumnDefinitionCollection definitions)
            {
                if (starWeight <= 0 || double.IsInfinity(effectiveSize))
                {
                    return;
                }

                var remaining = Math.Max(0, effectiveSize - Sum(sizes));
                for (int i = 0; i < sizes.Length; i++)
                {
                    var source = definitions[i];
                    if (source.Width.IsStar)
                    {
                        sizes[i] = Math.Min(
                            source.MaxWidth,
                            Math.Max(source.MinWidth, remaining * Math.Max(0, source.Width.Value) / starWeight));
                    }
                }
            }

            private static void ResolveStarSizes(
                double effectiveSize,
                double[] sizes,
                double starWeight,
                RowDefinitionCollection definitions)
            {
                if (starWeight <= 0 || double.IsInfinity(effectiveSize))
                {
                    return;
                }

                var remaining = Math.Max(0, effectiveSize - Sum(sizes));
                for (int i = 0; i < sizes.Length; i++)
                {
                    var source = definitions[i];
                    if (source.Height.IsStar)
                    {
                        sizes[i] = Math.Min(
                            source.MaxHeight,
                            Math.Max(source.MinHeight, remaining * Math.Max(0, source.Height.Value) / starWeight));
                    }
                }
            }

            private static void EnsureColumnAutoRangeSize(
                double[] sizes,
                int start,
                int span,
                double childDesiredSize,
                double spacing,
                ColumnDefinitionCollection definitions)
            {
                var requestedSize = Math.Max(0, childDesiredSize - (spacing * Math.Max(0, span - 1)));
                var currentSize = SumRange(sizes, start, span);
                var remaining = requestedSize - currentSize;
                if (remaining <= 0)
                {
                    return;
                }

                var autoCount = 0;
                for (int i = start; i < start + span; i++)
                {
                    if (definitions[i].Width.IsAuto)
                    {
                        autoCount++;
                    }
                }

                if (autoCount == 0)
                {
                    return;
                }

                var share = remaining / autoCount;
                for (int i = start; i < start + span; i++)
                {
                    var definition = definitions[i];
                    if (definition.Width.IsAuto)
                    {
                        sizes[i] = Math.Min(
                            definition.MaxWidth,
                            Math.Max(definition.MinWidth, sizes[i] + share));
                    }
                }
            }

            private static void EnsureRowAutoRangeSize(
                double[] sizes,
                int start,
                int span,
                double childDesiredSize,
                double spacing,
                RowDefinitionCollection definitions)
            {
                var requestedSize = Math.Max(0, childDesiredSize - (spacing * Math.Max(0, span - 1)));
                var currentSize = SumRange(sizes, start, span);
                var remaining = requestedSize - currentSize;
                if (remaining <= 0)
                {
                    return;
                }

                var autoCount = 0;
                for (int i = start; i < start + span; i++)
                {
                    if (definitions[i].Height.IsAuto)
                    {
                        autoCount++;
                    }
                }

                if (autoCount == 0)
                {
                    return;
                }

                var share = remaining / autoCount;
                for (int i = start; i < start + span; i++)
                {
                    var definition = definitions[i];
                    if (definition.Height.IsAuto)
                    {
                        sizes[i] = Math.Min(
                            definition.MaxHeight,
                            Math.Max(definition.MinHeight, sizes[i] + share));
                    }
                }
            }

            private static double GetOffset(double[] sizes, int index)
            {
                double offset = 0;
                for (int i = 0; i < index; i++)
                {
                    offset += sizes[i];
                }

                return offset;
            }

            private static double GetRangeSize(double[] sizes, int index, int span, double spacing)
            {
                double size = 0;
                for (int i = index; i < index + span; i++)
                {
                    size += sizes[i];
                }

                return Math.Max(0, size + (spacing * Math.Max(0, span - 1)));
            }

            private static double SumRange(double[] sizes, int start, int span)
            {
                double sum = 0;
                for (int i = start; i < start + span; i++)
                {
                    sum += sizes[i];
                }

                return sum;
            }

            private static double Sum(double[] sizes)
            {
                double sum = 0;
                for (int i = 0; i < sizes.Length; i++)
                {
                    sum += sizes[i];
                }

                return sum;
            }

            private void SyncDefinitions(bool useColumnGaps, bool useRowGaps)
            {
                ColumnDefinitions.Clear();
                for (int i = 0; i < _owner.ColumnDefinitions.Count; i++)
                {
                    ColumnDefinitions.Add(CloneColumnDefinition(_owner.ColumnDefinitions[i]));
                    if (useColumnGaps && i < _owner.ColumnDefinitions.Count - 1)
                    {
                        ColumnDefinitions.Add(CreateColumnGapDefinition(_owner.ColumnSpacing));
                    }
                }

                RowDefinitions.Clear();
                for (int i = 0; i < _owner.RowDefinitions.Count; i++)
                {
                    RowDefinitions.Add(CloneRowDefinition(_owner.RowDefinitions[i]));
                    if (useRowGaps && i < _owner.RowDefinitions.Count - 1)
                    {
                        RowDefinitions.Add(CreateRowGapDefinition(_owner.RowSpacing));
                    }
                }
            }

            private static ColumnDefinition CreateColumnGapDefinition(double spacing)
            {
                return new ColumnDefinition
                {
                    Width = new GridLength(spacing),
                    MaxWidth = spacing
                };
            }

            private static RowDefinition CreateRowGapDefinition(double spacing)
            {
                return new RowDefinition
                {
                    Height = new GridLength(spacing),
                    MaxHeight = spacing
                };
            }

            private static ColumnDefinition CloneColumnDefinition(ColumnDefinition source)
            {
                var maxWidth = source.MaxWidth;
                if (source.Width.IsAbsolute)
                {
                    maxWidth = Math.Min(maxWidth, Math.Max(source.MinWidth, source.Width.Value));
                }

                return new ColumnDefinition
                {
                    Width = source.Width,
                    MinWidth = source.MinWidth,
                    MaxWidth = maxWidth,
                    SharedSizeGroup = source.SharedSizeGroup
                };
            }

            private static RowDefinition CloneRowDefinition(RowDefinition source)
            {
                var maxHeight = source.MaxHeight;
                if (source.Height.IsAbsolute)
                {
                    maxHeight = Math.Min(maxHeight, Math.Max(source.MinHeight, source.Height.Value));
                }

                return new RowDefinition
                {
                    Height = source.Height,
                    MinHeight = source.MinHeight,
                    MaxHeight = maxHeight
                };
            }

            private static void RestoreLayout(List<LayoutScopeEntry> scope)
            {
                if (scope == null)
                {
                    return;
                }

                foreach (var entry in scope)
                {
                    Grid.SetColumn(entry.Child, entry.Column);
                    Grid.SetColumnSpan(entry.Child, entry.ColumnSpan);
                    Grid.SetRow(entry.Child, entry.Row);
                    Grid.SetRowSpan(entry.Child, entry.RowSpan);
                    if (entry.Child is FrameworkElement element)
                    {
                        element.Margin = entry.Margin;
                    }
                }
            }

            private readonly struct LayoutScopeEntry
            {
                public LayoutScopeEntry(
                    UIElement child,
                    int column,
                    int columnSpan,
                    int row,
                    int rowSpan,
                    Thickness margin)
                {
                    Child = child;
                    Column = column;
                    ColumnSpan = columnSpan;
                    Row = row;
                    RowSpan = rowSpan;
                    Margin = margin;
                }

                public UIElement Child { get; }

                public int Column { get; }

                public int ColumnSpan { get; }

                public int Row { get; }

                public int RowSpan { get; }

                public Thickness Margin { get; }
            }

            private readonly GridEx _owner;
        }
    }
}
