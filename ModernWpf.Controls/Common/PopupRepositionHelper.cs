using System;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace ModernWpf.Controls
{
    internal class PopupRepositionHelper : IDisposable
    {
        public PopupRepositionHelper(Popup popup, UIElement target)
        {
            m_popup = popup;
            m_target = target;

            m_popup.Opened += OnPopupOpened;
            m_popup.Closed += OnPopupClosed;
        }

        public void Dispose()
        {
            m_popup.Opened -= OnPopupOpened;
            m_popup.Closed -= OnPopupClosed;
            m_target.LayoutUpdated -= OnTargetLayoutUpdated;
            OnPopupClosed(null, null);
        }

        private void OnPopupOpened(object sender, EventArgs e)
        {
            if (m_isPopupOpen)
            {
                return;
            }

            m_isPopupOpen = true;

            m_target.LayoutUpdated += OnTargetLayoutUpdated;

            m_hostWindow = Window.GetWindow(m_target);
            if (m_hostWindow != null)
            {
                m_hostWindow.LocationChanged += OnHostWindowLocationChanged;
            }
        }

        private void OnPopupClosed(object sender, EventArgs e)
        {
            m_target.LayoutUpdated -= OnTargetLayoutUpdated;

            if (m_hostWindow != null)
            {
                m_hostWindow.LocationChanged -= OnHostWindowLocationChanged;
                m_hostWindow = null;
            }

            m_isPopupOpen = false;
            m_popupPosition = default;
        }

        private void OnTargetLayoutUpdated(object sender, EventArgs e)
        {
            RepositionPopup();
        }

        private void OnHostWindowLocationChanged(object sender, EventArgs e)
        {
            RepositionPopup();
        }

        private void RepositionPopup()
        {
            if (m_popup != null)
            {
                var child = m_popup.Child;
                if (child != null)
                {
                    if (child.IsVisible && m_target.IsVisible)
                    {
                        var position = child.TranslatePoint(new Point(), m_target);
                        if (m_popupPosition != position)
                        {
                            m_popupPosition = position;
                            m_popup.Reposition();
                        }
                    }
                }
            }
        }

        private readonly Popup m_popup;
        private readonly UIElement m_target;
        private bool m_isPopupOpen;
        private Point m_popupPosition;
        private Window m_hostWindow;
    }
}
