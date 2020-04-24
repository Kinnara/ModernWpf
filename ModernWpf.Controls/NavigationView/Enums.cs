// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace ModernWpf.Controls
{
    public enum NavigationViewDisplayMode
    {
        Minimal = 0,
        Compact = 1,
        Expanded = 2,
    }

    public enum NavigationViewBackButtonVisible
    {
        Collapsed = 0,
        Visible = 1,
        Auto = 2,
    }

    public enum NavigationViewPaneDisplayMode
    {
        Auto = 0,
        Left = 1,
        Top = 2,
        LeftCompact = 3,
        LeftMinimal = 4
    }

    public enum NavigationViewSelectionFollowsFocus
    {
        Disabled = 0,
        Enabled = 1
    }

    public enum NavigationViewShoulderNavigationEnabled
    {
        WhenSelectionFollowsFocus = 0,
        Always = 1,
        Never = 2
    }

    public enum NavigationViewOverflowLabelMode
    {
        MoreLabel = 0,
        NoLabel = 1
    }
}
