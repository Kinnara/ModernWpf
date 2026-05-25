using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static ModernWpf.Gallery.Tests.WpfGallerySnippetTestHelpers;

namespace ModernWpf.Gallery.Tests
{
    [TestClass]
    public class WpfGallerySourceShapeTests
    {
        [TestMethod]
        public void CopiedWpfGalleryCodeBehindClassesStayUnsealedLikeOfficialSource()
        {
            var repoRoot = GetRepoRoot();
            var wpfGalleryPageCodeBehind = Directory.EnumerateFiles(
                Path.Combine(repoRoot, "ModernWpf.Gallery", "Pages", "WpfGallery"),
                "*.xaml.cs",
                SearchOption.AllDirectories);
            var copiedTopLevelCodeBehind = new[]
            {
                Path.Combine(repoRoot, "ModernWpf.Gallery", "Pages", "HomePage.xaml.cs"),
                Path.Combine(repoRoot, "ModernWpf.Gallery", "Pages", "AllControlsPage.xaml.cs"),
                Path.Combine(repoRoot, "ModernWpf.Gallery", "Pages", "WhatsNewPage.xaml.cs"),
                Path.Combine(repoRoot, "ModernWpf.Gallery", "Pages", "SettingsPage.xaml.cs"),
                Path.Combine(repoRoot, "ModernWpf.Gallery", "Controls", "HeaderTile.xaml.cs"),
                Path.Combine(repoRoot, "ModernWpf.Gallery", "Controls", "TileGallery.xaml.cs")
            };

            foreach (var path in wpfGalleryPageCodeBehind.Concat(copiedTopLevelCodeBehind))
            {
                var source = File.ReadAllText(path);
                Assert.IsFalse(
                    source.Contains("public sealed partial class", StringComparison.Ordinal),
                    Path.GetRelativePath(repoRoot, path) + " should match the official WPF Gallery unsealed partial class shape.");
            }

            var sectionSource = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "WpfGallerySectionPages.cs");
            foreach (var className in new[]
            {
                "DesignGuidancePage",
                "SamplesPage",
                "BasicInputPage",
                "CollectionsPage",
                "DateAndTimePage",
                "LayoutPage",
                "MediaPage",
                "NavigationPage",
                "StatusAndInfoPage",
                "TextPage",
                "SystemPage"
            })
            {
                Assert.IsFalse(
                    sectionSource.Contains("public sealed class " + className + " : SectionPage", StringComparison.Ordinal),
                    className + " should remain unsealed like the official WPF Gallery section page type.");
            }
        }

        [TestMethod]
        public void CopiedItemCodeBehindKeepsOfficialViewModelPropertyBeforeConstructorShape()
        {
            foreach (var page in new[]
            {
                Tuple.Create("BasicInput", "ButtonPage", "ButtonPageViewModel"),
                Tuple.Create("BasicInput", "CheckBoxPage", "CheckBoxPageViewModel"),
                Tuple.Create("BasicInput", "ComboBoxPage", "ComboBoxPageViewModel"),
                Tuple.Create("BasicInput", "RadioButtonPage", "RadioButtonPageViewModel"),
                Tuple.Create("BasicInput", "SliderPage", "SliderPageViewModel"),
                Tuple.Create("Collections", "DataGridPage", "DataGridPageViewModel"),
                Tuple.Create("Collections", "ListBoxPage", "ListBoxPageViewModel"),
                Tuple.Create("Collections", "ListViewPage", "ListViewPageViewModel"),
                Tuple.Create("Collections", "TreeViewPage", "TreeViewPageViewModel"),
                Tuple.Create("DateAndTime", "CalendarPage", "CalendarPageViewModel"),
                Tuple.Create("DateAndTime", "DatePickerPage", "DatePickerPageViewModel"),
                Tuple.Create("DesignGuidance", "ColorPage", "ColorsPageViewModel"),
                Tuple.Create("DesignGuidance", "GeometryPage", "GeometryPageViewModel"),
                Tuple.Create("DesignGuidance", "SpacingPage", "SpacingPageViewModel"),
                Tuple.Create("Media", "CanvasPage", "CanvasPageViewModel"),
                Tuple.Create("Media", "ImagePage", "ImagePageViewModel"),
                Tuple.Create("Navigation", "MenuPage", "MenuPageViewModel"),
                Tuple.Create("Navigation", "TabControlPage", "TabControlPageViewModel"),
                Tuple.Create("Samples", "UserDashboardPage", "UserDashboardPageViewModel"),
                Tuple.Create("StatusAndInfo", "ProgressBarPage", "ProgressBarPageViewModel"),
                Tuple.Create("StatusAndInfo", "ToolTipPage", "ToolTipPageViewModel"),
                Tuple.Create("System", "ClipboardPage", "ClipboardPageViewModel"),
                Tuple.Create("System", "FileAndFolderDialogsPage", "FileAndFolderDialogsPageViewModel"),
                Tuple.Create("System", "MessageBoxPage", "MessageBoxPageViewModel"),
                Tuple.Create("Text", "LabelPage", "LabelPageViewModel"),
                Tuple.Create("Text", "PasswordBoxPage", "PasswordBoxPageViewModel"),
                Tuple.Create("Text", "RichTextEditPage", "RichTextEditPageViewModel"),
                Tuple.Create("Text", "TextBlockPage", "TextBlockPageViewModel"),
                Tuple.Create("Text", "TextBoxPage", "TextBoxPageViewModel")
            })
            {
                var source = ReadRepoFile(
                    "ModernWpf.Gallery",
                    "Pages",
                    "WpfGallery",
                    page.Item1,
                    page.Item2 + ".xaml.cs");
                var viewModelIndex = source.IndexOf(
                    "public " + page.Item3 + " ViewModel { get; }",
                    StringComparison.Ordinal);
                var constructorIndex = source.IndexOf(
                    "public " + page.Item2 + "(",
                    StringComparison.Ordinal);

                Assert.IsTrue(viewModelIndex >= 0, page.Item2 + " should expose its copied page-specific ViewModel property.");
                Assert.IsTrue(constructorIndex >= 0, page.Item2 + " should keep its copied constructor.");
                Assert.IsTrue(
                    viewModelIndex < constructorIndex,
                    page.Item2 + " should match the official WPF Gallery code-behind member order by declaring ViewModel before the constructor.");
            }
        }

        [TestMethod]
        public void MenuPageKeepsOfficialMenuItemSourceShape()
        {
            var xaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Navigation",
                "MenuPage.xaml");
            StringAssert.Contains(
                xaml,
                "<Style TargetType=\"MenuItem\" BasedOn=\"{StaticResource DefaultMenuItemStyle}\">");
            StringAssert.Contains(
                xaml,
                "<EventSetter Event=\"Click\" Handler=\"MenuItem_Click\"/>");
            StringAssert.Contains(
                xaml,
                "<MenuItem AutomationProperties.Name=\"Bold\" Tag=\"Bold\" >");
            StringAssert.Contains(
                xaml,
                "<MenuItem AutomationProperties.Name=\"Italic\" Tag=\"Italic\" >");
            StringAssert.Contains(
                xaml,
                "<MenuItem AutomationProperties.Name=\"Underlined\" Tag=\"Underlined\" >");

            var codeBehind = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Navigation",
                "MenuPage.xaml.cs");
            StringAssert.Contains(
                codeBehind,
                "StatusMenuItem.Text = (menuItem.Tag != null) ? $\"You pressed {menuItem.Tag}\" : $\"You pressed {menuItem.Header}\";");
        }
    }
}
