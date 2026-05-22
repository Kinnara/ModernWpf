using Microsoft.VisualStudio.TestTools.UnitTesting;
using ClipboardPage = ModernWpf.Gallery.Pages.WpfGallery.SystemPages.ClipboardPage;
using FileAndFolderDialogsPage = ModernWpf.Gallery.Pages.WpfGallery.SystemPages.FileAndFolderDialogsPage;
using MessageBoxPage = ModernWpf.Gallery.Pages.WpfGallery.SystemPages.MessageBoxPage;
using SystemPageViewModel = ModernWpf.Gallery.Pages.WpfGallery.SystemPages.WpfGallerySystemPageViewModel;
using static ModernWpf.Gallery.Tests.WpfGallerySnippetTestHelpers;

namespace ModernWpf.Gallery.Tests
{
    [TestClass]
    public class WpfGallerySystemSnippetTests
    {
        [TestMethod]
        public void SystemControlExamplesMatchOfficialWpfGallerySampleCode()
        {
            WpfTestHost.Run(() =>
            {
                AssertExamples(
                    new FileAndFolderDialogsPage(),
                    new ExpectedExample(
                        "Pick Single File",
                        "<Button Content=\"Pick Single File\" Click=\"PickSingleFileButton_Click\" />",
                        Lines(
                            "private void PickSingleFileButton_Click(object sender, RoutedEventArgs e)",
                            "{",
                            "var openFileDialog = new OpenFileDialog",
                            "{",
                            "Title = \"Select a file\",",
                            "Filter = \"All files (*.*)|*.*|Text files (*.txt)|*.txt\",",
                            "Multiselect = false",
                            "};",
                            "",
                            "if (openFileDialog.ShowDialog() == true)",
                            "{",
                            "// Use the selected file",
                            "string filePath = openFileDialog.FileName;",
                            "}",
                            "}")),
                    new ExpectedExample(
                        "Pick Multiple Files",
                        "<Button Content=\"Pick Multiple Files\" Click=\"PickMultipleFilesButton_Click\" />",
                        Lines(
                            "private void PickMultipleFilesButton_Click(object sender, RoutedEventArgs e)",
                            "{",
                            "var openFileDialog = new OpenFileDialog",
                            "{",
                            "Title = \"Select multiple files\",",
                            "Filter = \"All files (*.*)|*.*|Text files (*.txt)|*.txt\",",
                            "Multiselect = true",
                            "};",
                            "",
                            "if (openFileDialog.ShowDialog() == true)",
                            "{",
                            "// Access all selected files",
                            "string[] files = openFileDialog.FileNames;",
                            "}",
                            "}")),
                    new ExpectedExample(
                        "Save File",
                        "<Button Content=\"Save File\" Click=\"SaveFileButton_Click\" />",
                        Lines(
                            "private void SaveFileButton_Click(object sender, RoutedEventArgs e)",
                            "{",
                            "var saveFileDialog = new SaveFileDialog",
                            "{",
                            "Title = \"Save file\",",
                            "Filter = \"Text files (*.txt)|*.txt|All files (*.*)|*.*\",",
                            "DefaultExt = \"txt\"",
                            "};",
                            "",
                            "if (saveFileDialog.ShowDialog() == true)",
                            "{",
                            "try",
                            "{",
                            "System.IO.File.WriteAllText(saveFileDialog.FileName, fileContent);",
                            "}",
                            "catch (Exception ex)",
                            "{",
                            "// Handle error",
                            "}",
                            "}",
                            "}")),
                    new ExpectedExample(
                        "Pick Folder",
                        "<Button Content=\"Pick Folder\" Click=\"PickFolderButton_Click\" />",
                        Lines(
                            "private void PickFolderButton_Click(object sender, RoutedEventArgs e)",
                            "{",
                            "var folderBrowserDialog = new OpenFolderDialog",
                            "{",
                            "Title = \"Select a folder\"",
                            "};",
                            "",
                            "if (folderBrowserDialog.ShowDialog() == true)",
                            "{",
                            "// Use the selected folder",
                            "string folderPath = folderBrowserDialog.FolderName;",
                            "}",
                            "}")));

                AssertExamples(
                    new MessageBoxPage(),
                    new ExpectedExample(
                        "Simple MessageBox",
                        "<Button Content=\"Simple MessageBox\" Click=\"ShowDefaultMessageButton_Click\" />",
                        Lines(
                            "private void ShowDefaultMessageButton_Click(object sender, RoutedEventArgs e)",
                            "{",
                            "var result = MessageBox.Show(\"This is a simple message box!\");",
                            "}")),
                    new ExpectedExample(
                        "MessageBox with Custom Title and Description",
                        "<Button Content=\"Show MessageBox\" Click=\"ShowCustomTitleButton_Click\" />",
                        Lines(
                            "private void ShowCustomTitleButton_Click(object sender, RoutedEventArgs e)",
                            "{",
                            "var result = MessageBox.Show(",
                            "\"This is a detailed description of what happened or what action is needed.\",",
                            "\"Custom Title\");",
                            "}")),
                    new ExpectedExample(
                        "MessageBox with Different Buttons",
                        "<Button Content=\"Show MessageBox\" Click=\"ShowMessageBoxButton_Click\" />",
                        MessageBoxButtonSnippet(
                            "\tMessageBox.Show(\"Message\", \"Title\", MessageBoxButton.OK);")),
                    new ExpectedExample(
                        "MessageBox with Different Images",
                        "<Button Content=\"Show MessageBox\" Click=\"ShowMessageButton_Click\" />",
                        MessageBoxImageSnippet(
                            "\tMessageBox.Show(\"Message\", \"Title\", MessageBoxButton.OK, MessageBoxImage.None);")),
                    new ExpectedExample(
                        "Information, Error, and Warning MessageBox",
                        Lines(
                            "<WrapPanel Margin=\"0,0,0,10\">",
                            "<Button Content=\"Information\" Click=\"ShowInformationButton_Click\" />",
                            "<Button Content=\"Error\" Click=\"ShowErrorButton_Click\" />",
                            "<Button Content=\"Warning\" Click=\"ShowWarningButton_Click\" />",
                            "</WrapPanel>"),
                        Lines(
                            "// Information",
                            "private void ShowInformationButton_Click(object sender, RoutedEventArgs e)",
                            "{",
                            "MessageBox.Show(\"Operation completed successfully.\", \"Information\", MessageBoxButton.OK, MessageBoxImage.Information);",
                            "}",
                            "",
                            "// Error",
                            "private void ShowErrorButton_Click(object sender, RoutedEventArgs e)",
                            "{",
                            "MessageBox.Show(\"An error occurred!\", \"Error\", MessageBoxButton.OK, MessageBoxImage.Error);",
                            "}",
                            "",
                            "// Warning",
                            "private void ShowWarningButton_Click(object sender, RoutedEventArgs e)",
                            "{",
                            "MessageBox.Show(\"This action cannot be undone!\", \"Warning\", MessageBoxButton.OKCancel, MessageBoxImage.Warning);",
                            "}")),
                    new ExpectedExample(
                        "MessageBox with Custom Default Button",
                        "<Button Content=\"Show with 'No' as default\" Click=\"ShowCustomDefaultButton_Click\" />",
                        Lines(
                            "private void ShowCustomDefaultButton_Click(object sender, RoutedEventArgs e)",
                            "{",
                            "// Set 'No' as the default button (focused when dialog opens)",
                            "var result = MessageBox.Show(",
                            "\"Do you want to save changes? Press Enter to select the default 'No' button.\",",
                            "\"Save Changes\",",
                            "MessageBoxButton.YesNoCancel,",
                            "MessageBoxImage.Question,",
                            "MessageBoxResult.No);",
                            "}")));

                AssertExamples(
                    new ClipboardPage(),
                    new ExpectedExample(
                        "Copy text to Clipboard",
                        "<Button Content=\"Copy to Clipboard\" Click=\"CopyToClipboard_Click\" />",
                        Lines(
                            "private void CopyToClipboard_Click(object sender, RoutedEventArgs e)",
                            "{",
                            "Clipboard.SetText(textToCopy);",
                            "}")),
                    new ExpectedExample(
                        "Paste text from Clipboard",
                        "<Button Content=\"Paste from Clipboard\" Click=\"PasteFromClipboard_Click\" />",
                        Lines(
                            "private void PasteFromClipboard_Click(object sender, RoutedEventArgs e)",
                            "{",
                            "if (Clipboard.ContainsText())",
                            "{",
                            "string text = Clipboard.GetText();",
                            "}",
                            "}")),
                    new ExpectedExample(
                        "Clear Clipboard",
                        "<Button Content=\"Clear Clipboard\" Click=\"ClearClipboard_Click\" />",
                        Lines(
                            "private void ClearClipboard_Click(object sender, RoutedEventArgs e)",
                            "{",
                            "Clipboard.Clear();",
                            "}")),
                    new ExpectedExample(
                        "Check Clipboard data formats",
                        "<Button Content=\"Check Formats\" Click=\"CheckFormats_Click\" />",
                        Lines(
                            "private void CheckFormats_Click(object sender, RoutedEventArgs e)",
                            "{",
                            "bool hasText = Clipboard.ContainsText();",
                            "bool hasImage = Clipboard.ContainsImage();",
                            "bool hasFileDropList = Clipboard.ContainsFileDropList();",
                            "}")),
                    new ExpectedExample(
                        "Copy image to Clipboard",
                        "<Button Content=\"Copy Image\" Click=\"CopyImageToClipboard_Click\" />",
                        Lines(
                            "private void CopyImageToClipboard_Click(object sender, RoutedEventArgs e)",
                            "{",
                            "Clipboard.SetImage(bitmapSource);",
                            "}")),
                    new ExpectedExample(
                        "Paste image from Clipboard",
                        "<Button Content=\"Paste Image\" Click=\"PasteImageFromClipboard_Click\" />",
                        Lines(
                            "private void PasteImageFromClipboard_Click(object sender, RoutedEventArgs e)",
                            "{",
                            "if (Clipboard.ContainsImage())",
                            "{",
                            "BitmapSource image = Clipboard.GetImage();",
                            "}",
                            "}")));
            });
        }

