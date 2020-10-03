// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace ModernWpf.Controls
{
    public sealed class NavigationViewPaneClosingEventArgs : EventArgs
    {
        internal NavigationViewPaneClosingEventArgs()
        {
        }

        public bool Cancel
        {
            get => m_cancelled;
            set
            {
                m_cancelled = value;

                if (m_splitViewClosingArgs is { } args)
                {
                    args.Cancel = value;
                }
            }
        }

        internal void SplitViewClosingArgs(SplitViewPaneClosingEventArgs value)
        {
            m_splitViewClosingArgs = value;
        }

        SplitViewPaneClosingEventArgs m_splitViewClosingArgs;
        bool m_cancelled;
    }
}
