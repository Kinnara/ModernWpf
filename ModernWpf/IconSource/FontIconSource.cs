using System.Windows;
using System.Windows.Media;

namespace ModernWpf.Controls
{
	public class FontIconSource : IconSource
	{
		public FontIconSource()
		{
		}

		public static readonly DependencyProperty FontFamilyProperty =
			DependencyProperty.Register(
				nameof(FontFamily),
				typeof(FontFamily),
				typeof(FontIconSource),
				new PropertyMetadata(new FontFamily("Segoe MDL2 Assets")));

		public FontFamily FontFamily
		{
			get => (FontFamily)GetValue(FontFamilyProperty);
			set => SetValue(FontFamilyProperty, value);
		}

		public static readonly DependencyProperty FontSizeProperty =
			DependencyProperty.Register(
				nameof(FontSize),
				typeof(double),
				typeof(FontIconSource),
				new PropertyMetadata(20.0));

		public double FontSize
		{
			get => (double)GetValue(FontSizeProperty);
			set => SetValue(FontSizeProperty, value);
		}

		public static readonly DependencyProperty FontStyleProperty =
			DependencyProperty.Register(
				nameof(FontStyle),
				typeof(FontStyle),
				typeof(FontIconSource),
				new PropertyMetadata(FontStyles.Normal));

		public FontStyle FontStyle
		{
			get => (FontStyle)GetValue(FontStyleProperty);
			set => SetValue(FontStyleProperty, value);
		}

		public static readonly DependencyProperty FontWeightProperty =
			DependencyProperty.Register(
				nameof(FontWeight),
				typeof(FontWeight),
				typeof(FontIconSource),
				new PropertyMetadata(FontWeights.Normal));

		public FontWeight FontWeight
		{
			get => (FontWeight)GetValue(FontWeightProperty);
			set => SetValue(FontWeightProperty, value);
		}

		public static readonly DependencyProperty GlyphProperty =
			DependencyProperty.Register(
				nameof(Glyph),
				typeof(string),
				typeof(FontIconSource),
				new PropertyMetadata(default(string)));

		public string Glyph
		{
			get => (string)GetValue(GlyphProperty);
			set => SetValue(GlyphProperty, value);
		}
	}
}
