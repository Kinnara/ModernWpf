using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ModernWpf.Controls
{
    /// <summary>
    /// Represents an icon that uses an Image as its content.
    /// </summary>
    public class ImageIcon : IconElement
    {
        static ImageIcon()
        {
        }

        /// <summary>
        /// Initializes a new instance of the ImageIcon class.
        /// </summary>
        public ImageIcon()
        {
        }

        #region Source

        /// <summary>
        /// Identifies the Source dependency property.
        /// </summary>
        public static readonly DependencyProperty SourceProperty =
            Image.SourceProperty.AddOwner(
                typeof(ImageIcon),
                new FrameworkPropertyMetadata(OnSourceChanged));

        /// <summary>
        /// Gets or sets the URI of the image file to use as the icon.
        /// </summary>
        /// <returns>The URI of the image file to use as the icon. The default is **null**.</returns>
        public ImageSource Source
        {
            get => (ImageSource)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ImageIcon)d).ApplySource();
        }

        #endregion

        private protected override void InitializeChildren()
        {
            _image = new Image();

            ApplySource();

            Children.Add(_image);
        }

        private void ApplySource()
        {
            if (_image != null)
            {
                var source = Source;
                if (source != null)
                {
                    _image.Source = source;
                }
                else
                {
                    _image.ClearValue(Image.SourceProperty);
                }
            }
        }

        private Image _image;
    }
}
