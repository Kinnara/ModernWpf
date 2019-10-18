// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace ModernWpf.Controls
{
    [Flags]
    public enum AnimationContext
    {
        None = 0,
        CollectionChangeAdd = 1,
        CollectionChangeRemove = 2,
        CollectionChangeReset = 4,
        LayoutTransition = 8
    }

    [Flags]
    public enum ElementRealizationOptions
    {
        None = 0,
        ForceCreate = 1,
        SuppressAutoRecycle = 2
    }
}
