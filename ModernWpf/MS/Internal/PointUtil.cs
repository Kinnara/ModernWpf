// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using MS.Win32;
using System.Windows;

namespace MS.Internal
{
    internal static class PointUtil
    {
        internal static Rect ToRect(NativeMethods.RECT rc)
        {
            Rect rect = new Rect();
 
            rect.X      = rc.left;
            rect.Y      = rc.top;
            rect.Width  = rc.right  - rc.left;
            rect.Height = rc.bottom - rc.top;
 
            return rect;
        }
    }
}
