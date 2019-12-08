using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Windows;

namespace ModernWpf.SampleApp.Common
{
    class ActivityFeedLayout : VirtualizingLayout
    {
        #region Layout parameters

        // We'll cache copies of the dependency properties to avoid calling GetValue during layout since that
        // can be quite expensive due to the number of times we'd end up calling these.
        private double _rowSpacing;
        private double _colSpacing;
        private Size _minItemSize = Size.Empty;

        /// <summary>
        /// Gets or sets the size of the whitespace gutter to include between rows
        /// </summary>
        public double RowSpacing
        {
            get { return _rowSpacing; }
            set { SetValue(RowSpacingProperty, value); }
        }

        public static readonly DependencyProperty RowSpacingProperty =
            DependencyProperty.Register(
                "RowSpacing",
                typeof(double),
                typeof(ActivityFeedLayout),
                new PropertyMetadata(0d, OnPropertyChanged));

        /// <summary>
        /// Gets or sets the size of the whitespace gutter to include between items on the same row
        /// </summary>
        public double ColumnSpacing
        {
            get { return _colSpacing; }
            set { SetValue(ColumnSpacingProperty, value); }
        }

        public static readonly DependencyProperty ColumnSpacingProperty =
            DependencyProperty.Register(
                "ColumnSpacing",
                typeof(double),
                typeof(ActivityFeedLayout),
                new PropertyMetadata(0d, OnPropertyChanged));

        public Size MinItemSize
        {
            get { return _minItemSize; }
            set { SetValue(MinItemSizeProperty, value); }
        }

        public static readonly DependencyProperty MinItemSizeProperty =
            DependencyProperty.Register(
                "MinItemSize",
                typeof(Size),
                typeof(ActivityFeedLayout),
                new PropertyMetadata(Size.Empty, OnPropertyChanged));

        private static void OnPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var layout = obj as ActivityFeedLayout;
            if (args.Property == RowSpacingProperty)
            {
                layout._rowSpacing = (double)args.NewValue;
            }
            else if (args.Property == ColumnSpacingProperty)
            {
                layout._colSpacing = (double)args.NewValue;
            }
            else if (args.Property == MinItemSizeProperty)
            {
                layout._minItemSize = (Size)args.NewValue;
            }
            else
            {
                throw new InvalidOperationException("Don't know what you are talking about!");
            }

            layout.InvalidateMeasure();
        }

        #endregion

        #region Setup / teardown

        protected override void InitializeForContextCore(VirtualizingLayoutContext context)
        {
            base.InitializeForContextCore(context);

            var state = context.LayoutState as ActivityFeedLayoutState;
            if (state == null)
            {
                // Store any state we might need since (in theory) the layout could be in use by multiple 
                // elements simultaneously
                // In reality for the Xbox Activity Feed there's probably only a single instance.
                context.LayoutState = new ActivityFeedLayoutState();
            }
        }

        protected override void UninitializeForContextCore(VirtualizingLayoutContext context)
        {
            base.UninitializeForContextCore(context);

            // clear any state
            context.LayoutState = null;
        }

        #endregion

        #region Layout

