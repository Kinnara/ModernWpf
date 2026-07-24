using System;
using System.Windows;

namespace ModernWpf.Controls
{
    internal sealed class BreadcrumbLayout : NonVirtualizingLayout
    {
        public BreadcrumbLayout(BreadcrumbBar breadcrumb)
        {
            _breadcrumb = breadcrumb;
        }

        public bool EllipsisIsRendered { get; private set; }

        public int FirstRenderedItemIndexAfterEllipsis { get; private set; }

        public int VisibleItemsCount { get; private set; }

        protected override Size MeasureOverride(NonVirtualizingLayoutContext context, Size availableSize)
        {
            _availableSize = availableSize;

            var accumulatedCrumbsSize = new Size();
            var itemCount = GetItemCount(context);

            for (var i = 0; i < itemCount; i++)
            {
                var breadcrumbItem = GetElementAt(context, i);
                breadcrumbItem.Measure(availableSize);

                if (i != 0)
                {
                    accumulatedCrumbsSize.Width += breadcrumbItem.DesiredSize.Width;
                    accumulatedCrumbsSize.Height = Math.Max(accumulatedCrumbsSize.Height, breadcrumbItem.DesiredSize.Height);
                }
            }

            _ellipsisButton = itemCount > 0 ? GetElementAt(context, 0) : null;
            EllipsisIsRendered = accumulatedCrumbsSize.Width > availableSize.Width;

            return accumulatedCrumbsSize;
        }

        protected override Size ArrangeOverride(NonVirtualizingLayoutContext context, Size finalSize)
        {
            var itemCount = GetItemCount(context);
            var firstElementToRender = 0;
            FirstRenderedItemIndexAfterEllipsis = Math.Max(0, itemCount - 1);
            VisibleItemsCount = 0;

            if (EllipsisIsRendered && itemCount > 0)
            {
                firstElementToRender = GetFirstBreadcrumbBarItemToArrange(context);
                FirstRenderedItemIndexAfterEllipsis = firstElementToRender;
            }

            var accumulatedWidths = 0.0;
            var maxElementHeight = GetBreadcrumbBarItemsHeight(context, firstElementToRender);

            if (_ellipsisButton != null)
            {
                if (EllipsisIsRendered)
                {
                    ArrangeItem(_ellipsisButton, ref accumulatedWidths, maxElementHeight);
                }
                else
                {
                    HideItem(_ellipsisButton);
                }
            }

            for (var i = 1; i < itemCount; i++)
            {
                if (i < firstElementToRender)
                {
                    HideItem(GetElementAt(context, i));
                }
                else
                {
                    ArrangeItem(GetElementAt(context, i), ref accumulatedWidths, maxElementHeight);
                    VisibleItemsCount++;
                }
            }

            _breadcrumb?.ReIndexVisibleElementsForAccessibility();
            return finalSize;
        }

        private static int GetItemCount(NonVirtualizingLayoutContext context)
        {
            return context.Children?.Count ?? 0;
        }

        private static UIElement GetElementAt(NonVirtualizingLayoutContext context, int index)
        {
            return context.Children[index];
        }

        private static void ArrangeItem(UIElement breadcrumbItem, ref double accumulatedWidths, double maxElementHeight)
        {
            var elementSize = breadcrumbItem.DesiredSize;
            breadcrumbItem.Arrange(new Rect(accumulatedWidths, 0, elementSize.Width, maxElementHeight));
            accumulatedWidths += elementSize.Width;
        }

        private static void HideItem(UIElement breadcrumbItem)
        {
            breadcrumbItem.Arrange(new Rect(0, 0, 0, 0));
        }

        private int GetFirstBreadcrumbBarItemToArrange(NonVirtualizingLayoutContext context)
        {
            var itemCount = GetItemCount(context);
            if (itemCount == 0 || _ellipsisButton == null)
            {
                return 0;
            }

            var accumLength = GetElementAt(context, itemCount - 1).DesiredSize.Width + _ellipsisButton.DesiredSize.Width;

            for (var i = itemCount - 2; i >= 0; i--)
            {
                var newAccumLength = accumLength + GetElementAt(context, i).DesiredSize.Width;
                if (newAccumLength > _availableSize.Width)
                {
                    return i + 1;
                }

                accumLength = newAccumLength;
            }

            return 0;
        }

        private double GetBreadcrumbBarItemsHeight(NonVirtualizingLayoutContext context, int firstItemToRender)
        {
            var maxElementHeight = 0.0;

            if (EllipsisIsRendered && _ellipsisButton != null)
            {
                maxElementHeight = _ellipsisButton.DesiredSize.Height;
            }

            for (var i = firstItemToRender; i < GetItemCount(context); i++)
            {
                maxElementHeight = Math.Max(maxElementHeight, GetElementAt(context, i).DesiredSize.Height);
            }

            return maxElementHeight;
        }

        private readonly BreadcrumbBar _breadcrumb;
        private Size _availableSize;
        private UIElement _ellipsisButton;
    }
}
