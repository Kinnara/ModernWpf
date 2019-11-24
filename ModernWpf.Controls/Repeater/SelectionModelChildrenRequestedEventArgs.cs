// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace ModernWpf.Controls
{
    public sealed class SelectionModelChildrenRequestedEventArgs
    {
        internal SelectionModelChildrenRequestedEventArgs(object source, WeakReference<SelectionNode> sourceNode)
        {
            Initialize(source, sourceNode);
        }

        public object Source
        {
            get
            {
                if (!m_sourceNode.TryGetTarget(out var node))
                {
                    throw new Exception("Source can only be accesed in the ChildrenRequested event handler.");
                }

                return m_source;
            }
        }

        public IndexPath SourceIndex
        {
            get
            {
                if (!m_sourceNode.TryGetTarget(out var node))
                {
                    throw new Exception("SourceIndex can only be accesed in the ChildrenRequested event handler.");
                }

                return node.IndexPath;
            }
        }

        public object Children { get; set; }

        internal void Initialize(object source, WeakReference<SelectionNode> sourceNode)
        {
            m_source = source;
            m_sourceNode = sourceNode;
        }

        private object m_source;
        private WeakReference<SelectionNode> m_sourceNode;
    }
}
