using System;
using System.Globalization;
using System.Numerics;
using System.Windows.Media;

namespace ModernWpf.Controls
{
    internal static class ColorConversion
    {
        internal enum IncrementDirection
        {
            Lower,
            Higher
        }

        internal enum IncrementAmount
        {
            Small,
            Large
        }

        internal struct Rgb
        {
            public Rgb(double r, double g, double b)
            {
                R = r;
                G = g;
                B = b;
            }

            public double R;
            public double G;
            public double B;
        }

        internal struct Hsv
        {
            public Hsv(double h, double s, double v)
            {
                H = h;
                S = s;
                V = v;
            }

            public double H;
            public double S;
            public double V;
        }

        public static Vector4 RgbToHsv(Color color)
        {
            var hsv = RgbToHsv(RgbFromColor(color));
            return new Vector4((float)hsv.H, (float)hsv.S, (float)hsv.V, color.A / 255f);
        }

        public static Color HsvToRgb(Vector4 hsv)
        {
            return ColorFromRgba(new Hsv(hsv.X, hsv.Y, hsv.Z), Clamp01(hsv.W));
        }

        internal static Hsv RgbToHsv(Rgb rgb)
        {
            double hue = 0;
            double saturation = 0;

            double max = rgb.R >= rgb.G ? (rgb.R >= rgb.B ? rgb.R : rgb.B) : (rgb.G >= rgb.B ? rgb.G : rgb.B);
            double min = rgb.R <= rgb.G ? (rgb.R <= rgb.B ? rgb.R : rgb.B) : (rgb.G <= rgb.B ? rgb.G : rgb.B);
            double value = max;
            double chroma = max - min;

            if (chroma == 0)
            {
                hue = 0.0;
                saturation = 0.0;
            }
            else
            {
                if (rgb.R == max)
                {
                    hue = 60 * (rgb.G - rgb.B) / chroma;
                }
                else if (rgb.G == max)
                {
                    hue = 120 + 60 * (rgb.B - rgb.R) / chroma;
                }
                else
                {
                    hue = 240 + 60 * (rgb.R - rgb.G) / chroma;
                }

                if (hue < 0.0)
                {
                    hue += 360.0;
                }

                saturation = chroma / value;
            }

            return new Hsv(hue, saturation, value);
        }

        internal static Rgb HsvToRgb(Hsv hsv)
        {
            double hue = hsv.H;
            double saturation = Clamp01(hsv.S);
            double value = Clamp01(hsv.V);

            while (hue >= 360.0)
            {
                hue -= 360.0;
            }

            while (hue < 0.0)
            {
                hue += 360.0;
            }

            double chroma = saturation * value;
            double min = value - chroma;

            if (chroma == 0)
            {
                return new Rgb(min, min, min);
            }

            int sextant = (int)(hue / 60);
            double intermediateColorPercentage = hue / 60 - sextant;
            double max = chroma + min;

            double r = 0;
            double g = 0;
            double b = 0;

            switch (sextant)
            {
                case 0:
                    r = max;
                    g = min + chroma * intermediateColorPercentage;
                    b = min;
                    break;

                case 1:
                    r = min + chroma * (1 - intermediateColorPercentage);
                    g = max;
                    b = min;
                    break;

                case 2:
                    r = min;
                    g = max;
                    b = min + chroma * intermediateColorPercentage;
                    break;

                case 3:
                    r = min;
                    g = min + chroma * (1 - intermediateColorPercentage);
                    b = max;
                    break;

                case 4:
                    r = min + chroma * intermediateColorPercentage;
                    g = min;
                    b = max;
                    break;

                case 5:
                    r = max;
                    g = min;
                    b = min + chroma * (1 - intermediateColorPercentage);
                    break;
            }

            return new Rgb(r, g, b);
        }

        internal static Hsv IncrementColorChannel(
            Hsv originalHsv,
            ColorPickerHsvChannel channel,
            IncrementDirection direction,
            IncrementAmount amount,
            bool shouldWrap,
            double minBound,
            double maxBound)
        {
            Hsv newHsv = originalHsv;

            if (amount == IncrementAmount.Large)
            {
                return IncrementToMajorValue(newHsv, channel, direction, shouldWrap, minBound, maxBound);
            }

            newHsv.S *= 100;
            newHsv.V *= 100;

            double value = GetChannelValue(newHsv, channel);
            double previousValue = value;
            double increment = channel == ColorPickerHsvChannel.Hue ? 1 : 1;
            value += direction == IncrementDirection.Lower ? -increment : increment;

            if (value < minBound)
            {
                value = shouldWrap && previousValue == minBound ? maxBound : minBound;
            }

            if (value > maxBound)
            {
                value = shouldWrap && previousValue == maxBound ? minBound : maxBound;
            }

            SetChannelValue(ref newHsv, channel, value);
            newHsv.S /= 100;
            newHsv.V /= 100;

            return newHsv;
        }

        internal static double IncrementAlphaChannel(
            double originalAlpha,
            IncrementDirection direction,
            IncrementAmount amount,
            bool shouldWrap,
            double minBound,
            double maxBound)
        {
            double value = originalAlpha * 100;
            double previousValue = value;
            double increment = amount == IncrementAmount.Large ? 10 : 1;

            value += direction == IncrementDirection.Lower ? -increment : increment;

            if (value < minBound)
            {
                value = shouldWrap && previousValue == minBound ? maxBound : minBound;
            }

            if (value > maxBound)
            {
                value = shouldWrap && previousValue == maxBound ? minBound : maxBound;
            }

            return value / 100.0;
        }

