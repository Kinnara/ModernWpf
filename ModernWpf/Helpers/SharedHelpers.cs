using ModernWpf.Controls;

namespace ModernWpf.Helpers
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

				if (fontIconSource.FontFamily != null)
				{
					fontIcon.FontFamily = fontIconSource.FontFamily;
				}

				fontIcon.FontWeight = fontIconSource.FontWeight;
				fontIcon.FontStyle = fontIconSource.FontStyle;

				return fontIcon;
			}
			else if (iconSource is SymbolIconSource symbolIconSource)
			{
				SymbolIcon symbolIcon = new SymbolIcon();
				symbolIcon.Symbol = symbolIconSource.Symbol;

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

				return bitmapIcon;
			}
			else if (iconSource is PathIconSource pathIconSource)
			{
				PathIcon pathIcon = new PathIcon();

				if (pathIconSource.Data != null)
				{
					pathIcon.Data = pathIconSource.Data;
				}

				return pathIcon;
			}

			return null;
		}
	}
}
