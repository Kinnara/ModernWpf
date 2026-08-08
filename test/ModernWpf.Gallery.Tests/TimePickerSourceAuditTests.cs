using System;
using System.IO;
using System.Linq;
using System.Windows.Automation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Gallery.Models;
using ModernWpf.Gallery.Pages;
using Mux = ModernWpf.Controls;

namespace ModernWpf.Gallery.Tests
{
    [TestClass]
    public class TimePickerSourceAuditTests
    {
        [TestMethod]
        public void PinnedGalleryExamplesAreCataloguedAndExecutable()
        {
            WpfTestHost.Run(() =>
            {
                var item = GalleryCatalog.FindItem("TimePicker");
                Assert.IsNotNull(item);
                Assert.AreEqual("DateAndCalendar", item.GroupId);
                Assert.AreEqual("ModernWpf.Controls", item.ApiNamespace);
                Assert.IsTrue(item.Docs.Any(link => link.Title == "TimePicker - API"));

                var examples = DateTimeSampleFactory.CreateExamples("TimePicker");
                Assert.AreEqual(3, examples.Count);
                CollectionAssert.AreEqual(
                    new[]
                    {
                        "A simple TimePicker.",
                        "A TimePicker with a header and minute increments specified.",
                        "A TimePicker using a 24-hour clock, initialized to current time."
                    },
                    examples.Select(example => example.HeaderText).ToArray());

                var simple = (Mux.TimePicker)examples[0].ExampleContent;
                var incremented = (Mux.TimePicker)examples[1].ExampleContent;
                var twentyFourHour = (Mux.TimePicker)examples[2].ExampleContent;
                Assert.AreEqual("GallerySample_TimePicker_Simple", AutomationProperties.GetAutomationId(simple));
                Assert.AreEqual("Arrival time", incremented.Header);
                Assert.AreEqual(15, incremented.MinuteIncrement);
                Assert.AreEqual("24 hour clock", twentyFourHour.Header);
                Assert.AreEqual("24HourClock", twentyFourHour.ClockIdentifier);
                Assert.IsTrue(twentyFourHour.SelectedTime.HasValue);
                StringAssert.Contains(examples[2].CSharpCode, "DateTime.Now.TimeOfDay");

                Assert.IsNotNull(DateTimeSampleFactory.Create("TimePicker"));
                Assert.IsNull(DateTimeSampleFactory.Create("DatePicker"));
            });
        }

        [TestMethod]
        public void GalleryPortPinsOfficialTimePickerSourcesAndWpfAdaptation()
        {
            var root = FindRepoRoot();
            var audit = File.ReadAllText(Path.Combine(root, "docs", "timepicker-winui3-source-audit.md"));
            var factory = File.ReadAllText(Path.Combine(root, "ModernWpf.Gallery", "Pages", "DateTimeSampleFactory.cs"));

            StringAssert.Contains(audit, "3669519356c67f1376152c33ed8ea45003a91f3a");
            StringAssert.Contains(audit, "c63aca390ddef62974dc992e9b68544d4458e8ec");
            StringAssert.Contains(audit, "fd714a5df8be57ced001f1bcae704c45a9079155");
            StringAssert.Contains(factory, "A simple TimePicker.");
            StringAssert.Contains(factory, "Header=\"\"Arrival time\"\" MinuteIncrement=\"\"15\"");
            StringAssert.Contains(factory, "ClockIdentifier=\"\"24HourClock\"\"");
            StringAssert.Contains(factory, "DateTime.Now.TimeOfDay");
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
