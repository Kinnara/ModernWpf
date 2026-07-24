using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModernWpf.WinUI.Tests.IconSource;

[TestClass]
public class IconSourceSourceAuditTests
{
    [TestMethod]
    public void CurrentWinUI3IconSourceAndImageIconParityIsPinnedByAudit()
    {
        var repoRoot = FindRepoRoot();
        var audit = Read(repoRoot, "docs", "iconsource-imageicon-winui3-source-audit.md");
        var iconSource = Read(repoRoot, "ModernWpf", "IconSource", "IconSource.cs");
        var imageIconSource = Read(repoRoot, "ModernWpf", "IconSource", "ImageIconSource.cs");
        var imageIcon = Read(repoRoot, "ModernWpf", "IconElement", "ImageIcon.cs");
        var sharedHelpers = Read(repoRoot, "ModernWpf.Controls", "Common", "SharedHelpers.cs");
        var harness = Read(repoRoot, "tools", "visual-checks", "Run-GalleryVisualChecks.ps1");

        StringAssert.Contains(audit, "de3e767333c2f0717a6a70cb22bd192ced5ad885");
        StringAssert.Contains(audit, "16737fe3a48cd8fc9b337a13e1b04e17afd97882");
        StringAssert.Contains(audit, "29f62479d5c046a0b854a5868e5a7cd484572d87");
        StringAssert.Contains(audit, "39cda55d953431ffa920239051af63a0723ccca7");
        StringAssert.Contains(audit, "e34d4401bf097635cfdc313cd838bbe94985dc64");
        StringAssert.Contains(audit, "9eac297bd4547d04f056454a16ac98be405d59b9");
        StringAssert.Contains(audit, "100x89");
        StringAssert.Contains(audit, "1.49");
        StringAssert.Contains(audit, "1.17");

        StringAssert.Contains(iconSource, "if (Foreground is { } foreground)");
        StringAssert.Contains(iconSource, "element.Foreground = foreground;");
        StringAssert.Contains(iconSource, "new WeakReference<IconElement>(element)");
        StringAssert.Contains(iconSource, "element.SetValue(iconProp, args.NewValue)");

        StringAssert.Contains(imageIconSource, "return ImageIcon.SourceProperty;");
        StringAssert.Contains(imageIcon, "Children.Add(_image);");
        StringAssert.Contains(imageIcon, "_image.Source = Source;");
        StringAssert.Contains(sharedHelpers, "else if (iconSource is ImageIconSource imageIconSource)");
        StringAssert.Contains(sharedHelpers, "imageIcon.Source = imageIconSource.ImageSource;");

        StringAssert.Contains(harness, "\"ImageIcon\" { return \"GallerySample_IconElement_ImageExample1\" }");
        StringAssert.Contains(harness, "\"ImageIcon\" { return \"ImageExample1\" }");
        StringAssert.Contains(harness, "\"ImageIcon\" { return 2.0 }");
        StringAssert.Contains(harness, "\"ImageIcon\" { return 0 }");
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
