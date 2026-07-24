using ModernWpf.Gallery.Pages.WpfGallery;

namespace ModernWpf.Gallery.Pages.WpfGallery.SystemPages
{
    public abstract class SystemPageViewModelBase : WpfGalleryPageViewModel
    {
        protected SystemPageViewModelBase(string pageTitle, string pageDescription)
            : base(pageTitle, pageDescription)
        {
        }
    }

    /// <summary>
    /// Interaction logic for FileAndFolderDialogsPage.xaml
    /// </summary>
    public partial class FileAndFolderDialogsPageViewModel : SystemPageViewModelBase
    {
        private string _singleFilePath = "No file selected";
        private string _multipleFilesPath = "No files selected";
        private string _fileContent = "Enter text here to save to a file...";
        private string _savedFilePath = "No file saved";
        private string _selectedFolderPath = "No folder selected";

        public FileAndFolderDialogsPageViewModel()
            : base(
                  "File and Folder Dialogs",
                  "Use the OpenFileDialog, SaveFileDialog, and OpenFolderDialog to let users select files and folders in a secure way.")
        {
        }

        public string SingleFilePath
        {
            get { return _singleFilePath; }
            set { SetProperty(ref _singleFilePath, value); }
        }

        public string MultipleFilesPath
        {
            get { return _multipleFilesPath; }
            set { SetProperty(ref _multipleFilesPath, value); }
        }

        public string FileContent
        {
            get { return _fileContent; }
            set { SetProperty(ref _fileContent, value); }
        }

        public string SavedFilePath
        {
            get { return _savedFilePath; }
            set { SetProperty(ref _savedFilePath, value); }
        }

        public string SelectedFolderPath
        {
            get { return _selectedFolderPath; }
            set { SetProperty(ref _selectedFolderPath, value); }
        }
    }

