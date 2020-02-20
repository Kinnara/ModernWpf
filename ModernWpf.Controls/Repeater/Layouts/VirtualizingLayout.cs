// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Specialized;
using System.Windows;

namespace ModernWpf.Controls
{
    public class VirtualizingLayout : Layout, IVirtualizingLayoutOverrides
    {
        public VirtualizingLayout()
        {
        }

        protected virtual void InitializeForContextCore(VirtualizingLayoutContext context)
        {
        }

        protected virtual void UninitializeForContextCore(VirtualizingLayoutContext context)
        {
        }

        protected virtual Size MeasureOverride(VirtualizingLayoutContext context, Size availableSize)
        {
            throw new NotImplementedException();
        }

        protected virtual Size ArrangeOverride(VirtualizingLayoutContext context, Size finalSize)
        {
            // Do not throw. If the layout decides to arrange its
            // children during measure, then an ArrangeOverride is not required.
            return finalSize;
        }

        protected virtual void OnItemsChangedCore(VirtualizingLayoutContext context, object source, NotifyCollectionChangedEventArgs args)
        {
            InvalidateMeasure();
        }

        #region IVirtualizingLayoutOverrides
        void IVirtualizingLayoutOverrides.InitializeForContextCore(VirtualizingLayoutContext context)
        {
            InitializeForContextCore(context);
        }

        void IVirtualizingLayoutOverrides.UninitializeForContextCore(VirtualizingLayoutContext context)
        {
            UninitializeForContextCore(context);
        }

        Size IVirtualizingLayoutOverrides.MeasureOverride(VirtualizingLayoutContext context, Size availableSize)
        {
            return MeasureOverride(context, availableSize);
        }

        Size IVirtualizingLayoutOverrides.ArrangeOverride(VirtualizingLayoutContext context, Size finalSize)
        {
            return ArrangeOverride(context, finalSize);
        }

        void IVirtualizingLayoutOverrides.OnItemsChangedCore(VirtualizingLayoutContext context, object source, NotifyCollectionChangedEventArgs args)
        {
            OnItemsChangedCore(context, source, args);
        }
        #endregion
    }

    internal interface IVirtualizingLayoutOverrides
    {
        void InitializeForContextCore(VirtualizingLayoutContext context);
        void UninitializeForContextCore(VirtualizingLayoutContext context);
        Size MeasureOverride(VirtualizingLayoutContext context, Size availableSize);
        Size ArrangeOverride(VirtualizingLayoutContext context, Size finalSize);
        void OnItemsChangedCore(VirtualizingLayoutContext context, object source, NotifyCollectionChangedEventArgs args);
    }
}