using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ModernWpf
{
    internal static class OSVersionHelper
    {
        private static readonly Version _osVersion = GetOSVersion();

        internal static bool IsWindowsNT { get; } = Environment.OSVersion.Platform == PlatformID.Win32NT;

        internal static bool IsWindows8OrGreater { get; } = IsWindowsNT && _osVersion >= new Version(6, 2);

        internal static bool IsWindows10OrGreater { get; } = IsWindowsNT && _osVersion >= new Version(10, 0);

        private static Version GetOSVersion()
        {
            var osv = new RTL_OSVERSIONINFOEX();
            osv.dwOSVersionInfoSize = (uint)Marshal.SizeOf(osv);
            int ret = RtlGetVersion(out osv);
            Debug.Assert(ret == 0);
            return new Version((int)osv.dwMajorVersion, (int)osv.dwMinorVersion, (int)osv.dwBuildNumber);
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
