using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace ModernWpf.Gallery.Pages.WpfGallery.SystemPages
{
    public sealed partial class FileAndFolderDialogsPage : UserControl
    {
        public FileAndFolderDialogsPage()
        {
            ViewModel = new FileAndFolderDialogsPageViewModel();
            DataContext = this;
            InitializeComponent();
        }

        public FileAndFolderDialogsPageViewModel ViewModel { get; }

        private void PickSingleFileButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Select a file",
                Filter = "All files (*.*)|*.*|Text files (*.txt)|*.txt",
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() == true)
            {
                ViewModel.SingleFilePath = openFileDialog.FileName;
            }
        }

        private void PickMultipleFilesButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Select multiple files",
                Filter = "All files (*.*)|*.*|Text files (*.txt)|*.txt",
                Multiselect = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                ViewModel.MultipleFilesPath = "Selected " + openFileDialog.FileNames.Length + " file(s): " + string.Join(", ", openFileDialog.FileNames);
            }
        }

        private void SaveFileButton_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Title = "Save file",
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                DefaultExt = "txt"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    File.WriteAllText(saveFileDialog.FileName, ViewModel.FileContent);
                    ViewModel.SavedFilePath = "File saved successfully: " + saveFileDialog.FileName;
                }
                catch (Exception ex)
                {
                    ViewModel.SavedFilePath = "Error saving file: " + ex.Message;
                }
            }
        }

        private void PickFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var folderName = TryPickFolderWithOpenFolderDialog();
            if (!string.IsNullOrEmpty(folderName))
            {
                ViewModel.SelectedFolderPath = folderName;
            }
        }

        private static string TryPickFolderWithOpenFolderDialog()
        {
            var dialogType = Type.GetType("Microsoft.Win32.OpenFolderDialog, PresentationFramework");
            if (dialogType == null)
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }

            var dialog = Activator.CreateInstance(dialogType);
            dialogType.GetProperty("Title").SetValue(dialog, "Select a folder", null);
            var showDialog = dialogType.GetMethod("ShowDialog", Type.EmptyTypes);
            var result = showDialog.Invoke(dialog, null) as bool?;
            if (result == true)
            {
                return dialogType.GetProperty("FolderName").GetValue(dialog, null) as string;
            }

            return null;
        }
    }
}