        [TestMethod]
        public void MessageBoxDynamicSnippetsMatchOfficialWpfGallerySampleCode()
        {
            var viewModel = new SystemPageViewModel("MessageBox", string.Empty);

            var buttonSnippets = new[]
            {
                MessageBoxButtonSnippet(
                    "\tMessageBox.Show(\"Message\", \"Title\", MessageBoxButton.OK);"),
                MessageBoxButtonSnippet(
                    "\tvar result = MessageBox.Show(\"Message\", \"Title\", MessageBoxButton.OKCancel);",
                    "\tif (result == MessageBoxResult.OK)",
                    "\t{",
                    "\t    // User clicked OK",
                    "\t}"),
                MessageBoxButtonSnippet(
                    "\tvar result = MessageBox.Show(\"Message\", \"Title\", MessageBoxButton.AbortRetryIgnore);",
                    "\tif (result == MessageBoxResult.Abort)",
                    "\t{",
                    "\t    // User clicked Abort",
                    "\t}",
                    "\telse if (result == MessageBoxResult.Retry)",
                    "\t{",
                    "\t    // User clicked Retry",
                    "\t}",
                    "\telse if (result == MessageBoxResult.Ignore)",
                    "\t{",
                    "\t    // User clicked Ignore",
                    "\t}"),
                MessageBoxButtonSnippet(
                    "\tvar result = MessageBox.Show(\"Message\", \"Title\", MessageBoxButton.YesNoCancel);",
                    "\tif (result == MessageBoxResult.Yes)",
                    "\t{",
                    "\t    // User clicked Yes",
                    "\t}",
                    "\telse if (result == MessageBoxResult.No)",
                    "\t{",
                    "\t    // User clicked No",
                    "\t}"),
                MessageBoxButtonSnippet(
                    "\tvar result = MessageBox.Show(\"Message\", \"Title\", MessageBoxButton.YesNo);",
                    "\tif (result == MessageBoxResult.Yes)",
                    "\t{",
                    "\t    // User clicked Yes",
                    "\t}",
                    "\telse if (result == MessageBoxResult.No)",
                    "\t{",
                    "\t    // User clicked No",
                    "\t}"),
                MessageBoxButtonSnippet(
                    "\tvar result = MessageBox.Show(\"Message\", \"Title\", MessageBoxButton.RetryCancel);",
                    "\tif (result == MessageBoxResult.Retry)",
                    "\t{",
                    "\t    // User clicked Retry",
                    "\t}"),
                MessageBoxButtonSnippet(
                    "\tvar result = MessageBox.Show(\"Message\", \"Title\", MessageBoxButton.CancelTryContinue);",
                    "\tif (result == MessageBoxResult.TryAgain)",
                    "\t{",
                    "\t    // User clicked Try Again",
                    "\t}",
                    "\telse if (result == MessageBoxResult.Continue)",
                    "\t{",
                    "\t    // User clicked Continue",
                    "\t}")
            };

            Assert.AreEqual("<Button Content=\"Show MessageBox\" Click=\"ShowMessageBoxButton_Click\" />", viewModel.DifferentButtonsXamlCode);
            for (var i = 0; i < buttonSnippets.Length; i++)
            {
                viewModel.SelectedButtonIndex = i;
                Assert.AreEqual(buttonSnippets[i], viewModel.DifferentButtonsCSharpCode, "Button snippet " + i);
            }

            var imageSnippets = new[]
            {
                MessageBoxImageSnippet(
                    "\tMessageBox.Show(\"Message\", \"Title\", MessageBoxButton.OK, MessageBoxImage.None);"),
                MessageBoxImageSnippet(
                    "\t// MessageBoxImage.Error (also Hand, Stop)",
                    "\tMessageBox.Show(\"An error occurred!\", \"Error\", MessageBoxButton.OK, MessageBoxImage.Error);"),
                MessageBoxImageSnippet(
                    "\t// MessageBoxImage.Question",
                    "\tvar result = MessageBox.Show(\"Do you want to continue?\", \"Question\", MessageBoxButton.YesNo, MessageBoxImage.Question);"),
                MessageBoxImageSnippet(
                    "\t// MessageBoxImage.Warning (also Exclamation)",
                    "\tMessageBox.Show(\"Warning: This action may have consequences.\", \"Warning\", MessageBoxButton.OKCancel, MessageBoxImage.Warning);"),
                MessageBoxImageSnippet(
                    "\t// MessageBoxImage.Information (also Asterisk)",
                    "\tMessageBox.Show(\"Operation completed successfully.\", \"Information\", MessageBoxButton.OK, MessageBoxImage.Information);")
            };

            Assert.AreEqual("<Button Content=\"Show MessageBox\" Click=\"ShowMessageButton_Click\" />", viewModel.DifferentImagesXamlCode);
            for (var i = 0; i < imageSnippets.Length; i++)
            {
                viewModel.SelectedImageIndex = i;
                Assert.AreEqual(imageSnippets[i], viewModel.DifferentImagesCSharpCode, "Image snippet " + i);
            }
        }

        private static string MessageBoxButtonSnippet(params string[] contentLines)
        {
            return Lines("private void ShowMessageBoxButton_Click(object sender, RoutedEventArgs e)", "{") +
                "\n" +
                Lines(contentLines) +
                "\n}";
        }

        private static string MessageBoxImageSnippet(params string[] contentLines)
        {
            return Lines("private void ShowMessageBoxButton_Click(object sender, RoutedEventArgs e)", "{") +
                "\n" +
                Lines(contentLines) +
                "\n}";
        }
    }
}
