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
            IsVisibleChanged += OnIsVisibleChanged;
            Unloaded += OnUnloaded;
        }

        protected override int VisualChildrenCount => _hostVisual != null ? 1 : 0;

        protected override System.Collections.IEnumerator LogicalChildren
        {
            get
            {
                if (_hostVisual != null)
                {
                    yield return _hostVisual;
                }
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

        public event EventHandler ChildChanged;

        protected override Size MeasureOverride(Size availableSize)
        {
            if (ChildInternal == null && IsVisible)
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
            if (index != 0 || _hostVisual == null)
            {
                throw new IndexOutOfRangeException(nameof(index));
            }
            return _hostVisual;
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

        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                if (ChildInternal == null)
                {
                    InvalidateMeasure();
                }
            }
            else if (!IsLoaded)
            {
                UnloadChild();
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            UnloadChild();

            Debug.Assert(ChildInternal == null);
            Debug.Assert(!IsVisible);
        }

        private void AddHostVisual()
        {
            if (_hostVisual == null)
            {
                _hostVisual = new HostVisual();
                AddVisualChild(_hostVisual);
                AddLogicalChild(_hostVisual);
            }
        }

        private void RemoveHostVisual()
        {
            if (_hostVisual != null)
            {
                RemoveVisualChild(_hostVisual);
                RemoveLogicalChild(_hostVisual);
                _hostVisual = null;
            }
        }

        private void LoadChild(Size availableSize)
        {
            if (ChildInternal != null)
                return;

            AddHostVisual();

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
                var source = new VisualTargetPresentationSource(_hostVisual);

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

            RemoveHostVisual();
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
