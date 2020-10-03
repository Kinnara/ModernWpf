// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace ModernWpf.Controls
{
    public sealed class NavigationViewItemCollapsedEventArgs : EventArgs
    {
        internal NavigationViewItemCollapsedEventArgs(NavigationView navigationView)
        {
            m_navigationView = navigationView;
        }

        public NavigationViewItemBase CollapsedItemContainer { get; internal set; }

        public object CollapsedItem
        {
            get
            {
                if (m_collapsedItem != null)
                {
                    return m_collapsedItem;
                }

                if (m_navigationView is { } nv)
                {
                    m_collapsedItem = nv.MenuItemFromContainer(CollapsedItemContainer);
                    return m_collapsedItem;
                }

                return null;
            }
        }

        object m_collapsedItem;
        NavigationView m_navigationView;
    }
}
