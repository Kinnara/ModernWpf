using System;
using System.Windows;
using System.Windows.Media;

namespace ModernWpf.Controls.Primitives
{
    internal sealed class ProgressRingIndicator : FrameworkElement
    {
        // ProgressRingDeterminate.cpp and ProgressRingIndeterminate.cpp define a
        // 32x32 visual with an 8px ellipse and 1.5px stroke under a 1.77 scale.
        private const double LottieDesignSide = 32.0;
        private const double LottieShapeScale = 1.77;
        private const double LottieEllipseRadius = 8.0;
        private const double LottieStrokeThickness = 1.5;
        private const double DefaultResourceStrokeThickness = 4.0;

        public static readonly DependencyProperty ForegroundProperty =
            DependencyProperty.Register(
                nameof(Foreground),
                typeof(Brush),
                typeof(ProgressRingIndicator),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public Brush Foreground
        {
            get => (Brush)GetValue(ForegroundProperty);
            set => SetValue(ForegroundProperty, value);
        }

        public static readonly DependencyProperty BackgroundProperty =
            DependencyProperty.Register(
                nameof(Background),
                typeof(Brush),
                typeof(ProgressRingIndicator),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public Brush Background
        {
            get => (Brush)GetValue(BackgroundProperty);
            set => SetValue(BackgroundProperty, value);
        }

        public static readonly DependencyProperty StrokeThicknessProperty =
            DependencyProperty.Register(
                nameof(StrokeThickness),
                typeof(double),
                typeof(ProgressRingIndicator),
                new FrameworkPropertyMetadata(4.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public double StrokeThickness
        {
            get => (double)GetValue(StrokeThicknessProperty);
            set => SetValue(StrokeThicknessProperty, value);
        }

        public static readonly DependencyProperty IsIndeterminateProperty =
            DependencyProperty.Register(
                nameof(IsIndeterminate),
                typeof(bool),
                typeof(ProgressRingIndicator),
                new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

        public bool IsIndeterminate
        {
            get => (bool)GetValue(IsIndeterminateProperty);
            set => SetValue(IsIndeterminateProperty, value);
        }

        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register(
                nameof(Minimum),
                typeof(double),
                typeof(ProgressRingIndicator),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public double Minimum
        {
            get => (double)GetValue(MinimumProperty);
            set => SetValue(MinimumProperty, value);
        }

        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register(
                nameof(Maximum),
                typeof(double),
                typeof(ProgressRingIndicator),
                new FrameworkPropertyMetadata(100.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public double Maximum
        {
            get => (double)GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                nameof(Value),
                typeof(double),
                typeof(ProgressRingIndicator),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public double Value
        {
            get => (double)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public static readonly DependencyProperty IndeterminateStartAngleProperty =
            DependencyProperty.Register(
                nameof(IndeterminateStartAngle),
                typeof(double),
                typeof(ProgressRingIndicator),
                new FrameworkPropertyMetadata(305.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public double IndeterminateStartAngle
        {
            get => (double)GetValue(IndeterminateStartAngleProperty);
            set => SetValue(IndeterminateStartAngleProperty, value);
        }

        public static readonly DependencyProperty IndeterminateSweepAngleProperty =
            DependencyProperty.Register(
                nameof(IndeterminateSweepAngle),
                typeof(double),
                typeof(ProgressRingIndicator),
                new FrameworkPropertyMetadata(160.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public double IndeterminateSweepAngle
        {
            get => (double)GetValue(IndeterminateSweepAngleProperty);
            set => SetValue(IndeterminateSweepAngleProperty, value);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            var side = Math.Min(ActualWidth, ActualHeight);
            if (side <= 0)
            {
                return;
            }

            var strokeScale = Math.Max(0.0, StrokeThickness) / DefaultResourceStrokeThickness;
            var strokeThickness = side * LottieStrokeThickness * LottieShapeScale / LottieDesignSide * strokeScale;
            var radius = side * LottieEllipseRadius * LottieShapeScale / LottieDesignSide;
            if (strokeThickness <= 0.0 || radius <= 0.0)
            {
                return;
            }

            var center = new Point(ActualWidth / 2.0, ActualHeight / 2.0);
            var backgroundPen = CreatePen(Background, strokeThickness);
            if (backgroundPen != null)
            {
                drawingContext.DrawEllipse(null, backgroundPen, center, radius, radius);
            }

            var foregroundPen = CreatePen(Foreground, strokeThickness);
            if (foregroundPen == null)
            {
                return;
            }

            if (IsIndeterminate)
            {
                DrawArc(drawingContext, foregroundPen, center, radius, IndeterminateStartAngle, IndeterminateSweepAngle);
                return;
            }

            var range = Maximum - Minimum;
            if (range <= 0 || double.IsNaN(range))
            {
                return;
            }

            var progress = (Value - Minimum) / range;
            if (double.IsNaN(progress))
            {
                return;
            }

            progress = Math.Max(0.0, Math.Min(1.0, progress));
            if (progress <= 0.0)
            {
                return;
            }

            DrawArc(drawingContext, foregroundPen, center, radius, -90.0, 359.9 * progress);
        }

        private static Pen CreatePen(Brush brush, double strokeThickness)
        {
            if (brush == null || IsTransparent(brush))
            {
                return null;
            }

            return new Pen(brush, strokeThickness)
            {
                StartLineCap = PenLineCap.Round,
                EndLineCap = PenLineCap.Round
            };
        }

        private static bool IsTransparent(Brush brush)
        {
            var solid = brush as SolidColorBrush;
            return brush.Opacity <= 0 || solid?.Color.A == 0;
        }

        private static void DrawArc(DrawingContext drawingContext, Pen pen, Point center, double radius, double startAngle, double sweepAngle)
        {
            if (Math.Abs(sweepAngle) >= 359.9)
            {
                drawingContext.DrawEllipse(null, pen, center, radius, radius);
                return;
            }

            var start = PointOnCircle(center, radius, startAngle);
            var end = PointOnCircle(center, radius, startAngle + sweepAngle);
            var geometry = new StreamGeometry();
            using (var context = geometry.Open())
            {
                context.BeginFigure(start, false, false);
                context.ArcTo(
                    end,
                    new Size(radius, radius),
                    0.0,
                    Math.Abs(sweepAngle) > 180.0,
                    sweepAngle >= 0.0 ? SweepDirection.Clockwise : SweepDirection.Counterclockwise,
                    true,
                    false);
            }
            geometry.Freeze();

            drawingContext.DrawGeometry(null, pen, geometry);
        }

        private static Point PointOnCircle(Point center, double radius, double angle)
        {
            var radians = angle * Math.PI / 180.0;
            return new Point(
                center.X + radius * Math.Cos(radians),
                center.Y + radius * Math.Sin(radians));
        }
    }
}
