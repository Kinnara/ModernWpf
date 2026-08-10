// Copyright (c) Kinnara. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;

namespace ModernWpf.Controls
{
    public sealed class ItemCollectionTransitionProgress
    {
        internal ItemCollectionTransitionProgress(ItemCollectionTransition transition, UIElement element)
        {
            Transition = transition;
            Element = element;
        }

        public ItemCollectionTransition Transition { get; }

        public UIElement Element { get; }

        public void Complete()
        {
            if (!_isCompleted)
            {
                _isCompleted = true;
                Transition.OwningProvider.NotifyTransitionCompleted(Transition);
            }
        }

        private bool _isCompleted;
    }
}
