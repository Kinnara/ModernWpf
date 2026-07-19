using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModernWpf.WinUI.Tests.ListView;

[TestClass]
public class ListViewSourceAuditTests
{
    [TestMethod]
    public void CurrentWinUI3ListViewGridViewParityIsPinnedByAudit()
    {
        var repoRoot = FindRepoRoot();
        var audit = Read(repoRoot, "docs", "listview-winui3-source-audit.md");
        var listPeer = Read(repoRoot, "ModernWpf.Controls", "ListView", "ListViewBaseAutomationPeer.cs");
        var itemPeer = Read(repoRoot, "ModernWpf.Controls", "ListView", "ListViewBaseItemAutomationPeer.cs");
        var apiTests = Read(repoRoot, "test", "ModernWpf.WinUI.Tests", "ListView", "ListViewApiTests.cs");
        var galleryFactory = Read(repoRoot, "ModernWpf.Gallery", "Pages", "CollectionsSampleFactory.cs");
        var galleryTests = Read(repoRoot, "test", "ModernWpf.Gallery.Tests", "GalleryAutomationHookTests.cs");
        var harness = Read(repoRoot, "tools", "visual-checks", "Run-GalleryVisualChecks.ps1");

        StringAssert.Contains(audit, "de3e767333c2f0717a6a70cb22bd192ced5ad885");
        StringAssert.Contains(audit, "c70471c511a0168b61dcca13af9556465f26b673");
        StringAssert.Contains(audit, "8463f45162149de0ec3ad7df752596893fe3e13e");
        StringAssert.Contains(audit, "350c26f0410309eb7367b363cab82cba7735a7ea");
        StringAssert.Contains(audit, "49b4d53265cc2283ae5d5d6a10ab2f515417452b");
        StringAssert.Contains(audit, "29f62479d5c046a0b854a5868e5a7cd484572d87");
        StringAssert.Contains(audit, "14a4a1a2b8ddc527dc4a7d5f7e743d7c2bc97db7");
        StringAssert.Contains(audit, "7758954dccfead1b0f2ce7873c000c50390ca17f");
        StringAssert.Contains(audit, "3e588a5e862f0459139afe1e740af4e9041d2e1c");
        StringAssert.Contains(audit, "ea3e145e914010979f8018530a9543441aae3f12");
        StringAssert.Contains(audit, "b292e8fa9aa1f6024d16b8a5eb452e4427d45bda");
        StringAssert.Contains(audit, "a4ca9accccc27d9f0e7fdcfbddc2a5b8e306360d");
        StringAssert.Contains(audit, "c02c81ecb25fb3d1fdeadb903b1043ba81d61fed");
        StringAssert.Contains(audit, "248a2341c9c876a398715723ac6b7924d1271e3d");
        StringAssert.Contains(audit, "6708537e07293c2472ea07cc6e9a373a01afe2a4");
        StringAssert.Contains(audit, "a749cb7167629b7a89e297ec317fbda32ddb80ce");
        StringAssert.Contains(audit, "6114d9dfab83f1359254083b9dd277ae55707eea");
        StringAssert.Contains(audit, "c600ea6854faf485303abe378535372435bf6c9a");
        StringAssert.Contains(audit, "artifacts/visual-checks/20260718-200820-485-79776/report.md");
        StringAssert.Contains(audit, "artifacts/visual-checks/20260718-200857-428-3920/report.md");
        StringAssert.Contains(audit, "artifacts/gallery-recordings/20260718-200949-892/report.md");
        StringAssert.Contains(audit, "artifacts/gallery-recordings/20260718-201017-558/report.md");
        StringAssert.Contains(audit, "`657x412` / `657x412`, `1.61`");
        StringAssert.Contains(audit, "`657x412` / `657x412`, `1.60`");
        StringAssert.Contains(audit, "`122x18` / `120x19`, `6.40`");
        StringAssert.Contains(audit, "`122x18` / `120x19`, `6.64`");
        StringAssert.Contains(audit, "`0.795` maximum local output delta");
        StringAssert.Contains(audit, "`1.085` maximum local output delta");
        StringAssert.Contains(audit, "dxaml\\xcp\\dxaml\\lib\\ListViewBaseItemAutomationPeer_Partial.cpp");
        Assert.IsFalse(audit.Contains("`src\\dxaml", StringComparison.Ordinal));

        StringAssert.Contains(listPeer, "return OwnerListView is GridView ? nameof(GridView) : nameof(ListView);");
        StringAssert.Contains(listPeer, "return AutomationControlType.List;");
        StringAssert.Contains(itemPeer, "return _selectorAutomationPeer.OwnerListView.IsItemClickEnabled ? this : null;");
        StringAssert.Contains(itemPeer, "return nameof(GridViewItem);");
        StringAssert.Contains(itemPeer, "return nameof(ListViewItem);");
        StringAssert.Contains(itemPeer, "return AutomationControlType.ListItem;");

        StringAssert.Contains(apiTests, "AutomationPeersMatchWinUIClassTypesAndConditionalInvokePattern");
        StringAssert.Contains(apiTests, "GridViewItemAutomationInvokeRaisesItemClick");
        StringAssert.Contains(apiTests, "ItemClickUsesOwnContainerContentAndSpaceKey");

        StringAssert.Contains(galleryFactory, "Basic GridView with Simple DataTemplate");
        StringAssert.Contains(galleryFactory, "GridView with Layout Customization");
        StringAssert.Contains(galleryFactory, "Content inside of a GridView.");
        StringAssert.Contains(galleryFactory, "image.SetBinding(AutomationProperties.NameProperty, new Binding(\"Title\"));");
        StringAssert.Contains(galleryTests, "Assert.AreEqual(\"GridView\", basicGridPeer.GetClassName());");
        StringAssert.Contains(galleryTests, "Assert.AreEqual(\"GridViewItem\", basicGridItemPeer.GetClassName());");
        StringAssert.Contains(galleryTests, "Assert.IsNull(contentGridItemPeer.GetPattern(PatternInterface.Invoke));");
        StringAssert.Contains(galleryTests, "Assert.AreEqual(\"Item 1\", AutomationProperties.GetName(basicImage));");

        StringAssert.Contains(harness, "\"GridView\" { return 2.0 }");
        StringAssert.Contains(harness, "\"GridView\" { return 0 }");
        StringAssert.Contains(harness, "\"GridView\" { return 8.0 }");
        StringAssert.Contains(harness, "\"GridView\" { return 4 }");
        StringAssert.Contains(harness, "return \"GallerySample_GridView_ClickOutput0\"");
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

