// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Windows;

namespace ModernWpf.Controls
{
    public sealed class ItemsRepeaterElementIndexChangedEventArgs : EventArgs
    {
        internal ItemsRepeaterElementIndexChangedEventArgs(
            UIElement element,
            int oldIndex,
            int newIndex)
        {
            Update(element, oldIndex, newIndex);
        }

        public UIElement Element { get; private set; }
        public int OldIndex { get; private set; }
        public int NewIndex { get; private set; }

        internal void Update(UIElement element, int oldIndex, int newIndex)
        {
            Element = element;
            OldIndex = oldIndex;
            NewIndex = newIndex;
        }
    }
}