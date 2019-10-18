using System.Windows;
using System.Windows.Media;

namespace ModernWpf.SampleApp.Controls.Primitives
{
    public class VisualTargetPresentationSource : PresentationSource
    {
        private readonly VisualTarget _visualTarget;
        private bool _isDisposed = false;

        public VisualTargetPresentationSource(HostVisual hostVisual)
        {
            _visualTarget = new VisualTarget(hostVisual);
            AddSource();
        }

        public override Visual RootVisual
        {
            get => _visualTarget.RootVisual;
            set
            {
                Visual oldRoot = _visualTarget.RootVisual;

                // Set the root visual of the VisualTarget.  This visual will
                // now be used to visually compose the scene.
                _visualTarget.RootVisual = value;

                // Tell the PresentationSource that the root visual has
                // changed.  This kicks off a bunch of stuff like the
                // Loaded event.
                RootChanged(oldRoot, value);

                // Kickoff layout...
                if (value is UIElement rootElement)
                {
                    rootElement.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    rootElement.Arrange(new Rect(rootElement.DesiredSize));
                }
            }
        }

        protected override CompositionTarget GetCompositionTargetCore()
        {
            return _visualTarget;
        }

        public override bool IsDisposed => _isDisposed;

        internal void Dispose()
        {
            RemoveSource();
            _isDisposed = true;
        }
    }
}
