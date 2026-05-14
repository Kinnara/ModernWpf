using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ModernWpf.Media.Animation;

namespace ModernWpf.Controls
{
    public class BorderEx : Border
    {
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
                new PropertyMetadata(null));

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
                Background,
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
    }
}
