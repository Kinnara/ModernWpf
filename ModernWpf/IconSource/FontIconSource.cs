// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;
using System.Windows.Media;

namespace ModernWpf.Controls
{
    /// <summary>
    /// Represents an icon source that uses a glyph from the specified font.
    /// </summary>
    public class FontIconSource : IconSource
    {
        const string c_fontIconSourceDefaultFontFamily = "Segoe MDL2 Assets";

        /// <summary>
        /// Initializes a new instance of the <see cref="FontIconSource"/> class.
        /// </summary>
        public FontIconSource()
        {
        }

        /// <summary>
        /// Identifies the <see cref="FontFamily"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty FontFamilyProperty =
            DependencyProperty.Register(
                nameof(FontFamily),
                typeof(FontFamily),
                typeof(FontIconSource),
                new PropertyMetadata(new FontFamily(c_fontIconSourceDefaultFontFamily)));

        /// <summary>
        /// Gets or sets the font used to display the icon glyph.
        /// </summary>
        /// <returns>
        /// The font used to display the icon glyph.
        /// </returns>
        public FontFamily FontFamily
        {
            get => (FontFamily)GetValue(FontFamilyProperty);
            set => SetValue(FontFamilyProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="FontSize"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty FontSizeProperty =
            DependencyProperty.Register(
                nameof(FontSize),
                typeof(double),
                typeof(FontIconSource),
                new PropertyMetadata(20.0));

        /// <summary>
        /// Gets or sets the size of the icon glyph.
        /// </summary>
        /// <returns>
        /// A non-negative value that specifies the font size, measured in pixels.
        /// </returns>
        public double FontSize
        {
            get => (double)GetValue(FontSizeProperty);
            set => SetValue(FontSizeProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="FontStyle"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty FontStyleProperty =
            DependencyProperty.Register(
                nameof(FontStyle),
                typeof(FontStyle),
                typeof(FontIconSource),
                new PropertyMetadata(FontStyles.Normal));

        /// <summary>
        /// Gets or sets the font style for the icon glyph.
        /// </summary>
        /// <returns>
        /// A named constant of the enumeration that specifies the style in which the icon glyph is rendered.
        /// The default is <see cref="FontStyles.Normal"/>.
        /// </returns>
        public FontStyle FontStyle
        {
            get => (FontStyle)GetValue(FontStyleProperty);
            set => SetValue(FontStyleProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="FontWeight"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty FontWeightProperty =
            DependencyProperty.Register(
                nameof(FontWeight),
                typeof(FontWeight),
                typeof(FontIconSource),
                new PropertyMetadata(FontWeights.Normal));

        /// <summary>
        /// Gets or sets the thickness of the icon glyph.
        /// </summary>
        /// <returns>
        /// A value that specifies the thickness of the icon glyph.
        /// The default is <see cref="FontWeights.Normal"/>.
        /// </returns>
        public FontWeight FontWeight
        {
            get => (FontWeight)GetValue(FontWeightProperty);
            set => SetValue(FontWeightProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="Glyph"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty GlyphProperty =
            DependencyProperty.Register(
                nameof(Glyph),
                typeof(string),
                typeof(FontIconSource),
                new PropertyMetadata(string.Empty));

        /// <summary>
        /// Gets or sets the character code that identifies the icon glyph.
        /// </summary>
        /// <returns>
        /// The hexadecimal character code for the icon glyph.
        /// </returns>
        public string Glyph
        {
            get => (string)GetValue(GlyphProperty);
            set => SetValue(GlyphProperty, value);
        }
    }
}
