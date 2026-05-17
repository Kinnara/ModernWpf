using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ModernWpf.Controls
{
    internal interface IRefreshInfoProvider
    {
        void OnRefreshStarted();

        void OnRefreshCompleted();

        bool IsInteractingForRefresh { get; }

        double ExecutionRatio { get; }

        event TypedEventHandler<IRefreshInfoProvider, object> IsInteractingForRefreshChanged;

        event TypedEventHandler<IRefreshInfoProvider, RefreshInteractionRatioChangedEventArgs> InteractionRatioChanged;

        event TypedEventHandler<IRefreshInfoProvider, object> RefreshStarted;

        event TypedEventHandler<IRefreshInfoProvider, object> RefreshCompleted;
    }

    internal interface IRefreshInfoProviderAdapter : IDisposable
    {
        IRefreshInfoProvider AdaptFromTree(DependencyObject root, Size visualizerSize);

        void SetAnimations(FrameworkElement refreshVisualizerAnimatableContainer);
    }

    internal interface IAdapterAnimationHandler
    {
        void InteractionTrackerAnimation(FrameworkElement refreshVisualizer, FrameworkElement infoProvider);

        void RefreshRequestedAnimation(FrameworkElement refreshVisualizer, FrameworkElement infoProvider, double executionRatio);

        void RefreshCompletedAnimation(FrameworkElement refreshVisualizer, FrameworkElement infoProvider);
    }

    internal sealed class RefreshInfoProviderImpl : IRefreshInfoProvider
    {
        private const double DefaultExecutionRatio = 0.8;
        private const double AlwaysRaiseInteractionRatioTolerance = 0.05;
        private const int RaiseInteractionRatioChangedFrequency = 5;

        private readonly RefreshPullDirection _pullDirection;
        private readonly Size _refreshVisualizerSize;
        private int _interactionRatioChangedCount;
        private bool _isInteractingForRefresh;
        private bool _peeking;

        internal RefreshInfoProviderImpl(RefreshPullDirection pullDirection, Size refreshVisualizerSize)
        {
            _pullDirection = pullDirection;
            _refreshVisualizerSize = refreshVisualizerSize;
        }

        public bool IsInteractingForRefresh => _isInteractingForRefresh;

        public double ExecutionRatio { get; } = DefaultExecutionRatio;

        internal RefreshPullDirection PullDirection => _pullDirection;

        internal Size RefreshVisualizerSize => _refreshVisualizerSize;

        internal double InteractionRatio { get; private set; }

        public event TypedEventHandler<IRefreshInfoProvider, object> IsInteractingForRefreshChanged;

        public event TypedEventHandler<IRefreshInfoProvider, RefreshInteractionRatioChangedEventArgs> InteractionRatioChanged;

        public event TypedEventHandler<IRefreshInfoProvider, object> RefreshStarted;

        public event TypedEventHandler<IRefreshInfoProvider, object> RefreshCompleted;

        public void OnRefreshStarted()
        {
            RefreshStarted?.Invoke(this, null);
        }

        public void OnRefreshCompleted()
        {
            RefreshCompleted?.Invoke(this, null);
        }

        internal void UpdateIsInteractingForRefresh(bool value)
        {
            var isInteractingForRefresh = value && !_peeking;
            if (isInteractingForRefresh != _isInteractingForRefresh)
            {
                _isInteractingForRefresh = isInteractingForRefresh;
                IsInteractingForRefreshChanged?.Invoke(this, null);
            }
        }

        internal void RaiseInteractionRatioChanged(double interactionRatio)
        {
            InteractionRatio = Math.Max(0, Math.Min(1, interactionRatio));

            if (_interactionRatioChangedCount == 0
                || AreClose(InteractionRatio, 0)
                || AreClose(InteractionRatio, ExecutionRatio))
            {
                InteractionRatioChanged?.Invoke(this, new RefreshInteractionRatioChangedEventArgs(InteractionRatio));
                _interactionRatioChangedCount = 1;
            }
            else if (_interactionRatioChangedCount >= RaiseInteractionRatioChangedFrequency)
            {
                _interactionRatioChangedCount = 0;
            }
            else
            {
                _interactionRatioChangedCount++;
            }
        }

        internal void SetPeekingMode(bool peeking)
        {
            _peeking = peeking;
        }

        private static bool AreClose(double interactionRatio, double target)
        {
            return Math.Abs(interactionRatio - target) < AlwaysRaiseInteractionRatioTolerance;
        }
    }

    internal sealed class ScrollViewerIRefreshInfoProviderAdapter : IRefreshInfoProviderAdapter
    {
        private const double InitialOffsetThreshold = 1.0;
        private const double DefaultPullDimensionSize = 100.0;
        private const int MaxBfsDepth = 10;

        private readonly RefreshPullDirection _pullDirection;
        private readonly IAdapterAnimationHandler _animationHandler;
        private ScrollViewer _scrollViewer;
        private RefreshInfoProviderImpl _infoProvider;
        private FrameworkElement _refreshVisualizerContainer;
        private FrameworkElement _infoProviderElement;
        private Point _pullStartPoint;
        private bool _isPointerDown;

        internal ScrollViewerIRefreshInfoProviderAdapter(
            RefreshPullDirection pullDirection,
            IAdapterAnimationHandler animationHandler = null)
        {
            _pullDirection = pullDirection;
            _animationHandler = animationHandler ?? new ScrollViewerIRefreshInfoProviderDefaultAnimationHandler(pullDirection);
        }

        public IRefreshInfoProvider AdaptFromTree(DependencyObject root, Size visualizerSize)
        {
            var scrollViewer = AdaptFromTreeCore(root);
            return scrollViewer == null ? null : Adapt(scrollViewer, visualizerSize);
        }

        public void SetAnimations(FrameworkElement refreshVisualizerAnimatableContainer)
        {
            _refreshVisualizerContainer = refreshVisualizerAnimatableContainer ?? throw new ArgumentNullException(nameof(refreshVisualizerAnimatableContainer));
            _animationHandler.InteractionTrackerAnimation(_refreshVisualizerContainer, _infoProviderElement);
        }

        public void Dispose()
        {
            CleanupScrollViewer();
            _scrollViewer = null;
            _infoProvider = null;
            _refreshVisualizerContainer = null;
            _infoProviderElement = null;
        }

        internal IRefreshInfoProvider Adapt(ScrollViewer adaptee, Size visualizerSize)
        {
            if (adaptee == null)
            {
                throw new ArgumentNullException(nameof(adaptee));
            }

            CleanupScrollViewer();

            _scrollViewer = adaptee;
            _infoProviderElement = GetScrollContent() as FrameworkElement ?? _scrollViewer;
            _infoProvider = new RefreshInfoProviderImpl(_pullDirection, NormalizeVisualizerSize(visualizerSize));
            _infoProvider.RefreshStarted += OnRefreshStarted;
            _infoProvider.RefreshCompleted += OnRefreshCompleted;

            _scrollViewer.PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
            _scrollViewer.PreviewMouseMove += OnPreviewMouseMove;
            _scrollViewer.PreviewMouseLeftButtonUp += OnPreviewMouseLeftButtonUp;
            _scrollViewer.LostMouseCapture += OnLostMouseCapture;
            _scrollViewer.ScrollChanged += OnScrollViewerScrollChanged;

            return _infoProvider;
        }

        internal bool CanStartPull => _scrollViewer == null || IsWithinOffsetThreshold();

        internal double InteractionRatio => _infoProvider?.InteractionRatio ?? 0;

        internal void PullForTesting(double directedDelta, bool complete)
        {
            if (_infoProvider == null || !CanStartPull)
            {
                return;
            }

            StartInteraction(captureMouse: false);
            UpdatePullDelta(directedDelta);

            if (complete)
            {
                CompleteInteraction();
            }
        }

        private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs args)
        {
            if (!CanStartPull)
            {
                _isPointerDown = false;
                return;
            }

            _isPointerDown = true;
            _pullStartPoint = args.GetPosition(_scrollViewer);
        }

        private void OnPreviewMouseMove(object sender, MouseEventArgs args)
        {
            if (!_isPointerDown || _scrollViewer == null)
            {
                return;
            }

            var position = args.GetPosition(_scrollViewer);
            var delta = GetDirectedDelta(position.X - _pullStartPoint.X, position.Y - _pullStartPoint.Y);
            if (delta <= 0)
            {
                return;
            }

            StartInteraction(captureMouse: true);
            UpdatePullDelta(delta);
            args.Handled = true;
        }

        private void OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs args)
        {
            if (!_isPointerDown)
            {
                return;
            }

            CompleteInteraction();
        }

        private void OnLostMouseCapture(object sender, MouseEventArgs args)
        {
            if (_isPointerDown)
            {
                CompleteInteraction();
            }
        }

        private void OnScrollViewerScrollChanged(object sender, ScrollChangedEventArgs args)
        {
            if (_infoProvider?.IsInteractingForRefresh == true && !IsWithinOffsetThreshold())
            {
                _infoProvider.UpdateIsInteractingForRefresh(false);
            }
        }

        private void StartInteraction(bool captureMouse)
        {
            _isPointerDown = true;
            _infoProvider?.UpdateIsInteractingForRefresh(true);
            if (captureMouse)
            {
                _scrollViewer?.CaptureMouse();
            }
        }

        private void UpdatePullDelta(double directedDelta)
        {
            if (_infoProvider == null || directedDelta <= 0)
            {
                return;
            }

            _infoProvider.RaiseInteractionRatioChanged(directedDelta / GetPullDimension());
        }

        private void CompleteInteraction()
        {
            _isPointerDown = false;
            _scrollViewer?.ReleaseMouseCapture();
            _infoProvider?.UpdateIsInteractingForRefresh(false);
            _infoProvider?.RaiseInteractionRatioChanged(0);
        }

        private double GetPullDimension()
        {
            var size = _infoProvider?.RefreshVisualizerSize ?? new Size(1, 1);
            var dimension = IsOrientationVertical() ? size.Height : size.Width;
            if (dimension <= 0 || double.IsNaN(dimension))
            {
                return DefaultPullDimensionSize;
            }

            return Math.Max(DefaultPullDimensionSize, dimension);
        }

        private double GetDirectedDelta(double horizontalDelta, double verticalDelta)
        {
            switch (_pullDirection)
            {
                case RefreshPullDirection.LeftToRight:
                    return horizontalDelta;
                case RefreshPullDirection.RightToLeft:
                    return -horizontalDelta;
                case RefreshPullDirection.BottomToTop:
                    return -verticalDelta;
                default:
                    return verticalDelta;
            }
        }

        private void OnRefreshStarted(IRefreshInfoProvider sender, object args)
        {
            _animationHandler.RefreshRequestedAnimation(_refreshVisualizerContainer, _infoProviderElement, sender.ExecutionRatio);
        }

        private void OnRefreshCompleted(IRefreshInfoProvider sender, object args)
        {
            _animationHandler.RefreshCompletedAnimation(_refreshVisualizerContainer, _infoProviderElement);
        }

        private bool IsWithinOffsetThreshold()
        {
            if (_scrollViewer == null)
            {
                return true;
            }

            switch (_pullDirection)
            {
                case RefreshPullDirection.LeftToRight:
                    return _scrollViewer.HorizontalOffset < InitialOffsetThreshold;
                case RefreshPullDirection.RightToLeft:
                    return _scrollViewer.HorizontalOffset > _scrollViewer.ScrollableWidth - InitialOffsetThreshold;
                case RefreshPullDirection.BottomToTop:
                    return _scrollViewer.VerticalOffset > _scrollViewer.ScrollableHeight - InitialOffsetThreshold;
                default:
                    return _scrollViewer.VerticalOffset < InitialOffsetThreshold;
            }
        }

        private bool IsOrientationVertical()
        {
            return _pullDirection == RefreshPullDirection.TopToBottom ||
                _pullDirection == RefreshPullDirection.BottomToTop;
        }

        private FrameworkElement GetScrollContent()
        {
            return _scrollViewer?.Content as FrameworkElement;
        }

        private ScrollViewer AdaptFromTreeCore(DependencyObject root)
        {
            if (root is ScrollViewer scrollViewer)
            {
                return scrollViewer;
            }

            for (var depth = 0; depth < MaxBfsDepth; depth++)
            {
                var result = AdaptFromTreeRecursiveHelper(root, depth);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        private static ScrollViewer AdaptFromTreeRecursiveHelper(DependencyObject root, int depth)
        {
            if (root == null)
            {
                return null;
            }

            var numChildren = VisualTreeHelper.GetChildrenCount(root);
            if (depth == 0)
            {
                for (var i = 0; i < numChildren; i++)
                {
                    if (VisualTreeHelper.GetChild(root, i) is ScrollViewer scrollViewer)
                    {
                        return scrollViewer;
                    }
                }

                return null;
            }

            for (var i = 0; i < numChildren; i++)
            {
                var recursiveResult = AdaptFromTreeRecursiveHelper(VisualTreeHelper.GetChild(root, i), depth - 1);
                if (recursiveResult != null)
                {
                    return recursiveResult;
                }
            }

            return null;
        }

        private static Size NormalizeVisualizerSize(Size visualizerSize)
        {
            var width = visualizerSize.Width;
            var height = visualizerSize.Height;
            return new Size(
                width <= 0 || double.IsNaN(width) ? 1 : width,
                height <= 0 || double.IsNaN(height) ? 1 : height);
        }

        private void CleanupScrollViewer()
        {
            if (_scrollViewer != null)
            {
                _scrollViewer.PreviewMouseLeftButtonDown -= OnPreviewMouseLeftButtonDown;
                _scrollViewer.PreviewMouseMove -= OnPreviewMouseMove;
                _scrollViewer.PreviewMouseLeftButtonUp -= OnPreviewMouseLeftButtonUp;
                _scrollViewer.LostMouseCapture -= OnLostMouseCapture;
                _scrollViewer.ScrollChanged -= OnScrollViewerScrollChanged;
            }

            if (_infoProvider != null)
            {
                _infoProvider.RefreshStarted -= OnRefreshStarted;
                _infoProvider.RefreshCompleted -= OnRefreshCompleted;
            }
        }
    }

    internal sealed class ScrollViewerIRefreshInfoProviderDefaultAnimationHandler : IAdapterAnimationHandler
    {
        private readonly RefreshPullDirection _pullDirection;

        internal ScrollViewerIRefreshInfoProviderDefaultAnimationHandler(RefreshPullDirection pullDirection)
        {
            _pullDirection = pullDirection;
        }

        public void InteractionTrackerAnimation(FrameworkElement refreshVisualizer, FrameworkElement infoProvider)
        {
            ResetTranslation(refreshVisualizer);
            ResetTranslation(infoProvider);
        }

        public void RefreshRequestedAnimation(FrameworkElement refreshVisualizer, FrameworkElement infoProvider, double executionRatio)
        {
            ApplyTranslation(refreshVisualizer, 0);
            ApplyTranslation(infoProvider, GetRefreshRequestedOffset(refreshVisualizer, executionRatio));
        }

        public void RefreshCompletedAnimation(FrameworkElement refreshVisualizer, FrameworkElement infoProvider)
        {
            ApplyTranslation(refreshVisualizer, GetRefreshCompletedOffset(refreshVisualizer));
            ResetTranslation(infoProvider);
        }

        private double GetRefreshRequestedOffset(FrameworkElement refreshVisualizer, double executionRatio)
        {
            return GetSignedOffset(refreshVisualizer, executionRatio);
        }

        private double GetRefreshCompletedOffset(FrameworkElement refreshVisualizer)
        {
            return -GetSignedOffset(refreshVisualizer, 1);
        }

        private double GetSignedOffset(FrameworkElement refreshVisualizer, double ratio)
        {
            if (refreshVisualizer == null)
            {
                return 0;
            }

            var size = IsOrientationVertical() ? refreshVisualizer.ActualHeight : refreshVisualizer.ActualWidth;
            var offset = size * ratio;
            return IsPullDirectionFar() ? -offset : offset;
        }

        private void ApplyTranslation(FrameworkElement element, double offset)
        {
            if (element == null)
            {
                return;
            }

            var transform = EnsureTranslateTransform(element);
            if (IsOrientationVertical())
            {
                transform.Y = offset;
            }
            else
            {
                transform.X = offset;
            }
        }

        private static void ResetTranslation(FrameworkElement element)
        {
            if (element == null)
            {
                return;
            }

            var transform = EnsureTranslateTransform(element);
            transform.X = 0;
            transform.Y = 0;
        }

        private static TranslateTransform EnsureTranslateTransform(FrameworkElement element)
        {
            if (element.RenderTransform is TranslateTransform translateTransform)
            {
                return translateTransform;
            }

            translateTransform = new TranslateTransform();
            element.RenderTransform = translateTransform;
            return translateTransform;
        }

        private bool IsOrientationVertical()
        {
            return _pullDirection == RefreshPullDirection.TopToBottom ||
                _pullDirection == RefreshPullDirection.BottomToTop;
        }

        private bool IsPullDirectionFar()
        {
            return _pullDirection == RefreshPullDirection.BottomToTop ||
                _pullDirection == RefreshPullDirection.RightToLeft;
        }
    }
}
