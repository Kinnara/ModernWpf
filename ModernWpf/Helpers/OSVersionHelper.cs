using System;
using System.Runtime.InteropServices;

namespace ModernWpf
{
    internal static class OSVersionHelper
    {
        internal static bool IsWindowsNT { get; } = Environment.OSVersion.Platform == PlatformID.Win32NT;

        internal static bool IsWindows10 { get; } = IsWindowsNT && IsWindows10Impl();

        private static bool IsWindows10Impl()
        {
            var osv = new RTL_OSVERSIONINFOEX();
            osv.dwOSVersionInfoSize = (uint)Marshal.SizeOf(osv);
            int ret = RtlGetVersion(out osv);
            return ret == 0 && osv.dwMajorVersion >= 10;
        }

        [DllImport("ntdll.dll")]
        private static extern int RtlGetVersion(out RTL_OSVERSIONINFOEX lpVersionInformation);

        [StructLayout(LayoutKind.Sequential)]
        private struct RTL_OSVERSIONINFOEX
        {
            internal uint dwOSVersionInfoSize;
            internal uint dwMajorVersion;
            internal uint dwMinorVersion;
            internal uint dwBuildNumber;
            internal uint dwPlatformId;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            internal string szCSDVersion;
        }
    }
}
