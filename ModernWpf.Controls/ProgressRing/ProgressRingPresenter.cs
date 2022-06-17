using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    public class ProgressRingPresenter : Control
    {
        static ProgressRingPresenter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ProgressRingPresenter), new FrameworkPropertyMetadata(typeof(ProgressRingPresenter)));
        }

        public ProgressRingPresenter()
        {
            SetValue(TemplateSettingsPropertyKey, new ProgressRingPresenterTemplateSettings());

            SizeChanged += OnSizeChanged;
        }

        #region TemplateSettings

        private static readonly DependencyPropertyKey TemplateSettingsPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(TemplateSettings),
                typeof(ProgressRingPresenterTemplateSettings),
                typeof(ProgressRingPresenter),
                null);

        public static readonly DependencyProperty TemplateSettingsProperty =
            TemplateSettingsPropertyKey.DependencyProperty;

        public ProgressRingPresenterTemplateSettings TemplateSettings
        {
            get => (ProgressRingPresenterTemplateSettings)GetValue(TemplateSettingsProperty);
        }

        #endregion

        #region Value

        public double Value
        {
            get => (double)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                nameof(Value),
                typeof(double),
                typeof(ProgressRingPresenter),
                new FrameworkPropertyMetadata(OnValuePropertyChanged));

        private static void OnValuePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((ProgressRingPresenter)sender).OnRangeBasePropertyChanged(args);
        }

        #endregion

        #region StrokeThickness

        public double StrokeThickness
        {
            get => (double)GetValue(StrokeThicknessProperty);
            set => SetValue(StrokeThicknessProperty, value);
        }

        public static readonly DependencyProperty StrokeThicknessProperty =
            DependencyProperty.Register(
                nameof(StrokeThickness),
                typeof(double),
                typeof(ProgressRingPresenter),
                new FrameworkPropertyMetadata(OnStrokeThicknessPropertyChanged));

        private static void OnStrokeThicknessPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((ProgressRingPresenter)sender).OnStrokeThicknessPropertyChanged(args);
        }

        #endregion

        public override void OnApplyTemplate()
        {
            ApplyPathGeometry();

            base.OnApplyTemplate();
        }

        void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateRing();
        }

        void OnRangeBasePropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            UpdateSegment();
        }

        void OnStrokeThicknessPropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            UpdateRing();
        }

        void ApplyPathGeometry()
        {
            // TemplateSetting properties from WUXC for backwards compatibility.
            var templateSettings = TemplateSettings;

            ArcSegment OutlineArcPart = new ArcSegment
            {
                IsLargeArc = true,
                SweepDirection = SweepDirection.Clockwise
            };

            BindingOperations.SetBinding(OutlineArcPart, ArcSegment.PointProperty, new Binding
            {
                Source = templateSettings,
                Path = new PropertyPath(nameof(TemplateSettings.OutlineArcPoint))
            });

            BindingOperations.SetBinding(OutlineArcPart, ArcSegment.SizeProperty, new Binding
            {
                Source = templateSettings,
                Path = new PropertyPath(nameof(TemplateSettings.OutlineArcSize))
            });

            PathFigure OutlineFigurePart = new PathFigure
            {
                Segments = new PathSegmentCollection
                {
                    OutlineArcPart
                }
            };

            BindingOperations.SetBinding(OutlineFigurePart, PathFigure.StartPointProperty, new Binding
            {
                Source = templateSettings,
                Path = new PropertyPath(nameof(TemplateSettings.OutlineFigureStartPoint))
            });

            PathGeometry OutlinePath = new PathGeometry
            {
                Figures = new PathFigureCollection
                {
                    OutlineFigurePart
                }
            };

            templateSettings.OutlinePath = OutlinePath;

            ArcSegment RingArcPart = new ArcSegment
            {
                IsLargeArc = true,
                SweepDirection = SweepDirection.Clockwise
            };

            BindingOperations.SetBinding(RingArcPart, ArcSegment.IsLargeArcProperty, new Binding
            {
                Source = templateSettings,
                Path = new PropertyPath(nameof(TemplateSettings.RingArcIsLargeArc))
            });

            BindingOperations.SetBinding(RingArcPart, ArcSegment.PointProperty, new Binding
            {
                Source = templateSettings,
                Path = new PropertyPath(nameof(TemplateSettings.RingArcPoint))
            });

            BindingOperations.SetBinding(RingArcPart, ArcSegment.SizeProperty, new Binding
            {
                Source = templateSettings,
                Path = new PropertyPath(nameof(TemplateSettings.RingArcSize))
            });

            PathFigure RingFigurePart = new PathFigure
            {
                Segments = new PathSegmentCollection
                {
                    RingArcPart
                }
            };

            BindingOperations.SetBinding(RingFigurePart, PathFigure.StartPointProperty, new Binding
            {
                Source = templateSettings,
                Path = new PropertyPath(nameof(TemplateSettings.RingFigureStartPoint))
            });

            PathGeometry RingPath = new PathGeometry
            {
                Figures = new PathFigureCollection
                {
                    RingFigurePart
                }
            };

            templateSettings.RingPath = RingPath;
        }

        Size ComputeEllipseSize(double thickness, double actualWidth, double actualHeight)
        {
            double safeThickness = Math.Max(thickness, (double)0.0);
            double width = Math.Max((actualWidth - safeThickness) / 2.0, 0.0);
            double height = Math.Max((actualHeight - safeThickness) / 2.0, 0.0);

            return new Size(width, height);
        }

        void UpdateSegment()
        {
            var templateSettings = TemplateSettings;

            double angle()
            {
                double normalizedRange()
                {
                    // normalizedRange offsets calculation to display a full ring when value = 100%
                    // std::nextafter is set as a float as winrt::Point takes floats
                    return Math.Min(Value, 0.999999940395355224609375);
                }

                return 2 * Math.PI * normalizedRange();
            }

            double thickness = StrokeThickness;
            var size = ComputeEllipseSize(thickness, ActualWidth, ActualHeight);
            double translationFactor = Math.Max(thickness / 2.0, 0.0);

            double x = (Math.Sin(angle()) * size.Width) + size.Width + translationFactor;
            double y = (((Math.Cos(angle()) * size.Height) - size.Height) * -1) + translationFactor;

            templateSettings.RingArcIsLargeArc = angle() >= Math.PI;
            templateSettings.RingArcPoint = new Point(x, y);
        }

        void UpdateRing()
        {
            var templateSettings = TemplateSettings;

            double thickness = StrokeThickness;
            var size = ComputeEllipseSize(thickness, ActualWidth, ActualHeight);

            double segmentWidth = size.Width;
            double translationFactor = Math.Max(thickness / 2.0, 0.0);

            templateSettings.OutlineFigureStartPoint = new Point(segmentWidth + translationFactor, translationFactor);

            templateSettings.RingFigureStartPoint = new Point(segmentWidth + translationFactor, translationFactor);

            templateSettings.OutlineArcSize = new Size(segmentWidth, size.Height);
            templateSettings.OutlineArcPoint = new Point(segmentWidth + translationFactor - 0.05d, translationFactor);

            templateSettings.RingArcSize = new Size(segmentWidth, size.Height);

            UpdateSegment();
        }
    }
}
