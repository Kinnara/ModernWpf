using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModernWpf.WinUI.Tests.PersonPicture;

[TestClass]
public class PersonPictureSourceAuditTests
{
    [TestMethod]
    public void CurrentWinUI3PersonPictureParityIsPinnedByAudit()
    {
        var repoRoot = FindRepoRoot();
        var audit = Read(repoRoot, "docs", "personpicture-winui3-source-audit.md");
        var control = Read(repoRoot, "ModernWpf.Controls", "PersonPicture", "PersonPicture.cs");
        var properties = Read(repoRoot, "ModernWpf.Controls", "PersonPicture", "PersonPicture.properties.g.cs");
        var template = Read(repoRoot, "ModernWpf.Controls", "PersonPicture", "PersonPicture.xaml");
        var peer = Read(repoRoot, "ModernWpf.Controls", "PersonPicture", "PersonPictureAutomationPeer.cs");
        var galleryFactory = Read(repoRoot, "ModernWpf.Gallery", "Pages", "MediaSampleFactory.cs");
        var harness = Read(repoRoot, "tools", "visual-checks", "Run-GalleryVisualChecks.ps1");

        StringAssert.Contains(audit, "de3e767333c2f0717a6a70cb22bd192ced5ad885");
        StringAssert.Contains(audit, "8463f45162149de0ec3ad7df752596893fe3e13e");
        StringAssert.Contains(audit, "29f62479d5c046a0b854a5868e5a7cd484572d87");
        StringAssert.Contains(audit, "14a4a1a2b8ddc527dc4a7d5f7e743d7c2bc97db7");
        StringAssert.Contains(audit, "fa62c1631427dec8bd0c3e92b9bb4e9fac9fd067");
        StringAssert.Contains(audit, "7747fd1000a0db817a7ceba76c70b1b61199eb6f");
        StringAssert.Contains(audit, "60e636281541e844a97827a563e5ed46b2cfc716");
        StringAssert.Contains(audit, "1bd0a1c6f84ab4565c2b2c8fd10ba092e4ebb98c");
        StringAssert.Contains(audit, "228efb45b2e22ae1d482304ecf9b2a6af1496011");
        StringAssert.Contains(audit, "f9dbcaf5193ff2226f370518c77b50c5c0777ea2");
        StringAssert.Contains(audit, "e0e8b9aabb828e0d67c6ac7b6c2aae8db705e4af");
        StringAssert.Contains(audit, "f5918072ecb8c1ecc8fda46432ff403d0c127969");
        StringAssert.Contains(audit, "4a68256600df687ad13177f6c62cd1b4fae26b8f");
        StringAssert.Contains(audit, "d705d1c283d05cdf38ee710795924431706f91f6");
        StringAssert.Contains(audit, "4a822bd76ff2be755f1247d68f63734a49860fe9");
        StringAssert.Contains(audit, "265630278d14a4598d1d146b9121bf6a7b301e4c");
        StringAssert.Contains(audit, "129f1e913ef06ec42375259282a95b1c19c0052a");
        StringAssert.Contains(audit, "artifacts/visual-checks/20260718-132501-654-69848/report.md");
        StringAssert.Contains(audit, "artifacts/visual-checks/20260718-132425-300-73904/report.md");
        StringAssert.Contains(audit, "| `96x96` / `96x96` | `0.39` |");
        StringAssert.Contains(audit, "| `96x96` / `96x96` | `0.35` |");

        StringAssert.Contains(control, "templateSettings.ActualInitials = initials;");
        StringAssert.Contains(control, "var imageBrush = templateSettings.ActualImageBrush;");
        StringAssert.Contains(control, "VisualStateManager.GoToState(this, \"BadgeWithImageSource\", false);");
        StringAssert.Contains(control, "GetLocalizedPluralBadgeItemStringResource(BadgeNumber)");
        StringAssert.Contains(properties, "public static readonly DependencyProperty PreferSmallImageProperty");
        StringAssert.Contains(properties, "new PropertyMetadata(false, OnPreferSmallImagePropertyChanged)");

        StringAssert.Contains(template, "x:Key=\"DefaultPersonPictureStyle\"");
        StringAssert.Contains(template, "x:Name=\"PersonPictureEllipse\"");
        StringAssert.Contains(template, "x:Name=\"InitialsTextBlock\"");
        StringAssert.Contains(template, "x:Name=\"BadgeNumberTextBlock\"");
        StringAssert.Contains(template, "x:Name=\"NoPhotoOrInitials\"");
        StringAssert.Contains(template, "x:Name=\"BadgeWithImageSource\"");
        StringAssert.Contains(template, "Value=\"&#xE77B;\"");

        StringAssert.Contains(peer, "return AutomationControlType.Text;");
        StringAssert.Contains(peer, "return nameof(PersonPicture);");

        StringAssert.Contains(galleryFactory, "Select different looks for the person picture.");
        StringAssert.Contains(galleryFactory, "Assets/SampleMedia/shoulder-tap-static-payload.png");
        StringAssert.Contains(galleryFactory, "Name = \"ProfileImageRadio\"");
        StringAssert.Contains(galleryFactory, "Name = \"DisplayNameRadio\"");
        StringAssert.Contains(galleryFactory, "Name = \"InitialsRadio\"");
        StringAssert.Contains(galleryFactory, "personPicture.DisplayName = \"Jane Doe\";");
        StringAssert.Contains(galleryFactory, "personPicture.Initials = \"SB\";");

        StringAssert.Contains(harness, "function New-PersonPictureReferencePrimaryCrop");
        StringAssert.Contains(harness, "\"PersonPicture\" { return 0.5 }");
        StringAssert.Contains(harness, "\"PersonPicture\" { return 0 }");
        StringAssert.Contains(harness, "Cropped the WinUI PersonPicture avatar from the first example content.");
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
