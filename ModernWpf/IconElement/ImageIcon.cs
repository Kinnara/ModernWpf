using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ModernWpf.Controls
{
    /// <summary>
    /// Represents an icon that uses an image as its content.
    /// </summary>
    public class ImageIcon : IconElement
    {
        /// <summary>
        /// Initializes a new instance of the ImageIcon class.
        /// </summary>
        public ImageIcon()
        {
        }

        /// <summary>
        /// Identifies the Source dependency property.
        /// </summary>
        public static readonly DependencyProperty SourceProperty =
            Image.SourceProperty.AddOwner(
                typeof(ImageIcon),
                new FrameworkPropertyMetadata(OnSourceChanged));

        /// <summary>
        /// Gets or sets the image source displayed by the icon.
        /// </summary>
        /// <returns>The image source displayed by the icon. The default is <see langword="null"/>.</returns>
        public ImageSource Source
        {
            get => (ImageSource)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ImageIcon)d).ApplySource();
        }

        private protected override void InitializeChildren()
        {
            _image = new Image
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Stretch = Stretch.Uniform
            };

            ApplySource();

            Children.Add(_image);
        }

        private void ApplySource()
        {
            if (_image != null)
            {
                _image.Source = Source;
            }
        }

        private Image _image;
    }
}
