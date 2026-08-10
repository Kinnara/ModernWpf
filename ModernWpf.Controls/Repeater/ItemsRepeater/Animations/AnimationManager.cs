// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Specialized;
using System.Collections.Generic;
using System.Windows;

namespace ModernWpf.Controls
{
    // Internal component that contains all
    // the animation related logic for ItemsRepeater.
    internal class AnimationManager
    {
        public AnimationManager(ItemsRepeater owner)
        {
            m_owner = owner;
            // ItemsRepeater is not fully constructed yet. Don't interact with it.
        }

        public void OnAnimatorChanged(ElementAnimator newAnimator)
        {
            // While an element is hiding, we have ownership of it. We need
            // to know when its animation completes so that we give it back
            // to the view generator.
            if (m_animator != null)
            {
                m_animator.HideAnimationCompleted -= OnHideAnimationCompleted;
            }

            m_animator = newAnimator;

            if (newAnimator != null)
            {
                newAnimator.HideAnimationCompleted += OnHideAnimationCompleted;
            }
        }

        public void OnTransitionProviderChanged(ItemCollectionTransitionProvider newTransitionProvider)
        {
            if (m_transitionProvider != null)
            {
                m_transitionProvider.TransitionCompleted -= OnTransitionCompleted;
            }

            CompleteTransitioningOutElements();
            m_transitionProvider = newTransitionProvider;

            if (newTransitionProvider != null)
            {
                newTransitionProvider.TransitionCompleted += OnTransitionCompleted;
            }
        }

        public void OnLayoutChanging()
        {
            m_hasRecordedLayoutTransitions = true;
        }

        public void OnItemsSourceChanged(object source, NotifyCollectionChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    m_hasRecordedAdds = true;
                    break;
                case NotifyCollectionChangedAction.Remove:
                    m_hasRecordedRemoves = true;
                    break;
                case NotifyCollectionChangedAction.Replace:
                    m_hasRecordedAdds = true;
                    m_hasRecordedRemoves = true;
                    break;
                case NotifyCollectionChangedAction.Reset:
                    m_hasRecordedResets = true;
                    break;
            }
        }

        public void OnElementPrepared(UIElement element)
        {
            if (m_transitionProvider != null)
            {
                var triggers = GetTransitionTriggers(includeRemove: false);
                if (triggers != 0)
                {
                    m_transitionProvider.QueueTransition(
                        new ItemCollectionTransition(
                            m_transitionProvider,
                            element,
                            ItemCollectionTransitionOperation.Add,
                            triggers));
                }
            }
            else if (m_animator != null)
            {
                var context = AnimationContext.None;
                if (m_hasRecordedAdds) context |= AnimationContext.CollectionChangeAdd;
                if (m_hasRecordedResets) context |= AnimationContext.CollectionChangeReset;
                if (m_hasRecordedLayoutTransitions) context |= AnimationContext.LayoutTransition;

                if (context != AnimationContext.None)
                {
                    m_animator.OnElementShown(element, context);
                }
            }
        }

        public bool ClearElement(UIElement element)
        {
            bool canClear = false;

            if (m_transitionProvider != null)
            {
                var triggers = ItemCollectionTransitionTriggers.CollectionChangeRemove;
                if (m_hasRecordedResets)
                {
                    triggers = ItemCollectionTransitionTriggers.CollectionChangeReset;
                }

                if (m_hasRecordedRemoves || m_hasRecordedResets)
                {
                    var transition = new ItemCollectionTransition(
                        m_transitionProvider,
                        element,
                        ItemCollectionTransitionOperation.Remove,
                        triggers);

                    canClear = m_transitionProvider.ShouldAnimate(transition);
                    if (canClear)
                    {
                        m_transitioningOutElements.Add(element);
                        m_transitionProvider.QueueTransition(transition);
                    }
                }
            }
            else if (m_animator != null)
            {
                var context = AnimationContext.None;
                if (m_hasRecordedRemoves) context |= AnimationContext.CollectionChangeRemove;
                if (m_hasRecordedResets) context |= AnimationContext.CollectionChangeReset;

                canClear =
                    context != AnimationContext.None &&
                    m_animator.HasHideAnimation(element, context);

                if (canClear)
                {
                    m_animator.OnElementHidden(element, context);
                }
            }

            return canClear;
        }

