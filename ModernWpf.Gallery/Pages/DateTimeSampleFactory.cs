using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Mux = ModernWpf.Controls;

namespace ModernWpf.Gallery.Pages
{
    internal static class DateTimeSampleFactory
    {
        private const string SimpleTimePickerXaml =
@"<ui:TimePicker />";

        private const string HeaderAndIncrementTimePickerXaml =
@"<ui:TimePicker Header=""Arrival time"" MinuteIncrement=""15"" />";

        private const string TwentyFourHourTimePickerXaml =
@"<ui:TimePicker x:Name=""timePicker""
               ClockIdentifier=""24HourClock""
               Header=""24 hour clock"" />";

        private const string TwentyFourHourTimePickerCode =
@"timePicker.SelectedTime = DateTime.Now.TimeOfDay;";

        public static UIElement Create(string uniqueId)
        {
            if (!string.Equals(uniqueId, "TimePicker", StringComparison.Ordinal))
            {
                return null;
            }

            var panel = new GallerySamplePanel
            {
                Orientation = Orientation.Vertical
            };
            GalleryAutomation.WithAutomationId(panel, GalleryAutomation.SampleRootId("TimePicker"));

            panel.Children.Add(CreatePicker("Simple"));
            panel.Children.Add(CreatePicker("HeaderMinuteIncrement", "Arrival time", 15));
            panel.Children.Add(CreatePicker(
                "TwentyFourHour",
                "24 hour clock",
                selectedTime: DateTime.Now.TimeOfDay,
                clockIdentifier: "24HourClock"));
            return panel;
        }

        public static IReadOnlyList<GalleryExample> CreateExamples(string uniqueId)
        {
            if (!string.Equals(uniqueId, "TimePicker", StringComparison.Ordinal))
            {
                return Array.Empty<GalleryExample>();
            }

            return new[]
            {
                new GalleryExample(
                    "A simple TimePicker.",
                    CreatePicker("Simple"),
                    SimpleTimePickerXaml,
                    null),
                new GalleryExample(
                    "A TimePicker with a header and minute increments specified.",
                    CreatePicker("HeaderMinuteIncrement", "Arrival time", 15),
                    HeaderAndIncrementTimePickerXaml,
                    null),
                new GalleryExample(
                    "A TimePicker using a 24-hour clock, initialized to current time.",
                    CreatePicker(
                        "TwentyFourHour",
                        "24 hour clock",
                        selectedTime: DateTime.Now.TimeOfDay,
                        clockIdentifier: "24HourClock"),
                    TwentyFourHourTimePickerXaml,
                    TwentyFourHourTimePickerCode)
            };
        }

        private static Mux.TimePicker CreatePicker(
            string automationName,
            object header = null,
            int minuteIncrement = 1,
            TimeSpan? selectedTime = null,
            string clockIdentifier = null)
        {
            var picker = new Mux.TimePicker
            {
                Header = header,
                MinuteIncrement = minuteIncrement,
                SelectedTime = selectedTime,
                Margin = new Thickness(0, 0, 0, 16)
            };
            if (clockIdentifier != null)
            {
                picker.ClockIdentifier = clockIdentifier;
            }

            return GalleryAutomation.WithAutomationId(
                picker,
                GalleryAutomation.SampleElementId("TimePicker", automationName));
        }
    }
}
