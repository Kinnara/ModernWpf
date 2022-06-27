using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace ModernWpf.Controls.Primitives
{
    /// <summary>
    /// Represents a panel that arranges its items horizontally if there is available space, otherwise vertically.
    /// </summary>
    public class InfoBarPanel : Panel
    {
        bool m_isVertical = false;

        public InfoBarPanel()
        {
        }

        #region ForceVertical

        public bool ForceVertical
        {
            get => (bool)GetValue(ForceVerticalProperty);
            set => SetValue(ForceVerticalProperty, value);
        }

        public static readonly DependencyProperty ForceVerticalProperty =
            DependencyProperty.Register(
                nameof(ForceVertical),
                typeof(bool),
                typeof(InfoBarPanel),
                new PropertyMetadata(false));

        #endregion

        #region HorizontalOrientationMargin

        /// <summary>
        /// Gets the identifier for the HorizontalOrientationMargin dependency property.
        /// </summary>
        public static readonly DependencyProperty HorizontalOrientationMarginProperty =
            DependencyProperty.Register(
                "HorizontalOrientationMargin",
                typeof(Thickness),
                typeof(InfoBarPanel),
                null);

        /// <summary>
        /// Gets the HorizontalOrientationMargin from an object.
        /// </summary>
        /// <param name="control">The object that has an HorizontalOrientationMargin.</param>
        /// <returns>The object's HorizontalOrientationMargin.</returns>
        public static Thickness GetHorizontalOrientationMargin(DependencyObject control)
        {
            return (Thickness)control.GetValue(HorizontalOrientationMarginProperty);
        }

        /// <summary>
        /// Sets the HorizontalOrientationMargin to an object.
        /// </summary>
        /// <param name="control">The object that the HorizontalOrientationMargin value will be set to.</param>
        /// <param name="value">The value of the HorizontalOrientationMargin.</param>
        public static void SetHorizontalOrientationMargin(DependencyObject control, Thickness value)
        {
            control.SetValue(HorizontalOrientationMarginProperty, value);
        }

        #endregion

        #region HorizontalOrientationPadding

        /// <summary>
        /// Gets and sets the distance between the edges of the <see cref="InfoBarPanel"/> and its children when the panel is oriented horizontally.
        /// </summary>
        /// <value>The distance between the edges of the <see cref="InfoBarPanel"/> and its children when the panel is oriented horizontally.</value>
        public Thickness HorizontalOrientationPadding
        {
            get => (Thickness)GetValue(HorizontalOrientationPaddingProperty);
            set => SetValue(HorizontalOrientationPaddingProperty, value);
        }

        /// <summary>
        /// Gets the identifier for the <see cref="HorizontalOrientationPadding"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty HorizontalOrientationPaddingProperty =
            DependencyProperty.Register(
                nameof(HorizontalOrientationPadding),
                typeof(Thickness),
                typeof(InfoBarPanel),
                null);

        #endregion

        #region VerticalOrientationMargin

        /// <summary>
        /// Gets the identifier for the VerticalOrientationMargin dependency property.
        /// </summary>
        public static readonly DependencyProperty VerticalOrientationMarginProperty =
            DependencyProperty.Register(
                "VerticalOrientationMargin",
                typeof(Thickness),
                typeof(InfoBarPanel),
                null);

        /// <summary>
        /// Gets the VerticalOrientationMargin from an object.
        /// </summary>
        /// <param name="control">The object that has an VerticalOrientationMargin.</param>
        /// <returns>The object's VerticalOrientationMargin.</returns>
        public static Thickness GetVerticalOrientationMargin(DependencyObject control)
        {
            return (Thickness)control.GetValue(VerticalOrientationMarginProperty);
        }

        /// <summary>
        /// Sets the VerticalOrientationMargin to an object.
        /// </summary>
        /// <param name="control">The object that the VerticalOrientationMargin value will be set to.</param>
        /// <param name="value">The value of the VerticalOrientationMargin.</param>
        public static void SetVerticalOrientationMargin(DependencyObject control, Thickness value)
        {
            control.SetValue(VerticalOrientationMarginProperty, value);
        }

        #endregion

        #region VerticalOrientationPadding

        /// <summary>
        /// Gets and sets the distance between the edges of the <see cref="InfoBarPanel"/> and its children when the panel is oriented vertically.
        /// </summary>
        /// <value>The distance between the edges of the <see cref="InfoBarPanel"/> and its children when the panel is oriented vertically.</value>
        public Thickness VerticalOrientationPadding
        {
            get => (Thickness)GetValue(VerticalOrientationPaddingProperty);
            set => SetValue(VerticalOrientationPaddingProperty, value);
        }

        /// <summary>
        /// Gets the identifier for the <see cref="VerticalOrientationPadding"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty VerticalOrientationPaddingProperty =
            DependencyProperty.Register(
                nameof(VerticalOrientationPadding),
                typeof(Thickness),
                typeof(InfoBarPanel),
                null);

        #endregion

        protected override Size MeasureOverride(Size availableSize)
        {
            Size desiredSize = new Size();

            double totalWidth = 0;
            double totalHeight = 0;
            double widthOfWidest = 0;
            double heightOfTallest = 0;
            double heightOfTallestInHorizontal = 0;
            int nItems = 0;

            var parent = this.Parent as FrameworkElement;
            double minHeight = parent == null ? 0 : (parent.MinHeight - (Margin.Top + Margin.Bottom));

            var children = Children;
            var childCount = children.Count;
            foreach (UIElement child in children)
            {
                child.Measure(availableSize);
                var childDesiredSize = child.DesiredSize;

                if (childDesiredSize.Width != 0 && childDesiredSize.Height != 0)
                {
                    // Add up the width of all items if they were laid out horizontally
                    var horizontalMargin = InfoBarPanel.GetHorizontalOrientationMargin(child);
                    // Ignore left margin of first and right margin of last child
                    totalWidth += childDesiredSize.Width + (nItems > 0 ? (float)horizontalMargin.Left : 0) + (nItems < childCount - 1 ? (float)horizontalMargin.Right : 0);

                    // Add up the height of all items if they were laid out vertically
                    var verticalMargin = InfoBarPanel.GetVerticalOrientationMargin(child);
                    // Ignore top margin of first and bottom margin of last child
                    totalHeight += childDesiredSize.Height + (nItems > 0 ? (float)verticalMargin.Top : 0) + (nItems < childCount - 1 ? (float)verticalMargin.Bottom : 0);

                    if (childDesiredSize.Width > widthOfWidest)
                    {
                        widthOfWidest = childDesiredSize.Width;
                    }

                    if (childDesiredSize.Height > heightOfTallest)
                    {
                        heightOfTallest = childDesiredSize.Height;
                    }

                    double childHeightInHorizontal = childDesiredSize.Height + horizontalMargin.Top + horizontalMargin.Bottom;
                    if (childHeightInHorizontal > heightOfTallestInHorizontal)
                    {
                        heightOfTallestInHorizontal = childHeightInHorizontal;
                    }

                    nItems++;
                }
            }

            // Since this panel is inside a *-sized grid column, availableSize.Width should not be infinite
            // If there is only one item inside the panel, we will count it as vertical (the margins work out better that way)
            // Also, if the height of any item is taller than the desired min height of the InfoBar,
            // the items should be laid out vertically even though they may seem to fit due to text wrapping.
            if (ForceVertical || nItems == 1 || totalWidth > availableSize.Width || (minHeight > 0F && heightOfTallestInHorizontal > minHeight))
            {
                m_isVertical = true;
                var verticalPadding = VerticalOrientationPadding;

                desiredSize.Width = widthOfWidest + verticalPadding.Left + verticalPadding.Right;
                desiredSize.Height = totalHeight + verticalPadding.Top + verticalPadding.Bottom;
            }
            else
            {
                m_isVertical = false;
                var horizontalPadding = HorizontalOrientationPadding;

                desiredSize.Width = totalWidth + horizontalPadding.Left + horizontalPadding.Right;
                desiredSize.Height = heightOfTallest + horizontalPadding.Top + horizontalPadding.Bottom;
            }

            return desiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            Size result = finalSize;

            if (m_isVertical)
            {
                // Layout elements vertically
                var verticalOrientationPadding = VerticalOrientationPadding;
                double verticalOffset = verticalOrientationPadding.Top;

                bool hasPreviousElement = false;
                foreach (UIElement child in Children)
                {
                    var childAsFe = child as FrameworkElement;
                    if (childAsFe != null)
                    {
                        var desiredSize = child.DesiredSize;
                        if (desiredSize.Width != 0 && desiredSize.Height != 0)
                        {
                            var verticalMargin = InfoBarPanel.GetVerticalOrientationMargin(child);

                            verticalOffset += hasPreviousElement ? (float)verticalMargin.Top : 0;
                            child.Arrange(new Rect(verticalOrientationPadding.Left + verticalMargin.Left, verticalOffset, desiredSize.Width, desiredSize.Height));
                            verticalOffset += desiredSize.Height + verticalMargin.Bottom;

                            hasPreviousElement = true;
                        }
                    }
                }
            }
            else
            {
                // Layout elements horizontally
                var horizontalOrientationPadding = HorizontalOrientationPadding;
                double horizontalOffset = horizontalOrientationPadding.Left;
                bool hasPreviousElement = false;

                var children = Children;
                var childCount = children.Count;
                for (int i = 0; i < childCount; i++)
                {
                    var child = children[i];
                    var childAsFe = child as FrameworkElement;
                    if (childAsFe != null)
                    {
                        var desiredSize = child.DesiredSize;
                        if (desiredSize.Width != 0 && desiredSize.Height != 0)
                        {
                            var horizontalMargin = InfoBarPanel.GetHorizontalOrientationMargin(child);

                            horizontalOffset += hasPreviousElement ? horizontalMargin.Left : 0;
                            if (i < childCount - 1)
                            {
                                child.Arrange(new Rect(horizontalOffset, horizontalOrientationPadding.Top + horizontalMargin.Top, desiredSize.Width, desiredSize.Height));
                            }
                            else
                            {
                                // Give the rest of the horizontal space to the last child.
                                child.Arrange(new Rect(horizontalOffset, horizontalOrientationPadding.Top + horizontalMargin.Top, Math.Max(desiredSize.Width, finalSize.Width - horizontalOffset), desiredSize.Height));
                            }

                            horizontalOffset += desiredSize.Width + horizontalMargin.Right;

                            hasPreviousElement = true;
                        }
                    }
                }
            }

            return result;
        }
    }
}
