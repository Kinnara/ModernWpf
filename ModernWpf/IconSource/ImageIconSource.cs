using System.Windows;
using System.Windows.Media;

namespace ModernWpf.Controls
{
    /// <summary>
    /// Represents an icon source that uses an image as its content.
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
        /// Identifies the <see cref="ImageSource"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register(
                nameof(ImageSource),
                typeof(ImageSource),
                typeof(ImageIconSource));

        /// <summary>
        /// Gets or sets the image source displayed by the created icon.
        /// </summary>
        /// <returns>The image source displayed by the icon. The default is <see langword="null"/>.</returns>
        public ImageSource ImageSource
        {
            get => (ImageSource)GetValue(ImageSourceProperty);
            set => SetValue(ImageSourceProperty, value);
        }

        protected override IconElement CreateIconElementCore()
        {
            ImageIcon imageIcon = new();

            if (ImageSource is { } imageSource)
            {
                imageIcon.Source = imageSource;
            }

            if (Foreground is { } newForeground)
            {
                imageIcon.Foreground = newForeground;
            }

            return imageIcon;
        }

        protected override DependencyProperty GetIconElementPropertyCore(DependencyProperty sourceProperty)
        {
            if (sourceProperty == ImageSourceProperty)
            {
                return ImageIcon.SourceProperty;
            }

            return base.GetIconElementPropertyCore(sourceProperty);
        }
    }
}
