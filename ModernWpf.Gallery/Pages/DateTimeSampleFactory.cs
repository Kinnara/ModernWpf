using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using ModernWpf.Gallery.Models;
using ModernWpf.Controls.Primitives;
using Calendar = System.Windows.Controls.Calendar;

namespace ModernWpf.Gallery.Pages
{
    internal static class DateTimeSampleFactory
    {
        private const string CalendarDatePickerXaml =
@"<CalendarDatePicker PlaceholderText=""Pick a date"" Header=""Calendar"" />";

        private const string CalendarViewXaml =
@"<CalendarView
    SelectionMode=""$(SelectionMode)""
    IsGroupLabelVisible=""$(IsGroupLabelVisible)""
    IsOutOfScopeEnabled=""$(IsOutOfScopeEnabled)""
    Language=""$(Language)""
    CalendarIdentifier=""$(CalendarIdentifier)"" />";

        private const string TimePickerSimpleXaml =
@"<TimePicker/>";

        private const string TimePickerHeaderXaml =
@"<TimePicker Header=""Arrival time"" MinuteIncrement=""15"" />";

        private const string TimePickerTwentyFourHourXaml =
@"<xmlns:sys=""using:System"">

<TimePicker ClockIdentifier=""24HourClock"" Header=""24 hour clock"" SelectedTime=""{x:Bind sys:DateTime.Now.TimeOfDay}"" />";

        public static IReadOnlyList<GalleryExample> CreateExamples(string uniqueId)
        {
            switch (uniqueId)
            {
                case "CalendarDatePicker":
                    return CreateCalendarDatePickerExamples();
                case "CalendarView":
                    return CreateCalendarViewExamples();
                case "TimePicker":
                    return CreateTimePickerExamples();
                default:
                    return Array.Empty<GalleryExample>();
            }
        }

        public static UIElement Create(string uniqueId)
        {
            switch (uniqueId)
            {
                case "CalendarDatePicker":
                    return CreateCalendarDatePickerSample();
                case "CalendarView":
                    return CreateCalendarViewSample();
                case "TimePicker":
                    return CreateTimePickerSample();
                default:
                    return null;
            }
        }

        private static UIElement CreateCalendarDatePickerSample()
        {
            var root = new GallerySamplePanel
            {
                Margin = new Thickness(0, 0, 0, 12)
            };
            GalleryAutomation.WithAutomationId(root, GalleryAutomation.SampleRootId("CalendarDatePicker"));
            root.Children.Add(CreateCalendarDatePickerExampleContent(assignRootAutomationId: false));
            return root;
        }

        private static IReadOnlyList<GalleryExample> CreateCalendarDatePickerExamples()
        {
            return new[]
            {
                new GalleryExample(
                    "CalendarDatePicker with a header and placeholder text.",
                    CreateCalendarDatePickerExampleContent(assignRootAutomationId: true),
                    CalendarDatePickerXaml,
                    null)
            };
        }

        private static GallerySamplePanel CreateCalendarDatePickerExampleContent(bool assignRootAutomationId)
        {
            var root = CreateExampleRoot("CalendarDatePicker", assignRootAutomationId);
            var output = CreateOutput("No date selected.");
            var picker = new DatePicker
            {
                Name = "CalendarDatePicker1",
                HorizontalAlignment = HorizontalAlignment.Left
            };
            GalleryAutomation.WithAutomationId(picker, GalleryAutomation.SampleElementId("CalendarDatePicker", "CalendarDatePicker"));
            AutomationProperties.SetName(picker, "Calendar");
            ControlHelper.SetHeader(picker, "Calendar");
            ControlHelper.SetPlaceholderText(picker, "Pick a date");
            picker.SelectedDateChanged += delegate
            {
                output.Text = FormatSelectedDate("Selected date", picker.SelectedDate);
            };

            root.Children.Add(picker);
            root.Children.Add(output);
            return root;
        }

        private static UIElement CreateCalendarViewSample()
        {
            var root = new GallerySamplePanel
            {
                Margin = new Thickness(0, 0, 0, 12)
            };
            GalleryAutomation.WithAutomationId(root, GalleryAutomation.SampleRootId("CalendarView"));
            root.Children.Add(CreateCalendarViewExampleContent(assignRootAutomationId: false));
            return root;
        }

