using System.Windows;
using System.Windows.Markup;
using ModernWpf.Controls.Primitives;
using WpfOrientation = System.Windows.Controls.Orientation;

namespace ModernWpf.Controls
{
    [ContentProperty(nameof(Children))]
    public sealed class ItemsWrapGrid : WrapGrid
    {
        public static readonly DependencyProperty GroupPaddingProperty =
            DependencyProperty.Register(
                nameof(GroupPadding),
                typeof(Thickness),
                typeof(ItemsWrapGrid),
                new FrameworkPropertyMetadata(
                    new Thickness(),
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.AffectsArrange));

        public Thickness GroupPadding
        {
            get => (Thickness)GetValue(GroupPaddingProperty);
            set => SetValue(GroupPaddingProperty, value);
        }

        public static new readonly DependencyProperty OrientationProperty =
            VariableSizedWrapGrid.OrientationProperty;

        public new WpfOrientation Orientation
        {
            get => (WpfOrientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        public static new readonly DependencyProperty MaximumRowsOrColumnsProperty =
            VariableSizedWrapGrid.MaximumRowsOrColumnsProperty;

        public new int MaximumRowsOrColumns
        {
            get => (int)GetValue(MaximumRowsOrColumnsProperty);
            set => SetValue(MaximumRowsOrColumnsProperty, value);
        }

        public static new readonly DependencyProperty ItemWidthProperty =
            VariableSizedWrapGrid.ItemWidthProperty;

        public new double ItemWidth
        {
            get => (double)GetValue(ItemWidthProperty);
            set => SetValue(ItemWidthProperty, value);
        }

        public static new readonly DependencyProperty ItemHeightProperty =
            VariableSizedWrapGrid.ItemHeightProperty;

        public new double ItemHeight
        {
            get => (double)GetValue(ItemHeightProperty);
            set => SetValue(ItemHeightProperty, value);
        }

        public static readonly DependencyProperty GroupHeaderPlacementProperty =
            DependencyProperty.Register(
                nameof(GroupHeaderPlacement),
                typeof(GroupHeaderPlacement),
                typeof(ItemsWrapGrid),
                new FrameworkPropertyMetadata(
                    GroupHeaderPlacement.Top,
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.AffectsArrange));

        public GroupHeaderPlacement GroupHeaderPlacement
        {
            get => (GroupHeaderPlacement)GetValue(GroupHeaderPlacementProperty);
            set => SetValue(GroupHeaderPlacementProperty, value);
        }

        public static readonly DependencyProperty CacheLengthProperty =
            DependencyProperty.Register(
                nameof(CacheLength),
                typeof(double),
                typeof(ItemsWrapGrid),
                new PropertyMetadata(0.0),
                IsValidCacheLength);

        public double CacheLength
        {
            get => (double)GetValue(CacheLengthProperty);
            set => SetValue(CacheLengthProperty, value);
        }

        public static readonly DependencyProperty AreStickyGroupHeadersEnabledProperty =
            DependencyProperty.Register(
                nameof(AreStickyGroupHeadersEnabled),
                typeof(bool),
                typeof(ItemsWrapGrid),
                new PropertyMetadata(true));

        public bool AreStickyGroupHeadersEnabled
        {
            get => (bool)GetValue(AreStickyGroupHeadersEnabledProperty);
            set => SetValue(AreStickyGroupHeadersEnabledProperty, value);
        }

        public int FirstCacheIndex { get; private set; } = -1;

        public int FirstVisibleIndex { get; private set; } = -1;

        public int LastVisibleIndex { get; private set; } = -1;

        public int LastCacheIndex { get; private set; } = -1;

        public PanelScrollingDirection ScrollingDirection { get; private set; } = PanelScrollingDirection.None;

        protected override Size MeasureOverride(Size availableSize)
        {
            var size = base.MeasureOverride(availableSize);
            UpdateRealizedRange();
            return size;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var size = base.ArrangeOverride(finalSize);
            UpdateRealizedRange();
            return size;
        }

        private void UpdateRealizedRange()
        {
            var count = Children.Count;
            if (count == 0)
            {
                FirstCacheIndex = -1;
                FirstVisibleIndex = -1;
                LastVisibleIndex = -1;
                LastCacheIndex = -1;
            }
            else
            {
                FirstCacheIndex = 0;
                FirstVisibleIndex = 0;
                LastVisibleIndex = count - 1;
                LastCacheIndex = count - 1;
            }

            ScrollingDirection = PanelScrollingDirection.None;
        }

        private static bool IsValidCacheLength(object value)
        {
            return value is double cacheLength && cacheLength >= 0.0 && !double.IsNaN(cacheLength);
        }
    }
}
