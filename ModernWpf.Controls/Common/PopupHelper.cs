using System;
using System.Reflection;
using System.Windows.Controls.Primitives;

namespace ModernWpf.Controls
{
    internal static class PopupHelper
    {
        public static void Reposition(this Popup popup)
        {
            if (popup is null)
            {
                throw new ArgumentNullException(nameof(popup));
            }

            if (s_repositionDelegate == null && !s_failedToCreateRepositionDelegate)
            {
                try
                {
                    var method = typeof(Popup).GetMethod("Reposition", BindingFlags.Instance | BindingFlags.NonPublic);
                    s_repositionDelegate = (Action<Popup>)Delegate.CreateDelegate(typeof(Action<Popup>), method);
                }
                catch (Exception)
                {
                    s_failedToCreateRepositionDelegate = true;
                }
            }

            if (s_repositionDelegate != null)
            {
                s_repositionDelegate(popup);
            }
            else
            {
                var offset = popup.HorizontalOffset;
                popup.SetCurrentValue(Popup.HorizontalOffsetProperty, offset + 0.1);
                popup.SetCurrentValue(Popup.HorizontalOffsetProperty, offset);
            }
        }

        private static Action<Popup> s_repositionDelegate;
        private static bool s_failedToCreateRepositionDelegate;
    }
}
