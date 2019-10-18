// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;

namespace ModernWpf.Controls
{
    public abstract class VirtualizingLayoutContext : LayoutContext
    {
        protected VirtualizingLayoutContext()
        {
        }

        public object GetItemAt(int index)
        {
            return GetItemAtCore(index);
        }

        public UIElement GetOrCreateElementAt(int index)
        {
            // Calling this way because GetOrCreateElementAtCore is ambiguous.
            // Use .as instead of try_as because try_as uses non-delegating inner and we need to call the outer for overrides.
            return GetOrCreateElementAtCore(index, ElementRealizationOptions.None);
        }

        public UIElement GetOrCreateElementAt(int index, ElementRealizationOptions options)
        {
            // Calling this way because GetOrCreateElementAtCore is ambiguous.
            // Use .as instead of try_as because try_as uses non-delegating inner and we need to call the outer for overrides.
            return GetOrCreateElementAtCore(index, options);
        }

        public void RecycleElement(UIElement element)
        {
            RecycleElementCore(element);
        }

        protected internal abstract int ItemCountCore();

        protected abstract object GetItemAtCore(int index);

        protected abstract Rect RealizationRectCore();

        protected abstract UIElement GetOrCreateElementAtCore(int index, ElementRealizationOptions options);

        protected abstract void RecycleElementCore(UIElement element);

        public Point LayoutOrigin
        {
            get => LayoutOriginCore;
            set => LayoutOriginCore = value;
        }

        public int ItemCount => ItemCountCore();

        public Rect RealizationRect => RealizationRectCore();

        public int RecommendedAnchorIndex => RecommendedAnchorIndexCore;

        protected virtual Point LayoutOriginCore { get; set; }

        protected virtual int RecommendedAnchorIndexCore { get; }

        internal NonVirtualizingLayoutContext GetNonVirtualizingContextAdapter()
        {
            if (m_contextAdapter == null)
            {
                m_contextAdapter = new VirtualLayoutContextAdapter(this);
            }
            return m_contextAdapter;
        }

        private NonVirtualizingLayoutContext m_contextAdapter;
    }
}