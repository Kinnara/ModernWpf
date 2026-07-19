using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModernWpf.WinUI.Tests.AutoSuggestBox;

[TestClass]
public class AutoSuggestBoxSourceAuditTests
{
    [TestMethod]
    public void CurrentWinUI3AutoSuggestBoxParityIsPinnedByAudit()
    {
        var repoRoot = FindRepoRoot();
        var audit = Read(repoRoot, "docs", "autosuggestbox-winui3-source-audit.md");
        var control = Read(repoRoot, "ModernWpf.Controls", "AutoSuggestBox", "AutoSuggestBox.cs");
        var eventArgs = Read(repoRoot, "ModernWpf.Controls", "AutoSuggestBox", "AutoSuggestBoxTextChangedEventArgs.cs");
        var list = Read(repoRoot, "ModernWpf.Controls", "AutoSuggestBox", "AutoSuggestBoxListView.cs");
        var template = Read(repoRoot, "ModernWpf.Controls", "AutoSuggestBox", "AutoSuggestBox.xaml");
        var shadowChrome = Read(repoRoot, "ModernWpf", "Controls", "Primitives", "ThemeShadowChrome.cs");
        var peer = Read(repoRoot, "ModernWpf.Controls", "AutoSuggestBox", "AutoSuggestBoxAutomationPeer.cs");
        var galleryFactory = Read(repoRoot, "ModernWpf.Gallery", "Pages", "TextSampleFactory.cs");
        var harness = Read(repoRoot, "tools", "visual-checks", "Run-GalleryVisualChecks.ps1");

        StringAssert.Contains(audit, "de3e767333c2f0717a6a70cb22bd192ced5ad885");
        StringAssert.Contains(audit, "8463f45162149de0ec3ad7df752596893fe3e13e");
        StringAssert.Contains(audit, "beabd047460bf5d43a41fcf8bddf7730188bd5a7");
        StringAssert.Contains(audit, "49b4d5326b4deba8c036e63a7e676715a5de4f3a");
        StringAssert.Contains(audit, "29f62479d5c046a0b854a5868e5a7cd484572d87");
        StringAssert.Contains(audit, "14a4a1a2b8ddc527dc4a7d5f7e743d7c2bc97db7");
        StringAssert.Contains(audit, "2b8474321ebfcadf268039d6fc4f24ea96276c7d");
        StringAssert.Contains(audit, "57119bacd035f7e466ecb19d964d6c4250ccc4a9");
        StringAssert.Contains(audit, "6e3443b581675f79c833226a41e2a531472579ec");
        StringAssert.Contains(audit, "7758954dccfead1b0f2ce7873c000c50390ca17f");
        StringAssert.Contains(audit, "177fca6665b16fe29bd745768475e6f479fda394");
        StringAssert.Contains(audit, "7cd762ebf3f870d238c428a8ca551d3cd311ee4a");
        StringAssert.Contains(audit, "39e8d87fee36da16ea8a9cb439400821ae55d703");
        StringAssert.Contains(audit, "ee0e163b3d815a149db9190d668c216c1aad64a3");
        StringAssert.Contains(audit, "16d9d90037e46fdf5a16c5685bd905be0d257b0c");
        StringAssert.Contains(audit, "e6397a4aeacac6d1f3b026d50e489b7dfb5e545d");
        StringAssert.Contains(audit, "bb1e6fda90b12819a33eda7cdcd0afb866990513");
        StringAssert.Contains(audit, "3b627d5f1710cdd1dad7515189aafffb273d8d3e");
        StringAssert.Contains(audit, "0302c81d8dacc625f2cf88549f11149eaa65e5c6");
        StringAssert.Contains(audit, "e1b6d89711b84f3bbad36581c519f856a8144cf5");
        StringAssert.Contains(audit, "35eb630681ab9dff5987d281a593f64e3ef45c3b");
        StringAssert.Contains(audit, "a3049872eba26e7ebac51528bb4fa2375e94e882");
        StringAssert.Contains(audit, "artifacts/visual-checks/20260718-122824-866-56568/report.md");
        StringAssert.Contains(audit, "artifacts/visual-checks/20260718-122854-442-15432/report.md");
        StringAssert.Contains(audit, "artifacts/visual-checks/20260718-122944-036-93464/report.json");
        StringAssert.Contains(audit, "| `300x32` / `300x32` | `0.01` |");

        StringAssert.Contains(control, "m_popupRepositionHelper = new PopupRepositionHelper(m_suggestionsPopup, this);");
        StringAssert.Contains(control, "m_userTypedText = m_textBox.Text;");
        StringAssert.Contains(control, "new Action(() => SubmitQuery(e.ClickedItem))");
        StringAssert.Contains(control, "internal void ProgrammaticSubmitQuery()");
        StringAssert.Contains(control, "private void SubmitQuery(object chosenSuggestion)");
        StringAssert.Contains(control, "private void UpdateCornerRadius(bool isPopupOpen)");

        StringAssert.Contains(eventArgs, "source.TextChangedEventCounter == m_counter");
        StringAssert.Contains(list, "OnItemClick(item);");
        StringAssert.Contains(list, "switch (SelectionMode)");
        StringAssert.Contains(list, "case SelectionMode.Multiple:");
        StringAssert.Contains(list, "case SelectionMode.Extended:");

        StringAssert.Contains(template, "<ui:ThemeShadowChrome");
        StringAssert.Contains(template, "WindowedPopupInsetMode=\"Medium\"");
        StringAssert.Contains(template, "x:Name=\"SuggestionsContainer\"");
        StringAssert.Contains(template, "x:Name=\"SuggestionsList\"");
        StringAssert.Contains(template, "IsItemClickEnabled=\"True\"");
        StringAssert.Contains(shadowChrome, "new PropertyMetadata(32d, OnDepthChanged)");

        StringAssert.Contains(peer, "patternInterface == PatternInterface.Invoke");
        StringAssert.Contains(peer, "return nameof(AutoSuggestBox);");
        StringAssert.Contains(peer, "return AutomationControlType.Group;");
        StringAssert.Contains(peer, "ProgrammaticSubmitQuery();");

        StringAssert.Contains(galleryFactory, "A basic autosuggest box.");
        StringAssert.Contains(galleryFactory, "An AutoSuggestBox that provides a SearchBox experience");
        StringAssert.Contains(galleryFactory, "Name = \"Control1\"");
        StringAssert.Contains(galleryFactory, "AutomationProperties.SetName(box, \"Basic AutoSuggestBox\");");
        StringAssert.Contains(galleryFactory, "Name = \"SuggestionOutput\"");
        StringAssert.Contains(galleryFactory, "Name = \"Control2\"");
        StringAssert.Contains(galleryFactory, "PlaceholderText = \"Type a control name\"");
        StringAssert.Contains(galleryFactory, "QueryIcon = new Mux.SymbolIcon(Mux.Symbol.Find)");

        StringAssert.Contains(harness, "\"AutoSuggestBox\" { return 0.1 }");
        StringAssert.Contains(harness, "\"AutoSuggestBox\" { return 0 }");
        StringAssert.Contains(harness, "\"AutoSuggestBox\" { return \"ae\" }");
        StringAssert.Contains(harness, "\"AutoSuggestBox\" { return @(\"Aegean\") }");
        StringAssert.Contains(harness, "[GalleryVisualNative]::TypeText($text)");
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
