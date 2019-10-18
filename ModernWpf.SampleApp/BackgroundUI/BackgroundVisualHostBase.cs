using System;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using ModernWpf.SampleApp.Controls.Primitives;

namespace ModernWpf.SampleApp.Controls
{
    public abstract class BackgroundVisualHostBase : FrameworkElement
    {
        private readonly Dispatcher _dispatcher;
        private ThreadedVisualHelper _threadedHelper;
        private HostVisual _hostVisual;

        protected BackgroundVisualHostBase()
        {
        }

        protected BackgroundVisualHostBase(Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        protected override int VisualChildrenCount
        {
            get { return _hostVisual != null ? 1 : 0; }
        }

        public void InvalidateContent()
        {
            HideContentHelper();
            CreateContentHelper();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            InvalidateContent();
        }

        protected override Visual GetVisualChild(int index)
        {
            if (_hostVisual != null && index == 0)
                return _hostVisual;

            throw new IndexOutOfRangeException("index");
        }

        protected override System.Collections.IEnumerator LogicalChildren
        {
            get
            {
                if (_hostVisual != null)
                    yield return _hostVisual;
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            if (_threadedHelper != null)
            {
                _threadedHelper.Measure(availableSize);
                return _threadedHelper.DesiredSize;
            }

            return base.MeasureOverride(availableSize);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (_threadedHelper != null)
            {
                _threadedHelper.Arrange(new Rect(finalSize));
                return finalSize;
            }

            return base.ArrangeOverride(finalSize);
        }

        protected abstract Visual CreateContent();

        private void CreateContentHelper()
        {
            _threadedHelper = new ThreadedVisualHelper(CreateContent, SafeInvalidateMeasure, _dispatcher);
            _hostVisual = _threadedHelper.HostVisual;
            AddVisualChild(_hostVisual);
        }

        private void HideContentHelper()
        {
            if (_threadedHelper != null)
            {
                _threadedHelper.Exit();
                _threadedHelper = null;
                InvalidateMeasure();
            }
        }

        private void SafeInvalidateMeasure()
        {
            Dispatcher.BeginInvoke(new Action(InvalidateMeasure), DispatcherPriority.Loaded);
        }

        private class ThreadedVisualHelper
        {
            private readonly AutoResetEvent _sync = new AutoResetEvent(false);
            private readonly CreateContentFunction _createContent;
            private readonly Action _invalidateMeasure;
            private readonly bool _usingExternalDispatcher;
            private Visual _content;

            public HostVisual HostVisual { get; }

            public Size DesiredSize
            {
                get
                {
                    if (_content is UIElement element)
                    {
                        return element.DesiredSize;
                    }
                    return new Size(0, 0);
                }
            }

            private Dispatcher Dispatcher { get; set; }

            public ThreadedVisualHelper(
                CreateContentFunction createContent,
                Action invalidateMeasure,
                Dispatcher dispatcher = null)
            {
                HostVisual = new HostVisual();
                _createContent = createContent;
                _invalidateMeasure = invalidateMeasure;

                if (dispatcher != null)
                {
                    _usingExternalDispatcher = true;
                    Dispatcher = dispatcher;
                    Dispatcher.BeginInvoke((Action)CreateAndShowContent);
                }
                else
                {
                    Thread backgroundUI = new Thread(CreateAndShowContent)
                    {
                        Name = "BackgroundVisualHostThread",
                        IsBackground = true
                    };
                    backgroundUI.SetApartmentState(ApartmentState.STA);
                    backgroundUI.Start();
                }

                _sync.WaitOne();
            }

            public void Exit()
            {
                if (!_usingExternalDispatcher)
                {
                    Dispatcher?.BeginInvokeShutdown(DispatcherPriority.Send);
                }
            }

            public void Measure(Size availableSize)
            {
                if (_content is UIElement element)
                {
                    element.Dispatcher.Invoke(() =>
                    {
                        element.Measure(availableSize);
                    });
                }
            }

            public void Arrange(Rect finalRect)
            {
                if (_content is UIElement element)
                {
                    element.Dispatcher.Invoke(() =>
                    {
                        element.Arrange(finalRect);
                    });
                }
            }

            private void CreateAndShowContent()
            {
                if (Dispatcher == null)
                {
                    Dispatcher = Dispatcher.CurrentDispatcher;
                }

                VisualTargetPresentationSource source = new VisualTargetPresentationSource(HostVisual);
                _sync.Set();
                _content = _createContent();
                source.RootVisual = _content;
                _invalidateMeasure();

                if (!_usingExternalDispatcher)
                {
                    Dispatcher.Run();
                }

                source.Dispose();
            }
        }
    }
}
