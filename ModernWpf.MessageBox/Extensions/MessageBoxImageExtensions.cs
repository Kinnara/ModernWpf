using System;
using System.Windows;

namespace ModernWpf.Extensions
{
    internal static class MessageBoxImageExtensions
    {
        public static SymbolGlyph ToSymbol(this MessageBoxImage image)
        {
            return image switch
            {
                MessageBoxImage.Error => SymbolGlyph.Error,
                MessageBoxImage.Information => SymbolGlyph.Info,
                MessageBoxImage.Warning => SymbolGlyph.Warning,
                MessageBoxImage.Question => SymbolGlyph.StatusCircleQuestionMark,
                MessageBoxImage.None => (SymbolGlyph)0x2007,
                _ => throw new NotSupportedException(),
            };
        }
    }
}
