using System.Windows.Media;

namespace ModernWpf.Controls
{
    internal static class SharedHelpers
    {
        public static IconElement MakeIconElementFrom(IconSource iconSource)
        {
            if (iconSource is FontIconSource fontIconSource)
            {
                FontIcon fontIcon = new FontIcon();

                fontIcon.Glyph = fontIconSource.Glyph;
                fontIcon.FontSize = fontIconSource.FontSize;
                if (fontIconSource.Foreground is Brush newForeground)
                {
                    fontIcon.Foreground = newForeground;
                }

                if (fontIconSource.FontFamily != null)
                {
                    fontIcon.FontFamily = fontIconSource.FontFamily;
                }

                fontIcon.FontWeight = fontIconSource.FontWeight;
                fontIcon.FontStyle = fontIconSource.FontStyle;
                //fontIcon.IsTextScaleFactorEnabled(fontIconSource.IsTextScaleFactorEnabled());
                //fontIcon.MirroredWhenRightToLeft(fontIconSource.MirroredWhenRightToLeft());

                return fontIcon;
            }
            else if (iconSource is SymbolIconSource symbolIconSource)
            {
                SymbolIcon symbolIcon = new SymbolIcon();
                symbolIcon.Symbol = symbolIconSource.Symbol;
                if (symbolIconSource.Foreground is Brush newForeground)
                {
                    symbolIcon.Foreground = newForeground;
                }

                return symbolIcon;
            }
            else if (iconSource is BitmapIconSource bitmapIconSource)
            {
                BitmapIcon bitmapIcon = new BitmapIcon();

                if (bitmapIconSource.UriSource != null)
                {
                    bitmapIcon.UriSource = bitmapIconSource.UriSource;
                }

                bitmapIcon.ShowAsMonochrome = bitmapIconSource.ShowAsMonochrome;
                if (bitmapIconSource.Foreground is Brush newForeground)
                {
                    bitmapIcon.Foreground = newForeground;
                }

                return bitmapIcon;
            }
            else if (iconSource is PathIconSource pathIconSource)
            {
                PathIcon pathIcon = new PathIcon();

                if (pathIconSource.Data != null)
                {
                    pathIcon.Data = pathIconSource.Data;
                }
                if (pathIconSource.Foreground is Brush newForeground)
                {
                    pathIcon.Foreground = newForeground;
                }

                return pathIcon;
            }

            return null;
        }
    }
}
