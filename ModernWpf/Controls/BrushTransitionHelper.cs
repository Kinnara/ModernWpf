using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ModernWpf.Controls
{
    internal sealed class BrushTransitionHelper
    {
        public BrushTransitionHelper(Action invalidateVisual)
        {
            _invalidateVisual = invalidateVisual ?? throw new ArgumentNullException(nameof(invalidateVisual));
        }

        public bool IsTransitioning => _transitionBrush != null;

        public Brush GetEffectiveBrush(Brush brush)
        {
            return _transitionBrush ?? brush;
        }

        public void OnBrushChanged(Brush oldBrush, Brush newBrush, BrushTransition transition)
        {
            if (!TryStartTransition(oldBrush, newBrush, transition))
            {
                ClearTransition();
            }

            _invalidateVisual();
        }

        public void OnTransitionChanged(BrushTransition transition)
        {
            if (transition == null || transition.Duration <= TimeSpan.Zero)
            {
                ClearTransition();
            }
        }

        public void ClearTransition()
        {
            if (_transitionBrush == null)
            {
                return;
            }

            _generation++;
            _transitionBrush.BeginAnimation(SolidColorBrush.ColorProperty, null);
            _transitionBrush = null;
            _targetBrush = null;
            _invalidateVisual();
        }

        private bool TryStartTransition(Brush oldBrush, Brush newBrush, BrushTransition transition)
        {
            if (transition == null ||
                transition.Duration <= TimeSpan.Zero ||
                ReferenceEquals(oldBrush, newBrush) ||
                !Helper.IsAnimationsEnabled)
            {
                return false;
            }

            if (!(newBrush is SolidColorBrush newSolidColorBrush) ||
                newSolidColorBrush.HasAnimatedProperties)
            {
                return false;
            }

            var isHandoff = _transitionBrush != null && ReferenceEquals(_targetBrush, oldBrush);
            if (!isHandoff)
            {
                if (!TryGetStartColor(oldBrush, newSolidColorBrush.Color, out var startColor))
                {
                    return false;
                }

                _transitionBrush = new SolidColorBrush(startColor);
            }

            _targetBrush = newBrush;
            _generation++;

            var generation = _generation;
            var animation = new ColorAnimation
            {
                To = newSolidColorBrush.Color,
                Duration = new Duration(transition.Duration),
                FillBehavior = FillBehavior.HoldEnd
            };

            animation.Completed += (sender, args) =>
            {
                if (_generation == generation)
                {
                    ClearTransition();
                }
            };

            _transitionBrush.BeginAnimation(
                SolidColorBrush.ColorProperty,
                animation,
                HandoffBehavior.SnapshotAndReplace);
            return true;
        }

        private static bool TryGetStartColor(Brush oldBrush, Color targetColor, out Color startColor)
        {
            if (oldBrush == null)
            {
                startColor = Color.FromArgb(0, targetColor.R, targetColor.G, targetColor.B);
                return true;
            }

            if (oldBrush is SolidColorBrush oldSolidColorBrush &&
                !oldSolidColorBrush.HasAnimatedProperties)
            {
                startColor = oldSolidColorBrush.Color;
                return true;
            }

            startColor = default;
            return false;
        }

        private readonly Action _invalidateVisual;
        private SolidColorBrush _transitionBrush;
        private Brush _targetBrush;
        private int _generation;
    }
}