        protected override Size MeasureOverride(VirtualizingLayoutContext context, Size availableSize)
        {
            if (MinItemSize == Size.Empty)
            {
                var firstElement = context.GetOrCreateElementAt(0);
                firstElement.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                // setting the member value directly to skip invalidating layout
                _minItemSize = firstElement.DesiredSize;
            }

            // Determine which rows need to be realized.  We know every row will have the same height and 
            // only contain 3 items.  Use that to determine the index for the first and last item that 
            // will be within that realization rect.
            var firstRowIndex = Math.Max(
                (int)(context.RealizationRect.Y / (MinItemSize.Height + RowSpacing)) - 1,
                0);
            var lastRowIndex = Math.Min(
                (int)(context.RealizationRect.Bottom / (MinItemSize.Height + RowSpacing)) + 1,
                context.ItemCount / 3);

            // Determine which items will appear on those rows and what the rect will be for each item
            var state = context.LayoutState as ActivityFeedLayoutState;
            state.LayoutRects.Clear();

            // Save the index of the first realized item.  We'll use it as a starting point during arrange.
            state.FirstRealizedIndex = firstRowIndex * 3;

            // ideal item width that will expand/shrink to fill available space
            double desiredItemWidth = Math.Max(MinItemSize.Width, (availableSize.Width - ColumnSpacing * 3) / 4);

            // Foreach item between the first and last index, 
            //     Call GetElementOrCreateElementAt which causes an element to either be realized or retrieved 
            //       from a recycle pool
            //     Measure the element using an appropriate size
            // 
            // Any element that was previously realized which we don't retrieve in this pass (via a call to 
            // GetElementOrCreateAt) will be automatically cleared and set aside for later re-use.  
            // Note: While this work fine, it does mean that more elements than are required may be
            // created because it isn't until after our MeasureOverride completes that the unused elements 
            // will be recycled and available to use.  We could avoid this by choosing to track the first/last
            // index from the previous layout pass.  The diff between the previous range and current range 
            // would represent the elements that we can pre-emptively make available for re-use by calling 
            // context.RecycleElement(element).
            for (int rowIndex = firstRowIndex; rowIndex < lastRowIndex; rowIndex++)
            {
                int firstItemIndex = rowIndex * 3;
                var boundsForCurrentRow = CalculateLayoutBoundsForRow(rowIndex, desiredItemWidth);

                for (int columnIndex = 0; columnIndex < 3; columnIndex++)
                {
                    var index = firstItemIndex + columnIndex;
                    var rect = boundsForCurrentRow[index % 3];
                    var container = context.GetOrCreateElementAt(index);

                    container.Measure(
                        new Size(boundsForCurrentRow[columnIndex].Width, boundsForCurrentRow[columnIndex].Height));

                    state.LayoutRects.Add(boundsForCurrentRow[columnIndex]);
                }
            }

            // Calculate and return the size of all the content (realized or not) by figuring out 
            // what the bottom/right position of the last item would be.
            var extentHeight = (context.ItemCount / 3 - 1) * (MinItemSize.Height + RowSpacing) + MinItemSize.Height;

            // Report this as the desired size for the layout
            return new Size(desiredItemWidth * 4 + ColumnSpacing * 2, extentHeight);
        }

        protected override Size ArrangeOverride(VirtualizingLayoutContext context, Size finalSize)
        {
            // walk through the cache of containers and arrange
            var state = context.LayoutState as ActivityFeedLayoutState;
            var virtualContext = context as VirtualizingLayoutContext;
            int currentIndex = state.FirstRealizedIndex;

            foreach (var arrangeRect in state.LayoutRects)
            {
                var container = virtualContext.GetOrCreateElementAt(currentIndex);
                container.Arrange(arrangeRect);
                currentIndex++;
            }

            return finalSize;
        }

        #endregion
        #region Helper methods

        private Rect[] CalculateLayoutBoundsForRow(int rowIndex, double desiredItemWidth)
        {
            var boundsForRow = new Rect[3];

            var yoffset = rowIndex * (MinItemSize.Height + RowSpacing);
            boundsForRow[0].Y = boundsForRow[1].Y = boundsForRow[2].Y = yoffset;
            boundsForRow[0].Height = boundsForRow[1].Height = boundsForRow[2].Height = MinItemSize.Height;

            if (rowIndex % 2 == 0)
            {
                // Left tile (narrow)
                boundsForRow[0].X = 0;
                boundsForRow[0].Width = desiredItemWidth;
                // Middle tile (narrow)
                boundsForRow[1].X = boundsForRow[0].Right + ColumnSpacing;
                boundsForRow[1].Width = desiredItemWidth;
                // Right tile (wide)
                boundsForRow[2].X = boundsForRow[1].Right + ColumnSpacing;
                boundsForRow[2].Width = desiredItemWidth * 2 + ColumnSpacing;
            }
            else
            {
                // Left tile (wide)
                boundsForRow[0].X = 0;
                boundsForRow[0].Width = desiredItemWidth * 2 + ColumnSpacing;
                // Middle tile (narrow)
                boundsForRow[1].X = boundsForRow[0].Right + ColumnSpacing;
                boundsForRow[1].Width = desiredItemWidth;
                // Right tile (narrow)
                boundsForRow[2].X = boundsForRow[1].Right + ColumnSpacing;
                boundsForRow[2].Width = desiredItemWidth;
            }

            return boundsForRow;
        }

        #endregion
    }

    internal class ActivityFeedLayoutState
    {
        public int FirstRealizedIndex { get; set; }

        /// <summary>
        /// List of layout bounds for items starting with the
        /// FirstRealizedIndex.
        /// </summary>
        public List<Rect> LayoutRects
        {
            get
            {
                if (_layoutRects == null)
                {
                    _layoutRects = new List<Rect>();
                }

                return _layoutRects;
            }
        }

        private List<Rect> _layoutRects;
    }
}
