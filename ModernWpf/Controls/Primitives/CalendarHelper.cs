using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace ModernWpf.Controls.Primitives
{
    public static class CalendarHelper
    {
        #region AutoReleaseMouseCapture

        public static bool GetAutoReleaseMouseCapture(Calendar calendar)
        {
            return (bool)calendar.GetValue(AutoReleaseMouseCaptureProperty);
        }

        public static void SetAutoReleaseMouseCapture(Calendar calendar, bool value)
        {
            calendar.SetValue(AutoReleaseMouseCaptureProperty, value);
        }

        public static readonly DependencyProperty AutoReleaseMouseCaptureProperty = DependencyProperty.RegisterAttached(
            "AutoReleaseMouseCapture",
            typeof(bool),
            typeof(CalendarHelper),
            new PropertyMetadata(OnAutoReleaseMouseCaptureChanged));

        private static void OnAutoReleaseMouseCaptureChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var calendar = (Calendar)d;
            if ((bool)e.NewValue)
            {
                calendar.GotMouseCapture += OnCalendarGotMouseCapture;
            }
            else
            {
                calendar.GotMouseCapture -= OnCalendarGotMouseCapture;
            }
        }

        #endregion

        private static void OnCalendarGotMouseCapture(object sender, MouseEventArgs e)
        {
            var calendar = (Calendar)sender;
            if (calendar.SelectionMode != CalendarSelectionMode.MultipleRange)
            {
                UIElement originalElement = e.OriginalSource as UIElement;
                if (originalElement is CalendarDayButton || originalElement is CalendarItem)
                {
                    originalElement.ReleaseMouseCapture();
                }
            }
        }
    }
}
