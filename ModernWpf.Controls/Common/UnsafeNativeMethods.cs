using System;
using System.Runtime.InteropServices;

namespace ModernWpf
{
    internal class UnsafeNativeMethods
    {
        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern IntPtr GetActiveWindow();
    }
}
