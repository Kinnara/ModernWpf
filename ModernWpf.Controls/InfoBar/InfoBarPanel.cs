using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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
                var childDesiredSize = GetWinUIDesiredSize(child);

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
                        var desiredSize = GetWinUIDesiredSize(child);
                        if (desiredSize.Width != 0 && desiredSize.Height != 0)
                        {
                            var verticalMargin = GetVerticalOrientationMargin(child);
                            verticalOffset += hasPreviousElement ? verticalMargin.Top : 0;
                            var textLayoutRoundingOffset = GetWinUITextLayoutRoundingHeightAdjustment(child, desiredSize);
                            child.Arrange(new Rect(
                                verticalOrientationPadding.Left + verticalMargin.Left,
                                verticalOffset + textLayoutRoundingOffset,
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
                        var desiredSize = GetWinUIDesiredSize(child);
                        if (desiredSize.Width != 0 && desiredSize.Height != 0)
                        {
                            var horizontalMargin = GetHorizontalOrientationMargin(child);
                            horizontalOffset += hasPreviousElement ? horizontalMargin.Left : 0;
                            var textLayoutRoundingOffset = GetWinUITextLayoutRoundingHeightAdjustment(child, desiredSize);
                            child.Arrange(new Rect(
                                horizontalOffset,
                                horizontalOrientationPadding.Top + horizontalMargin.Top + textLayoutRoundingOffset,
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

        private Size GetWinUIDesiredSize(UIElement child)
        {
            var desiredSize = child.DesiredSize;
            var dpi = VisualTreeHelper.GetDpi(this);

            // WinUI TextBlock ceilings its page-node dimensions to physical pixels
            // before parent panels consume DesiredSize. WPF preserves fractional text
            // metrics, so reproduce that boundary for the source-ported panel.
            desiredSize.Width = Math.Ceiling(desiredSize.Width * dpi.DpiScaleX) / dpi.DpiScaleX;
            desiredSize.Height = Math.Ceiling(desiredSize.Height * dpi.DpiScaleY) / dpi.DpiScaleY;
            return desiredSize;
        }

        private static double GetWinUITextLayoutRoundingHeightAdjustment(UIElement child, Size roundedDesiredSize)
        {
            // WinUI TextBlock adds the layout-rounding height adjustment to its
            // content render offset so its last line is not clipped.
            return child is TextBlock
                ? Math.Max(0.0, roundedDesiredSize.Height - child.DesiredSize.Height)
                : 0.0;
        }

        private bool _isVertical;
    }
}
