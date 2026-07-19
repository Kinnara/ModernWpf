using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModernWpf.WinUI.Tests.NavigationView;

[TestClass]
public class NavigationViewSourceAuditTests
{
    [TestMethod]
    public void CurrentWinUI3ResourceAndTemplateParityIsPinnedByAudit()
    {
        var repoRoot = FindRepoRoot();
        var audit = File.ReadAllText(Path.Combine(repoRoot, "docs", "navigationview-winui3-source-audit.md"));
        var light = File.ReadAllText(Path.Combine(repoRoot, "ModernWpf", "ThemeResources", "Light.xaml"));
        var dark = File.ReadAllText(Path.Combine(repoRoot, "ModernWpf", "ThemeResources", "Dark.xaml"));
        var highContrast = File.ReadAllText(Path.Combine(repoRoot, "ModernWpf", "ThemeResources", "HighContrast.xaml"));
        var template = File.ReadAllText(Path.Combine(repoRoot, "ModernWpf.Controls", "NavigationView", "NavigationView.xaml"));
        var navigationView = File.ReadAllText(Path.Combine(repoRoot, "ModernWpf.Controls", "NavigationView", "NavigationView.cs"));
        var navigationViewItem = File.ReadAllText(Path.Combine(repoRoot, "ModernWpf.Controls", "NavigationView", "NavigationViewItem.cs"));

        StringAssert.Contains(audit, "de3e767333c2f0717a6a70cb22bd192ced5ad885");
        StringAssert.Contains(audit, "29f62479d5c046a0b854a5868e5a7cd484572d87");
        StringAssert.Contains(audit, "b6e31de9b2bdf825b894cc831581439ecfaf4579");
        StringAssert.Contains(audit, "834625ee535b767ca8ab3e381468e52ebed6aeb5");
        StringAssert.Contains(audit, "32fb2f2807190034bf5b6d914b6e00eb98945859");
        StringAssert.Contains(audit, "7f2bc04facd53a283debc100ffb1f0cf903c7971");
        StringAssert.Contains(audit, "controls/dev/NavigationView/NavigationView_themeresources.xaml");
        StringAssert.Contains(audit, "AcrylicInAppFillColorDefaultBrush");
        StringAssert.Contains(audit, "SystemControlPageBackgroundChromeLowBrush");
        StringAssert.Contains(audit, "SystemControlBackgroundChromeMediumBrush");

        StringAssert.Contains(light, "x:Key=\"NavigationViewContentBackground\" ResourceKey=\"LayerFillColorDefaultBrush\"");
        StringAssert.Contains(light, "x:Key=\"NavigationViewItemHeaderForeground\" ResourceKey=\"TextFillColorSecondaryBrush\"");
        StringAssert.Contains(light, "x:Key=\"NavigationViewContentGridBorderBrush\" ResourceKey=\"CardStrokeColorDefaultBrush\"");
        StringAssert.Contains(dark, "x:Key=\"NavigationViewContentBackground\" ResourceKey=\"LayerFillColorDefaultBrush\"");
        StringAssert.Contains(highContrast, "x:Key=\"NavigationViewContentBackground\" ResourceKey=\"SystemColorWindowColorBrush\"");

        StringAssert.Contains(template, "Background=\"{DynamicResource NavigationViewItemIconBackground}\"");
        StringAssert.Contains(template, "BorderBrush=\"{DynamicResource NavigationViewContentGridBorderBrush}\"");
        StringAssert.Contains(template, "Value=\"{DynamicResource NavigationViewItemHeaderForeground}\"");

        StringAssert.Contains(navigationView, "menuItemsActualHeight <= totalAvailableHeightHalf");
        StringAssert.Contains(navigationView, "totalAvailableHeight >= footerItemsRepeater.ActualHeight");
        StringAssert.Contains(navigationViewItem, "IsExpanded &&");
        StringAssert.Contains(navigationViewItem, "ShouldRepeaterShowInFlyout() &&");
        StringAssert.Contains(navigationViewItem, "FlyoutBase.GetAttachedFlyout(m_rootGrid) != null");
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
