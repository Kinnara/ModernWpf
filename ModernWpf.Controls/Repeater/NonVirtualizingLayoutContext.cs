// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Windows;

namespace ModernWpf.Controls
{
    public abstract class NonVirtualizingLayoutContext : LayoutContext
    {
        protected NonVirtualizingLayoutContext()
        {
        }

        public IReadOnlyList<UIElement> Children => ChildrenCore;

        protected abstract IReadOnlyList<UIElement> ChildrenCore { get; }

        internal VirtualizingLayoutContext GetVirtualizingContextAdapter()
        {
            if (m_contextAdapter == null)
            {
                m_contextAdapter = new LayoutContextAdapter(this);
            }
            return m_contextAdapter;
        }

        private VirtualizingLayoutContext m_contextAdapter;
    }
}