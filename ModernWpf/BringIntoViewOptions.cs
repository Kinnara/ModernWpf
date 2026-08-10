using System;
using System.Windows;

namespace ModernWpf
{
    public sealed class BringIntoViewOptions
    {
        private double _horizontalAlignmentRatio = double.NaN;
        private double _verticalAlignmentRatio = double.NaN;
        private double _horizontalOffset;
        private double _verticalOffset;

        public bool AnimationDesired { get; set; }

        public Rect? TargetRect { get; set; }

        public double HorizontalAlignmentRatio
        {
            get => _horizontalAlignmentRatio;
            set => _horizontalAlignmentRatio = ClampAlignmentRatio(value);
        }

        public double VerticalAlignmentRatio
        {
            get => _verticalAlignmentRatio;
            set => _verticalAlignmentRatio = ClampAlignmentRatio(value);
        }

        public double HorizontalOffset
        {
            get => _horizontalOffset;
            set => _horizontalOffset = ValidateOffset(value, nameof(value));
        }

        public double VerticalOffset
        {
            get => _verticalOffset;
            set => _verticalOffset = ValidateOffset(value, nameof(value));
        }

        private static double ClampAlignmentRatio(double value)
        {
            if (double.IsNaN(value))
            {
                return value;
            }

            return Math.Max(0.0, Math.Min(1.0, value));
        }

        private static double ValidateOffset(double value, string parameterName)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                throw new ArgumentException("The offset must be finite.", parameterName);
            }

            return value;
        }
    }
}
