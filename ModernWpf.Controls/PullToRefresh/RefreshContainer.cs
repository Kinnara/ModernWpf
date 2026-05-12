using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ModernWpf.Controls
{
    [TemplatePart(Name = RefreshVisualizerPresenterName, Type = typeof(Panel))]
    public class RefreshContainer : ContentControl
    {
        private const string RefreshVisualizerPresenterName = "RefreshVisualizerPresenter";
        private const double DefaultPullDimensionSize = 100;
        private const double PullStartThreshold = 8;
        private const double PullExecutionThreshold = 80;

        static RefreshContainer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RefreshContainer), new FrameworkPropertyMetadata(typeof(RefreshContainer)));
        }

        public RefreshContainer()
        {
            PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
            PreviewMouseMove += OnPreviewMouseMove;
            PreviewMouseLeftButtonUp += OnPreviewMouseLeftButtonUp;
            LostMouseCapture += OnLostMouseCapture;
        }

        public static readonly DependencyProperty VisualizerProperty =
            DependencyProperty.Register(
                nameof(Visualizer),
                typeof(RefreshVisualizer),
                typeof(RefreshContainer),
                new PropertyMetadata(null, OnVisualizerPropertyChanged));

        public RefreshVisualizer Visualizer
        {
            get => (RefreshVisualizer)GetValue(VisualizerProperty);
            set => SetValue(VisualizerProperty, value);
        }

        public static readonly DependencyProperty PullDirectionProperty =
            DependencyProperty.Register(
                nameof(PullDirection),
                typeof(RefreshPullDirection),
                typeof(RefreshContainer),
                new PropertyMetadata(RefreshPullDirection.TopToBottom, OnPullDirectionPropertyChanged));

        public RefreshPullDirection PullDirection
        {
            get => (RefreshPullDirection)GetValue(PullDirectionProperty);
            set => SetValue(PullDirectionProperty, value);
        }

        public event TypedEventHandler<RefreshContainer, RefreshRequestedEventArgs> RefreshRequested;

        public override void OnApplyTemplate()
        {
            if (_visualizer != null)
            {
                _visualizer.RefreshRequested -= OnVisualizerRefreshRequested;
                _visualizer.RefreshStateChanged -= OnVisualizerRefreshStateChanged;
            }

            base.OnApplyTemplate();

            _refreshVisualizerPresenter = GetTemplateChild(RefreshVisualizerPresenterName) as Panel;

            if (Visualizer == null)
            {
                Visualizer = new RefreshVisualizer();
            }

            AttachVisualizer();
            UpdatePullDirection();
            UpdateVisualizerPresenterState();
        }

        public void RequestRefresh()
        {
            Visualizer?.RequestRefresh();
        }

        internal double PullRatioForTesting => _pullRatio;

        internal bool CanStartPullForTesting => IsAtPullBoundary();

        internal void PullForTesting(double delta, bool complete)
        {
            StartPull(new Point());
            UpdatePull(delta);

            if (complete)
            {
                CompletePull();
            }
        }

        private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            StartPull(e.GetPosition(this));
        }

        private void OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!_isPointerDown)
            {
                return;
            }

            var position = e.GetPosition(this);
            var delta = GetDirectedDelta(position.X - _pullStartPoint.X, position.Y - _pullStartPoint.Y);
            UpdatePull(delta);

            if (_isPulling)
            {
                e.Handled = true;
            }
        }

        private void OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_isPointerDown)
            {
                return;
            }

            ReleaseMouseCapture();
            CompletePull();
        }

        private void OnLostMouseCapture(object sender, MouseEventArgs e)
        {
            if (_isPointerDown)
            {
                CompletePull();
            }
        }

        private void StartPull(Point position)
        {
            _isPointerDown = IsAtPullBoundary();
            _isPulling = false;
            _pullStartPoint = position;
        }

        private void UpdatePull(double delta)
        {
            if (!_isPointerDown || delta <= 0 || Visualizer == null)
            {
                return;
            }

            if (!_isPulling)
            {
                if (delta < PullStartThreshold)
                {
                    return;
                }

                _isPulling = true;
                CaptureMouse();
            }

            _pullRatio = Math.Min(1, delta / PullExecutionThreshold);
            Visualizer.UpdatePullProgress(_pullRatio);
            UpdateVisualizerPresenterState();
        }

        private void CompletePull()
        {
            var shouldRefresh = _isPulling && _pullRatio >= 1;

            _isPointerDown = false;
            _isPulling = false;
            _pullRatio = 0;

            if (shouldRefresh)
            {
                RequestRefresh();
            }
            else
            {
                Visualizer?.UpdatePullProgress(0);
            }

            UpdateVisualizerPresenterState();
        }

        private double GetDirectedDelta(double horizontalDelta, double verticalDelta)
        {
            switch (PullDirection)
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

        private static void OnVisualizerPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((RefreshContainer)d).OnVisualizerChanged(e.OldValue as RefreshVisualizer, e.NewValue as RefreshVisualizer);
        }

        private static void OnPullDirectionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((RefreshContainer)d).UpdatePullDirection();
        }

        private void OnVisualizerChanged(RefreshVisualizer oldVisualizer, RefreshVisualizer newVisualizer)
        {
            if (oldVisualizer != null)
            {
                oldVisualizer.RefreshRequested -= OnVisualizerRefreshRequested;
                oldVisualizer.RefreshStateChanged -= OnVisualizerRefreshStateChanged;
            }

            _visualizer = newVisualizer;
            AttachVisualizer();
            UpdatePullDirection();
            UpdateVisualizerPresenterState();
        }

        private void AttachVisualizer()
        {
            if (_refreshVisualizerPresenter != null)
            {
                _refreshVisualizerPresenter.Children.Clear();
                if (_visualizer != null)
                {
                    _refreshVisualizerPresenter.Children.Add(_visualizer);
                }
            }

            if (_visualizer != null)
            {
                _visualizer.RefreshRequested -= OnVisualizerRefreshRequested;
                _visualizer.RefreshRequested += OnVisualizerRefreshRequested;
                _visualizer.RefreshStateChanged -= OnVisualizerRefreshStateChanged;
                _visualizer.RefreshStateChanged += OnVisualizerRefreshStateChanged;
            }
        }

        private void UpdatePullDirection()
        {
            if (_refreshVisualizerPresenter == null)
            {
                return;
            }

            switch (PullDirection)
            {
                case RefreshPullDirection.LeftToRight:
                    _refreshVisualizerPresenter.HorizontalAlignment = HorizontalAlignment.Left;
                    _refreshVisualizerPresenter.VerticalAlignment = VerticalAlignment.Stretch;
                    SetVisualizerSize(width: DefaultPullDimensionSize, height: double.NaN);
                    break;

                case RefreshPullDirection.RightToLeft:
                    _refreshVisualizerPresenter.HorizontalAlignment = HorizontalAlignment.Right;
                    _refreshVisualizerPresenter.VerticalAlignment = VerticalAlignment.Stretch;
                    SetVisualizerSize(width: DefaultPullDimensionSize, height: double.NaN);
                    break;

                case RefreshPullDirection.BottomToTop:
                    _refreshVisualizerPresenter.HorizontalAlignment = HorizontalAlignment.Stretch;
                    _refreshVisualizerPresenter.VerticalAlignment = VerticalAlignment.Bottom;
                    SetVisualizerSize(width: double.NaN, height: DefaultPullDimensionSize);
                    break;

                default:
                    _refreshVisualizerPresenter.HorizontalAlignment = HorizontalAlignment.Stretch;
                    _refreshVisualizerPresenter.VerticalAlignment = VerticalAlignment.Top;
                    SetVisualizerSize(width: double.NaN, height: DefaultPullDimensionSize);
                    break;
            }
        }

        private void SetVisualizerSize(double width, double height)
        {
            if (_visualizer != null)
            {
                _visualizer.Width = width;
                _visualizer.Height = height;
            }
        }

        private bool IsAtPullBoundary()
        {
            var scrollViewer = FindScrollViewer(Content as DependencyObject);
            if (scrollViewer == null)
            {
                return true;
            }

            switch (PullDirection)
            {
                case RefreshPullDirection.LeftToRight:
                    return scrollViewer.HorizontalOffset <= 0;
                case RefreshPullDirection.RightToLeft:
                    return scrollViewer.HorizontalOffset >= scrollViewer.ScrollableWidth;
                case RefreshPullDirection.BottomToTop:
                    return scrollViewer.VerticalOffset >= scrollViewer.ScrollableHeight;
                default:
                    return scrollViewer.VerticalOffset <= 0;
            }
        }

        private static ScrollViewer FindScrollViewer(DependencyObject root)
        {
            if (root == null)
            {
                return null;
            }

            if (root is ScrollViewer scrollViewer)
            {
                return scrollViewer;
            }

            var childCount = VisualTreeHelper.GetChildrenCount(root);
            for (var i = 0; i < childCount; i++)
            {
                var match = FindScrollViewer(VisualTreeHelper.GetChild(root, i));
                if (match != null)
                {
                    return match;
                }
            }

            return null;
        }

        private void OnVisualizerRefreshStateChanged(RefreshVisualizer sender, RefreshStateChangedEventArgs args)
        {
            UpdateVisualizerPresenterState();
        }

        private void UpdateVisualizerPresenterState()
        {
            if (_refreshVisualizerPresenter == null || _visualizer == null)
            {
                return;
            }

            _refreshVisualizerPresenter.Opacity =
                _visualizer.State == RefreshVisualizerState.Idle ? _pullRatio : 1;
        }

        private void OnVisualizerRefreshRequested(RefreshVisualizer sender, RefreshRequestedEventArgs args)
        {
            var visualizerDeferral = args.GetDeferral();
            var containerArgs = new RefreshRequestedEventArgs(visualizerDeferral.Complete);

            RefreshRequested?.Invoke(this, containerArgs);
            containerArgs.CompleteEvent();
        }

        private Panel _refreshVisualizerPresenter;
        private RefreshVisualizer _visualizer;
        private Point _pullStartPoint;
        private double _pullRatio;
        private bool _isPointerDown;
        private bool _isPulling;
    }
}
