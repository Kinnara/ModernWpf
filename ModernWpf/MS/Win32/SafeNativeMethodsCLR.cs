// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.ComponentModel;
using System.Runtime.InteropServices;

namespace MS.Win32
{
    internal static partial class SafeNativeMethods
    {
        internal static void GetWindowRect(HandleRef hWnd, [In, Out] ref NativeMethods.RECT rect)
        {
            if(!SafeNativeMethodsPrivate.IntGetWindowRect(hWnd, ref rect))
            {
                throw new Win32Exception();
            }
        }

        private partial class SafeNativeMethodsPrivate
        {
            [DllImport(ExternDll.User32, EntryPoint = "GetWindowRect", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
            public static extern bool IntGetWindowRect(HandleRef hWnd, [In, Out] ref NativeMethods.RECT rect);
        }
    }
}
