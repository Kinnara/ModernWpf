using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.Controls
{
    /// <summary>
    /// Arranges slider range elements from their current explicit dimensions.
    /// WPF's Slider updates those dimensions after the normal Canvas measure
    /// pass, so Canvas can otherwise retain a stale zero DesiredSize.
    /// </summary>
    public class SliderRangeCanvas : Canvas
    {
        private static readonly DependencyPropertyDescriptor WidthDescriptor =
            DependencyPropertyDescriptor.FromProperty(FrameworkElement.WidthProperty, typeof(FrameworkElement));

        private static readonly DependencyPropertyDescriptor HeightDescriptor =
            DependencyPropertyDescriptor.FromProperty(FrameworkElement.HeightProperty, typeof(FrameworkElement));

        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            base.OnVisualChildrenChanged(visualAdded, visualRemoved);

            if (visualRemoved is FrameworkElement removedElement)
            {
                WidthDescriptor.RemoveValueChanged(removedElement, OnChildDimensionChanged);
                HeightDescriptor.RemoveValueChanged(removedElement, OnChildDimensionChanged);
            }

            if (visualAdded is FrameworkElement addedElement)
            {
                WidthDescriptor.AddValueChanged(addedElement, OnChildDimensionChanged);
                HeightDescriptor.AddValueChanged(addedElement, OnChildDimensionChanged);
            }
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            ArrangeChildren(arrangeSize);
            return arrangeSize;
        }

        private void ArrangeChildren(Size arrangeSize)
        {
            foreach (UIElement child in InternalChildren)
            {
                if (child == null)
                {
                    continue;
                }

                var childSize = child.DesiredSize;
                if (child is FrameworkElement element)
                {
                    if (!double.IsNaN(element.Width))
                    {
                        childSize.Width = element.Width;
                    }

                    if (!double.IsNaN(element.Height))
                    {
                        childSize.Height = element.Height;
                    }
                }

                var left = GetLeft(child);
                if (double.IsNaN(left))
                {
                    var right = GetRight(child);
                    left = double.IsNaN(right) ? 0 : arrangeSize.Width - childSize.Width - right;
                }

                var top = GetTop(child);
                if (double.IsNaN(top))
                {
                    var bottom = GetBottom(child);
                    top = double.IsNaN(bottom) ? 0 : arrangeSize.Height - childSize.Height - bottom;
                }

                child.Arrange(new Rect(new Point(left, top), childSize));
            }
        }

        private void OnChildDimensionChanged(object sender, EventArgs e)
        {
            if (RenderSize.Width > 0 || RenderSize.Height > 0)
            {
                ArrangeChildren(RenderSize);
            }

            InvalidateMeasure();
            InvalidateArrange();
        }
    }
}
