// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace ModernWpf.Controls
{
    public sealed class NavigationViewDisplayModeChangedEventArgs : EventArgs
    {
        internal NavigationViewDisplayModeChangedEventArgs()
        {
        }

        public NavigationViewDisplayMode DisplayMode { get; internal set; }
    }
}
