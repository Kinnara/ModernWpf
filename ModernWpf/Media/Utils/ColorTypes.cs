// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Windows.Media;

namespace ModernWpf.Media.Utils
{
    // Valid values for each channel are ∈ [0.0,1.0]
    // But sometimes it is useful to allow values outside that range during calculations as long as they are clamped eventually
    internal readonly struct NormalizedRGB : IEquatable<NormalizedRGB>
    {
        public const int DefaultRoundingPrecision = 5;

        public NormalizedRGB(double r, double g, double b, bool round = true, int roundingPrecision = DefaultRoundingPrecision)
        {
            if (round)
            {
                R = Math.Round(r, roundingPrecision);
                G = Math.Round(g, roundingPrecision);
                B = Math.Round(b, roundingPrecision);
            }
            else
            {
                R = r;
                G = g;
                B = b;
            }
        }

        public NormalizedRGB(in Color rgb, bool round = true, int roundingPrecision = DefaultRoundingPrecision)
        {
            if (round)
            {
                R = Math.Round((double)rgb.R / 255.0, roundingPrecision);
                G = Math.Round((double)rgb.G / 255.0, roundingPrecision);
                B = Math.Round((double)rgb.B / 255.0, roundingPrecision);
            }
            else
            {
                R = (double)rgb.R / 255.0;
                G = (double)rgb.G / 255.0;
                B = (double)rgb.B / 255.0;
            }
        }

        public Color Denormalize(byte a = 255)
        {
            return Color.FromArgb(a, MathUtils.ClampToByte(R * 255.0), MathUtils.ClampToByte(G * 255.0), MathUtils.ClampToByte(B * 255.0));
        }

        public readonly double R;
        public readonly double G;
        public readonly double B;

        #region IEquatable<NormalizedRGB>

        public bool Equals(NormalizedRGB other)
        {
            return R == other.R && G == other.G && B == other.B;
        }

        #endregion

        #region Equals

        public override bool Equals(object obj)
        {
            if (obj is NormalizedRGB other)
            {
                return R == other.R && G == other.G && B == other.B;
            }
            else
            {
                return base.Equals(obj);
            }
        }

        public override int GetHashCode()
        {
            return R.GetHashCode() ^ G.GetHashCode() ^ B.GetHashCode();
        }

        #endregion

        #region ToString

        public override string ToString()
        {
            return string.Format("{0},{1},{2}", R, G, B);
        }

        #endregion
    }

    // H ∈ [0.0,360.0]
    // S ∈ [0.0,1.0]
    // L ∈ [0.0,1.0]
    internal readonly struct HSL : IEquatable<HSL>
    {
        public const int DefaultRoundingPrecision = 5;

        public HSL(double h, double s, double l, bool round = true, int roundingPrecision = DefaultRoundingPrecision)
        {
            if (round)
            {
                H = Math.Round(h, roundingPrecision);
                S = Math.Round(s, roundingPrecision);
                L = Math.Round(l, roundingPrecision);
            }
            else
            {
                H = h;
                S = s;
                L = l;
            }
        }

        public readonly double H;
        public readonly double S;
        public readonly double L;

        #region IEquatable<HSL>

        public bool Equals(HSL other)
        {
            return H == other.H && S == other.S && L == other.L;
        }

        #endregion

        #region Equals

        public override bool Equals(object obj)
        {
            if (obj is HSL other)
            {
                return H == other.H && S == other.S && L == other.L;
            }
            else
            {
                return base.Equals(obj);
            }
        }

        public override int GetHashCode()
        {
            return H.GetHashCode() ^ S.GetHashCode() ^ L.GetHashCode();
        }

        #endregion

        #region ToString

        public override string ToString()
        {
            return string.Format("{0},{1},{2}", H, S, L);
        }

        #endregion
    }

