using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ModernWpf.Controls
{
    public partial class RefreshVisualizer : Control
    {
        private const string RootName = "Root";
        private const double MinimumIndicatorOpacity = 0.4;
        private const double DefaultIndicatorSize = 30;
        private const double ParallaxPositionRatio = 0.5;

        static RefreshVisualizer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RefreshVisualizer), new FrameworkPropertyMetadata(typeof(RefreshVisualizer)));
        }

        internal IRefreshInfoProvider InfoProvider
        {
            get => GetValue(InfoProviderProperty) as IRefreshInfoProvider;
            set => SetValue(InfoProviderProperty, value);
        }

        public event TypedEventHandler<RefreshVisualizer, RefreshRequestedEventArgs> RefreshRequested;

        public event TypedEventHandler<RefreshVisualizer, RefreshStateChangedEventArgs> RefreshStateChanged;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _root = GetTemplateChild(RootName) as Panel;
            _state = State;
            _orientation = Orientation;
            OnOrientationChangedImpl();

            _content = Content;
            if (_content == null)
            {
                SetCurrentValue(ContentProperty, CreateDefaultContent());
            }
            else
            {
                OnContentChangedImpl();
            }

            UpdateContent();
        }

        public void RequestRefresh()
        {
            UpdateRefreshState(RefreshVisualizerState.Refreshing);
            _refreshInfoProvider?.OnRefreshStarted();
            RaiseRefreshRequested();
        }

        internal void SetInternalPullDirection(RefreshPullDirection value)
        {
            _pullDirection = value;
            OnOrientationChangedImpl();
            UpdateContent();
        }

        private static void OnInfoProviderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((RefreshVisualizer)d).OnRefreshInfoProviderChanged(e.OldValue as IRefreshInfoProvider, e.NewValue as IRefreshInfoProvider);
        }

        private static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var visualizer = (RefreshVisualizer)d;
            visualizer._orientation = (RefreshVisualizerOrientation)e.NewValue;
            visualizer.OnOrientationChangedImpl();
            visualizer.UpdateContent();
        }

        private static void OnStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var visualizer = (RefreshVisualizer)d;
            var oldState = visualizer._state;
            visualizer._state = (RefreshVisualizerState)e.NewValue;
            visualizer.UpdateContent();
            visualizer.RaiseRefreshStateChanged(oldState, visualizer._state);
        }

        private static void OnContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var visualizer = (RefreshVisualizer)d;
            visualizer._content = e.NewValue as UIElement;
            visualizer.OnContentChangedImpl();
            visualizer.UpdateContent();
        }

        private void OnRefreshInfoProviderChanged(IRefreshInfoProvider oldProvider, IRefreshInfoProvider newProvider)
        {
            if (oldProvider != null)
            {
                oldProvider.IsInteractingForRefreshChanged -= RefreshInfoProvider_InteractingForRefreshChanged;
                oldProvider.InteractionRatioChanged -= RefreshInfoProvider_InteractionRatioChanged;
            }

            _refreshInfoProvider = newProvider;

            if (_refreshInfoProvider != null)
            {
                _refreshInfoProvider.IsInteractingForRefreshChanged += RefreshInfoProvider_InteractingForRefreshChanged;
                _refreshInfoProvider.InteractionRatioChanged += RefreshInfoProvider_InteractionRatioChanged;
                _executionRatio = _refreshInfoProvider.ExecutionRatio;
            }
            else
            {
                _executionRatio = 1.0;
            }
        }

        private void OnOrientationChangedImpl()
        {
            switch (_orientation)
            {
                case RefreshVisualizerOrientation.Auto:
                    switch (_pullDirection)
                    {
                        case RefreshPullDirection.LeftToRight:
                            _startingRotationAngle = -90.0;
                            break;
                        case RefreshPullDirection.RightToLeft:
                            _startingRotationAngle = 90.0;
                            break;
                        default:
                            _startingRotationAngle = 0.0;
                            break;
                    }
                    break;
                case RefreshVisualizerOrientation.Normal:
                    _startingRotationAngle = 0.0;
                    break;
                case RefreshVisualizerOrientation.Rotate270DegreesCounterclockwise:
                    _startingRotationAngle = -90.0;
                    break;
                case RefreshVisualizerOrientation.Rotate90DegreesCounterclockwise:
                    _startingRotationAngle = 90.0;
                    break;
                default:
                    _startingRotationAngle = 0.0;
                    break;
            }
        }

        private void OnContentChangedImpl()
        {
            if (_root == null)
            {
                return;
            }

            _root.Children.Clear();

            if (_content == null)
            {
                _content = CreateDefaultContent();
            }

            if (_content is FrameworkElement frameworkElement)
            {
                frameworkElement.HorizontalAlignment = HorizontalAlignment.Center;
                frameworkElement.VerticalAlignment = VerticalAlignment.Center;
                _containerPanel = _root;
            }
            else
            {
                var containerPanel = new Grid
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                _root.Children.Insert(0, containerPanel);
                _containerPanel = containerPanel;
            }

            _containerPanel.Children.Insert(0, _content);
        }

        private void UpdateContent()
        {
            if (_content == null)
            {
                return;
            }

            if (_content is UIElement content)
            {
                switch (_state)
                {
                    case RefreshVisualizerState.Idle:
                        SetContentOpacity(MinimumIndicatorOpacity);
                        SetContentRotation(_startingRotationAngle);
                        SetContentTranslation(0);
                        SetContentScale(1);
                        break;
                    case RefreshVisualizerState.Peeking:
                        SetContentOpacity(1);
                        SetContentRotation(_startingRotationAngle);
                        SetContentScale(1);
                        break;
                    case RefreshVisualizerState.Interacting:
                        ExecuteInteractingAnimations();
                        break;
                    case RefreshVisualizerState.Pending:
                        ExecuteScaleUpAnimation();
                        SetContentOpacity(1);
                        SetContentRotation(_startingRotationAngle);
                        break;
                    case RefreshVisualizerState.Refreshing:
                        ExecuteExecutingRotationAnimation();
                        SetContentOpacity(1);
                        UpdateRefreshingTranslation();
                        break;
                }
            }
        }

        private void ExecuteInteractingAnimations()
        {
            var clampedRatio = Math.Max(0, Math.Min(1, _interactionRatio / Math.Max(_executionRatio, double.Epsilon)));
            SetContentOpacity(MinimumIndicatorOpacity + ((1 - MinimumIndicatorOpacity) * clampedRatio));
            SetContentRotation(_startingRotationAngle + (360 * clampedRatio));
            SetContentTranslation(GetParallaxTranslation(clampedRatio));
            SetContentScale(1);
        }

        private void ExecuteScaleUpAnimation()
        {
            SetContentScale(1);

            if (!SharedHelpers.IsAnimationsEnabled || !(_content is FrameworkElement element))
            {
                return;
            }

            var scale = EnsureScaleTransform(element);
            var animation = new DoubleAnimationUsingKeyFrames
            {
                Duration = TimeSpan.FromMilliseconds(300)
            };
            animation.KeyFrames.Add(new DiscreteDoubleKeyFrame(1.5, KeyTime.FromPercent(0.5)));
            animation.KeyFrames.Add(new EasingDoubleKeyFrame(1.0, KeyTime.FromPercent(1.0)));
            scale.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, animation.Clone());
        }

        private void ExecuteExecutingRotationAnimation()
        {
            if (!SharedHelpers.IsAnimationsEnabled || !(_content is FrameworkElement element))
            {
                SetContentRotation(_startingRotationAngle);
                return;
            }

            var rotate = EnsureRotateTransform(element);
            var animation = new DoubleAnimation(
                _startingRotationAngle,
                _startingRotationAngle + 360,
                TimeSpan.FromMilliseconds(500))
            {
                RepeatBehavior = RepeatBehavior.Forever
            };
            rotate.BeginAnimation(RotateTransform.AngleProperty, animation);
        }

        private void UpdateRefreshingTranslation()
        {
            var translationRatio = (1 - (_refreshInfoProvider?.ExecutionRatio ?? 0.0)) * ParallaxPositionRatio;
            translationRatio = IsPullDirectionFar() ? -translationRatio : translationRatio;

            var rootSize = _root == null ? 0 : (IsPullDirectionVertical() ? _root.ActualHeight : _root.ActualWidth);
            SetContentTranslation(translationRatio * rootSize);
        }

        private void UpdateRefreshState(RefreshVisualizerState newState)
        {
            if (newState != _state)
            {
                SetValue(StatePropertyKey, newState);
            }
        }

        private void RaiseRefreshStateChanged(RefreshVisualizerState oldState, RefreshVisualizerState newState)
        {
            if (oldState != newState)
            {
                RefreshStateChanged?.Invoke(this, new RefreshStateChangedEventArgs(oldState, newState));
            }
        }

        private void RaiseRefreshRequested()
        {
            var args = new RefreshRequestedEventArgs(RefreshCompleted);
            args.IncrementDeferralCount();
            RefreshRequested?.Invoke(this, args);
            args.DecrementDeferralCount();
            args.CompleteEvent();
        }

        private void RefreshCompleted()
        {
            UpdateRefreshState(RefreshVisualizerState.Idle);
            _refreshInfoProvider?.OnRefreshCompleted();
        }

        private void RefreshInfoProvider_InteractingForRefreshChanged(IRefreshInfoProvider sender, object args)
        {
            _isInteractingForRefresh = sender.IsInteractingForRefresh;
            if (!_isInteractingForRefresh)
            {
                switch (_state)
                {
                    case RefreshVisualizerState.Pending:
                        RequestRefresh();
                        break;
                    case RefreshVisualizerState.Refreshing:
                        break;
                    default:
                        UpdateRefreshState(RefreshVisualizerState.Idle);
                        break;
                }
            }
        }

        private void RefreshInfoProvider_InteractionRatioChanged(IRefreshInfoProvider sender, RefreshInteractionRatioChangedEventArgs args)
        {
            var wasAtZero = _interactionRatio == 0.0;
            _interactionRatio = args.InteractionRatio;
            if (_isInteractingForRefresh)
            {
                if (_state == RefreshVisualizerState.Idle)
                {
                    if (wasAtZero)
                    {
                        if (_interactionRatio > _executionRatio)
                        {
                            UpdateRefreshState(RefreshVisualizerState.Pending);
                        }
                        else if (_interactionRatio > 0.0)
                        {
                            UpdateRefreshState(RefreshVisualizerState.Interacting);
                        }
                    }
                    else if (_interactionRatio > 0.0)
                    {
                        UpdateRefreshState(RefreshVisualizerState.Peeking);
                    }
                }
                else if (_state == RefreshVisualizerState.Interacting)
                {
                    if (_interactionRatio <= 0.0)
                    {
                        UpdateRefreshState(RefreshVisualizerState.Idle);
                    }
                    else if (_interactionRatio > _executionRatio)
                    {
                        UpdateRefreshState(RefreshVisualizerState.Pending);
                    }
                }
                else if (_state == RefreshVisualizerState.Pending)
                {
                    if (_interactionRatio <= _executionRatio)
                    {
                        UpdateRefreshState(RefreshVisualizerState.Interacting);
                    }
                    else if (_interactionRatio <= 0.0)
                    {
                        UpdateRefreshState(RefreshVisualizerState.Idle);
                    }
                }
            }
            else if (_state != RefreshVisualizerState.Refreshing)
            {
                UpdateRefreshState(_interactionRatio > 0.0 ? RefreshVisualizerState.Peeking : RefreshVisualizerState.Idle);
            }
        }

        private void SetContentOpacity(double opacity)
        {
            if (_content != null)
            {
                _content.Opacity = opacity;
            }
        }

        private void SetContentRotation(double angle)
        {
            if (_content is FrameworkElement element)
            {
                EnsureRotateTransform(element).Angle = angle;
            }
        }

        private void SetContentScale(double scale)
        {
            if (_content is FrameworkElement element)
            {
                var scaleTransform = EnsureScaleTransform(element);
                scaleTransform.ScaleX = scale;
                scaleTransform.ScaleY = scale;
            }
        }

        private void SetContentTranslation(double offset)
        {
            if (_content is FrameworkElement element)
            {
                var translate = EnsureTranslateTransform(element);
                if (IsPullDirectionVertical())
                {
                    translate.Y = offset;
                }
                else
                {
                    translate.X = offset;
                }
            }
        }

        private double GetParallaxTranslation(double ratio)
        {
            if (_root == null)
            {
                return 0;
            }

            var rootSize = IsPullDirectionVertical() ? _root.ActualHeight : _root.ActualWidth;
            var translation = ((1.0 - _executionRatio) * rootSize * ParallaxPositionRatio) * ratio;
            return IsPullDirectionFar() ? -translation : translation;
        }

        private RotateTransform EnsureRotateTransform(FrameworkElement element)
        {
            var group = EnsureTransformGroup(element);
            for (var i = 0; i < group.Children.Count; i++)
            {
                if (group.Children[i] is RotateTransform rotateTransform)
                {
                    return rotateTransform;
                }
            }

            var rotate = new RotateTransform();
            group.Children.Add(rotate);
            return rotate;
        }

        private ScaleTransform EnsureScaleTransform(FrameworkElement element)
        {
            var group = EnsureTransformGroup(element);
            for (var i = 0; i < group.Children.Count; i++)
            {
                if (group.Children[i] is ScaleTransform scaleTransform)
                {
                    return scaleTransform;
                }
            }

            var scale = new ScaleTransform();
            group.Children.Add(scale);
            return scale;
        }

        private TranslateTransform EnsureTranslateTransform(FrameworkElement element)
        {
            var group = EnsureTransformGroup(element);
            for (var i = 0; i < group.Children.Count; i++)
            {
                if (group.Children[i] is TranslateTransform translateTransform)
                {
                    return translateTransform;
                }
            }

            var translate = new TranslateTransform();
            group.Children.Add(translate);
            return translate;
        }

        private static TransformGroup EnsureTransformGroup(FrameworkElement element)
        {
            if (element.RenderTransform is TransformGroup transformGroup)
            {
                return transformGroup;
            }

            transformGroup = new TransformGroup();
            if (element.RenderTransform != null && element.RenderTransform != Transform.Identity)
            {
                transformGroup.Children.Add(element.RenderTransform);
            }

            element.RenderTransform = transformGroup;
            element.RenderTransformOrigin = new Point(0.5, 0.5);
            return transformGroup;
        }

        private bool IsPullDirectionVertical()
        {
            return _pullDirection == RefreshPullDirection.TopToBottom ||
                _pullDirection == RefreshPullDirection.BottomToTop;
        }

        private bool IsPullDirectionFar()
        {
            return _pullDirection == RefreshPullDirection.BottomToTop ||
                _pullDirection == RefreshPullDirection.RightToLeft;
        }

        private static SymbolIcon CreateDefaultContent()
        {
            return new SymbolIcon(Symbol.Refresh)
            {
                Width = DefaultIndicatorSize,
                Height = DefaultIndicatorSize
            };
        }

        private RefreshVisualizerOrientation _orientation = RefreshVisualizerOrientation.Auto;
        private RefreshVisualizerState _state = RefreshVisualizerState.Idle;
        private IRefreshInfoProvider _refreshInfoProvider;
        private UIElement _content;
        private bool _isInteractingForRefresh;
        private double _executionRatio = 0.8;
        private double _interactionRatio;
        private Panel _containerPanel;
        private Panel _root;
        private double _startingRotationAngle;
        private RefreshPullDirection _pullDirection = RefreshPullDirection.TopToBottom;
    }
}
