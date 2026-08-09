using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModernWpf.WinUI.Tests.TabView;

[TestClass]
public class TabViewSourceAuditTests
{
    [TestMethod]
    public void CurrentWinUIAndGalleryCutoffsArePinnedWithWpfAdaptations()
    {
        var root = FindRepositoryRoot();
        var audit = File.ReadAllText(Path.Combine(root, "docs", "tabview-winui3-source-audit.md"));

        StringAssert.Contains(audit, "e1aa8f64df98d6229f6cd4074d59b654616254da");
        StringAssert.Contains(audit, "a97562621a1d1ea397a38a3f512c9eef99db52d8");
        StringAssert.Contains(audit, "3669519356c67f1376152c33ed8ea45003a91f3a");
        StringAssert.Contains(audit, "System.Windows.Window");
        StringAssert.Contains(audit, "does not rename, subclass, or replace WPF's stock `TabControl`");
        StringAssert.Contains(audit, "Unlike WinUI's native move-size integration");
        StringAssert.Contains(audit, "complete WPF Window-based tear-out/rejoin event sequence");
    }

    [TestMethod]
    public void TabViewIsASeparateControlAndItsTemplateIsMerged()
    {
        var root = FindRepositoryRoot();
        var implementation = File.ReadAllText(Path.Combine(root, "ModernWpf.Controls", "TabView", "TabView.cs"));
        var itemImplementation = File.ReadAllText(Path.Combine(root, "ModernWpf.Controls", "TabView", "TabViewItem.cs"));
        var generic = File.ReadAllText(Path.Combine(root, "ModernWpf.Controls", "Themes", "Generic.xaml"));
        var stockStyle = File.ReadAllText(Path.Combine(root, "ModernWpf", "Styles", "TabControl.xaml"));

        StringAssert.Contains(implementation, "public partial class TabView : Control");
        StringAssert.Contains(itemImplementation, "public partial class TabViewItem : ListBoxItem");
        StringAssert.Contains(generic, "/ModernWpf.Controls;component/TabView/TabView.xaml");
        StringAssert.Contains(stockStyle, "TargetType=\"{x:Type TabControl}\"");
        Assert.IsFalse(stockStyle.Contains("ModernWpf.Controls.TabView"));
    }

    [TestMethod]
    public void GalleryVisualGateRoutesTheRealTabViewAndAllTenExamples()
    {
        var root = FindRepositoryRoot();
        var source = File.ReadAllText(Path.Combine(root, "tools", "visual-checks", "Run-GalleryVisualChecks.ps1"));

        StringAssert.Contains(source, "SelectorBar\", \"TabView\", \"NavigationView");
        StringAssert.Contains(source, "TabView = 10");
        Assert.AreEqual(
            2,
            CountOccurrences(source, "\"TabView\" { return \"GallerySample_TabView_TabView\" }"),
            "Both the required-sample and ModernWPF crop mappings must target the real control.");
        StringAssert.Contains(source, "\"TabView\" { return \"TabView1\" }");
        StringAssert.Contains(source, "if ($control -ne \"TabView\")");
        StringAssert.Contains(source, "$scrollPattern.SetScrollPercent([System.Windows.Automation.ScrollPattern]::NoScroll, 0)");
    }

    [TestMethod]
    public void AutomationPreservesWpfItemProvidersAndRaisesCollectionNotifications()
    {
        var root = FindRepositoryRoot();
        var peerSource = File.ReadAllText(Path.Combine(root, "ModernWpf.Controls", "TabView", "TabViewAutomationPeers.cs"));
        var controlSource = File.ReadAllText(Path.Combine(root, "ModernWpf.Controls", "TabView", "TabView.cs"));

        StringAssert.Contains(peerSource, "TabViewItemAutomationPeer : FrameworkElementAutomationPeer, ISelectionItemProvider, IScrollItemProvider");
        StringAssert.Contains(peerSource, "OwnerItem.BringIntoView()");
        StringAssert.Contains(peerSource, "AutomationEvents.SelectionItemPatternOnElementSelected");
        StringAssert.Contains(controlSource, "peer?.RaiseAutomationEvent(AutomationEvents.StructureChanged)");
        StringAssert.Contains(controlSource, "AutomationNotificationKind.ItemAdded");
        StringAssert.Contains(controlSource, "ResourceAccessor.GetLocalizedStringResource(SR_TabViewNewTabAddedNotification)");
    }

