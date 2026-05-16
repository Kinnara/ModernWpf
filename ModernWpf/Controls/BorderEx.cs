using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ModernWpf.Media.Animation;

namespace ModernWpf.Controls
{
    public class BorderEx : Border
    {
        static BorderEx()
        {
            CornerRadiusProperty.OverrideMetadata(
                typeof(BorderEx),
                new FrameworkPropertyMetadata(
                    new CornerRadius(),
                    FrameworkPropertyMetadataOptions.AffectsArrange |
                    FrameworkPropertyMetadataOptions.AffectsRender));

            BackgroundProperty.OverrideMetadata(
                typeof(BorderEx),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    OnBackgroundPropertyChanged));
        }

        public static readonly DependencyProperty BackgroundSizingProperty =
            DependencyProperty.Register(
                nameof(BackgroundSizing),
                typeof(ModernWpf.Controls.BackgroundSizing),
                typeof(BorderEx),
                new FrameworkPropertyMetadata(
                    ModernWpf.Controls.BackgroundSizing.InnerBorderEdge,
                    FrameworkPropertyMetadataOptions.AffectsRender));

        public BackgroundSizing BackgroundSizing
        {
            get => (BackgroundSizing)GetValue(BackgroundSizingProperty);
            set => SetValue(BackgroundSizingProperty, value);
        }

        public static readonly DependencyProperty BackgroundTransitionProperty =
            DependencyProperty.Register(
                nameof(BackgroundTransition),
                typeof(BrushTransition),
                typeof(BorderEx),
                new PropertyMetadata(null, OnBackgroundTransitionPropertyChanged));

        public BrushTransition BackgroundTransition
        {
            get => (BrushTransition)GetValue(BackgroundTransitionProperty);
            set => SetValue(BackgroundTransitionProperty, value);
        }

        public static readonly DependencyProperty ChildTransitionsProperty =
            DependencyProperty.Register(
                nameof(ChildTransitions),
                typeof(TransitionCollection),
                typeof(BorderEx),
                new PropertyMetadata(null));

        public TransitionCollection ChildTransitions
        {
            get => (TransitionCollection)GetValue(ChildTransitionsProperty);
            set => SetValue(ChildTransitionsProperty, value);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            LayoutChromeHelper.DrawChrome(
                drawingContext,
                RenderSize,
                GetEffectiveBackground(),
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

        internal Brush EffectiveBackground => GetEffectiveBackground();

        private static void OnBackgroundPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var border = (BorderEx)d;
            if (border.BackgroundTransition != null || border._backgroundTransitionHelper?.IsTransitioning == true)
            {
                border.BackgroundTransitionHelper.OnBrushChanged(
                    (Brush)e.OldValue,
                    (Brush)e.NewValue,
                    border.BackgroundTransition);
            }
        }

        private static void OnBackgroundTransitionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((BorderEx)d)._backgroundTransitionHelper?.OnTransitionChanged((BrushTransition)e.NewValue);
        }

        private Brush GetEffectiveBackground()
        {
            return _backgroundTransitionHelper?.GetEffectiveBrush(Background) ?? Background;
        }

        private BrushTransitionHelper BackgroundTransitionHelper =>
            _backgroundTransitionHelper ?? (_backgroundTransitionHelper = new BrushTransitionHelper(InvalidateVisual));

        private BrushTransitionHelper _backgroundTransitionHelper;
    }
}
