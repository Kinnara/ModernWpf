using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ModernWpf.Controls
{
    /// <summary>
    /// Represents an icon that uses a bitmap as its content.
    /// </summary>
    public class BitmapIcon : IconElement
    {
        static BitmapIcon()
        {
            ForegroundProperty.OverrideMetadata(typeof(BitmapIcon), new FrameworkPropertyMetadata(OnForegroundChanged));
        }

        /// <summary>
        /// Initializes a new instance of the BitmapIcon class.
        /// </summary>
        public BitmapIcon()
        {
        }

        #region UriSource

        /// <summary>
        /// Identifies the UriSource dependency property.
        /// </summary>
        public static readonly DependencyProperty UriSourceProperty =
            BitmapImage.UriSourceProperty.AddOwner(
                typeof(BitmapIcon),
                new FrameworkPropertyMetadata(OnUriSourceChanged));

        /// <summary>
        /// Gets or sets the Uniform Resource Identifier (URI) of the bitmap to use as the
        /// icon content.
        /// </summary>
        /// <returns>The Uri of the bitmap to use as the icon content. The default is **null**.</returns>
        public Uri UriSource
        {
            get => (Uri)GetValue(UriSourceProperty);
            set => SetValue(UriSourceProperty, value);
        }

        private static void OnUriSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((BitmapIcon)d).ApplyUriSource();
        }

        #endregion

        internal override void InitializeChildren()
        {
            _placeholder = new Image
            {
                Visibility = Visibility.Hidden
            };

            _opacityMask = new ImageBrush();
            _fill = new Rectangle
            {
                OpacityMask = _opacityMask
            };

            ApplyForeground();
            ApplyUriSource();

            Children.Add(_placeholder);
            Children.Add(_fill);
        }

        private static void OnForegroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((BitmapIcon)d).ApplyForeground();
        }

        private void ApplyForeground()
        {
            if (_fill != null)
            {
                _fill.Fill = Foreground;
            }
        }

        private void ApplyUriSource()
        {
            if (_opacityMask != null)
            {
                var uriSource = UriSource;
                if (uriSource != null)
                {
                    var imageSource = new BitmapImage(uriSource);
                    _placeholder.Source = imageSource;
                    _opacityMask.ImageSource = imageSource;
                }
                else
                {
                    _placeholder.ClearValue(Image.SourceProperty);
                    _opacityMask.ClearValue(ImageBrush.ImageSourceProperty);
                }
            }
        }

        private Image _placeholder;
        private Rectangle _fill;
        private ImageBrush _opacityMask;
    }
}
