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
                Assert.IsTrue(
                    sectionSource.Contains("public partial class " + className + " : SectionPage", StringComparison.Ordinal),
                    className + " should keep the official WPF Gallery partial section page declaration shape.");
            }
        }

        [TestMethod]
        public void TopLevelCodeBehindKeepsOfficialPageBaseDeclarationShape()
        {
            foreach (var page in new[]
            {
                "HomePage",
                "AllControlsPage",
                "WhatsNewPage",
                "SettingsPage"
            })
            {
                var source = ReadRepoFile(
                    "ModernWpf.Gallery",
                    "Pages",
                    page + ".xaml.cs");

                Assert.IsTrue(
                    source.Contains("public partial class " + page + " : Page", StringComparison.Ordinal),
                    page + " should match the official WPF Gallery top-level page base declaration shape.");
            }
        }

        [TestMethod]
        public void CopiedWpfGalleryViewModelClassesStayUnsealedLikeOfficialSource()
        {
            var repoRoot = GetRepoRoot();
            var wpfGalleryViewModels = Directory.EnumerateFiles(
                Path.Combine(repoRoot, "ModernWpf.Gallery", "Pages", "WpfGallery"),
                "*ViewModel*.cs",
                SearchOption.AllDirectories);
            var copiedTopLevelViewModels = new[]
            {
                Path.Combine(repoRoot, "ModernWpf.Gallery", "Pages", "SettingsPage.xaml.cs")
            };

            foreach (var path in wpfGalleryViewModels.Concat(copiedTopLevelViewModels))
            {
                var source = File.ReadAllText(path);
                Assert.IsFalse(
                    source.Contains("public sealed class ", StringComparison.Ordinal),
                    Path.GetRelativePath(repoRoot, path) + " should match the official WPF Gallery unsealed viewmodel class shape.");
            }
        }

        [TestMethod]
        public void CopiedWpfGalleryViewModelClassesKeepOfficialPartialDeclarationShape()
        {
            var repoRoot = GetRepoRoot();
            foreach (var file in new[]
            {
                new
                {
                    RelativePath = Path.Combine("ModernWpf.Gallery", "Pages", "SettingsPage.xaml.cs"),
                    ClassNames = new[] { "SettingsPageViewModel" }
                },
                new
                {
                    RelativePath = Path.Combine("ModernWpf.Gallery", "Pages", "WpfGallery", "WpfGalleryNavigationPageViewModels.cs"),
                    ClassNames = new[]
                    {
                        "DashboardPageViewModel",
                        "WhatsNewPageViewModel",
                        "AllSamplesPageViewModel",
                        "DesignGuidancePageViewModel",
                        "SamplesPageViewModel",
                        "BasicInputPageViewModel",
                        "CollectionsPageViewModel",
                        "DateAndTimePageViewModel",
                        "LayoutPageViewModel",
                        "MediaPageViewModel",
                        "NavigationPageViewModel",
                        "StatusAndInfoPageViewModel",
                        "TextPageViewModel",
                        "SystemPageViewModel"
                    }
                },
                new
                {
                    RelativePath = Path.Combine("ModernWpf.Gallery", "Pages", "WpfGallery", "BasicInput", "BasicInputPageViewModels.cs"),
                    ClassNames = new[] { "ButtonPageViewModel", "CheckBoxPageViewModel", "ComboBoxPageViewModel", "RadioButtonPageViewModel", "SliderPageViewModel" }
                },
                new
                {
                    RelativePath = Path.Combine("ModernWpf.Gallery", "Pages", "WpfGallery", "Collections", "CollectionsPageViewModels.cs"),
                    ClassNames = new[] { "DataGridPageViewModel", "ListBoxPageViewModel", "ListViewPageViewModel", "TreeViewPageViewModel" }
                },
                new
                {
                    RelativePath = Path.Combine("ModernWpf.Gallery", "Pages", "WpfGallery", "DateAndTime", "DateAndTimePageViewModels.cs"),
                    ClassNames = new[] { "CalendarPageViewModel", "DatePickerPageViewModel" }
                },
                new
                {
                    RelativePath = Path.Combine("ModernWpf.Gallery", "Pages", "WpfGallery", "DesignGuidance", "DesignGuidancePageViewModels.cs"),
                    ClassNames = new[] { "ColorsPageViewModel", "TypographyPageViewModel", "SpacingPageViewModel", "GeometryPageViewModel" }
                },
                new
                {
                    RelativePath = Path.Combine("ModernWpf.Gallery", "Pages", "WpfGallery", "DesignGuidance", "IconographyPageViewModel.cs"),
                    ClassNames = new[] { "IconographyPageViewModel" }
                },
                new
                {
                    RelativePath = Path.Combine("ModernWpf.Gallery", "Pages", "WpfGallery", "Layout", "LayoutPageViewModels.cs"),
                    ClassNames = new[] { "BorderPageViewModel", "ExpanderPageViewModel", "GridPageViewModel", "GridSplitterPageViewModel", "GroupBoxPageViewModel", "ResizeGripPageViewModel", "StackPanelPageViewModel" }
                },
                new
                {
                    RelativePath = Path.Combine("ModernWpf.Gallery", "Pages", "WpfGallery", "Media", "MediaPageViewModels.cs"),
                    ClassNames = new[] { "CanvasPageViewModel", "ImagePageViewModel" }
                },
                new
                {
                    RelativePath = Path.Combine("ModernWpf.Gallery", "Pages", "WpfGallery", "Navigation", "NavigationPageViewModels.cs"),
                    ClassNames = new[] { "MenuPageViewModel", "TabControlPageViewModel", "FramePageViewModel", "NavigationWindowPageViewModel" }
                },
                new
                {
                    RelativePath = Path.Combine("ModernWpf.Gallery", "Pages", "WpfGallery", "Samples", "UserDashboardPageViewModel.cs"),
                    ClassNames = new[] { "UserDashboardPageViewModel" }
                },
                new
                {
                    RelativePath = Path.Combine("ModernWpf.Gallery", "Pages", "WpfGallery", "StatusAndInfo", "StatusAndInfoPageViewModels.cs"),
                    ClassNames = new[] { "ProgressBarPageViewModel", "ToolTipPageViewModel" }
                },
                new
                {
                    RelativePath = Path.Combine("ModernWpf.Gallery", "Pages", "WpfGallery", "System", "SystemPageViewModels.cs"),
                    ClassNames = new[] { "FileAndFolderDialogsPageViewModel", "MessageBoxPageViewModel", "ClipboardPageViewModel" }
                },
                new
                {
                    RelativePath = Path.Combine("ModernWpf.Gallery", "Pages", "WpfGallery", "Text", "TextPageViewModels.cs"),
                    ClassNames = new[] { "LabelPageViewModel", "TextBoxPageViewModel", "TextBlockPageViewModel", "HyperlinkPageViewModel", "RichTextEditPageViewModel", "PasswordBoxPageViewModel" }
                }
            })
            {
                var source = File.ReadAllText(Path.Combine(repoRoot, file.RelativePath));
                foreach (var className in file.ClassNames)
                {
                    Assert.IsTrue(
                        source.Contains("public partial class " + className, StringComparison.Ordinal),
                        className + " should match the official WPF Gallery partial viewmodel declaration shape.");
                }
            }
        }

        [TestMethod]
        public void CopiedWpfGalleryModelClassesStayUnsealedLikeOfficialSource()
        {
            var repoRoot = GetRepoRoot();
            foreach (var file in new[]
            {
                new
                {
                    RelativePath = Path.Combine("ModernWpf.Gallery", "Models", "Product.cs"),
                    SealedDeclaration = "public sealed class Product",
                    UnsealedDeclaration = "public class Product"
                },
                new
                {
                    RelativePath = Path.Combine("ModernWpf.Gallery", "Models", "Person.cs"),
                    SealedDeclaration = "public sealed class Person",
                    UnsealedDeclaration = "public class Person"
                },
                new
                {
                    RelativePath = Path.Combine("ModernWpf.Gallery", "Pages", "WpfGallery", "DesignGuidance", "IconData.cs"),
                    SealedDeclaration = "public sealed class IconData",
                    UnsealedDeclaration = "public class IconData"
                },
                new
                {
                    RelativePath = Path.Combine("ModernWpf.Gallery", "Pages", "WpfGallery", "Samples", "UserDashboardUser.cs"),
                    SealedDeclaration = "public sealed class UserDashboardUser",
                    UnsealedDeclaration = "public class UserDashboardUser : INotifyPropertyChanged"
                }
            })
            {
                var path = Path.Combine(repoRoot, file.RelativePath);
                var source = File.ReadAllText(path);
                Assert.IsFalse(
                    source.Contains(file.SealedDeclaration, StringComparison.Ordinal),
                    Path.GetRelativePath(repoRoot, path) + " should match the official WPF Gallery unsealed model declaration shape.");
                Assert.IsTrue(
                    source.Contains(file.UnsealedDeclaration, StringComparison.Ordinal),
                    Path.GetRelativePath(repoRoot, path) + " should keep the copied WPF Gallery model declaration shape.");
            }
        }

        [TestMethod]
        public void TopLevelCodeBehindKeepsOfficialViewModelMemberOrderShape()
        {
            foreach (var page in new[]
            {
                Tuple.Create("AllControlsPage", "AllSamplesPageViewModel"),
                Tuple.Create("WhatsNewPage", "WhatsNewPageViewModel"),
                Tuple.Create("SettingsPage", "SettingsPageViewModel")
            })
            {
                var source = ReadRepoFile(
                    "ModernWpf.Gallery",
                    "Pages",
                    page.Item1 + ".xaml.cs");
                var viewModelIndex = source.IndexOf(
                    "public " + page.Item2 + " ViewModel { get; }",
                    StringComparison.Ordinal);
                var constructorIndex = source.IndexOf(
                    "public " + page.Item1 + "(",
                    StringComparison.Ordinal);

                Assert.IsTrue(viewModelIndex >= 0, page.Item1 + " should expose its copied page-specific ViewModel property.");
                Assert.IsTrue(constructorIndex >= 0, page.Item1 + " should keep its copied view-model constructor.");
                Assert.IsTrue(
                    viewModelIndex < constructorIndex,
                    page.Item1 + " should match the official WPF Gallery top-level code-behind member order by declaring ViewModel before the copied constructor.");
            }

            var homeSource = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "HomePage.xaml.cs");
            AssertContainsInOrder(
                homeSource,
                "public HomePage(DashboardPageViewModel viewModel)",
                "public DashboardPageViewModel ViewModel { get; }");
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
                "x:Name=\"Navigation\"",
                "AutomationProperties.Name=\"Navigation Pane\"",
                "IsBackButtonVisible=\"Collapsed\"",
                "IsPaneToggleButtonVisible=\"False\"",
                "IsSettingsVisible=\"False\"",
                "OpenPaneLength=\"258\"",
                "PaneDisplayMode=\"Left\"");
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
            AssertContainsInOrder(
                navigationRootXaml,
                "x:Name=\"ContentFrameBorder\"",
                "Margin=\"4,0,0,0\"",
                "Padding=\"24,16,24,0\"",
                "Background=\"{DynamicResource LayerFillColorDefaultBrush}\"",
                "BorderBrush=\"{DynamicResource CardStrokeColorDefaultBrush}\"",
                "BorderThickness=\"1\"",
                "CornerRadius=\"8,0,0,0\"");
            Assert.IsFalse(
                navigationRootXaml.Contains("one pixel narrower", StringComparison.Ordinal),
                "The retained NavigationView pane should use the official 258px left shell width instead of a local one-pixel compensation comment.");

            var mainWindowCode = ReadRepoFile(
                "ModernWpf.Gallery",
                "MainWindow.xaml.cs");

            AssertContainsInOrder(
                mainWindowCode,
                "SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;",
                "StateChanged += OnWindowStateChanged;",
                "Activated += OnWindowActivationChanged;",
                "Deactivated += OnWindowActivationChanged;",
                "private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)",
                "Dispatcher.Invoke(() =>",
                "UpdateMainWindowVisuals();",
                "SystemEvents.UserPreferenceChanged -= SystemEvents_UserPreferenceChanged;",
                "MainGrid.Margin = GetMainGridMargin(WindowState, SystemParameters.HighContrast);",
                "UpdateTitleBarButtonsVisibility();",
                "if (SystemParameters.HighContrast)",
                "HighContrastBorder.SetResourceReference(",
                "System.Windows.Controls.Border.BorderBrushProperty,",
                "IsActive ? SystemColors.ActiveCaptionBrushKey : SystemColors.InactiveCaptionBrushKey);",
                "HighContrastBorder.BorderThickness = GetHighContrastBorderThickness(SystemParameters.HighContrast);",
                "chrome.NonClientFrameEdges = GetPrefferedNonClientFrameEdges();",
                "NonClientFrameEdges = GetPrefferedNonClientFrameEdges()",
                "return isHighContrast ? new Thickness(8, 1, 8, 8) : new Thickness(0);",
                "internal static NonClientFrameEdges GetPrefferedNonClientFrameEdges()",
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
        public void SharedControlExampleKeepsOfficialSourceCodeTemplateShape()
        {
            var xaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Controls",
                "ControlExample.xaml");
            var normalizedXaml = xaml.Replace("\r\n", "\n").Replace('\r', '\n');

            StringAssert.Contains(
                xaml,
                "<controls:NullToVisibilityConverter x:Key=\"NullToVisibilityConverter\" />");
            StringAssert.Contains(
                normalizedXaml,
                "Visibility=\"{TemplateBinding HeaderText,\n                            Converter={StaticResource NullToVisibilityConverter}}\" />");
            AssertContainsInOrder(
                xaml,
                "<Border",
                "Grid.Row=\"1\"",
                "Padding=\"16\"",
                "Background=\"{DynamicResource SolidBackgroundFillColorBaseBrush}\"",
                "BorderBrush=\"{DynamicResource CardStrokeColorDefaultBrush}\"",
                "BorderThickness=\"1,1,1,0\"",
                "CornerRadius=\"8,8,0,0\"",
                "TextElement.FontSize=\"{StaticResource BodyTextBlockFontSize}\">");
            AssertContainsInOrder(
                xaml,
                "<Expander",
                "Grid.Row=\"2\"",
                "AutomationProperties.Name=\"{Binding HeaderText, RelativeSource={RelativeSource TemplatedParent}, StringFormat=View Source Code for {0}}\"",
                "Header=\"Source code\"",
                "MinHeight=\"42\">",
                "<MultiDataTrigger>",
                "<Condition Binding=\"{Binding XamlCode, RelativeSource={RelativeSource TemplatedParent}}\" Value=\"{x:Null}\" />",
                "<Condition Binding=\"{Binding CSharpCode, RelativeSource={RelativeSource TemplatedParent}}\" Value=\"{x:Null}\" />",
                "<Setter Property=\"Visibility\" Value=\"Collapsed\" />");
            StringAssert.Contains(
                xaml,
                "<Button Grid.Column=\"1\" Padding=\"8\" Command=\"ApplicationCommands.Copy\" CommandParameter=\"Copy_XamlCode\" ToolTipService.ToolTip=\"Copy to clipboard\" AutomationProperties.Name=\"Copy XAML Code\" >");
            StringAssert.Contains(
                xaml,
                "<TextBlock x:Name=\"CopyGlyphTextBlock\" FontFamily=\"{StaticResource SymbolThemeFontFamily}\" Text=\"&#xE8C8;\"/>");
            StringAssert.Contains(
                xaml,
                "<TextBlock x:Name=\"SuccessGlyphTextBlock\" FontFamily=\"{StaticResource SymbolThemeFontFamily}\" Text=\"&#xE73E;\" Opacity=\"0\" />");
            AssertContainsInOrder(
                xaml,
                "<EventTrigger RoutedEvent=\"Button.Click\">",
                "<EventTrigger.Actions>",
                "<Storyboard BeginTime=\"00:00:00\">",
                "<DoubleAnimation Duration=\"0:0:0.333\" Storyboard.TargetName=\"CopyGlyphTextBlock\" Storyboard.TargetProperty=\"Opacity\" To=\"0\" />",
                "<DoubleAnimation Duration=\"0:0:0.666\" BeginTime=\"0:0:0.333\" Storyboard.TargetName=\"SuccessGlyphTextBlock\" Storyboard.TargetProperty=\"Opacity\" To=\"1\" />",
                "<DoubleAnimation Storyboard.TargetName=\"SuccessGlyphTextBlock\" BeginTime=\"0:0:2\" Duration=\"0:0:0.01\" Storyboard.TargetProperty=\"Opacity\" To=\"0\" />",
                "<DoubleAnimation Storyboard.TargetName=\"CopyGlyphTextBlock\" BeginTime=\"0:0:2.1\" Duration=\"0:0:0.01\" Storyboard.TargetProperty=\"Opacity\" To=\"1\" />",
                "</EventTrigger.Actions>");
            AssertContainsInOrder(
                xaml,
                "<TextBox",
                "Style=\"{StaticResource SelectionTextBox}\"",
                "AutomationProperties.Name=\"XAML Source Code\"",
                "Text=\"{TemplateBinding XamlCode}\"/>",
                "<Border",
                "x:Name=\"Border\"",
                "Margin=\"0,20\"",
                "BorderThickness=\"1\"",
                "Visibility=\"Visible\" />");
            AssertContainsInOrder(
                xaml,
                "<StackPanel x:Name=\"CSharpCodeBlock\">",
                "<TextBlock",
                "Margin=\"0,0,0,5\"",
                "Style=\"{StaticResource BodyStrongTextBlockStyle}\"",
                "Foreground=\"{DynamicResource TextFillColorPrimaryBrush}\"",
                "Text=\"C#\" />");
            StringAssert.Contains(
                xaml,
                "<Button Grid.Column=\"1\" Padding=\"8\" Command=\"ApplicationCommands.Copy\" CommandParameter=\"Copy_CSharpCode\" FocusManager.IsFocusScope=\"True\" >");
            AssertContainsInOrder(
                xaml,
                "<TextBox",
                "Style=\"{StaticResource SelectionTextBox}\"",
                "AutomationProperties.Name=\"CSharp Source Code\"",
                "Text=\"{TemplateBinding CSharpCode}\" />");
        }

        [TestMethod]
        public void SharedSupportControlCodeBehindKeepsOfficialDependencyPropertyMemberOrderShape()
        {
            var controlExampleSource = ReadRepoFile(
                "ModernWpf.Gallery",
                "Controls",
                "ControlExample.cs");
            var normalizedControlExampleSource = controlExampleSource.Replace("\r\n", "\n").Replace('\r', '\n');
            StringAssert.Contains(
                normalizedControlExampleSource,
                "public static readonly DependencyProperty HeaderTextProperty = DependencyProperty.Register(\n" +
                "            nameof(HeaderText),\n" +
                "            typeof(string),\n" +
                "            typeof(ControlExample),\n" +
                "            new PropertyMetadata(null)\n" +
                "        );");
            StringAssert.Contains(
                normalizedControlExampleSource,
                "public static readonly DependencyProperty XamlCodeSourceProperty = DependencyProperty.Register(\n" +
                "            nameof(XamlCodeSource),\n" +
                "            typeof(Uri),\n" +
                "            typeof(ControlExample),\n" +
                "            new PropertyMetadata(\n" +
                "                null,\n" +
                "                static (o, args) => ((ControlExample)o).OnXamlCodeSourceChanged((Uri)args.NewValue)\n" +
                "            )\n" +
                "        );");
            AssertContainsInOrder(
                controlExampleSource,
                "CommandManager.RegisterClassCommandBinding(typeof(ControlExample), new CommandBinding(ApplicationCommands.Copy, Copy_SourceCode));",
                "public static readonly DependencyProperty HeaderTextProperty = DependencyProperty.Register(",
                "public static readonly DependencyProperty ExampleContentProperty = DependencyProperty.Register(",
                "public static readonly DependencyProperty XamlCodeProperty = DependencyProperty.Register(",
                "public static readonly DependencyProperty XamlCodeSourceProperty = DependencyProperty.Register(",
                "static (o, args) => ((ControlExample)o).OnXamlCodeSourceChanged((Uri)args.NewValue)",
                "public static readonly DependencyProperty CSharpCodeProperty = DependencyProperty.Register(",
                "public static readonly DependencyProperty CSharpCodeSourceProperty = DependencyProperty.Register(",
                "static (o, args) => ((ControlExample)o).OnCSharpCodeSourceChanged((Uri)args.NewValue)",
                "public string HeaderText",
                "public object ExampleContent",
                "public string XamlCode",
                "public Uri XamlCodeSource",
                "public string CSharpCode",
                "public Uri CSharpCodeSource",
                "private void OnXamlCodeSourceChanged(Uri uri)",
                "XamlCode = LoadResource(uri);",
                "private void OnCSharpCodeSourceChanged(Uri uri)",
                "CSharpCode = LoadResource(uri);",
                "private static void Copy_SourceCode(object sender, RoutedEventArgs e)",
                "if (sender is ControlExample controlExample)",
                "var executedArgs = (ExecutedRoutedEventArgs)e;",
                "switch (executedArgs.Parameter.ToString())",
                "case \"Copy_XamlCode\":",
                "Clipboard.SetText(controlExample.XamlCode);",
                "RaiseCopyNotification(executedArgs);",
                "case \"Copy_CSharpCode\":",
                "Clipboard.SetText(controlExample.CSharpCode);");

            var pageHeaderSource = ReadRepoFile(
                "ModernWpf.Gallery",
                "Controls",
                "PageHeader.cs");
            var normalizedPageHeaderSource = pageHeaderSource.Replace("\r\n", "\n").Replace('\r', '\n');
            StringAssert.Contains(
                normalizedPageHeaderSource,
                "public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(\n" +
                "            nameof(Title),\n" +
                "            typeof(string),\n" +
                "            typeof(PageHeader),\n" +
                "            new PropertyMetadata(null)\n" +
                "        );");
            AssertContainsInOrder(
                pageHeaderSource,
                "public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(",
                "public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(",
                "public static readonly DependencyProperty ShowDescriptionProperty = DependencyProperty.Register(",
                "public string Title",
                "public string Description",
                "public bool ShowDescription");
        }

        [TestMethod]
        public void SharedPageHeaderKeepsOfficialTemplateSourceShape()
        {
            var xaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Controls",
                "PageHeader.xaml");
            var normalizedXaml = xaml.Replace("\r\n", "\n").Replace('\r', '\n');

            StringAssert.Contains(
                xaml,
                "<controls:NullToVisibilityConverter x:Key=\"NullToVisibilityConverter\"/>");
            StringAssert.Contains(
                xaml,
                "<Setter Property=\"Focusable\" Value=\"False\"/>");
            StringAssert.Contains(
                normalizedXaml,
                "<StackPanel\n                        VerticalAlignment=\"Center\">");
            AssertContainsInOrder(
                xaml,
                "<Label",
                "x:Name=\"TitleTextBlock\"",
                "AutomationProperties.Name=\"{Binding Title, StringFormat='{}{0} Page', RelativeSource={RelativeSource Mode=TemplatedParent}}\"",
                "pages:GalleryAutomation.HeadingLevel=\"Level1\"",
                "KeyboardNavigation.IsTabStop=\"True\"",
                "KeyboardNavigation.TabIndex=\"0\"",
                "Focusable=\"True\"",
                "<TextBlock Style=\"{StaticResource TitleTextBlockStyle}\"",
                "Text=\"{TemplateBinding Title}\" />");
            AssertContainsInOrder(
                xaml,
                "<Label",
                "pages:GalleryAutomation.HeadingLevel=\"Level2\"",
                "KeyboardNavigation.IsTabStop=\"True\"",
                "KeyboardNavigation.TabIndex=\"1\"",
                "Visibility=\"{TemplateBinding Description, Converter={StaticResource NullToVisibilityConverter}}\"",
                "Focusable=\"True\"",
                "Style=\"{StaticResource BodyTextBlockStyle}\"/>");
            AssertContainsInOrder(
                xaml,
                "<Trigger Property=\"ShowDescription\" Value=\"False\">",
                "<Setter TargetName=\"DescriptionTextBlock\"",
                "Property=\"Visibility\"",
                "Value=\"Hidden\"/>");
        }

        [TestMethod]
        public void SharedHeaderTileKeepsOfficialDeclarationSourceShape()
        {
            var xaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Controls",
                "HeaderTile.xaml");
            var normalizedXaml = xaml.Replace("\r\n", "\n").Replace('\r', '\n');

            StringAssert.Contains(
                xaml,
                "<UserControl x:Class=\"ModernWpf.Gallery.Controls.HeaderTile\"");
            StringAssert.Contains(
                xaml,
                "Width=\"198\" Height=\"220\"");
            Assert.IsFalse(
                xaml.Contains("d:DesignHeight", StringComparison.Ordinal),
                "The shared HeaderTile should keep the official root size declaration without local design-time dimensions.");
            Assert.IsFalse(
                xaml.Contains("d:DesignWidth", StringComparison.Ordinal),
                "The shared HeaderTile should keep the official root size declaration without local design-time dimensions.");
            AssertContainsInOrder(
                xaml,
                "<Button",
                "x:Name=\"RootButton\"",
                "Margin=\"6\"",
                "BorderThickness=\"1\"",
                "HorizontalAlignment=\"Stretch\"",
                "VerticalAlignment=\"Stretch\"",
                "HorizontalContentAlignment=\"Stretch\"",
                "VerticalContentAlignment=\"Stretch\"",
                "AutomationProperties.Name=\"{Binding Title, RelativeSource={RelativeSource AncestorType=local:HeaderTile}}\"",
                "Click=\"RootButton_Click\"",
                "Padding=\"24\">");
            StringAssert.Contains(
                xaml,
                "<SolidColorBrush x:Key=\"ButtonBackground\" Color=\"{Binding Color, Source={StaticResource AcrylicBackgroundFillColorDefaultBrush}}\" Opacity=\"0.8\" />");
            StringAssert.Contains(
                xaml,
                "<SolidColorBrush x:Key=\"ButtonBackgroundPointerOver\" Color=\"{Binding Color, Source={StaticResource AcrylicBackgroundFillColorDefaultBrush}}\" Opacity=\"0.9\" />");
            StringAssert.Contains(
                xaml,
                "<SolidColorBrush x:Key=\"ButtonBackgroundPressed\" Color=\"{Binding Color, Source={StaticResource AcrylicBackgroundFillColorDefaultBrush}}\" Opacity=\"1.0\" />");
            StringAssert.Contains(
                normalizedXaml,
                "<Grid x:Name=\"ContentGrid\"\n            HorizontalAlignment=\"Stretch\"");
            AssertContainsInOrder(
                xaml,
                "<TextBlock",
                "Grid.RowSpan=\"3\"",
                "Margin=\"-12\"",
                "HorizontalAlignment=\"Right\"",
                "VerticalAlignment=\"Bottom\"",
                "FontSize=\"16\"",
                "FontFamily=\"{StaticResource SymbolThemeFontFamily}\"",
                "Foreground=\"{DynamicResource TextFillColorPrimaryBrush}\"",
                "Text=\"&#xE8A7;\" />");
            AssertContainsInOrder(
                xaml,
                "<StackPanel",
                "Grid.Row=\"1\"",
                "Orientation=\"Vertical\"",
                "Margin=\"0 16 0 0\">",
                "<TextBlock",
                "x:Name=\"TitleText\"",
                "FontSize=\"18\"",
                "Foreground=\"{DynamicResource TextFillColorPrimaryBrush}\"",
                "Style=\"{StaticResource BodyTextBlockStyle}\"",
                "Text=\"{Binding Title, RelativeSource={RelativeSource AncestorType=local:HeaderTile}}\"",
                "Margin=\"0 0 0 8\"/>",
                "<TextBlock",
                "Foreground=\"{DynamicResource TextFillColorPrimaryBrush}\"",
                "TextTrimming=\"CharacterEllipsis\"",
                "Style=\"{StaticResource CaptionTextBlockStyle}\"",
                "Text=\"{Binding Description, RelativeSource={RelativeSource AncestorType=local:HeaderTile}}\" />");

            var code = ReadRepoFile(
                "ModernWpf.Gallery",
                "Controls",
                "HeaderTile.xaml.cs");
            AssertContainsInOrder(
                code,
                "ApplyButtonResources(SystemParameters.HighContrast);",
                "if (!highContrast)",
                "RootButton.Resources[\"ButtonBackground\"] = new SolidColorBrush { Color = color, Opacity = 0.8 };",
                "RootButton.Resources[\"ButtonBackgroundPointerOver\"] = new SolidColorBrush { Color = color, Opacity = 0.9 };",
                "RootButton.Resources[\"ButtonBackgroundPressed\"] = new SolidColorBrush { Color = color, Opacity = 1.0 };",
                "RootButton.Resources[\"ButtonBackground\"] = SystemColors.ControlBrush;",
                "RootButton.Resources[\"ButtonBackgroundPointerOver\"] = SystemColors.ControlBrush;",
                "RootButton.Resources[\"ButtonBackgroundPressed\"] = SystemColors.ControlBrush;");
        }

        [TestMethod]
        public void SharedHeaderTileCodeBehindKeepsOfficialMemberAndUserPreferenceHandlerShape()
        {
            var source = ReadRepoFile(
                "ModernWpf.Gallery",
                "Controls",
                "HeaderTile.xaml.cs");

            Assert.IsFalse(
                source.Contains("OnUserPreferenceChanged", StringComparison.Ordinal),
                "HeaderTile should keep the official SystemEvents_UserPreferenceChanged handler name.");
            AssertContainsInOrder(
                source,
                "InitializeComponent();",
                "UpdateButtonResources();",
                "SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;",
                "ThemeManager.Current.ActualApplicationThemeChanged += OnActualApplicationThemeChanged;",
                "Unloaded += OnUnloaded;",
                "private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)",
                "Dispatcher.Invoke(() =>\n            {\n                UpdateButtonResources();\n            });",
                "private void OnActualApplicationThemeChanged(ThemeManager sender, object args)",
                "Dispatcher.Invoke(() =>\n            {\n                UpdateButtonResources();\n            });",
                "private void OnUnloaded(object sender, RoutedEventArgs e)",
                "SystemEvents.UserPreferenceChanged -= SystemEvents_UserPreferenceChanged;",
                "ThemeManager.Current.ActualApplicationThemeChanged -= OnActualApplicationThemeChanged;",
                "private void UpdateButtonResources()",
                "ApplyButtonResources(SystemParameters.HighContrast);",
                "public string Title",
                "public static readonly DependencyProperty TitleProperty",
                "DependencyProperty.Register(\"Title\", typeof(string), typeof(HeaderTile), new PropertyMetadata(\"\"));",
                "public string Description",
                "public static readonly DependencyProperty DescriptionProperty",
                "DependencyProperty.Register(\"ColorExplanation\", typeof(string), typeof(HeaderTile), new PropertyMetadata(\"\"));",
                "public string Link",
                "public static readonly DependencyProperty LinkProperty",
                "DependencyProperty.Register(\"Link\", typeof(string), typeof(HeaderTile), new PropertyMetadata(null));",
                "public object Source",
                "get { return (object)GetValue(SourceProperty); }",
                "public static readonly DependencyProperty SourceProperty",
                "DependencyProperty.Register(\"Source\", typeof(object), typeof(HeaderTile), new PropertyMetadata(null));",
                "private void RootButton_Click(object sender, RoutedEventArgs e)",
                "Process.Start(new ProcessStartInfo(Link) { UseShellExecute = true });",
                "protected override AutomationPeer OnCreateAutomationPeer()");
        }

        [TestMethod]
        public void SharedTileGalleryKeepsOfficialDeclarationSourceShape()
        {
            var xaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Controls",
                "TileGallery.xaml");

            AssertContainsInOrder(
                xaml,
                "<UserControl x:Class=\"ModernWpf.Gallery.Controls.TileGallery\"",
                "xmlns:mc=\"http://schemas.openxmlformats.org/markup-compatibility/2006\"",
                "xmlns:d=\"http://schemas.microsoft.com/expression/blend/2008\"",
                "xmlns:local=\"clr-namespace:ModernWpf.Gallery.Controls\"",
                "mc:Ignorable=\"d\"",
                "d:DesignHeight=\"450\" d:DesignWidth=\"800\">",
                "<Style x:Key=\"TileGalleryScrollButtonStyle\" BasedOn=\"{StaticResource DefaultButtonStyle}\" TargetType=\"Button\">");
            AssertContainsInOrder(
                xaml,
                "<ScrollViewer x:Name=\"RootScrollViewer\"",
                "VerticalScrollBarVisibility=\"Disabled\"",
                "HorizontalScrollBarVisibility=\"Hidden\"",
                "SizeChanged=\"RootScrollViewer_SizeChanged\">",
                "<StackPanel x:Name=\"TilesPanel\"",
                "Orientation=\"Horizontal\">");
            AssertContainsInOrder(
                xaml,
                "<local:HeaderTile",
                "Title=\"Getting started\"",
                "Description=\"An overview of app development options, tools, and samples.\"",
                "Link=\"https://learn.microsoft.com/windows/apps/get-started/\"",
                "Margin=\"24 0 6 0\">",
                "Source=\"pack://application:,,,/ModernWpf.Gallery;component/Assets/AppIcons/WPFGallery_48px.png\"");
            AssertContainsInOrder(
                xaml,
                "Title=\"Windows design\"",
                "Description=\"Design guidelines and toolkits for creating native app experiences.\"",
                "Link=\"https://learn.microsoft.com/windows/apps/design/\">",
                "Source=\"pack://application:,,,/ModernWpf.Gallery;component/Assets/HomeHeaderTiles/Header-WindowsDesign.png\"");
            AssertContainsInOrder(
                xaml,
                "Title=\"WPF GitHub\"",
                "Description=\"A robust UI framework for your desktop applications.\"",
                "Link=\"https://github.com/dotnet/wpf\">",
                "<Viewbox Height=\"52\" Margin=\"-20 0 0 0\">",
                "<Path Data=\"{StaticResource GitHubIconGeometry}\" Fill=\"{DynamicResource TextFillColorPrimaryBrush}\"/>",
                "Title=\"Code samples\"",
                "Description=\"Find WPF samples that demonstrate specific tasks, features, and APIs.\"",
                "Link=\"https://github.com/microsoft/WPF-Samples\">",
                "<Viewbox Height=\"52\" Margin=\"-20 0 0 0\">",
                "<Path Data=\"{StaticResource GitHubIconGeometry}\" Fill=\"{DynamicResource TextFillColorPrimaryBrush}\"/>",
                "Title=\"Partner Center\"",
                "Description=\"Upload your app to the Store.\"",
                "Link=\"https://developer.microsoft.com/windows/\">",
                "Source=\"pack://application:,,,/ModernWpf.Gallery;component/Assets/HomeHeaderTiles/Header-Store.dark.png\"");
            AssertContainsInOrder(
                xaml,
                "<Button x:Name=\"ScrollBackButton\"",
                "Style=\"{DynamicResource TileGalleryScrollButtonStyle}\"",
                "Margin=\"8,-16,0,0\"",
                "AutomationProperties.Name=\"Scroll left\"",
                "Click=\"ScrollBackButton_Click\"",
                "ToolTip=\"Scroll left\">",
                "<TextBlock FontFamily=\"{StaticResource SymbolThemeFontFamily}\" FontSize=\"8\" Text=\"&#xEDD9;\" />",
                "<Button x:Name=\"ScrollForwardButton\"",
                "Style=\"{DynamicResource TileGalleryScrollButtonStyle}\"",
                "Margin=\"0,-16,8,0\"",
                "HorizontalAlignment=\"Right\"",
                "AutomationProperties.Name=\"Scroll right\"",
                "Click=\"ScrollForwardButton_Click\"",
                "ToolTip=\"Scroll right\">",
                "<TextBlock FontFamily=\"{StaticResource SymbolThemeFontFamily}\" FontSize=\"8\" Text=\"&#xEDDA;\" />");
        }

        [TestMethod]
        public void SharedTileGalleryCodeBehindKeepsOfficialScrollHandlerSourceShape()
        {
            var source = ReadRepoFile(
                "ModernWpf.Gallery",
                "Controls",
                "TileGallery.xaml.cs");

            AssertContainsInOrder(
                source,
                "public partial class TileGallery : UserControl",
                "public TileGallery()",
                "InitializeComponent();",
                "private void ScrollBackButton_Click(object sender, RoutedEventArgs e)",
                "double newOffSet = RootScrollViewer.HorizontalOffset - 210;",
                "RootScrollViewer.ScrollToHorizontalOffset(newOffSet);",
                "UpdateScrollButtonsVisibility(newOffSet);",
                "private void ScrollForwardButton_Click(object sender, RoutedEventArgs e)",
                "double newOffSet = RootScrollViewer.HorizontalOffset + 210;",
                "RootScrollViewer.ScrollToHorizontalOffset(newOffSet);",
                "UpdateScrollButtonsVisibility(newOffSet);",
                "private void UpdateScrollButtonsVisibility()",
                "double offset = RootScrollViewer.HorizontalOffset;",
                "UpdateScrollButtonsVisibility(offset);",
                "private void UpdateScrollButtonsVisibility(double newOffset)",
                "ScrollBackButton.Visibility = Visibility.Visible;",
                "ScrollForwardButton.Visibility = Visibility.Visible;",
                "if (RootScrollViewer.ActualWidth < TilesPanel.ActualWidth)",
                "if (newOffset <= 0)",
                "ScrollBackButton.Visibility = Visibility.Collapsed;",
                "else if (newOffset >= RootScrollViewer.ScrollableWidth)",
                "ScrollForwardButton.Visibility = Visibility.Collapsed;",
                "private void RootScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)",
                "UpdateScrollButtonsVisibility();");
        }

        [TestMethod]
        public void SharedColorPageExampleKeepsOfficialTemplateSourceShape()
        {
            var xaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Controls",
                "ColorPageExample.xaml");
            var normalizedXaml = xaml.Replace("\r\n", "\n").Replace('\r', '\n');

            StringAssert.Contains(
                xaml,
                "<Setter Property=\"Background\" Value=\"{DynamicResource SolidBackgroundFillColorBaseBrush}\"/>");
            StringAssert.Contains(
                xaml,
                "<Border BorderThickness=\"1\" Margin=\"0,36,0,0\" Padding=\"12\" CornerRadius=\"8\" BorderBrush=\"{DynamicResource CardStrokeColorDefaultBrush}\" Background=\"{TemplateBinding Background}\">");
            StringAssert.Contains(
                normalizedXaml,
                "</Grid.RowDefinitions>\n\n                            <TextBlock Margin=\"0,0,0,12\" Style=\"{DynamicResource SubtitleTextBlockStyle}\" Text=\"{TemplateBinding Title}\" />");
            StringAssert.Contains(
                xaml,
                "<TextBlock Style=\"{DynamicResource CaptionTextBlockStyle}\" Text=\"{TemplateBinding Description}\" Grid.Row=\"1\"/>");

            var code = ReadRepoFile(
                "ModernWpf.Gallery",
                "Controls",
                "ColorPageExample.cs");
            AssertContainsInOrder(
                code,
                "[ContentProperty(nameof(ExampleContent))]",
                "public partial class ColorPageExample : UserControl",
                "public string Description",
                "public static readonly DependencyProperty DescriptionProperty",
                "DependencyProperty.Register(\"Description\", typeof(string), typeof(ColorPageExample), new PropertyMetadata(\"\"));",
                "public string Title",
                "public static readonly DependencyProperty TitleProperty",
                "DependencyProperty.Register(\"Title\", typeof(string), typeof(ColorPageExample), new PropertyMetadata(\"\"));",
                "public UIElement ExampleContent",
                "public static readonly DependencyProperty ExampleContentProperty",
                "DependencyProperty.Register(\"ExampleContent\", typeof(UIElement), typeof(ColorPageExample), new PropertyMetadata(null));");
        }

        [TestMethod]
        public void SharedColorTileTemplateKeepsOfficialDeclarationSourceShape()
        {
            var xaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Controls",
                "ColorTile.xaml");

            AssertContainsInOrder(
                xaml,
                "<Border Style=\"{DynamicResource ColorTilesPanelStyle}\" BorderThickness=\"0\" CornerRadius=\"{TemplateBinding TileRadius}\" Background=\"{TemplateBinding Background}\">",
                "Name=\"ColorNameTextBlock\"",
                "Foreground=\"{TemplateBinding Foreground}\"",
                "Style=\"{DynamicResource BodyStrongTextBlockStyle}\"",
                "Text=\"{TemplateBinding ColorName}\"",
                "x:Name=\"CopyBrushNameButton\"",
                "AutomationProperties.Name=\"{Binding ColorBrushName, StringFormat='{}Copy brush name {0} to clipboard', RelativeSource={RelativeSource Mode=TemplatedParent}}\"",
                "Grid.RowSpan=\"4\"",
                "Grid.Column=\"1\"",
                "Grid.ColumnSpan=\"2\"",
                "Padding=\"4\"",
                "Margin=\"0,12,12,0\"",
                "Background=\"Transparent\"",
                "BorderBrush=\"Transparent\"",
                "Foreground=\"{TemplateBinding Foreground}\"",
                "Command=\"ApplicationCommands.Copy\"",
                "CommandTarget=\"{Binding RelativeSource={RelativeSource AncestorType={x:Type controls:ColorTile}}}\"",
                "FocusManager.IsFocusScope=\"True\"",
                "ToolTipService.ToolTip=\"Copy brush name\"",
                "<TextBlock x:Name=\"CopyGlyphTextBlock\" FontFamily=\"{StaticResource SymbolThemeFontFamily}\" FontSize=\"16\" Text=\"&#xE8C8;\" />",
                "<TextBlock x:Name=\"SuccessGlyphTextBlock\" FontFamily=\"{StaticResource SymbolThemeFontFamily}\" FontSize=\"16\" Text=\"&#xE73E;\" Opacity=\"0\" />",
                "<DoubleAnimation Duration=\"0:0:0.333\" Storyboard.TargetName=\"CopyGlyphTextBlock\" Storyboard.TargetProperty=\"Opacity\" To=\"0\" />",
                "<DoubleAnimation Duration=\"0:0:0.666\" BeginTime=\"0:0:0.333\" Storyboard.TargetName=\"SuccessGlyphTextBlock\" Storyboard.TargetProperty=\"Opacity\" To=\"1\" />",
                "<DoubleAnimation Storyboard.TargetName=\"SuccessGlyphTextBlock\" BeginTime=\"0:0:2\" Duration=\"0:0:0.01\" Storyboard.TargetProperty=\"Opacity\" To=\"0\" />",
                "<DoubleAnimation Storyboard.TargetName=\"CopyGlyphTextBlock\" BeginTime=\"0:0:2.1\" Duration=\"0:0:0.01\" Storyboard.TargetProperty=\"Opacity\" To=\"1\" />",
                "Name=\"ColorExplanationTextBlock\"",
                "Text=\"{TemplateBinding ColorExplanation}\"",
                "Name=\"ColorBrushNameTextBlock\"",
                "Text=\"{TemplateBinding ColorBrushName}\"",
                "Visibility=\"{Binding ShowWarning, Converter={StaticResource BooleanToVisibilityConverter}, RelativeSource={RelativeSource TemplatedParent}}\"",
                "Visibility=\"{Binding ShowSeparator, Converter={StaticResource BooleanToVisibilityConverter}, RelativeSource={RelativeSource TemplatedParent}}\"",
                "<Setter Property=\"Foreground\" Value=\"{DynamicResource SystemColorWindowTextColorBrush}\" TargetName=\"ColorExplanationTextBlock\" />",
                "<Setter Property=\"Background\" Value=\"{DynamicResource SystemColorWindowColorBrush}\" TargetName=\"ColorExplanationTextBlock\" />",
                "<Setter Property=\"Foreground\" Value=\"{DynamicResource SystemColorWindowTextColorBrush}\" TargetName=\"ColorBrushNameTextBlock\" />",
                "<Setter Property=\"Background\" Value=\"{DynamicResource SystemColorWindowColorBrush}\" TargetName=\"ColorBrushNameTextBlock\" />",
                "<Setter Property=\"Foreground\" Value=\"{DynamicResource SystemColorWindowTextColorBrush}\" TargetName=\"ColorNameTextBlock\" />",
                "<Setter Property=\"Background\" Value=\"{DynamicResource SystemColorWindowColorBrush}\" TargetName=\"ColorNameTextBlock\" />",
                "<Setter Property=\"Foreground\" Value=\"{DynamicResource SystemColorWindowTextColorBrush}\" TargetName=\"CopyBrushNameButton\" />",
                "<Setter Property=\"Background\" Value=\"{DynamicResource SystemColorWindowColorBrush}\" TargetName=\"CopyBrushNameButton\" />");

            Assert.IsFalse(
                xaml.Contains("x:Name=\"ColorNameTextBlock\"", StringComparison.Ordinal),
                "The copied ColorTile template should keep the official Name= source shape for the color name TextBlock.");
            Assert.IsFalse(
                xaml.Contains("x:Name=\"ColorExplanationTextBlock\"", StringComparison.Ordinal),
                "The copied ColorTile template should keep the official Name= source shape for the color explanation TextBlock.");
            Assert.IsFalse(
                xaml.Contains("x:Name=\"ColorBrushNameTextBlock\"", StringComparison.Ordinal),
                "The copied ColorTile template should keep the official Name= source shape for the color brush name TextBlock.");
        }

        [TestMethod]
        public void SharedColorTileCodeBehindKeepsOfficialMemberAndCopyHandlerSourceShape()
        {
            var source = ReadRepoFile(
                "ModernWpf.Gallery",
                "Controls",
                "ColorTile.cs");

            AssertContainsInOrder(
                source,
                "public partial class ColorTile : UserControl",
                "static ColorTile()",
                "CommandManager.RegisterClassCommandBinding(typeof(ColorTile), new CommandBinding(ApplicationCommands.Copy, Copy_ColorBrushName));",
                "public CornerRadius TileRadius",
                "public static readonly DependencyProperty TileRadiusProperty",
                "DependencyProperty.Register(\"TileRadius\", typeof(CornerRadius), typeof(ColorTile), new PropertyMetadata(new CornerRadius(0)));",
                "public string ColorName",
                "public static readonly DependencyProperty ColorNameProperty",
                "DependencyProperty.Register(\"ColorName\", typeof(string), typeof(ColorTile), new PropertyMetadata(\"\"));",
                "public string ColorExplanation",
                "public static readonly DependencyProperty ColorExplanationProperty",
                "DependencyProperty.Register(\"ColorExplanation\", typeof(string), typeof(ColorTile), new PropertyMetadata(\"\"));",
                "public string ColorBrushName",
                "public static readonly DependencyProperty ColorBrushNameProperty",
                "DependencyProperty.Register(\"ColorBrushName\", typeof(string), typeof(ColorTile), new PropertyMetadata(\"\"));",
                "public string ColorValue",
                "public static readonly DependencyProperty ColorValueProperty",
                "DependencyProperty.Register(\"ColorValue\", typeof(string), typeof(ColorTile), new PropertyMetadata(\"\"));",
                "public bool ShowSeparator",
                "public static readonly DependencyProperty ShowSeparatorProperty",
                "DependencyProperty.Register(\"ShowSeparator\", typeof(bool), typeof(ColorTile), new PropertyMetadata(true));",
                "public bool ShowWarning",
                "public static readonly DependencyProperty ShowWarningProperty",
                "DependencyProperty.Register(\"ShowWarning\", typeof(bool), typeof(ColorTile), new PropertyMetadata(false));",
                "private static void Copy_ColorBrushName(object sender, RoutedEventArgs e)",
                "if (sender is ColorTile colorTile)",
                "if (!string.IsNullOrEmpty(colorTile.ColorBrushName))",
                "Clipboard.SetText(colorTile.ColorBrushName);",
                "RaiseNotification(colorTile);");
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
                "<Page",
                "x:Class=\"ModernWpf.Gallery.Pages.HomePage\"",
                "xmlns:d=\"http://schemas.microsoft.com/expression/blend/2008\"",
                "xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages\"",
                "xmlns:mc=\"http://schemas.openxmlformats.org/markup-compatibility/2006\"",
                "xmlns:controls=\"clr-namespace:ModernWpf.Gallery.Controls\"",
                "xmlns:pages=\"clr-namespace:ModernWpf.Gallery.Pages\"",
                "Title=\"DashboardPage\"",
                "d:DesignHeight=\"450\"",
                "d:DesignWidth=\"800\"",
                "Foreground=\"{DynamicResource TextFillColorPrimaryBrush}\"",
                "mc:Ignorable=\"d\"",
                "Margin=\"-24,-16,-24,12\">");
            Assert.IsFalse(
                xaml.Contains("x:Name=\"ContentRootGrid\"", StringComparison.Ordinal),
                "The copied Home page should use the official Dashboard ScrollViewer root shape instead of a local-only root name.");
            AssertContainsInOrder(
                xaml,
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
            var normalizedXaml = xaml.Replace("\r\n", "\n").Replace('\r', '\n');

            AssertContainsInOrder(
                xaml,
                "<Page",
                "x:Class=\"ModernWpf.Gallery.Pages.WhatsNewPage\"",
                "xmlns:mc=\"http://schemas.openxmlformats.org/markup-compatibility/2006\"",
                "xmlns:d=\"http://schemas.microsoft.com/expression/blend/2008\"",
                "xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages\"",
                "xmlns:controls=\"clr-namespace:ModernWpf.Gallery.Controls\"",
                "mc:Ignorable=\"d\"",
                "Foreground=\"{DynamicResource TextFillColorPrimaryBrush}\"",
                "d:DesignHeight=\"450\"",
                "d:DesignWidth=\"800\"",
                "Title=\"What's New in WPF\">");
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
            AssertContainsInOrder(
                normalizedXaml,
                "<TextBlock Style=\"{StaticResource TitleTextBlockStyle}\" Margin=\"0 0 0 12\">\n                    .NET 10\n                </TextBlock>",
                "<TextBlock Style=\"{StaticResource SubtitleTextBlockStyle}\" Margin=\"0 0 0 12\">\n                    New and Enhanced Fluent Styles\n                </TextBlock>",
                "<TextBlock TextWrapping=\"Wrap\" Margin=\"0 0 0 12\">\n                    <Run>\n                        The WPF Grid supports a shorthand syntax for defining row and column sizes using the RowDefinitions and ColumnDefinitions attribute.",
                "<controls:ControlExample\n                    Margin=\"2 10 2 24\"\n                    HeaderText=\"Grid Shorthand Syntax Sample\"",
                "<TextBlock Style=\"{StaticResource TitleTextBlockStyle}\" Margin=\"0 0 0 12\">\n                    .NET 9\n                </TextBlock>",
                "<TextBlock Style=\"{StaticResource SubtitleTextBlockStyle}\" Margin=\"0 24 0 12\">\n                    Hyphen based ligature support\n                </TextBlock>",
                "<TextBlock Margin=\"0 0 16 0\" FontFamily=\"Cascadia Code\" Text=\"-->\" />");
            StringAssert.Contains(
                xaml,
                "Background=\"{DynamicResource SystemControlBackgroundAccentBrush}\"");
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
                "<Page",
                "x:Class=\"ModernWpf.Gallery.Pages.SettingsPage\"",
                "xmlns:d=\"http://schemas.microsoft.com/expression/blend/2008\"",
                "xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages\"",
                "xmlns:controls=\"clr-namespace:ModernWpf.Gallery.Controls\"",
                "xmlns:mc=\"http://schemas.openxmlformats.org/markup-compatibility/2006\"",
                "Title=\"SettingsPage\"",
                "d:DesignHeight=\"450\"",
                "d:DesignWidth=\"800\"",
                "Foreground=\"{DynamicResource TextFillColorPrimaryBrush}\"",
                "mc:Ignorable=\"d\">");
            AssertContainsInOrder(
                xaml,
                "<Style x:Key=\"SettingsCardStyle\" TargetType=\"Border\">",
                "<Setter Property=\"Padding\" Value=\"0,16,0,16\" />",
                "<Setter Property=\"BorderThickness\" Value=\"0,0,0,1\" />",
                "<Setter Property=\"BorderBrush\" Value=\"{DynamicResource ExpanderHeaderBorderBrush}\" />");
            Assert.IsFalse(
                xaml.Contains("x:Name=\"ContentRootGrid\"", StringComparison.Ordinal),
                "The copied Settings root should be located structurally instead of by a local-only name.");
            StringAssert.Contains(
                xaml,
                "<Grid Style=\"{StaticResource GalleryPageRootStyle}\">");
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
                "<Page",
                "x:Class=\"ModernWpf.Gallery.Pages.AllControlsPage\"",
                "xmlns:d=\"http://schemas.microsoft.com/expression/blend/2008\"",
                "xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages\"",
                "xmlns:mc=\"http://schemas.openxmlformats.org/markup-compatibility/2006\"",
                "xmlns:controls=\"clr-namespace:ModernWpf.Gallery.Controls\"",
                "Title=\"AllSamplesPage\"",
                "Foreground=\"{DynamicResource TextFillColorPrimaryBrush}\"",
                "mc:Ignorable=\"d\">");
            Assert.IsFalse(
                xaml.Contains("d:DesignHeight=", StringComparison.Ordinal),
                "All Controls should match the official AllSamples root without local design-time dimensions.");
            Assert.IsFalse(
                xaml.Contains("d:DesignWidth=", StringComparison.Ordinal),
                "All Controls should match the official AllSamples root without local design-time dimensions.");
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
                "<Page",
                "x:Class=\"ModernWpf.Gallery.Pages.SectionPage\"",
                "xmlns:d=\"http://schemas.microsoft.com/expression/blend/2008\"",
                "xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages\"",
                "xmlns:mc=\"http://schemas.openxmlformats.org/markup-compatibility/2006\"",
                "xmlns:controls=\"clr-namespace:ModernWpf.Gallery.Controls\"",
                "Title=\"NavigationPage\"",
                "d:DesignHeight=\"450\"",
                "d:DesignWidth=\"800\"",
                "Foreground=\"{DynamicResource TextFillColorPrimaryBrush}\"",
                "mc:Ignorable=\"d\">");
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
        public void SimpleDateMediaAndStatusCodeBehindKeepOfficialConstructorParagraphShape()
        {
            foreach (var page in new[]
            {
                Tuple.Create("DateAndTime", "CalendarPage"),
                Tuple.Create("DateAndTime", "DatePickerPage"),
                Tuple.Create("Media", "CanvasPage"),
                Tuple.Create("Media", "ImagePage"),
                Tuple.Create("StatusAndInfo", "ProgressBarPage"),
                Tuple.Create("StatusAndInfo", "ToolTipPage")
            })
            {
                var source = ReadRepoFile(
                    "ModernWpf.Gallery",
                    "Pages",
                    "WpfGallery",
                    page.Item1,
                    page.Item2 + ".xaml.cs");
                var normalizedSource = source.Replace("\r\n", "\n").Replace('\r', '\n');

                StringAssert.Contains(
                    normalizedSource,
                    "            DataContext = this;\n\n            InitializeComponent();");
            }
        }

        [TestMethod]
        public void BasicInputCodeBehindKeepsOfficialConstructorParagraphShape()
        {
            var buttonSource = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "BasicInput",
                "ButtonPage.xaml.cs").Replace("\r\n", "\n").Replace('\r', '\n');
            StringAssert.Contains(
                buttonSource,
                "            DataContext = this;\n            InitializeComponent();");

            var checkBoxSource = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "BasicInput",
                "CheckBoxPage.xaml.cs").Replace("\r\n", "\n").Replace('\r', '\n');
            StringAssert.Contains(
                checkBoxSource,
                "        public CheckBoxPageViewModel ViewModel { get; }\n        public CheckBoxPage(CheckBoxPageViewModel viewModel)");

            foreach (var page in new[]
            {
                "ComboBoxPage",
                "RadioButtonPage",
                "SliderPage"
            })
            {
                var source = ReadRepoFile(
                    "ModernWpf.Gallery",
                    "Pages",
                    "WpfGallery",
                    "BasicInput",
                    page + ".xaml.cs").Replace("\r\n", "\n").Replace('\r', '\n');

                StringAssert.Contains(
                    source,
                    "            DataContext = this;\n\n            InitializeComponent();");
            }
        }

        [TestMethod]
        public void CollectionsCodeBehindKeepsOfficialConstructorAdjacencyShape()
        {
            foreach (var page in new[]
            {
                Tuple.Create("DataGridPage", "DataGridPageViewModel"),
                Tuple.Create("ListBoxPage", "ListBoxPageViewModel"),
                Tuple.Create("TreeViewPage", "TreeViewPageViewModel")
            })
            {
                var source = ReadRepoFile(
                    "ModernWpf.Gallery",
                    "Pages",
                    "WpfGallery",
                    "Collections",
                    page.Item1 + ".xaml.cs").Replace("\r\n", "\n").Replace('\r', '\n');

                StringAssert.Contains(
                    source,
                    "        public " + page.Item2 + " ViewModel { get; }\n        public " + page.Item1 + "(");
            }

            var listViewSource = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Collections",
                "ListViewPage.xaml.cs").Replace("\r\n", "\n").Replace('\r', '\n');
            StringAssert.Contains(
                listViewSource,
                "        public ListViewPageViewModel ViewModel { get; }\n\n        public ListViewPage(ListViewPageViewModel viewModel)");

            var dataGridSource = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Collections",
                "DataGridPage.xaml.cs");
            AssertContainsInOrder(
                dataGridSource,
                "SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;",
                "Loaded += OnLoaded;",
                "Unloaded += OnUnloaded;",
                "private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)",
                "Dispatcher.Invoke(() =>",
                "UpdatePageVisuals();",
                "SystemEvents.UserPreferenceChanged -= SystemEvents_UserPreferenceChanged;");
        }

        [TestMethod]
        public void TextCodeBehindKeepsOfficialConstructorAdjacencyShape()
        {
            foreach (var page in new[]
            {
                Tuple.Create("LabelPage", "LabelPageViewModel"),
                Tuple.Create("PasswordBoxPage", "PasswordBoxPageViewModel"),
                Tuple.Create("RichTextEditPage", "RichTextEditPageViewModel"),
                Tuple.Create("TextBlockPage", "TextBlockPageViewModel"),
                Tuple.Create("TextBoxPage", "TextBoxPageViewModel")
            })
            {
                var source = ReadRepoFile(
                    "ModernWpf.Gallery",
                    "Pages",
                    "WpfGallery",
                    "Text",
                    page.Item1 + ".xaml.cs").Replace("\r\n", "\n").Replace('\r', '\n');

                StringAssert.Contains(
                    source,
                    "        public " + page.Item2 + " ViewModel { get; }\n        public " + page.Item1 + "(");
            }

            var hyperlinkSource = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Text",
                "HyperlinkPage.xaml.cs").Replace("\r\n", "\n").Replace('\r', '\n');
            AssertContainsInOrder(
                hyperlinkSource,
                "public HyperlinkPage(HyperlinkPageViewModel viewModel)",
                "InitializeComponent();",
                "public HyperlinkPageViewModel ViewModel { get; }");
        }

        [TestMethod]
        public void MessageBoxCodeBehindKeepsOfficialShowCallSourceShape()
        {
            var source = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "System",
                "MessageBoxPage.xaml.cs");
            var normalizedSource = source.Replace("\r\n", "\n").Replace('\r', '\n');

            StringAssert.Contains(
                normalizedSource,
                "            var result = MessageBox.Show(\"This is a detailed description of what happened or what action is needed.\", \"Custom Title\");\n            ViewModel.CustomTitleResult = $\"Result: {result}\";");
            StringAssert.Contains(
                normalizedSource,
                "            var result = MessageBox.Show($\"This MessageBox has {buttonName} button(s).\", $\"{buttonName} Button(s)\", buttonType);\n            ViewModel.DifferentButtonsResult = $\"Result: {result}\";");
            StringAssert.Contains(
                normalizedSource,
                "            var result = MessageBox.Show($\"This MessageBox displays the {imageName} icon.\", $\"{imageName} Icon\", MessageBoxButton.OK, imageType);\n            ViewModel.DifferentImagesResult = $\"Result: {result}\";");
            StringAssert.Contains(
                normalizedSource,
                "        // 6. Common Messages (Information, Error, Warning)\n        private void ShowCommonInformation_Click(object sender, RoutedEventArgs e)");
            StringAssert.Contains(
                normalizedSource,
                "        // 7. Custom Default Button\n        private void ShowCustomDefaultButton_Click(object sender, RoutedEventArgs e)");
            StringAssert.Contains(
                normalizedSource,
                "            var result = MessageBox.Show(\"Do you want to save changes? Press Enter to select the default 'No' button.\", \"Save Changes\", MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.No);\n            ViewModel.CustomDefaultResult = $\"User selected: {result}\";");

            AssertContainsInOrder(
                normalizedSource,
                "var buttonType = GetMessageBoxButton(ViewModel.SelectedButtonIndex);",
                "var result = MessageBox.Show($\"This MessageBox has {buttonName} button(s).\", $\"{buttonName} Button(s)\", buttonType);",
                "private static MessageBoxButton GetMessageBoxButton(int index)");
            AssertContainsInOrder(
                normalizedSource,
                "var imageType = GetMessageBoxImage(ViewModel.SelectedImageIndex);",
                "var result = MessageBox.Show($\"This MessageBox displays the {imageName} icon.\", $\"{imageName} Icon\", MessageBoxButton.OK, imageType);",
                "private static MessageBoxImage GetMessageBoxImage(int index)");
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
            var normalizedCheckBoxXaml = checkBoxXaml.Replace("\r\n", "\n").Replace('\r', '\n');
            StringAssert.Contains(
                checkBoxXaml,
                "<controls:ControlExample Margin=\"10\" HeaderText=\"A 2-state CheckBox.\" XamlCode=\"&lt;CheckBox Content=&quot;Two-state CheckBox&quot; /&gt;\">");
            StringAssert.Contains(
                normalizedCheckBoxXaml,
                "</controls:ControlExample>\n\n\n                <controls:ControlExample\n                    Margin=\"10,32,10,10\"\n                    HeaderText=\"A 3-state CheckBox.\"");
            StringAssert.Contains(
                normalizedCheckBoxXaml,
                "</controls:ControlExample>\n            </StackPanel>\n\n        </ScrollViewer>");

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
            var normalizedRadioButtonXaml = radioButtonXaml.Replace("\r\n", "\n").Replace('\r', '\n');
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
            StringAssert.Contains(
                normalizedRadioButtonXaml,
                "</controls:ControlExample>\n            </StackPanel>\n\n        </ScrollViewer>");

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
            StringAssert.Contains(
                listViewXaml,
                "<Label Foreground=\"{DynamicResource TextFillColorPrimaryBrush}\" Opacity=\"0.7\" Content=\"Selection mode\" Target=\"{Binding ElementName=SelectionModeComboBox}\" />");
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

            var dataGridXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Collections",
                "DataGridPage.xaml");
            var normalizedDataGridXaml = dataGridXaml.Replace("\r\n", "\n").Replace('\r', '\n');
            AssertContainsInOrder(
                dataGridXaml,
                "<controls:ControlExample",
                "Margin=\"10\"",
                "HeaderText=\"Default DataGrid with ItemsSource.\"",
                "XamlCode=\"&lt;DataGrid ItemsSource=&quot;{Binding ViewModel.ProductsCollection, Mode=TwoWay}&quot; /&gt;\"",
                "<DataGrid",
                "x:Name=\"SampleDataGrid\"",
                "Height=\"400\"",
                "AutomationProperties.Name=\"Sample Data Grid\"",
                "ItemsSource=\"{Binding ViewModel.ProductsCollection, Mode=TwoWay}\" />");
            StringAssert.Contains(
                normalizedDataGridXaml,
                "</controls:ControlExample>\n\n            </StackPanel>");
            StringAssert.Contains(
                normalizedDataGridXaml,
                "</Grid>\n\n\n</Page>");

            var treeViewXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Collections",
                "TreeViewPage.xaml");
            AssertContainsInOrder(
                treeViewXaml,
                "<Grid Margin=\"0,0,0,24\">",
                "<controls:ControlExample",
                "Margin=\"10\"",
                "HeaderText=\"Simple TreeView.\"",
                "XamlCode=\"&lt;TreeView AllowDrop=&quot;True&quot; ScrollViewer.CanContentScroll=&quot;False&quot;&gt;",
                "<TreeView",
                "AllowDrop=\"True\"",
                "AutomationProperties.Name=\"Sample TreeView\"",
                "ScrollViewer.CanContentScroll=\"False\">",
                "<TreeViewItem",
                "Header=\"Work Documents\"",
                "IsExpanded=\"True\"",
                "IsSelected=\"True\">",
                "<TreeViewItem Header=\"Feature Schedule\" />",
                "<TreeViewItem Header=\"Overall Project Plan\" />",
                "<TreeViewItem Header=\"Personal Documents\">",
                "<TreeViewItem Header=\"Contractor contact info\" />",
                "<TreeViewItem Header=\"Home Remodel\">",
                "<TreeViewItem Header=\"Paint Color Scheme\" />",
                "<TreeViewItem Header=\"Flooring Woodgrain Type\" />",
                "<TreeViewItem Header=\"Kitchen Cabinet Style\" />");
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
                    AssertContainsInOrder(
                        xaml,
                        "<Page x:Class=\"ModernWpf.Gallery.Pages.WpfGallery.Layout.GridSplitterPage\"",
                        "xmlns:mc=\"http://schemas.openxmlformats.org/markup-compatibility/2006\"",
                        "xmlns:d=\"http://schemas.microsoft.com/expression/blend/2008\"",
                        "xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages.WpfGallery.Layout\"",
                        "xmlns:sys=\"clr-namespace:System;assembly=System.Runtime\"",
                        "xmlns:controls=\"clr-namespace:ModernWpf.Gallery.Controls\"",
                        "mc:Ignorable=\"d\"",
                        "d:DesignHeight=\"450\" d:DesignWidth=\"800\"",
                        "Title=\"GridSplitterPage\"",
                        "Foreground=\"{DynamicResource TextFillColorPrimaryBrush}\">");
                }
                else if (page.Item1 == "GridPage.xaml")
                {
                    AssertContainsInOrder(
                        xaml,
                        "<Page x:Class=\"ModernWpf.Gallery.Pages.WpfGallery.Layout.GridPage\"",
                        "xmlns:mc=\"http://schemas.openxmlformats.org/markup-compatibility/2006\"",
                        "xmlns:d=\"http://schemas.microsoft.com/expression/blend/2008\"",
                        "xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages.WpfGallery.Layout\"",
                        "xmlns:controls=\"clr-namespace:ModernWpf.Gallery.Controls\"",
                        "mc:Ignorable=\"d\"",
                        "d:DesignHeight=\"450\" d:DesignWidth=\"800\"",
                        "Title=\"GridPage\"",
                        "Foreground=\"{DynamicResource TextFillColorPrimaryBrush}\">");
                }
                else if (page.Item1 == "GroupBoxPage.xaml")
                {
                    AssertContainsInOrder(
                        xaml,
                        "<Page x:Class=\"ModernWpf.Gallery.Pages.WpfGallery.Layout.GroupBoxPage\"",
                        "xmlns:mc=\"http://schemas.openxmlformats.org/markup-compatibility/2006\"",
                        "xmlns:d=\"http://schemas.microsoft.com/expression/blend/2008\"",
                        "xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages.WpfGallery.Layout\"",
                        "xmlns:controls=\"clr-namespace:ModernWpf.Gallery.Controls\"",
                        "mc:Ignorable=\"d\"",
                        "d:DesignHeight=\"450\" d:DesignWidth=\"800\"",
                        "Title=\"GroupBoxPage\"",
                        "Foreground=\"{DynamicResource TextFillColorPrimaryBrush}\">");
                }
                else if (page.Item1 == "ResizeGripPage.xaml")
                {
                    AssertContainsInOrder(
                        xaml,
                        "<Page x:Class=\"ModernWpf.Gallery.Pages.WpfGallery.Layout.ResizeGripPage\"",
                        "xmlns:mc=\"http://schemas.openxmlformats.org/markup-compatibility/2006\"",
                        "xmlns:d=\"http://schemas.microsoft.com/expression/blend/2008\"",
                        "xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages.WpfGallery.Layout\"",
                        "xmlns:controls=\"clr-namespace:ModernWpf.Gallery.Controls\"",
                        "mc:Ignorable=\"d\"",
                        "d:DesignHeight=\"450\" d:DesignWidth=\"800\"",
                        "Title=\"ResizeGripPage\"",
                        "Foreground=\"{DynamicResource TextFillColorPrimaryBrush}\">");
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
            var normalizedGridXaml = gridXaml.Replace("\r\n", "\n").Replace('\r', '\n');
            StringAssert.Contains(
                normalizedGridXaml,
                "XamlCode=\"&lt;Grid ShowGridLines=&quot;True&quot;&gt;&#10;\n    &lt;Grid.RowDefinitions&gt;&#10;\n        &lt;RowDefinition Height=&quot;*&quot; /&gt;&#10;");
            StringAssert.Contains(
                normalizedGridXaml,
                "    &lt;TextBlock Grid.Row=&quot;2&quot; Grid.Column=&quot;2&quot; Text=&quot;Cell 9&quot; /&gt;&#10;\n&lt;/Grid&gt;\">");
            StringAssert.Contains(
                normalizedGridXaml,
                "XamlCode=\"&lt;Grid&gt;&#10;\n    &lt;Grid.RowDefinitions&gt;&#10;\n        &lt;RowDefinition Height=&quot;Auto&quot; /&gt;&#10;");
            StringAssert.Contains(
                normalizedGridXaml,
                "    &lt;Border Grid.Row=&quot;2&quot; Grid.Column=&quot;2&quot; Background=&quot;{DynamicResource ControlFillColorDefaultBrush}&quot; Margin=&quot;5&quot; Padding=&quot;10&quot;&gt;&#10;\n        &lt;TextBlock Text=&quot;Row 2, Column 2&quot; /&gt;&#10;\n    &lt;/Border&gt;&#10;\n&lt;/Grid&gt;\">");
            StringAssert.Contains(
                normalizedGridXaml,
                "XamlCode=\"&lt;Grid RowDefinitions=&quot;Auto,*,Auto&quot; ColumnDefinitions=&quot;100,2*,*&quot;&gt;&#10;\n    &lt;Border Grid.Row=&quot;0&quot; Grid.Column=&quot;0&quot; Background=&quot;{DynamicResource ControlFillColorDefaultBrush}&quot; Margin=&quot;5&quot; Padding=&quot;10&quot;&gt;&#10;");
            StringAssert.Contains(
                normalizedGridXaml,
                "    &lt;Border Grid.Row=&quot;2&quot; Grid.Column=&quot;0&quot; Grid.ColumnSpan=&quot;3&quot; Background=&quot;{DynamicResource ControlFillColorDefaultBrush}&quot; Margin=&quot;5&quot; Padding=&quot;10&quot;&gt;&#10;\n        &lt;TextBlock Text=&quot;Footer (Auto height, spans all columns)&quot; /&gt;&#10;\n    &lt;/Border&gt;&#10;\n&lt;/Grid&gt;\">");
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
            StringAssert.Contains(
                gridSplitterXaml.Replace("\r\n", "\n").Replace('\r', '\n'),
                "XamlCode=\"&lt;Grid Height=&quot;400&quot;&gt;&#10;\n    &lt;Grid.RowDefinitions&gt;&#10;");
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
                "<GridSplitter Grid.RowSpan=\"5\" Grid.Column=\"1\" ResizeDirection=\"Columns\"/>",
                "<GridSplitter Grid.Row=\"1\" Grid.ColumnSpan=\"3\" ResizeDirection=\"Rows\"/>",
                "<GridSplitter Grid.Row=\"3\" Grid.ColumnSpan=\"1\" ResizeDirection=\"Rows\"/>");

            var groupBoxXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Layout",
                "GroupBoxPage.xaml");
            var normalizedGroupBoxXaml = groupBoxXaml.Replace("\r\n", "\n").Replace('\r', '\n');
            StringAssert.Contains(
                normalizedGroupBoxXaml,
                "XamlCode=\"&lt;GroupBox &#10;\n   Header=&quot;User Information&quot; &#10;\n   HorizontalAlignment=&quot;Left&quot; &#10;\n   VerticalAlignment=&quot;Center&quot; &#10;\n   Width=&quot;400&quot;&gt;&#10;");
            StringAssert.Contains(
                normalizedGroupBoxXaml,
                "        &lt;Button Content=&quot;Submit&quot; HorizontalAlignment=&quot;Right&quot; Width=&quot;100&quot; Margin=&quot;0,10,0,0&quot; /&gt;&#10;\n    &lt;/StackPanel&gt;&#10;&lt;/GroupBox&gt;\">");
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
            StringAssert.Contains(
                resizeGripXaml,
                "<LineBreak/>");
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

            var resizeGripCode = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Layout",
                "ResizeGripPage.xaml.cs");
            AssertContainsInOrder(
                resizeGripCode,
                "private void OpenResizeGripWindow_Click(object sender, RoutedEventArgs e)",
                "Window window = new Window()",
                "ResizeMode = ResizeMode.CanResizeWithGrip,",
                "Content = new TextBlock",
                "Text = \"ResizeGrip is present at the bottom right corner of the window\",",
                "window.Show();");

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
            AssertContainsInOrder(
                colorXaml,
                "<Page x:Class=\"ModernWpf.Gallery.Pages.WpfGallery.DesignGuidance.ColorPage\"",
                "xmlns:mc=\"http://schemas.openxmlformats.org/markup-compatibility/2006\"",
                "xmlns:d=\"http://schemas.microsoft.com/expression/blend/2008\"",
                "xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages.WpfGallery.DesignGuidance\"",
                "xmlns:controls=\"clr-namespace:ModernWpf.Gallery.Controls\"",
                "xmlns:sys=\"clr-namespace:System;assembly=mscorlib\"",
                "mc:Ignorable=\"d\"",
                "Foreground=\"{DynamicResource TextFillColorPrimaryBrush}\"",
                "d:DesignHeight=\"450\" d:DesignWidth=\"800\"",
                "Title=\"ColorsPage\">");
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
                AssertContainsInOrder(
                    xaml,
                    page == "TypographyPage.xaml"
                        ? "<Page x:Class=\"ModernWpf.Gallery.Pages.WpfGallery.DesignGuidance.TypographyPage\""
                        : "<Page x:Class=\"ModernWpf.Gallery.Pages.WpfGallery.DesignGuidance.SpacingPage\"",
                    "xmlns:mc=\"http://schemas.openxmlformats.org/markup-compatibility/2006\"",
                    "xmlns:d=\"http://schemas.microsoft.com/expression/blend/2008\"",
                    "xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages.WpfGallery.DesignGuidance\"",
                    "mc:Ignorable=\"d\" Foreground=\"{DynamicResource TextFillColorPrimaryBrush}\"",
                    "xmlns:controls=\"clr-namespace:ModernWpf.Gallery.Controls\"",
                    "d:DesignHeight=\"450\" d:DesignWidth=\"800\"",
                    page == "TypographyPage.xaml"
                        ? "Title=\"TypographyPage\">"
                        : "Title=\"SpacingPage\">");
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
            AssertContainsInOrder(
                geometryXaml,
                "<Page x:Class=\"ModernWpf.Gallery.Pages.WpfGallery.DesignGuidance.GeometryPage\"",
                "xmlns:mc=\"http://schemas.openxmlformats.org/markup-compatibility/2006\"",
                "xmlns:d=\"http://schemas.microsoft.com/expression/blend/2008\"",
                "xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages.WpfGallery.DesignGuidance\"",
                "mc:Ignorable=\"d\" Foreground=\"{DynamicResource TextFillColorPrimaryBrush}\"",
                "xmlns:controls=\"clr-namespace:ModernWpf.Gallery.Controls\"",
                "d:DesignHeight=\"450\" d:DesignWidth=\"800\"",
                "Title=\"GeometryPage\">");
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
            var normalizedIconographyXaml = iconographyXaml.Replace("\r\n", "\n").Replace('\r', '\n');
            AssertContainsInOrder(
                iconographyXaml,
                "<Page x:Class=\"ModernWpf.Gallery.Pages.WpfGallery.DesignGuidance.IconographyPage\"",
                "xmlns:d=\"http://schemas.microsoft.com/expression/blend/2008\"",
                "xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"",
                "xmlns:mc=\"http://schemas.openxmlformats.org/markup-compatibility/2006\"",
                "xmlns:controls=\"clr-namespace:ModernWpf.Gallery.Controls\"",
                "mc:Ignorable=\"d\"",
                "Foreground=\"{DynamicResource TextFillColorPrimaryBrush}\"",
                "d:DesignHeight=\"450\" d:DesignWidth=\"800\"",
                "d:Background=\"White\"",
                "Loaded=\"OnLoaded\"",
                "Title=\"IconsPage\">");
            Assert.IsFalse(
                iconographyXaml.Contains("xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages.WpfGallery.DesignGuidance\"", StringComparison.Ordinal),
                "IconographyPage.xaml should keep the current official root namespace shape, which has no local namespace declaration.");
            StringAssert.Contains(
                iconographyXaml,
                "<controls:PageHeader Margin=\"2,0,0,32\" Title=\"{Binding ViewModel.PageTitle}\" Description=\"{Binding ViewModel.PageDescription}\" />");
            AssertContainsInOrder(
                iconographyXaml,
                "<Button Grid.Column=\"1\"",
                "Padding=\"8\"",
                "FocusManager.IsFocusScope=\"True\"",
                "Command=\"ApplicationCommands.Copy\"",
                "AutomationProperties.Name=\"{Binding Tag, StringFormat='{}Copy {0} to clipboard', RelativeSource={RelativeSource Mode=TemplatedParent}}\"",
                "ToolTipService.ToolTip=\"Copy to clipboard\"",
                "CommandParameter=\"{TemplateBinding Content}\"",
                "CommandTarget=\"{Binding RelativeSource={RelativeSource AncestorType=Page}}\">");
            StringAssert.Contains(
                iconographyXaml,
                "<TextBlock x:Name=\"CopyGlyphTextBlock\" FontFamily=\"{StaticResource SymbolThemeFontFamily}\" Text=\"&#xE8C8;\"/>");
            StringAssert.Contains(
                iconographyXaml,
                "<TextBlock x:Name=\"SuccessGlyphTextBlock\" FontFamily=\"{StaticResource SymbolThemeFontFamily}\" Text=\"&#xE73E;\" Opacity=\"0\" />");
            StringAssert.Contains(
                normalizedIconographyXaml,
                "                                                    <DoubleAnimation Duration=\"0:0:0.333\" Storyboard.TargetName=\"CopyGlyphTextBlock\" Storyboard.TargetProperty=\"Opacity\" To=\"0\" />\n                                                    <DoubleAnimation Duration=\"0:0:0.666\" BeginTime=\"0:0:0.333\" Storyboard.TargetName=\"SuccessGlyphTextBlock\" Storyboard.TargetProperty=\"Opacity\" To=\"1\" />\n                                                    <DoubleAnimation Storyboard.TargetName=\"SuccessGlyphTextBlock\" BeginTime=\"0:0:2\" Duration=\"0:0:0.01\" Storyboard.TargetProperty=\"Opacity\" To=\"0\" />\n                                                    <DoubleAnimation Storyboard.TargetName=\"CopyGlyphTextBlock\" BeginTime=\"0:0:2.1\" Duration=\"0:0:0.01\" Storyboard.TargetProperty=\"Opacity\" To=\"1\" />");
            AssertContainsInOrder(
                iconographyXaml,
                "<Expander Grid.Row=\"1\"",
                "Header=\"Instructions on how to use Segoe Fluent Icons\"",
                "IsExpanded=\"False\"",
                "Margin=\"2 -8 0 0\">");
            AssertContainsInOrder(
                iconographyXaml,
                "<Run FontWeight=\"SemiBold\">How to get the font</Run>",
                "<LineBreak />",
                "On Windows 10: Segoe Fluent Icons is not included by default on Windows 10.",
                "<LineBreak/>",
                "<LineBreak/>",
                "<Span FontWeight=\"SemiBold\">How to use the font</Span>",
                "<LineBreak/>",
                "For optimal appearance, use these specific sizes: 16, 20, 24, 32, 40, 48, and 64.",
                "<LineBreak/>",
                "<Hyperlink Click=\"Open_IconDesignGuidelinesPage\">layering</Hyperlink>",
                "<LineBreak/>",
                "<LineBreak/>",
                "<Run FontWeight=\"SemiBold\">XAML</Run>",
                "<LineBreak/>",
                "<Span>&lt;Grid&gt;</Span>",
                "<LineBreak/>",
                "<Span>&lt;TextBlock FontFamily=\"{StaticResource SymbolThemeFontFamily}\" Text=\"&amp;#xEB51;\" Foreground=\"#C72335\"/&gt;</Span>",
                "<LineBreak/>",
                "<Span>&#x09;&lt;TextBlock FontFamily=\"{StaticResource SymbolThemeFontFamily}\" Text=\"&amp;#xEB51;\" /&gt;</Span>",
                "<LineBreak/>",
                "<Span>&lt;/Grid&gt;</Span>");
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
        public void DesignGuidanceColorCodeBehindKeepsOfficialConstructorAndHandlerOrderShape()
        {
            var source = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "DesignGuidance",
                "ColorPage.xaml.cs").Replace("\r\n", "\n").Replace('\r', '\n');

            StringAssert.Contains(
                source,
                "        public ColorsPageViewModel ViewModel { get; }\n        public ColorPage(ColorsPageViewModel viewModel)");
            AssertContainsInOrder(
                source,
                "public ColorPage(ColorsPageViewModel viewModel)",
                "InitializeComponent();",
                "ViewModel = viewModel;",
                "DataContext = this;",
                "private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)",
                "var section = WpfGalleryColorSectionFactory.Create(PageSelector.SelectedIndex);",
                "ColorSubpageNavigationFrame.Navigate(section);",
                "private void OnLoaded(object sender, RoutedEventArgs e)",
                "PageSelector.SelectedItem = ResolveInitialSubpage();",
                "private object ResolveInitialSubpage()");
        }

        [TestMethod]
        public void DesignGuidanceDesignImageCodeBehindKeepsOfficialUserPreferenceHandlerShape()
        {
            foreach (var page in new[]
            {
                Tuple.Create("SpacingPage", "SpacingPageViewModel"),
                Tuple.Create("GeometryPage", "GeometryPageViewModel")
            })
            {
                var source = ReadRepoFile(
                    "ModernWpf.Gallery",
                    "Pages",
                    "WpfGallery",
                    "DesignGuidance",
                    page.Item1 + ".xaml.cs").Replace("\r\n", "\n").Replace('\r', '\n');

                AssertContainsInOrder(
                    source,
                    "public " + page.Item2 + " ViewModel { get; }",
                    "public " + page.Item1 + "(" + page.Item2 + " viewModel)",
                    "InitializeComponent();",
                    "UpdateImageResources();",
                    "ViewModel = viewModel;",
                    "DataContext = this;",
                    "Loaded += OnLoaded;",
                    "SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;",
                    "ThemeManager.AddActualThemeChangedHandler(this, OnActualThemeChanged);",
                    "Unloaded += OnUnloaded;",
                    "private void OnLoaded(object sender, RoutedEventArgs e)",
                    "UpdateImageResources();",
                    "private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)",
                    "Dispatcher.Invoke(() =>\n            {\n                UpdateImageResources();\n            });",
                    "private void OnActualThemeChanged(object sender, RoutedEventArgs e)",
                    "UpdateImageResources();",
                    "private void OnUnloaded(object sender, RoutedEventArgs e)",
                    "Loaded -= OnLoaded;",
                    "SystemEvents.UserPreferenceChanged -= SystemEvents_UserPreferenceChanged;",
                    "ThemeManager.RemoveActualThemeChangedHandler(this, OnActualThemeChanged);",
                    "Unloaded -= OnUnloaded;");
            }
        }

        [TestMethod]
        public void DesignGuidanceColorSubsectionRootsKeepOfficialSourceShape()
        {
            foreach (var section in new[]
            {
                "Text",
                "Fill",
                "Stroke",
                "Background",
                "Signal",
                "HighContrast"
            })
            {
                var sectionName = section + "Section";
                var xaml = ReadRepoFile(
                    "ModernWpf.Gallery",
                    "Pages",
                    "WpfGallery",
                    "DesignGuidance",
                    sectionName + ".xaml");

                AssertContainsInOrder(
                    xaml,
                    "<Page x:Class=\"ModernWpf.Gallery.Pages.WpfGallery.DesignGuidance." + sectionName + "\"",
                    "xmlns:mc=\"http://schemas.openxmlformats.org/markup-compatibility/2006\"",
                    "xmlns:d=\"http://schemas.microsoft.com/expression/blend/2008\"",
                    "xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages.WpfGallery.DesignGuidance\"",
                    "xmlns:controls=\"clr-namespace:ModernWpf.Gallery.Controls\"",
                    "mc:Ignorable=\"d\"",
                    "d:DesignHeight=\"450\" d:DesignWidth=\"800\"");

                if (section == "Background" || section == "Signal")
                {
                    AssertContainsInOrder(
                        xaml,
                        "Foreground=\"{DynamicResource WindowForeground}\"",
                        "Title=\"" + sectionName + "\">");
                }
                else
                {
                    AssertContainsInOrder(
                        xaml,
                        "Title=\"" + sectionName + "\"",
                        "Foreground=\"{DynamicResource WindowForeground}\">");
                }
            }
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
                "xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages.WpfGallery.DesignGuidance\"",
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
            var normalizedXaml = xaml.Replace("\r\n", "\n").Replace('\r', '\n');

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
            StringAssert.Contains(
                normalizedXaml,
                "            </ListView>\n            <Button\n                x:Name=\"NewUserButton\"");
            AssertContainsInOrder(
                xaml,
                "Margin=\"12,6,0,0\"",
                "Text=\"{Binding Name, Mode=OneWay}\"",
                "pages:GalleryAutomation.HeadingLevel=\"Level3\" />");
            StringAssert.Contains(
                normalizedXaml,
                "                Visibility=\"{Binding ViewModel.SelectedUser, Converter={StaticResource NullToVisibilityConverter}}\">\n                    <Ellipse\n                      x:Name=\"UserAvatar\"\n                      Width=\"96\"\n                      Height=\"96\"\n                      Margin=\"12\"\n                      HorizontalAlignment=\"Center\"\n                      VerticalAlignment=\"Center\">");
            StringAssert.Contains(
                normalizedXaml,
                "                    <StackPanel x:Name=\"UserDetailHeaderPanel\" Orientation=\"Vertical\" VerticalAlignment=\"Center\">\n                        <TextBlock\n                          x:Name=\"UserDetailHeaderNameBox\"\n                          FontSize=\"24\" Text=\"{Binding ViewModel.SelectedUser.Name}\" />\n                        <TextBlock\n                          x:Name=\"UserDetailHeaderCompanyBox\"\n                          FontSize=\"16\" Text=\"{Binding ViewModel.SelectedUser.Company}\" />\n                    </StackPanel>\n            </StackPanel>");
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
                "<TextBlock FontSize=\"14\" HorizontalAlignment=\"Left\" VerticalAlignment=\"Center\" Visibility=\"{Binding ViewModel.DeletedName, Converter={StaticResource EmptyToVisibilityConverter }}\" FontStyle=\"Italic\">");
            StringAssert.Contains(
                normalizedXaml,
                "                            <TextBlock FontSize=\"14\" HorizontalAlignment=\"Left\" VerticalAlignment=\"Center\" Visibility=\"{Binding ViewModel.DeletedName, Converter={StaticResource EmptyToVisibilityConverter }}\" FontStyle=\"Italic\">\n                                 <Run Text=\"User\" />\n                                 <Run Text=\"{Binding ViewModel.DeletedName, Mode=OneWay}\" />\n                                 <Run Text=\"Deleted!\" />\n                            </TextBlock>");
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
            StringAssert.Contains(
                normalizedXaml,
                "                                Content=\"Cancel\" />\n\n                      </StackPanel>\n                    </StackPanel>\n                </ScrollViewer>");
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
            AssertContainsInOrder(
                frameXaml,
                "<Page x:Class=\"ModernWpf.Gallery.Pages.WpfGallery.Navigation.FramePage\"",
                "xmlns:mc=\"http://schemas.openxmlformats.org/markup-compatibility/2006\"",
                "xmlns:d=\"http://schemas.microsoft.com/expression/blend/2008\"",
                "xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages.WpfGallery.Navigation\"",
                "xmlns:controls=\"clr-namespace:ModernWpf.Gallery.Controls\"",
                "mc:Ignorable=\"d\"",
                "d:DesignHeight=\"450\" d:DesignWidth=\"800\"",
                "Title=\"FramePage\"",
                "Foreground=\"{DynamicResource TextFillColorPrimaryBrush}\">");
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
            AssertContainsInOrder(
                navigationWindowXaml,
                "<Page x:Class=\"ModernWpf.Gallery.Pages.WpfGallery.Navigation.NavigationWindowPage\"",
                "xmlns:mc=\"http://schemas.openxmlformats.org/markup-compatibility/2006\"",
                "xmlns:d=\"http://schemas.microsoft.com/expression/blend/2008\"",
                "xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages.WpfGallery.Navigation\"",
                "xmlns:controls=\"clr-namespace:ModernWpf.Gallery.Controls\"",
                "mc:Ignorable=\"d\"",
                "d:DesignHeight=\"450\" d:DesignWidth=\"800\"",
                "Title=\"NavigationWindowPage\"",
                "Foreground=\"{DynamicResource TextFillColorPrimaryBrush}\">");
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

            foreach (var page in new[]
            {
                Tuple.Create(
                    "LabelPage.xaml",
                    "                </controls:ControlExample>\n            </StackPanel>\n\n        </ScrollViewer>\n    </Grid>\n\n</Page>"),
                Tuple.Create(
                    "TextBoxPage.xaml",
                    "                </controls:ControlExample>\n            </StackPanel>\n\n        </ScrollViewer>\n    </Grid>\n\n</Page>"),
                Tuple.Create(
                    "PasswordBoxPage.xaml",
                    "                </controls:ControlExample>\n\n            </StackPanel>\n\n        </ScrollViewer>\n    </Grid>\n\n</Page>"),
                Tuple.Create(
                    "RichTextEditPage.xaml",
                    "            </StackPanel>\n        </ScrollViewer>\n    </Grid>\n\n</Page>"),
                Tuple.Create(
                    "TextBlockPage.xaml",
                    "                </controls:ControlExample>\n\n            </StackPanel>\n\n        </ScrollViewer>\n    </Grid>\n\n</Page>")
            })
            {
                var xaml = ReadRepoFile(
                    "ModernWpf.Gallery",
                    "Pages",
                    "WpfGallery",
                    "Text",
                    page.Item1);
                StringAssert.Contains(
                    xaml.Replace("\r\n", "\n").Replace('\r', '\n'),
                    page.Item2);
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
                "<Page x:Class=\"ModernWpf.Gallery.Pages.WpfGallery.Text.HyperlinkPage\"",
                "xmlns:mc=\"http://schemas.openxmlformats.org/markup-compatibility/2006\"",
                "xmlns:d=\"http://schemas.microsoft.com/expression/blend/2008\"",
                "xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages.WpfGallery.Text\"",
                "xmlns:controls=\"clr-namespace:ModernWpf.Gallery.Controls\"",
                "mc:Ignorable=\"d\"",
                "d:DesignHeight=\"450\" d:DesignWidth=\"800\"",
                "Title=\"HyperlinkPage\"",
                "Foreground=\"{DynamicResource TextFillColorPrimaryBrush}\">");
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
            StringAssert.Contains(
                hyperlinkXaml.Replace("\r\n", "\n").Replace('\r', '\n'),
                "<TextBlock Margin=\"20\">\n                                <Hyperlink NavigateUri=\"https://www.microsoft.com\" RequestNavigate=\"Hyperlink_RequestNavigate\">\n                                    Hyperlink\n                                </Hyperlink>\n                        </TextBlock>");
            StringAssert.Contains(
                hyperlinkXaml.Replace("\r\n", "\n").Replace('\r', '\n'),
                "            </ScrollViewer>\n        </Grid>\n\n    </Grid>\n</Page>");
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
            AssertContainsInOrder(
                calendarXaml,
                "<controls:ControlExample Margin=\"10\" HeaderText=\"A basic Calendar control.\">",
                "<controls:ControlExample.XamlCode>",
                "&lt;Calendar/&gt;",
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
            AssertContainsInOrder(
                imageXaml,
                "<controls:ControlExample",
                "Margin=\"10\"",
                "HeaderText=\"Standand Image from a local file.\"",
                "XamlCode=\"&lt;Image Height=&quot;100&quot; Source=&quot;Assets\\MyImage.jpg&quot; /&gt;\"",
                "<Image",
                "Height=\"200\"",
                "HorizontalAlignment=\"Left\"",
                "Source=\"pack://application:,,,/ModernWpf.Gallery;component/Assets/win11-dashboard.png\" />");
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
            StringAssert.Contains(
                fileDialogsXaml.Replace("\r\n", "\n").Replace('\r', '\n'),
                "                </controls:ControlExample>\n\n            </StackPanel>\n        </ScrollViewer>\n    </Grid>\n</Page>");
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
                "HeaderText=\"Pick Multiple Files\"",
                "XamlCode=\"&lt;Button Content=&quot;Pick Multiple Files&quot; Click=&quot;PickMultipleFilesButton_Click&quot; /&gt;\"",
                "<Button",
                "Content=\"Pick multiple files\"",
                "Click=\"PickMultipleFilesButton_Click\"",
                "Margin=\"0,0,0,10\" />",
                "<TextBlock",
                "Text=\"{Binding ViewModel.MultipleFilesPath}\"",
                "TextWrapping=\"Wrap\" />",
                "HeaderText=\"Save File\"",
                "XamlCode=\"&lt;Button Content=&quot;Save File&quot; Click=&quot;SaveFileButton_Click&quot; /&gt;\"",
                "<TextBox",
                "Text=\"{Binding ViewModel.FileContent, UpdateSourceTrigger=PropertyChanged}\"",
                "AcceptsReturn=\"True\"",
                "TextWrapping=\"Wrap\"",
                "MinHeight=\"80\"",
                "Margin=\"0,0,0,10\"",
                "VerticalScrollBarVisibility=\"Auto\"",
                "AutomationProperties.Name=\"Save File Text Box\"",
                "AutomationProperties.HelpText=\"The text in the textbox will be saved to a file on button click\"/>",
                "<Button",
                "Content=\"Save a file\"",
                "Click=\"SaveFileButton_Click\"",
                "Margin=\"0,0,0,10\" />",
                "<TextBlock",
                "Text=\"{Binding ViewModel.SavedFilePath}\"",
                "TextWrapping=\"Wrap\" />",
                "HeaderText=\"Pick Folder\"",
                "XamlCode=\"&lt;Button Content=&quot;Pick Folder&quot; Click=&quot;PickFolderButton_Click&quot; /&gt;\"",
                "<Button",
                "Content=\"Pick a folder\"",
                "Click=\"PickFolderButton_Click\"",
                "Margin=\"0,0,0,10\" />",
                "<TextBlock",
                "Text=\"{Binding ViewModel.SelectedFolderPath}\"",
                "TextWrapping=\"Wrap\" />");

            var messageBoxXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "System",
                "MessageBoxPage.xaml");
            StringAssert.Contains(
                messageBoxXaml.Replace("\r\n", "\n").Replace('\r', '\n'),
                "                </controls:ControlExample>\n\n            </StackPanel>\n        </ScrollViewer>\n    </Grid>\n</Page>");
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
            var normalizedClipboardXaml = clipboardXaml.Replace("\r\n", "\n").Replace('\r', '\n');
            StringAssert.Contains(
                normalizedClipboardXaml,
                "<Border Grid.Row=\"1\"\n                Background=\"{DynamicResource SubtleFillColorSecondaryBrush}\"");
            StringAssert.Contains(
                normalizedClipboardXaml,
                "<TextBlock Grid.Column=\"0\"\n                           FontFamily=\"{StaticResource SymbolThemeFontFamily}\"");
            StringAssert.Contains(
                normalizedClipboardXaml,
                "<TextBlock Grid.Column=\"1\"\n                           TextWrapping=\"Wrap\"");
            StringAssert.Contains(
                normalizedClipboardXaml,
                "<Hyperlink NavigateUri=\"https://learn.microsoft.com/en-us/dotnet/desktop/winforms/migration/clipboard-dataobject-net10\"\n                               RequestNavigate=\"Hyperlink_RequestNavigate\">");
            AssertContainsInOrder(
                clipboardXaml,
                "<Border Grid.Row=\"1\"",
                "Background=\"{DynamicResource SubtleFillColorSecondaryBrush}\"",
                "BorderBrush=\"{DynamicResource AccentFillColorDefaultBrush}\"",
                "BorderThickness=\"1\"",
                "CornerRadius=\"4\"",
                "Padding=\"16,12\"",
                "Margin=\"0,0,0,16\">",
                "<TextBlock Grid.Column=\"0\"",
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