    /// <summary>
    /// Interaction logic for MessageBoxPage.xaml
    /// </summary>
    public partial class MessageBoxPageViewModel : SystemPageViewModelBase
    {
        private string _defaultMessageResult = "No message shown yet";
        private string _customTitleResult = "No message shown yet";
        private int _selectedButtonIndex = 0;
        private string _differentButtonsResult = "No button clicked yet";
        private string _differentButtonsXamlCode = "<Button Content=\"Show MessageBox\" Click=\"ShowMessageBoxButton_Click\" />";
        private string _differentButtonsCSharpCode = string.Format(_differentButtonsMessageBoxSampleCSharpCodeString, "\tMessageBox.Show(\"Message\", \"Title\", MessageBoxButton.OK);");
        private int _selectedImageIndex = 0;
        private string _differentImagesResult = "No image example shown yet";
        private string _differentImagesXamlCode = "<Button Content=\"Show MessageBox\" Click=\"ShowMessageButton_Click\" />";
        private string _differentImagesCSharpCode = string.Format(_differentImagesMessageBoxSampleCSharpCodeString, "\tMessageBox.Show(\"Message\", \"Title\", MessageBoxButton.OK, MessageBoxImage.None);");
        private string _commonMessagesResult = "No common message shown yet";
        private string _commonMessagesXamlCode = @"<WrapPanel Margin=""0,0,0,10"">
    <Button Content=""Information"" Click=""ShowInformationButton_Click"" />
    <Button Content=""Error"" Click=""ShowErrorButton_Click"" />
    <Button Content=""Warning"" Click=""ShowWarningButton_Click"" />
</WrapPanel>";
        private string _commonMessagesCSharpCode = @"// Information
private void ShowInformationButton_Click(object sender, RoutedEventArgs e)
{
    MessageBox.Show(""Operation completed successfully."", ""Information"", MessageBoxButton.OK, MessageBoxImage.Information);
}

// Error
private void ShowErrorButton_Click(object sender, RoutedEventArgs e)
{
    MessageBox.Show(""An error occurred!"", ""Error"", MessageBoxButton.OK, MessageBoxImage.Error);
}

// Warning
private void ShowWarningButton_Click(object sender, RoutedEventArgs e)
{
    MessageBox.Show(""This action cannot be undone!"", ""Warning"", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
}";
        private string _customDefaultResult = "No selection made";

        public MessageBoxPageViewModel()
            : base("MessageBox", "")
        {
        }

        public string DefaultMessageResult
        {
            get { return _defaultMessageResult; }
            set { SetProperty(ref _defaultMessageResult, value); }
        }

        public string CustomTitleResult
        {
            get { return _customTitleResult; }
            set { SetProperty(ref _customTitleResult, value); }
        }

        public int SelectedButtonIndex
        {
            get { return _selectedButtonIndex; }
            set
            {
                if (SetProperty(ref _selectedButtonIndex, value))
                {
                    UpdateButtonCodeSnippets(value);
                }
            }
        }

        public string DifferentButtonsResult
        {
            get { return _differentButtonsResult; }
            set { SetProperty(ref _differentButtonsResult, value); }
        }

        public string DifferentButtonsXamlCode
        {
            get { return _differentButtonsXamlCode; }
            private set { SetProperty(ref _differentButtonsXamlCode, value); }
        }

        public string DifferentButtonsCSharpCode
        {
            get { return _differentButtonsCSharpCode; }
            private set { SetProperty(ref _differentButtonsCSharpCode, value); }
        }

        public int SelectedImageIndex
        {
            get { return _selectedImageIndex; }
            set
            {
                if (SetProperty(ref _selectedImageIndex, value))
                {
                    UpdateImageCodeSnippets(value);
                }
            }
        }

        public string DifferentImagesResult
        {
            get { return _differentImagesResult; }
            set { SetProperty(ref _differentImagesResult, value); }
        }

        public string DifferentImagesXamlCode
        {
            get { return _differentImagesXamlCode; }
            private set { SetProperty(ref _differentImagesXamlCode, value); }
        }

        public string DifferentImagesCSharpCode
        {
            get { return _differentImagesCSharpCode; }
            private set { SetProperty(ref _differentImagesCSharpCode, value); }
        }

        public string CommonMessagesResult
        {
            get { return _commonMessagesResult; }
            set { SetProperty(ref _commonMessagesResult, value); }
        }

        public string CommonMessagesXamlCode
        {
            get { return _commonMessagesXamlCode; }
            private set { SetProperty(ref _commonMessagesXamlCode, value); }
        }

        public string CommonMessagesCSharpCode
        {
            get { return _commonMessagesCSharpCode; }
            private set { SetProperty(ref _commonMessagesCSharpCode, value); }
        }

        public string CustomDefaultResult
        {
            get { return _customDefaultResult; }
            set { SetProperty(ref _customDefaultResult, value); }
        }

        private void UpdateButtonCodeSnippets(int index)
        {
            string content = index switch
            {
                0 => "\tMessageBox.Show(\"Message\", \"Title\", MessageBoxButton.OK);",
                1 => "\tvar result = MessageBox.Show(\"Message\", \"Title\", MessageBoxButton.OKCancel);\n" +
                    "\tif (result == MessageBoxResult.OK)\n\t{\n\t    // User clicked OK\n\t}",
                2 => "\tvar result = MessageBox.Show(\"Message\", \"Title\", MessageBoxButton.AbortRetryIgnore);\n" +
                    "\tif (result == MessageBoxResult.Abort)\n\t{\n\t    // User clicked Abort\n\t}\n" +
                    "\telse if (result == MessageBoxResult.Retry)\n\t{\n\t    // User clicked Retry\n\t}\n" +
                    "\telse if (result == MessageBoxResult.Ignore)\n\t{\n\t    // User clicked Ignore\n\t}",
                3 => "\tvar result = MessageBox.Show(\"Message\", \"Title\", MessageBoxButton.YesNoCancel);\n" +
                    "\tif (result == MessageBoxResult.Yes)\n\t{\n\t    // User clicked Yes\n\t}\n" +
                    "\telse if (result == MessageBoxResult.No)\n\t{\n\t    // User clicked No\n\t}",
                4 => "\tvar result = MessageBox.Show(\"Message\", \"Title\", MessageBoxButton.YesNo);\n" +
                    "\tif (result == MessageBoxResult.Yes)\n\t{\n\t    // User clicked Yes\n\t}\n" +
                    "\telse if (result == MessageBoxResult.No)\n\t{\n\t    // User clicked No\n\t}",
                5 => "\tvar result = MessageBox.Show(\"Message\", \"Title\", MessageBoxButton.RetryCancel);\n" +
                    "\tif (result == MessageBoxResult.Retry)\n\t{\n\t    // User clicked Retry\n\t}",
                6 => "\tvar result = MessageBox.Show(\"Message\", \"Title\", MessageBoxButton.CancelTryContinue);\n" +
                    "\tif (result == MessageBoxResult.TryAgain)\n\t{\n\t    // User clicked Try Again\n\t}\n" +
                    "\telse if (result == MessageBoxResult.Continue)\n\t{\n\t    // User clicked Continue\n\t}",
                _ => "\tMessageBox.Show(\"Message\", \"Title\", MessageBoxButton.OK);"
            };

            DifferentButtonsCSharpCode = string.Format(_differentButtonsMessageBoxSampleCSharpCodeString, content);
        }

        private void UpdateImageCodeSnippets(int index)
        {
            string content = index switch
            {
                0 => "\tMessageBox.Show(\"Message\", \"Title\", MessageBoxButton.OK, MessageBoxImage.None);",
                1 => "\t// MessageBoxImage.Error (also Hand, Stop)\n" +
                    "\tMessageBox.Show(\"An error occurred!\", \"Error\", MessageBoxButton.OK, MessageBoxImage.Error);",
                2 => "\t// MessageBoxImage.Question\n" +
                    "\tvar result = MessageBox.Show(\"Do you want to continue?\", \"Question\", MessageBoxButton.YesNo, MessageBoxImage.Question);",
                3 => "\t// MessageBoxImage.Warning (also Exclamation)\n" +
                    "\tMessageBox.Show(\"Warning: This action may have consequences.\", \"Warning\", MessageBoxButton.OKCancel, MessageBoxImage.Warning);",
                4 => "\t// MessageBoxImage.Information (also Asterisk)\n" +
                    "\tMessageBox.Show(\"Operation completed successfully.\", \"Information\", MessageBoxButton.OK, MessageBoxImage.Information);",
                _ => "\tMessageBox.Show(\"Message\", \"Title\", MessageBoxButton.OK, MessageBoxImage.None);"
            };

            DifferentImagesCSharpCode = string.Format(_differentImagesMessageBoxSampleCSharpCodeString, content);
        }

        private const string _differentButtonsMessageBoxSampleCSharpCodeString = "private void ShowMessageBoxButton_Click(object sender, RoutedEventArgs e)\n{{\n{0}\n}}";
        private const string _differentImagesMessageBoxSampleCSharpCodeString = "private void ShowMessageBoxButton_Click(object sender, RoutedEventArgs e)\n{{\n{0}\n}}";
    }

    public partial class ClipboardPageViewModel : SystemPageViewModelBase
    {
        private string _copyStatus = "";
        private string _pastedText = "";
        private string _clearStatus = "";
        private string _formatsInfo = "";
        private string _copyImageStatus = "";
        private string _pasteImageStatus = "";

        public ClipboardPageViewModel()
            : base("Clipboard", "")
        {
        }

        public string CopyStatus
        {
            get { return _copyStatus; }
            set { SetProperty(ref _copyStatus, value); }
        }

        public string PastedText
        {
            get { return _pastedText; }
            set { SetProperty(ref _pastedText, value); }
        }

        public string ClearStatus
        {
            get { return _clearStatus; }
            set { SetProperty(ref _clearStatus, value); }
        }

        public string FormatsInfo
        {
            get { return _formatsInfo; }
            set { SetProperty(ref _formatsInfo, value); }
        }

        public string CopyImageStatus
        {
            get { return _copyImageStatus; }
            set { SetProperty(ref _copyImageStatus, value); }
        }

        public string PasteImageStatus
        {
            get { return _pasteImageStatus; }
            set { SetProperty(ref _pasteImageStatus, value); }
        }
    }
}
