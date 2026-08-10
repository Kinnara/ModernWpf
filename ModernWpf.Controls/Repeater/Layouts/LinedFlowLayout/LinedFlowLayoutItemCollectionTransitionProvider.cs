// Copyright (c) Kinnara. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace ModernWpf.Controls
{
    public class LinedFlowLayoutItemCollectionTransitionProvider : ItemCollectionTransitionProvider
    {
        private static readonly Duration QuickDuration = new Duration(TimeSpan.FromMilliseconds(120));
        private static readonly Duration DefaultDuration = new Duration(TimeSpan.FromMilliseconds(250));

        public LinedFlowLayoutItemCollectionTransitionProvider()
        {
        }

        protected override bool ShouldAnimateCore(ItemCollectionTransition transition)
        {
            return true;
        }

        protected override void StartTransitions(IList<ItemCollectionTransition> transitions)
        {
            bool hasAdds = false;
            bool hasRemoves = false;
            bool hasMoves = false;
            foreach (var transition in transitions)
            {
                hasAdds |= transition.Operation == ItemCollectionTransitionOperation.Add;
                hasRemoves |= transition.Operation == ItemCollectionTransitionOperation.Remove;
                hasMoves |= transition.Operation == ItemCollectionTransitionOperation.Move;
            }

            foreach (var transition in transitions)
            {
                if (transition.Operation == ItemCollectionTransitionOperation.Remove &&
                    hasAdds &&
                    (transition.Triggers & ItemCollectionTransitionTriggers.CollectionChangeReset) != 0)
                {
                    continue;
                }

                if (transition.Operation == ItemCollectionTransitionOperation.Move &&
                    (transition.Triggers & ItemCollectionTransitionTriggers.LayoutTransition) != 0)
                {
                    continue;
                }

                TimeSpan delay = TimeSpan.Zero;
                if (transition.Operation == ItemCollectionTransitionOperation.Move && hasRemoves)
                {
                    delay = QuickDuration.TimeSpan;
                }
                else if (transition.Operation == ItemCollectionTransitionOperation.Add)
                {
                    if (hasRemoves)
                    {
                        delay += QuickDuration.TimeSpan;
                    }

                    if (hasMoves)
                    {
                        delay += DefaultDuration.TimeSpan + DefaultDuration.TimeSpan;
                    }
                }

                StartTransition(transition, delay);
            }
        }

        private void StartTransition(ItemCollectionTransition transition, TimeSpan delay)
        {
            var element = transition.Element;
            if (_activeTransitions.TryGetValue(element, out ActiveTransition previousTransition))
            {
                CompleteTransition(element, previousTransition);
            }

            var progress = transition.Start();

            var originalTransform = element.RenderTransform;
            var originalTransformOrigin = element.RenderTransformOrigin;
            var translate = new TranslateTransform();
            var scale = new ScaleTransform(1.0, 1.0);
            var transformGroup = new TransformGroup();
            if (originalTransform != null && originalTransform != Transform.Identity)
            {
                transformGroup.Children.Add(originalTransform);
            }

            transformGroup.Children.Add(translate);
            transformGroup.Children.Add(scale);
            element.RenderTransform = transformGroup;
            element.RenderTransformOrigin = new Point(0.5, 0.5);

            var duration = transition.Operation == ItemCollectionTransitionOperation.Move
                ? DefaultDuration
                : QuickDuration;
            var easing = new CubicEase { EasingMode = EasingMode.EaseOut };
            var xAnimation = new DoubleAnimation(0.0, duration)
            {
                BeginTime = delay,
                EasingFunction = easing,
                FillBehavior = FillBehavior.Stop
            };
            var yAnimation = new DoubleAnimation(0.0, duration)
            {
                BeginTime = delay,
                EasingFunction = easing,
                FillBehavior = FillBehavior.Stop
            };

            if (transition.Operation == ItemCollectionTransitionOperation.Move &&
                Math.Abs(transition.OldBounds.Y - transition.NewBounds.Y) < 0.01)
            {
                xAnimation.From = transition.OldBounds.X - transition.NewBounds.X;
                yAnimation.From = transition.OldBounds.Y - transition.NewBounds.Y;
            }
            else if (transition.Operation == ItemCollectionTransitionOperation.Add &&
                (transition.Triggers & ItemCollectionTransitionTriggers.CollectionChangeAdd) == 0)
            {
                duration = DefaultDuration;
                xAnimation.Duration = duration;
                yAnimation.Duration = duration;
                yAnimation.From = 100.0;
            }

            var opacityAnimation = transition.Operation == ItemCollectionTransitionOperation.Add &&
                (transition.Triggers & ItemCollectionTransitionTriggers.CollectionChangeAdd) == 0
                ? new DoubleAnimation(0.0, 1.0, duration)
                : new DoubleAnimation(1.0, 1.0, duration);
            opacityAnimation.BeginTime = delay;
            opacityAnimation.EasingFunction = easing;
            opacityAnimation.FillBehavior = FillBehavior.Stop;

            DoubleAnimation scaleAnimation = null;
            DoubleAnimationUsingKeyFrames crossLineScaleAnimation = null;
            if (transition.Operation == ItemCollectionTransitionOperation.Add &&
                (transition.Triggers & ItemCollectionTransitionTriggers.CollectionChangeAdd) != 0)
            {
                scaleAnimation = new DoubleAnimation(0.0, 1.0, QuickDuration)
                {
                    BeginTime = delay,
                    EasingFunction = easing,
                    FillBehavior = FillBehavior.Stop
                };
            }
            else if (transition.Operation == ItemCollectionTransitionOperation.Remove)
            {
                scaleAnimation = new DoubleAnimation(1.0, 0.0, QuickDuration)
                {
                    BeginTime = delay,
                    EasingFunction = easing,
                    FillBehavior = FillBehavior.Stop
                };
            }
            else if (transition.Operation == ItemCollectionTransitionOperation.Move &&
                Math.Abs(transition.OldBounds.Y - transition.NewBounds.Y) >= 0.01)
            {
                crossLineScaleAnimation = new DoubleAnimationUsingKeyFrames
                {
                    BeginTime = delay,
                    Duration = DefaultDuration,
                    FillBehavior = FillBehavior.Stop
                };
                crossLineScaleAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(1.0, KeyTime.FromPercent(0.0)));
                crossLineScaleAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(0.0, KeyTime.FromPercent(0.5), easing));
                crossLineScaleAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(1.0, KeyTime.FromPercent(1.0), easing));
            }

            ActiveTransition activeTransition = null;
            EventHandler onCompleted = null;
            onCompleted = (sender, args) =>
            {
                CompleteTransition(element, activeTransition);
            };

            var completionTimer = new DispatcherTimer(DispatcherPriority.Background)
            {
                Interval = delay + duration.TimeSpan + TimeSpan.FromSeconds(1)
            };
            EventHandler onCompletionTimeout = null;
            onCompletionTimeout = (sender, args) => CompleteTransition(element, activeTransition);
            completionTimer.Tick += onCompletionTimeout;

            activeTransition = new ActiveTransition(
                progress,
                originalTransform,
                originalTransformOrigin,
                transformGroup,
                translate,
                scale,
                opacityAnimation,
                onCompleted,
                completionTimer,
                onCompletionTimeout);
            _activeTransitions[element] = activeTransition;
            opacityAnimation.Completed += onCompleted;
            try
            {
                translate.BeginAnimation(TranslateTransform.XProperty, xAnimation);
                translate.BeginAnimation(TranslateTransform.YProperty, yAnimation);
                if (scaleAnimation != null)
                {
                    scale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
                    scale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation.Clone());
                }
                else if (crossLineScaleAnimation != null)
                {
                    scale.BeginAnimation(ScaleTransform.ScaleXProperty, crossLineScaleAnimation);
                    scale.BeginAnimation(ScaleTransform.ScaleYProperty, crossLineScaleAnimation.Clone());
                }

                element.BeginAnimation(UIElement.OpacityProperty, opacityAnimation);
                completionTimer.Start();
            }
            catch
            {
                CompleteTransition(element, activeTransition);
                throw;
            }
        }

        private void CompleteTransition(UIElement element, ActiveTransition activeTransition)
        {
            if (activeTransition == null ||
                !_activeTransitions.TryGetValue(element, out ActiveTransition currentTransition) ||
                !ReferenceEquals(activeTransition, currentTransition))
            {
                return;
            }

            _activeTransitions.Remove(element);
            activeTransition.OpacityAnimation.Completed -= activeTransition.CompletedHandler;
            activeTransition.CompletionTimer.Tick -= activeTransition.CompletionTimeoutHandler;
            activeTransition.CompletionTimer.Stop();

            element.BeginAnimation(UIElement.OpacityProperty, null);
            activeTransition.Translate.BeginAnimation(TranslateTransform.XProperty, null);
            activeTransition.Translate.BeginAnimation(TranslateTransform.YProperty, null);
            activeTransition.Scale.BeginAnimation(ScaleTransform.ScaleXProperty, null);
            activeTransition.Scale.BeginAnimation(ScaleTransform.ScaleYProperty, null);
            if (ReferenceEquals(element.RenderTransform, activeTransition.TransformGroup))
            {
                element.RenderTransform = activeTransition.OriginalTransform;
                element.RenderTransformOrigin = activeTransition.OriginalTransformOrigin;
            }

            activeTransition.Progress.Complete();
        }

        private sealed class ActiveTransition
        {
            internal ActiveTransition(
                ItemCollectionTransitionProgress progress,
                Transform originalTransform,
                Point originalTransformOrigin,
                TransformGroup transformGroup,
                TranslateTransform translate,
                ScaleTransform scale,
                DoubleAnimation opacityAnimation,
                EventHandler completedHandler,
                DispatcherTimer completionTimer,
                EventHandler completionTimeoutHandler)
            {
                Progress = progress;
                OriginalTransform = originalTransform;
                OriginalTransformOrigin = originalTransformOrigin;
                TransformGroup = transformGroup;
                Translate = translate;
                Scale = scale;
                OpacityAnimation = opacityAnimation;
                CompletedHandler = completedHandler;
                CompletionTimer = completionTimer;
                CompletionTimeoutHandler = completionTimeoutHandler;
            }

            internal ItemCollectionTransitionProgress Progress { get; }

            internal Transform OriginalTransform { get; }

            internal Point OriginalTransformOrigin { get; }

            internal TransformGroup TransformGroup { get; }

            internal TranslateTransform Translate { get; }

            internal ScaleTransform Scale { get; }

            internal DoubleAnimation OpacityAnimation { get; }

            internal EventHandler CompletedHandler { get; }

            internal DispatcherTimer CompletionTimer { get; }

            internal EventHandler CompletionTimeoutHandler { get; }
        }

        private readonly Dictionary<UIElement, ActiveTransition> _activeTransitions =
            new Dictionary<UIElement, ActiveTransition>();
    }
}
