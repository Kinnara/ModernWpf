// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Windows.Media;

namespace ModernWpf.Media.Utils
{
    internal static class ColorUtils
    {
        public const int DefaultRoundingPrecision = 5;

        // This ignores the Alpha channel of the input color
        public static HSL RGBToHSL(Color rgb, bool round = true, int roundingPrecision = DefaultRoundingPrecision)
        {
            return RGBToHSL(new NormalizedRGB(rgb, false), round, roundingPrecision);
        }

        public static HSL RGBToHSL(in NormalizedRGB rgb, bool round = true, int roundingPrecision = DefaultRoundingPrecision)
        {
            double max = Math.Max(rgb.R, Math.Max(rgb.G, rgb.B));
            double min = Math.Min(rgb.R, Math.Min(rgb.G, rgb.B));
            double delta = max - min;

            double hue = 0;
            if (delta == 0)
            {
                hue = 0;
            }
            else if (max == rgb.R)
            {
                hue = 60 * (((rgb.G - rgb.B) / delta) % 6);
            }
            else if (max == rgb.G)
            {
                hue = 60 * ((rgb.B - rgb.R) / delta + 2);
            }
            else
            {
                hue = 60 * ((rgb.R - rgb.G) / delta + 4);
            }
            if (hue < 0)
            {
                hue += 360;
            }

            double lit = (max + min) / 2;

            double sat = 0;
            if (delta != 0)
            {
                sat = delta / (1 - Math.Abs(2 * lit - 1));
            }

            return new HSL(hue, sat, lit, round, roundingPrecision);
        }

        public static LAB LCHToLAB(in LCH lch, bool round = true, int roundingPrecision = DefaultRoundingPrecision)
        {
            // LCH lit == LAB lit
            double a = 0;
            double b = 0;
            // In chroma.js this case is handled by having the rgb -> lch conversion special case h == 0. In that case it changes h to NaN. Which then requires some NaN checks elsewhere.
            // it seems preferable to handle the case of h = 0 here
            if (lch.H != 0)
            {
                a = Math.Cos(MathUtils.DegreesToRadians(lch.H)) * lch.C;
                b = Math.Sin(MathUtils.DegreesToRadians(lch.H)) * lch.C;
            }

            return new LAB(lch.L, a, b, round, roundingPrecision);
        }

        // This discontinuity in the C parameter at 0 means that floating point errors will often result in values near 0 giving unpredictable results. 
        // EG: 0.0000001 gives a very different result than -0.0000001
        public static LCH LABToLCH(in LAB lab, bool round = true, int roundingPrecision = DefaultRoundingPrecision)
        {
            // LCH lit == LAB lit
            double h = (MathUtils.RadiansToDegrees(Math.Atan2(lab.B, lab.A)) + 360) % 360;
            double c = Math.Sqrt(lab.A * lab.A + lab.B * lab.B);

            return new LCH(lab.L, c, h, round, roundingPrecision);
        }

        // This conversion uses the D65 constants for 2 degrees. That determines the constants used for the pure white point of the XYZ space of 0.95047, 1.0, 1.08883
        public static XYZ LABToXYZ(in LAB lab, bool round = true, int roundingPrecision = DefaultRoundingPrecision)
        {
            double y = (lab.L + 16.0) / 116.0;
            double x = y + (lab.A / 500.0);
            double z = y - (lab.B / 200.0);

            double LABToXYZHelper(double i)
            {
                if (i > 0.206896552)
                {
                    return Math.Pow(i, 3);
                }
                else
                {
                    return 0.12841855 * (i - 0.137931034);
                }
            }

            x = 0.95047 * LABToXYZHelper(x);
            y = LABToXYZHelper(y);
            z = 1.08883 * LABToXYZHelper(z);

            return new XYZ(x, y, z, round, roundingPrecision);
        }

        // This conversion uses the D65 constants for 2 degrees. That determines the constants used for the pure white point of the XYZ space of 0.95047, 1.0, 1.08883
        public static LAB XYZToLAB(in XYZ xyz, bool round = true, int roundingPrecision = DefaultRoundingPrecision)
        {
            double XYZToLABHelper(double i)
            {
                if (i > 0.008856452)
                {
                    return Math.Pow(i, 1.0 / 3.0);
                }
                else
                {
                    return i / 0.12841855 + 0.137931034;
                }
            }

            double x = XYZToLABHelper(xyz.X / 0.95047);
            double y = XYZToLABHelper(xyz.Y);
            double z = XYZToLABHelper(xyz.Z / 1.08883);

            double l = (116.0 * y) - 16.0;
            double a = 500.0 * (x - y);
            double b = -200.0 * (z - y);

            return new LAB(l, a, b, round, roundingPrecision);
        }

        // This ignores the Alpha channel of the input color
        public static XYZ RGBToXYZ(Color rgb, bool round = true, int roundingPrecision = DefaultRoundingPrecision)
        {
            return RGBToXYZ(new NormalizedRGB(rgb, false), round, roundingPrecision);
        }

        public static XYZ RGBToXYZ(in NormalizedRGB rgb, bool round = true, int roundingPrecision = DefaultRoundingPrecision)
        {
            double RGBToXYZHelper(double i)
            {
                if (i <= 0.04045)
                {
                    return i / 12.92;
                }
                else
                {
                    return Math.Pow((i + 0.055) / 1.055, 2.4);
                }
            }

            double r = RGBToXYZHelper(rgb.R);
            double g = RGBToXYZHelper(rgb.G);
            double b = RGBToXYZHelper(rgb.B);

            double x = r * 0.4124564 + g * 0.3575761 + b * 0.1804375;
            double y = r * 0.2126729 + g * 0.7151522 + b * 0.0721750;
            double z = r * 0.0193339 + g * 0.1191920 + b * 0.9503041;

            return new XYZ(x, y, z, round, roundingPrecision);
        }

