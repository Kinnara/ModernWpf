// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;

namespace ModernWpf.Controls
{
    internal delegate void ConfigurationChangedEventHandler(IRepeaterScrollingSurface sender);

    internal delegate void PostArrangeEventHandler(IRepeaterScrollingSurface sender);

    internal delegate void ViewportChangedEventHandler(IRepeaterScrollingSurface sender, bool isFinal);

    internal interface IRepeaterScrollingSurface
    {
        bool IsHorizontallyScrollable { get; }
        bool IsVerticallyScrollable { get; }
        UIElement AnchorElement { get; }
        event ConfigurationChangedEventHandler ConfigurationChanged;
        event PostArrangeEventHandler PostArrange;
        event ViewportChangedEventHandler ViewportChanged;
        void RegisterAnchorCandidate(UIElement element);
        void UnregisterAnchorCandidate(UIElement element);
        Rect GetRelativeViewport(UIElement child);
    }
}