using System;
using System.Reflection;
using System.Windows.Controls.Primitives;
using ModernWpf.Controls.Primitives;

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

            if (PopupPositioner.GetPositioner(popup) is { } positioner)
            {
                positioner.Reposition();
            }
            else
            {
                if (s_reposition.Value is { } reposition)
                {
                    reposition(popup);
                }
                else
                {
                    var offset = popup.HorizontalOffset;
                    popup.SetCurrentValue(Popup.HorizontalOffsetProperty, offset + 0.1);
                    popup.InvalidateProperty(Popup.HorizontalOffsetProperty);
                }
            }
        }

        private static Action<Popup> CreateRepositionDelegate()
        {
            try
            {
                return DelegateHelper.CreateDelegate<Action<Popup>>(
                    typeof(Popup),
                    nameof(Reposition),
                    BindingFlags.Instance | BindingFlags.NonPublic);
            }
            catch
            {
                return null;
            }
        }

        private static readonly Lazy<Action<Popup>> s_reposition = new Lazy<Action<Popup>>(CreateRepositionDelegate);
    }
}