        public void OnElementBoundsChanged(UIElement element, Rect oldBounds, Rect newBounds)
        {
            if (m_transitionProvider != null)
            {
                var triggers = GetTransitionTriggers(includeRemove: true);
                if (triggers == 0)
                {
                    triggers = ItemCollectionTransitionTriggers.LayoutTransition;
                }

                m_transitionProvider.QueueTransition(
                    new ItemCollectionTransition(
                        m_transitionProvider,
                        element,
                        triggers,
                        oldBounds,
                        newBounds));
            }
            else if (m_animator != null)
            {
                var context = AnimationContext.None;
                if (m_hasRecordedAdds) context |= AnimationContext.CollectionChangeAdd;
                if (m_hasRecordedRemoves) context |= AnimationContext.CollectionChangeRemove;
                if (m_hasRecordedResets) context |= AnimationContext.CollectionChangeReset;
                if (m_hasRecordedLayoutTransitions) context |= AnimationContext.LayoutTransition;

                m_animator.OnElementBoundsChanged(element, context, oldBounds, newBounds);
            }
        }

        public void OnOwnerArranged()
        {
            m_hasRecordedAdds = false;
            m_hasRecordedRemoves = false;
            m_hasRecordedResets = false;
            m_hasRecordedLayoutTransitions = false;
        }

        private void OnHideAnimationCompleted(ElementAnimator sender, UIElement element)
        {
            if (CachedVisualTreeHelpers.GetParent(element) == m_owner)
            {
                m_owner.ViewManager.ClearElementToElementFactory(element);

                // Invalidate arrange so that repeater can arrange this element off-screen.
                m_owner.InvalidateArrange();
            }
        }

        private ItemCollectionTransitionTriggers GetTransitionTriggers(bool includeRemove)
        {
            var triggers = (ItemCollectionTransitionTriggers)0;
            if (m_hasRecordedAdds) triggers |= ItemCollectionTransitionTriggers.CollectionChangeAdd;
            if (includeRemove && m_hasRecordedRemoves) triggers |= ItemCollectionTransitionTriggers.CollectionChangeRemove;
            if (m_hasRecordedResets) triggers |= ItemCollectionTransitionTriggers.CollectionChangeReset;
            if (m_hasRecordedLayoutTransitions) triggers |= ItemCollectionTransitionTriggers.LayoutTransition;
            return triggers;
        }

        private void OnTransitionCompleted(
            ItemCollectionTransitionProvider sender,
            ItemCollectionTransitionCompletedEventArgs args)
        {
            if (args.Transition.Operation == ItemCollectionTransitionOperation.Remove &&
                m_transitioningOutElements.Remove(args.Element) &&
                CachedVisualTreeHelpers.GetParent(args.Element) == m_owner)
            {
                m_owner.ViewManager.ClearElementToElementFactory(args.Element);
                m_owner.InvalidateArrange();
            }
        }

        private void CompleteTransitioningOutElements()
        {
            if (m_transitioningOutElements.Count == 0)
            {
                return;
            }

            var elements = new List<UIElement>(m_transitioningOutElements);
            m_transitioningOutElements.Clear();
            foreach (var element in elements)
            {
                if (CachedVisualTreeHelpers.GetParent(element) == m_owner)
                {
                    m_owner.ViewManager.ClearElementToElementFactory(element);
                }
            }

            m_owner.InvalidateArrange();
        }

        private readonly ItemsRepeater m_owner;
        private ElementAnimator m_animator;
        private ItemCollectionTransitionProvider m_transitionProvider;
        private readonly HashSet<UIElement> m_transitioningOutElements = new HashSet<UIElement>();

        // We infer the animation context
        // from heuristics like whether or not
        // we observed a collection change or a
        // layout transition during the current
        // tick.
        private bool m_hasRecordedAdds;
        private bool m_hasRecordedRemoves;
        private bool m_hasRecordedResets;
        private bool m_hasRecordedLayoutTransitions;
    }
}