    // H ∈ [0.0,360.0]
    // S ∈ [0.0,1.0]
    // V ∈ [0.0,1.0]
    internal readonly struct HSV : IEquatable<HSV>
    {
        public readonly double H;
        public readonly double S;
        public readonly double V;

        #region IEquatable<HSV>

        public bool Equals(HSV other)
        {
            return H == other.H && S == other.S && V == other.V;
        }

        #endregion

        #region Equals

        public override bool Equals(object obj)
        {
            if (obj is HSV other)
            {
                return H == other.H && S == other.S && V == other.V;
            }
            else
            {
                return base.Equals(obj);
            }
        }

        public override int GetHashCode()
        {
            return H.GetHashCode() ^ S.GetHashCode() ^ V.GetHashCode();
        }

        #endregion

        #region ToString

        public override string ToString()
        {
            return string.Format("{0},{1},{2}", H, S, V);
        }

        #endregion
    }

    internal readonly struct LAB : IEquatable<LAB>
    {
        public const int DefaultRoundingPrecision = 5;

        public LAB(double l, double a, double b, bool round = true, int roundingPrecision = DefaultRoundingPrecision)
        {
            if (round)
            {
                L = Math.Round(l, roundingPrecision);
                A = Math.Round(a, roundingPrecision);
                B = Math.Round(b, roundingPrecision);
            }
            else
            {
                L = l;
                A = a;
                B = b;
            }
        }

        public readonly double L;
        public readonly double A;
        public readonly double B;

        #region IEquatable<LAB>

        public bool Equals(LAB other)
        {
            return L == other.L && A == other.A && B == other.B;
        }

        #endregion

        #region Equals

        public override bool Equals(object obj)
        {
            if (obj is LAB other)
            {
                return L == other.L && A == other.A && B == other.B;
            }
            else
            {
                return base.Equals(obj);
            }
        }

        public override int GetHashCode()
        {
            return L.GetHashCode() ^ A.GetHashCode() ^ B.GetHashCode();
        }

        #endregion

        #region ToString

        public override string ToString()
        {
            return string.Format("{0},{1},{2}", L, A, B);
        }

        #endregion
    }

    internal readonly struct LCH : IEquatable<LCH>
    {
        public const int DefaultRoundingPrecision = 5;

        public LCH(double l, double c, double h, bool round = true, int roundingPrecision = DefaultRoundingPrecision)
        {
            if (round)
            {
                L = Math.Round(l, roundingPrecision);
                C = Math.Round(c, roundingPrecision);
                H = Math.Round(h, roundingPrecision);
            }
            else
            {
                L = l;
                C = c;
                H = h;
            }
        }

        public readonly double L;
        public readonly double C;
        public readonly double H;

        #region IEquatable<LCH>

        public bool Equals(LCH other)
        {
            return L == other.L && C == other.C && H == other.H;
        }

        #endregion

        #region Equals

        public override bool Equals(object obj)
        {
            if (obj is LCH other)
            {
                return L == other.L && C == other.C && H == other.H;
            }
            else
            {
                return base.Equals(obj);
            }
        }

        public override int GetHashCode()
        {
            return L.GetHashCode() ^ C.GetHashCode() ^ H.GetHashCode();
        }

        #endregion

        #region ToString

        public override string ToString()
        {
            return string.Format("{0},{1},{2}", L, C, H);
        }

        #endregion
    }

    internal readonly struct XYZ : IEquatable<XYZ>
    {
        public const int DefaultRoundingPrecision = 5;

        public XYZ(double x, double y, double z, bool round = true, int roundingPrecision = DefaultRoundingPrecision)
        {
            if (round)
            {
                X = Math.Round(x, roundingPrecision);
                Y = Math.Round(y, roundingPrecision);
                Z = Math.Round(z, roundingPrecision);
            }
            else
            {
                X = x;
                Y = y;
                Z = z;
            }
        }

        public readonly double X;
        public readonly double Y;
        public readonly double Z;

        #region IEquatable<XYZ>

        public bool Equals(XYZ other)
        {
            return X == other.X && Y == other.Y && Z == other.Z;
        }

        #endregion

        #region Equals

        public override bool Equals(object obj)
        {
            if (obj is XYZ other)
            {
                return X == other.X && Y == other.Y && Z == other.Z;
            }
            else
            {
                return base.Equals(obj);
            }
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode();
        }

        #endregion

        #region ToString

        public override string ToString()
        {
            return string.Format("{0},{1},{2}", X, Y, Z);
        }

        #endregion
    }
}
