using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using ModernWpf.Controls.Primitives;
using Calendar = System.Windows.Controls.Calendar;

namespace ModernWpf.SampleApp.Pages
{
    internal static class DateTimeSampleFactory
    {
        public static UIElement Create(string uniqueId)
        {
            switch (uniqueId)
            {
                case "CalendarDatePicker":
                    return CreateCalendarDatePickerSample();
                case "CalendarView":
                    return CreateCalendarViewSample();
                case "DatePicker":
                    return CreateDatePickerSample();
                case "TimePicker":
                    return CreateTimePickerSample();
                default:
                    return null;
            }
        }

        private static UIElement CreateCalendarDatePickerSample()
        {
            var panel = CreateSamplePanel("CalendarDatePicker maps to WPF DatePicker with ModernWpf header and placeholder styling.");
            var output = CreateOutput("No date selected.");
            var picker = new DatePicker
            {
                Width = 260,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            ControlHelper.SetHeader(picker, "Calendar");
            ControlHelper.SetPlaceholderText(picker, "Pick a date");
            picker.SelectedDateChanged += delegate
            {
                output.Text = FormatSelectedDate("Selected date", picker.SelectedDate);
            };

            panel.Children.Add(picker);
            panel.Children.Add(output);
            return panel;
        }

        private static UIElement CreateCalendarViewSample()
        {
            var panel = CreateSamplePanel("CalendarView maps to WPF Calendar with ModernWpf calendar resources and selectable modes.");
            var calendar = new Calendar
            {
                SelectionMode = CalendarSelectionMode.SingleDate,
                IsTodayHighlighted = true,
                VerticalAlignment = VerticalAlignment.Top
            };
            var output = CreateOutput("No date selected.");
            calendar.SelectedDatesChanged += delegate
            {
                output.Text = CreateCalendarSelectionSummary(calendar);
            };

            var options = new StackPanel
            {
                Width = 240,
                Margin = new Thickness(24, 0, 0, 0)
            };
            options.Children.Add(new TextBlock
            {
                Text = "Options",
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 8)
            });

            var today = new CheckBox
            {
                Content = "Highlight today",
                IsChecked = calendar.IsTodayHighlighted
            };
            today.Checked += delegate { calendar.IsTodayHighlighted = true; };
            today.Unchecked += delegate { calendar.IsTodayHighlighted = false; };
            options.Children.Add(today);

            var mode = new ComboBox
            {
                Margin = new Thickness(0, 12, 0, 0),
                ItemsSource = new[]
                {
                    CalendarSelectionMode.None,
                    CalendarSelectionMode.SingleDate,
                    CalendarSelectionMode.SingleRange,
                    CalendarSelectionMode.MultipleRange
                },
                SelectedItem = calendar.SelectionMode
            };
            ControlHelper.SetHeader(mode, "SelectionMode");
            mode.SelectionChanged += delegate
            {
                if (mode.SelectedItem is CalendarSelectionMode)
                {
                    calendar.SelectionMode = (CalendarSelectionMode)mode.SelectedItem;
                    output.Text = CreateCalendarSelectionSummary(calendar);
                }
            };
            options.Children.Add(mode);

            var row = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };
            row.Children.Add(calendar);
            row.Children.Add(options);

            panel.Children.Add(row);
            panel.Children.Add(output);
            return panel;
        }

