using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModernWpf.WinUI.Tests.RadioButtons;

[TestClass]
public class RadioButtonsSourceAuditTests
{
    [TestMethod]
    public void CurrentWinUI3RadioButtonsParityIsPinnedByAudit()
    {
        var repoRoot = FindRepoRoot();
        var audit = Read(repoRoot, "docs", "radiobuttons-winui3-source-audit.md");
        var control = Read(repoRoot, "ModernWpf.Controls", "RadioButtons", "RadioButtons.cs");
        var template = Read(repoRoot, "ModernWpf.Controls", "RadioButtons", "RadioButtons.xaml");
        var peer = Read(repoRoot, "ModernWpf.Controls", "RadioButtons", "RadioButtonsAutomationPeer.cs");
        var factory = Read(repoRoot, "ModernWpf.Controls", "RadioButtons", "RadioButtonsElementFactory.cs");

        StringAssert.Contains(audit, "de3e767333c2f0717a6a70cb22bd192ced5ad885");
        StringAssert.Contains(audit, "controls/dev/RadioButtons/RadioButtons.cpp");
        StringAssert.Contains(audit, "8855743667ca40c93bf655f3779f36cb576088ac");
        StringAssert.Contains(audit, "29f62479d5c046a0b854a5868e5a7cd484572d87");
        StringAssert.Contains(audit, "no RadioButtons sample or page");
        StringAssert.Contains(audit, "8d585ed47a80dcb34a2e823e120a5114c9dfb040");
        StringAssert.Contains(audit, "0796f498aaa3a51f2b16bd00094336922e992438");

        StringAssert.Contains(control, "var removedItems = previousSelectedItem != null");
        StringAssert.Contains(control, "var addedItems = newSelectedItem != null");
        StringAssert.Contains(control, "Array.Empty<object>()");
        StringAssert.Contains(control, "new SelectionChangedEventArgs(SelectionChangedEvent, removedItems, addedItems)");
        Assert.IsFalse(control.Contains("new[] { previousSelectedItem }, new[] { newSelectedItem }", StringComparison.Ordinal));
        StringAssert.Contains(control, "AutomationProperties.PositionInSetProperty, args.Index + 1");
        StringAssert.Contains(control, "AutomationProperties.SizeOfSetProperty, itemSourceView.Count");

        StringAssert.Contains(template, "x:Name=\"HeaderContentPresenter\"");
        StringAssert.Contains(template, "x:Name=\"InnerRepeater\"");
        StringAssert.Contains(template, "ColumnMajorUniformToLargestGridLayout");
        StringAssert.Contains(template, "MaxColumns=\"{Binding Value, Source={StaticResource RadioButtonsMaxColumnsProxy}}\"");
        StringAssert.Contains(template, "<ui:VisualStateEx.Setters>");

        StringAssert.Contains(peer, "return nameof(RadioButtons);");
        StringAssert.Contains(peer, "AutomationControlType.Group");
        StringAssert.Contains(peer, "radioButtons.Header");
        StringAssert.Contains(factory, "newValue is DataTemplateSelector selector");
        StringAssert.Contains(factory, "ContentTemplateSelector = itemTemplateWrapper.TemplateSelector");
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
