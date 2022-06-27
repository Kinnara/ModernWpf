// Ported from https://github.com/dotMorten/UniversalWPF/blob/main/src/UniversalWPF/TwoPaneView/DisplayRegionHelper.cs

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;

namespace ModernWpf.Controls
{
    internal class DisplayRegionHelper
    {
        public static Rect WindowRect(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero || !GetWindowRect(hwnd, out RECT rect))
                return Rect.Empty;
            return rect.AsRect();
        }

        internal static DisplayRegionHelperInfo GetRegionInfo(IntPtr hwnd)
        {
            DisplayRegionHelperInfo info = new DisplayRegionHelperInfo(TwoPaneViewMode.SinglePane);

            if (hwnd != IntPtr.Zero && s_isGetContentRectsSupported)
            {
                List<Rect> rects = GetRegions(hwnd);
                if (rects != null && rects.Count == 2)
                {
                    info.Regions[0] = rects[0];
                    info.Regions[1] = rects[1];

                    // Determine orientation. If neither of these are true, default to doing nothing.
                    if (info.Regions[0].X < info.Regions[1].X && info.Regions[0].Y == info.Regions[1].Y)
                    {
                        // Double portrait
                        info.Mode = TwoPaneViewMode.Wide;
                    }
                    else if (info.Regions[0].X == info.Regions[1].X && info.Regions[0].Y < info.Regions[1].Y)
                    {
                        // Double landscape
                        info.Mode = TwoPaneViewMode.Tall;
                    }
                }
            }
            return info;
        }

        static bool s_isGetContentRectsSupported = true;

        private static List<Rect> GetRegions(IntPtr hwnd)
        {
            try
            {
                uint count = 2;
                RECT[] regions = new RECT[2];
                bool result = GetContentRects(hwnd, ref count, regions);
                if (result)
                {
                    List<Rect> rects = new List<Rect>((int)count);
                    for (int i = 0; i < (int)count; i++)
                    {
                        rects.Add(regions[i].AsRect());
                    }
                    return rects;
                }
            }
            catch (EntryPointNotFoundException) // Expected to throw on older OS
            {
                s_isGetContentRectsSupported = false;
            }
            return null;
        }


        [DllImport("user32.dll")]
        [System.Security.SuppressUnmanagedCodeSecurity()]
        private static extern bool GetContentRects(IntPtr hwnd, ref uint count, [MarshalAs(UnmanagedType.LPArray)] RECT[] rects);

        /// <summary>
        /// Retrieves the dimensions of the bounding rectangle of the specified window.
        /// The dimensions are given in screen coordinates that are relative to the upper-left corner of the screen.
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="lpRect"></param>
        /// <returns></returns>
        [DllImport("user32.dll", SetLastError = true)]
        [System.Security.SuppressUnmanagedCodeSecurity()]
        private static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
            public Rect AsRect() => new Rect(Left, Top, Right - Left, Top - Bottom);
        }
    }
}
