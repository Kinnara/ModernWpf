using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ModernWpf.Controls
{
    /// <summary>
    /// Represents an icon that uses a glyph from the Segoe MDL2 Assets font as its content.
    /// </summary>
    public sealed class SymbolIcon : IconElement
    {
        /// <summary>
        /// Initializes a new instance of the SymbolIcon class.
        /// </summary>
        public SymbolIcon()
        {
        }

        /// <summary>
        /// Initializes a new instance of the SymbolIcon class using the specified symbol.
        /// </summary>
        /// <param name="symbol">
        /// A named constant of the enumeration that specifies the Segoe MDL2 Assets glyph
        /// to use. The default is **null**.
        /// </param>
        public SymbolIcon(Symbol symbol)
        {
            Symbol = symbol;
        }

        #region Symbol

        /// <summary>
        /// Identifies the Symbol dependency property.
        /// </summary>
        public static readonly DependencyProperty SymbolProperty =
            DependencyProperty.Register(
                nameof(Symbol),
                typeof(Symbol),
                typeof(SymbolIcon),
                new PropertyMetadata(Symbol.Emoji, OnSymbolChanged));

        /// <summary>
        /// Gets or sets the Segoe MDL2 Assets glyph used as the icon content.
        /// </summary>
        /// <returns>
        /// A named constant of the numeration that specifies the Segoe MDL2 Assets glyph
        /// to use.
        /// </returns>
        public Symbol Symbol
        {
            get => (Symbol)GetValue(SymbolProperty);
            set => SetValue(SymbolProperty, value);
        }

        private static void OnSymbolChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SymbolIcon)d).OnSymbolChanged(e);
        }

        private void OnSymbolChanged(DependencyPropertyChangedEventArgs e)
        {
            if (_textBlock != null)
            {
                _textBlock.Text = ConvertToString((Symbol)e.NewValue);
            }
        }

        #endregion

        #region FontSize

        internal static readonly DependencyProperty FontSizeProperty =
            DependencyProperty.Register(
                nameof(FontSize),
                typeof(double),
                typeof(SymbolIcon),
                new PropertyMetadata(20.0, OnFontSizeChanged));

        internal double FontSize
        {
            get => (double)GetValue(FontSizeProperty);
            set => SetValue(FontSizeProperty, value);
        }

        private static void OnFontSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SymbolIcon)d).OnFontSizeChanged(e);
        }

        private void OnFontSizeChanged(DependencyPropertyChangedEventArgs e)
        {
            if (_textBlock != null)
            {
                _textBlock.FontSize = (double)e.NewValue;
            }
        }

        #endregion

        private protected override void InitializeChildren()
        {
            _textBlock = new TextBlock
            {
                Style = null,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                FontSize = FontSize,
                FontStyle = FontStyles.Normal,
                FontWeight = FontWeights.Normal,
                Text = ConvertToString(Symbol)
            };

            _textBlock.SetResourceReference(TextBlock.FontFamilyProperty, "SymbolThemeFontFamily");

            if (ShouldInheritForegroundFromVisualParent)
            {
                _textBlock.Foreground = VisualParentForeground;
            }

            Children.Add(_textBlock);
        }

        private protected override void OnShouldInheritForegroundFromVisualParentChanged()
        {
            if (_textBlock != null)
            {
                if (ShouldInheritForegroundFromVisualParent)
                {
                    _textBlock.Foreground = VisualParentForeground;
                }
                else
                {
                    _textBlock.ClearValue(TextBlock.ForegroundProperty);
                }
            }
        }

        private protected override void OnVisualParentForegroundPropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            if (ShouldInheritForegroundFromVisualParent && _textBlock != null)
            {
                _textBlock.Foreground = (Brush)args.NewValue;
            }
        }

        private static string ConvertToString(Symbol symbol)
        {
            return char.ConvertFromUtf32((int)symbol).ToString();
        }

        private TextBlock _textBlock;
    }
}