        internal static Rgb HexToRgb(string input)
        {
            return HexToRgba(input).rgb;
        }

        internal static string RgbToHex(Rgb rgb)
        {
            int rByte = ToByte(rgb.R);
            int gByte = ToByte(rgb.G);
            int bByte = ToByte(rgb.B);
            int hexValue = (rByte << 16) + (gByte << 8) + bByte;
            return "#" + hexValue.ToString("X6", CultureInfo.InvariantCulture);
        }

        internal static (Rgb rgb, double alpha) HexToRgba(string input)
        {
            if (string.IsNullOrEmpty(input) || input[0] != '#')
            {
                return (new Rgb(-1, -1, -1), -1);
            }

            string payload = input.Substring(1);
            if (payload.Length != 6 && payload.Length != 8)
            {
                return (new Rgb(-1, -1, -1), -1);
            }

            if (!uint.TryParse(payload, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint hex))
            {
                return (new Rgb(-1, -1, -1), -1);
            }

            byte a = payload.Length == 8 ? (byte)((hex & 0xff000000) >> 24) : (byte)255;
            byte r = (byte)((hex & 0x00ff0000) >> 16);
            byte g = (byte)((hex & 0x0000ff00) >> 8);
            byte b = (byte)(hex & 0x000000ff);

            return (new Rgb(r / 255.0, g / 255.0, b / 255.0), a / 255.0);
        }

        internal static string RgbaToHex(Rgb rgb, double alpha)
        {
            int aByte = ToByte(alpha);
            int rByte = ToByte(rgb.R);
            int gByte = ToByte(rgb.G);
            int bByte = ToByte(rgb.B);
            uint hexValue = (uint)((aByte << 24) + (rByte << 16) + (gByte << 8) + bByte);
            return "#" + hexValue.ToString("X8", CultureInfo.InvariantCulture);
        }

        internal static Color ColorFromRgba(Rgb rgb, double alpha = 1.0)
        {
            return Color.FromArgb(ToByte(alpha), ToByte(rgb.R), ToByte(rgb.G), ToByte(rgb.B));
        }

        internal static Color ColorFromRgba(Hsv hsv, double alpha = 1.0)
        {
            return ColorFromRgba(HsvToRgb(hsv), alpha);
        }

        internal static Rgb RgbFromColor(Color color)
        {
            return new Rgb(color.R / 255.0, color.G / 255.0, color.B / 255.0);
        }

        public static Vector4 ClampHsv(Vector4 hsv, int minHue, int maxHue, int minSaturation, int maxSaturation, int minValue, int maxValue)
        {
            return new Vector4(
                (float)Clamp(hsv.X, minHue, maxHue),
                (float)Clamp(hsv.Y, minSaturation / 100.0, maxSaturation / 100.0),
                (float)Clamp(hsv.Z, minValue / 100.0, maxValue / 100.0),
                (float)Clamp01(hsv.W));
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

        internal static int? TryParseInt(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }

            return int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out int value) ? value : (int?)null;
        }

        internal static double Clamp(double value, double min, double max)
        {
            return Math.Max(min, Math.Min(max, value));
        }

        internal static double Clamp01(double value)
        {
            return Clamp(value, 0, 1);
        }

        internal static byte ToByte(double value)
        {
            return (byte)Math.Round(Clamp01(value) * 255, MidpointRounding.AwayFromZero);
        }

        private static Hsv IncrementToMajorValue(
            Hsv hsv,
            ColorPickerHsvChannel channel,
            IncrementDirection direction,
            bool shouldWrap,
            double minBound,
            double maxBound)
        {
            if (channel == ColorPickerHsvChannel.Saturation || channel == ColorPickerHsvChannel.Value)
            {
                minBound /= 100.0;
                maxBound /= 100.0;
            }

            double value = GetChannelValue(hsv, channel);
            double increment = channel == ColorPickerHsvChannel.Hue ? 30.0 : 0.1;
            value += direction == IncrementDirection.Lower ? -increment : increment;

            if (value > maxBound)
            {
                value = shouldWrap ? minBound : maxBound;
            }

            if (value < minBound)
            {
                value = shouldWrap ? maxBound : minBound;
            }

            if (increment > 0)
            {
                value = Math.Round(value / increment, MidpointRounding.AwayFromZero) * increment;
            }

            value = Clamp(value, minBound, maxBound);
            SetChannelValue(ref hsv, channel, value);
            return hsv;
        }

        private static double GetChannelValue(Hsv hsv, ColorPickerHsvChannel channel)
        {
            switch (channel)
            {
                case ColorPickerHsvChannel.Hue:
                    return hsv.H;
                case ColorPickerHsvChannel.Saturation:
                    return hsv.S;
                case ColorPickerHsvChannel.Value:
                    return hsv.V;
                default:
                    return 0;
            }
        }

        private static void SetChannelValue(ref Hsv hsv, ColorPickerHsvChannel channel, double value)
        {
            switch (channel)
            {
                case ColorPickerHsvChannel.Hue:
                    hsv.H = value;
                    break;
                case ColorPickerHsvChannel.Saturation:
                    hsv.S = value;
                    break;
                case ColorPickerHsvChannel.Value:
                    hsv.V = value;
                    break;
            }
        }
    }
}
