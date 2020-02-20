// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Windows;

namespace ModernWpf.Controls
{
    public class NonVirtualizingLayout : Layout, INonVirtualizingLayoutOverrides
    {
        public NonVirtualizingLayout()
        {
        }

        protected virtual void InitializeForContextCore(NonVirtualizingLayoutContext context)
        {
        }

        protected virtual void UninitializeForContextCore(NonVirtualizingLayoutContext context)
        {
        }

        protected virtual Size MeasureOverride(NonVirtualizingLayoutContext context, Size availableSize)
        {
            throw new NotImplementedException();
        }

        protected virtual Size ArrangeOverride(NonVirtualizingLayoutContext context, Size finalSize)
        {
            throw new NotImplementedException();
        }

        #region INonVirtualizingLayoutOverrides
        void INonVirtualizingLayoutOverrides.InitializeForContextCore(NonVirtualizingLayoutContext context)
        {
            InitializeForContextCore(context);
        }

        void INonVirtualizingLayoutOverrides.UninitializeForContextCore(NonVirtualizingLayoutContext context)
        {
            UninitializeForContextCore(context);
        }

        Size INonVirtualizingLayoutOverrides.MeasureOverride(NonVirtualizingLayoutContext context, Size availableSize)
        {
            return MeasureOverride(context, availableSize);
        }

        Size INonVirtualizingLayoutOverrides.ArrangeOverride(NonVirtualizingLayoutContext context, Size finalSize)
        {
            return ArrangeOverride(context, finalSize);
        }
        #endregion
    }

    internal interface INonVirtualizingLayoutOverrides
    {
        void InitializeForContextCore(NonVirtualizingLayoutContext context);
        void UninitializeForContextCore(NonVirtualizingLayoutContext context);
        Size MeasureOverride(NonVirtualizingLayoutContext context, Size availableSize);
        Size ArrangeOverride(NonVirtualizingLayoutContext context, Size finalSize);
    }
}
