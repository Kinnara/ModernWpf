// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Windows;

namespace ModernWpf.Controls
{
    public abstract class NonVirtualizingLayout : Layout
    {
        protected NonVirtualizingLayout()
        {
        }

        protected internal virtual void InitializeForContextCore(NonVirtualizingLayoutContext context)
        {
        }

        protected internal virtual void UninitializeForContextCore(NonVirtualizingLayoutContext context)
        {
        }

        protected internal abstract Size MeasureOverride(NonVirtualizingLayoutContext context, Size availableSize);

        protected internal abstract Size ArrangeOverride(NonVirtualizingLayoutContext context, Size finalSize);
    }
}
