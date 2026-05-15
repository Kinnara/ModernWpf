using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace ModernWpf.Controls.Primitives
{
    public interface IScrollSnapPointsInfo
    {
        bool AreHorizontalSnapPointsRegular { get; }

        bool AreVerticalSnapPointsRegular { get; }

        event EventHandler<object> HorizontalSnapPointsChanged;

        event EventHandler<object> VerticalSnapPointsChanged;

        IReadOnlyList<float> GetIrregularSnapPoints(Orientation orientation, SnapPointsAlignment alignment);

        float GetRegularSnapPoints(Orientation orientation, SnapPointsAlignment alignment, out float offset);
    }
}
