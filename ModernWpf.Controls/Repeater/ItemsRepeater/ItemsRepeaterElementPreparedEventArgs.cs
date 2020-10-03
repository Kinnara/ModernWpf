// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Windows;

namespace ModernWpf.Controls
{
    public sealed class ItemsRepeaterElementPreparedEventArgs : EventArgs
    {
        internal ItemsRepeaterElementPreparedEventArgs(
            UIElement element,
            int index)
        {
            Update(element, index);
        }

        public UIElement Element { get; private set; }
        public int Index { get; private set; }

        internal void Update(UIElement element, int index)
        {
            Element = element;
            Index = index;
        }
    }
}