        private static IReadOnlyList<GalleryExample> CreateCalendarViewExamples()
        {
            return new[]
            {
                new GalleryExample(
                    "A basic calendar view.",
                    CreateCalendarViewExampleContent(assignRootAutomationId: true),
                    CalendarViewXaml,
                    null)
            };
        }

        private static GallerySamplePanel CreateCalendarViewExampleContent(bool assignRootAutomationId)
        {
            var root = CreateExampleRoot("CalendarView", assignRootAutomationId);
            var calendar = new Calendar
            {
                Name = "Control1",
                SelectionMode = CalendarSelectionMode.SingleDate,
                IsTodayHighlighted = true,
                VerticalAlignment = VerticalAlignment.Top
            };
            GalleryAutomation.WithAutomationId(calendar, GalleryAutomation.SampleElementId("CalendarView", "CalendarView"));
            AutomationProperties.SetName(calendar, "CalendarView");
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
                Text = "CalendarView Options",
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 8)
            });

            var isGroupLabelVisible = new CheckBox
            {
                Name = "isGroupLabelVisible",
                Content = "IsGroupLabelVisible",
                IsChecked = true
            };
            options.Children.Add(isGroupLabelVisible);

            var isOutOfScopeEnabled = new CheckBox
            {
                Name = "isOutOfScopeEnabled",
                Content = "IsOutOfScopeEnabled",
                IsChecked = true
            };
            isOutOfScopeEnabled.Checked += delegate
            {
                calendar.DisplayDateStart = null;
                calendar.DisplayDateEnd = null;
            };
            isOutOfScopeEnabled.Unchecked += delegate
            {
                calendar.DisplayDateStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                calendar.DisplayDateEnd = calendar.DisplayDateStart.Value.AddMonths(1).AddDays(-1);
            };
            options.Children.Add(isOutOfScopeEnabled);

            var mode = new ComboBox
            {
                Name = "selectionMode",
                Margin = new Thickness(0, 12, 0, 0),
                ItemsSource = new[]
                {
                    "None",
                    "Single",
                    "Multiple"
                },
                SelectedItem = "Single"
            };
            ControlHelper.SetHeader(mode, "SelectionMode");
            mode.SelectionChanged += delegate
            {
                switch (mode.SelectedItem as string)
                {
                    case "None":
                        calendar.SelectionMode = CalendarSelectionMode.None;
                        break;
                    case "Multiple":
                        calendar.SelectionMode = CalendarSelectionMode.MultipleRange;
                        break;
                    default:
                        calendar.SelectionMode = CalendarSelectionMode.SingleDate;
                        break;
                }

                output.Text = CreateCalendarSelectionSummary(calendar);
            };
            options.Children.Add(mode);

            var calendarIdentifier = new ComboBox
            {
                Name = "calendarIdentifier",
                Width = 220,
                Margin = new Thickness(0, 10, 0, 0),
                ItemsSource = new[]
                {
                    "GregorianCalendar",
                    "HebrewCalendar",
                    "HijriCalendar",
                    "JapaneseCalendar",
                    "JulianCalendar",
                    "KoreanCalendar",
                    "PersianCalendar",
                    "TaiwanCalendar",
                    "ThaiCalendar",
                    "UmAlQuraCalendar"
                },
                SelectedItem = "GregorianCalendar"
            };
            ControlHelper.SetHeader(calendarIdentifier, "CalendarIdentifier");
            options.Children.Add(calendarIdentifier);

            var calendarLanguages = new ComboBox
            {
                Name = "calendarLanguages",
                Width = 220,
                Margin = new Thickness(0, 10, 0, 0),
                ItemsSource = new[]
                {
                    "English (United States)",
                    "Arabic",
                    "Hebrew",
                    "Japanese",
                    "Korean",
                    "Thai"
                },
                SelectedIndex = 0
            };
            ControlHelper.SetHeader(calendarLanguages, "Language");
            options.Children.Add(calendarLanguages);

            var row = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };
            row.Children.Add(calendar);
            row.Children.Add(options);

            root.Children.Add(row);
            root.Children.Add(output);
            return root;
        }

        private static UIElement CreateTimePickerSample()
        {
            var root = new GallerySamplePanel
            {
                Margin = new Thickness(0, 0, 0, 12)
            };
            GalleryAutomation.WithAutomationId(root, GalleryAutomation.SampleRootId("TimePicker"));
            root.Children.Add(CreateTimeSelector(
                namePrefix: "TimePicker1",
                header: null,
                use24HourClock: false,
                minuteIncrement: 5,
                initialTime: new TimeSpan(9, 30, 0),
                assignPrimaryAutomationId: true));
            return root;
        }

        private static IReadOnlyList<GalleryExample> CreateTimePickerExamples()
        {
            return new[]
            {
                new GalleryExample(
                    "A simple TimePicker.",
                    CreateTimeSelector(
                        namePrefix: "TimePicker1",
                        header: null,
                        use24HourClock: false,
                        minuteIncrement: 5,
                        initialTime: new TimeSpan(9, 30, 0),
                        assignPrimaryAutomationId: true,
                        rootAutomationId: GalleryAutomation.SampleRootId("TimePicker")),
                    TimePickerSimpleXaml,
                    null),
                new GalleryExample(
                    "A TimePicker with a header and minute increments specified.",
                    CreateTimeSelector(
                        namePrefix: "TimePicker2",
                        header: "Arrival time",
                        use24HourClock: false,
                        minuteIncrement: 15,
                        initialTime: new TimeSpan(14, 15, 0),
                        assignPrimaryAutomationId: false),
                    TimePickerHeaderXaml,
                    null),
                new GalleryExample(
                    "A TimePicker using a 24-hour clock, initialized to current time.",
                    CreateTimeSelector(
                        namePrefix: "TimePicker3",
                        header: "24 hour clock",
                        use24HourClock: true,
                        minuteIncrement: 5,
                        initialTime: DateTime.Now.TimeOfDay,
                        assignPrimaryAutomationId: false),
                    TimePickerTwentyFourHourXaml,
                    null)
            };
        }

        private static UIElement CreateTimeSelector(
            string namePrefix,
            string header,
            bool use24HourClock,
            int minuteIncrement,
            TimeSpan initialTime,
            bool assignPrimaryAutomationId,
            string rootAutomationId = null)
        {
            var container = new GallerySamplePanel
            {
                Margin = new Thickness(0, 0, 0, 18)
            };
            if (rootAutomationId != null)
            {
                GalleryAutomation.WithAutomationId(container, rootAutomationId);
            }

            if (header != null)
            {
                container.Children.Add(new TextBlock
                {
                    Text = header,
                    FontWeight = FontWeights.SemiBold,
                    Margin = new Thickness(0, 0, 0, 6)
                });
            }

            var row = new StackPanel
            {
                Name = namePrefix,
                Orientation = Orientation.Horizontal
            };
            AutomationProperties.SetName(row, string.IsNullOrEmpty(header) ? "TimePicker" : header);
            if (assignPrimaryAutomationId)
            {
                GalleryAutomation.WithAutomationId(row, GalleryAutomation.SampleElementId("TimePicker", "TimePicker"));
            }

            var output = CreateOutput(string.Empty);
            var hour = new ComboBox
            {
                Name = namePrefix + "HourComboBox",
                Width = 84,
                ItemsSource = CreateHourItems(use24HourClock),
                SelectedItem = CreateSelectedHour(initialTime, use24HourClock)
            };
            ControlHelper.SetHeader(hour, "Hour");

            var minute = new ComboBox
            {
                Name = namePrefix + "MinuteComboBox",
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
                    Name = namePrefix + "PeriodComboBox",
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

        private static GallerySamplePanel CreateExampleRoot(string uniqueId, bool assignRootAutomationId)
        {
            var root = new GallerySamplePanel
            {
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            if (assignRootAutomationId)
            {
                GalleryAutomation.WithAutomationId(root, GalleryAutomation.SampleRootId(uniqueId));
            }

            return root;
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
