using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ModernWpf.Controls
{
    internal static class LayoutChromeHelper
    {
        public static void DrawChrome(
            DrawingContext drawingContext,
            Size renderSize,
            Brush background,
            BackgroundSizing backgroundSizing,
            Brush borderBrush,
            Thickness borderThickness,
            CornerRadius cornerRadius)
        {
            if (renderSize.Width <= 0 || renderSize.Height <= 0)
            {
                return;
            }

            var hasBackground = IsVisibleBrush(background);
            var hasBorder = IsVisibleBrush(borderBrush) && !IsZero(borderThickness);
            if (!hasBackground && !hasBorder)
            {
                return;
            }

            var outerRect = new Rect(renderSize);
            var innerRect = Deflate(outerRect, borderThickness);

            if (innerRect.Width > 0 && innerRect.Height > 0)
            {
                Geometry outerGeometry = null;
                Geometry innerGeometry = null;

                if (hasBorder || (hasBackground && backgroundSizing == BackgroundSizing.OuterBorderEdge))
                {
                    outerGeometry = CreateRoundedRectangleGeometry(outerRect, cornerRadius, borderThickness, true);
                }

                if (hasBorder || (hasBackground && backgroundSizing == BackgroundSizing.InnerBorderEdge))
                {
                    innerGeometry = CreateRoundedRectangleGeometry(innerRect, cornerRadius, borderThickness, false);
                }

                if (hasBackground)
                {
                    var backgroundGeometry = backgroundSizing == BackgroundSizing.OuterBorderEdge ? outerGeometry : innerGeometry;
                    if (backgroundGeometry != null)
                    {
                        drawingContext.DrawGeometry(background, null, backgroundGeometry);
                    }
                }

                if (hasBorder && outerGeometry != null && innerGeometry != null)
                {
                    var borderGeometry = new CombinedGeometry(GeometryCombineMode.Exclude, outerGeometry, innerGeometry);
                    borderGeometry.Freeze();
                    drawingContext.DrawGeometry(borderBrush, null, borderGeometry);
                }
            }
            else
            {
                var outerGeometry = CreateRoundedRectangleGeometry(outerRect, cornerRadius, borderThickness, true);

                if (hasBackground && backgroundSizing == BackgroundSizing.OuterBorderEdge)
                {
                    drawingContext.DrawGeometry(background, null, outerGeometry);
                }

                if (hasBorder)
                {
                    drawingContext.DrawGeometry(borderBrush, null, outerGeometry);
                }
            }
        }

        private static bool IsVisibleBrush(Brush brush)
        {
            if (brush == null || brush.Opacity <= 0)
            {
                return false;
            }

            return !(brush is SolidColorBrush solidColorBrush && solidColorBrush.Color.A == 0);
        }

        public static Geometry CreateRoundedRectangleGeometry(Rect rect, CornerRadius cornerRadius)
        {
            return CreateRoundedRectangleGeometry(rect, cornerRadius, new Thickness(), false);
        }

        public static Geometry CreateRoundedLayoutClip(Size layoutSlotSize, CornerRadius cornerRadius, Geometry baseClip)
        {
            Geometry roundedClip = null;
            if (HasNonZeroCornerRadius(cornerRadius) && layoutSlotSize.Width > 0 && layoutSlotSize.Height > 0)
            {
                roundedClip = CreateRoundedRectangleGeometry(new Rect(layoutSlotSize), cornerRadius);
            }

            if (roundedClip == null)
            {
                return baseClip;
            }

            if (baseClip == null)
            {
                return roundedClip;
            }

            var combinedClip = new CombinedGeometry(GeometryCombineMode.Intersect, baseClip, roundedClip);
            if (combinedClip.CanFreeze)
            {
                combinedClip.Freeze();
            }

            return combinedClip;
        }

        public static Geometry CreateRoundedRectangleGeometry(Rect rect, CornerRadius cornerRadius, Thickness borderThickness, bool isOuter)
        {
            var points = CalculateRoundedCornersRectangle(rect, cornerRadius, borderThickness, isOuter);
            var geometry = new StreamGeometry();

            using (var context = geometry.Open())
            {
                context.BeginFigure(points.TopRight, true, true);

                AddArc(context, points.RightTop, points.TopRight, points.RightTop);

                context.LineTo(points.RightBottom, true, false);
                AddArc(context, points.BottomRight, points.RightBottom, points.BottomRight);

                context.LineTo(points.BottomLeft, true, false);
                AddArc(context, points.LeftBottom, points.BottomLeft, points.LeftBottom);

                context.LineTo(points.LeftTop, true, false);
                AddArc(context, points.TopLeft, points.LeftTop, points.TopLeft);
            }

            geometry.Freeze();
            return geometry;
        }

        public static bool FillContainsRoundedRectangle(Size renderSize, CornerRadius cornerRadius, Point point)
        {
            if (renderSize.Width <= 0 || renderSize.Height <= 0)
            {
                return false;
            }

            return CreateRoundedRectangleGeometry(new Rect(renderSize), cornerRadius).FillContains(point);
        }

        public static bool HasNonZeroCornerRadius(CornerRadius cornerRadius)
        {
            return cornerRadius.TopLeft != 0 ||
                cornerRadius.TopRight != 0 ||
                cornerRadius.BottomRight != 0 ||
                cornerRadius.BottomLeft != 0;
        }

        public static Rect Deflate(Rect rect, Thickness thickness)
        {
            return new Rect(
                rect.Left + thickness.Left,
                rect.Top + thickness.Top,
                Math.Max(0, rect.Width - thickness.Left - thickness.Right),
                Math.Max(0, rect.Height - thickness.Top - thickness.Bottom));
        }

        public static Size Deflate(Size size, Thickness thickness)
        {
            return new Size(
                Math.Max(0, size.Width - thickness.Left - thickness.Right),
                Math.Max(0, size.Height - thickness.Top - thickness.Bottom));
        }

        public static Size Inflate(Size size, Thickness thickness)
        {
            return new Size(
                size.Width + thickness.Left + thickness.Right,
                size.Height + thickness.Top + thickness.Bottom);
        }

        public static Thickness Add(Thickness first, Thickness second)
        {
            return new Thickness(
                first.Left + second.Left,
                first.Top + second.Top,
                first.Right + second.Right,
                first.Bottom + second.Bottom);
        }

        private static void AddArc(StreamGeometryContext context, Point point, Point start, Point end)
        {
            var size = new Size(Math.Abs(end.X - start.X), Math.Abs(end.Y - start.Y));

            if (size.Width > 0 && size.Height > 0)
            {
                context.ArcTo(point, size, 0, false, SweepDirection.Clockwise, true, false);
            }
            else
            {
                context.LineTo(point, true, false);
            }
        }

        private static RoundedRectanglePoints CalculateRoundedCornersRectangle(Rect rect, CornerRadius cornerRadius, Thickness borderThickness, bool isOuter)
        {
            var left = 0.5 * borderThickness.Left;
            var top = 0.5 * borderThickness.Top;
            var right = 0.5 * borderThickness.Right;
            var bottom = 0.5 * borderThickness.Bottom;

            double leftTop;
            double topLeft;
            double topRight;
            double rightTop;
            double rightBottom;
            double bottomRight;
            double bottomLeft;
            double leftBottom;

            if (isOuter)
            {
                if (IsCloseToZero(cornerRadius.TopLeft))
                {
                    leftTop = 0;
                    topLeft = 0;
                }
                else
                {
                    leftTop = cornerRadius.TopLeft + left;
                    topLeft = cornerRadius.TopLeft + top;
                }

                if (IsCloseToZero(cornerRadius.TopRight))
                {
                    topRight = 0;
                    rightTop = 0;
                }
                else
                {
                    topRight = cornerRadius.TopRight + top;
                    rightTop = cornerRadius.TopRight + right;
                }

                if (IsCloseToZero(cornerRadius.BottomRight))
                {
                    rightBottom = 0;
                    bottomRight = 0;
                }
                else
                {
                    rightBottom = cornerRadius.BottomRight + right;
                    bottomRight = cornerRadius.BottomRight + bottom;
                }

                if (IsCloseToZero(cornerRadius.BottomLeft))
                {
                    bottomLeft = 0;
                    leftBottom = 0;
                }
                else
                {
                    bottomLeft = cornerRadius.BottomLeft + bottom;
                    leftBottom = cornerRadius.BottomLeft + left;
                }
            }
            else
            {
                leftTop = Math.Max(0, cornerRadius.TopLeft - left);
                topLeft = Math.Max(0, cornerRadius.TopLeft - top);
                topRight = Math.Max(0, cornerRadius.TopRight - top);
                rightTop = Math.Max(0, cornerRadius.TopRight - right);
                rightBottom = Math.Max(0, cornerRadius.BottomRight - right);
                bottomRight = Math.Max(0, cornerRadius.BottomRight - bottom);
                bottomLeft = Math.Max(0, cornerRadius.BottomLeft - bottom);
                leftBottom = Math.Max(0, cornerRadius.BottomLeft - left);
            }

            var points = new RoundedRectanglePoints
            {
                TopLeft = new Point(leftTop, 0),
                TopRight = new Point(rect.Width - rightTop, 0),
                LeftTop = new Point(0, topLeft),
                LeftBottom = new Point(0, rect.Height - bottomLeft),
                RightTop = new Point(rect.Width, topRight),
                RightBottom = new Point(rect.Width, rect.Height - bottomRight),
                BottomLeft = new Point(leftBottom, rect.Height),
                BottomRight = new Point(rect.Width - rightBottom, rect.Height),
            };

            if (points.TopLeft.X > points.TopRight.X)
            {
                var x = leftTop / (leftTop + rightTop) * rect.Width;
                points.TopLeft.X = x;
                points.TopRight.X = x;
            }

            if (points.RightTop.Y > points.RightBottom.Y)
            {
                var y = topRight / (topRight + bottomRight) * rect.Height;
                points.RightTop.Y = y;
                points.RightBottom.Y = y;
            }

            if (points.BottomRight.X < points.BottomLeft.X)
            {
                var x = leftBottom / (leftBottom + rightBottom) * rect.Width;
                points.BottomRight.X = x;
                points.BottomLeft.X = x;
            }

            if (points.LeftBottom.Y < points.LeftTop.Y)
            {
                var y = topLeft / (topLeft + bottomLeft) * rect.Height;
                points.LeftBottom.Y = y;
                points.LeftTop.Y = y;
            }

            points.Offset(rect.Left, rect.Top);

            return points;
        }

        private static bool IsCloseToZero(double value)
        {
            return Math.Abs(value) < 0.000001;
        }

        private static bool IsZero(Thickness thickness)
        {
            return thickness.Left == 0 && thickness.Top == 0 && thickness.Right == 0 && thickness.Bottom == 0;
        }

        private struct RoundedRectanglePoints
        {
            public Point TopLeft;
            public Point TopRight;
            public Point LeftTop;
            public Point LeftBottom;
            public Point RightTop;
            public Point RightBottom;
            public Point BottomLeft;
            public Point BottomRight;

            public void Offset(double offsetX, double offsetY)
            {
                TopLeft.Offset(offsetX, offsetY);
                TopRight.Offset(offsetX, offsetY);
                LeftTop.Offset(offsetX, offsetY);
                LeftBottom.Offset(offsetX, offsetY);
                RightTop.Offset(offsetX, offsetY);
                RightBottom.Offset(offsetX, offsetY);
                BottomLeft.Offset(offsetX, offsetY);
                BottomRight.Offset(offsetX, offsetY);
            }
        }
    }

    internal sealed class LayoutChromeDecorator : Decorator
    {
        public Brush Background
        {
            get => _background;
            set
            {
                if (!ReferenceEquals(_background, value))
                {
                    var oldValue = _background;
                    _background = value;
                    if (BackgroundTransition != null || _backgroundTransitionHelper?.IsTransitioning == true)
                    {
                        BackgroundTransitionHelper.OnBrushChanged(oldValue, value, BackgroundTransition);
                    }
                    else
                    {
                        InvalidateVisual();
                    }
                }
            }
        }

        public BrushTransition BackgroundTransition
        {
            get => _backgroundTransition;
            set
            {
                if (!ReferenceEquals(_backgroundTransition, value))
                {
                    _backgroundTransition = value;
                    _backgroundTransitionHelper?.OnTransitionChanged(value);
                }
            }
        }

        public BackgroundSizing BackgroundSizing
        {
            get => _backgroundSizing;
            set
            {
                if (_backgroundSizing != value)
                {
                    _backgroundSizing = value;
                    InvalidateVisual();
                }
            }
        }

        public Brush BorderBrush
        {
            get => _borderBrush;
            set
            {
                if (!Equals(_borderBrush, value))
                {
                    _borderBrush = value;
                    InvalidateVisual();
                }
            }
        }

        public Thickness BorderThickness
        {
            get => _borderThickness;
            set
            {
                if (!_borderThickness.Equals(value))
                {
                    _borderThickness = value;
                    InvalidateMeasure();
                    InvalidateVisual();
                }
            }
        }

        public CornerRadius CornerRadius
        {
            get => _cornerRadius;
            set
            {
                if (!_cornerRadius.Equals(value))
                {
                    _cornerRadius = value;
                    InvalidateArrange();
                    InvalidateVisual();
                }
            }
        }

        public Thickness Padding
        {
            get => _padding;
            set
            {
                if (!_padding.Equals(value))
                {
                    _padding = value;
                    InvalidateMeasure();
                    InvalidateVisual();
                }
            }
        }

        protected override Size MeasureOverride(Size constraint)
        {
            var child = Child;
            var chrome = GetChromeThickness();
            if (child == null)
            {
                return LayoutChromeHelper.Inflate(new Size(), chrome);
            }

            child.Measure(LayoutChromeHelper.Deflate(constraint, chrome));
            return LayoutChromeHelper.Inflate(child.DesiredSize, chrome);
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            var child = Child;
            if (child != null)
            {
                var chrome = GetChromeThickness();
                child.Arrange(new Rect(
                    chrome.Left,
                    chrome.Top,
                    Math.Max(0, arrangeSize.Width - chrome.Left - chrome.Right),
                    Math.Max(0, arrangeSize.Height - chrome.Top - chrome.Bottom)));
            }

            return arrangeSize;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            LayoutChromeHelper.DrawChrome(
                drawingContext,
                RenderSize,
                EffectiveBackground,
                BackgroundSizing,
                BorderBrush,
                BorderThickness,
                CornerRadius);
        }

        protected override Geometry GetLayoutClip(Size layoutSlotSize)
        {
            return LayoutChromeHelper.CreateRoundedLayoutClip(
                layoutSlotSize,
                CornerRadius,
                base.GetLayoutClip(layoutSlotSize));
        }

        protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
        {
            if (!LayoutChromeHelper.FillContainsRoundedRectangle(RenderSize, CornerRadius, hitTestParameters.HitPoint))
            {
                return null;
            }

            return base.HitTestCore(hitTestParameters);
        }

        private Thickness GetChromeThickness()
        {
            return LayoutChromeHelper.Add(BorderThickness, Padding);
        }

        internal Brush EffectiveBackground => _backgroundTransitionHelper?.GetEffectiveBrush(Background) ?? Background;

        private BrushTransitionHelper BackgroundTransitionHelper =>
            _backgroundTransitionHelper ?? (_backgroundTransitionHelper = new BrushTransitionHelper(InvalidateVisual));

        private Brush _background;
        private BrushTransition _backgroundTransition;
        private BrushTransitionHelper _backgroundTransitionHelper;
        private BackgroundSizing _backgroundSizing = BackgroundSizing.InnerBorderEdge;
        private Brush _borderBrush;
        private Thickness _borderThickness;
        private CornerRadius _cornerRadius;
        private Thickness _padding;
    }
}
