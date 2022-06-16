using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ModernWpf.Controls
{
    /// <summary>
    /// Represents an icon source that uses a bitmap as its content.
    /// </summary>
    public class ImageIconSource : IconSource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImageIconSource"/> class.
        /// </summary>
        public ImageIconSource()
        {
        }

        /// <summary>
        /// Identifies the <see cref="Source"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SourceProperty =
            Image.SourceProperty.AddOwner(typeof(ImageIconSource));

        /// <summary>
        /// Gets or sets the URI of the image file to use as the icon.
        /// </summary>
        /// <returns>The <see cref="Uri"/> of the image file to use as the icon. The default is <see langword="null"/>.</returns>
        public ImageSource Source
        {
            get => (ImageSource)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }
    }
}
