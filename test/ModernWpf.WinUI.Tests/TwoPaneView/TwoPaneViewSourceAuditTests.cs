using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModernWpf.WinUI.Tests.TwoPaneView
{
    [TestClass]
    public class TwoPaneViewSourceAuditTests
    {
        [TestMethod]
        public void CurrentWinUI3TwoPaneViewParityIsPinnedByAudit()
        {
            var root = FindRepoRoot();
            var audit = Read(root, "docs", "twopaneview-winui3-source-audit.md");
            var control = Read(root, "ModernWpf.Controls", "TwoPaneView", "TwoPaneView.cs");
            var template = Read(root, "ModernWpf.Controls", "TwoPaneView", "TwoPaneView.xaml");

            StringAssert.Contains(audit, "6a556bb28fc227acd2ec8fe67ee64853f559084b");
            StringAssert.Contains(audit, "3669519356c67f1376152c33ed8ea45003a91f3a");
            foreach (var blob in new[]
            {
            "b609b7af6c0bf707e9428c415f3b5422d2997808",
            "609166c6a1537b2bb213c10ba3f47c2bf7dbda79",
            "9c81354814cc82d10c10e82ad95c95fb090ca7e8",
            "c7821c2691543cf5854ae4306c24ecf1345fc1a6",
            "24171b6c8443ac7af71c4e98728b3f33b62ce234",
            "21e76072e07d47e75b660e0f1f3eeed4878b06e2",
            "1b4fcfeb7fab71e5ed9c10a16c1f50e0e9b57ed2",
            "75abbb3614e260a75c23a968088aec170423516e",
            "916b6b9052141395a4f8e9c0e3e7863fa53d6d64",
            "d87a0b003405753e75734b1e6b0fdbc679430906",
            "f6169a907f5baf14c514e1f0a66874225249199e",
            "d8e0dfe6ed4f7fad84237ab32b7eb185d7646795"
        })
            {
                StringAssert.Contains(audit, blob);
            }

            foreach (var property in new[]
            {
            "Pane1Property",
            "Pane2Property",
            "Pane1LengthProperty",
            "Pane2LengthProperty",
            "PanePriorityProperty",
            "ModeProperty",
            "WideModeConfigurationProperty",
            "TallModeConfigurationProperty",
            "MinWideModeWidthProperty",
            "MinTallModeHeightProperty"
        })
            {
                StringAssert.Contains(control, "DependencyProperty " + property);
            }

            StringAssert.Contains(control, "ActualWidth > MinWideModeWidth");
            StringAssert.Contains(control, "ActualHeight > MinTallModeHeight");
            StringAssert.Contains(control, "ModeChanged?.Invoke(this, this);");
            StringAssert.Contains(control, "new PropertyMetadata(641d");
            StringAssert.Contains(control, "double.IsNaN(value) || value < 0 ? 0d : value");

            foreach (var state in new[]
            {
            "ViewMode_OneOnly",
            "ViewMode_TwoOnly",
            "ViewMode_LeftRight",
            "ViewMode_RightLeft",
            "ViewMode_TopBottom",
            "ViewMode_BottomTop"
        })
            {
                StringAssert.Contains(template, state);
            }

            StringAssert.Contains(audit, "no `ApplicationView`");
            StringAssert.Contains(audit, "does not invent fake spanning");
            StringAssert.Contains(audit, "has no current `TwoPaneView` page");
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
