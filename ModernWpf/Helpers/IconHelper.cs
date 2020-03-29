using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace ModernWpf
{
    internal static class IconHelper
    {
        internal const int MAX_PATH = 260;

        public static void GetDefaultIconHandles(IntPtr[] largeIconHandle, IntPtr[] smallIconHandle)
        {
            // Get the handle of the module that created the running process.
            string iconModuleFile = GetModuleFileName(new HandleRef());

            // We don't really care about the return value.  Handles will be invalid on error.
            _ = NativeMethods.ExtractIconEx(iconModuleFile, 0, largeIconHandle, smallIconHandle, 1);
        }

        public static bool DestroyIcon(IntPtr icon)
        {
            bool result = NativeMethods.DestroyIcon(icon);
            int error = Marshal.GetLastWin32Error();

            if (!result)
            {
                // To be consistent with out other PInvoke wrappers
                // we should "throw" here.  But we don't want to
                // introduce new "throws" w/o time to follow up on any
                // new problems that causes.
                Debug.WriteLine("DestroyIcon failed.  Error = " + error);
                //throw new Win32Exception();
            }

            return result;
        }

        private static string GetModuleFileName(HandleRef hModule)
        {
            // .Net is currently far behind Windows with regard to supporting paths longer than MAX_PATH.
            // At one point it was tested trying to load UNC paths longer than MAX_PATH and mscorlib threw
            // FileIOExceptions before WPF was even on the stack.
            // All the same, we still want to have this grow-and-retry logic because the CLR can be hosted
            // in a native application.  Callers bothering to use this rather than Assembly based reflection
            // are likely doing so because of (at least the potential for) the returned name referring to a
            // native module.
            StringBuilder buffer = new StringBuilder(MAX_PATH);
            while (true)
            {
                int size = NativeMethods.GetModuleFileName(hModule, buffer, buffer.Capacity);
                if (size == 0)
                {
                    throw new Win32Exception();
                }

                // GetModuleFileName returns nSize when it's truncated but does NOT set the last error.
                // MSDN documentation says this has changed in Windows 2000+.
                if (size == buffer.Capacity)
                {
                    // Enlarge the buffer and try again.
                    buffer.EnsureCapacity(buffer.Capacity * 2);
                    continue;
                }

                return buffer.ToString();
            }
        }

        private static class NativeMethods
        {
            [DllImport("Shell32.dll", CharSet = CharSet.Auto)]
            public extern static int ExtractIconEx(string lpszFile, int nIconIndex, IntPtr[] phiconLarge, IntPtr[] phiconSmall, uint nIcons);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public extern static bool DestroyIcon(IntPtr hIcon);

            [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern int GetModuleFileName(HandleRef hModule, StringBuilder buffer, int length);
        }
    }
}
