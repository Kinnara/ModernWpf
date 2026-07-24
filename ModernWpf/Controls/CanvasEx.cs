using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using ModernWpf.Media.Animation;

namespace ModernWpf.Controls
{
    [ContentProperty(nameof(Children))]
    public class CanvasEx : Canvas
    {
        static CanvasEx()
        {
            BackgroundProperty.OverrideMetadata(
                typeof(CanvasEx),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    OnBackgroundPropertyChanged));
        }

        public static new readonly DependencyProperty LeftProperty =
            Canvas.LeftProperty.AddOwner(typeof(CanvasEx));

        public static new double GetLeft(UIElement element) => (double)element.GetValue(LeftProperty);

        public static new void SetLeft(UIElement element, double length) => element.SetValue(LeftProperty, length);

        public static new readonly DependencyProperty TopProperty =
            Canvas.TopProperty.AddOwner(typeof(CanvasEx));

        public static new double GetTop(UIElement element) => (double)element.GetValue(TopProperty);

        public static new void SetTop(UIElement element, double length) => element.SetValue(TopProperty, length);

        public static new readonly DependencyProperty ZIndexProperty =
            Panel.ZIndexProperty.AddOwner(typeof(CanvasEx));

        public static new int GetZIndex(UIElement element) => (int)element.GetValue(ZIndexProperty);

        public static new void SetZIndex(UIElement element, int value) => element.SetValue(ZIndexProperty, value);

        public static readonly DependencyProperty BackgroundTransitionProperty =
            DependencyProperty.Register(
                nameof(BackgroundTransition),
                typeof(BrushTransition),
                typeof(CanvasEx),
                new PropertyMetadata(null, OnBackgroundTransitionPropertyChanged));

        public BrushTransition BackgroundTransition
        {
            get => (BrushTransition)GetValue(BackgroundTransitionProperty);
            set => SetValue(BackgroundTransitionProperty, value);
        }

        public static readonly DependencyProperty ChildrenTransitionsProperty =
            DependencyProperty.Register(
                nameof(ChildrenTransitions),
                typeof(TransitionCollection),
                typeof(CanvasEx),
                new PropertyMetadata(null));

        public TransitionCollection ChildrenTransitions
        {
            get => (TransitionCollection)GetValue(ChildrenTransitionsProperty);
            set => SetValue(ChildrenTransitionsProperty, value);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            var background = EffectiveBackground;
            if (background != null)
            {
                drawingContext.DrawRectangle(background, null, new Rect(RenderSize));
            }
        }

        internal Brush EffectiveBackground => _backgroundTransitionHelper?.GetEffectiveBrush(Background) ?? Background;

        private static void OnBackgroundPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var canvas = (CanvasEx)d;
            if (canvas.BackgroundTransition != null || canvas._backgroundTransitionHelper?.IsTransitioning == true)
            {
                canvas.BackgroundTransitionHelper.OnBrushChanged(
                    (Brush)e.OldValue,
                    (Brush)e.NewValue,
                    canvas.BackgroundTransition);
            }
        }

        private static void OnBackgroundTransitionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CanvasEx)d)._backgroundTransitionHelper?.OnTransitionChanged((BrushTransition)e.NewValue);
        }

        private BrushTransitionHelper BackgroundTransitionHelper =>
            _backgroundTransitionHelper ?? (_backgroundTransitionHelper = new BrushTransitionHelper(InvalidateVisual));

        private BrushTransitionHelper _backgroundTransitionHelper;
    }
}
