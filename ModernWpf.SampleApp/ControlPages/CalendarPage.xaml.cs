using System.Windows;

namespace ModernWpf.SampleApp.ControlPages
{
    public partial class CalendarPage
    {
        public CalendarPage()
        {
            InitializeComponent();
        }

        private void AddDatesInPastToBlackoutDates(object sender, RoutedEventArgs e)
        {
            calendar.BlackoutDates.AddDatesInPast();
        }
    }
}
