using System;
using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.Controls.Primitives
{
    public partial class InfoBarPanel : Panel
    {
        protected override Size MeasureOverride(Size availableSize)
        {
            var desiredSize = default(Size);
            var totalWidth = 0.0;
            var totalHeight = 0.0;
            var widthOfWidest = 0.0;
            var heightOfTallest = 0.0;
            var heightOfTallestInHorizontal = 0.0;
            var itemIndex = 0;

            var minHeight = Parent is FrameworkElement parent
                ? Math.Max(0.0, parent.MinHeight - (Margin.Top + Margin.Bottom))
                : 0.0;

            foreach (UIElement child in InternalChildren)
            {
                child.Measure(availableSize);
                var childDesiredSize = child.DesiredSize;

                if (childDesiredSize.Width != 0 && childDesiredSize.Height != 0)
                {
                    var horizontalMargin = GetHorizontalOrientationMargin(child);
                    totalWidth += childDesiredSize.Width +
                        (itemIndex > 0 ? horizontalMargin.Left : 0) +
                        (itemIndex < InternalChildren.Count - 1 ? horizontalMargin.Right : 0);

                    var verticalMargin = GetVerticalOrientationMargin(child);
                    totalHeight += childDesiredSize.Height +
                        (itemIndex > 0 ? verticalMargin.Top : 0) +
                        (itemIndex < InternalChildren.Count - 1 ? verticalMargin.Bottom : 0);

                    widthOfWidest = Math.Max(widthOfWidest, childDesiredSize.Width);
                    heightOfTallest = Math.Max(heightOfTallest, childDesiredSize.Height);
                    heightOfTallestInHorizontal = Math.Max(
                        heightOfTallestInHorizontal,
                        childDesiredSize.Height + horizontalMargin.Top + horizontalMargin.Bottom);

                    itemIndex++;
                }
            }

            if (itemIndex == 1 || totalWidth > availableSize.Width || (minHeight > 0 && heightOfTallestInHorizontal > minHeight))
            {
                _isVertical = true;
                var verticalPadding = VerticalOrientationPadding;
                desiredSize.Width = widthOfWidest + verticalPadding.Left + verticalPadding.Right;
                desiredSize.Height = totalHeight + verticalPadding.Top + verticalPadding.Bottom;
            }
            else
            {
                _isVertical = false;
                var horizontalPadding = HorizontalOrientationPadding;
                desiredSize.Width = totalWidth + horizontalPadding.Left + horizontalPadding.Right;
                desiredSize.Height = heightOfTallest + horizontalPadding.Top + horizontalPadding.Bottom;
            }

            return desiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (_isVertical)
            {
                var verticalOrientationPadding = VerticalOrientationPadding;
                var verticalOffset = verticalOrientationPadding.Top;
                var hasPreviousElement = false;

                foreach (UIElement child in InternalChildren)
                {
                    if (child is FrameworkElement)
                    {
                        var desiredSize = child.DesiredSize;
                        if (desiredSize.Width != 0 && desiredSize.Height != 0)
                        {
                            var verticalMargin = GetVerticalOrientationMargin(child);
                            verticalOffset += hasPreviousElement ? verticalMargin.Top : 0;
                            child.Arrange(new Rect(
                                verticalOrientationPadding.Left + verticalMargin.Left,
                                verticalOffset,
                                desiredSize.Width,
                                desiredSize.Height));
                            verticalOffset += desiredSize.Height + verticalMargin.Bottom;
                            hasPreviousElement = true;
                        }
                    }
                }
            }
            else
            {
                var horizontalOrientationPadding = HorizontalOrientationPadding;
                var horizontalOffset = horizontalOrientationPadding.Left;
                var hasPreviousElement = false;

                for (var i = 0; i < InternalChildren.Count; i++)
                {
                    var child = InternalChildren[i];
                    if (child is FrameworkElement)
                    {
                        var desiredSize = child.DesiredSize;
                        if (desiredSize.Width != 0 && desiredSize.Height != 0)
                        {
                            var horizontalMargin = GetHorizontalOrientationMargin(child);
                            horizontalOffset += hasPreviousElement ? horizontalMargin.Left : 0;
                            child.Arrange(new Rect(
                                horizontalOffset,
                                horizontalOrientationPadding.Top + horizontalMargin.Top,
                                i < InternalChildren.Count - 1 ? desiredSize.Width : Math.Max(desiredSize.Width, finalSize.Width - horizontalOffset),
                                desiredSize.Height));
                            horizontalOffset += desiredSize.Width + horizontalMargin.Right;
                            hasPreviousElement = true;
                        }
                    }
                }
            }

            return finalSize;
        }

        private bool _isVertical;
    }
}
