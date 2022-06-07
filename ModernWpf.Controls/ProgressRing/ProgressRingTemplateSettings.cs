using System.Windows;
using System.Windows.Media;

namespace ModernWpf.Controls.Primitives
{
    public sealed class ProgressRingTemplateSettings : DependencyObject
    {
        internal ProgressRingTemplateSettings()
        {
        }

        // TemplateSetting properties from WUXC for backwards compatibility.

        #region OutlinePath

        private static readonly DependencyPropertyKey OutlinePathPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(OutlinePath),
                typeof(PathGeometry),
                typeof(ProgressRingTemplateSettings),
                null);

        public static readonly DependencyProperty OutlinePathProperty = OutlinePathPropertyKey.DependencyProperty;

        public PathGeometry OutlinePath
        {
            get => (PathGeometry)GetValue(OutlinePathProperty);
            internal set => SetValue(OutlinePathPropertyKey, value);
        }

        #endregion

        #region RingPath

        private static readonly DependencyPropertyKey RingPathPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(RingPath),
                typeof(PathGeometry),
                typeof(ProgressRingTemplateSettings),
                null);

        public static readonly DependencyProperty RingPathProperty = RingPathPropertyKey.DependencyProperty;

        public PathGeometry RingPath
        {
            get => (PathGeometry)GetValue(RingPathProperty);
            internal set => SetValue(RingPathPropertyKey, value);
        }

        #endregion

        #region RingArcIsLargeArc

        private static readonly DependencyPropertyKey RingArcIsLargeArcPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(RingArcIsLargeArc),
                typeof(bool),
                typeof(ProgressRingTemplateSettings),
                null);

        public static readonly DependencyProperty RingArcIsLargeArcProperty = RingArcIsLargeArcPropertyKey.DependencyProperty;

        public bool RingArcIsLargeArc
        {
            get => (bool)GetValue(RingArcIsLargeArcProperty);
            internal set => SetValue(RingArcIsLargeArcPropertyKey, value);
        }

        #endregion

        #region RingArcPoint

        private static readonly DependencyPropertyKey RingArcPointPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(RingArcPoint),
                typeof(Point),
                typeof(ProgressRingTemplateSettings),
                null);

        public static readonly DependencyProperty RingArcPointProperty = RingArcPointPropertyKey.DependencyProperty;

        public Point RingArcPoint
        {
            get => (Point)GetValue(RingArcPointProperty);
            internal set => SetValue(RingArcPointPropertyKey, value);
        }

        #endregion

        #region RingArcSize

        private static readonly DependencyPropertyKey RingArcSizePropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(RingArcSize),
                typeof(Size),
                typeof(ProgressRingTemplateSettings),
                null);

        public static readonly DependencyProperty RingArcSizeProperty = RingArcSizePropertyKey.DependencyProperty;

        public Size RingArcSize
        {
            get => (Size)GetValue(RingArcSizeProperty);
            internal set => SetValue(RingArcSizePropertyKey, value);
        }

        #endregion

        #region OutlineFigureStartPoint

        private static readonly DependencyPropertyKey OutlineFigureStartPointPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(OutlineFigureStartPoint),
                typeof(Point),
                typeof(ProgressRingTemplateSettings),
                null);

        public static readonly DependencyProperty OutlineFigureStartPointProperty = OutlineFigureStartPointPropertyKey.DependencyProperty;

        public Point OutlineFigureStartPoint
        {
            get => (Point)GetValue(OutlineFigureStartPointProperty);
            internal set => SetValue(OutlineFigureStartPointPropertyKey, value);
        }

        #endregion

        #region OutlineArcSize

        private static readonly DependencyPropertyKey OutlineArcSizePropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(OutlineArcSize),
                typeof(Size),
                typeof(ProgressRingTemplateSettings),
                null);

        public static readonly DependencyProperty OutlineArcSizeProperty = OutlineArcSizePropertyKey.DependencyProperty;

        public Size OutlineArcSize
        {
            get => (Size)GetValue(OutlineArcSizeProperty);
            internal set => SetValue(OutlineArcSizePropertyKey, value);
        }

        #endregion

        #region OutlineArcPoint

        private static readonly DependencyPropertyKey OutlineArcPointPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(OutlineArcPoint),
                typeof(Point),
                typeof(ProgressRingTemplateSettings),
                null);

        public static readonly DependencyProperty OutlineArcPointProperty = OutlineArcPointPropertyKey.DependencyProperty;

        public Point OutlineArcPoint
        {
            get => (Point)GetValue(OutlineArcPointProperty);
            internal set => SetValue(OutlineArcPointPropertyKey, value);
        }

        #endregion

        #region RingFigureStartPoint

        private static readonly DependencyPropertyKey RingFigureStartPointPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(RingFigureStartPoint),
                typeof(Point),
                typeof(ProgressRingTemplateSettings),
                null);

        public static readonly DependencyProperty RingFigureStartPointProperty = RingFigureStartPointPropertyKey.DependencyProperty;

        public Point RingFigureStartPoint
        {
            get => (Point)GetValue(RingFigureStartPointProperty);
            internal set => SetValue(RingFigureStartPointPropertyKey, value);
        }

        #endregion

        #region EllipseDiameter

        private static readonly DependencyPropertyKey EllipseDiameterPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(EllipseDiameter),
                typeof(double),
                typeof(ProgressRingTemplateSettings),
                null);

        public static readonly DependencyProperty EllipseDiameterProperty = EllipseDiameterPropertyKey.DependencyProperty;

        public double EllipseDiameter
        {
            get => (double)GetValue(EllipseDiameterProperty);
            internal set => SetValue(EllipseDiameterPropertyKey, value);
        }

        #endregion

        #region EllipseOffset

        private static readonly DependencyPropertyKey EllipseOffsetPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(EllipseOffset),
                typeof(Thickness),
                typeof(ProgressRingTemplateSettings),
                null);

        public static readonly DependencyProperty EllipseOffsetProperty = EllipseOffsetPropertyKey.DependencyProperty;

        public Thickness EllipseOffset
        {
            get => (Thickness)GetValue(EllipseOffsetProperty);
            internal set => SetValue(EllipseOffsetPropertyKey, value);
        }

        #endregion

        #region MaxSideLength

        private static readonly DependencyPropertyKey MaxSideLengthPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(MaxSideLength),
                typeof(double),
                typeof(ProgressRingTemplateSettings),
                null);

        public static readonly DependencyProperty MaxSideLengthProperty = MaxSideLengthPropertyKey.DependencyProperty;

        public double MaxSideLength
        {
            get => (double)GetValue(MaxSideLengthProperty);
            internal set => SetValue(MaxSideLengthPropertyKey, value);
        }

        #endregion
    }
}
