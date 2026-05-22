using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ModernWpf.Gallery.Pages.WpfGallery.SystemPages
{
    public abstract class SystemPageViewModelBase : INotifyPropertyChanged
    {
        protected SystemPageViewModelBase(string pageTitle, string pageDescription)
        {
            PageTitle = pageTitle;
            PageDescription = pageDescription;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string PageTitle { get; }

        public string PageDescription { get; }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public sealed class FileAndFolderDialogsPageViewModel : SystemPageViewModelBase
    {
        private string _fileContent = "Enter text here to save to a file...";
        private string _multipleFilesPath = "No files selected";
        private string _savedFilePath = "No file saved";
        private string _selectedFolderPath = "No folder selected";
        private string _singleFilePath = "No file selected";

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

    public sealed class MessageBoxPageViewModel : SystemPageViewModelBase
    {
        private string _commonMessagesResult = "No common message shown yet";
        private string _customDefaultResult = "No selection made";
        private string _customTitleResult = "No message shown yet";
        private string _defaultMessageResult = "No message shown yet";
        private string _differentButtonsCSharpCode;
        private string _differentButtonsResult = "No button clicked yet";
        private string _differentImagesCSharpCode;
        private string _differentImagesResult = "No image example shown yet";
        private int _selectedButtonIndex;
        private int _selectedImageIndex;

        public MessageBoxPageViewModel()
            : base("MessageBox", string.Empty)
        {
            UpdateButtonCodeSnippets(0);
            UpdateImageCodeSnippets(0);
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
            get { return "<Button Content=\"Show MessageBox\" Click=\"ShowMessageBoxButton_Click\" />"; }
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
            get { return "<Button Content=\"Show MessageBox\" Click=\"ShowMessageButton_Click\" />"; }
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
            get
            {
                return "<WrapPanel Margin=\"0,0,0,10\">\n" +
                    "    <Button Content=\"Information\" Click=\"ShowInformationButton_Click\" />\n" +
                    "    <Button Content=\"Error\" Click=\"ShowErrorButton_Click\" />\n" +
                    "    <Button Content=\"Warning\" Click=\"ShowWarningButton_Click\" />\n" +
                    "</WrapPanel>";
            }
        }

        public string CommonMessagesCSharpCode
        {
            get
            {
                return "// Information\n" +
                    "private void ShowInformationButton_Click(object sender, RoutedEventArgs e)\n" +
                    "{\n" +
                    "    MessageBox.Show(\"Operation completed successfully.\", \"Information\", MessageBoxButton.OK, MessageBoxImage.Information);\n" +
                    "}\n\n" +
                    "// Error\n" +
                    "private void ShowErrorButton_Click(object sender, RoutedEventArgs e)\n" +
                    "{\n" +
                    "    MessageBox.Show(\"An error occurred!\", \"Error\", MessageBoxButton.OK, MessageBoxImage.Error);\n" +
                    "}\n\n" +
                    "// Warning\n" +
                    "private void ShowWarningButton_Click(object sender, RoutedEventArgs e)\n" +
                    "{\n" +
                    "    MessageBox.Show(\"This action cannot be undone!\", \"Warning\", MessageBoxButton.OKCancel, MessageBoxImage.Warning);\n" +
                    "}";
            }
        }

        public string CustomDefaultResult
        {
            get { return _customDefaultResult; }
            set { SetProperty(ref _customDefaultResult, value); }
        }

        private void UpdateButtonCodeSnippets(int index)
        {
            string content;
            switch (index)
            {
                case 1:
                    content = "\tvar result = MessageBox.Show(\"Message\", \"Title\", MessageBoxButton.OKCancel);\n" +
                        "\tif (result == MessageBoxResult.OK)\n\t{\n\t    // User clicked OK\n\t}";
                    break;
                case 2:
                    content = "\tvar result = MessageBox.Show(\"Message\", \"Title\", MessageBoxButton.AbortRetryIgnore);\n" +
                        "\tif (result == MessageBoxResult.Abort)\n\t{\n\t    // User clicked Abort\n\t}\n" +
                        "\telse if (result == MessageBoxResult.Retry)\n\t{\n\t    // User clicked Retry\n\t}\n" +
                        "\telse if (result == MessageBoxResult.Ignore)\n\t{\n\t    // User clicked Ignore\n\t}";
                    break;
                case 3:
                    content = "\tvar result = MessageBox.Show(\"Message\", \"Title\", MessageBoxButton.YesNoCancel);\n" +
                        "\tif (result == MessageBoxResult.Yes)\n\t{\n\t    // User clicked Yes\n\t}\n" +
                        "\telse if (result == MessageBoxResult.No)\n\t{\n\t    // User clicked No\n\t}";
                    break;
                case 4:
                    content = "\tvar result = MessageBox.Show(\"Message\", \"Title\", MessageBoxButton.YesNo);\n" +
                        "\tif (result == MessageBoxResult.Yes)\n\t{\n\t    // User clicked Yes\n\t}\n" +
                        "\telse if (result == MessageBoxResult.No)\n\t{\n\t    // User clicked No\n\t}";
                    break;
                case 5:
                    content = "\tvar result = MessageBox.Show(\"Message\", \"Title\", MessageBoxButton.RetryCancel);\n" +
                        "\tif (result == MessageBoxResult.Retry)\n\t{\n\t    // User clicked Retry\n\t}";
                    break;
                case 6:
                    content = "\tvar result = MessageBox.Show(\"Message\", \"Title\", MessageBoxButton.CancelTryContinue);\n" +
                        "\tif (result == MessageBoxResult.TryAgain)\n\t{\n\t    // User clicked Try Again\n\t}\n" +
                        "\telse if (result == MessageBoxResult.Continue)\n\t{\n\t    // User clicked Continue\n\t}";
                    break;
                default:
                    content = "\tMessageBox.Show(\"Message\", \"Title\", MessageBoxButton.OK);";
                    break;
            }

            DifferentButtonsCSharpCode = string.Format(DifferentButtonsMessageBoxSampleCSharpCodeString, content);
        }

        private void UpdateImageCodeSnippets(int index)
        {
            string content;
            switch (index)
            {
                case 1:
                    content = "\t// MessageBoxImage.Error (also Hand, Stop)\n" +
                        "\tMessageBox.Show(\"An error occurred!\", \"Error\", MessageBoxButton.OK, MessageBoxImage.Error);";
                    break;
                case 2:
                    content = "\t// MessageBoxImage.Question\n" +
                        "\tvar result = MessageBox.Show(\"Do you want to continue?\", \"Question\", MessageBoxButton.YesNo, MessageBoxImage.Question);";
                    break;
                case 3:
                    content = "\t// MessageBoxImage.Warning (also Exclamation)\n" +
                        "\tMessageBox.Show(\"Warning: This action may have consequences.\", \"Warning\", MessageBoxButton.OKCancel, MessageBoxImage.Warning);";
                    break;
                case 4:
                    content = "\t// MessageBoxImage.Information (also Asterisk)\n" +
                        "\tMessageBox.Show(\"Operation completed successfully.\", \"Information\", MessageBoxButton.OK, MessageBoxImage.Information);";
                    break;
                default:
                    content = "\tMessageBox.Show(\"Message\", \"Title\", MessageBoxButton.OK, MessageBoxImage.None);";
                    break;
            }

            DifferentImagesCSharpCode = string.Format(DifferentImagesMessageBoxSampleCSharpCodeString, content);
        }

        private const string DifferentButtonsMessageBoxSampleCSharpCodeString = "private void ShowMessageBoxButton_Click(object sender, RoutedEventArgs e)\n{{\n{0}\n}}";
        private const string DifferentImagesMessageBoxSampleCSharpCodeString = "private void ShowMessageBoxButton_Click(object sender, RoutedEventArgs e)\n{{\n{0}\n}}";
    }

    public sealed class ClipboardPageViewModel : SystemPageViewModelBase
    {
        private string _copyImageStatus = string.Empty;
        private string _copyStatus = string.Empty;
        private string _clearStatus = string.Empty;
        private string _formatsInfo = string.Empty;
        private string _pastedText = string.Empty;
        private string _pasteImageStatus = string.Empty;

        public ClipboardPageViewModel()
            : base("Clipboard", string.Empty)
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
