// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Windows;

namespace ModernWpf.Controls
{
    internal class LayoutContextAdapter : VirtualizingLayoutContext
    {
        public LayoutContextAdapter(NonVirtualizingLayoutContext nonVirtualizingContext)
        {
            m_nonVirtualizingContext = new WeakReference<NonVirtualizingLayoutContext>(nonVirtualizingContext);
        }

        protected override object LayoutStateCore
        {
            get
            {
                if (m_nonVirtualizingContext.TryGetTarget(out var context))
                {
                    return context.LayoutState;
                }
                return null;
            }
            set
            {
                if (m_nonVirtualizingContext.TryGetTarget(out var context))
                {
                    ((ILayoutContextOverrides)context).LayoutStateCore = value;
                }
            }
        }

        protected override int ItemCountCore()
        {
            if (m_nonVirtualizingContext.TryGetTarget(out var context))
            {
                return context.Children.Count;
            }
            return 0;
        }

        protected override object GetItemAtCore(int index)
        {
            if (m_nonVirtualizingContext.TryGetTarget(out var context))
            {
                return context.Children[index];
            }
            return null;
        }

        protected override UIElement GetOrCreateElementAtCore(int index, ElementRealizationOptions options)
        {
            if (m_nonVirtualizingContext.TryGetTarget(out var context))
            {
                return context.Children[index];
            }
            return null;
        }

        protected override void RecycleElementCore(UIElement element)
        {
        }

        protected int GetElementIndexCore(UIElement element)
        {
            if (m_nonVirtualizingContext.TryGetTarget(out var context))
            {
                var children = context.Children;
                for (int i = 0; i < children.Count; i++)
                {
                    if (children[i] == element)
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        protected override Rect RealizationRectCore()
        {
            return new Rect(0, 0, double.PositiveInfinity, double.PositiveInfinity);
        }

        protected override int RecommendedAnchorIndexCore => -1;

        protected override Point LayoutOriginCore
        {
            get => new Point(0, 0);
            set
            {
                if (value != new Point(0, 0))
                {
                    throw new ArgumentOutOfRangeException(nameof(value),
                        "LayoutOrigin must be at (0,0) when RealizationRect is infinite sized.");
                }
            }
        }

        private readonly WeakReference<NonVirtualizingLayoutContext> m_nonVirtualizingContext;
    }
}
