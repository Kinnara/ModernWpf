// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;

namespace ModernWpf.Controls
{
    public class StackLayoutState
    {
        internal void InitializeForContext(
            VirtualizingLayoutContext context,
            IFlowLayoutAlgorithmDelegates callbacks)
        {
            FlowAlgorithm.InitializeForContext(context, callbacks);
            if (m_estimationBuffer.Count == 0)
            {
                m_estimationBuffer.Resize(BufferSize, 0.0);
            }

            ((ILayoutContextOverrides)context).LayoutStateCore = this;
        }

        internal void UninitializeForContext(VirtualizingLayoutContext context)
        {
            FlowAlgorithm.UninitializeForContext(context);
        }

        internal void OnElementMeasured(int elementIndex, double majorSize, double minorSize)
        {
            int estimationBufferIndex = elementIndex % m_estimationBuffer.Count;
            bool alreadyMeasured = m_estimationBuffer[estimationBufferIndex] != 0;
            if (!alreadyMeasured)
            {
                TotalElementsMeasured++;
            }

            TotalElementSize -= m_estimationBuffer[estimationBufferIndex];
            TotalElementSize += majorSize;
            m_estimationBuffer[estimationBufferIndex] = majorSize;

            MaxArrangeBounds = Math.Max(MaxArrangeBounds, minorSize);
        }

        internal void OnMeasureStart()
        {
            MaxArrangeBounds = 0.0;
        }

        internal FlowLayoutAlgorithm FlowAlgorithm { get; } = new FlowLayoutAlgorithm();
        internal double TotalElementSize { get; private set; }
        // During the measure pass, as we measure the elements, we will keep track
        // of the largest arrange bounds in the non-virtualizing direction. This value
        // is going to be used in the calculation of the extent.
        internal double MaxArrangeBounds { get; private set; }
        internal int TotalElementsMeasured { get; private set; }

        private readonly List<double> m_estimationBuffer = new List<double>();
        private const int BufferSize = 100;
    }
}
