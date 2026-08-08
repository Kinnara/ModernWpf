using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModernWpf.WinUI.Tests.TimePicker
{
    [TestClass]
    public class TimePickerSourceAuditTests
    {
        [TestMethod]
        public void CurrentWinUI3TimePickerParityIsPinnedByAudit()
        {
            var root = FindRepoRoot();
            var audit = Read(root, "docs", "timepicker-winui3-source-audit.md");
            var picker = Read(root, "ModernWpf.Controls", "TimePicker", "TimePicker.cs");
            var args = Read(root, "ModernWpf.Controls", "TimePicker", "TimePickerValueChangedEventArgs.cs");
            var peer = Read(root, "ModernWpf.Controls", "TimePicker", "TimePickerAutomationPeer.cs");
            var template = Read(root, "ModernWpf.Controls", "TimePicker", "TimePicker.xaml");
            var resources = Read(root, "ModernWpf.Controls", "TimePicker", "Strings", "Resources.resx");
            var light = Read(root, "ModernWpf", "ThemeResources", "Light.xaml");
            var dark = Read(root, "ModernWpf", "ThemeResources", "Dark.xaml");
            var highContrast = Read(root, "ModernWpf", "ThemeResources", "HighContrast.xaml");

            StringAssert.Contains(audit, "6a556bb28fc227acd2ec8fe67ee64853f559084b");
            StringAssert.Contains(audit, "3669519356c67f1376152c33ed8ea45003a91f3a");
            foreach (var blob in new[]
            {
            "2cfb8a7df11925a858a9c877c0886c2ccc3231bf",
            "9c1d6ba50c49f52648bca4e950d28aeb5ab33d55",
            "cf26552e09287ac315955985086b5da27e9b440b",
            "abe13005f2d704be462949ec7ba4543ca40fe43f",
            "cfe4eca2cba02bd9c12549425ec080ecf50a3d2d",
            "788034e244aa4d871e21297be58e2f4834062d0a",
            "95c9752724554d40e851b57ceec44ebe30b72247",
            "1a7270f56ba0b2fd7b16d6ded63ddc71dc51db98",
            "59ca2cdb06e3b9c1dd29ee0b63d1d9ade3a3c335",
            "e04859ce8a58f81b8ce2ff1c1c29ccf152dba340",
            "dfa763828ae36228594968e4f60e2ad4cbe19ed3",
            "98198cfd0218c6bdefc34d4fe7ffc4b0add0d2ec",
            "b7ee6f14031aba9345a23df3a940126e53d42399",
            "e831f93398f9d18d093766bcc04117352b04be03",
            "2e0b17ab6919ab81b11711f889ff392077440907",
            "c63aca390ddef62974dc992e9b68544d4458e8ec",
            "4a7ed1bcb73916cd081284021d1ece166062a8e3",
            "fd714a5df8be57ced001f1bcae704c45a9079155",
            "b0b0fb503ffda006f748e0815ec92ec91480f9c3",
            "ad1c8b248626baba2e08b232463bea4c08bb6773",
            "da3e955ed32b54eea364437c1bb4c7d1bfa311b6"
        })
            {
                StringAssert.Contains(audit, blob);
            }

            StringAssert.Contains(picker, "public static readonly DependencyProperty SelectedTimeProperty");
            StringAssert.Contains(picker, "public static readonly DependencyProperty HeaderPlacementProperty");
            StringAssert.Contains(picker, "public event EventHandler<TimePickerValueChangedEventArgs> TimeChanged;");
            StringAssert.Contains(picker, "public event TypedEventHandler<TimePicker, TimePickerSelectedValueChangedEventArgs> SelectedTimeChanged;");
            StringAssert.Contains(picker, "TimeChanged?.Invoke");
            StringAssert.Contains(picker, "SelectedTimeChanged?.Invoke");
            Assert.IsTrue(picker.IndexOf("TimeChanged?.Invoke", StringComparison.Ordinal) <
                picker.IndexOf("SelectedTimeChanged?.Invoke", StringComparison.Ordinal));
            StringAssert.Contains(picker, "var normalizedTime = NormalizeTimeValue(newTime);");
            StringAssert.Contains(picker, "SetCurrentValue(TimeProperty, normalizedTime);");
            StringAssert.Contains(picker, "value.Ticks % TimeSpan.TicksPerDay");
            StringAssert.Contains(picker, "MinuteIncrement == 0 ? 60 : MinuteIncrement");
            StringAssert.Contains(picker, "IsEnabledChanged += OnIsEnabledChanged;");
            StringAssert.Contains(picker, "SR_TimePickerFlyoutButtonAutomationName");
            Assert.IsFalse(picker.Contains("public const string TwelveHourClockIdentifier", StringComparison.Ordinal));
            StringAssert.Contains(args, "public TimeSpan? OldTime { get; }");
            StringAssert.Contains(args, "public TimeSpan? NewTime { get; }");

            StringAssert.Contains(peer, "return AutomationControlType.Group;");
            StringAssert.Contains(peer, "return nameof(TimePicker);");
            StringAssert.Contains(peer, "SR_TimePickerAutomationName");
            StringAssert.Contains(template, "x:Name=\"FlyoutButton\"");
            StringAssert.Contains(template, "x:Name=\"PART_Popup\"");
            StringAssert.Contains(template, "x:Name=\"HourPicker\"");
            StringAssert.Contains(template, "x:Name=\"MinutePicker\"");
            StringAssert.Contains(template, "x:Name=\"PeriodPicker\"");
            StringAssert.Contains(template, "x:Name=\"AcceptButton\"");
            StringAssert.Contains(template, "x:Name=\"DismissButton\"");
            StringAssert.Contains(template, "x:Key=\"TimePickerFlyoutButtonStyle\"");
            StringAssert.Contains(template, "TimePickerButtonBackgroundPointerOver");
            StringAssert.Contains(template, "TimePickerButtonBackgroundPressed");
            StringAssert.Contains(template, "TimePickerButtonBackgroundDisabled");
            StringAssert.Contains(template, "Target=\"FirstColumnDivider.Fill\"");
            StringAssert.Contains(template, "Target=\"SecondColumnDivider.Fill\"");
            StringAssert.Contains(resources, "<value>{0} {1} time picker</value>");

            foreach (var theme in new[] { light, dark })
            {
                StringAssert.Contains(theme, "x:Key=\"TimePickerButtonBorderBrush\" ResourceKey=\"ControlElevationBorderBrush\"");
                StringAssert.Contains(theme, "x:Key=\"TimePickerButtonBackgroundPointerOver\" ResourceKey=\"ControlFillColorSecondaryBrush\"");
                StringAssert.Contains(theme, "x:Key=\"TimePickerButtonForegroundPressed\" ResourceKey=\"TextFillColorSecondaryBrush\"");
                StringAssert.Contains(theme, "x:Key=\"TimePickerButtonForegroundDefault\" ResourceKey=\"TextFillColorSecondaryBrush\"");
                StringAssert.Contains(theme, "x:Key=\"TimePickerFlyoutPresenterHighlightFill\" ResourceKey=\"AccentFillColorDefaultBrush\"");
                StringAssert.Contains(theme, "x:Key=\"TimePickerFlyoutPresenterHighlightForegroundColor\" ResourceKey=\"TextOnAccentAAFillColorPrimary\"");
            }

            StringAssert.Contains(highContrast, "x:Key=\"TimePickerButtonBackground\" ResourceKey=\"SystemColorButtonFaceColorBrush\"");
            StringAssert.Contains(highContrast, "x:Key=\"TimePickerButtonBackgroundPointerOver\" ResourceKey=\"SystemColorHighlightTextColorBrush\"");
            StringAssert.Contains(highContrast, "x:Key=\"TimePickerFlyoutPresenterBorderBrush\" ResourceKey=\"SystemColorWindowTextColorBrush\"");
            StringAssert.Contains(highContrast, "x:Key=\"TimePickerFlyoutPresenterHighlightFill\" ResourceKey=\"SystemColorHighlightColorBrush\"");
            StringAssert.Contains(highContrast, "x:Key=\"TimePickerButtonForegroundDefault\" ResourceKey=\"SystemColorButtonTextColorBrush\"");
            StringAssert.Contains(highContrast, "x:Key=\"TimePickerFlyoutPresenterHighlightForegroundColor\" ResourceKey=\"SystemColorHighlightTextColor\"");

            StringAssert.Contains(audit, "finite `ListBox` selectors");
            StringAssert.Contains(audit, "does not create a synthetic");
            StringAssert.Contains(audit, "AccentFillColorDefaultBrush");
            StringAssert.Contains(audit, "SystemColorButtonFaceColorBrush");
            StringAssert.Contains(audit, "SystemColorHighlightTextColor");
        }

        private static string Read(string root, params string[] parts)
        {
            return File.ReadAllText(Path.Combine(new[] { root }.Concat(parts).ToArray()));
        }

        private static string FindRepoRoot()
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null)
            {
                if (File.Exists(Path.Combine(directory.FullName, "ModernWpf.sln")))
                {
                    return directory.FullName;
                }

                directory = directory.Parent;
            }

            Assert.Fail("Could not locate ModernWpf.sln from the test output directory.");
            return string.Empty;
        }
    }
}
