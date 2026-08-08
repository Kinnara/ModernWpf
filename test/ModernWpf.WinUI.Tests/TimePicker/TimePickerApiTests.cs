using System;
using System.Globalization;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Automation.Peers;
using ModernWpf.Controls;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.TimePicker
{
    [TestClass]
    public class TimePickerApiTests
    {
        [TestMethod]
        public void DefaultsMatchCurrentWinUIContract()
        {
            WpfTestHost.Run(() =>
            {
                var picker = new ModernWpf.Controls.TimePicker();
                var pattern = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern;
                var expectedClock = pattern.IndexOf('H') >= 0
                    ? ModernWpf.Controls.TimePicker.TwentyFourHourClockIdentifier
                    : ModernWpf.Controls.TimePicker.TwelveHourClockIdentifier;

                Assert.IsNull(picker.Header);
                Assert.IsNull(picker.HeaderTemplate);
                Assert.AreEqual(expectedClock, picker.ClockIdentifier);
                Assert.AreEqual(1, picker.MinuteIncrement);
                Assert.AreEqual(TimeSpan.FromTicks(-1), picker.Time);
                Assert.IsNull(picker.SelectedTime);
                Assert.AreEqual(LightDismissOverlayMode.Auto, picker.LightDismissOverlayMode);
                Assert.AreEqual(ControlHeaderPlacement.Top, picker.HeaderPlacement);
            });
        }

        [TestMethod]
        public void SettersValidationAndCoercionMatchCurrentWinUI()
        {
            WpfTestHost.Run(() =>
            {
                var template = new DataTemplate();
                var picker = new ModernWpf.Controls.TimePicker
                {
                    Header = "Start time",
                    HeaderTemplate = template,
                    ClockIdentifier = ModernWpf.Controls.TimePicker.TwentyFourHourClockIdentifier,
                    MinuteIncrement = 5,
                    LightDismissOverlayMode = LightDismissOverlayMode.On,
                    HeaderPlacement = ControlHeaderPlacement.Left,
                    CornerRadius = new CornerRadius(7)
                };

                Assert.AreEqual("Start time", picker.Header);
                Assert.AreSame(template, picker.HeaderTemplate);
                Assert.AreEqual(ModernWpf.Controls.TimePicker.TwentyFourHourClockIdentifier, picker.ClockIdentifier);
                Assert.AreEqual(5, picker.MinuteIncrement);
                Assert.AreEqual(LightDismissOverlayMode.On, picker.LightDismissOverlayMode);
                Assert.AreEqual(ControlHeaderPlacement.Left, picker.HeaderPlacement);
                Assert.AreEqual(new CornerRadius(7), picker.CornerRadius);

                Assert.ThrowsExactly<ArgumentException>(() => picker.ClockIdentifier = "SystemClock");
                Assert.ThrowsExactly<ArgumentException>(() => picker.MinuteIncrement = -1);
                Assert.ThrowsExactly<ArgumentException>(() => picker.MinuteIncrement = 60);
                Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => picker.Time = TimeSpan.FromMinutes(-2));

                picker.Time = TimeSpan.FromHours(25) + TimeSpan.FromMinutes(47) + TimeSpan.FromSeconds(30);
                Assert.AreEqual(new TimeSpan(1, 45, 0), picker.Time);
                Assert.AreEqual(new TimeSpan(1, 45, 0), picker.SelectedTime);

                picker.Time = TimeSpan.FromTicks(-1);
                Assert.IsNull(picker.SelectedTime);
            });
        }

        [TestMethod]
        public void SelectedTimeSynchronizesAndRaisesBothSourceEventsInOrder()
        {
            WpfTestHost.Run(() =>
            {
                var picker = new ModernWpf.Controls.TimePicker();
                var eventOrder = string.Empty;
                TimePickerValueChangedEventArgs? timeArgs = null;
                TimePickerSelectedValueChangedEventArgs? selectedArgs = null;

                picker.TimeChanged += (sender, args) =>
                {
                    Assert.AreSame(picker, sender);
                    eventOrder += "T";
                    timeArgs = args;
                };
                picker.SelectedTimeChanged += (sender, args) =>
                {
                    Assert.AreSame(picker, sender);
                    eventOrder += "S";
                    selectedArgs = args;
                };

                picker.SelectedTime = new TimeSpan(2, 15, 0);

                var expected = new TimeSpan(2, 15, 0);
                Assert.AreEqual(expected, picker.SelectedTime);
                Assert.AreEqual(expected, picker.Time);
                Assert.AreEqual("TS", eventOrder);
                var capturedTimeArgs = timeArgs ?? throw new AssertFailedException("TimeChanged was not raised.");
                var capturedSelectedArgs = selectedArgs ?? throw new AssertFailedException("SelectedTimeChanged was not raised.");
                Assert.AreEqual(TimeSpan.FromTicks(-1), capturedTimeArgs.OldTime);
                Assert.AreEqual(expected, capturedTimeArgs.NewTime);
                Assert.IsNull(capturedSelectedArgs.OldTime);
                Assert.AreEqual(expected, capturedSelectedArgs.NewTime);
            });
        }

        [TestMethod]
        public void DirectSelectedTimeAssignmentPreservesItsValueWhileTimeIsCoerced()
        {
            WpfTestHost.Run(() =>
            {
                var picker = new ModernWpf.Controls.TimePicker
                {
                    MinuteIncrement = 5
                };
                TimePickerValueChangedEventArgs? timeArgs = null;
                TimePickerSelectedValueChangedEventArgs? selectedArgs = null;
                picker.TimeChanged += (sender, args) => timeArgs = args;
                picker.SelectedTimeChanged += (sender, args) => selectedArgs = args;

                var selectedTime = new TimeSpan(7, 47, 19);
                picker.SelectedTime = selectedTime;

                Assert.AreEqual(selectedTime, picker.SelectedTime);
                Assert.AreEqual(new TimeSpan(7, 45, 0), picker.Time);
                var capturedTimeArgs = timeArgs ?? throw new AssertFailedException("TimeChanged was not raised.");
                var capturedSelectedArgs = selectedArgs ?? throw new AssertFailedException("SelectedTimeChanged was not raised.");
                Assert.AreEqual(selectedTime, capturedTimeArgs.OldTime);
                Assert.AreEqual(new TimeSpan(7, 45, 0), capturedTimeArgs.NewTime);
                Assert.AreEqual(selectedTime, capturedSelectedArgs.OldTime);
                Assert.AreEqual(new TimeSpan(7, 45, 0), capturedSelectedArgs.NewTime);
            });
        }

        [TestMethod]
        public void MinuteIncrementRoundsDownAndRegeneratesSelectors()
        {
            WpfTestHost.Run(() =>
            {
                var picker = new ModernWpf.Controls.TimePicker
                {
                    ClockIdentifier = ModernWpf.Controls.TimePicker.TwelveHourClockIdentifier,
                    SelectedTime = new TimeSpan(17, 12, 0)
                };
                var changes = 0;
                picker.TimeChanged += (sender, args) => changes++;

                picker.MinuteIncrement = 5;

                Assert.AreEqual(new TimeSpan(17, 10, 0), picker.Time);
                Assert.AreEqual(new TimeSpan(17, 10, 0), picker.SelectedTime);
                Assert.AreEqual(1, changes);

                using var host = new TestWindowHost(picker, width: 500, height: 500);
                host.UpdateLayout();
                picker.OpenFlyoutForAutomation();

                Assert.AreEqual(12, Part<ListBox>(picker, "MinutePicker").Items.Count);
                picker.MinuteIncrement = 0;
                Assert.AreEqual(1, Part<ListBox>(picker, "MinutePicker").Items.Count);
                Assert.AreEqual("00", Part<ListBox>(picker, "MinutePicker").Items[0]);
            });
        }

        [TestMethod]
        public void TemplateShowsPlaceholdersAndSwitchesBetweenTwelveAndTwentyFourHourClocks()
        {
            WpfTestHost.Run(() =>
            {
                var picker = new ModernWpf.Controls.TimePicker
                {
                    ClockIdentifier = ModernWpf.Controls.TimePicker.TwelveHourClockIdentifier,
                    Header = "Start time"
                };

                using var host = new TestWindowHost(picker, width: 500, height: 300);
                host.UpdateLayout();

                var hour = Part<TextBlock>(picker, "HourTextBlock");
                var minute = Part<TextBlock>(picker, "MinuteTextBlock");
                var period = Part<TextBlock>(picker, "PeriodTextBlock");
                Assert.AreEqual("hour", hour.Text);
                Assert.AreEqual("minute", minute.Text);
                Assert.AreEqual(Visibility.Visible, period.Visibility);

                picker.SelectedTime = new TimeSpan(17, 15, 0);
                host.UpdateLayout();
                Assert.AreEqual("5", hour.Text);
                Assert.AreEqual("15", minute.Text);
                Assert.AreEqual(CultureInfo.CurrentCulture.DateTimeFormat.PMDesignator, period.Text);

                picker.ClockIdentifier = ModernWpf.Controls.TimePicker.TwentyFourHourClockIdentifier;
                host.UpdateLayout();
                Assert.AreEqual("17", hour.Text);
                Assert.AreEqual("15", minute.Text);
                Assert.AreEqual(Visibility.Collapsed, period.Visibility);
            });
        }

        [TestMethod]
        public void FlyoutAcceptAndDismissPreserveSourceSelectionSemantics()
        {
            WpfTestHost.Run(() =>
            {
                var picker = new ModernWpf.Controls.TimePicker
                {
                    ClockIdentifier = ModernWpf.Controls.TimePicker.TwelveHourClockIdentifier,
                    MinuteIncrement = 5,
                    SelectedTime = new TimeSpan(9, 10, 0)
                };

                using var host = new TestWindowHost(picker, width: 500, height: 500);
                host.UpdateLayout();

                picker.OpenFlyoutForAutomation();
                Assert.IsTrue(picker.IsFlyoutOpen);
                Assert.AreEqual(12, Part<ListBox>(picker, "HourPicker").Items.Count);
                Assert.AreEqual(12, Part<ListBox>(picker, "MinutePicker").Items.Count);
                Assert.AreEqual(2, Part<ListBox>(picker, "PeriodPicker").Items.Count);

                Part<ListBox>(picker, "HourPicker").SelectedIndex = 4;
                Part<ListBox>(picker, "MinutePicker").SelectedIndex = 6;
                Part<ListBox>(picker, "PeriodPicker").SelectedIndex = 1;
                Part<Button>(picker, "AcceptButton").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

                Assert.IsFalse(picker.IsFlyoutOpen);
                Assert.AreEqual(new TimeSpan(16, 30, 0), picker.SelectedTime);

                picker.OpenFlyoutForAutomation();
                Part<ListBox>(picker, "HourPicker").SelectedIndex = 7;
                Part<ListBox>(picker, "MinutePicker").SelectedIndex = 9;
                Part<Button>(picker, "DismissButton").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

                Assert.IsFalse(picker.IsFlyoutOpen);
                Assert.AreEqual(new TimeSpan(16, 30, 0), picker.SelectedTime);
            });
        }

        [TestMethod]
        public void KeyboardAcceptCancelAndDisabledOpeningUseWpfPopupContract()
        {
            WpfTestHost.Run(() =>
            {
                var picker = new ModernWpf.Controls.TimePicker
                {
                    ClockIdentifier = ModernWpf.Controls.TimePicker.TwentyFourHourClockIdentifier,
                    MinuteIncrement = 15,
                    SelectedTime = new TimeSpan(9, 0, 0)
                };

                using var host = new TestWindowHost(picker, width: 500, height: 500);
                host.UpdateLayout();

                picker.OpenFlyoutForAutomation();
                Part<ListBox>(picker, "HourPicker").SelectedIndex = 16;
                Part<ListBox>(picker, "MinutePicker").SelectedIndex = 2;
                RaisePreviewKey(picker, Key.Escape);

                Assert.IsFalse(picker.IsFlyoutOpen);
                Assert.AreEqual(new TimeSpan(9, 0, 0), picker.SelectedTime);

                picker.OpenFlyoutForAutomation();
                Part<ListBox>(picker, "HourPicker").SelectedIndex = 16;
                Part<ListBox>(picker, "MinutePicker").SelectedIndex = 2;
                RaisePreviewKey(picker, Key.Enter);

                Assert.IsFalse(picker.IsFlyoutOpen);
                Assert.AreEqual(new TimeSpan(16, 30, 0), picker.SelectedTime);

                picker.IsEnabled = false;
                picker.OpenFlyoutForAutomation();
                Assert.IsFalse(picker.IsFlyoutOpen);
            });
        }

        [TestMethod]
        public void AutomationPeerUsesSourceGroupRoleAndHeaderFallback()
        {
            WpfTestHost.Run(() =>
            {
                var picker = new ModernWpf.Controls.TimePicker { Header = "TimePickerTest" };
                using var host = new TestWindowHost(picker, width: 500, height: 300);
                host.UpdateLayout();

                var peer = UIElementAutomationPeer.CreatePeerForElement(picker) ?? new TimePickerAutomationPeer(picker);

                Assert.AreEqual("TimePicker", peer.GetClassName());
                Assert.AreEqual(AutomationControlType.Group, peer.GetAutomationControlType());
                Assert.AreEqual("TimePickerTest", peer.GetName());
                Assert.AreEqual(
                    "TimePickerTest time picker",
                    AutomationProperties.GetName(Part<Button>(picker, "FlyoutButton")));

                picker.ClockIdentifier = ModernWpf.Controls.TimePicker.TwentyFourHourClockIdentifier;
                picker.SelectedTime = new TimeSpan(17, 30, 0);
                var flyoutName = AutomationProperties.GetName(Part<Button>(picker, "FlyoutButton"));
                StringAssert.StartsWith(flyoutName, "TimePickerTest 17:30");
                StringAssert.EndsWith(flyoutName, " time picker");

                picker.Header = null;
                Assert.AreEqual("Time picker", peer.GetName());
            });
        }

        [TestMethod]
        public void AutomationValueUsesCultureFieldOrderForBothClockIdentifiers()
        {
            WpfTestHost.Run(() =>
            {
                var originalCulture = CultureInfo.CurrentCulture;
                try
                {
                    CultureInfo.CurrentCulture = new CultureInfo("ko-KR", useUserOverride: false);
                    var picker = new ModernWpf.Controls.TimePicker
                    {
                        Header = "TimePickerTest",
                        ClockIdentifier = ModernWpf.Controls.TimePicker.TwelveHourClockIdentifier,
                        SelectedTime = new TimeSpan(17, 30, 0)
                    };
                    using var host = new TestWindowHost(picker, width: 500, height: 300);
                    host.UpdateLayout();

                    var flyoutButton = Part<Button>(picker, "FlyoutButton");
                    var twelveHourName = AutomationProperties.GetName(flyoutButton);
                    StringAssert.StartsWith(
                        twelveHourName,
                        $"TimePickerTest {CultureInfo.CurrentCulture.DateTimeFormat.PMDesignator} 5:30");
                    Assert.AreEqual(0, Grid.GetColumn(Part<FrameworkElement>(picker, "PeriodPickerHost")));
                    Assert.AreEqual(2, Grid.GetColumn(Part<FrameworkElement>(picker, "HourPickerHost")));
                    Assert.AreEqual(4, Grid.GetColumn(Part<FrameworkElement>(picker, "MinutePickerHost")));

                    picker.ClockIdentifier = ModernWpf.Controls.TimePicker.TwentyFourHourClockIdentifier;
                    var twentyFourHourName = AutomationProperties.GetName(flyoutButton);
                    StringAssert.StartsWith(twentyFourHourName, "TimePickerTest 17:30");
                    Assert.AreEqual(0, Grid.GetColumn(Part<FrameworkElement>(picker, "HourPickerHost")));
                    Assert.AreEqual(2, Grid.GetColumn(Part<FrameworkElement>(picker, "MinutePickerHost")));
                    Assert.AreEqual(Visibility.Collapsed, Part<FrameworkElement>(picker, "PeriodPickerHost").Visibility);
                }
                finally
                {
                    CultureInfo.CurrentCulture = originalCulture;
                }
            });
        }

        [TestMethod]
        public void HeaderPlacementMovesHeaderWithoutChangingThePickerContract()
        {
            WpfTestHost.Run(() =>
            {
                var picker = new ModernWpf.Controls.TimePicker
                {
                    Header = "Start time",
                    HeaderPlacement = ControlHeaderPlacement.Left
                };
                using var host = new TestWindowHost(picker, width: 600, height: 300);
                host.UpdateLayout();

                var header = Part<ContentPresenter>(picker, "HeaderContentPresenter");
                var button = Part<Button>(picker, "FlyoutButton");
                Assert.AreEqual(0, Grid.GetRow(header));
                Assert.AreEqual(0, Grid.GetColumn(header));
                Assert.AreEqual(0, Grid.GetRow(button));
                Assert.AreEqual(1, Grid.GetColumn(button));

                picker.HeaderPlacement = ControlHeaderPlacement.Top;
                host.UpdateLayout();
                Assert.AreEqual(0, Grid.GetRow(header));
                Assert.AreEqual(0, Grid.GetColumn(header));
                Assert.AreEqual(1, Grid.GetRow(button));
                Assert.AreEqual(0, Grid.GetColumn(button));
            });
        }

        private static T Part<T>(ModernWpf.Controls.TimePicker picker, string name)
            where T : DependencyObject
        {
            var part = picker.Template.FindName(name, picker) as T;
            Assert.IsNotNull(part, $"Expected template part '{name}'.");
            return part;
        }

        private static void RaisePreviewKey(UIElement target, Key key)
        {
            var source = PresentationSource.FromVisual(target);
            Assert.IsNotNull(source, "The hosted control must have a presentation source.");
            var args = new KeyEventArgs(
                Keyboard.PrimaryDevice,
                source,
                Environment.TickCount,
                key)
            {
                RoutedEvent = Keyboard.PreviewKeyDownEvent
            };

            target.RaiseEvent(args);
            Assert.IsTrue(args.Handled, $"Expected {key} to be handled by the open TimePicker popup.");
        }
    }
}
