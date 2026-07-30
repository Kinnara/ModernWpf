using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModernWpf.Gallery.Tests
{
    [TestClass]
    public class CommandBarSourceAuditTests
    {
        [TestMethod]
        public void CurrentCommandBarSourcesBehaviorAccessibilityAndPixelGatesArePinnedByAudit()
        {
            var root = FindRepoRoot();
            var audit = Read(root, "docs", "commandbar-winui3-source-audit.md");
            var properties = Read(root, "ModernWpf.Controls", "CommandBar", "CommandBar.properties.g.cs");
            var template = Read(root, "ModernWpf.Controls", "CommandBar", "CommandBar.xaml");
            var control = Read(root, "ModernWpf.Controls", "CommandBar", "CommandBar.cs");
            var dynamicOverflow = Read(root, "ModernWpf.Controls", "CommandBar", "CommandBarDynamicOverflow.cs");
            var peer = Read(root, "ModernWpf.Controls", "CommandBar", "CommandBarAutomationPeer.cs");
            var style = Read(root, "ModernWpf", "Styles", "CommandBar.xaml");
            var rootResources = Read(root, "ModernWpf", "ModernWpfControlsResources.xaml");
            var publicResourceKeys = Read(root, "ModernWpf", "PublicResourceKeys.Shipped.txt");
            var appBarButton = Read(root, "ModernWpf.Controls", "CommandBar", "AppBarButton.xaml");
            var publicDocumentation = Read(root, "ModernWpf.Controls", "ModernWpf.Controls.xml");
            var sample = Read(root, "ModernWpf.Gallery", "Pages", "MenusToolbarsSampleFactory.cs");
            var sampleTests = Read(root, "test", "ModernWpf.Gallery.Tests", "GalleryAutomationHookTests.cs");
            var productTests = Read(root, "test", "ModernWpf.WinUI.Tests", "CommandBar", "CommandBarApiTests.cs");
            var harness = Read(root, "tools", "visual-checks", "Run-GalleryVisualChecks.ps1");
            var recorder = Read(root, "tools", "visual-checks", "Record-GalleryControlInteractions.ps1");

            StringAssert.Contains(audit, "de3e767333c2f0717a6a70cb22bd192ced5ad885");
            StringAssert.Contains(audit, "eb75504a1978df0d37a3ad4574d6f72bf4d21583");
            StringAssert.Contains(audit, "a97562621a1d1ea397a38a3f512c9eef99db52d8");
            StringAssert.Contains(audit, "f4dc3eb367f4bcecac1793829d9a221e924e5bfb");
            StringAssert.Contains(audit, "8dca4cd76468ac49cd2aa31cafa2e320835cb17b");
            StringAssert.Contains(audit, "f524c6d543ea735b7b4e833294891eec448b8b5f");
            StringAssert.Contains(audit, "ecf554e134db0793668a5993f87f8c80e487ef04");
            StringAssert.Contains(audit, "3089af2b982481552e3f713ddfccd1edab1b5bc2");
            StringAssert.Contains(audit, "12f3fdcfffa7e0cb7fb32698c674b2ab86bb5b8e");
            StringAssert.Contains(audit, "efa158fccd2cc4094a390d1e15b6aa4e92cbb4e7");
            StringAssert.Contains(audit, "5da716a0536e14b9dc582cf63cac27ef161e1622");
            StringAssert.Contains(audit, "29f62479d5c046a0b854a5868e5a7cd484572d87");
            StringAssert.Contains(audit, "e2c92a5672467c5184198379f8b4b438bfeba8f3");
            StringAssert.Contains(audit, "452cf2578ca1b15c106fd57632dfd11c07f80af0");
            StringAssert.Contains(audit, "7a55d8dd9ac97cd10ba0d8bbf11fe5c1c70c2670");

            StringAssert.Contains(properties, "public bool IsSticky");
            StringAssert.Contains(properties, "nameof(IsSticky)");
            StringAssert.Contains(template, "PlacementTarget=\"{Binding ElementName=ContentRoot}\"");
            StringAssert.Contains(template, "StaysOpen=\"true\"");
            StringAssert.Contains(template, "MinWidth=\"{DynamicResource CommandBarOverflowMinWidth}\"");
            StringAssert.Contains(template, "Data=\"{DynamicResource CommandBarMoreButtonIconData}\"");
            StringAssert.Contains(rootResources, "x:Key=\"CommandBarMoreButtonIconData\"");
            StringAssert.Contains(
                publicResourceKeys,
                "ModernWpfControlsResources.xaml|CommandBarMoreButtonIconData");
            StringAssert.Contains(control, "InputManager.Current.PreProcessInput += OnPreProcessInput");
            StringAssert.Contains(control, "TryLightDismissForTesting(inputSource)");
            StringAssert.Contains(control, "if (!IsSticky)");
            StringAssert.Contains(control, "RefreshOverflowPopupPosition()");
            StringAssert.Contains(control, "CustomPlacementMode.BottomEdgeAlignedRight");
            StringAssert.Contains(control, "IsCompactHeightDifferenceSignificant(");
            StringAssert.Contains(control, "0.5 / rasterizationScale");
            StringAssert.Contains(control, "public event EventHandler<object> Opening");
            StringAssert.Contains(control, "public event EventHandler<object> Closing");
            StringAssert.Contains(control, "public event TypedEventHandler<CommandBar, DynamicOverflowItemsChangingEventArgs> DynamicOverflowItemsChanging");
            StringAssert.Contains(control, "RaiseDynamicOverflowItemsChangingIfNeeded(movedPrimaryCommands)");
            StringAssert.Contains(control, "OnClosed(null)");
            StringAssert.Contains(dynamicOverflow, "public enum CommandBarDynamicOverflowAction");
            StringAssert.Contains(dynamicOverflow, "AddingToOverflow = 0");
            StringAssert.Contains(dynamicOverflow, "RemovingFromOverflow = 1");
            StringAssert.Contains(dynamicOverflow, "public sealed class DynamicOverflowItemsChangingEventArgs");
            StringAssert.Contains(dynamicOverflow, "public CommandBarDynamicOverflowAction Action { get; internal set; }");
            StringAssert.Contains(peer, "internal class CommandBarAutomationPeer");
            StringAssert.Contains(peer, "return \"ApplicationBar\"");
            StringAssert.Contains(peer, "return \"app bar\"");
            StringAssert.Contains(peer, "PatternInterface.Toggle");
            StringAssert.Contains(peer, "PatternInterface.ExpandCollapse");
            StringAssert.Contains(peer, "PatternInterface.Window && GetImpl().IsOpen");
            StringAssert.Contains(peer, "AutomationControlType.Custom");
            StringAssert.Contains(peer, "public bool IsModal => true");
            StringAssert.Contains(peer, "public bool IsTopmost => true");
            StringAssert.Contains(productTests, "CommandBarDynamicOverflowChangingEventUsesCurrentWinUIActionAndTiming");
            StringAssert.Contains(productTests, "CommandBarOpenLifecycleUsesCurrentWinUIEventAndVirtualHookOrder");
            StringAssert.Contains(productTests, "CommandBarAutomationPeerUsesCurrentWinUIAppBarPatterns");
            StringAssert.Contains(productTests, "CommandBarMoreButtonIconDataCanBeOverriddenPerInstance");
            StringAssert.Contains(productTests, "CommandBarAutoOverflowButtonUsesPhysicalPixelCompactHeightThreshold");
            StringAssert.Contains(productTests, "CommandBarCompactHeightThresholdUsesFractionalRasterizationScale");
            StringAssert.Contains(audit, "Issue #262");
            StringAssert.Contains(audit, "`CommandBarMoreButtonIconData`");
            StringAssert.Contains(publicDocumentation, "E:ModernWpf.Controls.CommandBar.DynamicOverflowItemsChanging");
            Assert.IsFalse(publicDocumentation.Contains("T:ModernWpf.Automation.Peers.CommandBarAutomationPeer"));
            StringAssert.Contains(style, "AppBarButtonTextLabelOnRightMargin\">8,16,13,10");
            StringAssert.Contains(style, "AppBarToggleButtonTextLabelOnRightMargin\">8,16,13,10");
            StringAssert.Contains(appBarButton, "Margin=\"24,0,13,0\"");

            StringAssert.Contains(sample, "CreateCommandBarExamples(sampleSnippets)");
            StringAssert.Contains(sample, "FindSnippetText(sampleSnippets, \"CommandBarLabelsSide.txt\")");
            StringAssert.Contains(sample, "commandBar.IsOpen = true");
            StringAssert.Contains(sample, "commandBar.IsSticky = true");
            StringAssert.Contains(sample, "Ctrl+Subtract");
            StringAssert.Contains(sample, "updatesOutput: false");
            StringAssert.Contains(sampleTests, "new AppBarButtonAutomationPeer(addButton)");
            StringAssert.Contains(sampleTests, "new AppBarButtonAutomationPeer(settingsButton)");
            StringAssert.Contains(sampleTests, "PatternInterface.Invoke");
            StringAssert.Contains(sampleTests, "new TextBlockAutomationPeer(selectedOptionText)");

            StringAssert.Contains(harness, "\"CommandBar\" { return \"PrimaryCommandBar\" }");
            StringAssert.Contains(harness, "function Save-CommandBarOpenSurfaceCrop");
            StringAssert.Contains(harness, "Save-ScreenElementCrop $openElement $path \"CommandBarOpenSurface\" 0 $window");
            StringAssert.Contains(harness, "GallerySample_CommandBar_SettingsButton");
            StringAssert.Contains(harness, "settingsButton");
            StringAssert.Contains(harness, "if ($control -eq \"CommandBar\")");
            StringAssert.Contains(harness, "Invoke-ElementPatternOnce $window $element");
            StringAssert.Contains(harness, "\"CommandBar\" { return 2.5 }");
            StringAssert.Contains(harness, "\"CommandBar\" { return 2.5 }");
            StringAssert.Contains(recorder, "\"CommandBar\" { return \"GallerySample_CommandBar_AddButton\" }");
            StringAssert.Contains(audit, "artifacts/visual-checks/20260719-015618-975-54260/report.md");
            StringAssert.Contains(audit, "artifacts/visual-checks/20260719-015715-320-93036/report.md");
            StringAssert.Contains(audit, "artifacts/gallery-recordings/20260719-015752-861/report.md");
            StringAssert.Contains(audit, "artifacts/gallery-recordings/20260719-015932-813/report.md");
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
