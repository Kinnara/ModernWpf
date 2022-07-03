namespace ModernWpf.Extensions
{
    internal static class SymbolGlyphExtensions
    {
        public static string ToGlyph(this SymbolGlyph symbol) =>
            char.ConvertFromUtf32((int)symbol);
    }
}
