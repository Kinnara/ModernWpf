// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ModernWpf.Controls
{
    /// <summary>
    /// Represents an icon source that uses a bitmap as its content.
    /// </summary>
    public class BitmapIconSource : IconSource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BitmapIconSource"/> class.
        /// </summary>
        public BitmapIconSource()
        {
        }

        /// <summary>
        /// Identifies the <see cref="UriSource"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty UriSourceProperty =
            BitmapImage.UriSourceProperty.AddOwner(typeof(BitmapIconSource));

        /// <summary>
        /// Gets or sets the Uniform Resource Identifier (URI) of the bitmap to use as the icon content.
        /// </summary>
        /// <returns>
        /// The <see cref="Uri"/> of the bitmap to use as the icon content. The default is <see langword="null"/>.
        /// </returns>
        public Uri UriSource
        {
            get => (Uri)GetValue(UriSourceProperty);
            set => SetValue(UriSourceProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="ShowAsMonochrome"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ShowAsMonochromeProperty =
            DependencyProperty.Register(
                nameof(ShowAsMonochrome),
                typeof(bool),
                typeof(BitmapIconSource),
                new PropertyMetadata(true));

        /// <summary>
        /// Gets or sets a value that indicates whether the bitmap is shown in a single color.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> to show the bitmap in a single color;
        /// <see langword="false"/> to show the bitmap in full color. The default is <see langword="true"/>.
        /// </returns>
        public bool ShowAsMonochrome
        {
            get => (bool)GetValue(ShowAsMonochromeProperty);
            set => SetValue(ShowAsMonochromeProperty, value);
        }
    }
}
