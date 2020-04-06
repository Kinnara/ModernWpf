using System.Windows;
using System.Windows.Media;

namespace SamplesCommon
{
    public class FontOverrides : ResourceDictionary
    {
        public FontFamily FontFamily
        {
            get => _fontFamily;
            set
            {
                if (_fontFamily != value)
                {
                    _fontFamily = value;

                    if (_fontFamily != null)
                    {
                        foreach (var key in s_keys)
                        {
                            this[key] = _fontFamily;
                        }
                    }
                    else
                    {
                        foreach (var key in s_keys)
                        {
                            Remove(key);
                        }
                    }
                }
            }
        }

        private static readonly object[] s_keys =
        {
            SystemFonts.MessageFontFamilyKey,
            "ContentControlThemeFontFamily",
            "PivotHeaderItemFontFamily",
            "PivotTitleFontFamily"
        };

        private FontFamily _fontFamily;
    }
}
