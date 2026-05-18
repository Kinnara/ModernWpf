using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ModernWpf.Controls
{
    [TemplatePart(Name = RootName, Type = typeof(Panel))]
    [TemplatePart(Name = RefreshVisualizerPresenterName, Type = typeof(Panel))]
    public partial class RefreshContainer : ContentControl
    {
        private const string RootName = "Root";
        private const string RefreshVisualizerPresenterName = "RefreshVisualizerPresenter";
        private const double DefaultPullDimensionSize = 100;
        private const int MaxBfsDepth = 10;

        static RefreshContainer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RefreshContainer), new FrameworkPropertyMetadata(typeof(RefreshContainer)));
        }

        public RefreshContainer()
        {
            RefreshInfoProviderAdapter = new ScrollViewerIRefreshInfoProviderAdapter(PullDirection);
            _hasDefaultRefreshInfoProviderAdapter = true;
        }

        internal IRefreshInfoProviderAdapter RefreshInfoProviderAdapter
        {
            get => _refreshInfoProviderAdapter;
            set
            {
                if (!ReferenceEquals(_refreshInfoProviderAdapter, value))
                {
                    _refreshInfoProviderAdapter?.Dispose();
                }

                _refreshInfoProviderAdapter = value;
                _hasDefaultRefreshInfoProviderAdapter = false;
                OnRefreshInfoProviderAdapterChanged();
            }
        }

        internal double PullRatioForTesting
        {
            get
            {
                EnsureRefreshInfoProvider();
                return (_refreshInfoProviderAdapter as ScrollViewerIRefreshInfoProviderAdapter)?.InteractionRatio ?? 0;
            }
        }

        internal bool CanStartPullForTesting
        {
            get
            {
                EnsureRefreshInfoProvider();
                return (_refreshInfoProviderAdapter as ScrollViewerIRefreshInfoProviderAdapter)?.CanStartPull ?? true;
            }
        }

        public event TypedEventHandler<RefreshContainer, RefreshRequestedEventArgs> RefreshRequested;

        public override void OnApplyTemplate()
        {
            if (_refreshVisualizer != null)
            {
                _refreshVisualizer.SizeChanged -= OnVisualizerSizeChanged;
                _refreshVisualizer.RefreshRequested -= OnVisualizerRefreshRequested;
            }

            base.OnApplyTemplate();

            _root = GetTemplateChild(RootName) as Panel;
            _refreshVisualizerPresenter = GetTemplateChild(RefreshVisualizerPresenterName) as Panel;

            _refreshVisualizer = Visualizer;
            if (_refreshVisualizer == null)
            {
                Visualizer = new RefreshVisualizer();
                _hasDefaultRefreshVisualizer = true;
            }
            else
            {
                OnRefreshVisualizerChangedImpl();
                _hasDefaultRefreshVisualizer = false;
            }

            _refreshPullDirection = PullDirection;
            OnPullDirectionChangedImpl();
            OnRefreshInfoProviderAdapterChanged();
        }

        public void RequestRefresh()
        {
            _refreshVisualizer?.RequestRefresh();
        }

        internal void PullForTesting(double delta, bool complete)
        {
            EnsureRefreshInfoProvider();
            (_refreshInfoProviderAdapter as ScrollViewerIRefreshInfoProviderAdapter)?.PullForTesting(delta, complete);
        }

        private static void OnVisualizerPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((RefreshContainer)d).OnRefreshVisualizerChanged(e.OldValue as RefreshVisualizer, e.NewValue as RefreshVisualizer);
        }

        private static void OnPullDirectionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((RefreshContainer)d).OnPullDirectionChanged((RefreshPullDirection)e.NewValue);
        }

        private void OnRefreshVisualizerChanged(RefreshVisualizer oldVisualizer, RefreshVisualizer newVisualizer)
        {
            if (oldVisualizer != null)
            {
                oldVisualizer.SizeChanged -= OnVisualizerSizeChanged;
                oldVisualizer.RefreshRequested -= OnVisualizerRefreshRequested;
            }

            _refreshVisualizer = newVisualizer;
            _hasDefaultRefreshVisualizer = false;
            OnRefreshVisualizerChangedImpl();
        }

        private void OnRefreshVisualizerChangedImpl()
        {
            if (_refreshVisualizerPresenter != null)
            {
                _refreshVisualizerPresenter.Children.Clear();
                if (_refreshVisualizer != null)
                {
                    _refreshVisualizerPresenter.Children.Add(_refreshVisualizer);
                }
            }

            if (_refreshVisualizer != null)
            {
                _refreshVisualizer.SizeChanged -= OnVisualizerSizeChanged;
                _refreshVisualizer.SizeChanged += OnVisualizerSizeChanged;
                _refreshVisualizer.RefreshRequested -= OnVisualizerRefreshRequested;
                _refreshVisualizer.RefreshRequested += OnVisualizerRefreshRequested;
            }

            OnRefreshInfoProviderAdapterChanged();
        }

        private void OnPullDirectionChanged(RefreshPullDirection value)
        {
            _refreshPullDirection = value;
            OnPullDirectionChangedImpl();

            if (_hasDefaultRefreshInfoProviderAdapter)
            {
                _refreshInfoProviderAdapter?.Dispose();
                _refreshInfoProviderAdapter = new ScrollViewerIRefreshInfoProviderAdapter(PullDirection);
                OnRefreshInfoProviderAdapterChanged();
            }
        }

        private void OnPullDirectionChangedImpl()
        {
            if (_refreshVisualizerPresenter == null)
            {
                return;
            }

            switch (_refreshPullDirection)
            {
                case RefreshPullDirection.LeftToRight:
                    _refreshVisualizerPresenter.VerticalAlignment = VerticalAlignment.Stretch;
                    _refreshVisualizerPresenter.HorizontalAlignment = HorizontalAlignment.Left;
                    if (_hasDefaultRefreshVisualizer)
                    {
                        SetDefaultVisualizerDirectionAndSize(RefreshPullDirection.LeftToRight, double.NaN, DefaultPullDimensionSize);
                    }
                    break;

                case RefreshPullDirection.RightToLeft:
                    _refreshVisualizerPresenter.VerticalAlignment = VerticalAlignment.Stretch;
                    _refreshVisualizerPresenter.HorizontalAlignment = HorizontalAlignment.Right;
                    if (_hasDefaultRefreshVisualizer)
                    {
                        SetDefaultVisualizerDirectionAndSize(RefreshPullDirection.RightToLeft, double.NaN, DefaultPullDimensionSize);
                    }
                    break;

                case RefreshPullDirection.BottomToTop:
                    _refreshVisualizerPresenter.VerticalAlignment = VerticalAlignment.Bottom;
                    _refreshVisualizerPresenter.HorizontalAlignment = HorizontalAlignment.Stretch;
                    if (_hasDefaultRefreshVisualizer)
                    {
                        SetDefaultVisualizerDirectionAndSize(RefreshPullDirection.BottomToTop, DefaultPullDimensionSize, double.NaN);
                    }
                    break;

                default:
                    _refreshVisualizerPresenter.VerticalAlignment = VerticalAlignment.Top;
                    _refreshVisualizerPresenter.HorizontalAlignment = HorizontalAlignment.Stretch;
                    if (_hasDefaultRefreshVisualizer)
                    {
                        SetDefaultVisualizerDirectionAndSize(RefreshPullDirection.TopToBottom, DefaultPullDimensionSize, double.NaN);
                    }
                    break;
            }
        }

        private void SetDefaultVisualizerDirectionAndSize(RefreshPullDirection pullDirection, double height, double width)
        {
            if (_refreshVisualizer == null)
            {
                return;
            }

            _refreshVisualizer.SetInternalPullDirection(pullDirection);
            _refreshVisualizer.Height = height;
            _refreshVisualizer.Width = width;
        }

        private void OnRefreshInfoProviderAdapterChanged()
        {
            if (_root == null || _refreshVisualizer == null)
            {
                return;
            }

            var firstChildAsInfoProvider = _root.Children.Count > 0 ? _root.Children[0] as IRefreshInfoProvider : null;
            if (firstChildAsInfoProvider != null)
            {
                _refreshVisualizer.InfoProvider = firstChildAsInfoProvider;
                return;
            }

            IRefreshInfoProvider providerFromAdapter = null;
            if (_refreshInfoProviderAdapter != null)
            {
                providerFromAdapter = _refreshInfoProviderAdapter.AdaptFromTree(_root, GetRefreshVisualizerSize());
                if (providerFromAdapter != null)
                {
                    _refreshVisualizer.InfoProvider = providerFromAdapter;
                    _refreshInfoProviderAdapter.SetAnimations(_refreshVisualizer);
                }
            }

            if (providerFromAdapter == null)
            {
                _refreshVisualizer.InfoProvider = SearchTreeForIRefreshInfoProvider();
            }
        }

        private IRefreshInfoProvider SearchTreeForIRefreshInfoProvider()
        {
            if (_root == null)
            {
                return null;
            }

            if (_root is IRefreshInfoProvider rootAsProvider)
            {
                return rootAsProvider;
            }

            for (var depth = 0; depth < MaxBfsDepth; depth++)
            {
                var result = SearchTreeForIRefreshInfoProviderRecursiveHelper(_root, depth);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        private static IRefreshInfoProvider SearchTreeForIRefreshInfoProviderRecursiveHelper(DependencyObject root, int depth)
        {
            var numChildren = root == null ? 0 : VisualTreeHelper.GetChildrenCount(root);
            if (depth == 0)
            {
                for (var i = 0; i < numChildren; i++)
                {
                    if (VisualTreeHelper.GetChild(root, i) is IRefreshInfoProvider provider)
                    {
                        return provider;
                    }
                }

                return null;
            }

            for (var i = 0; i < numChildren; i++)
            {
                var result = SearchTreeForIRefreshInfoProviderRecursiveHelper(VisualTreeHelper.GetChild(root, i), depth - 1);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        private void OnVisualizerSizeChanged(object sender, SizeChangedEventArgs args)
        {
            if (_hasDefaultRefreshInfoProviderAdapter)
            {
                _refreshInfoProviderAdapter?.Dispose();
                _refreshInfoProviderAdapter = new ScrollViewerIRefreshInfoProviderAdapter(PullDirection);
                OnRefreshInfoProviderAdapterChanged();
            }
        }

        private void OnVisualizerRefreshRequested(RefreshVisualizer sender, RefreshRequestedEventArgs args)
        {
            _visualizerRefreshCompletedDeferral = args.GetDeferral();
            RaiseRefreshRequested();
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
            _visualizerRefreshCompletedDeferral?.Complete();
            _visualizerRefreshCompletedDeferral = null;
        }

        private void EnsureRefreshInfoProvider()
        {
            if (_refreshVisualizer != null && _refreshVisualizer.InfoProvider == null)
            {
                OnRefreshInfoProviderAdapterChanged();
            }
        }

        private Size GetRefreshVisualizerSize()
        {
            var width = _refreshVisualizer.Width;
            var height = _refreshVisualizer.Height;

            if (width <= 0 || double.IsNaN(width))
            {
                width = _refreshVisualizer.RenderSize.Width;
            }

            if (height <= 0 || double.IsNaN(height))
            {
                height = _refreshVisualizer.RenderSize.Height;
            }

            if (width <= 0 || double.IsNaN(width))
            {
                width = DefaultPullDimensionSize;
            }

            if (height <= 0 || double.IsNaN(height))
            {
                height = DefaultPullDimensionSize;
            }

            return new Size(width, height);
        }

        private Panel _root;
        private Panel _refreshVisualizerPresenter;
        private RefreshVisualizer _refreshVisualizer;
        private RefreshDeferral _visualizerRefreshCompletedDeferral;
        private RefreshPullDirection _refreshPullDirection = RefreshPullDirection.TopToBottom;
        private IRefreshInfoProviderAdapter _refreshInfoProviderAdapter;
        private bool _hasDefaultRefreshVisualizer;
        private bool _hasDefaultRefreshInfoProviderAdapter;
    }
}
