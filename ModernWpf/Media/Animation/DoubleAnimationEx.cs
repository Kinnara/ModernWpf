using System.Windows;
using System.Windows.Media.Animation;

namespace ModernWpf.Media.Animation
{
    public class DoubleAnimationEx : DoubleAnimation
    {
        private double? _min;
        private double? _max;

        public DoubleAnimationEx()
        {
        }

        public DoubleAnimationEx(double toValue, Duration duration) : base(toValue, duration)
        {
        }

        public DoubleAnimationEx(double toValue, Duration duration, FillBehavior fillBehavior) : base(toValue, duration, fillBehavior)
        {
        }

        public DoubleAnimationEx(double fromValue, double toValue, Duration duration) : base(fromValue, toValue, duration)
        {
        }

        public DoubleAnimationEx(double fromValue, double toValue, Duration duration, FillBehavior fillBehavior) : base(fromValue, toValue, duration, fillBehavior)
        {
        }

        public static readonly DependencyProperty MinProperty =
            DependencyProperty.Register(
                nameof(Min),
                typeof(double?),
                typeof(DoubleAnimationEx),
                new PropertyMetadata(OnMinChanged));

        public double? Min
        {
            get => (double?)GetValue(MinProperty);
            set => SetValue(MinProperty, value);
        }

        private static void OnMinChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DoubleAnimationEx)d)._min = (double?)e.NewValue;
        }

        public static readonly DependencyProperty MaxProperty =
            DependencyProperty.Register(
                nameof(Max),
                typeof(double?),
                typeof(DoubleAnimationEx),
                new PropertyMetadata(OnMaxChanged));

        public double? Max
        {
            get => (double?)GetValue(MaxProperty);
            set => SetValue(MaxProperty, value);
        }

        private static void OnMaxChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DoubleAnimationEx)d)._max = (double?)e.NewValue;
        }

        protected override double GetCurrentValueCore(double defaultOriginValue, double defaultDestinationValue, AnimationClock animationClock)
        {
            double value = base.GetCurrentValueCore(defaultOriginValue, defaultDestinationValue, animationClock);

            double? min = _min;
            if (min.HasValue && value < min.Value)
            {
                return min.Value;
            }

            double? max = _max;
            if (max.HasValue && value > max.Value)
            {
                return max.Value;
            }

            return value;
        }

        protected override Freezable CreateInstanceCore()
        {
            return new DoubleAnimationEx();
        }
    }
}
