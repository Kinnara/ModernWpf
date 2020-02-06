using MahApps.Metro.Controls;
using System;
using System.Windows;
using System.Windows.Controls;

namespace MahAppsSample.ControlPages
{
    public partial class SimpleTimePickerPage : Page
    {
        public SimpleTimePickerPage()
        {
            InitializeComponent();
        }

        private void TimePicker_SelectedDateTimeChanged(object sender, TimePickerBaseSelectionChangedEventArgs<DateTime?> e)
        {
        }

        private void SetValueToNow_Click(object sender, RoutedEventArgs e)
        {
            TimePicker.SelectedDateTime = DateTime.Now;
        }

        private void ClearValue_Click(object sender, RoutedEventArgs e)
        {
            TimePicker.ClearValue(TimePickerBase.SelectedDateTimeProperty);
        }
    }
}
