// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Windows;

namespace ModernWpf.Controls.Primitives
{
    public class ColumnMajorUniformToLargestGridLayout : NonVirtualizingLayout
    {
        public ColumnMajorUniformToLargestGridLayout()
        {
        }

        #region ColumnSpacing

        public static readonly DependencyProperty ColumnSpacingProperty =
            DependencyProperty.Register(
                nameof(ColumnSpacing),
                typeof(double),
                typeof(ColumnMajorUniformToLargestGridLayout),
                new FrameworkPropertyMetadata(OnColumnSpacingPropertyChanged));

        public double ColumnSpacing
        {
            get => (double)GetValue(ColumnSpacingProperty);
            set => SetValue(ColumnSpacingProperty, value);
        }

        private static void OnColumnSpacingPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var owner = (ColumnMajorUniformToLargestGridLayout)sender;
            owner.OnColumnSpacingPropertyChanged(args);
        }

        private void OnColumnSpacingPropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            InvalidateMeasure();
        }

        #endregion

        #region RowSpacing

        public static readonly DependencyProperty RowSpacingProperty =
            DependencyProperty.Register(
                nameof(RowSpacing),
                typeof(double),
                typeof(ColumnMajorUniformToLargestGridLayout),
                new FrameworkPropertyMetadata(OnRowSpacingPropertyChanged));

        public double RowSpacing
        {
            get => (double)GetValue(RowSpacingProperty);
            set => SetValue(RowSpacingProperty, value);
        }

        private static void OnRowSpacingPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var owner = (ColumnMajorUniformToLargestGridLayout)sender;
            owner.OnRowSpacingPropertyChanged(args);
        }

        private void OnRowSpacingPropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            InvalidateMeasure();
        }

        #endregion

        #region MaxColumns

        public static readonly DependencyProperty MaxColumnsProperty =
            DependencyProperty.Register(
                nameof(MaxColumns),
                typeof(int),
                typeof(ColumnMajorUniformToLargestGridLayout),
                new FrameworkPropertyMetadata(1, OnMaxColumnsPropertyChanged),
                ValidateMaxColumns);

        public int MaxColumns
        {
            get => (int)GetValue(MaxColumnsProperty);
            set => SetValue(MaxColumnsProperty, value);
        }

        private static void OnMaxColumnsPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var owner = (ColumnMajorUniformToLargestGridLayout)sender;
            owner.OnMaxColumnsPropertyChanged(args);
        }

        private void OnMaxColumnsPropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            InvalidateMeasure();
        }

        private static bool ValidateMaxColumns(object value)
        {
            return (int)value > 0;
        }

        #endregion

        protected override Size MeasureOverride(NonVirtualizingLayoutContext context, Size availableSize)
        {
            var children = context.Children;
            if (children != null && children.Count > 0)
            {
                Size calculateLargestChildSize()
                {
                    var largestChildWidth = 0.0;
                    var largestChildHeight = 0.0;
                    foreach (var child in children)
                    {
                        child.Measure(availableSize);
                        var desiredSize = child.DesiredSize;
                        if (desiredSize.Width > largestChildWidth)
                        {
                            largestChildWidth = desiredSize.Width;
                        }
                        if (desiredSize.Height > largestChildHeight)
                        {
                            largestChildHeight = desiredSize.Height;
                        }
                    }
                    return new Size(largestChildWidth, largestChildHeight);
                };
                m_largestChildSize = calculateLargestChildSize();

                m_actualColumnCount = CalculateColumns(children.Count, m_largestChildSize.Width, availableSize.Width);
                var maxItemsPerColumn = (int)Math.Ceiling((double)children.Count / m_actualColumnCount);
                return new Size(
                    (m_largestChildSize.Width * m_actualColumnCount) +
                    (ColumnSpacing * (m_actualColumnCount - 1)),
                    (m_largestChildSize.Height * maxItemsPerColumn) +
                    (RowSpacing * (maxItemsPerColumn - 1))
                );
            }
            return new Size(0, 0);
        }

        protected override Size ArrangeOverride(NonVirtualizingLayoutContext context, Size finalSize)
        {
            var children = context.Children;
            if (children != null)
            {
                var itemCount = children.Count;
                var minitemsPerColumn = (int)Math.Floor((double)itemCount / m_actualColumnCount);
                var numberOfColumnsWithExtraElements = itemCount % m_actualColumnCount;

                var columnSpacing = ColumnSpacing;
                var rowSpacing = RowSpacing;

                var horizontalOffset = 0.0;
                var verticalOffset = 0.0;
                var index = 0;
                var column = 0;
                foreach (var child in children)
                {
                    var desiredSize = child.DesiredSize;
                    child.Arrange(new Rect(horizontalOffset, verticalOffset, desiredSize.Width, desiredSize.Height));
                    if (column < numberOfColumnsWithExtraElements)
                    {
                        if (index % (minitemsPerColumn + 1) == minitemsPerColumn)
                        {
                            horizontalOffset += m_largestChildSize.Width + columnSpacing;
                            verticalOffset = 0.0;
                            column++;
                        }
                        else
                        {
                            verticalOffset += m_largestChildSize.Height + rowSpacing;
                        }
                    }
                    else
                    {
                        var indexAfterExtraLargeColumns = index - (numberOfColumnsWithExtraElements * (minitemsPerColumn + 1));
                        if (indexAfterExtraLargeColumns % minitemsPerColumn == minitemsPerColumn - 1)
                        {
                            horizontalOffset += m_largestChildSize.Width + columnSpacing;
                            verticalOffset = 0.0;
                            column++;
                        }
                        else
                        {
                            verticalOffset += m_largestChildSize.Height + rowSpacing;
                        }
                    }
                    index++;
                }

                if (m_testHooksEnabled)
                {
                    //Testhooks setup
                    if (m_largerColumns != numberOfColumnsWithExtraElements ||
                        m_columns != column ||
                        m_rows != minitemsPerColumn)
                    {
                        m_largerColumns = numberOfColumnsWithExtraElements;
                        m_columns = column;
                        m_rows = minitemsPerColumn;

                        LayoutChanged?.Invoke(this, null);
                    }
                }
            }
            return finalSize;
        }

        int CalculateColumns(int childCount, double maxItemWidth, double availableWidth)
        {
            /*
            --------------------------------------------------------------
            |      |-----------|-----------| | widthNeededForExtraColumn |
            |                                |                           |
            |      |------|    |------|      | ColumnSpacing             |
            | |----|      |----|      |----| | maxItemWidth              |
            |  O RB        O RB        O RB  |                           |
            --------------------------------------------------------------
            */

            // Every column execpt the first takes this ammount of space to fit on screen.
            var widthNeededForExtraColumn = ColumnSpacing + maxItemWidth;
            // The number of columns from data and api ignoring available space
            var requestedColumnCount = Math.Min(MaxColumns, childCount);

            // If columns can be added with effectively 0 extra space return as many columns as needed.
            if (widthNeededForExtraColumn < double.Epsilon)
            {
                return requestedColumnCount;
            }

            var extraWidthAfterFirstColumn = availableWidth - maxItemWidth;
            var maxExtraColumns = Math.Max(0.0, Math.Floor(extraWidthAfterFirstColumn / widthNeededForExtraColumn));

            // The smaller of number of columns from data and api and
            // the number of columns the available space can support
            var effectiveColumnCount = Math.Min(requestedColumnCount, maxExtraColumns + 1);
            // return 1 even if there isn't any data
            return Math.Max(1, (int)effectiveColumnCount);
        }

        //Testhooks helpers, only function while m_testHooksEnabled == true
        internal void SetTestHooksEnabled(bool enabled)
        {
            m_testHooksEnabled = enabled;
        }

        internal int GetRows()
        {
            return m_rows;
        }

        internal int GetColumns()
        {
            return m_columns;
        }

        internal int GetLargerColumns()
        {
            return m_largerColumns;
        }

        internal event TypedEventHandler<ColumnMajorUniformToLargestGridLayout, object> LayoutChanged;

        int m_actualColumnCount = 1;
        Size m_largestChildSize;

        //Testhooks helpers, only function while m_testHooksEnabled == true
        bool m_testHooksEnabled = false;

        int m_rows = -1;
        int m_columns = -1;
        int m_largerColumns = -1;
    }
}
