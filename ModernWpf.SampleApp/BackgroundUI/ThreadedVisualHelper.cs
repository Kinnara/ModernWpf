using System;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace ModernWpf.SampleApp.Helpers
{
    public class ThreadedVisualHelper
    {
        private readonly AutoResetEvent _sync = new AutoResetEvent(false);
        private readonly Func<Visual> _createContent;
        private readonly Action _invalidateMeasure;

        public ThreadedVisualHelper(
            Func<Visual> createContent,
            Action invalidateMeasure)
        {
            HostVisual = new HostVisual();
            _createContent = createContent;
            _invalidateMeasure = invalidateMeasure;

            Thread backgroundUi = new Thread(CreateAndShowContent);
            backgroundUi.SetApartmentState(ApartmentState.STA);
            backgroundUi.Name = "BackgroundVisualHostThread";
            backgroundUi.IsBackground = true;
            backgroundUi.Start();

            _sync.WaitOne();
        }

        public HostVisual HostVisual { get; }
        public Size DesiredSize { get; private set; }
        private Dispatcher Dispatcher { get; set; }

        public void Exit()
        {
            Dispatcher.BeginInvokeShutdown(DispatcherPriority.Send);
        }

        private void CreateAndShowContent()
        {
            Dispatcher = Dispatcher.CurrentDispatcher;
            VisualTargetPresentationSource source =
                new VisualTargetPresentationSource(HostVisual);
            _sync.Set();
            source.RootVisual = _createContent();
            DesiredSize = source.DesiredSize;
            _invalidateMeasure();

            Dispatcher.Run();
            source.Dispose();
        }
    }
}
