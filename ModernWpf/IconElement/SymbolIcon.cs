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
            // Symbol keeps the legacy public enum values for API compatibility,
            // while current WinUI renders the recommended Segoe Fluent Icons
            // codepoints to avoid collisions in the legacy E0-E5 ranges.
            var codePoint = ((int)symbol) switch
            {
                0xE10B => 0xE8FB,
                0xE168 => 0xE910,
                0xE109 => 0xE710,
                0xE1E2 => 0xE8FA,
                0xE1A7 => 0xE7EF,
                0xE1A1 => 0xE8E3,
                0xE1A2 => 0xE8E4,
                0xE1A0 => 0xE8E2,
                0xE179 => 0xE71D,
                0xE16C => 0xE723,
                0xE12D => 0xE8A2,
                0xE189 => 0xE8D6,
                0xE112 => 0xE72B,
                0xE1D8 => 0xE73F,
                0xE1E0 => 0xE8F8,
                0xE19B => 0xE8DD,
                0xE12F => 0xE8A4,
                0xE155 => 0xE7C5,
                0xE133 => 0xE8FD,
                0xE1D0 => 0xE8EF,
                0xE163 => 0xE787,
                0xE161 => 0xE8BF,
                0xE1DB => 0xE8F5,
                0xE162 => 0xE8C0,
                0xE114 => 0xE722,
                0xE10A => 0xE711,
                0xE15A => 0xE8BA,
                0xE1C9 => 0xE8EA,
                0xE164 => 0xE8C1,
                0xE106 => 0xE894,
                0xE1C5 => 0xE8E6,
                0xE121 => 0xE823,
                0xE190 => 0xE7F0,
                0xE127 => 0xE89F,
                0xE134 => 0xE90A,
                0xE13D => 0xE77B,
                0xE187 => 0xE8D4,
                0xE136 => 0xE779,
                0xE181 => 0xE8CF,
                0xE16F => 0xE8C8,
                0xE123 => 0xE7A8,
                0xE16B => 0xE8C6,
                0xE107 => 0xE74D,
                0xE1D1 => 0xE8F0,
                0xE194 => 0xE8D8,
                0xE17A => 0xE8CD,
                0xE19E => 0xE8E0,
                0xE147 => 0xE90E,
                0xE145 => 0xE90C,
                0xE146 => 0xE90D,
                0xE130 => 0xE8A5,
                0xE118 => 0xE896,
                0xE104 => 0xE70F,
                0xE11D => 0xE899,
                0xE170 => 0xE76E,
                0xE113 => 0xE734,
                0xE16E => 0xE71C,
                0xE11A => 0xE721,
                0xE129 => 0xE7C1,
                0xE188 => 0xE8B7,
                0xE185 => 0xE8D2,
                0xE186 => 0xE8D3,
                0xE1C6 => 0xE8E7,
                0xE1C7 => 0xE8E8,
                0xE1C8 => 0xE8E9,
                0xE111 => 0xE72A,
                0xE1E9 => 0xE908,
                0xE1D9 => 0xE740,
                0xE700 => 0xE700,
                0xE12B => 0xE774,
                0xE143 => 0xE8AD,
                0xE1E4 => 0xE8FC,
                0xE184 => 0xE8D1,
                0xE137 => 0xE778,
                0xE11B => 0xE897,
                0xE16A => 0xE8C5,
                0xE193 => 0xE7E6,
                0xE10F => 0xE80F,
                0xE150 => 0xE8B5,
                0xE151 => 0xE8B6,
                0xE171 => 0xE8C9,
                0xE199 => 0xE8DB,
                0xE144 => 0xE765,
                0xE11F => 0xE89B,
                0xE1D3 => 0xE8F1,
                0xE19F => 0xE8E1,
                0xE19D => 0xE8DF,
                0xE167 => 0xE71B,
                0xE14C => 0xEA37,
                0xE119 => 0xE715,
                0xE135 => 0xE8A8,
                0xE120 => 0xE89C,
                0xE172 => 0xE8CA,
                0xE165 => 0xE8C2,
                0xE178 => 0xE912,
                0xE1C4 => 0xE707,
                0xE17B => 0xE8CE,
                0xE139 => 0xE7B7,
                0xE1D5 => 0xE77C,
                0xE15F => 0xE8BD,
                0xE1D6 => 0xE720,
                0xE10C => 0xE712,
                0xE19C => 0xE8DE,
                0xE142 => 0xE90B,
                0xE198 => 0xE74F,
                0xE1DA => 0xE8F4,
                0xE17C => 0xE78B,
                0xE101 => 0xE893,
                0xE1E6 => 0xE905,
                0xE1A5 => 0xE8E5,
                0xE197 => 0xE8DA,
                0xE126 => 0xE8A0,
                0xE17D => 0xE7AC,
                0xE14F => 0xE8B4,
                0xE1A6 => 0xE7EE,
                0xE1CE => 0xE734,
                0xE132 => 0xE729,
                0xE160 => 0xE7C3,
                0xE16D => 0xE77F,
                0xE103 => 0xE769,
                0xE125 => 0xE716,
                0xE192 => 0xE8D7,
                0xE13A => 0xE717,
                0xE1D4 => 0xE780,
                0xE158 => 0xE8B9,
                0xE141 => 0xE718,
                0xE18A => 0xE18A,
                0xE102 => 0xE768,
                0xE1D7 => 0xE8F3,
                0xE295 => 0xE8FF,
                0xE12A => 0xE8A1,
                0xE100 => 0xE892,
                0xE749 => 0xE749,
                0xE182 => 0xE8D0,
                0xE131 => 0xE8A6,
                0xE166 => 0xE8C3,
                0xE10D => 0xE7A6,
                0xE149 => 0xE72C,
                0xE148 => 0xE8AF,
                0xE108 => 0xE738,
                0xE13E => 0xE8AC,
                0xE15E => 0xE90F,
                0xE1CD => 0xE8EE,
                0xE1CC => 0xE8ED,
                0xE1DE => 0xE730,
                0xE1CA => 0xE8EB,
                0xE14A => 0xE7AD,
                0xE124 => 0xE89E,
                0xE105 => 0xE74E,
                0xE159 => 0xE78C,
                0xE294 => 0xE8FE,
                0xE14E => 0xE8B3,
                0xE122 => 0xE724,
                0xE18C => 0xE7B5,
                0xE18D => 0xE97B,
                0xE115 => 0xE713,
                0xE72D => 0xE72D,
                0xE14D => 0xE719,
                0xE169 => 0xE8C4,
                0xE15C => 0xE8BC,
                0xE14B => 0xE8B1,
                0xE173 => 0xE786,
                0xE1CF => 0xE735,
                0xE174 => 0xE8CB,
                0xE15B => 0xE71A,
                0xE191 => 0xE620,
                0xE1C3 => 0xE913,
                0xE13C => 0xE8AB,
                0xE1E1 => 0xE8F9,
                0xE117 => 0xE895,
                0xE1DF => 0xE8F7,
                0xE1CB => 0xE8EC,
                0xE1D2 => 0xF5F0,
                0xE1E8 => 0xE907,
                0xE1E3 => 0xE7C9,
                0xE12C => 0xE78A,
                0xE1E7 => 0xE906,
                0xE11E => 0xE89A,
                0xE19A => 0xE8DC,
                0xE10E => 0xE7A7,
                0xE195 => 0xE8D9,
                0xE196 => 0xE77A,
                0xE1DD => 0xE8F6,
                0xE110 => 0xE74A,
                0xE11C => 0xE898,
                0xE116 => 0xE714,
                0xE13B => 0xE8AA,
                0xE18B => 0xE890,
                0xE138 => 0xE8A9,
                0xE15D => 0xE767,
                0xE156 => 0xE8B8,
                0xE128 => 0xE909,
                0xE990 => 0xE990,
                0xE1E5 => 0xE904,
                0xE1A3 => 0xE71E,
                0xE12E => 0xE8A3,
                0xE1A4 => 0xE71F,
                _ => (int)symbol
            };

            return char.ConvertFromUtf32(codePoint);
        }

        private TextBlock _textBlock;
    }
}
