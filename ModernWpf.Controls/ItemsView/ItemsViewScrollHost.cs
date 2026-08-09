using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    internal sealed class ItemsViewScrollHost : ScrollViewer
    {
        private IScrollController _verticalScrollController;
        private ScrollBarVisibility _originalVerticalScrollBarVisibility;
        private int _nextCorrelationId;

        public ItemsViewScrollHost()
        {
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            ScrollChanged += OnScrollChanged;
            SizeChanged += OnSizeChanged;
        }

        internal IScrollController VerticalScrollController
        {
            get => _verticalScrollController;
            set
            {
                if (ReferenceEquals(_verticalScrollController, value))
                {
                    return;
                }

                bool hadController = _verticalScrollController != null;
                DetachVerticalScrollController();
                _verticalScrollController = value;

                if (_verticalScrollController != null)
                {
                    if (!hadController)
                    {
                        _originalVerticalScrollBarVisibility = VerticalScrollBarVisibility;
                        VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
                    }

                    AttachVerticalScrollController();
                    UpdateVerticalScrollController();
                }
                else if (hadController)
                {
                    VerticalScrollBarVisibility = _originalVerticalScrollBarVisibility;
                }
            }
        }

        private void AttachVerticalScrollController()
        {
            _verticalScrollController.ScrollToRequested += OnScrollToRequested;
            _verticalScrollController.ScrollByRequested += OnScrollByRequested;
            _verticalScrollController.AddScrollVelocityRequested += OnAddScrollVelocityRequested;
        }

        private void DetachVerticalScrollController()
        {
            if (_verticalScrollController == null)
            {
                return;
            }

            _verticalScrollController.ScrollToRequested -= OnScrollToRequested;
            _verticalScrollController.ScrollByRequested -= OnScrollByRequested;
            _verticalScrollController.AddScrollVelocityRequested -= OnAddScrollVelocityRequested;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateVerticalScrollController();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _verticalScrollController?.SetIsScrollable(false);
        }

        private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            UpdateVerticalScrollController();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateVerticalScrollController();
        }

        private void OnScrollToRequested(
            IScrollController sender,
            ScrollControllerScrollToRequestedEventArgs args)
        {
            args.CorrelationId = GetNextCorrelationId();
            ScrollToVerticalOffset(ClampOffset(args.Offset));
            QueueCompletion(sender, args.CorrelationId);
        }

        private void OnScrollByRequested(
            IScrollController sender,
            ScrollControllerScrollByRequestedEventArgs args)
        {
            args.CorrelationId = GetNextCorrelationId();
            ScrollToVerticalOffset(ClampOffset(VerticalOffset + args.OffsetDelta));
            QueueCompletion(sender, args.CorrelationId);
        }

        private void OnAddScrollVelocityRequested(
            IScrollController sender,
            ScrollControllerAddScrollVelocityRequestedEventArgs args)
        {
            args.CorrelationId = GetNextCorrelationId();

            // WPF has no ScrollPresenter compositor velocity operation. Apply
            // one display-frame worth of the requested velocity so external
            // controllers retain deterministic keyboard and wheel behavior.
            ScrollToVerticalOffset(ClampOffset(VerticalOffset + (args.OffsetVelocity / 60.0)));
            QueueCompletion(sender, args.CorrelationId);
        }

        private void QueueCompletion(IScrollController controller, int correlationId)
        {
            Dispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new Action(() =>
                {
                    UpdateVerticalScrollController();
                    controller.NotifyRequestedScrollCompleted(correlationId);
                }));
        }

        private void UpdateVerticalScrollController()
        {
            IScrollController controller = _verticalScrollController;
            if (controller == null)
            {
                return;
            }

            double maximum = Math.Max(0, ScrollableHeight);
            double offset = ClampOffset(VerticalOffset);
            double viewport = Math.Max(0, ViewportHeight);

            controller.SetValues(0, maximum, offset, viewport);
            controller.SetIsScrollable(IsEnabled && maximum > 0);
        }

        private double ClampOffset(double offset)
        {
            if (double.IsNaN(offset))
            {
                return VerticalOffset;
            }

            return Math.Max(0, Math.Min(Math.Max(0, ScrollableHeight), offset));
        }

        private int GetNextCorrelationId()
        {
            do
            {
                _nextCorrelationId = unchecked(_nextCorrelationId + 1);
            }
            while (_nextCorrelationId == -1);

            return _nextCorrelationId;
        }
    }
}
