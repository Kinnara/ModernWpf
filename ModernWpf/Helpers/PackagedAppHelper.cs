using System.Runtime.InteropServices;
using System.Text;

namespace ModernWpf
{
    internal static class PackagedAppHelper
    {
        private const long APPMODEL_ERROR_NO_PACKAGE = 15700L;

        public static bool IsPackagedApp
        {
            get
            {
                if (OSVersionHelper.IsWindows8OrGreater)
                {
                    int length = 0;
                    var sb = new StringBuilder(0);
                    GetCurrentPackageFullName(ref length, sb);

                    sb.Length = length;
                    int result = GetCurrentPackageFullName(ref length, sb);

                    return result != APPMODEL_ERROR_NO_PACKAGE;
                }

                return false;
            }
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int GetCurrentPackageFullName(ref int packageFullNameLength, StringBuilder packageFullName);
    }
}
