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
                StringAssert.Contains(
                    xaml,
                    "<controls:PageHeader Margin=\"0,0,0,32\" Title=\"{Binding ViewModel.PageTitle}\" ShowDescription=\"False\" />");
                StringAssert.Contains(
                    xaml,
                    "<ScrollViewer Grid.Row=\"1\" Margin=\"0,0,0,24\" Padding=\"0,0,24,0\">");
            }

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
                "DataGridPage.xaml",
                "ListBoxPage.xaml",
                "ListViewPage.xaml",
                "TreeViewPage.xaml"
            })
            {
                var xaml = ReadRepoFile(
                    "ModernWpf.Gallery",
                    "Pages",
                    "WpfGallery",
                    "Collections",
                    page);
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
                    "<controls:PageHeader Margin=\"0,0,0,32\" Title=\"{Binding ViewModel.PageTitle}\" ShowDescription=\"False\" />"),
                Tuple.Create(
                    "DateAndTime",
                    "DatePickerPage.xaml",
                    "<controls:PageHeader Margin=\"0,0,0,32\" Title=\"{Binding ViewModel.PageTitle}\" ShowDescription=\"False\" />"),
                Tuple.Create(
                    "Media",
                    "CanvasPage.xaml",
                    "<controls:PageHeader Margin=\"0,0,0,32\" Title=\"{Binding ViewModel.PageTitle}\" Description=\"{Binding ViewModel.PageDescription}\" />"),
                Tuple.Create(
                    "Media",
                    "ImagePage.xaml",
                    "<controls:PageHeader Margin=\"0,0,0,32\" Title=\"{Binding ViewModel.PageTitle}\" Description=\"{Binding ViewModel.PageDescription}\" />")
            })
            {
                var xaml = ReadRepoFile(
                    "ModernWpf.Gallery",
                    "Pages",
                    "WpfGallery",
                    page.Item1,
                    page.Item2);
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
