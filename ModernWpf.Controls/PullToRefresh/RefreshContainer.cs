using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.Controls
{
    [TemplatePart(Name = RefreshVisualizerPresenterName, Type = typeof(Panel))]
    public class RefreshContainer : ContentControl
    {
        private const string RefreshVisualizerPresenterName = "RefreshVisualizerPresenter";
        private const double DefaultPullDimensionSize = 100;

        static RefreshContainer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RefreshContainer), new FrameworkPropertyMetadata(typeof(RefreshContainer)));
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
            }

            base.OnApplyTemplate();

            _refreshVisualizerPresenter = GetTemplateChild(RefreshVisualizerPresenterName) as Panel;

            if (Visualizer == null)
            {
                Visualizer = new RefreshVisualizer();
            }

            AttachVisualizer();
            UpdatePullDirection();
        }

        public void RequestRefresh()
        {
            Visualizer?.RequestRefresh();
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
            }

            _visualizer = newVisualizer;
            AttachVisualizer();
            UpdatePullDirection();
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

        private void OnVisualizerRefreshRequested(RefreshVisualizer sender, RefreshRequestedEventArgs args)
        {
            var visualizerDeferral = args.GetDeferral();
            var containerArgs = new RefreshRequestedEventArgs(visualizerDeferral.Complete);

            RefreshRequested?.Invoke(this, containerArgs);
            containerArgs.CompleteEvent();
        }

        private Panel _refreshVisualizerPresenter;
        private RefreshVisualizer _visualizer;
    }
}
