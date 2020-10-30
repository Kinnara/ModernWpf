using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ModernWpf.Controls
{
	public class BitmapIconSource : IconSource
	{
		public BitmapIconSource()
		{
		}

		public static readonly DependencyProperty UriSourceProperty =
			BitmapImage.UriSourceProperty.AddOwner(typeof(BitmapIconSource));

		public Uri UriSource
		{
			get => (Uri)GetValue(UriSourceProperty);
			set => SetValue(UriSourceProperty, value);
		}

		public static readonly DependencyProperty ShowAsMonochromeProperty =
			DependencyProperty.Register(
				nameof(ShowAsMonochrome),
				typeof(bool),
				typeof(BitmapIconSource),
				new PropertyMetadata(true));

		public bool ShowAsMonochrome
		{
			get => (bool)GetValue(ShowAsMonochromeProperty);
			set => SetValue(ShowAsMonochromeProperty, value);
		}
	}
}
