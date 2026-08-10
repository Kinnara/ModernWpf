// Copyright (c) Kinnara. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Windows;

namespace ModernWpf.Controls
{
    internal sealed class LinedFlowLayoutState
    {
        internal List<LinedFlowLayoutLine> Lines { get; } = new List<LinedFlowLayoutLine>();

        internal Dictionary<int, UIElement> RealizedElements { get; } = new Dictionary<int, UIElement>();

        internal Dictionary<int, Rect> ArrangeBounds { get; } = new Dictionary<int, Rect>();

        internal Dictionary<int, double> AspectRatios { get; } = new Dictionary<int, double>();

        internal Dictionary<int, double> MinWidths { get; } = new Dictionary<int, double>();

        internal Dictionary<int, double> MaxWidths { get; } = new Dictionary<int, double>();

        internal Dictionary<int, int> ItemToLine { get; } = new Dictionary<int, int>();

        internal double AverageAspectRatio { get; set; } = 1.0;

        internal double AverageItemsPerLine { get; set; }

        internal double MeasuredWidth { get; set; }

        internal int ItemCount { get; set; }

        internal void ClearLayout()
        {
            Lines.Clear();
            ArrangeBounds.Clear();
            ItemToLine.Clear();
            RealizedElements.Clear();
        }

        internal void ClearItemsInfo()
        {
            AspectRatios.Clear();
            MinWidths.Clear();
            MaxWidths.Clear();
            AverageAspectRatio = 1.0;
        }
    }

    internal sealed class LinedFlowLayoutLine
    {
        internal List<int> ItemIndexes { get; } = new List<int>();

        internal List<double> ItemWidths { get; } = new List<double>();

        internal double Y { get; set; }

        internal double DesiredWidth { get; set; }
    }
}
