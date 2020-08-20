using System;
using System.Windows;
using System.Windows.Input;

namespace ModernWpf.Input
{
    internal class GettingFocusHelper : IDisposable
    {
        public GettingFocusHelper(UIElement owner)
        {
            _owner = owner;
            _owner.PreviewGotKeyboardFocus += OnPreviewGotKeyboardFocus;
        }

        public void Dispose()
        {
            if (_owner != null)
            {
                _owner.PreviewGotKeyboardFocus -= OnPreviewGotKeyboardFocus;
                _owner = null;
            }
        }

        public event TypedEventHandler<UIElement, GettingFocusEventArgs> GettingFocus;

        private void OnPreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (_ignoreGotFocus)
            {
                return;
            }

            var gettingFocus = GettingFocus;
            if (gettingFocus != null)
            {
                try
                {
                    _ignoreGotFocus = true;

                    var args = new GettingFocusEventArgs(e);

                    gettingFocus(sender as UIElement, args);

                    if (args.Cancel)
                    {
                        e.Handled = true;
                    }
                }
                finally
                {
                    _ignoreGotFocus = false;
                }
            }
        }

        private UIElement _owner;
        private bool _ignoreGotFocus;
    }
}
