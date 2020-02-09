// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;

namespace ModernWpf.Controls
{
    internal class VirtualLayoutContextAdapter : NonVirtualizingLayoutContext
    {
        public VirtualLayoutContextAdapter(VirtualizingLayoutContext virtualizingContext)
        {
            m_virtualizingContext = new WeakReference<VirtualizingLayoutContext>(virtualizingContext);
        }

        protected override object LayoutStateCore
        {
            get
            {
                if (m_virtualizingContext.TryGetTarget(out var context))
                {
                    return context.LayoutState;
                }
                return null;
            }
            set
            {
                if (m_virtualizingContext.TryGetTarget(out var context))
                {
                    context.LayoutState = value;
                }
            }
        }

        protected override IReadOnlyList<UIElement> ChildrenCore
        {
            get
            {
                if (m_children == null)
                {
                    m_virtualizingContext.TryGetTarget(out var context);
                    m_children = new ChildrenCollection<UIElement>(context);
                }

                return m_children;
            }
        }

        private readonly WeakReference<VirtualizingLayoutContext> m_virtualizingContext;
        private IReadOnlyList<UIElement> m_children;

        private class ChildrenCollection<T> : IReadOnlyList<T>, IEnumerable<T> where T : UIElement
        {
            public ChildrenCollection(VirtualizingLayoutContext context)
            {
                m_context = context;
            }

            public int Count => m_context.ItemCount;

            public T this[int index] => (T)m_context.GetOrCreateElementAt(index, ElementRealizationOptions.None);

            public IEnumerator<T> GetEnumerator()
            {
                return new Iterator(this);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            private class Iterator : IEnumerator<T>
            {
                public Iterator(IReadOnlyList<T> childCollection)
                {
                    m_childCollection = childCollection;
                }

                ~Iterator()
                {
                }

                public T Current
                {
                    get
                    {
                        if (m_currentIndex < m_childCollection.Count)
                        {
                            return m_childCollection[m_currentIndex];
                        }
                        else
                        {
                            throw new InvalidOperationException();
                        }
                    }
                }

                object IEnumerator.Current => Current;

                public bool MoveNext()
                {
                    if (m_currentIndex < m_childCollection.Count)
                    {
                        ++m_currentIndex;
                        return (m_currentIndex < m_childCollection.Count);
                    }
                    else
                    {
                        throw new InvalidOperationException();
                    }
                }

                public void Reset()
                {
                    m_currentIndex = -1;
                }

                public void Dispose()
                {
                }

                private readonly IReadOnlyList<T> m_childCollection;
                private int m_currentIndex = -1;
            }

            private readonly VirtualizingLayoutContext m_context;
        }
    }
}