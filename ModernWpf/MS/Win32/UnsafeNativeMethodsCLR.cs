// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;

namespace MS.Win32
{
    internal partial class UnsafeNativeMethods
    {
        [DllImport(ExternDll.User32, EntryPoint = "SetWindowPos", ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = true)]
        public static extern bool SetWindowPos(HandleRef hWnd, HandleRef hWndInsertAfter, int x, int y, int cx, int cy, int flags);

        [DllImport(ExternDll.User32, EntryPoint="ClientToScreen", SetLastError=true, ExactSpelling=true, CharSet=CharSet.Auto)]
        private static extern int IntClientToScreen(HandleRef hWnd, [In, Out] NativeMethods.POINT pt);
 
        [DllImport(ExternDll.User32, ExactSpelling=true, CharSet=CharSet.Auto)]
        public static extern IntPtr GetActiveWindow();
    }
}
