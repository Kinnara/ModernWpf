// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Specialized;

namespace ModernWpf.Controls
{
    public class ItemsSourceView : INotifyCollectionChanged
    {
        public ItemsSourceView(object source)
        {
        }

        public int Count
        {
            get
            {
                if (m_cachedSize == -1)
                {
                    // Call the override the very first time. After this,
                    // we can just update the size when there is a data source change.
                    m_cachedSize = GetSizeCore();
                }

                return m_cachedSize;
            }
        }

        public object GetAt(int index)
        {
            return GetAtCore(index);
        }

        public bool HasKeyIndexMapping => HasKeyIndexMappingCore();

        public string KeyFromIndex(int index)
        {
            return KeyFromIndexCore(index);
        }

        public int IndexFromKey(string key)
        {
            return IndexFromKeyCore(key);
        }

        public int IndexOf(object value)
        {
            return IndexOfCore(value);
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        internal void OnItemsSourceChanged(NotifyCollectionChangedEventArgs args)
        {
            m_cachedSize = GetSizeCore();
            CollectionChanged?.Invoke(this, args);
        }

        internal virtual int GetSizeCore()
        {
            throw new NotImplementedException();
        }

        internal virtual object GetAtCore(int index)
        {
            throw new NotImplementedException();
        }

        internal virtual bool HasKeyIndexMappingCore()
        {
            throw new NotImplementedException();
        }

        internal virtual string KeyFromIndexCore(int index)
        {
            throw new NotImplementedException();
        }

        internal virtual int IndexFromKeyCore(string id)
        {
            throw new NotImplementedException();
        }

        internal virtual int IndexOfCore(object value)
        {
            throw new NotImplementedException();
        }

        private int m_cachedSize = -1;

        internal class CollectionChangedRevoker : EventRevoker<ItemsSourceView, NotifyCollectionChangedEventHandler>
        {
            public CollectionChangedRevoker(ItemsSourceView source, NotifyCollectionChangedEventHandler handler) : base(source, handler)
            {
            }

            protected override void AddHandler(ItemsSourceView source, NotifyCollectionChangedEventHandler handler)
            {
                source.CollectionChanged += handler;
            }

            protected override void RemoveHandler(ItemsSourceView source, NotifyCollectionChangedEventHandler handler)
            {
                source.CollectionChanged -= handler;
            }
        }
    }


}