    [TestMethod]
    public void TemplateConsumesPinnedButtonItemAndSeparatorStateResources()
    {
        var root = FindRepositoryRoot();
        var template = File.ReadAllText(Path.Combine(root, "ModernWpf.Controls", "TabView", "TabView.xaml"));
        var itemSource = File.ReadAllText(Path.Combine(root, "ModernWpf.Controls", "TabView", "TabViewItem.cs"));
        var controlSource = File.ReadAllText(Path.Combine(root, "ModernWpf.Controls", "TabView", "TabView.cs"));

        StringAssert.Contains(template, "x:Key=\"DefaultTabViewScrollButtonStyle\"");
        Assert.AreEqual(2, CountOccurrences(template, "Style=\"{StaticResource DefaultTabViewScrollButtonStyle}\""));
        StringAssert.Contains(template, "TabViewScrollButtonBackgroundPointerOver");
        StringAssert.Contains(template, "TabViewScrollButtonForegroundPressed");
        StringAssert.Contains(template, "TabViewScrollButtonBorderBrushDisabled");
        StringAssert.Contains(template, "x:Key=\"DefaultTabViewCloseButtonStyle\"");
        StringAssert.Contains(template, "TabViewItemHeaderCloseButtonBackgroundPointerOver");
        StringAssert.Contains(template, "TabViewItemHeaderPressedCloseButtonForeground");
        StringAssert.Contains(template, "TabViewItemHeaderSelectedCloseButtonBackground");
        StringAssert.Contains(template, "x:Name=\"TabSeparator\"");
        StringAssert.Contains(template, "TabViewItemSeparatorMargin");
        StringAssert.Contains(template, "TabViewItemHeaderBackgroundPressed");
        StringAssert.Contains(template, "TabViewItemIconForegroundPointerOver");
        StringAssert.Contains(template, "TabViewItemIconForegroundSelected");
        StringAssert.Contains(template, "TabViewItemIconForegroundDisabled");
        StringAssert.Contains(itemSource, "CaptureMouse()");
        StringAssert.Contains(itemSource, "Owner?.UpdateTabSeparators()");
        StringAssert.Contains(itemSource, "Loaded += OnItemLoaded;");
        StringAssert.Contains(itemSource, "Unloaded += OnItemUnloaded;");
        StringAssert.Contains(itemSource, "private void AttachCornerRadiusListener()");
        StringAssert.Contains(itemSource, "IsLoaded && _geometryCornerRadiusProbe != null");
        StringAssert.Contains(itemSource, "CornerRadiusDescriptor?.AddValueChanged(");
        StringAssert.Contains(itemSource, "private void DetachCornerRadiusListener()");
        StringAssert.Contains(itemSource, "CornerRadiusDescriptor?.RemoveValueChanged(");
        StringAssert.Contains(controlSource, "CollectionChangedEventManager.AddHandler(");
        StringAssert.Contains(controlSource, "CollectionChangedEventManager.RemoveHandler(");
        Assert.IsFalse(controlSource.Contains("_itemsSourceNotifier.CollectionChanged +=", StringComparison.Ordinal));
        StringAssert.Contains(controlSource, "index + 1 == SelectedIndex");
        StringAssert.Contains(controlSource, "nextTab?.IsMouseOver == true");
    }

    private static int CountOccurrences(string source, string value)
    {
        var count = 0;
        var index = 0;
        while ((index = source.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += value.Length;
        }

        return count;
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "ModernWpf.sln")))
        {
            directory = directory.Parent;
        }

        Assert.IsNotNull(directory, "Could not locate the repository root.");
        return directory.FullName;
    }
}
