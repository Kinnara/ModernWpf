// Copyright (c) Kinnara. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Windows;

namespace ModernWpf.Controls
{
    [Flags]
    public enum ItemCollectionTransitionTriggers
    {
        CollectionChangeAdd = 1,
        CollectionChangeRemove = 2,
        CollectionChangeReset = 4,
        LayoutTransition = 8
    }

    public enum ItemCollectionTransitionOperation
    {
        Add = 0,
        Remove = 1,
        Move = 2
    }

    public sealed class ItemCollectionTransition
    {
        internal ItemCollectionTransition(
            ItemCollectionTransitionProvider owningProvider,
            UIElement element,
            ItemCollectionTransitionOperation operation,
            ItemCollectionTransitionTriggers triggers)
            : this(owningProvider, element, operation, triggers, Rect.Empty, Rect.Empty)
        {
        }

        internal ItemCollectionTransition(
            ItemCollectionTransitionProvider owningProvider,
            UIElement element,
            ItemCollectionTransitionTriggers triggers,
            Rect oldBounds,
            Rect newBounds)
            : this(
                owningProvider,
                element,
                ItemCollectionTransitionOperation.Move,
                triggers,
                oldBounds,
                newBounds)
        {
        }

        private ItemCollectionTransition(
            ItemCollectionTransitionProvider owningProvider,
            UIElement element,
            ItemCollectionTransitionOperation operation,
            ItemCollectionTransitionTriggers triggers,
            Rect oldBounds,
            Rect newBounds)
        {
            OwningProvider = owningProvider ?? throw new ArgumentNullException(nameof(owningProvider));
            Element = element ?? throw new ArgumentNullException(nameof(element));
            Operation = operation;
            Triggers = triggers;
            OldBounds = oldBounds;
            NewBounds = newBounds;
        }

        public ItemCollectionTransitionOperation Operation { get; }

        public ItemCollectionTransitionTriggers Triggers { get; }

        public Rect OldBounds { get; }

        public Rect NewBounds { get; }

        public bool HasStarted => _progress != null;

        public ItemCollectionTransitionProgress Start()
        {
            if (_progress == null)
            {
                _progress = new ItemCollectionTransitionProgress(this, Element);
            }

            return _progress;
        }

        internal ItemCollectionTransitionProvider OwningProvider { get; }

        internal UIElement Element { get; }

        private ItemCollectionTransitionProgress _progress;
    }
}
