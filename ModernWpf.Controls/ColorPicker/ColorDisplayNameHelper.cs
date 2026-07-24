using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Media;

namespace ModernWpf.Controls
{
    // WPF has no equivalent of WinUI's ColorDisplayNameHelper.  Use the
    // nearest named WPF color so ColorPicker accessibility strings retain the
    // same friendly-color semantics instead of exposing an ARGB hex value.
    internal static class ColorDisplayNameHelper
    {
        private static readonly Lazy<IReadOnlyList<NamedColor>> NamedColors =
            new Lazy<IReadOnlyList<NamedColor>>(CreateNamedColors);

        internal static string ToDisplayName(Color color)
        {
            NamedColor closest = null;
            long closestDistance = long.MaxValue;

            foreach (NamedColor candidate in NamedColors.Value)
            {
                if (candidate.Color == color)
                {
                    return candidate.DisplayName;
                }

                long alpha = color.A - candidate.Color.A;
                long red = color.R - candidate.Color.R;
                long green = color.G - candidate.Color.G;
                long blue = color.B - candidate.Color.B;
                long distance = alpha * alpha + red * red + green * green + blue * blue;
                if (distance < closestDistance)
                {
                    closest = candidate;
                    closestDistance = distance;
                }
            }

            return closest?.DisplayName ?? color.ToString();
        }

        private static IReadOnlyList<NamedColor> CreateNamedColors()
        {
            return typeof(Colors)
                .GetProperties(BindingFlags.Public | BindingFlags.Static)
                .Where(property => property.PropertyType == typeof(Color))
                .OrderBy(property => property.Name, StringComparer.Ordinal)
                .Select(property => new NamedColor(
                    (Color)property.GetValue(null, null),
                    SplitPascalCase(property.Name)))
                .ToArray();
        }

        private static string SplitPascalCase(string value)
        {
            var result = new StringBuilder(value.Length + 4);
            for (int index = 0; index < value.Length; index++)
            {
                char character = value[index];
                if (index > 0 && char.IsUpper(character) && char.IsLower(value[index - 1]))
                {
                    result.Append(' ');
                }

                result.Append(character);
            }

            return result.ToString();
        }

        private sealed class NamedColor
        {
            internal NamedColor(Color color, string displayName)
            {
                Color = color;
                DisplayName = displayName;
            }

            internal Color Color { get; }

            internal string DisplayName { get; }
        }
    }
}
