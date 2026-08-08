using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Gallery.Models;
using ModernWpf.Gallery.Pages;
using Mux = ModernWpf.Controls;

namespace ModernWpf.Gallery.Tests
{
    [TestClass]
    public class TwoPaneViewSourceAuditTests
    {
        [TestMethod]
        public void WpfAdaptiveGalleryExampleExposesLiveModeAndOptions()
        {
            WpfTestHost.Run(() =>
            {
                var item = GalleryCatalog.FindItem("TwoPaneView");
                Assert.IsNotNull(item);
                Assert.AreEqual("Layout", item.GroupId);
                Assert.AreEqual("ModernWpf.Controls", item.ApiNamespace);

                var examples = LayoutSampleFactory.CreateExamples("TwoPaneView");
                Assert.AreEqual(1, examples.Count);
                Assert.AreEqual(
                    "A TwoPaneView that adapts between single, wide, and tall layouts.",
                    examples[0].HeaderText);

                var content = (UIElement)examples[0].ExampleContent;
                var view = FindLogicalDescendants<Mux.TwoPaneView>(content).Single();
                var status = FindLogicalDescendants<TextBlock>(content).Single(textBlock =>
                    AutomationProperties.GetAutomationId(textBlock) == "GallerySample_TwoPaneView_Mode");
                var options = (UIElement)examples[0].OptionsContent;
                var width = FindLogicalDescendants<Slider>(options).Single(slider =>
                    AutomationProperties.GetAutomationId(slider) == "GallerySample_TwoPaneView_Width");
                var height = FindLogicalDescendants<Slider>(options).Single(slider =>
                    AutomationProperties.GetAutomationId(slider) == "GallerySample_TwoPaneView_Height");
                Assert.AreEqual("GallerySample_TwoPaneView_View", AutomationProperties.GetAutomationId(view));
                Assert.AreEqual(600d, width.Value);
                Assert.AreEqual(330d, height.Value);

                var window = new Window
                {
                    Content = content,
                    Width = 900,
                    Height = 600,
                    Left = -32000,
                    Top = -32000,
                    ShowInTaskbar = false,
                    WindowStyle = WindowStyle.None
                };
                try
                {
                    window.Show();
                    WpfTestHost.DoEvents();
                    window.UpdateLayout();
                    Assert.AreEqual(Mux.TwoPaneViewMode.Wide, view.Mode);
                    Assert.AreEqual("Current mode: Wide", status.Text);

                    width.Value = 400;
                    window.UpdateLayout();
                    WpfTestHost.DoEvents();
                    Assert.AreEqual(400d, view.Width);
                    Assert.AreEqual(Mux.TwoPaneViewMode.Tall, view.Mode);
                    Assert.AreEqual("Current mode: Tall", status.Text);

                    height.Value = 200;
                    window.UpdateLayout();
                    WpfTestHost.DoEvents();
                    Assert.AreEqual(200d, view.Height);
                    Assert.AreEqual(Mux.TwoPaneViewMode.SinglePane, view.Mode);
                    Assert.AreEqual("Current mode: SinglePane", status.Text);
                }
                finally
                {
                    window.Close();
                }
            });
        }

        [TestMethod]
        public void GalleryPortPinsProductSourceAndDocumentsMissingUpstreamPage()
        {
            var root = FindRepoRoot();
            var audit = File.ReadAllText(Path.Combine(root, "docs", "twopaneview-winui3-source-audit.md"));
            var factory = File.ReadAllText(Path.Combine(root, "ModernWpf.Gallery", "Pages", "LayoutSampleFactory.cs"));
            var catalog = File.ReadAllText(Path.Combine(root, "ModernWpf.Gallery", "Samples", "Data", "ControlInfoData.json"));

            StringAssert.Contains(audit, "6a556bb28fc227acd2ec8fe67ee64853f559084b");
            StringAssert.Contains(audit, "has no current `TwoPaneView` page");
            StringAssert.Contains(factory, "Current mode: ");
            StringAssert.Contains(factory, "Wide configuration");
            StringAssert.Contains(factory, "Tall configuration");
            StringAssert.Contains(factory, "View width");
            StringAssert.Contains(factory, "View height");
            StringAssert.Contains(catalog, "does not infer a physical display hinge");
        }

        private static System.Collections.Generic.IEnumerable<T> FindLogicalDescendants<T>(DependencyObject root)
            where T : DependencyObject
        {
            if (root is T match)
            {
                yield return match;
            }

            foreach (var child in LogicalTreeHelper.GetChildren(root).OfType<DependencyObject>())
            {
                foreach (var descendant in FindLogicalDescendants<T>(child))
                {
                    yield return descendant;
                }
            }
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
