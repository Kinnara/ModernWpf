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
        public void ShellChromeKeepsWpfGalleryHighContrastSourceShape()
        {
            var mainWindowXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "MainWindow.xaml");

            AssertContainsInOrder(
                mainWindowXaml,
                "x:Key=\"GalleryTitleBarButtonStyle\"",
                "<MultiDataTrigger>",
                "<Condition Binding=\"{Binding Path=(SystemParameters.HighContrast)}\" Value=\"True\" />",
                "<Condition Binding=\"{Binding IsMouseOver, RelativeSource={RelativeSource Mode=Self}}\" Value=\"True\" />",
                "<Setter Property=\"Background\" Value=\"{DynamicResource SystemColorHighlightColorBrush}\" />",
                "<Setter Property=\"Foreground\" Value=\"{DynamicResource SystemColorHighlightTextColorBrush}\" />");
            AssertContainsInOrder(
                mainWindowXaml,
                "x:Key=\"GalleryTitleBarDefaultCloseButtonStyle\"",
                "<MultiDataTrigger>",
                "<Condition Binding=\"{Binding Path=(SystemParameters.HighContrast)}\" Value=\"True\" />",
                "<Condition Binding=\"{Binding IsMouseOver, RelativeSource={RelativeSource Mode=Self}}\" Value=\"True\" />",
                "<Setter Property=\"Background\" Value=\"{DynamicResource SystemColorHighlightColorBrush}\" />",
                "<Setter Property=\"Foreground\" Value=\"{DynamicResource SystemColorHighlightTextColorBrush}\" />");
            AssertContainsInOrder(
                mainWindowXaml,
                "x:Name=\"HighContrastBorder\"",
                "BorderBrush=\"Transparent\"",
                "BorderThickness=\"8 1 8 8\"");
            AssertContainsInOrder(
                mainWindowXaml,
                "x:Name=\"BackButton\"",
                "Height=\"36\"",
                "MinWidth=\"36\"",
                "Margin=\"8,0\"",
                "VerticalAlignment=\"Center\"",
                "AutomationProperties.Name=\"Back\"",
                "Style=\"{StaticResource GalleryTitleBarButtonStyle}\"",
                "Command=\"{Binding ViewModel.BackCommand}\"",
                "IsEnabled=\"{Binding ViewModel.CanNavigateback}\"",
                "winShell:WindowChrome.IsHitTestVisibleInChrome=\"True\"",
                "ToolTipService.ToolTip=\"Back\"");
            AssertContainsInOrder(
                mainWindowXaml,
                "Text=\"&#xE72B;\"",
                "Style=\"{StaticResource CaptionTextBlockStyle}\"",
                "pages:GalleryAutomation.HeadingLevel=\"Level1\"",
                "Text=\"{Binding ViewModel.ApplicationTitle}\"");

            var navigationRootXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Shell",
                "NavigationRootPage.xaml");

            AssertContainsInOrder(
                navigationRootXaml,
                "x:Key=\"GalleryNavigationFooterButtonStyle\"",
                "<MultiDataTrigger>",
                "<Condition Binding=\"{Binding Path=(SystemParameters.HighContrast)}\" Value=\"True\" />",
                "<Condition Binding=\"{Binding IsMouseOver, RelativeSource={RelativeSource Mode=Self}}\" Value=\"True\" />",
                "<Setter Property=\"Background\" Value=\"{DynamicResource SystemColorHighlightColorBrush}\" />",
                "<Setter Property=\"Foreground\" Value=\"{DynamicResource SystemColorHighlightTextColorBrush}\" />");
            AssertContainsInOrder(
                navigationRootXaml,
                "x:Name=\"SettingsButton\"",
                "Width=\"250\"",
                "Height=\"36\"",
                "Margin=\"0,4,0,0\"",
                "Padding=\"{StaticResource ButtonPadding}\"",
                "HorizontalContentAlignment=\"Left\"",
                "VerticalContentAlignment=\"Center\"",
                "AutomationProperties.Name=\"Settings\"",
                "Click=\"OnSettingsButtonClick\"",
                "Command=\"{Binding Value.ViewModel.SettingsCommand, Source={StaticResource NavigationRootDataContextProxy}}\"",
                "Style=\"{StaticResource GalleryNavigationFooterButtonStyle}\"");

            var mainWindowCode = ReadRepoFile(
                "ModernWpf.Gallery",
                "MainWindow.xaml.cs");

            AssertContainsInOrder(
                mainWindowCode,
                "SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;",
                "StateChanged += OnWindowStateChanged;",
                "Activated += OnWindowActivationChanged;",
                "Deactivated += OnWindowActivationChanged;",
                "MainGrid.Margin = GetMainGridMargin(WindowState, SystemParameters.HighContrast);",
                "UpdateTitleBarButtonsVisibility();",
                "if (SystemParameters.HighContrast)",
                "HighContrastBorder.SetResourceReference(",
                "System.Windows.Controls.Border.BorderBrushProperty,",
                "IsActive ? SystemColors.ActiveCaptionBrushKey : SystemColors.InactiveCaptionBrushKey);",
                "HighContrastBorder.BorderThickness = GetHighContrastBorderThickness(SystemParameters.HighContrast);",
                "return isHighContrast ? new Thickness(8, 1, 8, 8) : new Thickness(0);",
                "if (isHighContrast || !isWindows11OrGreater)");

            var navigationRootCode = ReadRepoFile(
                "ModernWpf.Gallery",
                "Shell",
                "NavigationRootPage.xaml.cs");

            AssertContainsInOrder(
                navigationRootCode,
                "ThemeManager.Current.ActualApplicationThemeChanged += OnActualApplicationThemeChanged;",
                "SystemParameters.StaticPropertyChanged += OnSystemParametersChanged;",
                "ThemeManager.Current.ActualApplicationThemeChanged -= OnActualApplicationThemeChanged;",
                "SystemParameters.StaticPropertyChanged -= OnSystemParametersChanged;",
                "if (string.Equals(e.PropertyName, nameof(SystemParameters.HighContrast), StringComparison.Ordinal))",
                "AlignNavigationViewItemResourcesWithWpfGalleryTreeView();");
        }

        [TestMethod]
        public void ItemPageWrapperAvoidsLocalOnlyAutomationHooks()
        {
            var xaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "ItemPage.xaml");

            Assert.IsFalse(
                xaml.Contains("x:Name=\"PageHeader\"", StringComparison.Ordinal),
                "The generic wrapper header should be located structurally instead of by a local-only name.");
            Assert.IsFalse(
                xaml.Contains("AutomationProperties.AutomationId=\"GallerySampleHost\"", StringComparison.Ordinal),
                "The generic wrapper should not expose the local-only GallerySampleHost automation ID.");
            Assert.IsFalse(
                xaml.Contains("x:Name=\"DirectPageContentHost\"", StringComparison.Ordinal),
                "The direct-page wrapper frame should be located structurally instead of by a local-only name.");
            AssertContainsInOrder(
                xaml,
                "<ItemsControl",
                "ItemsSource=\"{Binding Examples}\"",
                "<controls:ControlExample",
                "HeaderText=\"{Binding HeaderText}\"",
                "XamlCode=\"{Binding XamlCode}\"",
                "CSharpCode=\"{Binding CSharpCode}\"",
                "ExampleContent=\"{Binding ExampleContent}\"",
                "Margin=\"{Binding Margin}\" />");
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
                "<Border",
                "CornerRadius=\"8,0,0,0\"",
                "Grid.RowSpan=\"2\"",
                "<StackPanel",
                "Margin=\"36,48,0,0\"",
                "VerticalAlignment=\"Top\"",
                "TextElement.Foreground=\"Black\"",
                "<TextBlock",
                "Style=\"{StaticResource SubtitleTextBlockStyle}\"",
                "Text=\".NET 10\"",
                "Margin=\"0,0,0,2\"",
                "pages:GalleryAutomation.HeadingLevel=\"Level1\" />",
                "<TextBlock",
                "Style=\"{StaticResource TitleLargeTextBlockStyle}\"",
                "Text=\"WPF Gallery\"",
                "Margin=\"0,0,0,8\"",
                "pages:GalleryAutomation.HeadingLevel=\"Level1\" />",
                "<Border",
                "Background=\"Transparent\"",
                "CornerRadius=\"8,8,8,8\"",
                "MaxWidth=\"300\"",
                "HorizontalAlignment=\"Left\"",
                "<TextBlock",
                "MaxWidth=\"300\"",
                "Margin=\"0,0,0,0\"",
                "Style=\"{StaticResource BodyStrongTextBlockStyle}\"",
                "Text=\"A collection of controls, guidelines and samples to build great WPF applications\"",
                "TextAlignment=\"Left\"",
                "HorizontalAlignment=\"Left\"",
                "Padding=\"0,8,12,8\" />");
            Assert.AreEqual(
                1,
                xaml.Split(new[] { "Foreground=\"Black\"" }, StringSplitOptions.None).Length - 1,
                "Home hero text should inherit black foreground from the source-shaped StackPanel.");
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

            var comboBoxXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "BasicInput",
                "ComboBoxPage.xaml");
            AssertContainsInOrder(
                comboBoxXaml,
                "<StackPanel Margin=\"0,0,0,24\">",
                "HeaderText=\"A ComboBox with items defined inline.\"",
                "AutomationProperties.Name=\"Sample defined inline\"",
                "<ComboBoxItem Content=\"Blue\" />",
                "<ComboBoxItem Content=\"Green\" />",
                "<ComboBoxItem Content=\"Red\" />",
                "<ComboBoxItem Content=\"Yellow\" />",
                "HeaderText=\"A ComboBox with ItemsSource set.\"",
                "AutomationProperties.Name=\"Sample item source set\"",
                "ItemsSource=\"{Binding ViewModel.ComboBoxFontFamilies, RelativeSource={RelativeSource Mode=FindAncestor, AncestorType=local:ComboBoxPage}, Mode=OneWay}\"",
                "<ComboBox.ItemTemplate>",
                "<TextBlock FontFamily=\"{Binding}\" Text=\"{Binding}\" />",
                "HeaderText=\"An editable ComboBox.\"",
                "AutomationProperties.Name=\"Editable\"",
                "IsEditable=\"True\"",
                "ItemsSource=\"{Binding ViewModel.ComboBoxFontSizes, RelativeSource={RelativeSource Mode=FindAncestor, AncestorType=local:ComboBoxPage}, Mode=OneWay}\"",
                "SelectedIndex=\"0\" />");

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

            var sliderXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "BasicInput",
                "SliderPage.xaml");
            var normalizedSliderXaml = sliderXaml.Replace("\r\n", "\n").Replace('\r', '\n');
            AssertContainsInOrder(
                sliderXaml,
                "<StackPanel Margin=\"0,0,0,24\">",
                "HeaderText=\"A simple slider.\"",
                "AutomationProperties.Name=\"Simple\"",
                "Value=\"{Binding ViewModel.SimpleSliderValue, RelativeSource={RelativeSource Mode=FindAncestor, AncestorType=local:SliderPage}, Mode=TwoWay}\"",
                "Text=\"{Binding ViewModel.SimpleSliderValue, RelativeSource={RelativeSource Mode=FindAncestor, AncestorType=local:SliderPage}, Mode=OneWay}\"",
                "HeaderText=\"A slider with steps and range specified.\"",
                "AutomationProperties.Name=\"Range and steps specified\"",
                "TickFrequency=\"50\"",
                "Value=\"{Binding ViewModel.RangeSliderValue, RelativeSource={RelativeSource Mode=FindAncestor, AncestorType=local:SliderPage}, Mode=TwoWay}\"",
                "HeaderText=\"A slider with tick marks.\"",
                "AutomationProperties.Name=\"Tick marks\"",
                "TickFrequency=\"20\"",
                "TickPlacement=\"Both\"",
                "Value=\"{Binding ViewModel.MarksSliderValue, RelativeSource={RelativeSource Mode=FindAncestor, AncestorType=local:SliderPage}, Mode=TwoWay}\"",
                "HeaderText=\"A vertical slider with range and tick marks specified.\"",
                "AutomationProperties.Name=\"Vertical\"",
                "Orientation=\"Vertical\"",
                "Value=\"{Binding ViewModel.VerticalSliderValue, RelativeSource={RelativeSource Mode=FindAncestor, AncestorType=local:SliderPage}, Mode=TwoWay}\"");
            StringAssert.Contains(
                normalizedSliderXaml,
                "</controls:ControlExample>\n            </StackPanel>\n\n        </ScrollViewer>");
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
                Tuple.Create("BorderPage.xaml", false),
                Tuple.Create("ExpanderPage.xaml", false),
                Tuple.Create("GridPage.xaml", false),
                Tuple.Create("GridSplitterPage.xaml", false),
                Tuple.Create("GroupBoxPage.xaml", true),
                Tuple.Create("ResizeGripPage.xaml", true),
                Tuple.Create("StackPanelPage.xaml", false)
            })
            {
                var xaml = ReadRepoFile(
                    "ModernWpf.Gallery",
                    "Pages",
                    "WpfGallery",
                    "Layout",
                    page.Item1);
                var normalizedXaml = xaml.Replace("\r\n", "\n").Replace('\r', '\n');
                StringAssert.Contains(
                    xaml,
                    "xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages.WpfGallery.Layout\"");
                if (page.Item1 == "GridSplitterPage.xaml")
                {
                    StringAssert.Contains(
                        xaml,
                        "xmlns:sys=\"clr-namespace:System;assembly=System.Runtime\"");
                }

                if (page.Item2)
                {
                    StringAssert.Contains(
                        normalizedXaml,
                        "        <Grid x:Name=\"ContentPagePane\" Height=\"Auto\">\n            <Grid.RowDefinitions>");
                    StringAssert.Contains(
                        normalizedXaml,
                        "            </Grid.RowDefinitions>\n            <controls:PageHeader");
                }
                else
                {
                    StringAssert.Contains(
                        normalizedXaml,
                        "<Grid x:Name=\"ContentPagePane\" Height=\"Auto\">\n\n        <Grid.RowDefinitions>");
                    StringAssert.Contains(
                        normalizedXaml,
                        "</Grid.RowDefinitions>\n        <controls:PageHeader");
                }

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

            var borderXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Layout",
                "BorderPage.xaml");
            AssertContainsInOrder(
                borderXaml,
                "<Border BorderBrush=\"Gray\" BorderThickness=\"2\" Padding=\"10\">",
                "<Border BorderBrush=\"CornflowerBlue\" BorderThickness=\"2\" CornerRadius=\"10\" Padding=\"15\" Background=\"LightBlue\">",
                "<TextBlock Text=\"Rounded Border\" Foreground=\"Black\" />",
                "<Border BorderBrush=\"DarkSlateGray\" BorderThickness=\"1,2,4,8\" Padding=\"10\">");

            var gridXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Layout",
                "GridPage.xaml");
            AssertContainsInOrder(
                gridXaml,
                "HeaderText=\"A Grid with custom sizing and spanning\"",
                "<Border Grid.Row=\"0\" Grid.Column=\"0\" Background=\"{DynamicResource ControlFillColorDefaultBrush}\" Margin=\"5\" Padding=\"10\">",
                "<Border Grid.Row=\"1\" Grid.Column=\"0\" Grid.ColumnSpan=\"3\" Background=\"{DynamicResource ControlFillColorSecondaryBrush}\" Margin=\"5\" Padding=\"10\">",
                "HeaderText=\"Grid using XAML shorthand syntax\"",
                "XamlCode=\"&lt;Grid RowDefinitions=&quot;Auto,*,Auto&quot; ColumnDefinitions=&quot;100,2*,*&quot;&gt;",
                "<Grid Height=\"300\">",
                "<Grid.RowDefinitions>",
                "<Border Grid.Row=\"0\" Grid.Column=\"0\" Background=\"{DynamicResource ControlFillColorDefaultBrush}\" Margin=\"5\" Padding=\"10\">",
                "<Border Grid.Row=\"1\" Grid.Column=\"0\" Grid.ColumnSpan=\"3\" Background=\"{DynamicResource ControlAltFillColorSecondaryBrush}\" Margin=\"5\" Padding=\"10\">",
                "<TextBlock Text=\"Main Content Area (fills available space)\" VerticalAlignment=\"Center\" HorizontalAlignment=\"Center\" />");

            var gridSplitterXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Layout",
                "GridSplitterPage.xaml");
            AssertContainsInOrder(
                gridSplitterXaml,
                "<TextBlock Style=\"{DynamicResource TitleTextBlockStyle}\" Text=\"Grid Splitter\" Margin=\"0 0 0 10\"/>",
                "<Border",
                "BorderBrush=\"{DynamicResource ControlElevationBorderBrush}\"",
                "BorderThickness=\"2\"",
                "Grid.Row=\"1\"",
                "Padding=\"10\"",
                "CornerRadius=\"4\">",
                "<TextBlock TextWrapping=\"Wrap\" Text=\"{StaticResource SampleText}\" />",
                "<GridSplitter Grid.RowSpan=\"5\" Grid.Column=\"1\" ResizeDirection=\"Columns\" />",
                "<GridSplitter Grid.Row=\"1\" Grid.ColumnSpan=\"3\" ResizeDirection=\"Rows\" />",
                "<GridSplitter Grid.Row=\"3\" Grid.ColumnSpan=\"1\" ResizeDirection=\"Rows\" />");

            var groupBoxXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Layout",
                "GroupBoxPage.xaml");
            AssertContainsInOrder(
                groupBoxXaml,
                "<GroupBox",
                "Header=\"User Information\"",
                "HorizontalAlignment=\"Left\"",
                "VerticalAlignment=\"Center\"",
                "Width=\"400\">",
                "<TextBox Name=\"NameTextBox\" Width=\"280\" Margin=\"10,0,0,20\" AutomationProperties.Name=\"Name Field\" />",
                "<TextBlock Width=\"100\" Text=\"Gender:\" Margin=\"0,10,0,0\" />",
                "<TextBox Name=\"GenderTextBox\" Width=\"280\" Margin=\"10,0,0,20\" AutomationProperties.Name=\"Gender Field\" />",
                "<Button Content=\"Submit\" HorizontalAlignment=\"Right\" Margin=\"0,10,0,0\" />");
            StringAssert.Contains(
                groupBoxXaml,
                "&lt;Button Content=&quot;Submit&quot; HorizontalAlignment=&quot;Right&quot; Width=&quot;100&quot; Margin=&quot;0,10,0,0&quot; /&gt;");

            var resizeGripXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Layout",
                "ResizeGripPage.xaml");
            StringAssert.Contains(
                resizeGripXaml,
                "<StackPanel Orientation=\"Vertical\" Grid.Row=\"1\">");
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

            var stackPanelXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Layout",
                "StackPanelPage.xaml");
            AssertContainsInOrder(
                stackPanelXaml,
                "<StackPanel Orientation=\"Vertical\">",
                "<Rectangle Width=\"100\" Height=\"30\" Fill=\"CornflowerBlue\" Margin=\"5\" />",
                "<Rectangle Width=\"100\" Height=\"30\" Fill=\"LightCoral\" Margin=\"5\" />",
                "<Rectangle Width=\"100\" Height=\"30\" Fill=\"MediumSeaGreen\" Margin=\"5\" />",
                "<StackPanel Orientation=\"Horizontal\">",
                "<Rectangle Width=\"100\" Height=\"30\" Fill=\"CornflowerBlue\" Margin=\"5\" />",
                "<Rectangle Width=\"100\" Height=\"30\" Fill=\"LightCoral\" Margin=\"5\" />",
                "<Rectangle Width=\"100\" Height=\"30\" Fill=\"MediumSeaGreen\" Margin=\"5\" />");
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
            var normalizedColorXaml = colorXaml.Replace("\r\n", "\n").Replace('\r', '\n');
            StringAssert.Contains(
                colorXaml,
                "xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages.WpfGallery.DesignGuidance\"");
            StringAssert.Contains(
                normalizedColorXaml,
                "<Grid x:Name=\"ContentPagePane\" Height=\"Auto\">\n\n        <Grid.RowDefinitions>");
            StringAssert.Contains(
                normalizedColorXaml,
                "</Grid.RowDefinitions>\n\n        <controls:PageHeader");
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
                var normalizedXaml = xaml.Replace("\r\n", "\n").Replace('\r', '\n');
                StringAssert.Contains(
                    xaml,
                    "xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages.WpfGallery.DesignGuidance\"");
                Assert.IsFalse(
                    xaml.Contains("x:Name=\"ContentPagePane\"", StringComparison.Ordinal),
                    page + " should keep the official unnamed root Grid shape.");
                StringAssert.Contains(
                    normalizedXaml,
                    "<Grid>\n        <Grid.RowDefinitions>");
                StringAssert.Contains(
                    normalizedXaml,
                    "</Grid.RowDefinitions>\n\n        <controls:PageHeader");
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
            var normalizedGeometryXaml = geometryXaml.Replace("\r\n", "\n").Replace('\r', '\n');
            StringAssert.Contains(
                geometryXaml,
                "xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages.WpfGallery.DesignGuidance\"");
            Assert.IsFalse(
                geometryXaml.Contains("x:Name=\"ContentPagePane\"", StringComparison.Ordinal),
                "GeometryPage.xaml should keep the official unnamed root Grid shape.");
            StringAssert.Contains(
                normalizedGeometryXaml,
                "<Grid>\n        <Grid.RowDefinitions>");
            StringAssert.Contains(
                normalizedGeometryXaml,
                "</Grid.RowDefinitions>\n\n        <controls:PageHeader");
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
            Assert.IsFalse(
                iconographyXaml.Contains("xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages.WpfGallery.DesignGuidance\"", StringComparison.Ordinal),
                "IconographyPage.xaml should keep the current official root namespace shape, which has no local namespace declaration.");
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
            AssertContainsInOrder(
                iconographyXaml,
                "<Grid",
                "Grid.Column=\"1\"",
                "Grid.Row=\"0\"",
                "Background=\"{DynamicResource ButtonBackground}\"",
                "<ItemsControl",
                "x:Name=\"TagsItemsControl\"",
                "ItemsSource=\"{Binding ViewModel.SelectedIcon.Tags}\"",
                "Margin=\"0,0,0,12\"",
                "Visibility=\"{Binding RelativeSource={RelativeSource Self}, Path=HasItems, Converter={StaticResource BooleanToVisibilityConverter}}\"",
                "AutomationProperties.Name=\"Selected Icon Tags\"",
                "<Button",
                "Style=\"{StaticResource IconTagChipButtonStyle}\"",
                "Command=\"{Binding ViewModel.ApplyTagFilterCommand, RelativeSource={RelativeSource AncestorType=Page}}\"",
                "AutomationProperties.Name=\"{Binding}\"",
                "CommandParameter=\"{Binding}\"",
                "<TextBlock",
                "Text=\"{Binding}\"",
                "Style=\"{StaticResource CaptionTextBlockStyle}\"",
                "Foreground=\"{DynamicResource TextFillColorPrimaryBrush}\"");
            AssertContainsInOrder(
                iconographyXaml,
                "<Grid Grid.Row=\"5\">",
                "<StackPanel",
                "Margin=\"0,0,0,0\"",
                "Orientation=\"Horizontal\"",
                "HorizontalAlignment=\"Left\"",
                "<Button",
                "Style=\"{StaticResource PaginationButtonStyle}\"",
                "Command=\"{Binding ViewModel.PreviousPageCommand}\"",
                "Margin=\"0,0,8,0\"",
                "Padding=\"8\"",
                "ToolTip=\"Previous Page\"",
                "AutomationProperties.Name=\"Previous Page\"",
                "<TextBlock",
                "FontFamily=\"{StaticResource SymbolThemeFontFamily}\"",
                "Text=\"&#xF08D;\"",
                "FontSize=\"12\"",
                "<Button",
                "Style=\"{StaticResource PaginationButtonStyle}\"",
                "Command=\"{Binding ViewModel.NextPageCommand}\"",
                "Padding=\"8\"",
                "ToolTip=\"Next Page\"",
                "AutomationProperties.Name=\"Next Page\"",
                "<StackPanel Orientation=\"Horizontal\" Grid.Column=\"1\">",
                "<TextBlock",
                "Style=\"{StaticResource BodyTextBlockStyle}\"",
                "Text=\"Icons per page\"",
                "Margin=\"10,0,0,0\"",
                "VerticalAlignment=\"Center\"",
                "<ComboBox",
                "ItemsSource=\"{Binding ViewModel.PageSizeOptions}\"",
                "SelectedIndex=\"{Binding ViewModel.SelectedPageSizeIndex}\"",
                "AutomationProperties.Name=\"Icons per page\"",
                "Margin=\"10,0,0,0\"");
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
            var normalizedXaml = xaml.Replace("\r\n", "\n").Replace('\r', '\n');
            StringAssert.Contains(
                xaml,
                "xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages.WpfGallery.Navigation\"");
            StringAssert.Contains(
                normalizedXaml,
                "<Grid x:Name=\"ContentPagePane\" Height=\"Auto\">\n\n        <Grid.RowDefinitions>");
            StringAssert.Contains(
                normalizedXaml,
                "</Grid.RowDefinitions>\n        <controls:PageHeader");
            StringAssert.Contains(
                normalizedXaml,
                "<controls:PageHeader Margin=\"0,0,0,32\" Title=\"{Binding ViewModel.PageTitle}\" ShowDescription=\"False\" />\n\n        <ScrollViewer");
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
            var normalizedFrameXaml = frameXaml.Replace("\r\n", "\n").Replace('\r', '\n');
            StringAssert.Contains(
                frameXaml,
                "xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages.WpfGallery.Navigation\"");
            StringAssert.Contains(
                normalizedFrameXaml,
                "        <Grid x:Name=\"ContentPagePane\" Height=\"Auto\">\n            <Grid.RowDefinitions>");
            StringAssert.Contains(
                normalizedFrameXaml,
                "            </Grid.RowDefinitions>\n            <controls:PageHeader");
            StringAssert.Contains(
                normalizedFrameXaml,
                "<controls:PageHeader Margin=\"0,0,0,32\" Title=\"{Binding ViewModel.PageTitle}\" ShowDescription=\"False\" />\n            <ScrollViewer");
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
            var normalizedNavigationWindowXaml = navigationWindowXaml.Replace("\r\n", "\n").Replace('\r', '\n');
            StringAssert.Contains(
                navigationWindowXaml,
                "xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages.WpfGallery.Navigation\"");
            StringAssert.Contains(
                normalizedNavigationWindowXaml,
                "        <Grid x:Name=\"ContentPagePane\" Height=\"Auto\">\n            <Grid.RowDefinitions>");
            StringAssert.Contains(
                normalizedNavigationWindowXaml,
                "            </Grid.RowDefinitions>\n            <controls:PageHeader");
            StringAssert.Contains(
                normalizedNavigationWindowXaml,
                "<controls:PageHeader Margin=\"0,0,0,32\" Title=\"{Binding ViewModel.PageTitle}\" ShowDescription=\"False\" />\n            <ScrollViewer");
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
                "xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages.WpfGallery.Navigation\"",
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
                "xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages.WpfGallery.Navigation\"",
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
                "xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages.WpfGallery.Navigation\"",
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
            var normalizedXaml = xaml.Replace("\r\n", "\n").Replace('\r', '\n');
            StringAssert.Contains(
                xaml,
                "xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages.WpfGallery.Navigation\"");
            StringAssert.Contains(
                normalizedXaml,
                "<Grid x:Name=\"ContentPagePane\" Height=\"Auto\">\n\n        <Grid.RowDefinitions>");
            StringAssert.Contains(
                normalizedXaml,
                "</Grid.RowDefinitions>\n        <controls:PageHeader");
            StringAssert.Contains(
                normalizedXaml,
                "<controls:PageHeader Margin=\"0,0,0,32\" Title=\"{Binding ViewModel.PageTitle}\" ShowDescription=\"False\" />\n\n        <ScrollViewer");
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
                Tuple.Create("LabelPage.xaml", "<controls:PageHeader Margin=\"0,0,0,32\" Title=\"{Binding ViewModel.PageTitle}\" Description=\"{Binding ViewModel.PageDescription}\" />", false),
                Tuple.Create("TextBoxPage.xaml", "<controls:PageHeader Margin=\"0,0,0,32\" Title=\"{Binding ViewModel.PageTitle}\" Description=\"{Binding ViewModel.PageDescription}\" />", false),
                Tuple.Create("PasswordBoxPage.xaml", "<controls:PageHeader Margin=\"0,0,0,32\" Title=\"{Binding ViewModel.PageTitle}\" Description=\"{Binding ViewModel.PageDescription}\" />", false),
                Tuple.Create("RichTextEditPage.xaml", "<controls:PageHeader Margin=\"0,0,0,32\" Title=\"{Binding ViewModel.PageTitle}\" Description=\"{Binding ViewModel.PageDescription}\" />", false),
                Tuple.Create("TextBlockPage.xaml", "<controls:PageHeader Margin=\"0,0,0,32\" Title=\"{Binding ViewModel.PageTitle}\" Description=\"{Binding ViewModel.PageDescription}\" />", false),
                Tuple.Create("HyperlinkPage.xaml", "<controls:PageHeader Margin=\"0,0,0,32\" Title=\"{Binding ViewModel.PageTitle}\" ShowDescription=\"False\" />", true)
            })
            {
                var xaml = ReadRepoFile(
                    "ModernWpf.Gallery",
                    "Pages",
                    "WpfGallery",
                    "Text",
                    page.Item1);
                var normalizedXaml = xaml.Replace("\r\n", "\n").Replace('\r', '\n');
                StringAssert.Contains(
                    xaml,
                    "xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages.WpfGallery.Text\"");
                if (page.Item3)
                {
                    StringAssert.Contains(
                        normalizedXaml,
                        "        <Grid x:Name=\"ContentPagePane\" Height=\"Auto\">\n            <Grid.RowDefinitions>");
                    StringAssert.Contains(
                        normalizedXaml,
                        "            </Grid.RowDefinitions>\n            <controls:PageHeader");
                    StringAssert.Contains(
                        normalizedXaml,
                        page.Item2 + "\n            <ScrollViewer");
                }
                else
                {
                    StringAssert.Contains(
                        normalizedXaml,
                        "<Grid x:Name=\"ContentPagePane\" Height=\"Auto\">\n        <Grid.RowDefinitions>");
                    StringAssert.Contains(
                        normalizedXaml,
                        "</Grid.RowDefinitions>\n        <controls:PageHeader");
                    StringAssert.Contains(
                        normalizedXaml,
                        page.Item2 + "\n\n        <ScrollViewer");
                }

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
            AssertContainsInOrder(
                textBoxXaml,
                "xmlns:d=\"http://schemas.microsoft.com/expression/blend/2008\"",
                "xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages.WpfGallery.Text\"",
                "xmlns:mc=\"http://schemas.openxmlformats.org/markup-compatibility/2006\"",
                "xmlns:helpers=\"clr-namespace:ModernWpf.Gallery.Helpers\"");
            StringAssert.Contains(
                textBoxXaml,
                "<controls:ControlExample Margin=\"10\" HeaderText=\"A simple TextBox.\" XamlCode=\"&lt;TextBox /&gt;\">");
            AssertContainsInOrder(
                textBoxXaml,
                "<controls:ControlExample Margin=\"10,36,10,10\"",
                "HeaderText=\"A TextBox with input validation.\"",
                "XamlCode=\"&lt;TextBox&gt;");

            var passwordBoxXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Text",
                "PasswordBoxPage.xaml");
            AssertContainsInOrder(
                passwordBoxXaml,
                "<controls:ControlExample",
                "Margin=\"10\"",
                "HeaderText=\"A simple PasswordBox.\"",
                "XamlCode=\"&lt;PasswordBox /&gt;\"",
                "<PasswordBox AutomationProperties.Name=\"Simple Password Box\" />");

            var richTextEditXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Text",
                "RichTextEditPage.xaml");
            AssertContainsInOrder(
                richTextEditXaml,
                "<controls:ControlExample",
                "Margin=\"10\"",
                "HeaderText=\"A simple RichTextBox\"",
                "XamlCode=\"&lt;RichTextBox /&gt;\"",
                "<RichTextBox AutomationProperties.Name=\"simple rich text editor\" />");

            var textBlockXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Text",
                "TextBlockPage.xaml");
            AssertContainsInOrder(
                textBlockXaml,
                "<StackPanel Margin=\"0,0,0,24\">",
                "<controls:ControlExample",
                "Margin=\"10\"",
                "HeaderText=\"A simple TextBlock.\"",
                "XamlCode=\"&lt;TextBlock Text=&quot;I am a text block.&quot; /&gt;\"",
                "<TextBlock Text=\"I am a text block.\" />",
                "<controls:ControlExample",
                "Margin=\"10,36,10,10\"",
                "HeaderText=\"A TextBlock with style applied.\"",
                "XamlCode=\"&lt;TextBlock Text=&quot;I am a styled TextBlock.&quot; FontFamily=&quot;Comic Sans MS&quot; FontStyle=&quot;Italic&quot; /&gt;\"",
                "<TextBlock",
                "FontFamily=\"Comic Sans MS\"",
                "FontStyle=\"Italic\"",
                "Text=\"I am a styled TextBlock.\" />",
                "<controls:ControlExample",
                "Margin=\"10,36,10,10\"",
                "HeaderText=\"A TextBlock with inline text elements.\"",
                "XamlCode=\"&lt;TextBlock FontSize=&quot;14&quot;&gt;",
                "<TextBlock FontSize=\"14\">",
                "<Run FontFamily=\"Times New Roman\" Foreground=\"{DynamicResource TextFillColorPrimaryBrush}\" >",
                "Text in a TextBlock doesn't have to be a simple string.",
                "<LineBreak />",
                "<Span>",
                "Text can be <Bold>bold</Bold>,&#x20;",
                "<Italic>italic</Italic>,&#x20;",
                "or <Underline>underlined</Underline>",
                "<controls:ControlExample",
                "Margin=\"10,36,10,10\"",
                "HeaderText=\"A TextBlock with wrap property.\"",
                "XamlCode=\"&lt;TextBlock FontSize=&quot;14&quot; TextWrapping=&quot;Wrap&quot;&gt;",
                "<TextBlock FontSize=\"14\" TextWrapping=\"Wrap\">",
                "The TextBlock control provides flexible text support for WPF applications.",
                "It supports a number of properties that enable precise control of presentation, such as FontFamily, FontSize, FontWeight, TextEffects, and TextWrapping.");

            var hyperlinkXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Text",
                "HyperlinkPage.xaml");
            AssertContainsInOrder(
                hyperlinkXaml,
                "<controls:ControlExample",
                "Margin=\"10\"",
                "HeaderText=\"A Hyperlink\"",
                "XamlCode=\"&lt;TextBlock Margin=&quot;20&quot;&gt;&#10;    &lt;Hyperlink NavigateUri=&quot;https://www.microsoft.com&quot; RequestNavigate=&quot;Hyperlink_RequestNavigate&quot;&gt;&#10;        Lorem Ipsum link&#10;    &lt;/Hyperlink&gt;&#10;&lt;/TextBlock&gt;\"",
                "<TextBlock Margin=\"20\">",
                "<Hyperlink NavigateUri=\"https://www.microsoft.com\" RequestNavigate=\"Hyperlink_RequestNavigate\">",
                "Hyperlink",
                "</Hyperlink>");
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

            var progressBarXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "StatusAndInfo",
                "ProgressBarPage.xaml");
            AssertContainsInOrder(
                progressBarXaml,
                "<StackPanel Margin=\"0,0,0,24\">",
                "<controls:ControlExample",
                "Margin=\"10\"",
                "HeaderText=\"A simple progress bar.\"",
                "XamlCode=\"&lt;ProgressBar Value=&quot;40&quot; /&gt;\"",
                "<ProgressBar",
                "Margin=\"24\"",
                "AutomationProperties.Name=\"A determinate\"",
                "Value=\"40\" />",
                "<controls:ControlExample",
                "Margin=\"10,32,10,10\"",
                "HeaderText=\"An indeterminate progress bar.\"",
                "XamlCode=\"&lt;ProgressBar IsIndeterminate=&quot;True&quot; /&gt;\"",
                "<ProgressBar",
                "Margin=\"24\"",
                "AutomationProperties.Name=\"An indeterminate\"",
                "IsIndeterminate=\"True\" />");

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

            var datePickerXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "DateAndTime",
                "DatePickerPage.xaml");
            AssertContainsInOrder(
                datePickerXaml,
                "<controls:ControlExample Margin=\"10\" HeaderText=\"A basic DatePicker control.\">",
                "<controls:ControlExample.XamlCode>",
                "&lt;DatePicker/&gt;",
                "<DatePicker",
                "MinWidth=\"200\"",
                "HorizontalAlignment=\"Left\"",
                "AutomationProperties.Name=\"Pick a date\" />");

            var canvasXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Media",
                "CanvasPage.xaml");
            AssertContainsInOrder(
                canvasXaml,
                "<Grid Margin=\"0,0,0,24\">",
                "<controls:ControlExample Margin=\"10\" HeaderText=\"A basic Canvas inside the ViewBox\">",
                "<controls:ControlExample.XamlCode>",
                "&lt;Viewbox Width=&quot;200&quot; Height=&quot;200&quot; &gt;\\n",
                "\\t&lt;Canvas Width=&quot;47&quot; Height=&quot;123&quot;&gt;\\n",
                "\\t\\t&lt;Path Data=&quot;M0,19H18V84h29v15H0V19Z&quot; Fill=&quot;White&quot; /&gt;\\n",
                "\\t\\t&lt;Path Data=&quot;M46,80H29V15H0V0H46V80Z&quot; Fill=&quot;White&quot; /&gt;\\n",
                "<Viewbox Width=\"200\" Height=\"200\">",
                "<Canvas Width=\"47\" Height=\"123\">",
                "<Path Data=\"M0,19H18V84h29v15H0V19Z\" Fill=\"{DynamicResource TextFillColorSecondaryBrush}\" />",
                "<Path Data=\"M46,80H29V15H0V0H46V80Z\" Fill=\"{DynamicResource TextFillColorSecondaryBrush}\" />");

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

            var fileDialogsXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "System",
                "FileAndFolderDialogsPage.xaml");
            AssertContainsInOrder(
                fileDialogsXaml,
                "<controls:ControlExample",
                "Margin=\"10\"",
                "HeaderText=\"Pick Single File\"",
                "XamlCode=\"&lt;Button Content=&quot;Pick Single File&quot; Click=&quot;PickSingleFileButton_Click&quot; /&gt;\"",
                "<Button",
                "Content=\"Pick a single file\"",
                "Click=\"PickSingleFileButton_Click\"",
                "Margin=\"0,0,0,10\" />",
                "<TextBlock",
                "Text=\"{Binding ViewModel.SingleFilePath}\"",
                "TextWrapping=\"Wrap\" />",
                "HeaderText=\"Save File\"",
                "<TextBox",
                "Text=\"{Binding ViewModel.FileContent, UpdateSourceTrigger=PropertyChanged}\"",
                "AcceptsReturn=\"True\"",
                "TextWrapping=\"Wrap\"",
                "MinHeight=\"80\"",
                "Margin=\"0,0,0,10\"",
                "VerticalScrollBarVisibility=\"Auto\"",
                "AutomationProperties.Name=\"Save File Text Box\"",
                "AutomationProperties.HelpText=\"The text in the textbox will be saved to a file on button click\" />",
                "<Button",
                "Content=\"Save a file\"",
                "Click=\"SaveFileButton_Click\"",
                "Margin=\"0,0,0,10\" />");

            var messageBoxXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "System",
                "MessageBoxPage.xaml");
            AssertContainsInOrder(
                messageBoxXaml,
                "<controls:ControlExample",
                "HeaderText=\"MessageBox with Different Buttons\"",
                "XamlCode=\"{Binding ViewModel.DifferentButtonsXamlCode}\"",
                "CSharpCode=\"{Binding ViewModel.DifferentButtonsCSharpCode}\"",
                "<Button",
                "Content=\"Show MessageBox\"",
                "AutomationProperties.Name=\"MessageBox with Different Buttons\"",
                "Click=\"ShowButtonFromComboBox_Click\"",
                "Margin=\"0,0,0,10\" />",
                "<TextBlock",
                "Text=\"{Binding ViewModel.DifferentButtonsResult}\"",
                "TextWrapping=\"Wrap\" />",
                "<StackPanel Grid.Column=\"1\" Margin=\"10,0,0,0\">",
                "<TextBlock Text=\"Button Type:\" Margin=\"0,0,0,5\" />",
                "<ComboBox",
                "x:Name=\"ButtonTypeComboBox\"",
                "AutomationProperties.Name=\"MessageBox Button Selector\"",
                "SelectedIndex=\"{Binding ViewModel.SelectedButtonIndex}\"",
                "MinWidth=\"150\">",
                "HeaderText=\"Information, Error, and Warning MessageBox\"",
                "<WrapPanel Margin=\"0,0,0,10\">",
                "<Button Content=\"Information\" Click=\"ShowCommonInformation_Click\" Margin=\"0,0,5,0\" />",
                "<Button Content=\"Error\" Click=\"ShowCommonError_Click\" Margin=\"0,0,5,0\" />",
                "<Button Content=\"Warning\" Click=\"ShowCommonWarning_Click\" />",
                "<TextBlock",
                "Text=\"{Binding ViewModel.CommonMessagesResult}\"",
                "TextWrapping=\"Wrap\" />");

            var clipboardXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "System",
                "ClipboardPage.xaml");
            AssertContainsInOrder(
                clipboardXaml,
                "<Border",
                "Grid.Row=\"1\"",
                "Background=\"{DynamicResource SubtleFillColorSecondaryBrush}\"",
                "BorderBrush=\"{DynamicResource AccentFillColorDefaultBrush}\"",
                "BorderThickness=\"1\"",
                "CornerRadius=\"4\"",
                "Padding=\"16,12\"",
                "Margin=\"0,0,0,16\">",
                "<TextBlock",
                "Grid.Column=\"0\"",
                "FontFamily=\"{StaticResource SymbolThemeFontFamily}\"",
                "FontSize=\"16\"",
                "Text=\"&#xE946;\"",
                "AutomationProperties.Name=\"Info\"",
                "Foreground=\"{DynamicResource AccentFillColorDefaultBrush}\"",
                "VerticalAlignment=\"Top\"",
                "Margin=\"0,2,12,0\" />",
                "HeaderText=\"Copy text to Clipboard\"",
                "<TextBox",
                "x:Name=\"CopyTextBox\"",
                "Text=\"Hello, Clipboard!\"",
                "AutomationProperties.Name=\"Copy To Clipboard TextBox\"",
                "Margin=\"0,0,0,10\"",
                "Width=\"300\"",
                "HorizontalAlignment=\"Left\" />",
                "<Button",
                "Content=\"Copy to Clipboard\"",
                "Click=\"CopyToClipboard_Click\"",
                "Margin=\"0,0,0,10\" />",
                "HeaderText=\"Copy image to Clipboard\"",
                "<Image",
                "x:Name=\"SourceImage\"",
                "Source=\"pack://application:,,,/ModernWpf.Gallery;component/Assets/ControlImages/Clipboard.png\"",
                "Width=\"100\"",
                "Height=\"100\"",
                "HorizontalAlignment=\"Left\"",
                "Margin=\"0,0,0,10\" />",
                "HeaderText=\"Paste image from Clipboard\"",
                "<Border",
                "BorderBrush=\"Gray\"",
                "BorderThickness=\"1\"",
                "Width=\"200\"",
                "Height=\"200\"",
                "HorizontalAlignment=\"Left\">");
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
