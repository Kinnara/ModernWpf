// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Windows;

namespace ModernWpf.Controls
{
    internal class RepeaterLayoutContext : VirtualizingLayoutContext
    {
        public RepeaterLayoutContext(ItemsRepeater owner)
        {
            m_owner = new WeakReference<ItemsRepeater>(owner);
        }

        protected override object LayoutStateCore
        {
            get => GetOwner().LayoutState;
            set => GetOwner().LayoutState = value;
        }

        protected override int ItemCountCore()
        {
            var dataSource = GetOwner().ItemsSourceView;
            if (dataSource != null)
            {
                return dataSource.Count;
            }
            return 0;
        }

        protected override UIElement GetOrCreateElementAtCore(int index, ElementRealizationOptions options)
        {
            return GetOwner().GetElementImpl(index,
               (options & ElementRealizationOptions.ForceCreate) == ElementRealizationOptions.ForceCreate,
               (options & ElementRealizationOptions.SuppressAutoRecycle) == ElementRealizationOptions.SuppressAutoRecycle);
        }

        protected override object GetItemAtCore(int index)
        {
            return GetOwner().ItemsSourceView.GetAt(index);
        }

        protected override void RecycleElementCore(UIElement element)
        {
            var owner = GetOwner();
            owner.ClearElementImpl(element);
        }

        protected override Rect RealizationRectCore()
        {
            return GetOwner().RealizationWindow;
        }

        protected override int RecommendedAnchorIndexCore
        {
            get
            {
                int anchorIndex = -1;
                var repeater = GetOwner();
                var anchor = repeater.SuggestedAnchor;
                if (anchor != null)
                {
                    anchorIndex = repeater.GetElementIndex(anchor);
                }

                return anchorIndex;
            }
        }

        protected override Point LayoutOriginCore
        {
            get => GetOwner().LayoutOrigin;
            set => GetOwner().LayoutOrigin = value;
        }

        private ItemsRepeater GetOwner()
        {
            m_owner.TryGetTarget(out ItemsRepeater owner);
            return owner;
        }

        // We hold a weak reference to prevent a leaking reference
        // cycle between the ItemsRepeater and its layout.
        private readonly WeakReference<ItemsRepeater> m_owner;
    }
}
