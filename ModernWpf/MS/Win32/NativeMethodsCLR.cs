// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.InteropServices;

namespace MS.Win32
{
    internal partial class NativeMethods
    {
        public const int SWP_NOSIZE = 0x0001,
        SWP_NOMOVE = 0x0002,
        SWP_NOZORDER = 0x0004,
        SWP_NOACTIVATE = 0x0010,
        SWP_SHOWWINDOW = 0x0040,
        SWP_HIDEWINDOW = 0x0080,
        SWP_DRAWFRAME = 0x0020;

        [StructLayout(LayoutKind.Sequential)]
        public class POINT {
            public int x;
            public int y;
 
            public POINT() {
            }
 
            public POINT(int x, int y) {
                this.x = x;
                this.y = y;
            }
#if DEBUG
            public override string ToString() {
                return "{x=" + x + ", y=" + y + "}";
            }
#endif
        }

        // NOTE:  this replaces the RECT struct in NativeMethodsCLR.cs because it adds an extra method IsEmpty
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;

            public RECT(int left, int top, int right, int bottom)
            {
                this.left = left;
                this.top = top;
                this.right = right;
                this.bottom = bottom;
            }

            public int Width
            {
                get { return right - left; }
            }

            public int Height
            {
                get { return bottom - top; }
            }

            public void Offset(int dx, int dy)
            {
                left += dx;
                top += dy;
                right += dx;
                bottom += dy;
            }

            public bool IsEmpty
            {
                get
                {
                    return left >= right || top >= bottom;
                }
            }
        }
    }
}
