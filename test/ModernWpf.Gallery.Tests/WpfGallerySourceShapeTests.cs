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
        public void HomePageKeepsOfficialDashboardCardListDeclarationSourceShape()
        {
            var xaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "HomePage.xaml");

            AssertContainsInOrder(
                xaml,
                "<Grid x:Name=\"ContentRootGrid\">",
                "<ScrollViewer>",
                "<Grid>",
                "<Grid.RowDefinitions>",
                "<RowDefinition Height=\"Auto\" />",
                "<RowDefinition Height=\"Auto\" />",
                "<RowDefinition Height=\"*\" />");
            AssertContainsInOrder(
                xaml,
                "<controls:TileGallery",
                "Grid.Row=\"1\"",
                "HorizontalAlignment=\"Stretch\"",
                "Margin=\"0\" />");
            AssertContainsInOrder(
                xaml,
                "Text=\"Overview\" />",
                "<ItemsControl",
                "Margin=\"-20,0,0,0\"",
                "AutomationProperties.Name=\"Items in group\"",
                "ItemsSource=\"{Binding ViewModel.NavigationCards}\"",
                "Focusable=\"False\"",
                "ItemsPanel=\"{StaticResource WrapPanelTemplate}\"",
                "ItemTemplate=\"{StaticResource NavigationCardTemplate}\" />");
            AssertContainsInOrder(
                xaml,
                "Text=\"Recently added and updated\" />",
                "<ItemsControl",
                "Margin=\"-18,0,0,0\"",
                "AutomationProperties.Name=\"Recently Added and Updated Samples Section\"",
                "ItemsSource=\"{Binding ViewModel.RecentlyAddedOrUpdatedSamplesInfo}\"",
                "Focusable=\"False\"",
                "ItemsPanel=\"{StaticResource WrapPanelTemplate}\"",
                "ItemTemplate=\"{StaticResource NavigationCardTemplate}\" />");
        }

        [TestMethod]
        public void WhatsNewPageKeepsOfficialDeclarationSourceShape()
        {
            var xaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WhatsNewPage.xaml");

            StringAssert.Contains(
                xaml,
                "<Style x:Key=\"SubHeaderTextStyle\" TargetType=\"TextBlock\">");
            StringAssert.Contains(
                xaml,
                "<Style x:Key=\"LinkTextBlockStyle\" TargetType=\"TextBlock\">");
            AssertContainsInOrder(
                xaml,
                "<Grid x:Name=\"ContentPagePane\" Height=\"Auto\" Style=\"{StaticResource GalleryPageRootStyle}\">",
                "<Grid.RowDefinitions>",
                "<RowDefinition Height=\"Auto\" />",
                "<RowDefinition Height=\"*\" />");
            StringAssert.Contains(
                xaml,
                "<controls:PageHeader Margin=\"0,0,0,32\" Title=\"{Binding ViewModel.PageTitle}\" Description=\"{Binding ViewModel.PageDescription}\" ShowDescription=\"True\" />");
            StringAssert.Contains(
                xaml,
                "<ScrollViewer Grid.Row=\"1\" Margin=\"0,0,0,24\" Padding=\"0,0,24,0\">");
        }

        [TestMethod]
        public void SettingsPageKeepsOfficialSettingsDeclarationSourceShape()
        {
            var xaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "SettingsPage.xaml");

            AssertContainsInOrder(
                xaml,
                "<Style x:Key=\"SettingsCardStyle\" TargetType=\"Border\">",
                "<Setter Property=\"Padding\" Value=\"0,16,0,16\" />",
                "<Setter Property=\"BorderThickness\" Value=\"0,0,0,1\" />",
                "<Setter Property=\"BorderBrush\" Value=\"{DynamicResource ExpanderHeaderBorderBrush}\" />");
            AssertContainsInOrder(
                xaml,
                "<Grid",
                "x:Name=\"ContentRootGrid\"",
                "Style=\"{StaticResource GalleryPageRootStyle}\">");
            AssertContainsInOrder(
                xaml,
                "<controls:PageHeader",
                "Grid.Row=\"0\"",
                "Margin=\"0,0,0,40\"",
                "Title=\"{Binding ViewModel.PageTitle}\"",
                "Description=\"{Binding ViewModel.PageDescription}\"/>");
            StringAssert.Contains(
                xaml,
                "<ScrollViewer Grid.Row=\"1\" Margin=\"0,0,0,24\" Padding=\"0,0,24,0\">");
            StringAssert.Contains(
                xaml,
                "<TextBlock Text=\"Appearance &amp; behavior\" FontWeight=\"SemiBold\" Margin=\"10\" FontSize=\"14\"/>");
            StringAssert.Contains(
                xaml,
                "<Grid Background=\"Transparent\" Margin=\"0,0,0,20\">");
            StringAssert.Contains(
                xaml,
                "<Border Background=\"{DynamicResource ExpanderHeaderBackground}\" BorderBrush=\"{DynamicResource ExpanderHeaderBorderBrush}\" BorderThickness=\"{StaticResource ExpanderBorderThemeThickness}\" Padding=\"{StaticResource ExpanderPadding}\" CornerRadius=\"{DynamicResource ControlCornerRadius}\">");
            StringAssert.Contains(
                xaml,
                "<TextBlock x:Name=\"AppIcon\" AutomationProperties.Name=\"App Icon\" Grid.Column=\"0\" Width=\"20\" Height=\"20\"  Margin=\"10,5,10,5\" VerticalAlignment=\"Center\" FontFamily=\"{StaticResource SymbolThemeFontFamily}\" Text=\"&#xE790;\" FontSize=\"20\"/>");
            StringAssert.Contains(
                xaml,
                "<TextBlock Text=\"App theme\" FontSize=\"14\"/>");
            StringAssert.Contains(
                xaml,
                "<TextBlock Opacity=\"0.7\" FontSize=\"12\" Style=\"{StaticResource CaptionTextBlockStyle}\">Select which app theme to display</TextBlock>");
            StringAssert.Contains(
                xaml,
                "<ComboBox x:Name=\"Change_ThemeMode\" MinWidth=\"200\" HorizontalAlignment=\"Right\" SelectedIndex=\"2\" Grid.Column=\"2\" AutomationProperties.Name=\"Change ThemeMode\" SelectionChanged=\"ThemeMode_SelectionChanged\" Margin=\"10\">");
            StringAssert.Contains(
                xaml,
                "<TextBlock Text=\"About\" FontWeight=\"SemiBold\" Margin=\"10\" FontSize=\"14\"/>");
            StringAssert.Contains(
                xaml,
                "<TextBlock Opacity=\"0.7\" Style=\"{StaticResource CaptionTextBlockStyle}\">&#xA9; 2025 Microsoft. All rights reserved.</TextBlock>");
            StringAssert.Contains(
                xaml,
                "<TextBox Grid.Column=\"2\" Style=\"{StaticResource SelectionTextBox}\" Text=\"git clone https://github.com/microsoft/WPF-Samples.git\" Focusable=\"False\"/>");
            StringAssert.Contains(
                xaml,
                "<Button AutomationProperties.Name=\"Open Issues\" Grid.Column=\"2\" Padding=\"8\" FocusManager.IsFocusScope=\"True\" Click=\"Open_Issues\">");
            StringAssert.Contains(
                xaml,
                "<TextBlock FontFamily=\"{StaticResource SymbolThemeFontFamily}\" Text=\"&#xe8a7;\" />");
            StringAssert.Contains(
                xaml,
                "<GroupBox Grid.Row=\"2\" AutomationProperties.Name=\"Dependencies and References\" BorderThickness=\"0\">");
            StringAssert.Contains(
                xaml,
                "<Hyperlink Click=\"Open_DIInformation\" AutomationProperties.Name=\"Link to Dependency Injection NuGet Package\">Microsoft.Extensions.DependencyInjection</Hyperlink>");
            StringAssert.Contains(
                xaml,
                "<Hyperlink Click=\"Open_HostingInformation\" AutomationProperties.Name=\"Link to .NET Generic Host Package\">Microsoft.Extensions.Hosting</Hyperlink>");
            StringAssert.Contains(
                xaml,
                "<GroupBox Grid.Row=\"3\" AutomationProperties.Name=\"THIS CODE AND INFORMATION IS PROVIDED &#x2018;AS IS&#x2019; WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.\" BorderThickness=\"0\">");
        }

        [TestMethod]
        public void AllControlsPageKeepsOfficialAllSamplesDeclarationSourceShape()
        {
            var xaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "AllControlsPage.xaml");

            AssertContainsInOrder(
                xaml,
                "<Grid Style=\"{StaticResource GalleryPageRootStyle}\">",
                "<Grid.RowDefinitions>",
                "<RowDefinition Height=\"Auto\" />",
                "<RowDefinition Height=\"*\" />");
            AssertContainsInOrder(
                xaml,
                "<controls:PageHeader",
                "Grid.Row=\"0\"",
                "Margin=\"0,0,0,40\"",
                "Title=\"{Binding ViewModel.PageTitle}\"",
                "Description=\"{Binding ViewModel.PageDescription}\" />");
            AssertContainsInOrder(
                xaml,
                "<ScrollViewer",
                "Grid.Row=\"1\"",
                "Margin=\"0\"",
                "VerticalScrollBarVisibility=\"Auto\">");
            AssertContainsInOrder(
                xaml,
                "<ItemsControl",
                "Grid.Row=\"1\"",
                "Margin=\"-12,0,0,0\"",
                "AutomationProperties.Name=\"Items in group\"",
                "ItemsSource=\"{Binding ViewModel.NavigationCards}\"",
                "Focusable=\"False\"",
                "ItemsPanel=\"{StaticResource WrapPanelTemplate}\"",
                "ItemTemplate=\"{StaticResource NavigationCardTemplate}\" />");
        }

        [TestMethod]
        public void SectionPageKeepsOfficialSectionDeclarationSourceShape()
        {
            var xaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "SectionPage.xaml");

            AssertContainsInOrder(
                xaml,
                "<Grid Style=\"{StaticResource GalleryPageRootStyle}\">",
                "<Grid.RowDefinitions>",
                "<RowDefinition Height=\"Auto\" />",
                "<RowDefinition Height=\"*\" />");
            AssertContainsInOrder(
                xaml,
                "<controls:PageHeader",
                "Grid.Row=\"0\"",
                "Margin=\"0,0,0,40\"",
                "Title=\"{Binding ViewModel.PageTitle}\"",
                "Description=\"{Binding ViewModel.PageDescription}\" />");
            AssertContainsInOrder(
                xaml,
                "<ItemsControl",
                "Grid.Row=\"1\"",
                "Margin=\"-12,0,0,0\"",
                "AutomationProperties.Name=\"Items in group\"",
                "ItemsSource=\"{Binding ViewModel.NavigationCards}\"",
                "Focusable=\"False\"",
                "ItemsPanel=\"{StaticResource WrapPanelTemplate}\"",
                "ItemTemplate=\"{StaticResource NavigationCardTemplate}\" />");
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
        public void BasicInputPagesKeepOfficialHeaderAndSampleSourceShape()
        {
            foreach (var page in new[]
            {
                "ButtonPage.xaml",
                "CheckBoxPage.xaml",
                "ComboBoxPage.xaml",
                "RadioButtonPage.xaml",
                "SliderPage.xaml"
            })
            {
                var xaml = ReadRepoFile(
                    "ModernWpf.Gallery",
                    "Pages",
                    "WpfGallery",
                    "BasicInput",
                    page);
                var normalizedXaml = xaml.Replace("\r\n", "\n").Replace('\r', '\n');
                StringAssert.Contains(
                    xaml,
                    "xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages.WpfGallery.BasicInput\"");
                StringAssert.Contains(
                    normalizedXaml,
                    "<Grid x:Name=\"ContentPagePane\" Height=\"Auto\">\n\n        <Grid.RowDefinitions>");
                StringAssert.Contains(
                    normalizedXaml,
                    "</Grid.RowDefinitions>\n        <controls:PageHeader");
                StringAssert.Contains(
                    xaml,
                    "<controls:PageHeader Margin=\"0,0,0,32\" Title=\"{Binding ViewModel.PageTitle}\" ShowDescription=\"False\" />");
                StringAssert.Contains(
                    xaml,
                    "<ScrollViewer Grid.Row=\"1\" Margin=\"0,0,0,24\" Padding=\"0,0,24,0\">");
            }

            var buttonXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "BasicInput",
                "ButtonPage.xaml");
            AssertContainsInOrder(
                buttonXaml,
                "<!--<controls:ControlExample",
                "HeaderText=\"Button with Icon\"",
                "XamlCode=\"&lt;Button Content=&quot;Font Icon Button&quot; Icon=&quot;Fluent24&quot; /&gt;\"",
                "IsEnabled=\"{Binding ViewModel.IsUiButtonEnabled, RelativeSource={RelativeSource Mode=FindAncestor, AncestorType=local:ButtonPage}, Mode=OneWay}\"",
                "<!--<SymbolIcon Symbol=\"Fluent24\" />-->",
                "</controls:ControlExample>-->");
            AssertContainsInOrder(
                buttonXaml,
                "HeaderText=\"WPF Accent Button\"",
                "<!--<SymbolIcon Symbol=\"Fluent24\" />-->",
                "<TextBlock Text=\"WPF Accent Button\" />",
                "HeaderText=\"WPF Button with FontIcon\"",
                "HeaderText=\"WPF Button with FontIcon\"",
                "HeaderText=\"WPF Button with ImageIcon\"");

            var checkBoxXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "BasicInput",
                "CheckBoxPage.xaml");
            StringAssert.Contains(
                checkBoxXaml,
                "<controls:ControlExample Margin=\"10\" HeaderText=\"A 2-state CheckBox.\" XamlCode=\"&lt;CheckBox Content=&quot;Two-state CheckBox&quot; /&gt;\">");

            var radioButtonXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "BasicInput",
                "RadioButtonPage.xaml");
            StringAssert.Contains(
                radioButtonXaml,
                "<StackPanel Grid.Column=\"0\" KeyboardNavigation.TabNavigation=\"Once\" KeyboardNavigation.DirectionalNavigation=\"Cycle\">");
            AssertContainsInOrder(
                radioButtonXaml,
                "AutomationProperties.Name=\"Default Radio Option 1\"",
                "Content=\"Option 1\"",
                "GroupName=\"radio_group_one\"",
                "IsChecked=\"True\"",
                "GotKeyboardFocus=\"RadioButton_GotKeyboardFocus\"",
                "IsEnabled=");
            AssertContainsInOrder(
                radioButtonXaml,
                "AutomationProperties.Name=\"Left Flow Radio Option 1\"",
                "Content=\"Option 1\"",
                "FlowDirection=\"RightToLeft\"",
                "GroupName=\"radio_group_two\"",
                "GotKeyboardFocus=\"RadioButton_GotKeyboardFocus\"",
                "IsChecked=\"True\" />");
        }

        [TestMethod]
        public void CollectionsPagesKeepOfficialHeaderAndSampleSourceShape()
        {
            foreach (var page in new[]
            {
                Tuple.Create("DataGridPage.xaml", true),
                Tuple.Create("ListBoxPage.xaml", false),
                Tuple.Create("ListViewPage.xaml", true),
                Tuple.Create("TreeViewPage.xaml", false)
            })
            {
                var xaml = ReadRepoFile(
                    "ModernWpf.Gallery",
                    "Pages",
                    "WpfGallery",
                    "Collections",
                    page.Item1);
                var normalizedXaml = xaml.Replace("\r\n", "\n").Replace('\r', '\n');
                StringAssert.Contains(
                    xaml,
                    "xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages.WpfGallery.Collections\"");
                if (page.Item2)
                {
                    StringAssert.Contains(
                        xaml,
                        "xmlns:models=\"clr-namespace:ModernWpf.Gallery.Models\"");
                }

                StringAssert.Contains(
                    normalizedXaml,
                    "<Grid x:Name=\"ContentPagePane\" Height=\"Auto\">\n\n        <Grid.RowDefinitions>");
                StringAssert.Contains(
                    normalizedXaml,
                    "</Grid.RowDefinitions>\n        <controls:PageHeader");
                StringAssert.Contains(
                    xaml,
                    "<controls:PageHeader Margin=\"0,0,0,32\" Title=\"{Binding ViewModel.PageTitle}\" ShowDescription=\"False\" />");
                StringAssert.Contains(
                    xaml,
                    "<ScrollViewer Grid.Row=\"1\" Margin=\"0,0,0,24\" Padding=\"0,0,24,0\">");
            }

            var listBoxXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Collections",
                "ListBoxPage.xaml");
            StringAssert.Contains(
                listBoxXaml,
                "<!--<controls:ControlExample.XamlCode>");
            StringAssert.Contains(
                listBoxXaml,
                "\\t&lt;ListBoxItem Content=&quot;Blue&quot;/&gt;\\n");

            var listViewXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Collections",
                "ListViewPage.xaml");
            StringAssert.Contains(
                listViewXaml,
                "<!--<controls:ControlExample.XamlCode>");
            StringAssert.Contains(
                listViewXaml,
                "&lt;ListView ItemsSource=&quot;{Binding ViewModel.MyCollection}&quot;&gt;&lt;&gt;\\n");
            AssertContainsInOrder(
                listViewXaml,
                "AutomationProperties.Name=\"ListView with GridView\"",
                "<GridViewColumn",
                "Header=\"First Name\"",
                "Width=\"150\"",
                "DisplayMemberBinding=\"{Binding FirstName}\" />",
                "<GridViewColumn",
                "Header=\"Last Name\"",
                "Width=\"150\"",
                "DisplayMemberBinding=\"{Binding LastName}\" />",
                "<GridViewColumn",
                "Header=\"Company\"",
                "Width=\"200\"",
                "DisplayMemberBinding=\"{Binding Company}\" />");
        }

        [TestMethod]
        public void LayoutPagesKeepOfficialHeaderAndSampleSourceShape()
        {
            foreach (var page in new[]
            {
                "BorderPage.xaml",
                "ExpanderPage.xaml",
                "GridPage.xaml",
                "GridSplitterPage.xaml",
                "GroupBoxPage.xaml",
                "ResizeGripPage.xaml",
                "StackPanelPage.xaml"
            })
            {
                var xaml = ReadRepoFile(
                    "ModernWpf.Gallery",
                    "Pages",
                    "WpfGallery",
                    "Layout",
                    page);
                StringAssert.Contains(
                    xaml,
                    "<controls:PageHeader Margin=\"0,0,0,32\" Title=\"{Binding ViewModel.PageTitle}\" ShowDescription=\"False\" />");
                StringAssert.Contains(
                    xaml,
                    "<ScrollViewer Grid.Row=\"1\" Margin=\"0,0,0,24\" Padding=\"0,0,24,0\">");
            }

            var expanderXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Layout",
                "ExpanderPage.xaml");
            StringAssert.Contains(
                expanderXaml,
                "<!--  TODO: ExpandDirection  -->");

            var resizeGripXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Layout",
                "ResizeGripPage.xaml");
            AssertContainsInOrder(
                resizeGripXaml,
                "<controls:ControlExample",
                "Margin=\"10\"",
                "HeaderText=\"A ResizeGrip\"",
                "XamlCode=\"&lt;Window",
                "CSharpCode=\"private void OpenResizeGripWindow_Click");
            AssertContainsInOrder(
                resizeGripXaml,
                "<Button",
                "x:Name=\"OpenResizeGripWindow\"",
                "VerticalAlignment=\"Center\"",
                "HorizontalAlignment=\"Center\"",
                "Content=\"Open window with resize grip\"",
                "Click=\"OpenResizeGripWindow_Click\" />");
        }

        [TestMethod]
        public void DesignGuidancePagesKeepOfficialHeaderAndSampleSourceShape()
        {
            var colorXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "DesignGuidance",
                "ColorPage.xaml");
            StringAssert.Contains(
                colorXaml,
                "<controls:PageHeader Margin=\"0,0,0,32\" Title=\"{Binding ViewModel.PageTitle}\" Description=\"{Binding ViewModel.PageDescription}\" />");
            StringAssert.Contains(
                colorXaml,
                "<ScrollViewer Margin=\"0,0,0,24\" Grid.Row=\"1\" Padding=\"0,0,24,0\">");
            StringAssert.Contains(
                colorXaml,
                "<ComboBox x:Name=\"PageSelector\" SelectionChanged=\"OnSelectionChanged\" Loaded=\"OnLoaded\" Width=\"200\" AutomationProperties.Name=\"Page Selector\">");
            StringAssert.Contains(
                colorXaml,
                "<Frame x:Name=\"ColorSubpageNavigationFrame\" />");

            foreach (var page in new[]
            {
                "TypographyPage.xaml",
                "SpacingPage.xaml"
            })
            {
                var xaml = ReadRepoFile(
                    "ModernWpf.Gallery",
                    "Pages",
                    "WpfGallery",
                    "DesignGuidance",
                    page);
                StringAssert.Contains(
                    xaml,
                    "<controls:PageHeader Margin=\"0,0,0,32\" Title=\"{Binding ViewModel.PageTitle}\" Description=\"{Binding ViewModel.PageDescription}\" />");
                StringAssert.Contains(
                    xaml,
                    "<ScrollViewer Margin=\"0,0,0,24\" Padding=\"0,0,24,0\" HorizontalScrollBarVisibility=\"Auto\" Grid.Row=\"1\">");
            }

            var geometryXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "DesignGuidance",
                "GeometryPage.xaml");
            StringAssert.Contains(
                geometryXaml,
                "<controls:PageHeader Margin=\"0,0,0,32\" Title=\"{Binding ViewModel.PageTitle}\" ShowDescription=\"False\"/>");
            StringAssert.Contains(
                geometryXaml,
                "<ScrollViewer Margin=\"0,0,0,24\" Padding=\"0,0,24,0\" HorizontalScrollBarVisibility=\"Auto\" Grid.Row=\"1\">");
            StringAssert.Contains(
                geometryXaml,
                "<Border Height=\"300\" Width=\"500\" HorizontalAlignment=\"Left\">");

            var iconographyXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "DesignGuidance",
                "IconographyPage.xaml");
            StringAssert.Contains(
                iconographyXaml,
                "<controls:PageHeader Margin=\"2,0,0,32\" Title=\"{Binding ViewModel.PageTitle}\" Description=\"{Binding ViewModel.PageDescription}\" />");
            AssertContainsInOrder(
                iconographyXaml,
                "<Expander Grid.Row=\"1\"",
                "Header=\"Instructions on how to use Segoe Fluent Icons\"",
                "IsExpanded=\"False\"",
                "Margin=\"2 -8 0 0\">");
            AssertContainsInOrder(
                iconographyXaml,
                "<TextBox x:Name=\"IconsSearchBox\" Text=\"{Binding ViewModel.SearchText, UpdateSourceTrigger=PropertyChanged, Delay=500}\"",
                "AutomationProperties.Name=\"Search Icons by Name, Tag\"",
                "Width=\"500\"",
                "HorizontalAlignment=\"Left\"",
                "VerticalAlignment=\"Center\"",
                "GotKeyboardFocus=\"IconsSearchBox_GotKeyboardFocus\"",
                "LostKeyboardFocus=\"IconsSearchBox_LostKeyboardFocus\"",
                "TextChanged=\"IconsSearchBox_TextChanged\"/>");
        }

        [TestMethod]
        public void DesignGuidanceColorTextSectionKeepsOfficialSourceShape()
        {
            var xaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "DesignGuidance",
                "TextSection.xaml");

            AssertContainsInOrder(
                xaml,
                "<Page x:Class=\"ModernWpf.Gallery.Pages.WpfGallery.DesignGuidance.TextSection\"",
                "xmlns:mc=\"http://schemas.openxmlformats.org/markup-compatibility/2006\"",
                "xmlns:d=\"http://schemas.microsoft.com/expression/blend/2008\"",
                "xmlns:controls=\"clr-namespace:ModernWpf.Gallery.Controls\"",
                "mc:Ignorable=\"d\"",
                "d:DesignHeight=\"450\" d:DesignWidth=\"800\"",
                "Title=\"TextSection\"",
                "Foreground=\"{DynamicResource WindowForeground}\">");
            StringAssert.Contains(xaml, "<!--  Colors section  -->");
            AssertContainsInOrder(
                xaml,
                "<controls:ColorPageExample Title=\"Text\" Description=\"For UI labels and static text\">",
                "<TextBlock",
                "FontSize=\"42\"",
                "FontWeight=\"SemiBold\"",
                "Text=\"Aa\" />");
            AssertContainsInOrder(
                xaml,
                "<Border Style=\"{StaticResource ColorTilesPanelStyle}\" Margin=\"0,8\">",
                "<controls:ColorTile",
                "Background=\"{DynamicResource TextFillColorPrimaryBrush}\"",
                "ColorBrushName=\"TextFillColorPrimaryBrush\"",
                "TileRadius=\"8,0,0,8\"",
                "ColorExplanation=\"Rest or Hover\"",
                "ColorName=\"Text / Primary\"",
                "ColorValue=\"#000000 (E4, 89.56%)\"",
                "Foreground=\"{DynamicResource TextOnAccentFillColorPrimaryBrush}\"",
                "ShowSeparator=\"False\" />");
            AssertContainsInOrder(
                xaml,
                "<!--  Accent text  -->",
                "<Border Style=\"{StaticResource ColorTilesPanelStyle}\" Margin=\"0,8\">",
                "Background=\"{DynamicResource AccentTextFillColorPrimaryBrush}\"",
                "ColorBrushName=\"AccentTextFillColorPrimaryBrush\"",
                "ColorExplanation=\"Rest or Hover\"",
                "TileRadius=\"8,0,0,8\"",
                "ColorName=\"Accent Text / Primary\"");
            AssertContainsInOrder(
                xaml,
                "<!--  Text on accent  -->",
                "<Border Style=\"{StaticResource ColorTilesPanelStyle}\" Margin=\"0,8\">",
                "<controls:ColorTile",
                "Background=\"{DynamicResource TextOnAccentFillColorPrimaryBrush}\"",
                "ColorBrushName=\"TextOnAccentFillColorPrimaryBrush\"",
                "TileRadius=\"8,0,0,8\"",
                "ColorExplanation=\"Rest or Hover\"",
                "ColorName=\"Text on Accent / Primary\"",
                "Foreground=\"{DynamicResource TextFillColorPrimaryBrush}\" />",
                "<controls:ColorTile",
                "Grid.Column=\"2\"",
                "Background=\"{DynamicResource TextOnAccentFillColorSecondaryBrush}\"",
                "TileRadius=\"0,8,8,0\"",
                "ColorBrushName=\"TextOnAccentFillColorSecondaryBrush\"");
            AssertContainsInOrder(
                xaml,
                "<Border Style=\"{StaticResource ColorTilesPanelStyle}\" Margin=\"0,8,0,0\">",
                "Background=\"{DynamicResource TextOnAccentFillColorDisabledBrush}\"",
                "ColorBrushName=\"TextOnAccentFillColorDisabledBrush\"",
                "TileRadius=\"8,0,0,8\"",
                "ColorExplanation=\"Disabled only (not accessible)\"",
                "Background=\"{DynamicResource TextOnAccentFillColorSelectedTextBrush}\"",
                "TileRadius=\"0,8,8,0\"",
                "ColorBrushName=\"TextOnAccentFillColorSelectedTextBrush\"");
        }

        [TestMethod]
        public void SamplesPageKeepsOfficialUserDashboardSourceShape()
        {
            var xaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Samples",
                "UserDashboardPage.xaml");

            AssertContainsInOrder(
                xaml,
                "d:DesignHeight=\"450\"",
                "d:DesignWidth=\"800\"",
                "d:DataContext=\"{d:DesignInstance Type=samples:UserDashboardPage}\"",
                "Foreground=\"{DynamicResource TextFillColorPrimaryBrush}\"",
                "mc:Ignorable=\"d\"",
                "SizeChanged=\"Page_SizeChanged\"");
            StringAssert.Contains(
                xaml,
                "<Style TargetType=\"Label\" x:Key=\"GenericLabelStyle\">");
            StringAssert.Contains(
                xaml,
                "<Setter Property=\"Opacity\" Value=\"0.67\"/>");
            StringAssert.Contains(
                xaml,
                "<RowDefinition Height=\"*\" MaxHeight=\"280\"/>");
            StringAssert.Contains(
                xaml,
                "<Grid x:Name=\"UserListGrid\" Grid.Column=\"0\" Grid.RowSpan=\"2\" >");
            AssertContainsInOrder(
                xaml,
                "<ListView",
                "x:Name=\"UserList\"",
                "AutomationProperties.Name=\"Users\"",
                "Grid.Row=\"0\"",
                "Width=\"300\"",
                "Background=\"{DynamicResource CardBackgroundFillColorDefaultBrush}\"",
                "ItemsSource=\"{Binding ViewModel.Users, Mode=TwoWay}\"",
                "SelectedItem=\"{Binding ViewModel.SelectedUser, Mode=TwoWay}\"",
                "SelectionMode=\"Single\">");
            StringAssert.Contains(
                xaml,
                "<Style TargetType=\"ListViewItem\" BasedOn=\"{StaticResource DefaultListViewItemStyle}\">");
            StringAssert.Contains(
                xaml,
                "<Setter Property=\"AutomationProperties.Name\" Value=\"{Binding Name, Mode=OneWay}\"/>");
            AssertContainsInOrder(
                xaml,
                "Margin=\"12,6,0,0\"",
                "Text=\"{Binding Name, Mode=OneWay}\"",
                "pages:GalleryAutomation.HeadingLevel=\"Level3\" />");
            StringAssert.Contains(
                xaml,
                "<StackPanel Margin=\"20,0,20,0\" >");
            StringAssert.Contains(
                xaml,
                "<Label Content=\"First Name\" Style=\"{StaticResource GenericLabelStyle}\" FontWeight=\"SemiBold\" />");
            StringAssert.Contains(
                xaml,
                "<TextBox AutomationProperties.Name=\"First Name\" Margin=\"0,5,0,15\" Text=\"{Binding ViewModel.EditableUser.FirstName}\" IsReadOnly=\"{Binding ViewModel.IsReadOnly}\"/>");
            StringAssert.Contains(
                xaml,
                "<TextBlock x:Name=\"AgeIndicatorBox\" Padding=\"10 0 0 0\" Grid.Column=\"1\" Text=\"{Binding ViewModel.EditableUser.Age}\" VerticalAlignment=\"Center\" />");
            AssertContainsInOrder(
                xaml,
                "<Slider",
                "AutomationProperties.Name=\"Age\"",
                "Maximum=\"62\"",
                "Minimum=\"21\"",
                "IsSnapToTickEnabled=\"True\"",
                "Value=\"{Binding ViewModel.EditableUser.Age}\"",
                "ValueChanged=\"AgeSlider_ValueChanged\"",
                "IsEnabled=\"{Binding ViewModel.IsEditing}\"/>");
            StringAssert.Contains(
                xaml,
                "<DatePicker AutomationProperties.Name=\"Date of Joining\" Margin=\"0,5,0,15\" SelectedDate=\"{Binding ViewModel.EditableUser.DateOfJoining}\" IsEnabled=\"{Binding ViewModel.IsEditing}\"/>");
            StringAssert.Contains(
                xaml,
                "<CheckBox AutomationProperties.Name=\"Is user a new graduate ?\" VerticalAlignment=\"Center\" IsChecked=\"{Binding ViewModel.EditableUser.IsNewGraduate}\" IsEnabled=\"{Binding ViewModel.IsEditing}\"/>");
            StringAssert.Contains(
                xaml,
                "<TextBlock FontSize=\"14\" HorizontalAlignment=\"Left\" VerticalAlignment=\"Center\" Visibility=\"{Binding ViewModel.IsSaved, Converter={StaticResource BooleanToVisibilityConverter}}\" FontStyle=\"Italic\">");
            StringAssert.Contains(
                xaml,
                "<TextBlock FontSize=\"14\" HorizontalAlignment=\"Left\" VerticalAlignment=\"Center\" Visibility=\"{Binding ViewModel.DeletedName, Converter={StaticResource EmptyToVisibilityConverter}}\" FontStyle=\"Italic\">");
            AssertContainsInOrder(
                xaml,
                "x:Name=\"edit_button\"",
                "Margin=\"10\"",
                "Command=\"{Binding ViewModel.EditUserStartCommand}\"",
                "Visibility=\"{Binding ViewModel.IsReadOnly, Converter={StaticResource BooleanToVisibilityConverter}}\"",
                "Click=\"EditButton_Click\"",
                "Content=\"Edit\" />");
            AssertContainsInOrder(
                xaml,
                "x:Name=\"save_button\"",
                "Margin=\"10\"",
                "Command=\"{Binding ViewModel.EditUserCommitCommand}\"",
                "Visibility=\"{Binding ViewModel.IsEditing, Converter={StaticResource BooleanToVisibilityConverter}}\"",
                "Click=\"SaveButton_Click\"",
                "Content=\"Save\"/>");
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
            var normalizedXaml = xaml.Replace("\r\n", "\n");
            StringAssert.Contains(
                xaml,
                "<controls:PageHeader Margin=\"0,0,0,32\" Title=\"{Binding ViewModel.PageTitle}\" ShowDescription=\"False\" />");
            StringAssert.Contains(
                xaml,
                "<ScrollViewer Grid.Row=\"1\" Margin=\"0,0,0,24\" Padding=\"0,0,24,0\">");
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
            StringAssert.Contains(
                normalizedXaml,
                "<TextBlock\n                                    AutomationProperties.Name=\"Bold\"\n                                    Focusable=\"False\"\n                                    FontFamily=\"{StaticResource SymbolThemeFontFamily}\"\n                                    FontSize=\"12\"\n                                    Text=\"&#xE8DD;\" />");
            StringAssert.Contains(
                normalizedXaml,
                "<TextBlock\n                                    AutomationProperties.Name=\"Italic\"\n                                    Focusable=\"False\"\n                                    FontFamily=\"{StaticResource SymbolThemeFontFamily}\"\n                                    FontSize=\"12\"\n                                    Text=\"&#xE8DB;\" />");
            StringAssert.Contains(
                normalizedXaml,
                "<TextBlock\n                                    AutomationProperties.Name=\"Underlined\"\n                                    Focusable=\"False\"\n                                    FontFamily=\"{StaticResource SymbolThemeFontFamily}\"\n                                    FontSize=\"12\"\n                                    Text=\"&#xE8DC;\" />");

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

        [TestMethod]
        public void NavigationSupportPagesKeepOfficialWindowLauncherSourceShape()
        {
            var frameXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Navigation",
                "FramePage.xaml");
            StringAssert.Contains(
                frameXaml,
                "<controls:PageHeader Margin=\"0,0,0,32\" Title=\"{Binding ViewModel.PageTitle}\" ShowDescription=\"False\" />");
            StringAssert.Contains(
                frameXaml,
                "<ScrollViewer Grid.Row=\"1\" Margin=\"0,0,0,24\" Padding=\"0,0,24,0\">");
            AssertContainsInOrder(
                frameXaml,
                "<Button",
                "x:Name=\"OpenFrameWindow\"",
                "VerticalAlignment=\"Center\"",
                "HorizontalAlignment=\"Center\"",
                "Content=\"Open window to view Frame\"",
                "Click=\"OpenFrameWindow_Click\" />");

            var navigationWindowXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Navigation",
                "NavigationWindowPage.xaml");
            StringAssert.Contains(
                navigationWindowXaml,
                "<controls:PageHeader Margin=\"0,0,0,32\" Title=\"{Binding ViewModel.PageTitle}\" ShowDescription=\"False\" />");
            StringAssert.Contains(
                navigationWindowXaml,
                "<ScrollViewer Grid.Row=\"1\" Margin=\"0,0,0,24\" Padding=\"0,0,24,0\">");
            AssertContainsInOrder(
                navigationWindowXaml,
                "<controls:ControlExample",
                "Margin=\"10\"",
                "HeaderText=\"A Navigation Window\"",
                "XamlCode=\"&lt;NavigationWindow",
                "CSharpCode=\"private void OpenNavigationWindow_Click(object sender, RoutedEventArgs e)");
            AssertContainsInOrder(
                navigationWindowXaml,
                "<Button",
                "x:Name=\"OpenNavigationWindow\"",
                "VerticalAlignment=\"Center\"",
                "HorizontalAlignment=\"Center\"",
                "Content=\"Open window to view NavigationWindow\"",
                "Click=\"OpenNavigationWindow_Click\" />");

            var navigationWindowCodeBehind = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Navigation",
                "NavigationWindowPage.xaml.cs");
            AssertContainsInOrder(
                navigationWindowCodeBehind,
                "NavigationWindow window = new NavigationWindow()",
                "{",
                "Width = 800,",
                "Height = 450,",
                "Source = new Uri(\"/Pages/WpfGallery/Navigation/Page1.xaml\", UriKind.Relative)",
                "};",
                "window.Show();");

            var frameWindowXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Navigation",
                "FrameWindow.xaml");
            AssertContainsInOrder(
                frameWindowXaml,
                "<Window x:Class=\"ModernWpf.Gallery.Pages.WpfGallery.Navigation.FrameWindow\"",
                "xmlns:d=\"http://schemas.microsoft.com/expression/blend/2008\"",
                "xmlns:mc=\"http://schemas.openxmlformats.org/markup-compatibility/2006\"",
                "mc:Ignorable=\"d\"",
                "Title=\"FrameWindow\" Height=\"450\" Width=\"800\">");
            StringAssert.Contains(
                frameWindowXaml,
                "<Frame Source=\"/Pages/WpfGallery/Navigation/Page1.xaml\" NavigationUIVisibility=\"Visible\"/>");

            var page1Xaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Navigation",
                "Page1.xaml");
            AssertContainsInOrder(
                page1Xaml,
                "<Page x:Class=\"ModernWpf.Gallery.Pages.WpfGallery.Navigation.Page1\"",
                "xmlns:mc=\"http://schemas.openxmlformats.org/markup-compatibility/2006\"",
                "xmlns:d=\"http://schemas.microsoft.com/expression/blend/2008\"",
                "mc:Ignorable=\"d\"",
                "d:DesignHeight=\"450\" d:DesignWidth=\"800\"",
                "Title=\"Page1\">");
            StringAssert.Contains(
                page1Xaml,
                "<StackPanel HorizontalAlignment=\"Center\" VerticalAlignment=\"Center\">");
            StringAssert.Contains(
                page1Xaml,
                "<TextBlock Text=\"This is Page 1\" FontSize=\"20\" Margin=\"10\" Foreground=\"{DynamicResource TextFillColorPrimaryBrush}\"/>");
            StringAssert.Contains(
                page1Xaml,
                "<Hyperlink NavigateUri=\"Page2.xaml\">This is the link to Page 2</Hyperlink>");

            var page2Xaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Navigation",
                "Page2.xaml");
            AssertContainsInOrder(
                page2Xaml,
                "<Page x:Class=\"ModernWpf.Gallery.Pages.WpfGallery.Navigation.Page2\"",
                "xmlns:mc=\"http://schemas.openxmlformats.org/markup-compatibility/2006\"",
                "xmlns:d=\"http://schemas.microsoft.com/expression/blend/2008\"",
                "mc:Ignorable=\"d\"",
                "d:DesignHeight=\"450\" d:DesignWidth=\"800\"",
                "Title=\"Page2\">");
            StringAssert.Contains(
                page2Xaml,
                "<TextBlock Text=\"This is Page 2\" HorizontalAlignment=\"Center\" VerticalAlignment=\"Center\" Foreground=\"{DynamicResource TextFillColorPrimaryBrush}\" FontSize=\"20\"/>");
        }

        [TestMethod]
        public void TabControlPageKeepsOfficialTabHeaderSourceShape()
        {
            var xaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Navigation",
                "TabControlPage.xaml");
            StringAssert.Contains(
                xaml,
                "<controls:PageHeader Margin=\"0,0,0,32\" Title=\"{Binding ViewModel.PageTitle}\" ShowDescription=\"False\" />");
            StringAssert.Contains(
                xaml,
                "<ScrollViewer Grid.Row=\"1\" Margin=\"0,0,0,24\" Padding=\"0,0,24,0\">");
            StringAssert.Contains(
                xaml,
                "<!--<SymbolIcon Margin=\"0,0,6,0\" Symbol=\"XboxConsole24\" />-->");
            StringAssert.Contains(
                xaml,
                "<!--<SymbolIcon Margin=\"0,0,6,0\" Symbol=\"StoreMicrosoft16\" />-->");

            var codeBehind = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Navigation",
                "TabControlPage.xaml.cs");
            StringAssert.Contains(
                codeBehind.Replace("\r\n", "\n"),
                "DataContext = this;\n\n            InitializeComponent();");
        }

        [TestMethod]
        public void TextPagesKeepOfficialHeaderAndInputSampleSourceShape()
        {
            foreach (var page in new[]
            {
                Tuple.Create("LabelPage.xaml", "<controls:PageHeader Margin=\"0,0,0,32\" Title=\"{Binding ViewModel.PageTitle}\" Description=\"{Binding ViewModel.PageDescription}\" />"),
                Tuple.Create("TextBoxPage.xaml", "<controls:PageHeader Margin=\"0,0,0,32\" Title=\"{Binding ViewModel.PageTitle}\" Description=\"{Binding ViewModel.PageDescription}\" />"),
                Tuple.Create("PasswordBoxPage.xaml", "<controls:PageHeader Margin=\"0,0,0,32\" Title=\"{Binding ViewModel.PageTitle}\" Description=\"{Binding ViewModel.PageDescription}\" />"),
                Tuple.Create("RichTextEditPage.xaml", "<controls:PageHeader Margin=\"0,0,0,32\" Title=\"{Binding ViewModel.PageTitle}\" Description=\"{Binding ViewModel.PageDescription}\" />"),
                Tuple.Create("TextBlockPage.xaml", "<controls:PageHeader Margin=\"0,0,0,32\" Title=\"{Binding ViewModel.PageTitle}\" Description=\"{Binding ViewModel.PageDescription}\" />"),
                Tuple.Create("HyperlinkPage.xaml", "<controls:PageHeader Margin=\"0,0,0,32\" Title=\"{Binding ViewModel.PageTitle}\" ShowDescription=\"False\" />")
            })
            {
                var xaml = ReadRepoFile(
                    "ModernWpf.Gallery",
                    "Pages",
                    "WpfGallery",
                    "Text",
                    page.Item1);
                StringAssert.Contains(
                    xaml,
                    page.Item2);
                StringAssert.Contains(
                    xaml,
                    "<ScrollViewer Grid.Row=\"1\" Margin=\"0,0,0,24\" Padding=\"0,0,24,0\">");
            }

            var labelXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Text",
                "LabelPage.xaml");
            StringAssert.Contains(
                labelXaml,
                "<Label Content=\"I am a Label.\" Foreground=\"{DynamicResource TextFillColorPrimaryBrush}\" Opacity=\"0.7\" />");
            StringAssert.Contains(
                labelXaml,
                "<!--  Target=\"{Binding ElementName=TextBoxForLabel}\"  -->");

            var textBoxXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Text",
                "TextBoxPage.xaml");
            StringAssert.Contains(
                textBoxXaml,
                "<controls:ControlExample Margin=\"10\" HeaderText=\"A simple TextBox.\" XamlCode=\"&lt;TextBox /&gt;\">");
            AssertContainsInOrder(
                textBoxXaml,
                "<controls:ControlExample Margin=\"10,36,10,10\"",
                "HeaderText=\"A TextBox with input validation.\"",
                "XamlCode=\"&lt;TextBox&gt;");
        }

        [TestMethod]
        public void StatusAndInfoPagesKeepOfficialHeaderAndToolTipSourceShape()
        {
            foreach (var page in new[]
            {
                "ProgressBarPage.xaml",
                "ToolTipPage.xaml"
            })
            {
                var xaml = ReadRepoFile(
                    "ModernWpf.Gallery",
                    "Pages",
                    "WpfGallery",
                    "StatusAndInfo",
                    page);
                var normalizedXaml = xaml.Replace("\r\n", "\n").Replace('\r', '\n');
                StringAssert.Contains(
                    xaml,
                    "xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages.WpfGallery.StatusAndInfo\"");
                StringAssert.Contains(
                    normalizedXaml,
                    "<Grid x:Name=\"ContentPagePane\" Height=\"Auto\">\n\n        <Grid.RowDefinitions>");
                StringAssert.Contains(
                    normalizedXaml,
                    "</Grid.RowDefinitions>\n        <controls:PageHeader");
                StringAssert.Contains(
                    xaml,
                    "<controls:PageHeader Margin=\"0,0,0,32\" Title=\"{Binding ViewModel.PageTitle}\" ShowDescription=\"False\" />");
                StringAssert.Contains(
                    xaml,
                    "<ScrollViewer Grid.Row=\"1\" Margin=\"0,0,0,24\" Padding=\"0,0,24,0\">");
            }

            var toolTipXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "StatusAndInfo",
                "ToolTipPage.xaml");
            AssertContainsInOrder(
                toolTipXaml,
                "<Button",
                "Content=\"Button with a simple ToolTip.\"",
                "ToolTipService.InitialShowDelay=\"100\"",
                "ToolTipService.Placement=\"MousePoint\"",
                "AutomationProperties.Name=\"TooltipButton\"",
                "ToolTipService.ToolTip=\"Simple ToolTip\" />");
        }

        [TestMethod]
        public void DateAndMediaPagesKeepOfficialHeaderAndSimpleSampleSourceShape()
        {
            foreach (var page in new[]
            {
                Tuple.Create(
                    "DateAndTime",
                    "CalendarPage.xaml",
                    "<controls:PageHeader Margin=\"0,0,0,32\" Title=\"{Binding ViewModel.PageTitle}\" ShowDescription=\"False\" />",
                    "xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages.WpfGallery.DateAndTime\""),
                Tuple.Create(
                    "DateAndTime",
                    "DatePickerPage.xaml",
                    "<controls:PageHeader Margin=\"0,0,0,32\" Title=\"{Binding ViewModel.PageTitle}\" ShowDescription=\"False\" />",
                    "xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages.WpfGallery.DateAndTime\""),
                Tuple.Create(
                    "Media",
                    "CanvasPage.xaml",
                    "<controls:PageHeader Margin=\"0,0,0,32\" Title=\"{Binding ViewModel.PageTitle}\" Description=\"{Binding ViewModel.PageDescription}\" />",
                    "xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages.WpfGallery.Media\""),
                Tuple.Create(
                    "Media",
                    "ImagePage.xaml",
                    "<controls:PageHeader Margin=\"0,0,0,32\" Title=\"{Binding ViewModel.PageTitle}\" Description=\"{Binding ViewModel.PageDescription}\" />",
                    "xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages.WpfGallery.Media\"")
            })
            {
                var xaml = ReadRepoFile(
                    "ModernWpf.Gallery",
                    "Pages",
                    "WpfGallery",
                    page.Item1,
                    page.Item2);
                var normalizedXaml = xaml.Replace("\r\n", "\n").Replace('\r', '\n');
                StringAssert.Contains(xaml, page.Item4);
                StringAssert.Contains(
                    normalizedXaml,
                    "<Grid x:Name=\"ContentPagePane\" Height=\"Auto\">\n\n        <Grid.RowDefinitions>");
                StringAssert.Contains(
                    normalizedXaml,
                    "</Grid.RowDefinitions>\n        <controls:PageHeader");
                StringAssert.Contains(xaml, page.Item3);
                StringAssert.Contains(
                    xaml,
                    "<ScrollViewer Grid.Row=\"1\" Margin=\"0,0,0,24\" Padding=\"0,0,24,0\">");
            }

            var calendarXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "DateAndTime",
                "CalendarPage.xaml");
            StringAssert.Contains(
                calendarXaml,
                "<Calendar HorizontalAlignment=\"Left\" AutomationProperties.Name=\"Default\" KeyboardNavigation.IsTabStop=\"False\"/>");

            var imageXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Media",
                "ImagePage.xaml");
            StringAssert.Contains(
                imageXaml.Replace("\r\n", "\n").Replace('\r', '\n'),
                "</Page.Resources>\n\n\n    <Grid x:Name=\"ContentPagePane\" Height=\"Auto\">");
        }

        [TestMethod]
        public void SystemPagesKeepOfficialHeaderAndControlExampleSourceShape()
        {
            foreach (var page in new[]
            {
                Tuple.Create(
                    "FileAndFolderDialogsPage.xaml",
                    "<controls:PageHeader Margin=\"0,0,0,32\" Title=\"{Binding ViewModel.PageTitle}\" ShowDescription=\"False\" />",
                    "<ScrollViewer Grid.Row=\"1\" Margin=\"0,0,0,24\" Padding=\"0,0,24,0\">"),
                Tuple.Create(
                    "MessageBoxPage.xaml",
                    "<controls:PageHeader Margin=\"0,0,0,32\" Title=\"{Binding ViewModel.PageTitle}\" ShowDescription=\"False\" />",
                    "<ScrollViewer Grid.Row=\"1\" Margin=\"0,0,0,24\" Padding=\"0,0,24,0\">"),
                Tuple.Create(
                    "ClipboardPage.xaml",
                    "<controls:PageHeader Grid.Row=\"0\" Margin=\"0,0,0,32\" Title=\"{Binding ViewModel.PageTitle}\" ShowDescription=\"False\" />",
                    "<ScrollViewer Grid.Row=\"2\" Margin=\"0,0,0,24\" Padding=\"0,0,24,0\">")
            })
            {
                var xaml = ReadRepoFile(
                    "ModernWpf.Gallery",
                    "Pages",
                    "WpfGallery",
                    "System",
                    page.Item1);
                var normalizedXaml = xaml.Replace("\r\n", "\n").Replace('\r', '\n');
                StringAssert.Contains(
                    xaml,
                    "xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages.WpfGallery.SystemPages\"");
                StringAssert.Contains(
                    normalizedXaml,
                    "<Grid x:Name=\"ContentPagePane\" Height=\"Auto\">\n\n        <Grid.RowDefinitions>");
                if (page.Item1 == "ClipboardPage.xaml")
                {
                    StringAssert.Contains(
                        normalizedXaml,
                        "</Grid.RowDefinitions>\n\n        <controls:PageHeader");
                }
                else
                {
                    StringAssert.Contains(
                        normalizedXaml,
                        "</Grid.RowDefinitions>\n        <controls:PageHeader");
                }

                StringAssert.Contains(xaml, page.Item2);
                StringAssert.Contains(xaml, page.Item3);
                AssertControlExamplesKeepOfficialSourceAttributeOrder(xaml, page.Item1);
            }
        }

        private static void AssertControlExamplesKeepOfficialSourceAttributeOrder(string xaml, string pageName)
        {
            var searchIndex = 0;
            var exampleCount = 0;
            while (true)
            {
                var startIndex = xaml.IndexOf("<controls:ControlExample", searchIndex, StringComparison.Ordinal);
                if (startIndex < 0)
                {
                    break;
                }

                var endIndex = xaml.IndexOf(">", startIndex, StringComparison.Ordinal);
                Assert.IsTrue(endIndex > startIndex, pageName + " should have a closed ControlExample start tag.");
                var startTag = xaml.Substring(startIndex, endIndex - startIndex + 1);
                var headerIndex = startTag.IndexOf("HeaderText=", StringComparison.Ordinal);
                var xamlCodeIndex = startTag.IndexOf("XamlCode=", StringComparison.Ordinal);
                var csharpCodeIndex = startTag.IndexOf("CSharpCode=", StringComparison.Ordinal);

                Assert.IsTrue(headerIndex >= 0, pageName + " ControlExample should keep an official HeaderText attribute.");
                Assert.IsTrue(xamlCodeIndex >= 0, pageName + " ControlExample should keep an official XamlCode attribute.");
                Assert.IsTrue(csharpCodeIndex >= 0, pageName + " ControlExample should keep an official CSharpCode attribute.");
                Assert.IsTrue(
                    headerIndex < xamlCodeIndex && xamlCodeIndex < csharpCodeIndex,
                    pageName + " ControlExample should match the official HeaderText, XamlCode, CSharpCode attribute order.");

                exampleCount++;
                searchIndex = endIndex + 1;
            }

            Assert.IsTrue(exampleCount > 0, pageName + " should contain copied ControlExample samples.");
        }
    }
}
