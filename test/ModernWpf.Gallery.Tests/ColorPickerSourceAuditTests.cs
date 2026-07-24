using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModernWpf.Gallery.Tests
{
    [TestClass]
    public class ColorPickerSourceAuditTests
    {
        [TestMethod]
        public void CurrentColorPickerSourcesGalleryAccessibilityAndPixelGatesArePinnedByAudit()
        {
            var root = FindRepoRoot();
            var audit = Read(root, "docs", "colorpicker-winui3-source-audit.md");
            var sample = Read(root, "ModernWpf.Gallery", "Pages", "BasicInputSampleFactory.cs");
            var sampleTests = Read(root, "test", "ModernWpf.Gallery.Tests", "GalleryAutomationHookTests.cs");
            var harness = Read(root, "tools", "visual-checks", "Run-GalleryVisualChecks.ps1");
            var snippet = Read(root, "ModernWpf.Gallery", "Samples", "SampleCode", "ColorPicker", "ColorPickerProperties.txt");

            StringAssert.Contains(audit, "de3e767333c2f0717a6a70cb22bd192ced5ad885");
            StringAssert.Contains(audit, "886a769b57c3b8c41ddc841df4040b1967fce778");
            StringAssert.Contains(audit, "68f539f2388355850077999d1657b8cecdcbf6cb");
            StringAssert.Contains(audit, "79e667cdd62edc25bfbae110302f79c61cd39b42");
            StringAssert.Contains(audit, "fb5f7bd1a83acafccc63e43c0451d8775d82afcd");
            StringAssert.Contains(audit, "cf25e64606ef7b3e2bfe4a242bd4dc1a21467d9e");
            StringAssert.Contains(audit, "ff69cb7eb766097830b201c4b0785041027fa499");
            StringAssert.Contains(audit, "29f62479d5c046a0b854a5868e5a7cd484572d87");
            StringAssert.Contains(audit, "3d0e3397373b6238bf3c28bb3fda8c1a572f76a6");
            StringAssert.Contains(audit, "c3d167e1c00b026d12ed7faca1a08f30f790930a");
            StringAssert.Contains(audit, "96ab9df993aa5234453f483bba56cf81f385753e");

            StringAssert.Contains(sample, "CreateColorPickerExamples(sampleSnippets)");
            StringAssert.Contains(sample, "ColorPicker\\\\ColorPickerProperties.txt");
            StringAssert.Contains(sample, "embedOptionsInContent: false");
            StringAssert.Contains(sample, "optionsContent");
            Assert.IsFalse(sample.Contains("private const string ColorPickerPropertiesXaml", StringComparison.Ordinal));
            StringAssert.Contains(snippet, "--- header");
            StringAssert.Contains(snippet, "--- xaml");
            StringAssert.Contains(snippet, "ColorSpectrumShape=\"$(ColorSpectrumShape)\"");

            StringAssert.Contains(sampleTests, "new CheckBoxAutomationPeer(moreButtonCheck)");
            StringAssert.Contains(sampleTests, "new RadioButtonsAutomationPeer(shapeRadioButtons)");
            StringAssert.Contains(sampleTests, "AutomationControlType.Slider");
            StringAssert.Contains(sampleTests, "PatternInterface.Value");
            StringAssert.Contains(sampleTests, "PatternInterface.Toggle");

            StringAssert.Contains(harness, "\"ColorPicker\" { return \"ColorPicker editor surface\" }");
            StringAssert.Contains(harness, "\"ColorPicker\" { return 4.0 }");
            StringAssert.Contains(harness, "\"ColorPicker\" { return 0 }");
            StringAssert.Contains(harness, "function New-ColorPickerModernPrimaryCrop");
            StringAssert.Contains(harness, "function New-ColorPickerReferencePrimaryCrop");
            StringAssert.Contains(harness, "function New-ColorPickerStateInteractionCrop");
            StringAssert.Contains(harness, "$Height = [Math]::Max($Height, 900)");
            StringAssert.Contains(harness, "$primarySource = \"HexTextBox\"");
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