        private static UIElement CreateDatePickerSample()
        {
            var panel = CreateSamplePanel("DatePicker lets users set a date from a compact picker.");
            var standardOutput = CreateOutput("Standard date: not selected.");
            var standard = new DatePicker
            {
                Width = 260,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            ControlHelper.SetHeader(standard, "Pick a date");
            standard.SelectedDateChanged += delegate
            {
                standardOutput.Text = FormatSelectedDate("Standard date", standard.SelectedDate);
            };

            var monthDayOutput = CreateOutput("Month/day selection: not selected.");
            var monthDay = new DatePicker
            {
                Width = 260,
                Margin = new Thickness(0, 18, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            ControlHelper.SetHeader(monthDay, "Month and day");
            monthDay.SelectedDateChanged += delegate
            {
                if (monthDay.SelectedDate.HasValue)
                {
                    monthDayOutput.Text = "Month/day selection: " + monthDay.SelectedDate.Value.ToString("MMM d (ddd)", CultureInfo.CurrentCulture);
                }
                else
                {
                    monthDayOutput.Text = "Month/day selection: not selected.";
                }
            };

            panel.Children.Add(standard);
            panel.Children.Add(standardOutput);
            panel.Children.Add(monthDay);
            panel.Children.Add(monthDayOutput);
            return panel;
        }

        private static UIElement CreateTimePickerSample()
        {
            var panel = CreateSamplePanel("TimePicker maps to WPF combo-box selectors because ModernWpf does not currently expose a TimePicker control.");
            panel.Children.Add(CreateTimeSelector("Simple time", false, 5, new TimeSpan(9, 30, 0)));
            panel.Children.Add(CreateTimeSelector("Arrival time, 15 minute increments", false, 15, new TimeSpan(14, 15, 0)));
            panel.Children.Add(CreateTimeSelector("24 hour clock", true, 5, DateTime.Now.TimeOfDay));
            return panel;
        }

        private static UIElement CreateTimeSelector(string header, bool use24HourClock, int minuteIncrement, TimeSpan initialTime)
        {
            var container = new StackPanel
            {
                Margin = new Thickness(0, 0, 0, 18)
            };
            container.Children.Add(new TextBlock
            {
                Text = header,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 6)
            });

            var row = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };

            var output = CreateOutput(string.Empty);
            var hour = new ComboBox
            {
                Width = 84,
                ItemsSource = CreateHourItems(use24HourClock),
                SelectedItem = CreateSelectedHour(initialTime, use24HourClock)
            };
            ControlHelper.SetHeader(hour, "Hour");

            var minute = new ComboBox
            {
                Width = 84,
                Margin = new Thickness(8, 0, 0, 0),
                ItemsSource = CreateMinuteItems(minuteIncrement),
                SelectedItem = RoundMinute(initialTime.Minutes, minuteIncrement).ToString("00", CultureInfo.InvariantCulture)
            };
            ControlHelper.SetHeader(minute, "Minute");

            ComboBox period = null;
            if (!use24HourClock)
            {
                period = new ComboBox
                {
                    Width = 84,
                    Margin = new Thickness(8, 0, 0, 0),
                    ItemsSource = new[] { "AM", "PM" },
                    SelectedItem = initialTime.Hours >= 12 ? "PM" : "AM"
                };
                ControlHelper.SetHeader(period, "Period");
                row.Children.Add(hour);
                row.Children.Add(minute);
                row.Children.Add(period);
            }
            else
            {
                row.Children.Add(hour);
                row.Children.Add(minute);
            }

            Action update = delegate
            {
                output.Text = "Selected time: " + FormatSelectedTime(hour, minute, period, use24HourClock);
            };
            hour.SelectionChanged += delegate { update(); };
            minute.SelectionChanged += delegate { update(); };
            if (period != null)
            {
                period.SelectionChanged += delegate { update(); };
            }
            update();

            container.Children.Add(row);
            container.Children.Add(output);
            return container;
        }

        private static string CreateCalendarSelectionSummary(Calendar calendar)
        {
            if (calendar.SelectedDates.Count == 0)
            {
                return "No date selected.";
            }

            if (calendar.SelectedDates.Count == 1)
            {
                return "Selected date: " + calendar.SelectedDate.Value.ToString("D", CultureInfo.CurrentCulture);
            }

            return "Selected dates: " + calendar.SelectedDates.Count;
        }

        private static string FormatSelectedDate(string label, DateTime? date)
        {
            return date.HasValue
                ? label + ": " + date.Value.ToString("D", CultureInfo.CurrentCulture)
                : label + ": not selected.";
        }

        private static string FormatSelectedTime(ComboBox hour, ComboBox minute, ComboBox period, bool use24HourClock)
        {
            if (hour.SelectedItem == null || minute.SelectedItem == null)
            {
                return "not selected.";
            }

            if (use24HourClock)
            {
                return hour.SelectedItem + ":" + minute.SelectedItem;
            }

            return hour.SelectedItem + ":" + minute.SelectedItem + " " + period.SelectedItem;
        }

        private static string[] CreateHourItems(bool use24HourClock)
        {
            var length = use24HourClock ? 24 : 12;
            var hours = new string[length];
            for (var i = 0; i < length; i++)
            {
                hours[i] = use24HourClock
                    ? i.ToString("00", CultureInfo.InvariantCulture)
                    : (i + 1).ToString(CultureInfo.InvariantCulture);
            }

            return hours;
        }

        private static string CreateSelectedHour(TimeSpan time, bool use24HourClock)
        {
            if (use24HourClock)
            {
                return time.Hours.ToString("00", CultureInfo.InvariantCulture);
            }

            var hour = time.Hours % 12;
            if (hour == 0)
            {
                hour = 12;
            }

            return hour.ToString(CultureInfo.InvariantCulture);
        }

        private static string[] CreateMinuteItems(int increment)
        {
            var count = 60 / increment;
            var minutes = new string[count];
            for (var i = 0; i < count; i++)
            {
                minutes[i] = (i * increment).ToString("00", CultureInfo.InvariantCulture);
            }

            return minutes;
        }

        private static int RoundMinute(int minute, int increment)
        {
            return Math.Min(60 - increment, (minute / increment) * increment);
        }

        private static StackPanel CreateSamplePanel(string description)
        {
            var panel = new StackPanel
            {
                Margin = new Thickness(0, 0, 0, 12)
            };
            panel.Children.Add(new TextBlock
            {
                Text = description,
                TextWrapping = TextWrapping.Wrap,
                Opacity = 0.72,
                Margin = new Thickness(0, 0, 0, 12)
            });
            return panel;
        }

        private static TextBlock CreateOutput(string text)
        {
            return new TextBlock
            {
                Text = text,
                Margin = new Thickness(0, 12, 0, 0),
                TextWrapping = TextWrapping.Wrap
            };
        }
    }
}
