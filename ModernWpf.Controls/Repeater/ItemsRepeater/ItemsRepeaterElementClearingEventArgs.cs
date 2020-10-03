// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Windows;

namespace ModernWpf.Controls
{
    public sealed class ItemsRepeaterElementClearingEventArgs : EventArgs
    {
        internal ItemsRepeaterElementClearingEventArgs(
            UIElement element)
        {
            Update(element);
        }

        public UIElement Element { get; private set; }

        internal void Update(UIElement element)
        {
            Element = element;
        }
    }
}