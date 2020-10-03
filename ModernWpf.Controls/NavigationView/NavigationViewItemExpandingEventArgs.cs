// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace ModernWpf.Controls
{
    public sealed class NavigationViewItemExpandingEventArgs : EventArgs
    {
        internal NavigationViewItemExpandingEventArgs(NavigationView navigationView)
        {
            m_navigationView = navigationView;
        }

        public NavigationViewItemBase ExpandingItemContainer { get; internal set; }

        public object ExpandingItem
        {
            get
            {
                if (m_expandingItem != null)
                {
                    return m_expandingItem;
                }

                if (m_navigationView is { } nv)
                {
                    m_expandingItem = nv.MenuItemFromContainer(ExpandingItemContainer);
                    return m_expandingItem;
                }

                return null;
            }
        }

        object m_expandingItem;
        NavigationView m_navigationView;
    }
}
