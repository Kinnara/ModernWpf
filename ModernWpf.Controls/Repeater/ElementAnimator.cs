// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace ModernWpf.Controls
{
    // Given some elements and their animation context, ElementAnimator
    // animates them (show, hide and bounds change) and ensures the timing
    // is correct (hide -> bounds change -> show).
    // It's possible to customize the animations by inheriting from ElementAnimator
    // and overriding virtual/abstract members.
    public class ElementAnimator
    {
        public event ElementAnimationCompleted ShowAnimationCompleted;

        public event ElementAnimationCompleted HideAnimationCompleted;

        public event ElementAnimationCompleted BoundsChangeAnimationCompleted;

        public void OnElementShown(
            UIElement element,
            AnimationContext context)
        {
            if (HasShowAnimation(element, context))
            {
                HasShowAnimationsPending = true;
                SharedContext |= context;
                QueueElementForAnimation(new ElementInfo(
                    element,
                    AnimationTrigger.Show,
                    context));
            }
        }

        public void OnElementHidden(
            UIElement element,
            AnimationContext context)
        {
            if (HasHideAnimation(element, context))
            {
                HasHideAnimationsPending = true;
                SharedContext |= context;
                QueueElementForAnimation(new ElementInfo(
                    element,
                    AnimationTrigger.Hide,
                    context));
            }
        }

        public void OnElementBoundsChanged(
            UIElement element,
            AnimationContext context,
            Rect oldBounds,
            Rect newBounds)
        {
            if (HasBoundsChangeAnimation(element, context, oldBounds, newBounds))
            {
                HasBoundsChangeAnimationsPending = true;
                SharedContext |= context;
                QueueElementForAnimation(new ElementInfo(
                    element,
                    AnimationTrigger.BoundsChange,
                    context,
                    oldBounds,
                    newBounds));
            }
        }

        public bool HasShowAnimation(
            UIElement element,
            AnimationContext context)
        {
            return HasShowAnimationCore(element, context);
        }

        public bool HasHideAnimation(
            UIElement element,
            AnimationContext context)
        {
            return HasHideAnimationCore(element, context);
        }

        public bool HasBoundsChangeAnimation(
            UIElement element,
            AnimationContext context,
            Rect oldBounds,
            Rect newBounds)
        {
            return HasBoundsChangeAnimationCore(element, context, oldBounds, newBounds);
        }

        protected virtual bool HasShowAnimationCore(
            UIElement element,
            AnimationContext context)
        {
            throw new NotImplementedException();
        }

        protected virtual bool HasHideAnimationCore(
            UIElement element,
            AnimationContext context)
        {
            throw new NotImplementedException();
        }

        protected virtual bool HasBoundsChangeAnimationCore(
            UIElement element,
            AnimationContext context,
            Rect oldBounds,
            Rect newBounds)
        {
            throw new NotImplementedException();
        }

        protected virtual void StartShowAnimation(
            UIElement element,
            AnimationContext context)
        {
            throw new NotImplementedException();
        }

        protected virtual void StartHideAnimation(
            UIElement element,
            AnimationContext context)
        {
            throw new NotImplementedException();
        }

        protected virtual void StartBoundsChangeAnimation(
            UIElement element,
            AnimationContext context,
            Rect oldBounds,
            Rect newBounds)
        {
            throw new NotImplementedException();
        }

        protected bool HasShowAnimationsPending { get; private set; }

        protected bool HasHideAnimationsPending { get; private set; }

        protected bool HasBoundsChangeAnimationsPending { get; private set; }

        protected AnimationContext SharedContext { get; private set; }

        protected void OnShowAnimationCompleted(UIElement element)
        {
            ShowAnimationCompleted?.Invoke(this, element);
        }

        protected void OnHideAnimationCompleted(UIElement element)
        {
            HideAnimationCompleted?.Invoke(this, element);
        }

        protected void OnBoundsChangeAnimationCompleted(UIElement element)
        {
            BoundsChangeAnimationCompleted?.Invoke(this, element);
        }

        private void QueueElementForAnimation(ElementInfo elementInfo)
        {
            m_animatingElements.Add(elementInfo);
            if (m_animatingElements.Count == 1)
            {
                CompositionTarget.Rendering += OnRendering;
            }
        }

        private void OnRendering(object sender, EventArgs args)
        {
            CompositionTarget.Rendering -= OnRendering;

            try
            {
                for (int i = 0; i < m_animatingElements.Count; i++)
                {
                    var elementInfo = m_animatingElements[i];
                    switch (elementInfo.Trigger)
                    {
                        case AnimationTrigger.Show:
                            // Call into the derivied class's StartShowAnimation override
                            StartShowAnimation(elementInfo.Element, elementInfo.Context);
                            break;
                        case AnimationTrigger.Hide:
                            // Call into the derivied class's StartHideAnimation override
                            StartHideAnimation(elementInfo.Element, elementInfo.Context);
                            break;
                        case AnimationTrigger.BoundsChange:
                            // Call into the derivied class's StartBoundsChangeAnimation override
                            StartBoundsChangeAnimation(
                                elementInfo.Element,
                                elementInfo.Context,
                                elementInfo.OldBounds,
                                elementInfo.NewBounds);
                            break;
                    }
                }
            }
            finally
            {
                ResetState();
            }
        }

        private void ResetState()
        {
            m_animatingElements.Clear();
            HasShowAnimationsPending = HasHideAnimationsPending = HasBoundsChangeAnimationsPending = false;
            SharedContext = AnimationContext.None;
        }

        private enum AnimationTrigger
        {
            Show,
            Hide,
            BoundsChange
        }

        private struct ElementInfo
        {
            public ElementInfo(
                UIElement element,
                AnimationTrigger trigger,
                AnimationContext context)
            {
                Element = element;
                Trigger = trigger;
                Context = context;
                OldBounds = default;
                NewBounds = default;
            }

            public ElementInfo(
                UIElement element,
                AnimationTrigger trigger,
                AnimationContext context,
                Rect oldBounds,
                Rect newBounds)
            {
                Element = element;
                Trigger = trigger;
                Context = context;
                OldBounds = oldBounds;
                NewBounds = newBounds;
            }

            public UIElement Element { get; }
            public AnimationTrigger Trigger { get; }
            public AnimationContext Context { get; }
            // Valid for Trigger == BoundsChange
            public Rect OldBounds { get; }
            public Rect NewBounds { get; }
        }

        private readonly List<ElementInfo> m_animatingElements = new List<ElementInfo>();
    }

    public delegate void ElementAnimationCompleted(ElementAnimator sender, UIElement element);
}