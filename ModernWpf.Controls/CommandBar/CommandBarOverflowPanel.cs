using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace ModernWpf.Controls.Primitives
{
    public class CommandBarOverflowPanel : ToolBarOverflowPanel
    {
        public CommandBarOverflowPanel()
        {
            Loaded += OnLoaded;
        }

        internal bool HasToggleButton { get; private set; }

        internal bool HasMenuIcon { get; private set; }

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

                if (child is AppBarSeparator separator && IsPrimaryCommand(separator))
                {
                    UpdateSeparatorVisibility(i, separator);
                }

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

            if (visualRemoved is AppBarSeparator separator && IsPrimaryCommand(separator))
            {
                RestoreSeparatorVisibility(separator);
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

                if (!child.IsVisible)
                {
                    continue;
                }

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

            HasToggleButton = hasToggleButton;
            HasMenuIcon = hasMenuIcon;

            for (int i = 0, count = children.Count; i < count; ++i)
            {
                UIElement child = children[i];

                if (child is IAppBarElement element)
                {
                    element.UpdateApplicationViewState();
                }
            }
        }

        private bool IsPrimaryCommand(DependencyObject element)
        {
            return ToolBar.GetOverflowMode(element) != OverflowMode.Always;
        }

        private void UpdateSeparatorVisibility(int index, AppBarSeparator separator)
        {
            var visibility = separator.Visibility;
            if (index == 0)
            {
                if (visibility == Visibility.Visible)
                {
                    separator.SetCurrentValue(VisibilityProperty, Visibility.Collapsed);
                }
            }
            else
            {
                RestoreSeparatorVisibility(separator);
            }
        }

        private void RestoreSeparatorVisibility(AppBarSeparator separator)
        {
            if (separator.Visibility == Visibility.Collapsed &&
                DependencyPropertyHelper.GetValueSource(separator, VisibilityProperty).IsCurrent)
            {
                separator.InvalidateProperty(VisibilityProperty);
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateChildrenApplicationViewState();
        }
    }
}
