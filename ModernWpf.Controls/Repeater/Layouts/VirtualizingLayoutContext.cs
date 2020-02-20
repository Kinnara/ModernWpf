// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Windows;

namespace ModernWpf.Controls
{
    public class VirtualizingLayoutContext : LayoutContext, IVirtualizingLayoutContextOverrides
    {
        public VirtualizingLayoutContext()
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

        protected virtual int ItemCountCore()
        {
            throw new NotImplementedException();
        }

        protected virtual object GetItemAtCore(int index)
        {
            throw new NotImplementedException();
        }

        protected virtual Rect RealizationRectCore()
        {
            throw new NotImplementedException();
        }

        protected virtual UIElement GetOrCreateElementAtCore(int index, ElementRealizationOptions options)
        {
            throw new NotImplementedException();
        }

        protected virtual void RecycleElementCore(UIElement element)
        {
            throw new NotImplementedException();
        }

        public Point LayoutOrigin
        {
            get => LayoutOriginCore;
            set => LayoutOriginCore = value;
        }

        public int ItemCount => ItemCountCore();

        public Rect RealizationRect => RealizationRectCore();

        public int RecommendedAnchorIndex => RecommendedAnchorIndexCore;

        protected virtual Point LayoutOriginCore
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        protected virtual int RecommendedAnchorIndexCore => throw new NotImplementedException();

        internal NonVirtualizingLayoutContext GetNonVirtualizingContextAdapter()
        {
            if (m_contextAdapter == null)
            {
                m_contextAdapter = new VirtualLayoutContextAdapter(this);
            }
            return m_contextAdapter;
        }

        #region IVirtualizingLayoutContextOverrides
        int IVirtualizingLayoutContextOverrides.ItemCountCore()
        {
            return ItemCountCore();
        }

        object IVirtualizingLayoutContextOverrides.GetItemAtCore(int index)
        {
            return GetItemAtCore(index);
        }

        UIElement IVirtualizingLayoutContextOverrides.GetOrCreateElementAtCore(int index, ElementRealizationOptions options)
        {
            return GetOrCreateElementAtCore(index, options);
        }

        void IVirtualizingLayoutContextOverrides.RecycleElementCore(UIElement element)
        {
            RecycleElementCore(element);
        }

        Rect IVirtualizingLayoutContextOverrides.RealizationRectCore()
        {
            return RealizationRectCore();
        }

        int IVirtualizingLayoutContextOverrides.RecommendedAnchorIndexCore => RecommendedAnchorIndexCore;

        Point IVirtualizingLayoutContextOverrides.LayoutOriginCore
        {
            get => LayoutOriginCore;
            set => LayoutOriginCore = value;
        }
        #endregion

        private NonVirtualizingLayoutContext m_contextAdapter;
    }

    internal interface IVirtualizingLayoutContextOverrides
    {
        int ItemCountCore();
        object GetItemAtCore(int index);
        UIElement GetOrCreateElementAtCore(int index, ElementRealizationOptions options);
        void RecycleElementCore(UIElement element);

        Rect RealizationRectCore();

        int RecommendedAnchorIndexCore { get; }

        Point LayoutOriginCore { get; set; }
    }
}