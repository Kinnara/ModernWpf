using System;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using ModernWpf.Automation.Peers;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    public class ProgressRing : Control
    {
        const string s_ActiveStateName = "Active";
        const string s_InactiveStateName = "Inactive";
        const string s_SmallStateName = "Small";
        const string s_LargeStateName = "Large";

        static ProgressRing()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ProgressRing), new FrameworkPropertyMetadata(typeof(ProgressRing)));
        }

        public ProgressRing()
        {
            SetValue(TemplateSettingsPropertyKey, new ProgressRingTemplateSettings());

            SizeChanged += OnSizeChanged;
        }

        #region IsActive

        public bool IsActive
        {
            get => (bool)GetValue(IsActiveProperty);
            set => SetValue(IsActiveProperty, value);
        }

        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register(
                nameof(IsActive),
                typeof(bool),
                typeof(ProgressRing),
                new PropertyMetadata(true, OnIsActivePropertyChanged));

        private static void OnIsActivePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((ProgressRing)sender).OnIsActivePropertyChanged(args);
        }

        #endregion

        #region IsIndeterminate

        public bool IsIndeterminate
        {
            get => (bool)GetValue(IsIndeterminateProperty);
            set => SetValue(IsIndeterminateProperty, value);
        }

        public static readonly DependencyProperty IsIndeterminateProperty =
            DependencyProperty.Register(
                nameof(IsIndeterminate),
                typeof(bool),
                typeof(ProgressRing),
                new PropertyMetadata(true));

        #endregion

        #region Maximum

        public double Maximum
        {
            get => (double)GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }

        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register(
                nameof(Maximum),
                typeof(double),
                typeof(ProgressRing),
                new FrameworkPropertyMetadata(OnMaximumPropertyChanged));

        private static void OnMaximumPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((ProgressRing)sender).OnRangeBasePropertyChanged(args);
        }

        #endregion

        #region Minimum

        public double Minimum
        {
            get => (double)GetValue(MinimumProperty);
            set => SetValue(MinimumProperty, value);
        }

        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register(
                nameof(Minimum),
                typeof(double),
                typeof(ProgressRing),
                new FrameworkPropertyMetadata(OnMinimumPropertyChanged));

        private static void OnMinimumPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((ProgressRing)sender).OnRangeBasePropertyChanged(args);
        }

        #endregion

        #region TemplateSettings

        private static readonly DependencyPropertyKey TemplateSettingsPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(TemplateSettings),
                typeof(ProgressRingTemplateSettings),
                typeof(ProgressRing),
                null);

        public static readonly DependencyProperty TemplateSettingsProperty =
            TemplateSettingsPropertyKey.DependencyProperty;

        public ProgressRingTemplateSettings TemplateSettings
        {
            get => (ProgressRingTemplateSettings)GetValue(TemplateSettingsProperty);
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
                typeof(ProgressRing),
                new FrameworkPropertyMetadata(OnValuePropertyChanged));

        private static void OnValuePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((ProgressRing)sender).OnRangeBasePropertyChanged(args);
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
                typeof(ProgressRing),
                new FrameworkPropertyMetadata(OnStrokeThicknessPropertyChanged));

        private static void OnStrokeThicknessPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((ProgressRing)sender).OnStrokeThicknessPropertyChanged(args);
        }

        #endregion

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new ProgressRingAutomationPeer(this);
        }

        public override void OnApplyTemplate()
        {
            ApplyTemplateSettings();

            base.OnApplyTemplate();
        }

        void ApplyTemplateSettings()
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

        void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateSize();
            ChangeVisualState();
            UpdateRing();
        }

        void OnRangeBasePropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            UpdateSegment();
        }

        void OnIsActivePropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            ChangeVisualState();
        }

        void OnStrokeThicknessPropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            UpdateRing();
        }

        Size ComputeEllipseSize(double thickness, double actualWidth, double actualHeight)
        {
            double safeThickness = Math.Max(thickness, (double)0.0);
            double width = Math.Max((actualWidth - safeThickness) / 2.0, 0.0);
            double height = Math.Max((actualHeight - safeThickness) / 2.0, 0.0);

            return new Size(width, height);
        }

        void ChangeVisualState()
        {
            VisualStateManager.GoToState(this, IsActive ? s_ActiveStateName : s_InactiveStateName, true);
            VisualStateManager.GoToState(this, TemplateSettings.MaxSideLength < 60 ? s_SmallStateName : s_LargeStateName, true);
        }

        void UpdateSize()
        {
            // TemplateSetting properties from WUXC for backwards compatibility.
            var templateSettings = TemplateSettings;

            var (width, diameterValue, anchorPoint) = calcSettings();
            (double, double, double) calcSettings()
            {
                if (ActualWidth != 0)
                {
                    double width = Math.Min(ActualWidth, ActualHeight);

                    double diameterAdditive;
                    {
                        double init()
                        {
                            if (width <= 40.0)
                            {
                                return 1.0;
                            }
                            return 0.0;
                        }
                        diameterAdditive = init();
                    }

                    double diamaterValue = (width * 0.1) + diameterAdditive;
                    double anchorPoint = (width * 0.5) - diamaterValue;
                    return (width, diamaterValue, anchorPoint);
                }

                return (0.0, 0.0, 0.0);
            };

            templateSettings.EllipseDiameter = diameterValue;

            Thickness thicknessEllipseOffset = new Thickness(0, anchorPoint, 0, 0);

            templateSettings.EllipseOffset = thicknessEllipseOffset;
            templateSettings.MaxSideLength = width;
        }

        void UpdateSegment()
        {
            var templateSettings = TemplateSettings;

            double angle()
            {
                double normalizedRange()
                {
                    double minimum = Minimum;
                    double range = Maximum - minimum;
                    double delta = Value - minimum;

                    double normalizedRange = (range == 0.0) ? 0.0 : (delta / range);
                    // normalizedRange offsets calculation to display a full ring when value = 100%
                    // std::nextafter is set as a float as winrt::Point takes floats
                    return Math.Min(normalizedRange, 0.999999940395355224609375);
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
