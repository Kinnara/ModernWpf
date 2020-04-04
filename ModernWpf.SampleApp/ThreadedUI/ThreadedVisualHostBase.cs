using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace ModernWpf.SampleApp.ThreadedUI
{
    public abstract class ThreadedVisualHostBase : FrameworkElement
    {
        protected ThreadedVisualHostBase()
        {
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        protected override int VisualChildrenCount => 1;

        protected override System.Collections.IEnumerator LogicalChildren
        {
            get
            {
                yield return HostVisual;
            }
        }

        protected UIElement ChildInternal
        {
            get => _child;
            private set
            {
                if (_child != value)
                {
                    if (_child is FrameworkElement oldChildFE)
                    {
                        oldChildFE.SizeChanged -= OnChildSizeChanged;
                    }

                    _child = value;

                    if (_child is FrameworkElement newChildFE)
                    {
                        newChildFE.SizeChanged += OnChildSizeChanged;
                    }

                    OnChildChanged();

                    ChildChanged?.Invoke(this, EventArgs.Empty);

                    Dispatcher.BeginInvoke(InvalidateMeasure);
                }
            }
        }

        private HostVisual HostVisual
        {
            get
            {
                if (_hostVisual == null)
                {
                    _hostVisual = new HostVisual();
                    AddVisualChild(_hostVisual);
                    AddLogicalChild(_hostVisual);
                }
                return _hostVisual;
            }
        }

        public event EventHandler ChildChanged;

        protected override Size MeasureOverride(Size availableSize)
        {
            if (ChildInternal == null && IsLoaded)
            {
                LoadChild(availableSize);
            }

            var child = ChildInternal;
            if (child != null)
            {
                return child.Dispatcher.Invoke(() =>
                {
                    child.Measure(availableSize);
                    return child.DesiredSize;
                });
            }

            return base.MeasureOverride(availableSize);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var child = ChildInternal;
            if (child != null)
            {
                child.Dispatcher.Invoke(() =>
                {
                    child.Arrange(new Rect(finalSize));
                });
                return finalSize;
            }

            return base.ArrangeOverride(finalSize);
        }

        protected override Visual GetVisualChild(int index)
        {
            if (index != 0)
            {
                throw new IndexOutOfRangeException(nameof(index));
            }
            return HostVisual;
        }

        protected virtual void OnChildChanged()
        {
        }

        protected void InvalidateChild()
        {
            if (ChildInternal != null)
            {
                UnloadChild();
            }
            else
            {
                InvalidateMeasure();
            }
        }

        protected abstract UIElement CreateChild();

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (ChildInternal == null)
            {
                InvalidateMeasure();
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            UnloadChild();

            Debug.Assert(ChildInternal == null);
        }

        private void LoadChild(Size availableSize)
        {
            if (ChildInternal != null)
                return;

            _ = HostVisual;

            var thread = new Thread(ThreadProc)
            {
                IsBackground = true,
                Name = "Threaded Visual Host Thread"
            };
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            _sync.WaitOne();

            void ThreadProc()
            {
                var source = new VisualTargetPresentationSource(HostVisual);

                var child = CreateChild();
                if (child != null)
                {
                    source.RootVisual = child;
                    child.Measure(availableSize);
                    child.Arrange(new Rect(child.DesiredSize));
                }

                ChildInternal = child;

                _sync.Set();

                Dispatcher.Run();

                source.Dispose();
                source = null;
            }
        }

        private void UnloadChild()
        {
            var child = ChildInternal;
            if (child != null)
            {
                child.Dispatcher.Invoke(() => ChildInternal = null);
                child.Dispatcher.InvokeShutdown();
                InvalidateMeasure();
            }
        }

        private void OnChildSizeChanged(object sender, SizeChangedEventArgs e)
        {
            Dispatcher.BeginInvoke(InvalidateMeasure);
        }

        private HostVisual _hostVisual;
        private UIElement _child;
        private readonly AutoResetEvent _sync = new AutoResetEvent(false);
    }
}
