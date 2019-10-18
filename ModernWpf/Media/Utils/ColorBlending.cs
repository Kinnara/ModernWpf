// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace ModernWpf.Media.Utils
{
    internal enum ColorBlendMode { Burn, Darken, Dodge, Lighten, Multiply, Overlay, Screen };

    internal static class ColorBlending
    {
        public const double DefaultSaturationConstant = 18.0;

        public static NormalizedRGB SaturateViaLCH(in NormalizedRGB input, double saturation, double saturationConstant = DefaultSaturationConstant)
        {
            LCH lch = ColorUtils.RGBToLCH(input, false);
            double saturated = lch.C + saturation * saturationConstant;
            if (saturated < 0)
            {
                saturated = 0;
            }
            return ColorUtils.LCHToRGB(new LCH(lch.L, saturated, lch.H, false), false);
        }

        public static NormalizedRGB Blend(in NormalizedRGB bottom, in NormalizedRGB top, ColorBlendMode mode)
        {
            switch (mode)
            {
                case ColorBlendMode.Burn:
                    return BlendBurn(bottom, top);
                case ColorBlendMode.Darken:
                    return BlendDarken(bottom, top);
                case ColorBlendMode.Dodge:
                    return BlendDodge(bottom, top);
                case ColorBlendMode.Lighten:
                    return BlendLighten(bottom, top);
                case ColorBlendMode.Multiply:
                    return BlendMultiply(bottom, top);
                case ColorBlendMode.Overlay:
                    return BlendOverlay(bottom, top);
                case ColorBlendMode.Screen:
                    return BlendScreen(bottom, top);
                default:
                    throw new ArgumentException("Unknown blend mode", "mode");
            }
        }

        public static NormalizedRGB BlendBurn(in NormalizedRGB bottom, in NormalizedRGB top)
        {
            return new NormalizedRGB(BlendBurn(bottom.R, top.R), BlendBurn(bottom.G, top.G), BlendBurn(bottom.B, top.B), false);
        }

        // single channel in the range [0.0,1.0]
        public static double BlendBurn(double bottom, double top)
        {
            if (top == 0.0)
            {
                // Despite the discontinuity, other sources seem to use 0.0 here instead of 1
                return 0.0;
            }
            return 1.0 - (1.0 - bottom) / top;
        }

        public static NormalizedRGB BlendDarken(in NormalizedRGB bottom, in NormalizedRGB top)
        {
            return new NormalizedRGB(BlendDarken(bottom.R, top.R), BlendDarken(bottom.G, top.G), BlendDarken(bottom.B, top.B), false);
        }

        // single channel in the range [0.0,1.0]
        public static double BlendDarken(double bottom, double top)
        {
            return Math.Min(bottom, top);
        }

        public static NormalizedRGB BlendDodge(in NormalizedRGB bottom, in NormalizedRGB top)
        {
            return new NormalizedRGB(BlendDodge(bottom.R, top.R), BlendDodge(bottom.G, top.G), BlendDodge(bottom.B, top.B), false);
        }

        // single channel in the range [0.0,1.0]
        public static double BlendDodge(double bottom, double top)
        {
            if (top >= 1.0)
            {
                return 1.0;
            }
            double retVal = bottom / (1.0 - top);
            if (retVal >= 1.0)
            {
                return 1.0;
            }
            return retVal;
        }

        public static NormalizedRGB BlendLighten(in NormalizedRGB bottom, in NormalizedRGB top)
        {
            return new NormalizedRGB(BlendLighten(bottom.R, top.R), BlendLighten(bottom.G, top.G), BlendLighten(bottom.B, top.B), false);
        }

        // single channel in the range [0.0,1.0]
        public static double BlendLighten(double bottom, double top)
        {
            return Math.Max(bottom, top);
        }

        public static NormalizedRGB BlendMultiply(in NormalizedRGB bottom, in NormalizedRGB top)
        {
            return new NormalizedRGB(BlendMultiply(bottom.R, top.R), BlendMultiply(bottom.G, top.G), BlendMultiply(bottom.B, top.B), false);
        }

        // single channel in the range [0.0,1.0]
        public static double BlendMultiply(double bottom, double top)
        {
            return bottom * top;
        }

        public static NormalizedRGB BlendOverlay(in NormalizedRGB bottom, in NormalizedRGB top)
        {
            return new NormalizedRGB(BlendOverlay(bottom.R, top.R), BlendOverlay(bottom.G, top.G), BlendOverlay(bottom.B, top.B), false);
        }

        // single channel in the range [0.0,1.0]
        public static double BlendOverlay(double bottom, double top)
        {
            if (bottom < 0.5)
            {
                return MathUtils.ClampToUnit(2.0 * top * bottom);
            }
            else
            {
                return MathUtils.ClampToUnit(1.0 - 2.0 * (1.0 - top) * (1.0 - bottom));
            }
        }

        public static NormalizedRGB BlendScreen(in NormalizedRGB bottom, in NormalizedRGB top)
        {
            return new NormalizedRGB(BlendScreen(bottom.R, top.R), BlendScreen(bottom.G, top.G), BlendScreen(bottom.B, top.B), false);
        }

        // single channel in the range [0.0,1.0]
        public static double BlendScreen(double bottom, double top)
        {
            return 1.0 - (1.0 - top) * (1.0 - bottom);
        }
    }
}
