using System;
using System.Windows;
using System.Windows.Media;

namespace ModernWpf.SampleApp.ThreadedUI
{
    internal class VisualTargetPresentationSource : PresentationSource, IDisposable
    {
        public VisualTargetPresentationSource(HostVisual hostVisual)
        {
            _visualTarget = new VisualTarget(hostVisual);
            AddSource();
        }

        public override Visual RootVisual
        {
            get => _visualTarget?.RootVisual;
            set
            {
                CheckDisposed();

                RootVisualInternal = value;
            }
        }

        private Visual RootVisualInternal
        {
            set
            {
                if (_visualTarget.RootVisual != value)
                {
                    Visual oldRoot = _visualTarget.RootVisual;

                    _visualTarget.RootVisual = value;

                    RootChanged(oldRoot, value);
                }
            }
        }

        protected override CompositionTarget GetCompositionTargetCore()
        {
            return _isDisposed ? null : _visualTarget;
        }

        #region IDisposable Support

        public override bool IsDisposed => _isDisposed;

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    RemoveSource();

                    if (_visualTarget != null)
                    {
                        _visualTarget.Dispose();
                        _visualTarget = null;
                    }
                }

                _isDisposed = true;
            }
        }

        private void CheckDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(ToString());
            }
        }

        private bool _isDisposed;

        #endregion

        private VisualTarget _visualTarget;
    }
}
