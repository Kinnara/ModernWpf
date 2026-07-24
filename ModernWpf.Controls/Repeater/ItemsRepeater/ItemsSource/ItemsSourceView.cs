// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace ModernWpf.Controls
{
    public class ItemsSourceView : INotifyCollectionChanged
    {
        protected ItemsSourceView()
        {
        }

        public ItemsSourceView(object source)
        {
            Initialize(source);
        }

        ~ItemsSourceView()
        {
            UnListenToCollectionChanges();
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
            if (m_wrappedIterable != null)
            {
                m_vector = WrapIterable(m_wrappedIterable);
            }

            m_cachedSize = GetSizeCore();
            CollectionChanged?.Invoke(this, args);
        }

        internal virtual int GetSizeCore()
        {
            if (m_vectorView != null)
            {
                return m_vectorView.Count;
            }

            if (m_vector != null)
            {
                return m_vector.Count;
            }

            throw new NotImplementedException();
        }

        internal virtual object GetAtCore(int index)
        {
            if (m_vectorView != null)
            {
                return m_vectorView[index];
            }

            if (m_vector != null)
            {
                return m_vector[index];
            }

            throw new NotImplementedException();
        }

        internal virtual bool HasKeyIndexMappingCore()
        {
            return m_uniqueIdMapping != null;
        }

        internal virtual string KeyFromIndexCore(int index)
        {
            if (m_uniqueIdMapping != null)
            {
                return m_uniqueIdMapping.KeyFromIndex(index);
            }

            throw new NotImplementedException();
        }

        internal virtual int IndexFromKeyCore(string id)
        {
            if (m_uniqueIdMapping != null)
            {
                return m_uniqueIdMapping.IndexFromKey(id);
            }

            throw new NotImplementedException();
        }

        internal virtual int IndexOfCore(object value)
        {
            if (m_vectorView != null)
            {
                for (int i = 0; i < m_vectorView.Count; i++)
                {
                    if (EqualityComparer<object>.Default.Equals(m_vectorView[i], value))
                    {
                        return i;
                    }
                }
            }
            else if (m_vector != null)
            {
                var index = m_vector.IndexOf(value);
                if (index >= 0)
                {
                    return index;
                }
            }

            return -1;
        }

        private int m_cachedSize = -1;

        private void Initialize(object source)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (source is IList vector)
            {
                m_vector = vector;
            }
            else if (source is IReadOnlyList<object> vectorView)
            {
                m_vectorView = vectorView;
            }
            else if (source is IEnumerable iterable)
            {
                m_wrappedIterable = iterable;
                m_vector = WrapIterable(iterable);
            }
            else
            {
                throw new ArgumentException("Argument 'source' is not a supported vector.", nameof(source));
            }

            m_uniqueIdMapping = source as IKeyIndexMapping;
            ListenToCollectionChanges(source as INotifyCollectionChanged);
        }

        private static IList WrapIterable(IEnumerable iterable)
        {
            var vector = new List<object>();
            var iterator = iterable.GetEnumerator();
            while (iterator.MoveNext())
            {
                vector.Add(iterator.Current);
            }

            return vector;
        }

        private void ListenToCollectionChanges(INotifyCollectionChanged source)
        {
            if (source != null)
            {
                m_notifyCollectionChanged = source;
                CollectionChangedEventManager.AddHandler(source, OnCollectionChanged);
            }
        }

        private void UnListenToCollectionChanges()
        {
            if (m_notifyCollectionChanged != null)
            {
                CollectionChangedEventManager.RemoveHandler(m_notifyCollectionChanged, OnCollectionChanged);
                m_notifyCollectionChanged = null;
            }
        }

        private void OnCollectionChanged(
             object sender,
             NotifyCollectionChangedEventArgs e)
        {
            OnItemsSourceChanged(e);
        }

        private IList m_vector;
        private IReadOnlyList<object> m_vectorView;
        private IEnumerable m_wrappedIterable;
        private INotifyCollectionChanged m_notifyCollectionChanged;
        private IKeyIndexMapping m_uniqueIdMapping;

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
