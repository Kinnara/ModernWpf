using ModernWpf.Controls;

namespace ModernWpf.Extensions
{
    internal static class SymbolExtensions
    {
        public static string ToGlyph(this Symbol symbol) =>
            char.ConvertFromUtf32((int)symbol);
    }
}
