using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModernWpf.Gallery.Tests
{
    [TestClass]
    public class HyperlinkButtonSourceAuditTests
    {
        [TestMethod]
        public void CurrentHyperlinkButtonSourcesBehaviorAccessibilityAndPixelGateArePinnedByAudit()
        {
            var root = FindRepoRoot();
            var audit = Read(root, "docs", "hyperlinkbutton-winui3-source-audit.md");
            var control = Read(root, "ModernWpf.Controls", "HyperlinkButton", "HyperlinkButton.cs");
            var peer = Read(root, "ModernWpf.Controls", "HyperlinkButton", "HyperlinkButtonAutomationPeer.cs");
            var template = Read(root, "ModernWpf.Controls", "HyperlinkButton", "HyperlinkButton.xaml");
            var sample = Read(root, "ModernWpf.Gallery", "Pages", "BasicInputSampleFactory.cs");
            var sampleTests = Read(root, "test", "ModernWpf.Gallery.Tests", "GalleryAutomationHookTests.cs");
            var harness = Read(root, "tools", "visual-checks", "Run-GalleryVisualChecks.ps1");
            var navigateSnippet = Read(root, "ModernWpf.Gallery", "Samples", "SampleCode", "HyperlinkButton", "HyperlinkButtonNavigate.txt");
            var clickSnippet = Read(root, "ModernWpf.Gallery", "Samples", "SampleCode", "HyperlinkButton", "HyperlinkButtonClick.txt");

            StringAssert.Contains(audit, "de3e767333c2f0717a6a70cb22bd192ced5ad885");
            StringAssert.Contains(audit, "c70471c511a0168b61dcca13af9556465f26b673");
            StringAssert.Contains(audit, "8463f45162149de0ec3ad7df752596893fe3e13e");
            StringAssert.Contains(audit, "c0d59563ffb684a8f492715bb66c7bfa89a68313");
            StringAssert.Contains(audit, "cc561812c862c252ab41c5ce5a4a47d11024f563");
            StringAssert.Contains(audit, "93b5efd391803a229e63e55c315e5675fef4362e");
            StringAssert.Contains(audit, "3861ddde4574c2519b0e4f64d296db5d2dd2b5d5");
            StringAssert.Contains(audit, "29f62479d5c046a0b854a5868e5a7cd484572d87");
            StringAssert.Contains(audit, "89d2864e4545f894f6c80b0e2d41017112f348af");
            StringAssert.Contains(audit, "cab77b96249fd2fd7c05958efaa8abf234fd5de9");
            StringAssert.Contains(audit, "a28ff06e13d6fc028692829546039be7c71efd7c");
            StringAssert.Contains(audit, "5449d629c81cffb74e7b922e74cacf95577f365a");
            StringAssert.Contains(audit, "nine zero-duration");

            StringAssert.Contains(control, "AutomationEvents.InvokePatternOnInvoked");
            StringAssert.Contains(control, "base.OnClick()");
            StringAssert.Contains(control, "UseShellExecute = true");
            StringAssert.Contains(peer, "AutomationControlType.Hyperlink");
            StringAssert.Contains(peer, "return \"Hyperlink\"");
            StringAssert.Contains(peer, "PatternInterface.Invoke");
            StringAssert.Contains(peer, "throw new ElementNotEnabledException()");
            StringAssert.Contains(template, "x:Key=\"DefaultHyperlinkButtonStyle\"");
            StringAssert.Contains(template, "Padding\" Value=\"{DynamicResource ButtonPadding}");
            StringAssert.Contains(template, "x:Name=\"PointerOver\"");
            StringAssert.Contains(template, "x:Name=\"Pressed\"");
            StringAssert.Contains(template, "x:Name=\"Disabled\"");

            StringAssert.Contains(sample, "CreateHyperlinkButtonExamples(sampleSnippets)");
            StringAssert.Contains(sample, "HyperlinkButton\\\\HyperlinkButtonNavigate.txt");
            StringAssert.Contains(sample, "HyperlinkButton\\\\HyperlinkButtonClick.txt");
            StringAssert.Contains(sample, "\"DisableControl1\"");
            StringAssert.Contains(sample, "\"Disable hyperlink button\"");
            StringAssert.Contains(sample, "uriButton.IsEnabled = !isChecked");
            StringAssert.Contains(navigateSnippet, "Content=\"Microsoft home page\"");
            StringAssert.Contains(clickSnippet, "Content=\"ToggleButton\"");

            StringAssert.Contains(sampleTests, "new HyperlinkButtonAutomationPeer(uriButton)");
            StringAssert.Contains(sampleTests, "new HyperlinkButtonAutomationPeer(clickButton)");
            StringAssert.Contains(sampleTests, "PatternInterface.Invoke");
            StringAssert.Contains(sampleTests, "new CheckBoxAutomationPeer(disableControl)");
            StringAssert.Contains(sampleTests, "PatternInterface.Toggle");
            StringAssert.Contains(sampleTests, "Assert.ThrowsException<ElementNotEnabledException>");

            StringAssert.Contains(harness, "\"HyperlinkButton\" { return $true }");
            StringAssert.Contains(harness, "\"HyperlinkButton\" { return \"Control1\" }");
            StringAssert.Contains(harness, "\"HyperlinkButton\" { return 1.6 }");
        }

        private static string Read(string root, params string[] parts)
        {
            var path = root;
            foreach (var part in parts)
            {
                path = Path.Combine(path, part);
            }

            return File.ReadAllText(path);
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
