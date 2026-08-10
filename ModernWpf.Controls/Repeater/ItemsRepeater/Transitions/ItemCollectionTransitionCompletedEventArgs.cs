// Copyright (c) Kinnara. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;

namespace ModernWpf.Controls
{
    public sealed class ItemCollectionTransitionCompletedEventArgs
    {
        internal ItemCollectionTransitionCompletedEventArgs(ItemCollectionTransition transition)
        {
            Transition = transition;
            Element = transition.Element;
        }

        public ItemCollectionTransition Transition { get; }

        public UIElement Element { get; }
    }
}