        public static NormalizedRGB XYZToRGB(in XYZ xyz, bool round = true, int roundingPrecision = DefaultRoundingPrecision)
        {
            double XYZToRGBHelper(double i)
            {
                if (i <= 0.0031308)
                {
                    return i * 12.92;
                }
                else
                {
                    return 1.055 * Math.Pow(i, 1 / 2.4) - 0.055;
                }
            }

            double r = XYZToRGBHelper(xyz.X * 3.2404542 - xyz.Y * 1.5371385 - xyz.Z * 0.4985314);
            double g = XYZToRGBHelper(xyz.X * -0.9692660 + xyz.Y * 1.8760108 + xyz.Z * 0.0415560);
            double b = XYZToRGBHelper(xyz.X * 0.0556434 - xyz.Y * 0.2040259 + xyz.Z * 1.0572252);

            return new NormalizedRGB(MathUtils.ClampToUnit(r), MathUtils.ClampToUnit(g), MathUtils.ClampToUnit(b), round, roundingPrecision);
        }

        // This ignores the Alpha channel of the input color
        public static LAB RGBToLAB(Color rgb, bool round = true, int roundingPrecision = DefaultRoundingPrecision)
        {
            return RGBToLAB(new NormalizedRGB(rgb, false), round, roundingPrecision);
        }

        public static LAB RGBToLAB(in NormalizedRGB rgb, bool round = true, int roundingPrecision = DefaultRoundingPrecision)
        {
            XYZ xyz = RGBToXYZ(rgb, false);
            return XYZToLAB(xyz, round, roundingPrecision);
        }

        public static NormalizedRGB LABToRGB(in LAB lab, bool round = true, int roundingPrecision = DefaultRoundingPrecision)
        {
            XYZ xyz = LABToXYZ(lab, false);
            return XYZToRGB(xyz, round, roundingPrecision);
        }

        public static LCH RGBToLCH(in NormalizedRGB rgb, bool round = true, int roundingPrecision = DefaultRoundingPrecision)
        {
            // The discontinuity near 0 in LABToLCH means we should round here even if the bound param is false
            LAB lab = RGBToLAB(rgb, true, 4);

            // This appears redundant but is actually nescessary in order to prevent floating point rounding errors from throwing off the Atan2 computation in LABToLCH
            // https://msdn.microsoft.com/en-us/library/system.math.atan2(v=vs.110).aspx
            // For the RGB value 255,255,255 what happens is the a value appears to be rounded to 0 but is still treated as negative by Atan2 which then returns PI instead of 0

            double l = lab.L == 0 ? 0 : lab.L;
            double a = lab.A == 0 ? 0 : lab.A;
            double b = lab.B == 0 ? 0 : lab.B;

            return LABToLCH(new LAB(l, a, b, false), round, roundingPrecision);
        }

        public static NormalizedRGB LCHToRGB(in LCH lch, bool round = true, int roundingPrecision = DefaultRoundingPrecision)
        {
            LAB lab = LCHToLAB(lch, false);
            return LABToRGB(lab, round, roundingPrecision);
        }

        // This ignores the Alpha channel of both input colors

        public static Color InterpolateRGB(Color left, Color right, double position)
        {
            if (position <= 0)
            {
                return left;
            }
            if (position >= 1)
            {
                return right;
            }
            return Color.FromArgb(MathUtils.Lerp(left.A, right.A, position), MathUtils.Lerp(left.R, right.R, position), MathUtils.Lerp(left.G, right.G, position), MathUtils.Lerp(left.B, right.B, position));
        }

        public static NormalizedRGB InterpolateRGB(in NormalizedRGB left, in NormalizedRGB right, double position)
        {
            if (position <= 0)
            {
                return left;
            }
            if (position >= 1)
            {
                return right;
            }
            return new NormalizedRGB(MathUtils.Lerp(left.R, right.R, position), MathUtils.Lerp(left.G, right.G, position), MathUtils.Lerp(left.B, right.B, position), false);
        }

        // Generally looks better than RGB for interpolation
        public static LAB InterpolateLAB(in LAB left, in LAB right, double position)
        {
            if (position <= 0)
            {
                return left;
            }
            if (position >= 1)
            {
                return right;
            }
            return new LAB(MathUtils.Lerp(left.L, right.L, position), MathUtils.Lerp(left.A, right.A, position), MathUtils.Lerp(left.B, right.B, position), false);
        }

        // Possibly a better choice than LAB for very dark colors
        public static XYZ InterpolateXYZ(in XYZ left, in XYZ right, double position)
        {
            if (position <= 0)
            {
                return left;
            }
            if (position >= 1)
            {
                return right;
            }
            return new XYZ(MathUtils.Lerp(left.X, right.X, position), MathUtils.Lerp(left.Y, right.Y, position), MathUtils.Lerp(left.Z, right.Z, position), false);
        }

        public static NormalizedRGB InterpolateColor(in NormalizedRGB left, in NormalizedRGB right, double position, ColorScaleInterpolationMode mode)
        {
            switch (mode)
            {
                case ColorScaleInterpolationMode.LAB:
                    var leftLAB = ColorUtils.RGBToLAB(left, false);
                    var rightLAB = ColorUtils.RGBToLAB(right, false);
                    return LABToRGB(InterpolateLAB(leftLAB, rightLAB, position));
                case ColorScaleInterpolationMode.XYZ:
                    var leftXYZ = RGBToXYZ(left, false);
                    var rightXYZ = RGBToXYZ(right, false);
                    return XYZToRGB(InterpolateXYZ(leftXYZ, rightXYZ, position));
                default:
                    return InterpolateRGB(left, right, position);
            }
        }
    }
}
