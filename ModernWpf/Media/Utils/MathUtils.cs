// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace ModernWpf.Media.Utils
{
    internal static class MathUtils
    {
        public static byte ClampToByte(double c)
        {
            if (double.IsNaN(c))
            {
                return 0;
            }
            else if (double.IsPositiveInfinity(c))
            {
                return 255;
            }
            else if (double.IsNegativeInfinity(c))
            {
                return 0;
            }
            c = Math.Round(c);
            if (c <= 0)
            {
                return 0;
            }
            else if (c >= 255)
            {
                return 255;
            }
            else
            {
                return (byte)c;
            }
        }

        public static double ClampToUnit(double c)
        {
            if (double.IsNaN(c))
            {
                return 0;
            }
            else if (double.IsPositiveInfinity(c))
            {
                return 1;
            }
            else if (double.IsNegativeInfinity(c))
            {
                return 0;
            }
            if (c <= 0)
            {
                return 0;
            }
            else if (c >= 1)
            {
                return 1;
            }
            else
            {
                return c;
            }
        }

        public static double DegreesToRadians(double degrees)
        {
            return degrees * (Math.PI / 180.0);
        }

        public static double RadiansToDegrees(double radians)
        {
            return radians * (180.0 / Math.PI);
        }

        public static double Lerp(double left, double right, double scale)
        {
            if (scale <= 0)
            {
                return left;
            }
            else if (scale >= 1)
            {
                return right;
            }
            return left + scale * (right - left);
        }

        public static byte Lerp(byte left, byte right, double scale)
        {
            if (scale <= 0)
            {
                return left;
            }
            else if (scale >= 1)
            {
                return right;
            }
            else if (left == right)
            {
                return left;
            }
            double l = (double)left;
            double r = (double)right;
            return (byte)Math.Round(l + scale * (r - l));
        }
    }
}
