// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.Controls
{
    internal class LayoutPanelLayoutContext : NonVirtualizingLayoutContext
    {
        public LayoutPanelLayoutContext(LayoutPanel owner)
        {
            m_owner = new WeakReference<LayoutPanel>(owner);
        }

        protected override IReadOnlyList<UIElement> ChildrenCore => new UIElementCollectionView(GetOwner().Children);

        protected override object LayoutStateCore
        {
            get => GetOwner().LayoutState;
            set => GetOwner().LayoutState = value;
        }

        private LayoutPanel GetOwner()
        {
            m_owner.TryGetTarget(out var target);
            return target;
        }

        private readonly WeakReference<LayoutPanel> m_owner;

        private class UIElementCollectionView : IReadOnlyList<UIElement>
        {
            public UIElementCollectionView(UIElementCollection collection)
            {
                m_collection = collection;
            }

            public UIElement this[int index] => m_collection[index];

            public int Count => m_collection.Count;

            public IEnumerator<UIElement> GetEnumerator()
            {
                foreach (UIElement element in m_collection)
                {
                    yield return element;
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return m_collection.GetEnumerator();
            }

            private readonly UIElementCollection m_collection;

            //private class Enumerator : IEnumerator<UIElement>
            //{
            //    public Enumerator(UIElementCollection collection)
            //    {
            //        m_collection = collection;
            //    }

            //    public UIElement Current
            //    {
            //        get
            //        {
            //            if (m_currentIndex < m_collection.Count)
            //            {
            //                return m_collection[m_currentIndex];
            //            }
            //            else
            //            {
            //                throw new InvalidOperationException();
            //            }
            //        }
            //    }

            //    object IEnumerator.Current => Current;

            //    public bool MoveNext()
            //    {
            //        if (m_currentIndex < m_collection.Count)
            //        {
            //            ++m_currentIndex;
            //            return (m_currentIndex < m_collection.Count);
            //        }
            //        else
            //        {
            //            return false;
            //        }
            //    }

            //    public void Reset()
            //    {
            //        m_currentIndex = -1;
            //    }

            //    public void Dispose()
            //    {
            //    }

            //    private readonly UIElementCollection m_collection;
            //    private int m_currentIndex = -1;
            //}
        }
    }
}