// Copyright (c) Kinnara. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace ModernWpf.Controls
{
    public class ItemCollectionTransitionProvider
    {
        public ItemCollectionTransitionProvider()
        {
        }

        public event TypedEventHandler<ItemCollectionTransitionProvider, ItemCollectionTransitionCompletedEventArgs> TransitionCompleted;

        public bool ShouldAnimate(ItemCollectionTransition transition)
        {
            if (transition == null)
            {
                throw new ArgumentNullException(nameof(transition));
            }

            return ShouldAnimateCore(transition);
        }

        protected virtual bool ShouldAnimateCore(ItemCollectionTransition transition)
        {
            throw new NotImplementedException();
        }

        public void QueueTransition(ItemCollectionTransition transition)
        {
            if (transition == null)
            {
                throw new ArgumentNullException(nameof(transition));
            }

            if (!_isRenderingSubscribed)
            {
                _isRenderingSubscribed = true;
                CompositionTarget.Rendering += OnRendering;
            }

            _queuedTransitions.Add(transition);
            if (SystemParameters.ClientAreaAnimation && ShouldAnimate(transition))
            {
                _queuedAnimatedTransitions.Add(transition);
            }
        }

        protected virtual void StartTransitions(IList<ItemCollectionTransition> transitions)
        {
            throw new NotImplementedException();
        }

        internal void NotifyTransitionCompleted(ItemCollectionTransition transition)
        {
            TransitionCompleted?.Invoke(this, new ItemCollectionTransitionCompletedEventArgs(transition));
        }

        private void OnRendering(object sender, EventArgs e)
        {
            CompositionTarget.Rendering -= OnRendering;
            _isRenderingSubscribed = false;

            var transitions = _queuedTransitions.ToArray();
            var animatedTransitions = _queuedAnimatedTransitions.ToArray();
            _queuedTransitions.Clear();
            _queuedAnimatedTransitions.Clear();

            if (animatedTransitions.Length > 0)
            {
                StartTransitions(animatedTransitions);
            }

            foreach (var transition in transitions)
            {
                if (!transition.HasStarted)
                {
                    NotifyTransitionCompleted(transition);
                }
            }
        }

        private readonly List<ItemCollectionTransition> _queuedTransitions = new List<ItemCollectionTransition>();
        private readonly List<ItemCollectionTransition> _queuedAnimatedTransitions = new List<ItemCollectionTransition>();
        private bool _isRenderingSubscribed;
    }
}
