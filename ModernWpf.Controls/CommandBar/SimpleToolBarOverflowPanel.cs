using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace ModernWpf.Controls
{
    public class SimpleToolBarOverflowPanel : ToolBarOverflowPanel
    {
        public SimpleToolBarOverflowPanel()
        {
            Loaded += OnLoaded;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            base.MeasureOverride(constraint);

            Size stackDesiredSize = new Size();
            UIElementCollection children = InternalChildren;
            Size layoutSlotSize = constraint;

            layoutSlotSize.Height = double.PositiveInfinity;

            for (int i = 0, count = children.Count; i < count; ++i)
            {
                UIElement child = children[i];

                if (child == null) { continue; }

                child.Measure(layoutSlotSize);
                Size childDesiredSize = child.DesiredSize;

                stackDesiredSize.Width = Math.Max(stackDesiredSize.Width, childDesiredSize.Width);
                stackDesiredSize.Height += childDesiredSize.Height;
            }

            return stackDesiredSize;
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            UIElementCollection children = InternalChildren;
            Rect rcChild = new Rect(arrangeBounds);
            double previousChildSize = 0.0;

            for (int i = 0, count = children.Count; i < count; ++i)
            {
                UIElement child = children[i];

                if (child == null) { continue; }

                rcChild.Y += previousChildSize;
                previousChildSize = child.DesiredSize.Height;
                rcChild.Height = previousChildSize;
                rcChild.Width = Math.Max(arrangeBounds.Width, child.DesiredSize.Width);

                child.Arrange(rcChild);
            }
            return arrangeBounds;
        }

        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            base.OnVisualChildrenChanged(visualAdded, visualRemoved);

            if (visualAdded != null)
            {
                UpdateChildrenApplicationViewState();
            }
        }

        private void UpdateChildrenApplicationViewState()
        {
            bool hasToggleButton = false;
            bool hasMenuIcon = false;

            UIElementCollection children = InternalChildren;

            for (int i = 0, count = children.Count; i < count; ++i)
            {
                UIElement child = children[i];

                if (child is AppBarButton appBarButton)
                {
                    if (!hasMenuIcon && appBarButton.Icon != null)
                    {
                        hasMenuIcon = true;
                    }
                }
                else if (child is AppBarToggleButton appBarToggleButton)
                {
                    if (!hasMenuIcon && appBarToggleButton.Icon != null)
                    {
                        hasMenuIcon = true;
                    }

                    if (!hasToggleButton)
                    {
                        hasToggleButton = true;
                    }
                }

                if (hasMenuIcon && hasToggleButton)
                {
                    break;
                }
            }

            for (int i = 0, count = children.Count; i < count; ++i)
            {
                UIElement child = children[i];

                if (child is AppBarButton appBarButton)
                {
                    appBarButton.UpdateApplicationViewStateInOverflow(hasToggleButton, hasMenuIcon);
                }
                else if (child is AppBarToggleButton appBarToggleButton)
                {
                    appBarToggleButton.UpdateApplicationViewStateInOverflow(hasMenuIcon);
                }
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateChildrenApplicationViewState();
        }
    }
}
