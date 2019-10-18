using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace ModernWpf.Controls
{
    /// <summary>
    /// Represents an icon that uses a glyph from the specified font.
    /// </summary>
    public class FontIcon : IconElement
    {
        private Grid _layoutRoot;
        private TextBlock _textBlock;

        /// <summary>
        /// Initializes a new instance of the FontIcon class.
        /// </summary>
        public FontIcon()
        {
            SetResourceReference(FontFamilyProperty, "SymbolThemeFontFamily");
        }

        /// <summary>
        /// The identifier for the FontFamily dependency property.
        /// </summary>
        public static readonly DependencyProperty FontFamilyProperty =
                TextElement.FontFamilyProperty.AddOwner(
                        typeof(FontIcon),
                        new FrameworkPropertyMetadata(new FontFamily("Segoe MDL2 Assets"),
                            FrameworkPropertyMetadataOptions.Inherits,
                            OnFontFamilyChanged));

        /// <summary>
        /// Gets or sets the font used to display the icon glyph.
        /// </summary>
        /// <returns>The font used to display the icon glyph.</returns>
        [Bindable(true), Category("Appearance")]
        [Localizability(LocalizationCategory.Font)]
        public FontFamily FontFamily
        {
            get { return (FontFamily)GetValue(FontFamilyProperty); }
            set { SetValue(FontFamilyProperty, value); }
        }

        private static void OnFontFamilyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var fontIcon = (FontIcon)d;
            if (fontIcon._textBlock != null)
            {
                fontIcon._textBlock.FontFamily = (FontFamily)e.NewValue;
            }
        }

        /// <summary>
        /// The identifier for the FontSize dependency property.
        /// </summary>
        public static readonly DependencyProperty FontSizeProperty =
                TextElement.FontSizeProperty.AddOwner(
                        typeof(FontIcon),
                        new FrameworkPropertyMetadata(20d,
                            FrameworkPropertyMetadataOptions.Inherits,
                            OnFontSizeChanged));

        /// <summary>
        /// Gets or sets the size of the icon glyph.
        /// </summary>
        /// <returns>A non-negative value that specifies the font size, measured in pixels.</returns>
        [TypeConverter(typeof(FontSizeConverter))]
        [Bindable(true), Category("Appearance")]
        [Localizability(LocalizationCategory.None)]
        public double FontSize
        {
            get { return (double)GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }

        private static void OnFontSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var fontIcon = (FontIcon)d;
            if (fontIcon._textBlock != null)
            {
                fontIcon._textBlock.FontSize = (double)e.NewValue;
            }
        }

        /// <summary>
        /// The identifier for the FontStyle dependency property.
        /// </summary>
        public static readonly DependencyProperty FontStyleProperty =
                TextElement.FontStyleProperty.AddOwner(
                        typeof(FontIcon),
                        new FrameworkPropertyMetadata(FontStyles.Normal,
                            FrameworkPropertyMetadataOptions.Inherits,
                            OnFontStyleChanged));

        /// <summary>
        /// Gets or sets the font style for the icon glyph.
        /// </summary>
        /// <returns>
        /// A named constant of the enumeration that specifies the style in which the icon
        /// glyph is rendered. The default is **Normal**.
        /// </returns>
        [Bindable(true), Category("Appearance")]
        public FontStyle FontStyle
        {
            get { return (FontStyle)GetValue(FontStyleProperty); }
            set { SetValue(FontStyleProperty, value); }
        }

        private static void OnFontStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var fontIcon = (FontIcon)d;
            if (fontIcon._textBlock != null)
            {
                fontIcon._textBlock.FontStyle = (FontStyle)e.NewValue;
            }
        }

        /// <summary>
        /// The identifier for the FontWeight dependency property.
        /// </summary>
        public static readonly DependencyProperty FontWeightProperty =
                TextElement.FontWeightProperty.AddOwner(
                        typeof(FontIcon),
                        new FrameworkPropertyMetadata(FontWeights.Normal,
                            FrameworkPropertyMetadataOptions.Inherits,
                            OnFontWeightChanged));

        /// <summary>
        /// Gets or sets the thickness of the icon glyph.
        /// </summary>
        /// <returns>
        /// A value that specifies the thickness of the icon glyph. The default is **Normal**.
        /// </returns>
        [Bindable(true), Category("Appearance")]
        public FontWeight FontWeight
        {
            get { return (FontWeight)GetValue(FontWeightProperty); }
            set { SetValue(FontWeightProperty, value); }
        }

        private static void OnFontWeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var fontIcon = (FontIcon)d;
            if (fontIcon._textBlock != null)
            {
                fontIcon._textBlock.FontWeight = (FontWeight)e.NewValue;
            }
        }

        /// <summary>
        /// The identifier for the Glyph dependency property.
        /// </summary>
        public static readonly DependencyProperty GlyphProperty = DependencyProperty.Register(
            nameof(Glyph),
            typeof(string),
            typeof(FontIcon),
            new PropertyMetadata(string.Empty, OnGlyphChanged));

        /// <summary>
        /// Gets or sets the character code that identifies the icon glyph.
        /// </summary>
        /// <returns>The hexadecimal character code for the icon glyph.</returns>
        public string Glyph
        {
            get => (string)GetValue(GlyphProperty);
            set => SetValue(GlyphProperty, value);
        }

        private static void OnGlyphChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var fontIcon = (FontIcon)d;
            if (fontIcon._textBlock != null)
            {
                fontIcon._textBlock.Text = (string)e.NewValue;
            }
        }

        protected override int VisualChildrenCount => 1;

        protected override Visual GetVisualChild(int index)
        {
            if (index == 0)
            {
                EnsureLayoutRoot();
                return _layoutRoot;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            EnsureLayoutRoot();
            _layoutRoot.Measure(availableSize);
            return _layoutRoot.DesiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            EnsureLayoutRoot();
            _layoutRoot.Arrange(new Rect(new Point(), finalSize));
            return finalSize;
        }

        private void EnsureLayoutRoot()
        {
            if (_layoutRoot != null)
                return;

            _textBlock = new TextBlock
            {
                Style = null,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                FontFamily = FontFamily,
                FontSize = FontSize,
                FontStyle = FontStyle,
                FontWeight = FontWeight,
                Text = Glyph
            };

            _layoutRoot = new Grid
            {
                SnapsToDevicePixels = true,
                Children = { _textBlock }
            };

            AddVisualChild(_layoutRoot);
        }
    }
}
