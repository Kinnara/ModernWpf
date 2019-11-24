// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;

namespace ModernWpf.Controls
{
    internal class SelectedItems<T> : IReadOnlyList<T>
    {
        public SelectedItems(List<SelectedItemInfo> infos,
            Func<List<SelectedItemInfo>, int, T> getAtImpl)
        {
            m_infos = infos;
            m_getAtImpl = getAtImpl;
            foreach (var info in infos)
            {
                if (info.Node.TryGetTarget(out var node))
                {
                    m_totalCount += node.SelectedCount;
                }
                else
                {
                    throw new Exception("Selection changed after the SelectedIndices/Items property was read.");
                }
            }
        }

        ~SelectedItems()
        {
            m_infos.Clear();
        }

        public int Count => m_totalCount;

        public T this[int index] => m_getAtImpl(m_infos, index);

        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private class Enumerator : IEnumerator<T>
        {
            public Enumerator(IReadOnlyList<T> selectedItems)
            {
                m_selectedItems = selectedItems;
            }

            public void Dispose()
            {
            }

            public T Current
            {
                get
                {
                    var items = m_selectedItems;
                    if (m_currentIndex < items.Count)
                    {
                        return items[m_currentIndex];
                    }
                    else
                    {
                        throw new IndexOutOfRangeException();
                    }
                }
            }

            object IEnumerator.Current => Current;

            public bool MoveNext()
            {
                if (m_currentIndex < m_selectedItems.Count)
                {
                    ++m_currentIndex;
                    return m_currentIndex < m_selectedItems.Count;
                }
                else
                {
                    throw new IndexOutOfRangeException();
                }
            }

            public void Reset()
            {
                m_currentIndex = 1;
            }

            readonly IReadOnlyList<T> m_selectedItems;
            int m_currentIndex = -1;
        }

        List<SelectedItemInfo> m_infos;
        int m_totalCount;
        Func<List<SelectedItemInfo>, int, T> m_getAtImpl;
    }
}
