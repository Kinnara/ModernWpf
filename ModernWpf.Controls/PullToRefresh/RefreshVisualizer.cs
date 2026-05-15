using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.Controls
{
    public class RefreshVisualizer : Control
    {
        private const string RootName = "Root";
        private const double DefaultIndicatorSize = 30;

        private static readonly DependencyPropertyKey StatePropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(State),
                typeof(RefreshVisualizerState),
                typeof(RefreshVisualizer),
                new PropertyMetadata(RefreshVisualizerState.Idle));

        public static readonly DependencyProperty StateProperty = StatePropertyKey.DependencyProperty;

        static RefreshVisualizer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RefreshVisualizer), new FrameworkPropertyMetadata(typeof(RefreshVisualizer)));
        }

        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register(
                nameof(Orientation),
                typeof(RefreshVisualizerOrientation),
                typeof(RefreshVisualizer),
                new PropertyMetadata(RefreshVisualizerOrientation.Auto));

        public RefreshVisualizerOrientation Orientation
        {
            get => (RefreshVisualizerOrientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register(
                nameof(Content),
                typeof(UIElement),
                typeof(RefreshVisualizer),
                new PropertyMetadata(null, OnContentChanged));

        public UIElement Content
        {
            get => (UIElement)GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

        public RefreshVisualizerState State => (RefreshVisualizerState)GetValue(StateProperty);

        public event TypedEventHandler<RefreshVisualizer, RefreshRequestedEventArgs> RefreshRequested;

        public event TypedEventHandler<RefreshVisualizer, RefreshStateChangedEventArgs> RefreshStateChanged;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_root != null)
            {
                _root.Children.Clear();
            }

            _root = GetTemplateChild(RootName) as Panel;

            if (Content == null)
            {
                SetCurrentValue(ContentProperty, CreateDefaultContent());
            }
            else
            {
                UpdateContent();
            }
        }

        public void RequestRefresh()
        {
            if (State == RefreshVisualizerState.Refreshing)
            {
                return;
            }

            SetState(RefreshVisualizerState.Refreshing);

            var args = new RefreshRequestedEventArgs(CompleteRefresh);
            try
            {
                RefreshRequested?.Invoke(this, args);
            }
            finally
            {
                args.CompleteEvent();
            }
        }

        private void CompleteRefresh()
        {
            if (Dispatcher.CheckAccess())
            {
                SetState(RefreshVisualizerState.Idle);
            }
            else
            {
                Dispatcher.BeginInvoke(new System.Action(() => SetState(RefreshVisualizerState.Idle)));
            }
        }

        internal void UpdatePullProgress(double executionRatio)
        {
            if (State == RefreshVisualizerState.Refreshing)
            {
                return;
            }

            if (executionRatio <= 0)
            {
                SetState(RefreshVisualizerState.Idle);
            }
            else if (executionRatio >= 1)
            {
                SetState(RefreshVisualizerState.Pending);
            }
            else if (executionRatio >= 0.5)
            {
                SetState(RefreshVisualizerState.Interacting);
            }
            else
            {
                SetState(RefreshVisualizerState.Peeking);
            }
        }

        private void SetState(RefreshVisualizerState newState)
        {
            var oldState = State;
            if (oldState == newState)
            {
                return;
            }

            SetValue(StatePropertyKey, newState);
            RefreshStateChanged?.Invoke(this, new RefreshStateChangedEventArgs(oldState, newState));
        }

        private static void OnContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((RefreshVisualizer)d).UpdateContent();
        }

        private void UpdateContent()
        {
            if (_root == null)
            {
                return;
            }

            _root.Children.Clear();

            var content = Content ?? CreateDefaultContent();
            if (content is FrameworkElement frameworkElement)
            {
                frameworkElement.HorizontalAlignment = HorizontalAlignment.Center;
                frameworkElement.VerticalAlignment = VerticalAlignment.Center;
            }

            _root.Children.Add(content);
        }

        private static SymbolIcon CreateDefaultContent()
        {
            return new SymbolIcon(Symbol.Refresh)
            {
                Width = DefaultIndicatorSize,
                Height = DefaultIndicatorSize
            };
        }

        private Panel _root;
    }
}
