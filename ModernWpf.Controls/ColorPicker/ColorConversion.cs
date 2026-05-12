using System;
using System.Numerics;
using System.Windows.Media;

namespace ModernWpf.Controls
{
    internal static class ColorConversion
    {
        public static Vector4 RgbToHsv(Color color)
        {
            var r = color.R / 255.0;
            var g = color.G / 255.0;
            var b = color.B / 255.0;
            var max = Math.Max(r, Math.Max(g, b));
            var min = Math.Min(r, Math.Min(g, b));
            var delta = max - min;
            double hue;

            if (delta == 0)
            {
                hue = 0;
            }
            else if (max == r)
            {
                hue = 60 * (((g - b) / delta) % 6);
            }
            else if (max == g)
            {
                hue = 60 * (((b - r) / delta) + 2);
            }
            else
            {
                hue = 60 * (((r - g) / delta) + 4);
            }

            if (hue < 0)
            {
                hue += 360;
            }

            var saturation = max == 0 ? 0 : delta / max;
            return new Vector4((float)hue, (float)saturation, (float)max, color.A / 255f);
        }

        public static Color HsvToRgb(Vector4 hsv)
        {
            var hue = NormalizeHue(hsv.X);
            var saturation = Clamp01(hsv.Y);
            var value = Clamp01(hsv.Z);
            var alpha = Clamp01(hsv.W);
            var chroma = value * saturation;
            var x = chroma * (1 - Math.Abs(hue / 60 % 2 - 1));
            var m = value - chroma;
            double r;
            double g;
            double b;

            if (hue < 60)
            {
                r = chroma;
                g = x;
                b = 0;
            }
            else if (hue < 120)
            {
                r = x;
                g = chroma;
                b = 0;
            }
            else if (hue < 180)
            {
                r = 0;
                g = chroma;
                b = x;
            }
            else if (hue < 240)
            {
                r = 0;
                g = x;
                b = chroma;
            }
            else if (hue < 300)
            {
                r = x;
                g = 0;
                b = chroma;
            }
            else
            {
                r = chroma;
                g = 0;
                b = x;
            }

            return Color.FromArgb(
                ToByte(alpha),
                ToByte(r + m),
                ToByte(g + m),
                ToByte(b + m));
        }

        public static Vector4 ClampHsv(Vector4 hsv, int minHue, int maxHue, int minSaturation, int maxSaturation, int minValue, int maxValue)
        {
            return new Vector4(
                Clamp(hsv.X, minHue, maxHue),
                Clamp(hsv.Y, minSaturation / 100f, maxSaturation / 100f),
                Clamp(hsv.Z, minValue / 100f, maxValue / 100f),
                Clamp01(hsv.W));
        }

        public static void ValidateHue(int value, string propertyName)
        {
            if (value < 0 || value > 359)
            {
                throw new ArgumentException(propertyName + " must be between 0 and 359.");
            }
        }

        public static void ValidatePercentage(int value, string propertyName)
        {
            if (value < 0 || value > 100)
            {
                throw new ArgumentException(propertyName + " must be between 0 and 100.");
            }
        }

        private static double NormalizeHue(double hue)
        {
            hue %= 360;
            return hue < 0 ? hue + 360 : hue;
        }

        private static float Clamp(float value, float min, float max)
        {
            return Math.Max(min, Math.Min(max, value));
        }

        private static float Clamp01(float value)
        {
            return Clamp(value, 0, 1);
        }

        private static byte ToByte(double value)
        {
            return (byte)Math.Round(Clamp01((float)value) * 255, MidpointRounding.AwayFromZero);
        }
    }
}
