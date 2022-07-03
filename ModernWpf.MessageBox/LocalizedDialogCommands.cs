using System;
using System.Runtime.InteropServices;

namespace ModernWpf
{
    internal static class LocalizedDialogCommands
    {
        public static string GetString(DialogBoxCommand command)
        {
            return Marshal.PtrToStringAuto(MB_GetString((int)command))?.TrimStart('&')!;
        }

        /// <summary>
        /// Returns strings for standard message box buttons.
        /// </summary>
        /// <param name="strId">The id of the string to return. These are identified by the ID* values assigned to the predefined buttons.</param>
        /// <returns>The string, or NULL if not found</returns>
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr MB_GetString(int strId);

        /// <summary>
        /// Represents possible dialogbox command id values by the MB_GetString function.
        /// </summary>
        public enum DialogBoxCommand : int
        {
            IDOK = 0,
            IDCANCEL = 1,
            IDABORT = 2,
            IDRETRY = 3,
            IDIGNORE = 4,
            IDYES = 5,
            IDNO = 6,
            IDCLOSE = 7,
            IDHELP = 8,
            IDTRYAGAIN = 9,
            IDCONTINUE = 10
        }
    }
}
