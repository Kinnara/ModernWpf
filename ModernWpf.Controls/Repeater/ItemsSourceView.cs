// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Specialized;

namespace ModernWpf.Controls
{
    public abstract class ItemsSourceView : INotifyCollectionChanged
    {
        protected ItemsSourceView(object source)
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

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        internal void OnItemsSourceChanged(NotifyCollectionChangedEventArgs args)
        {
            m_cachedSize = GetSizeCore();
            CollectionChanged?.Invoke(this, args);
        }

        internal abstract int GetSizeCore();

        internal abstract object GetAtCore(int index);

        internal abstract bool HasKeyIndexMappingCore();

        internal abstract string KeyFromIndexCore(int index);

        internal abstract int IndexFromKeyCore(string id);

        private int m_cachedSize = -1;
    }
}
