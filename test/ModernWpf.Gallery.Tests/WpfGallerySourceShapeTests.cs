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
                var xamlFileName = Path.GetFileNameWithoutExtension(path);
                var className = Path.GetFileNameWithoutExtension(xamlFileName);
                var expectedBaseType = className == "FrameWindow"
                    ? "Window"
                    : className == "HeaderTile" || className == "TileGallery"
                        ? "UserControl"
                        : "Page";
                var declaration = "public partial class " + className + " : " + expectedBaseType;
                Assert.IsFalse(
                    source.Contains("public sealed partial class", StringComparison.Ordinal),
                    Path.GetRelativePath(repoRoot, path) + " should match the official WPF Gallery unsealed partial class shape.");
                Assert.IsTrue(
                    source.Contains(declaration, StringComparison.Ordinal),
                    Path.GetRelativePath(repoRoot, path) + " should keep the official WPF Gallery explicit code-behind base type shape.");
                AssertContainsInOrder(
                    source,
                    "/// Interaction logic for " + xamlFileName,
                    declaration);
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
        public void WpfGalleryPageViewModelProvidesObservableStateAdapter()
        {
            var observableSource = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "WpfGalleryObservableObject.cs");
            var source = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "WpfGalleryPageViewModel.cs");

            AssertContainsInOrder(
                observableSource,
                "public class WpfGalleryObservableObject : INotifyPropertyChanged",
                "public event PropertyChangedEventHandler PropertyChanged;",
                "protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)",
                "EqualityComparer<T>.Default.Equals(field, value)",
                "OnPropertyChanged(propertyName);",
                "protected void OnPropertyChanged([CallerMemberName] string propertyName = null)",
                "handler(this, new PropertyChangedEventArgs(propertyName));");
            AssertContainsInOrder(
                source,
                "public class WpfGalleryPageViewModel : WpfGalleryObservableObject",
                "private string _pageTitle;",
                "private string _pageDescription;",
                "public string PageTitle",
                "SetProperty(ref _pageTitle, value);",
                "public string PageDescription",
                "SetProperty(ref _pageDescription, value);");
            Assert.IsFalse(
                source.Contains("public event PropertyChangedEventHandler", StringComparison.Ordinal),
                "WpfGalleryPageViewModel should use the shared observable adapter instead of duplicating notification plumbing.");
            Assert.IsFalse(
                source.Contains("protected bool SetProperty<T>", StringComparison.Ordinal),
                "WpfGalleryPageViewModel should keep SetProperty on the shared observable adapter.");
        }

        [TestMethod]
        public void TopLevelWpfGalleryViewModelsKeepOfficialStateAndNavigateSourceShape()
        {
            var source = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "WpfGalleryNavigationPageViewModels.cs");

            AssertContainsInOrder(
                source,
                "public partial class DashboardPageViewModel : WpfGalleryPageViewModel",
                "private IReadOnlyList<GalleryGroup> _navigationCards = GalleryCatalog.OverviewGroups;",
                "private IReadOnlyList<GalleryItem> _recentlyAddedOrUpdatedSamplesInfo = GalleryCatalog.NewOrUpdatedItems;",
                "private readonly Action<object> _navigate;",
                "public DashboardPageViewModel(Action<object> navigate)",
                ": base(string.Empty, string.Empty)",
                "_navigate = navigate;",
                "NavigateCommand = new GalleryCommand(Navigate);",
                "public IReadOnlyList<GalleryGroup> NavigationCards",
                "SetProperty(ref _navigationCards, value ?? Array.Empty<GalleryGroup>());",
                "public IReadOnlyList<GalleryItem> RecentlyAddedOrUpdatedSamplesInfo",
                "SetProperty(ref _recentlyAddedOrUpdatedSamplesInfo, value ?? Array.Empty<GalleryItem>());",
                "public ICommand NavigateCommand { get; }",
                "public void Navigate(object pageType)",
                "_navigate(pageType);");
            AssertContainsInOrder(
                source,
                "public partial class WhatsNewPageViewModel : WpfGalleryPageViewModel",
                "private string _accentColorXamlCode = _accentColorBrushApiXamlUsage;",
                "private string _hyphenBasedLigatureXamlCode = _hyphenBasedLiagatureXamlUsage;",
                "private string _gridShorthandSyntaxXamlCode = _gridShorthandSyntaxXamlUsage;",
                "private readonly Action<object> _navigate;",
                "public WhatsNewPageViewModel(Action<object> navigate)",
                ": base(\"What's new in WPF\", \"Discover all the new features, enhancements and APIs introduced in WPF\")",
                "_navigate = navigate;",
                "NavigateCommand = new GalleryCommand(Navigate);",
                "public string AccentColorXamlCode",
                "SetProperty(ref _accentColorXamlCode, value);",
                "public string HyphenBasedLigatureXamlCode",
                "SetProperty(ref _hyphenBasedLigatureXamlCode, value);",
                "public string GridShorthandSyntaxXamlCode",
                "SetProperty(ref _gridShorthandSyntaxXamlCode, value);",
                "public ICommand NavigateCommand { get; }",
                "public void Navigate(object pageType)",
                "_navigate(pageType);",
                "private const string _accentColorBrushApiXamlUsage =",
                "private const string _hyphenBasedLiagatureXamlUsage =",
                "private const string _gridShorthandSyntaxXamlUsage =");
        }

        [TestMethod]
        public void SettingsViewModelKeepsOfficialObservableTitleSourceShape()
        {
            var source = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "SettingsPage.xaml.cs");

            AssertContainsInOrder(
                source,
                "using ModernWpf.Gallery.Pages.WpfGallery;",
                "public partial class SettingsPageViewModel : WpfGalleryPageViewModel",
                "public SettingsPageViewModel()",
                ": base(\"Settings\", null)");
            Assert.IsFalse(
                source.Contains("public string PageTitle", StringComparison.Ordinal),
                "Settings should reuse the shared observable page-title adapter instead of a computed PageTitle getter.");
            Assert.IsFalse(
                source.Contains("public string PageDescription", StringComparison.Ordinal),
                "Settings should reuse the shared observable page-description adapter instead of a computed PageDescription getter.");
        }

        [TestMethod]
        public void DesignGuidanceViewModelsKeepOfficialObservableStateSourceShape()
        {
            var simpleSource = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "DesignGuidance",
                "DesignGuidancePageViewModels.cs");

            AssertContainsInOrder(
                simpleSource,
                "public partial class ColorsPageViewModel : WpfGalleryPageViewModel",
                ": base(\"Colors\", \"Guide showing how to use colors in your app\")",
                "public partial class TypographyPageViewModel : WpfGalleryPageViewModel",
                ": base(\"Typography\", \"Guide showing how to use typography in your app\")",
                "public partial class SpacingPageViewModel : WpfGalleryPageViewModel",
                ": base(\"Spacing\", \"Guide showing how to use spacing in your app\")",
                "public partial class GeometryPageViewModel : WpfGalleryPageViewModel",
                ": base(\"Geometry\", string.Empty)");

            var iconographySource = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "DesignGuidance",
                "IconographyPageViewModel.cs");

            AssertContainsInOrder(
                iconographySource,
                "public partial class IconographyPageViewModel : WpfGalleryPageViewModel",
                "private ICollection<IconData> _allIcons = new List<IconData>();",
                "private IconData _selectedIcon;",
                "private string _searchText = string.Empty;",
                "private ObservableCollection<IconData> _searchFilteredIcons = new ObservableCollection<IconData>();",
                "private ObservableCollection<IconData> _displayedIcons = new ObservableCollection<IconData>();",
                "private int _currentPage = 1;",
                "private int _totalPages = 1;",
                "private int _selectedPageSizeIndex = 1;",
                "public IconographyPageViewModel()",
                ": base(\"Icons\", \"Guide showing how to use icons in your application.\")",
                "public ICollection<IconData> AllIcons",
                "SetProperty(ref _allIcons, value ?? new List<IconData>());",
                "public ObservableCollection<IconData> SearchFilteredIcons",
                "SetProperty(ref _searchFilteredIcons, value ?? new ObservableCollection<IconData>());",
                "public ObservableCollection<IconData> DisplayedIcons",
                "SetProperty(ref _displayedIcons, value ?? new ObservableCollection<IconData>());",
                "public IconData SelectedIcon",
                "SetProperty(ref _selectedIcon, value);",
                "public string SearchText",
                "if (SetProperty(ref _searchText, value))",
                "public List<string> PageSizeOptions { get; } = new List<string> { \"100\", \"250\", \"500\", \"1000\", \"All\" };",
                "AllIcons = ReadIconData().ToList();",
                "SelectedIcon = AllIcons.FirstOrDefault();",
                "SearchFilteredIcons = new ObservableCollection<IconData>(AllIcons);",
                "SearchFilteredIcons.Clear();",
                "var searchFilteredIconData = AllIcons.Where(icon =>",
                "private void ApplyTagFilter(string tag)",
                "var trimmedTag = tag.Trim();",
                "if (string.Equals(trimmedTag, SearchText, StringComparison.Ordinal))",
                "SearchText = trimmedTag;");
            Assert.IsFalse(
                iconographySource.Contains("public event PropertyChangedEventHandler", StringComparison.Ordinal),
                "Iconography should use the shared observable page-view-model adapter instead of local event plumbing.");
            Assert.IsFalse(
                iconographySource.Contains("private void OnPropertyChanged", StringComparison.Ordinal),
                "Iconography should use the shared SetProperty adapter instead of local OnPropertyChanged plumbing.");
        }

        [TestMethod]
        public void SystemViewModelsKeepOfficialObservableStateSourceShape()
        {
            var source = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "System",
                "SystemPageViewModels.cs");

            AssertContainsInOrder(
                source,
                "public abstract class SystemPageViewModelBase : WpfGalleryPageViewModel",
                "protected SystemPageViewModelBase(string pageTitle, string pageDescription)",
                ": base(pageTitle, pageDescription)",
                "public partial class FileAndFolderDialogsPageViewModel : SystemPageViewModelBase",
                "private string _singleFilePath = \"No file selected\";",
                "private string _multipleFilesPath = \"No files selected\";",
                "private string _fileContent = \"Enter text here to save to a file...\";",
                "private string _savedFilePath = \"No file saved\";",
                "private string _selectedFolderPath = \"No folder selected\";",
                ": base(",
                "\"File and Folder Dialogs\",",
                "\"Use the OpenFileDialog, SaveFileDialog, and OpenFolderDialog to let users select files and folders in a secure way.\")",
                "public string SingleFilePath",
                "SetProperty(ref _singleFilePath, value);",
                "public string MultipleFilesPath",
                "SetProperty(ref _multipleFilesPath, value);",
                "public string FileContent",
                "SetProperty(ref _fileContent, value);",
                "public string SavedFilePath",
                "SetProperty(ref _savedFilePath, value);",
                "public string SelectedFolderPath",
                "SetProperty(ref _selectedFolderPath, value);");
            AssertContainsInOrder(
                source,
                "public partial class MessageBoxPageViewModel : SystemPageViewModelBase",
                "private string _defaultMessageResult = \"No message shown yet\";",
                "private string _customTitleResult = \"No message shown yet\";",
                "private int _selectedButtonIndex = 0;",
                "private string _differentButtonsResult = \"No button clicked yet\";",
                "private string _differentButtonsXamlCode = \"<Button Content=\\\"Show MessageBox\\\" Click=\\\"ShowMessageBoxButton_Click\\\" />\";",
                "private string _differentButtonsCSharpCode = string.Format(_differentButtonsMessageBoxSampleCSharpCodeString, \"\\tMessageBox.Show(\\\"Message\\\", \\\"Title\\\", MessageBoxButton.OK);\");",
                "private int _selectedImageIndex = 0;",
                "private string _differentImagesResult = \"No image example shown yet\";",
                "private string _differentImagesXamlCode = \"<Button Content=\\\"Show MessageBox\\\" Click=\\\"ShowMessageButton_Click\\\" />\";",
                "private string _differentImagesCSharpCode = string.Format(_differentImagesMessageBoxSampleCSharpCodeString, \"\\tMessageBox.Show(\\\"Message\\\", \\\"Title\\\", MessageBoxButton.OK, MessageBoxImage.None);\");",
                "private string _commonMessagesResult = \"No common message shown yet\";",
                "private string _commonMessagesXamlCode = \"<WrapPanel Margin=\\\"0,0,0,10\\\">\\n\" +",
                "private string _commonMessagesCSharpCode = \"// Information\\n\" +",
                "private string _customDefaultResult = \"No selection made\";",
                "public string DifferentButtonsXamlCode",
                "private set { SetProperty(ref _differentButtonsXamlCode, value); }",
                "public string DifferentImagesXamlCode",
                "private set { SetProperty(ref _differentImagesXamlCode, value); }",
                "public string CommonMessagesXamlCode",
                "private set { SetProperty(ref _commonMessagesXamlCode, value); }",
                "public string CommonMessagesCSharpCode",
                "private set { SetProperty(ref _commonMessagesCSharpCode, value); }",
                "private const string _differentButtonsMessageBoxSampleCSharpCodeString =",
                "private const string _differentImagesMessageBoxSampleCSharpCodeString =");
            AssertContainsInOrder(
                source,
                "public partial class ClipboardPageViewModel : SystemPageViewModelBase",
                "private string _copyStatus = string.Empty;",
                "private string _pastedText = string.Empty;",
                "private string _clearStatus = string.Empty;",
                "private string _formatsInfo = string.Empty;",
                "private string _copyImageStatus = string.Empty;",
                "private string _pasteImageStatus = string.Empty;",
                "public ClipboardPageViewModel()",
                ": base(\"Clipboard\", string.Empty)");
            Assert.IsFalse(
                source.Contains("public event PropertyChangedEventHandler", StringComparison.Ordinal),
                "System view models should use the shared observable page-view-model adapter instead of local event plumbing.");
            Assert.IsFalse(
                source.Contains("private void OnPropertyChanged", StringComparison.Ordinal),
                "System view models should use the shared SetProperty adapter instead of local OnPropertyChanged plumbing.");
        }

        [TestMethod]
        public void WpfGalleryNavigationViewModelsKeepOfficialStateAndNavigateSourceShape()
        {
            var source = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "WpfGalleryNavigationPageViewModels.cs");

            AssertContainsInOrder(
                source,
                "public class WpfGalleryNavigationPageViewModel : WpfGalleryPageViewModel",
                "private IReadOnlyList<GalleryItem> _navigationCards;",
                "private readonly Action<object> _navigate;",
                "public WpfGalleryNavigationPageViewModel(",
                ": base(pageTitle, pageDescription)",
                "_navigationCards = navigationCards ?? Array.Empty<GalleryItem>();",
                "_navigate = navigate;",
                "NavigateCommand = new GalleryCommand(Navigate);",
                "public IReadOnlyList<GalleryItem> NavigationCards",
                "SetProperty(ref _navigationCards, value ?? Array.Empty<GalleryItem>());",
                "public ICommand NavigateCommand { get; }",
                "public void Navigate(object pageType)",
                "_navigate(pageType);");
            foreach (var className in new[]
            {
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
            })
            {
                StringAssert.Contains(
                    source,
                    "public partial class " + className + " : WpfGalleryNavigationPageViewModel");
            }
        }

        [TestMethod]
        public void TextViewModelsKeepOfficialTextBoxValidatedTextSourceShape()
        {
            var source = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Text",
                "TextPageViewModels.cs");

            AssertContainsInOrder(
                source,
                "public partial class TextBoxPageViewModel : WpfGalleryPageViewModel",
                "private string _validatedText = string.Empty;",
                "public TextBoxPageViewModel()",
                ": base(\"TextBox\", string.Empty)",
                "public string ValidatedText",
                "get { return _validatedText; }",
                "set { SetProperty(ref _validatedText, value); }");
        }

        [TestMethod]
        public void TextValidationRuleKeepsOfficialAlphabeticSourceShape()
        {
            var source = ReadRepoFile(
                "ModernWpf.Gallery",
                "Helpers",
                "AlphabeticValidationRule.cs");

            AssertContainsInOrder(
                source,
                "/// Validation rule that ensures the input contains only English alphabetic characters (a-z, A-Z).",
                "public class AlphabeticValidationRule : ValidationRule",
                "var input = value as string;",
                "if (string.IsNullOrEmpty(input))",
                "// Check if the input contains only English alphabetic characters (a-z, A-Z)",
                "if (!Regex.IsMatch(input, @\"^[a-zA-Z]+$\"))",
                "return new ValidationResult(false, \"Only English alphabetic characters (a-z, A-Z) are allowed.\");");
        }

        [TestMethod]
        public void SimpleItemViewModelsKeepOfficialObservableTitleSourceShape()
        {
            var dateSource = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "DateAndTime",
                "DateAndTimePageViewModels.cs");
            var mediaSource = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Media",
                "MediaPageViewModels.cs");
            var statusSource = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "StatusAndInfo",
                "StatusAndInfoPageViewModels.cs");
            var layoutSource = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Layout",
                "LayoutPageViewModels.cs");
            var navigationSource = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Navigation",
                "NavigationPageViewModels.cs");
            var textSource = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Text",
                "TextPageViewModels.cs");

            AssertContainsInOrder(
                dateSource,
                "public partial class CalendarPageViewModel : WpfGalleryPageViewModel",
                ": base(\"Calendar\", string.Empty)",
                "public partial class DatePickerPageViewModel : WpfGalleryPageViewModel",
                ": base(\"DatePicker\", string.Empty)");
            AssertContainsInOrder(
                mediaSource,
                "public partial class CanvasPageViewModel : WpfGalleryPageViewModel",
                ": base(\"Canvas\", string.Empty)",
                "public partial class ImagePageViewModel : WpfGalleryPageViewModel",
                ": base(\"Image\", string.Empty)");
            AssertContainsInOrder(
                statusSource,
                "public partial class ProgressBarPageViewModel : WpfGalleryPageViewModel",
                ": base(\"ProgressBar\", string.Empty)",
                "public partial class ToolTipPageViewModel : WpfGalleryPageViewModel",
                ": base(\"ToolTip\", string.Empty)");
            AssertContainsInOrder(
                layoutSource,
                "public partial class BorderPageViewModel : WpfGalleryPageViewModel",
                ": base(\"Border\", string.Empty)",
                "public partial class ExpanderPageViewModel : WpfGalleryPageViewModel",
                ": base(\"Expander\", string.Empty)",
                "public partial class GridPageViewModel : WpfGalleryPageViewModel",
                ": base(\"Grid\", string.Empty)",
                "public partial class GridSplitterPageViewModel : WpfGalleryPageViewModel",
                ": base(\"GridSplitter\", string.Empty)",
                "public partial class GroupBoxPageViewModel : WpfGalleryPageViewModel",
                ": base(\"GroupBox\", string.Empty)",
                "public partial class ResizeGripPageViewModel : WpfGalleryPageViewModel",
                ": base(\"ResizeGrip\", string.Empty)",
                "public partial class StackPanelPageViewModel : WpfGalleryPageViewModel",
                ": base(\"StackPanel\", string.Empty)");
            AssertContainsInOrder(
                navigationSource,
                "public partial class MenuPageViewModel : WpfGalleryPageViewModel",
                ": base(\"Menu\", string.Empty)",
                "public partial class TabControlPageViewModel : WpfGalleryPageViewModel",
                ": base(\"TabControl\", string.Empty)",
                "public partial class FramePageViewModel : WpfGalleryPageViewModel",
                ": base(\"Frame\", string.Empty)",
                "public partial class NavigationWindowPageViewModel : WpfGalleryPageViewModel",
                ": base(\"Navigation Window\", string.Empty)");
            AssertContainsInOrder(
                textSource,
                "public partial class LabelPageViewModel : WpfGalleryPageViewModel",
                ": base(\"Label\", string.Empty)",
                "public partial class TextBoxPageViewModel : WpfGalleryPageViewModel",
                ": base(\"TextBox\", string.Empty)",
                "public partial class TextBlockPageViewModel : WpfGalleryPageViewModel",
                ": base(\"TextBlock\", string.Empty)",
                "public partial class HyperlinkPageViewModel : WpfGalleryPageViewModel",
                ": base(\"Hyperlink\", string.Empty)",
                "public partial class RichTextEditPageViewModel : WpfGalleryPageViewModel",
                ": base(\"RichTextEdit\", string.Empty)",
                "public partial class PasswordBoxPageViewModel : WpfGalleryPageViewModel",
                ": base(\"PasswordBox\", string.Empty)");

            foreach (var source in new[] { dateSource, mediaSource, statusSource, layoutSource, navigationSource, textSource })
            {
                Assert.IsFalse(
                    source.Contains("public event PropertyChangedEventHandler", StringComparison.Ordinal),
                    "Simple copied item view models should use the shared observable page-view-model adapter.");
                Assert.IsFalse(
                    source.Contains("private void OnPropertyChanged", StringComparison.Ordinal),
                    "Simple copied item view models should keep OnPropertyChanged on the shared observable adapter.");
                Assert.IsFalse(
                    source.Contains("private bool SetProperty", StringComparison.Ordinal),
                    "Simple copied item view models should keep SetProperty on the shared observable adapter.");
            }
        }

        [TestMethod]
        public void BasicInputViewModelsKeepOfficialStateAndCommandSourceShape()
        {
            var source = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "BasicInput",
                "BasicInputPageViewModels.cs").Replace("\r\n", "\n").Replace('\r', '\n');

            AssertContainsInOrder(
                source,
                "public abstract class BasicInputPageViewModelBase : WpfGalleryPageViewModel",
                "protected BasicInputPageViewModelBase(string pageTitle)",
                ": base(pageTitle, string.Empty)",
                "protected static ICommand CreateCommand(Action<object> execute)",
                "public partial class ButtonPageViewModel : BasicInputPageViewModelBase",
                "private string _message = \"Hello World!\";",
                "private bool _isSimpleButtonEnabled = true;",
                "private bool _isUiButtonEnabled = true;",
                "SimpleButtonCheckboxCheckedCommand = CreateCommand(OnSimpleButtonCheckboxChecked);",
                "UiButtonCheckboxCheckedCommand = CreateCommand(OnUiButtonCheckboxChecked);",
                "public ICommand SimpleButtonCheckboxCheckedCommand { get; }",
                "public ICommand UiButtonCheckboxCheckedCommand { get; }",
                "public string Message",
                "SetProperty(ref _message, value);",
                "private void OnSimpleButtonCheckboxChecked(object sender)",
                "if (sender is not CheckBox checkbox)",
                "IsSimpleButtonEnabled = !(checkbox?.IsChecked ?? false);",
                "private void OnUiButtonCheckboxChecked(object sender)",
                "if (sender is not CheckBox checkbox)",
                "IsUiButtonEnabled = !(checkbox?.IsChecked ?? false);");
            AssertContainsInOrder(
                source,
                "public partial class CheckBoxPageViewModel : BasicInputPageViewModelBase",
                "private bool? _selectAllCheckBoxChecked = null;",
                "private bool _optionOneCheckBoxChecked = false;",
                "private bool _optionTwoCheckBoxChecked = true;",
                "private bool _optionThreeCheckBoxChecked = false;",
                "private void OnSelectAllChecked(object sender)",
                "if (sender is not CheckBox checkBox)",
                "checkBox.IsChecked = !(\n                    OptionOneCheckBoxChecked && OptionTwoCheckBoxChecked && OptionThreeCheckBoxChecked\n                );",
                "private void OnSingleChecked(object option)",
                "if (OptionOneCheckBoxChecked && OptionTwoCheckBoxChecked && OptionThreeCheckBoxChecked)",
                "SelectAllCheckBoxChecked = true;",
                "else if (!OptionOneCheckBoxChecked && !OptionTwoCheckBoxChecked && !OptionThreeCheckBoxChecked)",
                "SelectAllCheckBoxChecked = false;");
            AssertContainsInOrder(
                source,
                "public partial class ComboBoxPageViewModel : BasicInputPageViewModelBase",
                "private IList<string> _comboBoxFontFamilies = new ObservableCollection<string>",
                "\"Arial\",",
                "\"Comic Sans MS\",",
                "\"Segoe UI\",",
                "\"Times New Roman\"",
                "private IList<int> _comboBoxFontSizes = new ObservableCollection<int>",
                "8,",
                "72",
                "public IList<string> ComboBoxFontFamilies",
                "SetProperty(ref _comboBoxFontFamilies, value);",
                "public IList<int> ComboBoxFontSizes",
                "SetProperty(ref _comboBoxFontSizes, value);");
            AssertContainsInOrder(
                source,
                "public partial class RadioButtonPageViewModel : BasicInputPageViewModelBase",
                "private bool _isRadioButtonEnabled = true;",
                "private void OnRadioButtonCheckboxChecked(object sender)",
                "if (sender is not CheckBox checkbox)",
                "IsRadioButtonEnabled = !(checkbox?.IsChecked ?? false);");
            AssertContainsInOrder(
                source,
                "public partial class SliderPageViewModel : BasicInputPageViewModelBase",
                "private int _simpleSliderValue = 0;",
                "private int _rangeSliderValue = 500;",
                "private int _marksSliderValue = 0;",
                "private int _verticalSliderValue = 0;");
            Assert.IsFalse(
                source.Contains("public event PropertyChangedEventHandler", StringComparison.Ordinal),
                "Basic Input view models should use the shared observable page-view-model adapter instead of local event plumbing.");
            Assert.IsFalse(
                source.Contains("private void OnPropertyChanged", StringComparison.Ordinal),
                "Basic Input view models should use the shared SetProperty adapter instead of local OnPropertyChanged plumbing.");
        }

        [TestMethod]
        public void CollectionsViewModelsKeepOfficialConstructorAndSelectionModeSourceShape()
        {
            var source = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Collections",
                "CollectionsPageViewModels.cs");

            AssertContainsInOrder(
                source,
                "public abstract class CollectionsPageViewModelBase : WpfGalleryPageViewModel",
                "protected CollectionsPageViewModelBase(string pageTitle)",
                ": base(pageTitle, string.Empty)",
                "public partial class DataGridPageViewModel : CollectionsPageViewModelBase",
                "private ObservableCollection<Product> _productsCollection;",
                "public DataGridPageViewModel()",
                "_productsCollection = GenerateProducts();",
                "public ObservableCollection<Product> ProductsCollection");
            AssertContainsInOrder(
                source,
                "public partial class ListBoxPageViewModel : CollectionsPageViewModelBase",
                "private ObservableCollection<string> _listBoxItems;",
                "public ListBoxPageViewModel()",
                "_listBoxItems = new ObservableCollection<string>",
                "\"Arial\",",
                "\"Times New Roman\"");
            AssertContainsInOrder(
                source,
                "public partial class ListViewPageViewModel : CollectionsPageViewModelBase",
                "private int _listViewSelectionModeComboBoxSelectedIndex = 0;",
                "public int ListViewSelectionModeComboBoxSelectedIndex",
                "SetProperty(ref _listViewSelectionModeComboBoxSelectedIndex, value);",
                "UpdateListViewSelectionMode(value);",
                "private SelectionMode _listViewSelectionMode = SelectionMode.Single;",
                "private ObservableCollection<Person> _basicListViewItems;",
                "private ObservableCollection<Person> _gridViewItems;",
                "public ListViewPageViewModel()",
                "_basicListViewItems = GenerateBasicListViewPersons();",
                "_gridViewItems = GenerateGridViewPersons();",
                "private void UpdateListViewSelectionMode(int selectionModeIndex)",
                "ListViewSelectionMode = selectionModeIndex switch",
                "1 => SelectionMode.Multiple,",
                "2 => SelectionMode.Extended,",
                "_ => SelectionMode.Single");
            Assert.IsFalse(
                source.Contains("public event PropertyChangedEventHandler", StringComparison.Ordinal),
                "Collections view models should use the shared observable page-view-model adapter instead of local event plumbing.");
            Assert.IsFalse(
                source.Contains("private void OnPropertyChanged", StringComparison.Ordinal),
                "Collections view models should use the shared SetProperty adapter instead of local OnPropertyChanged plumbing.");
        }

        [TestMethod]
        public void CollectionsViewModelsKeepOfficialSampleGenerationSourceShape()
        {
            var source = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Collections",
                "CollectionsPageViewModels.cs");

            AssertContainsInOrder(
                source,
                "protected static ObservableCollection<Product> GenerateProducts()",
                "var random = CreateSampleRandom(ProductsVisualTestSeed);",
                "var products = new ObservableCollection<Product> { };",
                "var adjectives = new[] { \"Red\", \"Blueberry\" };",
                "var names = new[] { \"Marmalade\", \"Dumplings\", \"Soup\" };",
                "//var units = new[] { \"grams\", \"kilograms\", \"milliliters\" };",
                "for (int i = 0; i < 50; i++)",
                "products.Add(",
                "new Product",
                "ProductName =",
                "adjectives[random.Next(0, adjectives.Length)]",
                "+ \" \"",
                "+ names[random.Next(0, names.Length)],",
                "UnitPrice = Math.Round(random.NextDouble() * 20.0, 3)",
                "return products;");
            AssertContainsInOrder(
                source,
                "protected static ObservableCollection<Person> GenerateBasicListViewPersons()",
                "return GeneratePersons(BasicListViewVisualTestSeed);",
                "protected static ObservableCollection<Person> GenerateGridViewPersons()",
                "return GeneratePersons(GridViewVisualTestSeed);",
                "private static ObservableCollection<Person> GeneratePersons(int visualTestSeed)",
                "var random = CreateSampleRandom(visualTestSeed);",
                "var persons = new ObservableCollection<Person>();",
                "for (int i = 0; i < 50; i++)",
                "persons.Add(",
                "new Person(",
                "names[random.Next(0, names.Length)],",
                "surnames[random.Next(0, surnames.Length)],",
                "companies[random.Next(0, companies.Length)]",
                "return persons;");
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
                    UnsealedDeclaration = "public record Person"
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
        public void CopiedWpfGalleryProductModelKeepsOfficialSummaryAndPlaceholderShape()
        {
            var source = ReadRepoFile(
                "ModernWpf.Gallery",
                "Models",
                "Product.cs");

            AssertContainsInOrder(
                source,
                "/// <summary>",
                "/// Product class for DataGrid page",
                "/// </summary>",
                "public class Product",
                "public int ProductId { get; set; }",
                "public int ProductCode { get; set; }",
                "public string ProductName { get; set; }",
                "public string QuantityPerUnit { get; set; }",
                "public double UnitPrice { get; set; }",
                "// public string UnitPriceString => UnitPrice.ToString(\"F2\");",
                "public int UnitsInStock { get; set; }",
                "// public bool IsVirtual { get; set; }");
        }

        [TestMethod]
        public void CopiedWpfGalleryPersonModelKeepsOfficialRecordAndInitShape()
        {
            var source = ReadRepoFile(
                "ModernWpf.Gallery",
                "Models",
                "Person.cs");

            Assert.IsTrue(
                source.Contains("public record Person", StringComparison.Ordinal),
                "Person should match the official WPF Gallery record model declaration shape.");
            Assert.IsFalse(
                source.Contains("public class Person", StringComparison.Ordinal),
                "Person should not drift back to a local-only class declaration.");
            AssertContainsInOrder(
                source,
                "public string FirstName { get; init; }",
                "public string LastName { get; init; }",
                "public string Name => FirstName + \" \" + LastName;",
                "public string Company { get; init; }",
                "public Person(string firstName, string lastName, string company)");
        }

        [TestMethod]
        public void CopiedWpfGalleryIconDataModelKeepsOfficialPropertyAndGlyphShape()
        {
            var source = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "DesignGuidance",
                "IconData.cs");

            AssertContainsInOrder(
                source,
                "[DataMember]",
                "public string Name { get; set; }",
                "[DataMember]",
                "public string Code { get; set; }",
                "[DataMember]",
                "public List<string> Tags { get; set; } = [];",
                "public string Character => char.ConvertFromUtf32(Convert.ToInt32(Code, 16));",
                "public string CodeGlyph => \"\\\\x\" + Code;",
                "public string TextGlyph => \"&#x\" + Code + \";\";");
            Assert.IsFalse(
                source.Contains("catch (Exception)", StringComparison.Ordinal),
                "IconData.Character should keep the official expression-bodied glyph conversion shape.");
        }

        [TestMethod]
        public void CopiedWpfGalleryUserDashboardUserKeepsOfficialMemberOrderShape()
        {
            var source = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Samples",
                "UserDashboardUser.cs");

            AssertContainsInOrder(
                source,
                "private string _firstName;",
                "private string _lastName;",
                "private string _company;",
                "private string _address;",
                "private bool _isNewGraduate;",
                "private string _imageId = \"91\";",
                "private int _age;",
                "private DateTime _dateOfJoining;",
                "public string FirstName",
                "public string LastName",
                "public string Name => $\"{FirstName} {LastName}\";",
                "public string ImageId",
                "public string ImageKey => $\"p{ImageId}\";",
                "public string Company",
                "public string Address",
                "public int Age",
                "public DateTime DateOfJoining",
                "public bool IsNewGraduate",
                "public event PropertyChangedEventHandler PropertyChanged;",
                "protected void OnPropertyChanged(string propertyName)",
                "PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));",
                "public UserDashboardUser(string firstName, string lastName)",
                "public UserDashboardUser(UserDashboardUser user)",
                "public UserDashboardUser(string imageId, string firstName, string lastName, string company, string address, int age, DateTime dateOfJoining, bool isNewGraduate)");
            AssertContainsInOrder(
                source,
                "if (SetProperty(ref _firstName, value, nameof(FirstName)))",
                "OnPropertyChanged(nameof(Name));",
                "if (SetProperty(ref _lastName, value, nameof(LastName)))",
                "OnPropertyChanged(nameof(Name));",
                "if (SetProperty(ref _imageId, value, nameof(ImageId)))",
                "OnPropertyChanged(nameof(ImageKey));");
        }

        [TestMethod]
        public void UserDashboardViewModelKeepsOfficialObservableStateSourceShape()
        {
            var source = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Samples",
                "UserDashboardPageViewModel.cs");

            AssertContainsInOrder(
                source,
                "public partial class UserDashboardPageViewModel : WpfGalleryObservableObject",
                "private const int UsersVisualTestSeed = 32043;",
                "private ObservableCollection<UserDashboardUser> _users;",
                "private UserDashboardUser _selectedUser;",
                "private bool _isEditing;",
                "private UserDashboardUser _editableUser;",
                "private bool _isReadOnly = true;",
                "private bool _isSaved;",
                "private string _deletedName = string.Empty;",
                "private readonly RelayCommand _addUserCommand;",
                "private readonly DispatcherTimer _deletedMessageTimer;",
                "public UserDashboardPageViewModel()",
                "Users = GenerateUsers();",
                "_addUserCommand = new RelayCommand(delegate { AddUser(); });",
                "_deletedMessageTimer = CreateMessageTimer(delegate { DeletedName = string.Empty; });",
                "public string DeletedName",
                "if (SetProperty(ref _deletedName, value, \"DeletedName\") && !string.IsNullOrEmpty(value))",
                "public UserDashboardUser EditableUser",
                "set { SetProperty(ref _editableUser, value, \"EditableUser\"); }",
                "public bool IsEditing",
                "set { SetProperty(ref _isEditing, value, \"IsEditing\"); }",
                "public bool IsReadOnly",
                "set { SetProperty(ref _isReadOnly, value, \"IsReadOnly\"); }",
                "public bool IsSaved",
                "if (SetProperty(ref _isSaved, value, \"IsSaved\") && value)",
                "public UserDashboardUser SelectedUser",
                "if (SetProperty(ref _selectedUser, value, \"SelectedUser\") && value != null && value != EditableUser)",
                "public ObservableCollection<UserDashboardUser> Users",
                "set { SetProperty(ref _users, value, \"Users\"); }");
            Assert.IsFalse(
                source.Contains("public event PropertyChangedEventHandler", StringComparison.Ordinal),
                "UserDashboardPageViewModel should use the shared observable adapter instead of local notification plumbing.");
            Assert.IsFalse(
                source.Contains("private void OnPropertyChanged", StringComparison.Ordinal),
                "UserDashboardPageViewModel should keep OnPropertyChanged on the shared observable adapter.");
            Assert.IsFalse(
                source.Contains("private bool SetProperty", StringComparison.Ordinal),
                "UserDashboardPageViewModel should keep SetProperty on the shared observable adapter.");
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
                "<Style x:Key=\"GalleryTitleBarDefaultButtonStyle\" BasedOn=\"{StaticResource GalleryTitleBarButtonStyle}\" TargetType=\"Button\">",
                "<Setter Property=\"winShell:WindowChrome.IsHitTestVisibleInChrome\" Value=\"True\" />",
                "<Setter Property=\"Border.CornerRadius\" Value=\"0\" />",
                "<Style x:Key=\"GalleryTitleBarDefaultCloseButtonStyle\" BasedOn=\"{StaticResource GalleryTitleBarDefaultButtonStyle}\" TargetType=\"Button\">");
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
                "BorderThickness=\"8 1 8 8\"",
                "Grid.Row=\"0\"",
                "Grid.ColumnSpan=\"2\"",
                "Height=\"44\"");
            Assert.IsFalse(
                mainWindowXaml.Contains("Background=\"{DynamicResource WindowBackground}\"", StringComparison.Ordinal),
                "MainWindow should keep the official WPF Gallery source shape by applying WindowBackground from code-behind instead of the Window root declaration.");
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
                "Command=\"{Binding Value.ViewModel.SettingsCommand, Source={StaticResource NavigationRootDataContextProxy}}\"",
                "Style=\"{StaticResource GalleryNavigationFooterButtonStyle}\"",
                "Click=\"OnSettingsButtonClick\"",
                "<StackPanel Orientation=\"Horizontal\" Margin=\"11,0,0,0\">");
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
                "/// Interaction logic for MainWindow.xaml",
                "InitializeComponent();",
                "UpdateWindowBackground();",
                "ConfigureWindowChrome();",
                "UpdateMainWindowVisuals();",
                "SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;",
                "StateChanged += OnWindowStateChanged;",
                "Activated += OnWindowActivationChanged;",
                "Deactivated += OnWindowActivationChanged;",
                "private void UpdateWindowBackground()",
                "SetResourceReference(BackgroundProperty, \"WindowBackground\");",
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
                "private void OnActualApplicationThemeChanged(ThemeManager sender, object args)",
                "AlignNavigationViewShellResourcesWithWpfGallery();",
                "private void OnSystemParametersChanged(object sender, PropertyChangedEventArgs e)",
                "if (string.Equals(e.PropertyName, nameof(SystemParameters.HighContrast), StringComparison.Ordinal))",
                "AlignNavigationViewShellResourcesWithWpfGallery();");

            var appCode = ReadRepoFile(
                "ModernWpf.Gallery",
                "App.xaml.cs");

            AssertContainsInOrder(
                appCode,
                "/// Interaction logic for App.xaml",
                "protected override void OnStartup(StartupEventArgs e)",
                "ApplyTheme(options.Theme);",
                "var window = new MainWindow();");
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
        public void ActiveGalleryXamlAvoidsLocalOnlyAutomationHooks()
        {
            var repoRoot = GetRepoRoot();
            var activeXamlRoots = new[]
            {
                Path.Combine(repoRoot, "ModernWpf.Gallery", "Controls"),
                Path.Combine(repoRoot, "ModernWpf.Gallery", "Pages"),
                Path.Combine(repoRoot, "ModernWpf.Gallery", "Resources")
            };
            var activeXamlFiles = activeXamlRoots
                .SelectMany(root => Directory.EnumerateFiles(root, "*.xaml", SearchOption.AllDirectories))
                .Concat(new[] { Path.Combine(repoRoot, "ModernWpf.Gallery", "MainWindow.xaml") });
            var shellXamlFiles = Directory.EnumerateFiles(
                Path.Combine(repoRoot, "ModernWpf.Gallery", "Shell"),
                "*.xaml",
                SearchOption.AllDirectories);
            var forbiddenSnippets = new[]
            {
                "AutomationProperties.AutomationId=",
                "x:Name=\"ContentRootGrid\"",
                "x:Name=\"GallerySampleHost\"",
                "x:Name=\"AllControlsItemsControl\"",
                "x:Name=\"GroupItemsControl\"",
                "AutomationProperties.Name=\"GalleryItemPageTitle\"",
                "GalleryNav_",
                "ModernWpfGalleryMainWindow",
                "GalleryNavigationRoot",
                "GalleryNavigationView",
                "GalleryContentHost",
                "SettingsIcon"
            };
            var shellForbiddenSnippets = forbiddenSnippets
                .Where(snippet => !string.Equals(snippet, "AutomationProperties.AutomationId=", StringComparison.Ordinal))
                .ToArray();
            var violations = activeXamlFiles
                .SelectMany(path =>
                {
                    var source = File.ReadAllText(path);
                    return forbiddenSnippets
                        .Where(snippet => source.Contains(snippet, StringComparison.Ordinal))
                        .Select(snippet => Path.GetRelativePath(repoRoot, path) + ": " + snippet);
                })
                .Concat(shellXamlFiles.SelectMany(path =>
                {
                    var source = File.ReadAllText(path);
                    return shellForbiddenSnippets
                        .Where(snippet => source.Contains(snippet, StringComparison.Ordinal))
                        .Select(snippet => Path.GetRelativePath(repoRoot, path) + ": " + snippet);
                }))
                .OrderBy(violation => violation, StringComparer.Ordinal)
                .ToArray();

            CollectionAssert.AreEqual(Array.Empty<string>(), violations);
        }

        [TestMethod]
        public void ActiveGalleryCSharpAvoidsLocalOnlyAutomationIdAssignments()
        {
            var repoRoot = GetRepoRoot();
            var galleryRoot = Path.Combine(repoRoot, "ModernWpf.Gallery");
            var allowedAssignments = new[]
            {
                @"ModernWpf.Gallery\MainWindow.xaml.cs: AutomationProperties.SetAutomationId(this, ""ModernWpfGalleryMainWindow"");",
                @"ModernWpf.Gallery\Pages\GalleryAutomation.cs: AutomationProperties.SetAutomationId(element, automationId);",
                @"ModernWpf.Gallery\Shell\NavigationRootPage.xaml.cs: AutomationProperties.SetAutomationId(this, ""GalleryNavigationRoot"");",
                @"ModernWpf.Gallery\Shell\NavigationRootPage.xaml.cs: AutomationProperties.SetAutomationId(Navigation, ""GalleryNavigationView"");",
                @"ModernWpf.Gallery\Shell\NavigationRootPage.xaml.cs: AutomationProperties.SetAutomationId(ContentHost, ""GalleryContentHost"");"
            };
            var violations = Directory.EnumerateFiles(galleryRoot, "*.cs", SearchOption.AllDirectories)
                .SelectMany(path => File.ReadLines(path)
                    .Where(line => line.Contains("AutomationProperties.SetAutomationId(", StringComparison.Ordinal))
                    .Select(line => Path.GetRelativePath(repoRoot, path) + ": " + line.Trim()))
                .Where(assignment => !allowedAssignments.Contains(assignment, StringComparer.Ordinal))
                .OrderBy(assignment => assignment, StringComparer.Ordinal)
                .ToArray();

            CollectionAssert.AreEqual(Array.Empty<string>(), violations);
        }

        [TestMethod]
        public void VisualCheckScriptsAvoidRetiredItemPageTitleAutomationHook()
        {
            foreach (var relativePath in new[]
            {
                Path.Combine("tools", "visual-checks", "Run-GalleryVisualChecks.ps1"),
                Path.Combine("tools", "visual-checks", "Run-WpfGalleryVisualAudit.ps1")
            })
            {
                var source = File.ReadAllText(Path.Combine(GetRepoRoot(), relativePath));
                Assert.IsFalse(
                    source.Contains("GalleryItemPageTitle", StringComparison.Ordinal),
                    relativePath + " should not rely on the retired local-only GalleryItemPageTitle automation hook.");
            }
        }

        [TestMethod]
        public void WpfGalleryVisualAuditUsesSingleRenderedContentArtifactPriority()
        {
            var source = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "tools",
                "visual-checks",
                "Run-WpfGalleryVisualAudit.ps1"));

            AssertContainsInOrder(
                source,
                "function Get-ModernRenderedContentArtifactCandidates()",
                @"FileName = ""HomeContentRootPane.png""; Source = ""HomeContentRootPaneRenderedArtifact""",
                @"FileName = ""AllControlsContentRootPane.png""; Source = ""AllControlsContentRootPaneRenderedArtifact""",
                @"FileName = ""SettingsContentRootPane.png""; Source = ""SettingsContentRootPaneRenderedArtifact""",
                @"FileName = ""ContentPagePane.png""; Source = ""ContentPagePaneRenderedArtifact""",
                @"FileName = ""GalleryItemPageRoot.png""; Source = ""GalleryItemPageRootRenderedArtifact""",
                @"FileName = ""ContentRootGrid.png""; Source = ""ContentRootGridRenderedArtifact""",
                @"FileName = ""GalleryContentHost.png""; Source = ""GalleryContentHostRenderedArtifact""",
                "function Get-ModernRenderedContentArtifactCrop");
            AssertContainsInOrder(
                source,
                "function Get-ModernRenderedContentArtifactCrop",
                "foreach ($candidate in (Get-ModernRenderedContentArtifactCandidates))",
                "$contentCrop = Get-ImageArtifactInfo $path $candidate.Source",
                "return $contentCrop",
                "function Test-ModernRenderedContentArtifact");
            AssertContainsInOrder(
                source,
                "function Test-ModernRenderedContentArtifact",
                "return $null -ne (Get-ModernRenderedContentArtifactCrop $artifactDir)",
                "function Capture-Window");
        }

        [TestMethod]
        public void WpfGalleryPageStylesKeepOfficialResourceSetterSourceShape()
        {
            var xaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Resources",
                "PageStyles.xaml");

            AssertContainsInOrder(
                xaml,
                "<Style x:Key=\"BaseTextBlockStyle\" TargetType=\"TextBlock\">",
                "<Setter Property=\"FontSize\" Value=\"{StaticResource BodyTextBlockFontSize}\" />",
                "<Setter Property=\"FontWeight\" Value=\"SemiBold\" />",
                "<Setter Property=\"TextTrimming\" Value=\"CharacterEllipsis\" />",
                "<Setter Property=\"TextWrapping\" Value=\"Wrap\" />",
                "<Setter Property=\"LineStackingStrategy\" Value=\"MaxHeight\" />",
                "</Style>");
            AssertContainsInOrder(
                xaml,
                "x:Key=\"DisplayTextBlockStyle\"",
                "<Setter Property=\"FontSize\" Value=\"{StaticResource DisplayTextBlockFontSize}\" />",
                "<ImageBrush x:Key=\"p64\" ImageSource=\"pack://application:,,,/ModernWpf.Gallery;component/Assets/UserDashboard/64-100x100.jpg\" />",
                "<ImageBrush x:Key=\"p505\" ImageSource=\"pack://application:,,,/ModernWpf.Gallery;component/Assets/UserDashboard/505-100x100.jpg\" />",
                "<Style x:Key=\"ColorTilesPanelStyle\" TargetType=\"{x:Type Border}\">");
            AssertContainsInOrder(
                xaml,
                "<Style x:Key=\"ColorTilesPanelStyle\" TargetType=\"{x:Type Border}\">",
                "<Style.Setters>",
                "<Setter Property=\"Background\" Value=\"{DynamicResource ControlExampleDisplayBrush}\" />",
                "<Setter Property=\"BorderThickness\" Value=\"1\" />",
                "<Setter Property=\"BorderBrush\" Value=\"{DynamicResource CardStrokeColorDefaultBrush}\" />",
                "<Setter Property=\"CornerRadius\" Value=\"8\" />",
                "</Style.Setters>",
                "<Style x:Key=\"GalleryPageRootStyle\" TargetType=\"Grid\">");
        }

        [TestMethod]
        public void WpfGalleryConvertersKeepOfficialVisibilitySourceShape()
        {
            var nullConverterSource = ReadRepoFile(
                "ModernWpf.Gallery",
                "Controls",
                "NullToVisibilityConverter.cs");
            AssertContainsInOrder(
                nullConverterSource,
                "/// Converts a null value to Visibility.Collapsed",
                "return value is null ? Visibility.Collapsed : Visibility.Visible;",
                "throw new NotImplementedException();");

            var userDashboardConverterSource = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "Samples",
                "UserDashboardConverters.cs");
            AssertContainsInOrder(
                userDashboardConverterSource,
                "/// Converts an empty string to Visibility.Collapsed",
                "if (value is string str)",
                "return string.IsNullOrWhiteSpace(str) ? Visibility.Collapsed : Visibility.Visible;",
                "return value is null ? Visibility.Collapsed : Visibility.Visible;",
                "throw new NotImplementedException();",
                "/// Converts an image id to a brush");
        }

        [TestMethod]
        public void WpfGalleryTemplatesKeepOfficialNavigationCardSourceShape()
        {
            var xaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Resources",
                "Templates.xaml");
            var normalizedXaml = xaml.Replace("\r\n", "\n").Replace('\r', '\n');

            AssertContainsInOrder(
                xaml,
                "<ResourceDictionary",
                "xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"",
                "xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"",
                "xmlns:pages=\"clr-namespace:ModernWpf.Gallery.Pages\">",
                "<ItemsPanelTemplate x:Key=\"WrapPanelTemplate\">");
            StringAssert.Contains(
                normalizedXaml,
                "<WrapPanel Margin=\"10\"\n                Orientation=\"Horizontal\"/>");
            AssertContainsInOrder(
                xaml,
                "<DataTemplate x:Key=\"NavigationCardTemplate\">",
                "<Button",
                "Width=\"360\"",
                "Height=\"90\"",
                "Margin=\"7\"",
                "Padding=\"20,10\"",
                "HorizontalContentAlignment=\"Left\"",
                "AutomationProperties.Name=\"{Binding Title, StringFormat='{}{0}Page'}\"",
                "Command=\"{Binding ViewModel.NavigateCommand, RelativeSource={RelativeSource AncestorType={x:Type Page}}}\"",
                "CommandParameter=\"{Binding PageType}\">");
            StringAssert.Contains(
                normalizedXaml,
                "<Image Source=\"{Binding ImageSource}\"\n                        Width=\"50\"\n                        Height=\"50\"\n                        Margin=\"0,0,8,0\"/>");
            AssertContainsInOrder(
                xaml,
                "<TextBlock",
                "Margin=\"10,0,0,0\"",
                "Style=\"{StaticResource BodyStrongTextBlockStyle}\"",
                "Text=\"{Binding Title}\" pages:GalleryAutomation.HeadingLevel=\"Level3\" />",
                "<TextBlock",
                "Width=\"240\"",
                "Margin=\"10,0,0,0\"",
                "Foreground=\"{DynamicResource TextFillColorPrimaryBrush}\"",
                "Opacity=\"0.7\"",
                "Style=\"{StaticResource CaptionTextBlockStyle}\"",
                "Text=\"{Binding Description}\"/>");
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
            Assert.IsFalse(
                xaml.Contains("ControlExampleSourceExpanderStyle", StringComparison.Ordinal),
                "The source-code expander should use the official WPF Gallery default Expander template.");
            AssertContainsInOrder(
                xaml,
                "<Expander",
                "Grid.Row=\"2\"",
                "AutomationProperties.Name=\"{Binding HeaderText, RelativeSource={RelativeSource TemplatedParent}, StringFormat=View Source Code for {0}}\"",
                "Header=\"Source code\"",
                "<StackPanel>",
                "<StackPanel x:Name=\"XamlCodeBlock\">");
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
                "/// A control that displays an example of a control",
                "[ContentProperty(nameof(ExampleContent))]",
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
                "get => (string)GetValue(HeaderTextProperty);",
                "set => SetValue(HeaderTextProperty, value);",
                "public object ExampleContent",
                "get => GetValue(ExampleContentProperty);",
                "set => SetValue(ExampleContentProperty, value);",
                "public string XamlCode",
                "get => (string)GetValue(XamlCodeProperty);",
                "set => SetValue(XamlCodeProperty, value);",
                "public Uri XamlCodeSource",
                "get => (Uri)GetValue(XamlCodeSourceProperty);",
                "set => SetValue(XamlCodeSourceProperty, value);",
                "public string CSharpCode",
                "get => (string)GetValue(CSharpCodeProperty);",
                "set => SetValue(CSharpCodeProperty, value);",
                "public Uri CSharpCodeSource",
                "get => (Uri)GetValue(CSharpCodeSourceProperty);",
                "set => SetValue(CSharpCodeSourceProperty, value);",
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
                "get => (string)GetValue(TitleProperty);",
                "set => SetValue(TitleProperty, value);",
                "public string Description",
                "get => (string)GetValue(DescriptionProperty);",
                "set => SetValue(DescriptionProperty, value);",
                "public bool ShowDescription",
                "get => (bool)GetValue(ShowDescriptionProperty);",
                "set => SetValue(ShowDescriptionProperty, value);");
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
                "HeaderTile.xaml.cs")
                .Replace("\r\n", "\n")
                .Replace('\r', '\n');

            Assert.IsFalse(
                source.Contains("OnUserPreferenceChanged", StringComparison.Ordinal),
                "HeaderTile should keep the official SystemEvents_UserPreferenceChanged handler name.");
            AssertContainsInOrder(
                source,
                "/// Interaction logic for HeaderTile.xaml",
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
                "/// Interaction logic for TileGallery.xaml",
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
                "/// Interaction logic for ColorPageExample.xaml",
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
                "/// Interaction logic for ColorTile.xaml",
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
                "// Using a DependencyProperty as the backing store for ShowSeparator.  This enables animation, styling, binding, etc...",
                "public static readonly DependencyProperty ShowSeparatorProperty",
                "DependencyProperty.Register(\"ShowSeparator\", typeof(bool), typeof(ColorTile), new PropertyMetadata(true));",
                "public bool ShowWarning",
                "// Using a DependencyProperty as the backing store for ShowSeparator.  This enables animation, styling, binding, etc...",
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
            var normalizedXaml = xaml.Replace("\r\n", "\n").Replace('\r', '\n');

            AssertContainsInOrder(
                xaml,
                "<Page",
                "x:Class=\"ModernWpf.Gallery.Pages.HomePage\"",
                "xmlns:d=\"http://schemas.microsoft.com/expression/blend/2008\"",
                "xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages\"",
                "xmlns:mc=\"http://schemas.openxmlformats.org/markup-compatibility/2006\"",
                "xmlns:controls=\"clr-namespace:ModernWpf.Gallery.Controls\"",
                "xmlns:pages=\"clr-namespace:ModernWpf.Gallery.Pages\"",
                "xmlns:ui=\"http://schemas.modernwpf.com/2019\"",
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
                "<Style x:Key=\"HomePageRootStyle\" TargetType=\"Grid\">",
                "<MultiDataTrigger>",
                "<Condition Binding=\"{Binding ActualApplicationTheme, Source={x:Static ui:ThemeManager.Current}}\" Value=\"{x:Static ui:ApplicationTheme.Dark}\" />",
                "<Condition Binding=\"{Binding Path=(SystemParameters.HighContrast)}\" Value=\"False\" />",
                "<Setter Property=\"Background\" Value=\"#272727\" />",
                "<ScrollViewer >",
                "<Grid Style=\"{StaticResource HomePageRootStyle}\">",
                "<Grid.RowDefinitions>",
                "<RowDefinition Height=\"Auto\" />",
                "<RowDefinition Height=\"Auto\" />",
                "<RowDefinition Height=\"*\" />");
            StringAssert.Contains(
                normalizedXaml,
                "<ScrollViewer >\n\n        <Grid Style=\"{StaticResource HomePageRootStyle}\">");
            StringAssert.Contains(
                normalizedXaml,
                "<Border CornerRadius=\"8,0,0,0\"\n                    Grid.RowSpan=\"2\">\n                <Border.Background>\n                    <ImageBrush ImageSource=\"pack://application:,,,/ModernWpf.Gallery;component/Assets/win11-dashboard.light.png\" Stretch=\"UniformToFill\" />");
            AssertContainsInOrder(
                xaml,
                "<Border CornerRadius=\"8,0,0,0\"",
                "Grid.RowSpan=\"2\"",
                "<StackPanel Margin=\"36,48,0,0\" VerticalAlignment=\"Top\" TextElement.Foreground=\"Black\">",
                "<TextBlock Style=\"{StaticResource SubtitleTextBlockStyle}\" Text=\".NET 10\" Margin=\"0,0,0,2\" pages:GalleryAutomation.HeadingLevel=\"Level1\" />",
                "<TextBlock Style=\"{StaticResource TitleLargeTextBlockStyle}\" Text=\"WPF Gallery\" Margin=\"0,0,0,8\" pages:GalleryAutomation.HeadingLevel=\"Level1\" />",
                "<Border Background=\"Transparent\" CornerRadius=\"8,8,8,8\" MaxWidth=\"300\" HorizontalAlignment=\"Left\">",
                "<TextBlock",
                "MaxWidth=\"300\"",
                "Margin=\"0,0,0,0\"",
                "Style=\"{StaticResource BodyStrongTextBlockStyle}\"",
                "Text=\"A collection of controls, guidelines and samples to build great WPF applications\"",
                "TextAlignment=\"Left\"",
                "HorizontalAlignment=\"Left\"",
                "Padding=\"0,8,12,8\"/>");
            Assert.AreEqual(
                1,
                xaml.Split(new[] { "Foreground=\"Black\"" }, StringSplitOptions.None).Length - 1,
                "Home hero text should inherit black foreground from the source-shaped StackPanel.");
            AssertContainsInOrder(
                xaml,
                "<controls:TileGallery Grid.Row=\"1\" HorizontalAlignment=\"Stretch\" Margin=\"0\"/>",
                "<StackPanel Grid.Row=\"2\" Margin=\"32,24,0,0\" Orientation=\"Vertical\">");
            AssertContainsInOrder(
                xaml,
                "<TextBlock",
                "Style=\"{StaticResource BodyStrongTextBlockStyle}\"",
                "Text=\"Overview\"",
                "FontSize=\"16\"",
                "pages:GalleryAutomation.HeadingLevel=\"Level2\" />",
                "<ItemsControl",
                "Margin=\"-20,0,0,0\"",
                "AutomationProperties.Name=\"Items in group\"",
                "ItemsSource=\"{Binding ViewModel.NavigationCards}\"",
                "Focusable=\"False\"",
                "ItemsPanel=\"{StaticResource WrapPanelTemplate}\"",
                "ItemTemplate=\"{StaticResource NavigationCardTemplate}\" />");
            AssertContainsInOrder(
                xaml,
                "<TextBlock",
                "Style=\"{StaticResource BodyStrongTextBlockStyle}\"",
                "Text=\"Recently added and updated\"",
                "FontSize=\"16\"",
                "pages:GalleryAutomation.HeadingLevel=\"Level2\" />",
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
                "<Page x:Class=\"ModernWpf.Gallery.Pages.WhatsNewPage\"",
                "xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"",
                "xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"",
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
                "<Grid HorizontalAlignment=\"Left\">\n                        <Grid.RowDefinitions>",
                "<TextBlock Grid.Row=\"0\" Grid.Column=\"0\" FontWeight=\"Bold\" Margin=\"0 0 10 0\">Sl. No.</TextBlock>",
                "<TextBlock Grid.Row=\"0\" Grid.Column=\"1\" FontWeight=\"Bold\">Name</TextBlock>",
                "<TextBlock Grid.Row=\"0\" Grid.Column=\"2\" FontWeight=\"Bold\">Description</TextBlock>",
                "<TextBlock Grid.Row=\"1\" Grid.Column=\"2\" TextWrapping=\"Wrap\">Quadrilateral where all the adjacent sides form a right angle.</TextBlock>",
                "<TextBlock Grid.Row=\"2\" Grid.Column=\"2\" TextWrapping=\"Wrap\">Set of all points that are equidistant from a fixed point.</TextBlock>",
                "<TextBlock Style=\"{StaticResource TitleTextBlockStyle}\" Margin=\"0 0 0 12\">\n                    .NET 9\n                </TextBlock>",
                "<TextBlock Style=\"{StaticResource SubtitleTextBlockStyle}\" Margin=\"0 24 0 12\">\n                    Hyphen based ligature support\n                </TextBlock>",
                "<TextBlock Margin=\"0 0 16 0\" FontFamily=\"Cascadia Code\" Text=\"-->\" />");
            AssertContainsInOrder(
                xaml,
                "<Border CornerRadius=\"2 0 0 2\" Background=\"{DynamicResource SystemAccentColorDark3Brush}\" />",
                "<Border Background=\"{DynamicResource SystemAccentColorDark2Brush}\" />",
                "<Border Background=\"{DynamicResource SystemAccentColorDark1Brush}\" />",
                "<Border Background=\"{DynamicResource SystemControlBackgroundAccentBrush}\" />",
                "<Border Background=\"{DynamicResource SystemAccentColorLight1Brush}\" />",
                "<Border Background=\"{DynamicResource SystemAccentColorLight2Brush}\" />",
                "<Border CornerRadius=\"0 2 2 0\" Background=\"{DynamicResource SystemAccentColorLight3Brush}\" />");
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
                "xmlns:ui=\"http://schemas.modernwpf.com/2019\"",
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
                "<Style x:Key=\"SettingsPageRootStyle\" BasedOn=\"{StaticResource GalleryPageRootStyle}\" TargetType=\"Grid\">");
            AssertContainsInOrder(
                xaml,
                "<MultiDataTrigger>",
                "<Condition Binding=\"{Binding ActualApplicationTheme, Source={x:Static ui:ThemeManager.Current}}\" Value=\"{x:Static ui:ApplicationTheme.Dark}\" />",
                "<Condition Binding=\"{Binding Path=(SystemParameters.HighContrast)}\" Value=\"False\" />",
                "<Setter Property=\"Background\" Value=\"#272727\" />");
            StringAssert.Contains(
                xaml,
                "<Grid Style=\"{StaticResource SettingsPageRootStyle}\">");
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
                "InitializeComponent();",
                "SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;",
                "this.Loaded += (s, e) => UpdatePageVisuals();",
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
            StringAssert.Contains(
                gridSplitterXaml.Replace("\r\n", "\n").Replace('\r', '\n'),
                Lines(
                    "        <sys:String x:Key=\"SampleText\">",
                    "            Lorem Ipsum is simply dummy text of the printing and typesetting industry.",
                    "        Lorem Ipsum has been the industry's standard dummy text ever since the 1500s.",
                    "        </sys:String>",
                    "        <sys:String x:Key=\"SampleText2\">",
                    "            When an unknown printer took a galley of type and scrambled it to",
                    "        make a type specimen book.",
                    "        </sys:String>"));
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
                "<TextBox Name=\"NameTextBox\" Width=\"280\" Margin=\"10,0,0,20\" AutomationProperties.Name=\"Name Field\"/>",
                "<TextBlock Width=\"100\" Text=\"Gender:\" Margin=\"0,10,0,0\"/>",
                "<TextBox Name=\"GenderTextBox\" Width=\"280\" Margin=\"10,0,0,20\" AutomationProperties.Name=\"Gender Field\"/>",
                "<Button Content=\"Submit\" HorizontalAlignment=\"Right\" Margin=\"0,10,0,0\" />");
            StringAssert.Contains(
                normalizedGroupBoxXaml,
                "</StackPanel>\n                                <Button Content=\"Submit\" HorizontalAlignment=\"Right\" Margin=\"0,10,0,0\" />");
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
                if (page == "SpacingPage.xaml")
                {
                    AssertContainsInOrder(
                        xaml,
                        "x:Name=\"CardImage\"",
                        "Source=\"/Assets/Design/Cards.dark.png\"",
                        "AutomationProperties.Name=\"Example of spacing in a page with cards layout\"");
                    AssertContainsInOrder(
                        xaml,
                        "x:Name=\"DialogImage\"",
                        "Source=\"/Assets/Design/Dialog.dark.png\"",
                        "AutomationProperties.Name=\"Example of spacing in a form layout\"");
                }
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
            AssertContainsInOrder(
                geometryXaml,
                "x:Name=\"GeometryImage\"",
                "Source=\"/Assets/Design/Geometry.dark.png\"",
                "AutomationProperties.Name=\"Example of corner radius.\"");

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
                "xmlns:i=\"http://schemas.microsoft.com/xaml/behaviors\"",
                "xmlns:mc=\"http://schemas.openxmlformats.org/markup-compatibility/2006\"",
                "xmlns:controls=\"clr-namespace:ModernWpf.Gallery.Controls\"",
                "mc:Ignorable=\"d\"",
                "Foreground=\"{DynamicResource TextFillColorPrimaryBrush}\"",
                "d:DesignHeight=\"450\" d:DesignWidth=\"800\"",
                "d:Background=\"White\"",
                "Title=\"IconsPage\">");
            AssertContainsInOrder(
                iconographyXaml,
                "<i:Interaction.Triggers>",
                "<i:EventTrigger EventName=\"Loaded\">",
                "<i:InvokeCommandAction Command=\"{Binding ViewModel.LoadDataCommand}\" />",
                "</i:EventTrigger>",
                "</i:Interaction.Triggers>",
                "<Page.Resources>");
            Assert.IsFalse(
                iconographyXaml.Contains("xmlns:local=\"clr-namespace:ModernWpf.Gallery.Pages.WpfGallery.DesignGuidance\"", StringComparison.Ordinal),
                "IconographyPage.xaml should keep the current official root namespace shape, which has no local namespace declaration.");
            StringAssert.Contains(
                iconographyXaml,
                "<controls:PageHeader Margin=\"2,0,0,32\" Title=\"{Binding ViewModel.PageTitle}\" Description=\"{Binding ViewModel.PageDescription}\" />");
            AssertContainsInOrder(
                iconographyXaml,
                "<Style x:Key=\"CodeValuePresenterStyle\" TargetType=\"TextBlock\">",
                "<Setter Property=\"Padding\" Value=\"0 0 0 4\" />",
                "<Setter Property=\"Opacity\" Value=\"0.7\"/>",
                "<Setter Property=\"FontSize\" Value=\"14\"/>",
                "<Style x:Key=\"IconData\" TargetType=\"{x:Type ContentControl}\">",
                "<Setter Property=\"Focusable\" Value=\"False\"/>",
                "<Grid >",
                "<TextBlock Padding=\"0,6\" Grid.Column=\"0\" VerticalAlignment=\"Center\" Text=\"{TemplateBinding Content}\" Style=\"{StaticResource CodeValuePresenterStyle}\" TextWrapping=\"Wrap\"/>");
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
                "<Style x:Key=\"IconTagChipButtonStyle\" TargetType=\"Button\" BasedOn=\"{StaticResource DefaultButtonStyle}\">",
                "<Style x:Key=\"IconsListViewItemFocusVisualStyle\">",
                "<Rectangle",
                "RadiusX=\"4\"",
                "RadiusY=\"4\"",
                "Margin=\"5\"",
                "Stroke=\"{DynamicResource KeyboardFocusBorderColorBrush}\"",
                "StrokeThickness=\"2\" />",
                "<Style x:Key=\"PaginationButtonStyle\" TargetType=\"Button\" BasedOn=\"{StaticResource DefaultButtonStyle}\">",
                "<Border x:Name=\"ContentBorder\"",
                "<ContentPresenter x:Name=\"ContentPresenter\"",
                "RecognizesAccessKey=\"True\"",
                "HorizontalAlignment=\"{TemplateBinding HorizontalContentAlignment}\"",
                "VerticalAlignment=\"{TemplateBinding VerticalContentAlignment}\"",
                "Content=\"{TemplateBinding Content}\"",
                "ContentTemplate=\"{TemplateBinding ContentTemplate}\"");
            StringAssert.Contains(
                normalizedIconographyXaml,
                "<Grid Margin=\"0 0 0 10\">\n        <Grid.RowDefinitions>");
            AssertContainsInOrder(
                iconographyXaml,
                "<Expander Grid.Row=\"1\"",
                "Header=\"Instructions on how to use Segoe Fluent Icons\"",
                "IsExpanded=\"False\"",
                "Margin=\"2 -8 0 0\">");
            AssertContainsInOrder(
                normalizedIconographyXaml,
                "<Run FontWeight=\"SemiBold\">\n                How to get the font\n            </Run>",
                "<LineBreak />",
                "On Windows 10: Segoe Fluent Icons is not included by default on Windows 10.",
                "<LineBreak/>",
                "<LineBreak/>",
                "<Span FontWeight=\"SemiBold\">\n                How to use the font\n            </Span>",
                "<LineBreak/>",
                "For optimal appearance, use these specific sizes: 16, 20, 24, 32, 40, 48, and 64.",
                "<LineBreak/>",
                "<Hyperlink Click=\"Open_IconDesignGuidelinesPage\">\n                    layering</Hyperlink> and colorization effects can be achieved by drawing glyphs directly on top of each other.",
                "<LineBreak/>",
                "<LineBreak/>",
                "<Run FontWeight=\"SemiBold\">\n                XAML\n            </Run>",
                "<LineBreak/>",
                "<Span>\n                &lt;Grid&gt;\n            </Span>",
                "<LineBreak/>",
                "<Span>\n                &lt;TextBlock FontFamily=\"{StaticResource SymbolThemeFontFamily}\" Text=\"&amp;#xEB51;\" Foreground=\"#C72335\"/&gt;\n            </Span>",
                "<LineBreak/>",
                "<Span>\n                &#x09;&lt;TextBlock FontFamily=\"{StaticResource SymbolThemeFontFamily}\" Text=\"&amp;#xEB51;\" /&gt;\n            </Span>",
                "<LineBreak/>",
                "<Span>\n                &lt;/Grid&gt;\n            </Span>");
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
            StringAssert.Contains(
                iconographyXaml,
                "<TextBlock Grid.Row=\"2\" Style=\"{StaticResource BodyStrongTextBlockStyle}\" Text=\"Fluent Icons Library\" Margin=\"2,24,0,10\" />");
            StringAssert.Contains(
                normalizedIconographyXaml,
                "<TextBlock x:Name=\"IconsSearchBoxPlaceholder\" VerticalAlignment=\"Center\" Style=\"{StaticResource BodyTextBlockStyle}\" Text=\"Search Icons by Name, Tag\" Margin=\"14,0,0,0\"\n                       IsHitTestVisible=\"False\" Foreground=\"{DynamicResource TextFillColorSecondaryBrush}\"/>");
            StringAssert.Contains(
                normalizedIconographyXaml,
                "<Grid Grid.Row=\"4\" Margin=\"2 10 2 10\">\n            <Grid.ColumnDefinitions>\n                <ColumnDefinition Width=\"*\"/>\n                <ColumnDefinition Width=\"300\"/>\n            </Grid.ColumnDefinitions>\n\n            <Border CornerRadius=\"8 0 0 8\" Background=\"{DynamicResource SubtleFillColorSecondaryBrush}\" Grid.Column=\"0\"/>");
            AssertContainsInOrder(
                iconographyXaml,
                "<ListView AutomationProperties.Name=\"Icons\" ItemsSource=\"{Binding ViewModel.DisplayedIcons}\" ScrollViewer.HorizontalScrollBarVisibility=\"Disabled\" ScrollViewer.VerticalScrollBarVisibility=\"Visible\" Padding=\"4\" SelectedItem=\"{Binding ViewModel.SelectedIcon}\" SelectionMode=\"Single\" >",
                "<WrapPanel Orientation=\"Horizontal\" Margin=\"10\" HorizontalAlignment=\"Stretch\" VerticalAlignment=\"Stretch\"/>",
                "<Style TargetType=\"ListViewItem\" BasedOn=\"{StaticResource DefaultListViewItemStyle}\">",
                "<Setter Property=\"AutomationProperties.Name\" Value=\"{Binding Name, Mode=OneWay}\"/>",
                "<Border BorderThickness=\"4\" CornerRadius=\"8\" Background=\"{DynamicResource ButtonBackground}\">",
                "<Grid Width=\"96\" Height=\"96\" ToolTip=\"{Binding Name}\" >",
                "<TextBlock Focusable=\"False\" Grid.Row=\"0\" FontFamily=\"{StaticResource SymbolThemeFontFamily}\" Text=\"{Binding Character}\" AutomationProperties.Name=\"{Binding Name, StringFormat='{}{0} Icon'}\" FontSize=\"28\" Width=\"28\" Height=\"28\" VerticalAlignment=\"Center\" HorizontalAlignment=\"Center\"/>",
                "<TextBlock Focusable=\"False\" Grid.Row=\"1\" Text=\"{Binding Name}\" Style=\"{StaticResource CaptionTextBlockStyle}\" HorizontalAlignment=\"Center\" VerticalAlignment=\"Bottom\" Foreground=\"{DynamicResource TextFillColorPrimaryBrush}\" TextTrimming=\"CharacterEllipsis\" TextWrapping=\"NoWrap\" Margin=\"6,-4,6,8\"/>");
            AssertContainsInOrder(
                iconographyXaml,
                "<Grid Grid.Column=\"1\" Grid.Row=\"0\" Background=\"{DynamicResource ButtonBackground}\">",
                "<StackPanel Orientation=\"Vertical\" Margin=\"16\">",
                "<TextBlock Text=\"{Binding ViewModel.SelectedIcon.Name}\" Style=\"{StaticResource SubtitleTextBlockStyle}\" VerticalAlignment=\"Center\"/>",
                "<TextBlock Text=\"{Binding ViewModel.SelectedIcon.Character}\" FontFamily=\"{StaticResource SymbolThemeFontFamily}\" FontSize=\"50\" Margin=\"0,12,0,32\"",
                "AutomationProperties.Name=\"{Binding ViewModel.SelectedIcon.Name, StringFormat='{}{0} Icon'}\" HorizontalAlignment=\"Left\" VerticalAlignment=\"Center\"/>",
                "<TextBlock Text=\"Icon Name\"/>",
                "<ContentControl Style=\"{StaticResource IconData}\" Content=\"{Binding ViewModel.SelectedIcon.Name}\" Tag=\"Icon Name\"/>",
                "<TextBlock Text=\"XAML\"/>",
                "<TextBlock x:Name=\"XAMLCode\" Text=\"{Binding ViewModel.SelectedIcon.TextGlyph, StringFormat='&lt;TextBlock FontFamily=&#x22;{{StaticResource SymbolThemeFontFamily}}&#x22; Text=&#x22;{0}&#x22;/&gt;'}\" Visibility=\"Collapsed\"/>",
                "<ContentControl Style=\"{StaticResource IconData}\" Content=\"{Binding ElementName=XAMLCode, Path=Text}\" Tag=\"XAML Code\"/>");
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
            AssertContainsInOrder(
                iconographyXaml,
                "<StackPanel Margin=\"0,0,0,0\" Orientation=\"Horizontal\" HorizontalAlignment=\"Left\">",
                "<Button Style=\"{StaticResource PaginationButtonStyle}\" Command=\"{Binding ViewModel.PreviousPageCommand}\" Margin=\"0,0,8,0\" Padding=\"8\" ToolTip=\"Previous Page\"",
                "AutomationProperties.Name=\"Previous Page\">",
                "<TextBlock FontFamily=\"{StaticResource SymbolThemeFontFamily}\" Text=\"&#xF08D;\" FontSize=\"12\"/>",
                "<TextBlock Text=\"{Binding ViewModel.CurrentPage, StringFormat='Page {0} of'}\" VerticalAlignment=\"Center\" Margin=\"0,0,4,0\"/>",
                "<TextBlock Text=\"{Binding ViewModel.TotalPages}\" VerticalAlignment=\"Center\" Margin=\"0,0,8,0\"/>",
                "<Button Style=\"{StaticResource PaginationButtonStyle}\" Command=\"{Binding ViewModel.NextPageCommand}\" Padding=\"8\" ToolTip=\"Next Page\"",
                "AutomationProperties.Name=\"Next Page\">",
                "<TextBlock FontFamily=\"{StaticResource SymbolThemeFontFamily}\" Text=\"&#xF08F;\" FontSize=\"12\"/>",
                "<StackPanel Orientation=\"Horizontal\" Grid.Column=\"1\">",
                "<TextBlock Style=\"{StaticResource BodyTextBlockStyle}\" Text=\"Icons per page\" Margin=\"10,0,0,0\"",
                "VerticalAlignment=\"Center\"/>",
                "<ComboBox ItemsSource=\"{Binding ViewModel.PageSizeOptions}\"",
                "SelectedIndex=\"{Binding ViewModel.SelectedPageSizeIndex}\"",
                "AutomationProperties.Name=\"Icons per page\" Margin=\"10,0,0,0\"/>");
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
