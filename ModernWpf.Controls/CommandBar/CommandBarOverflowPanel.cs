using System;
using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.Controls.Primitives
{
    internal class CommandBarOverflowPanel : Panel
    {
        public CommandBarOverflowPanel()
        {
            Loaded += OnLoaded;
        }

        internal CommandBar OwnerCommandBar { get; set; }

        internal bool HasToggleButton { get; private set; }

        internal bool HasMenuIcon { get; private set; }

        protected override Size MeasureOverride(Size constraint)
        {
            UpdateChildrenApplicationViewState();

            Size desiredSize = new();
            UIElementCollection children = InternalChildren;
            Size childConstraint = constraint;
            childConstraint.Height = double.PositiveInfinity;

            for (int i = 0, count = children.Count; i < count; ++i)
            {
                UIElement child = children[i];

                if (child == null)
                {
                    continue;
                }

                child.Measure(childConstraint);
                Size childDesiredSize = child.DesiredSize;

                desiredSize.Width = Math.Max(desiredSize.Width, childDesiredSize.Width);
                desiredSize.Height += childDesiredSize.Height;
            }

            return desiredSize;
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            UIElementCollection children = InternalChildren;
            Rect childBounds = new(arrangeBounds);
            double previousChildHeight = 0.0;

            for (int i = 0, count = children.Count; i < count; ++i)
            {
                UIElement child = children[i];

                if (child == null)
                {
                    continue;
                }

                childBounds.Y += previousChildHeight;
                previousChildHeight = child.DesiredSize.Height;
                childBounds.Height = previousChildHeight;
                childBounds.Width = Math.Max(arrangeBounds.Width, child.DesiredSize.Width);

                child.Arrange(childBounds);
            }

            return arrangeBounds;
        }

        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            base.OnVisualChildrenChanged(visualAdded, visualRemoved);

            if (visualRemoved is DependencyObject removedElement)
            {
                AppBarElementProperties.SetUseOverflowStyle(removedElement, false);

                if (removedElement is IAppBarButtonElement appBarButtonElement)
                {
                    appBarButtonElement.SetOverflowStyleParams(false, false, false);
                    appBarButtonElement.UpdateTemplateSettings(0);
                }
            }

            UpdateChildrenApplicationViewState();
        }

        internal void UpdateChildrenApplicationViewState()
        {
            bool hasToggleButton = false;
            bool hasMenuIcon = false;

            UIElementCollection children = InternalChildren;

            for (int i = 0, count = children.Count; i < count; ++i)
            {
                UIElement child = children[i];

                if (child == null || !child.IsVisible)
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

            AppBarElementProperties.UpdateOverflowStyleParams(
                children,
                true,
                OwnerCommandBar?.GetInputModeForOverflowCommands() ?? AppBarButtonInputMode.Default);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateChildrenApplicationViewState();
        }
    }
}
