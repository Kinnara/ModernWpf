using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace ModernWpf.Helpers
{
    internal static class FontHelper
    {
        public static string SymbolThemeFontFamilyKey => GetSymbolThemeFontFamily();

        private static string GetSymbolThemeFontFamily()
        {
            if (CheckSysFontExisting("Segoe Fluent Icons"))
            {
                return "SegoeFluentIcons";
            }
            else if (CheckSysFontExisting("Segoe MDL2 Assets"))
            {
                return "SegoeMDL2Assets";
            }
            else if (CheckSysFontExisting("Segoe UI Symbol"))
            {
                return "SegoeUISymbol";
            }
            else
            {
                return "FluentSystemIcons";
            }
        }

        public static bool CheckSysFontExisting(string fontName = "文鼎細黑")
        {
            FontFamily font;
            try
            {
                font = new FontFamily(fontName);
                if (!font.FamilyNames.Any())
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }
}
