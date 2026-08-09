// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;

namespace ModernWpf.Controls
{
    internal sealed class LinedFlowLayoutState
    {
        internal void InitializeForContext(VirtualizingLayoutContext context, IFlowLayoutAlgorithmDelegates callbacks)
        {
            FlowAlgorithm.InitializeForContext(context, callbacks);
            context.LayoutState = this;
        }

        internal void UninitializeForContext(VirtualizingLayoutContext context)
        {
            FlowAlgorithm.UninitializeForContext(context);
        }

        internal void ClearItemsInfo()
        {
            AspectRatios.Clear();
            MinWidths.Clear();
            MaxWidths.Clear();
            UniformMinWidth = -1.0;
            UniformMaxWidth = -1.0;
            InfoStartIndex = -1;
            InfoLength = 0;
        }

        internal FlowLayoutAlgorithm FlowAlgorithm { get; } = new FlowLayoutAlgorithm();

        internal Dictionary<int, double> AspectRatios { get; } = new Dictionary<int, double>();

        internal Dictionary<int, double> MeasuredAspectRatios { get; } = new Dictionary<int, double>();

        internal Dictionary<int, double> MinWidths { get; } = new Dictionary<int, double>();

        internal Dictionary<int, double> MaxWidths { get; } = new Dictionary<int, double>();

        internal Dictionary<int, int> LockedLines { get; } = new Dictionary<int, int>();

        internal double[] PlannedWidths { get; set; } = new double[0];

        internal int[] LineIndices { get; set; } = new int[0];

        internal List<int> LineStartIndices { get; } = new List<int>();

        internal double UniformMinWidth { get; set; } = -1.0;

        internal double UniformMaxWidth { get; set; } = -1.0;

        internal double AvailableWidth { get; set; }

        internal double TotalHeight { get; set; }

        internal int InfoStartIndex { get; set; } = -1;

        internal int InfoLength { get; set; }

        internal bool NeedsRemeasure { get; set; }
    }
}
