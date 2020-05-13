// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace MS.Internal
{
    internal static class DoubleUtil
    {
        public static int DoubleToInt(double val)
        {
            return (0 < val) ? (int)(val + 0.5) : (int)(val - 0.5);
        }
    }
}
