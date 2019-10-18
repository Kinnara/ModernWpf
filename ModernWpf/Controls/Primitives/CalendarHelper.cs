using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace ModernWpf.Controls.Primitives
{
    public static class CalendarHelper
    {
        #region ReleaseMouseCapture

        public static bool GetReleaseMouseCapture(Calendar calendar)
        {
            return (bool)calendar.GetValue(ReleaseMouseCaptureProperty);
        }

        public static void SetReleaseMouseCapture(Calendar calendar, bool value)
        {
            calendar.SetValue(ReleaseMouseCaptureProperty, value);
        }

        public static readonly DependencyProperty ReleaseMouseCaptureProperty = DependencyProperty.RegisterAttached(
            "ReleaseMouseCapture",
            typeof(bool),
            typeof(CalendarHelper),
            new PropertyMetadata(OnReleaseMouseCaptureChanged));

        private static void OnReleaseMouseCaptureChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
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
