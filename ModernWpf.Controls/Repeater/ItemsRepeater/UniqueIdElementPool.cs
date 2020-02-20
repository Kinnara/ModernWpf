// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace ModernWpf.Controls
{
    internal class UniqueIdElementPool : IEnumerable<KeyValuePair<string, UIElement>>
    {
        public UniqueIdElementPool(ItemsRepeater owner)
        {
            m_owner = owner;
            // ItemsRepeater is not fully constructed yet. Don't interact with it.
        }

        public void Add(UIElement element)
        {
            Debug.Assert(m_owner.ItemsSourceView.HasKeyIndexMapping);

            var virtInfo = ItemsRepeater.GetVirtualizationInfo(element);
            var key = virtInfo.UniqueId;

            if (m_elementMap.ContainsKey(key))
            {
                string message = "The unique id provided (" + virtInfo.UniqueId + ") is not unique.";
                throw new Exception(message);
            }

            m_elementMap.Add(key, element);
        }

        public UIElement Remove(int index)
        {
            Debug.Assert(m_owner.ItemsSourceView.HasKeyIndexMapping);

            // Check if there is already a element in the mapping and if so, use it.
            string key = m_owner.ItemsSourceView.KeyFromIndex(index);
            if (m_elementMap.TryGetValue(key, out UIElement element))
            {
                m_elementMap.Remove(key);
            }

            return element;
        }

        public void Clear()
        {
            Debug.Assert(m_owner.ItemsSourceView.HasKeyIndexMapping);
            m_elementMap.Clear();
        }

        public IEnumerator<KeyValuePair<string, UIElement>> GetEnumerator()
        {
            return m_elementMap.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

#if DEBUG
        public bool IsEmpty => m_elementMap.Count == 0;
#endif

        private readonly ItemsRepeater m_owner;
        private readonly Dictionary<string, UIElement> m_elementMap = new Dictionary<string, UIElement>();
    }
}