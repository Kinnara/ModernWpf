// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;

namespace ModernWpf.Controls
{
    /// <summary>
    /// Represents an icon source that uses a glyph from the Segoe MDL2 Assets font as its content.
    /// </summary>
    public class SymbolIconSource : IconSource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SymbolIconSource"/> class.
        /// </summary>
        public SymbolIconSource()
        {
        }

        /// <summary>
        /// Identifies the <see cref="Symbol"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SymbolProperty =
            DependencyProperty.Register(
                nameof(Symbol),
                typeof(Symbol),
                typeof(SymbolIconSource),
                new PropertyMetadata(Symbol.Emoji));

        /// <summary>
        /// Gets or sets the Segoe MDL2 Assets glyph used as the icon content.
        /// </summary>
        /// <returns>
        /// A named constant of the enumeration that specifies the Segoe MDL2 Assets glyph to use.
        /// </returns>
        public Symbol Symbol
        {
            get => (Symbol)GetValue(SymbolProperty);
            set => SetValue(SymbolProperty, value);
        }
    }
}
