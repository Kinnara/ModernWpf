using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModernWpf.WinUI.Tests.NumberBox;

[TestClass]
public class NumberBoxSourceAuditTests
{
    [TestMethod]
    public void CurrentWinUI3NumberBoxParityIsPinnedByAudit()
    {
        var repoRoot = FindRepoRoot();
        var audit = Read(repoRoot, "docs", "numberbox-winui3-source-audit.md");
        var numberBox = Read(repoRoot, "ModernWpf.Controls", "NumberBox", "NumberBox.cs");
        var generatedProperties = Read(repoRoot, "ModernWpf.Controls", "NumberBox", "NumberBox.properties.g.cs");
        var rounder = Read(repoRoot, "ModernWpf.Controls", "NumberBox", "DefaultNumberRounder.cs");
        var template = Read(repoRoot, "ModernWpf.Controls", "NumberBox", "NumberBox.xaml");
        var peer = Read(repoRoot, "ModernWpf.Controls", "NumberBox", "NumberBoxAutomationPeer.cs");
        var galleryFactory = Read(repoRoot, "ModernWpf.Gallery", "Pages", "TextSampleFactory.cs");
        var harness = Read(repoRoot, "tools", "visual-checks", "Run-GalleryVisualChecks.ps1");

        StringAssert.Contains(audit, "de3e767333c2f0717a6a70cb22bd192ced5ad885");
        StringAssert.Contains(audit, "8463f45162149de0ec3ad7df752596893fe3e13e");
        StringAssert.Contains(audit, "c7e2f98d978c81c2b7b0054eb042a6f8f816ec9c");
        StringAssert.Contains(audit, "beabd047460bf5d43a41fcf8bddf7730188bd5a7");
        StringAssert.Contains(audit, "49b4d5326b4deba8c036e63a7e676715a5de4f3a");
        StringAssert.Contains(audit, "b4e5f2cafeae04f3a799123d48dca9516832becb");
        StringAssert.Contains(audit, "29f62479d5c046a0b854a5868e5a7cd484572d87");
        StringAssert.Contains(audit, "14a4a1a2b8ddc527dc4a7d5f7e743d7c2bc97db7");
        StringAssert.Contains(audit, "4bf79c2f673991f328230e60b218df60b3cabddb");
        StringAssert.Contains(audit, "d70025ee3655bbb03c2580470d596a31b3e07bcc");
        StringAssert.Contains(audit, "ac318960b35b1aad35892c4b85043fd7b29cad66");
        StringAssert.Contains(audit, "41e8febeaa1015777756586a05e0bef45ce21b59");
        StringAssert.Contains(audit, "9158e28b1bbbb736c57d00f468b4af87b8dfdb3e");
        StringAssert.Contains(audit, "b880e1c66ff99c2b711329a382a51cf22a859ea9");
        StringAssert.Contains(audit, "b7a63cfa89404f604300a79c679b4b395a977dcb");
        StringAssert.Contains(audit, "101a73a394586a997e5211486b6b4c78ccc7c0fb");
        StringAssert.Contains(audit, "6d7100f3a5ac67ecdef3ad6ddc183e10346e060d");
        StringAssert.Contains(audit, "ad12b22405f5537a69c5731b68336af45a6d835f");
        StringAssert.Contains(audit, "d76405d99f51fa57220753f0c8f5775c2b3f3fcb");
        StringAssert.Contains(audit, "c621e68d731f9e807f3e4e58cb35850ca90b4f69");
        StringAssert.Contains(audit, "83f06ada54b15cb4d7debbace7de08edc42b3b3f");
        StringAssert.Contains(audit, "df38c7a189f7aac42f2ba6767aa72d541edafa43");
        StringAssert.Contains(audit, "artifacts/visual-checks/20260718-121201-546-28252/report.md");
        StringAssert.Contains(audit, "artifacts/visual-checks/20260718-121231-516-71104/report.md");
        StringAssert.Contains(audit, "| `132x59` / `132x59` | `1.84` |");
        StringAssert.Contains(audit, "| `132x59` / `132x59` | `1.74` |");
        StringAssert.Contains(audit, "| `152x79` / `152x79` | `1.69` |");
        StringAssert.Contains(audit, "| `152x79` / `152x79` | `1.02` |");

        StringAssert.Contains(numberBox, "UnhookEvents();");
        StringAssert.Contains(numberBox, "m_popupRepositionHelper = new PopupRepositionHelper(m_popup, this);");
        StringAssert.Contains(numberBox, "m_displayRounder.SignificantDigits = 10;");
        StringAssert.Contains(numberBox, "var text = m_textBox.Text.Trim();");
        StringAssert.Contains(numberBox, "ReevaluateForwardedUIAProperties();");
        StringAssert.Contains(numberBox, "spinButtonsColumn.Width = spinButtonMode == NumberBoxSpinButtonPlacementMode.Inline");
        StringAssert.Contains(generatedProperties, "if (!double.IsNaN(value) || !double.IsNaN(Value))");
        StringAssert.Contains(rounder, "var singleValue = (float)value;");
        StringAssert.Contains(rounder, "singleValue.ToString(\"R\", CultureInfo.InvariantCulture)");
        StringAssert.Contains(rounder, "value.ToString(\"G\" + SignificantDigits, CultureInfo.InvariantCulture)");

        StringAssert.Contains(template, "x:Name=\"InputEater\"");
        StringAssert.Contains(template, "x:Name=\"PopupIndicator\"");
        StringAssert.Contains(template, "x:Name=\"UpSpinButton\"");
        StringAssert.Contains(template, "x:Name=\"PopupUpSpinButton\"");
        StringAssert.Contains(template, "MinHeight=\"19\"");
        StringAssert.Contains(template, "<ui:VisualStateEx x:Name=\"SpinButtonsVisible\">");
        StringAssert.Contains(template, "Target=\"UpSpinButton.IsEnabled\" Value=\"False\"");
        StringAssert.Contains(template, "Content=\"&#xE70E;\"");
        StringAssert.Contains(template, "Content=\"&#xE70D;\"");

        StringAssert.Contains(peer, "patternInterface == PatternInterface.RangeValue");
        StringAssert.Contains(peer, "return nameof(NumberBox);");
        StringAssert.Contains(peer, "name = numberBox.Header?.ToString();");
        StringAssert.Contains(peer, "return AutomationControlType.Spinner;");
        StringAssert.Contains(peer, "RangeValuePatternIdentifiers.ValueProperty");

        StringAssert.Contains(galleryFactory, "A NumberBox that evaluates expressions.");
        StringAssert.Contains(galleryFactory, "Name = \"NumberBoxSpinButtonPlacementExample\"");
        StringAssert.Contains(galleryFactory, "AutomationProperties.SetName(numberBox, \"NumberBox with spin button\");");
        StringAssert.Contains(galleryFactory, "Name = \"SpinButtonPlacementGroup\"");
        StringAssert.Contains(galleryFactory, "Name = \"FormattedNumberBox\"");
        StringAssert.Contains(galleryFactory, "NumberFormatter = new QuarterIncrementNumberFormatter()");

        StringAssert.Contains(harness, "\"NumberBox\" { return 2.5 }");
        StringAssert.Contains(harness, "\"NumberBox\" { return 2.0 }");
        StringAssert.Contains(harness, "\"NumberBox\" { return 0 }");
        StringAssert.Contains(harness, "\"NumberBox\" { return 10.0 }");
    }

    private static string Read(string repoRoot, params string[] parts)
    {
        var path = repoRoot;
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
