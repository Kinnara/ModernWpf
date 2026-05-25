using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using ModernWpf.Controls.Primitives;
using Mux = ModernWpf.Controls;

namespace ModernWpf.Gallery.Pages
{
    internal static class SystemSampleFactory
    {
        private const string StoragePickSingleFileXaml =
@"<StackPanel Spacing=""8"">
    <Button x:Name=""PickSingleFileButton"" Content=""Pick a single file"" Click=""PickSingleFileButton_Click""/>
    <TextBlock x:Name=""PickedSingleFileTextBlock"" Text=""No file picked""/>
</StackPanel>";

        private const string StoragePickSingleFileCSharp =
@"private async void PickSingleFileButton_Click(object sender, RoutedEventArgs e)
{
    if (sender is Button button)
    {
        //disable the button to avoid double-clicking
        button.IsEnabled = false;

        var picker = new FileOpenPicker(button.XamlRoot.ContentIslandEnvironment.AppWindowId);$(FileType)

        picker.CommitButtonText = ""$(CommitButtonText)"";

        picker.SuggestedStartLocation = PickerLocationId.$(SuggestedStartLocation);

        picker.ViewMode = PickerViewMode.$(ViewMode);

        // Show the picker dialog window
        var file = await picker.PickSingleFileAsync();
        PickedSingleFileTextBlock.Text = file != null
            ? ""Picked: "" + file.Path
            : ""No file selected."";

        //re-enable the button
        button.IsEnabled = true;
    }
}";

        private const string StoragePickMultipleFilesXaml =
@"<StackPanel Spacing=""8"">
    <Button x:Name=""PickMultipleFilesButton"" Content=""Pick multiple files"" Click=""PickMultipleFilesButton_Click""/>
    <TextBlock x:Name=""PickedMultipleFilesTextBlock"" Text=""No files picked""/>
</StackPanel>";

        private const string StoragePickMultipleFilesCSharp =
@"private async void PickMultipleFilesButton_Click(object sender, RoutedEventArgs e)
{
    if (sender is Button button)
    {
        //disable the button to avoid double-clicking
        button.IsEnabled = false;

        var picker = new FileOpenPicker(button.XamlRoot.ContentIslandEnvironment.AppWindowId);$(FileType)

        picker.CommitButtonText = ""$(CommitButtonText)"";

        picker.SuggestedStartLocation = PickerLocationId.$(SuggestedStartLocation);

        picker.ViewMode = PickerViewMode.$(ViewMode);

        // Show the picker dialog window
        var files = await picker.PickMultipleFilesAsync();

        if (files.Count > 0)
        {
            PickedMultipleFilesTextBlock.Text = """";
            foreach (var file in files)
            {
                PickedMultipleFilesTextBlock.Text += ""- Picked: "" + file.Path + Environment.NewLine;
            }
        }
        else
        {
            PickedMultipleFilesTextBlock.Text = ""No files selected."";
        }

        //re-enable the button
        button.IsEnabled = true;
    }
}";

        private const string StorageSaveFileXaml =
@"<StackPanel Spacing=""8"">
    <TextBox x:Name=""FileContentTextBox"" Header=""File content"" TextWrapping=""Wrap"" AcceptsReturn=""True""
                Width=""500"" Height=""200"" Text=""Hello, WinUI!"" IsSpellCheckEnabled=""False"" />
    <Button x:Name=""SaveFileButton"" Content=""Save a file"" Click=""SaveFileButton_Click"" />
    <TextBlock x:Name=""SavedFileTextBlock"" Grid.Column=""1"" Text=""No file saved"" />
</StackPanel>";

        private const string StorageSaveFileCSharp =
@"private async void SaveFileButton_Click(object sender, RoutedEventArgs e)
{
    if (sender is Button button)
    {
        button.IsEnabled = false;

        var picker = new FileSavePicker(button.XamlRoot.ContentIslandEnvironment.AppWindowId);
$(TxtFileType)$(JsonFileType)$(XmlFileType)
        picker.DefaultFileExtension = ""$(DefaultFileExtension)"";

        picker.SuggestedFileName = ""$(SuggestedFileName)"";

        picker.CommitButtonText = ""$(CommitButtonText)"";

        picker.SuggestedStartLocation = PickerLocationId.$(SuggestedStartLocation);

        picker.SuggestedFolder = ""$(SuggestedFolder)"";

        // Show the picker dialog
        var result = await picker.PickSaveFileAsync();

        if (result != null)
        {
            string savePath = result.Path;
            await File.WriteAllTextAsync(savePath, FileContentTextBox.Text);
            SavedFileTextBlock.Text = ""File saved to: "" + savePath;
        }
        else
        {
            SavedFileTextBlock.Text = ""File save canceled."";
        }

        button.IsEnabled = true;
    }
}";

        private const string StoragePickFolderXaml =
@"<StackPanel Spacing=""8"">
    <Button x:Name=""PickFolderButton"" Content=""Pick a folder"" Click=""PickFolderButton_Click"" />
    <TextBlock x:Name=""PickedFolderTextBlock"" Text=""No folder picked"" />
</StackPanel>";

        private const string StoragePickFolderCSharp =
@"private async void PickFolderButton_Click(object sender, RoutedEventArgs e)
{
    if (sender is Button button)
    {
        // disable the button to avoid double-clicking
        button.IsEnabled = false;

        // Clear previous returned folder name
        PickedFolderTextBlock.Text = """";

        var picker = new FolderPicker(button.XamlRoot.ContentIslandEnvironment.AppWindowId);

        picker.CommitButtonText = ""$(CommitButtonText)"";
        picker.SuggestedStartLocation = PickerLocationId.$(SuggestedStartLocation);
        picker.ViewMode = PickerViewMode.$(ViewMode);

        // Show the picker dialog window
        var folder = await picker.PickSingleFolderAsync();
        PickedFolderTextBlock.Text = folder != null
            ? ""Picked: "" + folder.Path
            : ""No folder selected."";

        // re-enable the button
        button.IsEnabled = true;
    }
}";

        private static readonly string[] PickerLocationIds =
        {
            "DocumentsLibrary",
            "ComputerFolder",
            "Desktop",
            "Downloads",
            "MusicLibrary",
            "PicturesLibrary",
            "VideosLibrary"
        };

        private static readonly string[] PickerViewModes =
        {
            "List",
            "Thumbnail"
        };

        public static UIElement Create(string uniqueId)
        {
            switch (uniqueId)
            {
                case "Clipboard":
                    return CreateClipboardSample();
                case "ContentIsland":
                    return CreateContentIslandSample();
                case "FileAndFolderDialogs":
                    return CreateFileAndFolderDialogsSample();
                case "MessageBox":
                    return CreateMessageBoxSample();
                case "StoragePickers":
                    return CreateStoragePickersSample();
                default:
                    return null;
            }
        }

        public static IReadOnlyList<GalleryExample> CreateExamples(string uniqueId)
        {
            switch (uniqueId)
            {
                case "StoragePickers":
                    return CreateStoragePickersExamples();
                default:
                    return Array.Empty<GalleryExample>();
            }
        }

        public static UIElement CreateIntroContent(string uniqueId)
        {
            switch (uniqueId)
            {
                case "StoragePickers":
                    return CreateStoragePickersIntroContent();
                default:
                    return null;
            }
        }

        private static UIElement CreateMessageBoxSample()
        {
            var panel = CreateSamplePanel("MessageBox displays a short modal message with standard buttons, icons, and result values.");
            var output = CreateOutput("No dialog result yet.");
            var row = CreateCommandRow();
            var showInfo = CreateButton("Show message");
            var showQuestion = CreateButton("Ask question");
            showInfo.Click += delegate
            {
                var result = System.Windows.MessageBox.Show(
                    Window.GetWindow((FrameworkElement)showInfo),
                    "The operation completed successfully.",
                    "ModernWpf Gallery",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                output.Text = "Message result: " + result;
            };
            showQuestion.Click += delegate
            {
                var result = System.Windows.MessageBox.Show(
                    Window.GetWindow((FrameworkElement)showQuestion),
                    "Do you want to continue?",
                    "ModernWpf Gallery",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);
                output.Text = "Question result: " + result;
            };
            row.Children.Add(showInfo);
            row.Children.Add(showQuestion);
            panel.Children.Add(row);
            panel.Children.Add(output);
            return panel;
        }

        private static UIElement CreateFileAndFolderDialogsSample()
        {
            return CreateStoragePickersSample();
        }

        private static UIElement CreateClipboardSample()
        {
            var panel = CreateSamplePanel("Clipboard maps to WPF Clipboard and DataObject APIs for text and multi-format content.");
            var input = new TextBox
            {
                Width = 420,
                Text = "ModernWpf Gallery clipboard sample",
                TextWrapping = TextWrapping.Wrap
            };
            ControlHelper.SetHeader(input, "Clipboard text");

            var formats = new ListBox
            {
                Width = 420,
                Height = 120,
                Margin = new Thickness(0, 12, 0, 0)
            };
            ControlHelper.SetHeader(formats, "Available formats");
            var output = CreateOutput("Ready.");

            Action refreshFormats = delegate
            {
                try
                {
                    var data = Clipboard.GetDataObject();
                    formats.ItemsSource = data == null ? Array.Empty<string>() : data.GetFormats().OrderBy(format => format).ToArray();
                    output.Text = "Clipboard formats refreshed.";
                }
                catch (Exception ex)
                {
                    output.Text = "Clipboard read failed: " + ex.Message;
                }
            };

            var commands = CreateCommandRow();
            var copyText = CreateButton("Copy text");
            var pasteText = CreateButton("Paste text");
            var copyPackage = CreateButton("Copy package");
            var clear = CreateButton("Clear");
            copyText.Click += delegate
            {
                try
                {
                    Clipboard.SetText(input.Text ?? string.Empty);
                    output.Text = "Copied text to the clipboard.";
                    refreshFormats();
                }
                catch (Exception ex)
                {
                    output.Text = "Clipboard write failed: " + ex.Message;
                }
            };
            pasteText.Click += delegate
            {
                try
                {
                    input.Text = Clipboard.ContainsText() ? Clipboard.GetText() : string.Empty;
                    output.Text = string.IsNullOrEmpty(input.Text) ? "Clipboard did not contain text." : "Pasted text from the clipboard.";
                    refreshFormats();
                }
                catch (Exception ex)
                {
                    output.Text = "Clipboard paste failed: " + ex.Message;
                }
            };
            copyPackage.Click += delegate
            {
                try
                {
                    var data = new DataObject();
                    data.SetText(input.Text ?? string.Empty);
                    data.SetData("ModernWpf.Sample.Timestamp", DateTime.Now.ToString("O"));
                    data.SetData(DataFormats.CommaSeparatedValue, "Name,Value" + Environment.NewLine + "Sample,ModernWpf");
                    Clipboard.SetDataObject(data, true);
                    output.Text = "Copied multi-format DataObject to the clipboard.";
                    refreshFormats();
                }
                catch (Exception ex)
                {
                    output.Text = "Clipboard package write failed: " + ex.Message;
                }
            };
            clear.Click += delegate
            {
                try
                {
                    Clipboard.Clear();
                    output.Text = "Clipboard cleared.";
                    refreshFormats();
                }
                catch (Exception ex)
                {
                    output.Text = "Clipboard clear failed: " + ex.Message;
                }
            };
            commands.Children.Add(copyText);
            commands.Children.Add(pasteText);
            commands.Children.Add(copyPackage);
            commands.Children.Add(clear);

            panel.Children.Add(input);
            panel.Children.Add(commands);
            panel.Children.Add(formats);
            panel.Children.Add(output);
            refreshFormats();
            return panel;
        }

        private static UIElement CreateContentIslandSample()
        {
            var panel = CreateSamplePanel("ContentIsland is represented as a WPF hosted-content surface with attach, detach, and focus lifecycle controls.");
            var hostedInput = new TextBox
            {
                Text = "Hosted input",
                Width = 220,
                Margin = new Thickness(0, 10, 0, 0)
            };
            var hostedProgress = new ProgressBar
            {
                Width = 220,
                Height = 8,
                Value = 62,
                Maximum = 100,
                Margin = new Thickness(0, 12, 0, 0)
            };
            var hostedContent = new StackPanel
            {
                Children =
                {
                    new TextBlock
                    {
                        Text = "Hosted content",
                        FontWeight = FontWeights.SemiBold,
                        FontSize = 18
                    },
                    new TextBlock
                    {
                        Text = "This surface models an island boundary that can be attached, detached, and focused.",
                        TextWrapping = TextWrapping.Wrap,
                        Opacity = 0.72,
                        Margin = new Thickness(0, 6, 0, 0)
                    },
                    hostedInput,
                    hostedProgress
                }
            };
            var island = new Border
            {
                Width = 460,
                MinHeight = 190,
                Padding = new Thickness(18),
                BorderThickness = new Thickness(1),
                BorderBrush = CreateBrush("#D8D8D8"),
                Background = CreateBrush("#FAFAFA"),
                Child = hostedContent
            };
            var output = CreateOutput("Island attached.");

            var commands = CreateCommandRow();
            var attach = CreateButton("Attach");
            var detach = CreateButton("Detach");
            var focus = CreateButton("Focus hosted input");
            attach.Click += delegate
            {
                island.Child = hostedContent;
                island.Background = CreateBrush("#FAFAFA");
                output.Text = "Island attached.";
            };
            detach.Click += delegate
            {
                island.Child = new TextBlock
                {
                    Text = "Island detached",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Opacity = 0.72
                };
                island.Background = CreateBrush("#F3F3F3");
                output.Text = "Island detached.";
            };
            focus.Click += delegate
            {
                if (island.Child == hostedContent)
                {
                    hostedInput.Focus();
                    output.Text = "Focus moved into hosted content.";
                }
                else
                {
                    output.Text = "Attach the island before moving focus.";
                }
            };
            commands.Children.Add(attach);
            commands.Children.Add(detach);
            commands.Children.Add(focus);

            panel.Children.Add(island);
            panel.Children.Add(commands);
            panel.Children.Add(output);
            return panel;
        }

        private static UIElement CreateStoragePickersSample()
        {
            return CreatePickSingleFileExampleContent(assignRootAutomationId: true);
        }

        private static UIElement CreateStoragePickersIntroContent()
        {
            return new Mux.InfoBar
            {
                IsClosable = false,
                IsOpen = true,
                Margin = new Thickness(0, 8, 0, 0),
                Message = "The picker reopens in the last selected location and view. The SuggestedStartLocation and ViewMode are only applied the first time the picker is opened (for example, right after app installation or when no previous selection exists)."
            };
        }

        private static IReadOnlyList<GalleryExample> CreateStoragePickersExamples()
        {
            return new[]
            {
                new GalleryExample(
                    "Pick single file",
                    CreatePickSingleFileExampleContent(assignRootAutomationId: true),
                    StoragePickSingleFileXaml,
                    StoragePickSingleFileCSharp),
                new GalleryExample(
                    "Pick multiple files",
                    CreatePickMultipleFilesExampleContent(),
                    StoragePickMultipleFilesXaml,
                    StoragePickMultipleFilesCSharp),
                new GalleryExample(
                    "Save file",
                    CreateSaveFileExampleContent(),
                    StorageSaveFileXaml,
                    StorageSaveFileCSharp),
                new GalleryExample(
                    "Pick folder",
                    CreatePickFolderExampleContent(),
                    StoragePickFolderXaml,
                    StoragePickFolderCSharp)
            };
        }

        private static GallerySamplePanel CreatePickSingleFileExampleContent(bool assignRootAutomationId)
        {
            var root = CreateStoragePickerExampleRoot(assignRootAutomationId);
            var pickedText = new TextBlock
            {
                Name = "PickedSingleFileTextBlock",
                Text = "No file picked",
                Margin = new Thickness(0, 8, 0, 0)
            };
            var button = CreateSourceButton("StoragePickers", "PickSingleFileButton", "Pick a single file");
            button.Click += delegate
            {
                button.IsEnabled = false;
                try
                {
                    var dialog = new OpenFileDialog
                    {
                        Title = "Pick a single file",
                        Filter = GetOpenFileDialogFilter(FindNamedElement<ComboBox>(root, "FileTypeComboBox1")),
                        Multiselect = false
                    };
                    pickedText.Text = ShowDialog(dialog, button) == true
                        ? "Picked: " + dialog.FileName
                        : "No file selected.";
                }
                finally
                {
                    button.IsEnabled = true;
                }
            };

            var example = CreateStackPanelWithSpacing();
            example.Children.Add(button);
            example.Children.Add(pickedText);
            root.Children.Add(CreateStoragePickerExampleLayout(example, CreateOpenPickerOptions("1", "Pick File")));
            return root;
        }

        private static GallerySamplePanel CreatePickMultipleFilesExampleContent()
        {
            var root = CreateStoragePickerExampleRoot(assignRootAutomationId: false);
            var pickedText = new TextBlock
            {
                Name = "PickedMultipleFilesTextBlock",
                Text = "No files picked",
                Margin = new Thickness(0, 8, 0, 0),
                TextWrapping = TextWrapping.Wrap
            };
            var button = CreateSourceButton("StoragePickers", "PickMultipleFilesButton", "Pick multiple files");
            button.Click += delegate
            {
                button.IsEnabled = false;
                try
                {
                    var dialog = new OpenFileDialog
                    {
                        Title = "Pick multiple files",
                        Filter = GetOpenFileDialogFilter(FindNamedElement<ComboBox>(root, "FileTypeComboBox2")),
                        Multiselect = true
                    };
                    if (ShowDialog(dialog, button) == true && dialog.FileNames.Length > 0)
                    {
                        pickedText.Text = string.Join(Environment.NewLine, dialog.FileNames.Select(path => "- Picked: " + path));
                    }
                    else
                    {
                        pickedText.Text = "No files selected.";
                    }
                }
                finally
                {
                    button.IsEnabled = true;
                }
            };

            var example = CreateStackPanelWithSpacing();
            example.Children.Add(button);
            example.Children.Add(pickedText);
            root.Children.Add(CreateStoragePickerExampleLayout(example, CreateOpenPickerOptions("2", "Pick Files")));
            return root;
        }

        private static GallerySamplePanel CreateSaveFileExampleContent()
        {
            var root = CreateStoragePickerExampleRoot(assignRootAutomationId: false);
            var fileContent = new TextBox
            {
                Name = "FileContentTextBox",
                Width = 500,
                Height = 200,
                Text = "Hello, WinUI!",
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            ControlHelper.SetHeader(fileContent, "File content");

            var savedText = new TextBlock
            {
                Name = "SavedFileTextBlock",
                Text = "No file saved",
                Margin = new Thickness(0, 8, 0, 0),
                TextWrapping = TextWrapping.Wrap
            };

            var button = CreateSourceButton("StoragePickers", "SaveFileButton", "Save a file");
            button.Margin = new Thickness(0, 8, 0, 0);
            button.Click += delegate
            {
                button.IsEnabled = false;
                try
                {
                    var suggestedName = FindNamedElement<TextBox>(root, "SuggestedFileNameTextBox");
                    var defaultExtension = FindNamedElement<ComboBox>(root, "DefaultExtensionComboBox");
                    var dialog = new SaveFileDialog
                    {
                        Title = "Save a file",
                        Filter = GetSaveFileDialogFilter(root),
                        DefaultExt = Convert.ToString(defaultExtension.SelectedItem),
                        FileName = string.IsNullOrEmpty(suggestedName.Text) ? "NewDocument" : suggestedName.Text
                    };
                    if (ShowDialog(dialog, button) == true)
                    {
                        File.WriteAllText(dialog.FileName, fileContent.Text ?? string.Empty);
                        savedText.Text = "File saved to: " + dialog.FileName;
                    }
                    else
                    {
                        savedText.Text = "File save canceled.";
                    }
                }
                finally
                {
                    button.IsEnabled = true;
                }
            };

            var example = CreateStackPanelWithSpacing();
            example.Children.Add(fileContent);
            example.Children.Add(button);
            example.Children.Add(savedText);
            root.Children.Add(CreateStoragePickerExampleLayout(example, CreateSavePickerOptions(root)));
            return root;
        }

        private static GallerySamplePanel CreatePickFolderExampleContent()
        {
            var root = CreateStoragePickerExampleRoot(assignRootAutomationId: false);
            var pickedText = new TextBlock
            {
                Name = "PickedFolderTextBlock",
                Text = "No folder picked",
                Margin = new Thickness(0, 8, 0, 0),
                TextWrapping = TextWrapping.Wrap
            };
            var button = CreateSourceButton("StoragePickers", "PickFolderButton", "Pick a folder");
            button.Click += delegate
            {
                button.IsEnabled = false;
                try
                {
                    var folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    pickedText.Text = string.IsNullOrEmpty(folder) ? "No folder selected." : "Picked: " + folder;
                }
                finally
                {
                    button.IsEnabled = true;
                }
            };

            var example = CreateStackPanelWithSpacing();
            example.Children.Add(button);
            example.Children.Add(pickedText);
            root.Children.Add(CreateStoragePickerExampleLayout(example, CreateFolderPickerOptions()));
            return root;
        }

        private static GallerySamplePanel CreateStoragePickerExampleRoot(bool assignRootAutomationId)
        {
            var root = new GallerySamplePanel();
            if (assignRootAutomationId)
            {
                GalleryAutomation.WithAutomationId(root, GalleryAutomation.SampleRootId("StoragePickers"));
            }

            return root;
        }

        private static Grid CreateStoragePickerExampleLayout(UIElement example, UIElement options)
        {
            var layout = new Grid();
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            Grid.SetColumn(example, 0);
            Grid.SetColumn(options, 1);
            layout.Children.Add(example);
            layout.Children.Add(options);
            return layout;
        }

        private static StackPanel CreateOpenPickerOptions(string suffix, string commitText)
        {
            var options = CreateOptionsPanel();
            options.Children.Add(CreateFileTypeComboBox("FileTypeComboBox" + suffix));
            options.Children.Add(CreateTextBox("CommitButtonTextTextBox" + (suffix == "1" ? string.Empty : suffix), "Commit button text", commitText, "Open"));
            options.Children.Add(CreatePickerLocationComboBox("PickerLocationComboBox" + suffix));
            options.Children.Add(CreatePickerViewModeComboBox("PickerViewModeComboBox" + suffix));
            return options;
        }

        private static StackPanel CreateSavePickerOptions(DependencyObject root)
        {
            var options = CreateOptionsPanel();
            options.Children.Add(new TextBlock { Text = "File types:" });
            options.Children.Add(new CheckBox { Name = "TxtCheckBox", Content = "Text Files (*.txt)" });
            options.Children.Add(new CheckBox { Name = "JsonCheckBox", Content = "JSON Files (*.json)" });
            options.Children.Add(new CheckBox { Name = "XmlCheckBox", Content = "XML Files (*.xml)" });

            var defaultExtension = new ComboBox
            {
                Name = "DefaultExtensionComboBox",
                Width = 200,
                SelectedIndex = 0,
                Margin = new Thickness(0, 8, 0, 0)
            };
            ControlHelper.SetHeader(defaultExtension, "Default extension");
            defaultExtension.Items.Add(".txt");
            defaultExtension.Items.Add(".json");
            defaultExtension.Items.Add(".xml");
            options.Children.Add(defaultExtension);

            options.Children.Add(CreateTextBox("SuggestedFileNameTextBox", "Suggested file name", "NewDocument", null));
            options.Children.Add(CreateTextBox("CommitButtonTextTextBox3", "Commit button text", "Save File", "Save"));
            options.Children.Add(CreatePickerLocationComboBox("PickerLocationComboBox3"));

            var suggestedFolderGrid = new Grid();
            suggestedFolderGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            suggestedFolderGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            var suggestedFolder = CreateTextBox("SuggestedFolderTextBox", "Suggested folder ", string.Empty, "Optional");
            suggestedFolder.Width = 148;
            suggestedFolder.IsReadOnly = true;
            suggestedFolderGrid.Children.Add(suggestedFolder);
            var selectFolder = new Button
            {
                Name = "SelectSuggestedFolderButton",
                Margin = new Thickness(8, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Bottom,
                Content = "...",
                ToolTip = "Select folder"
            };
            AutomationProperties.SetName(selectFolder, "Select folder");
            selectFolder.Click += delegate
            {
                suggestedFolder.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            };
            Grid.SetColumn(selectFolder, 1);
            suggestedFolderGrid.Children.Add(selectFolder);
            options.Children.Add(suggestedFolderGrid);
            return options;
        }

        private static StackPanel CreateFolderPickerOptions()
        {
            var options = CreateOptionsPanel();
            options.Children.Add(CreateTextBox("CommitButtonTextTextBox4", "Commit button text", "Pick Folder", "Select Folder"));
            options.Children.Add(CreatePickerLocationComboBox("PickerLocationComboBox4"));
            options.Children.Add(CreatePickerViewModeComboBox("PickerViewModeComboBox3"));
            return options;
        }

        private static StackPanel CreateOptionsPanel()
        {
            return new StackPanel
            {
                Width = 240,
                Orientation = Orientation.Vertical,
                Margin = new Thickness(24, 0, 0, 0)
            };
        }

        private static StackPanel CreateStackPanelWithSpacing()
        {
            return new StackPanel
            {
                Orientation = Orientation.Vertical
            };
        }

        private static ComboBox CreateFileTypeComboBox(string name)
        {
            var comboBox = new ComboBox
            {
                Name = name,
                Width = 200
            };
            ControlHelper.SetHeader(comboBox, "File type");
            comboBox.Items.Add(CreateComboBoxItem("All Files (*)", "*", true));
            comboBox.Items.Add(CreateComboBoxItem("Text Files (*.txt)", ".txt", false));
            comboBox.Items.Add(CreateComboBoxItem("Images (*.jpg, *.png)", "images", false));
            comboBox.SelectedIndex = 0;
            return comboBox;
        }

        private static ComboBoxItem CreateComboBoxItem(string content, string tag, bool isSelected)
        {
            return new ComboBoxItem
            {
                Content = content,
                Tag = tag,
                IsSelected = isSelected
            };
        }

        private static TextBox CreateTextBox(string name, string header, string text, string placeholder)
        {
            var textBox = new TextBox
            {
                Name = name,
                Text = text,
                Margin = new Thickness(0, 8, 0, 0)
            };
            ControlHelper.SetHeader(textBox, header);
            if (!string.IsNullOrEmpty(placeholder))
            {
                ControlHelper.SetPlaceholderText(textBox, placeholder);
            }

            return textBox;
        }

        private static ComboBox CreatePickerLocationComboBox(string name)
        {
            var comboBox = new ComboBox
            {
                Name = name,
                Width = 200,
                ItemsSource = PickerLocationIds,
                SelectedIndex = 0,
                Margin = new Thickness(0, 8, 0, 0)
            };
            ControlHelper.SetHeader(comboBox, "Suggested start location");
            return comboBox;
        }

        private static ComboBox CreatePickerViewModeComboBox(string name)
        {
            var comboBox = new ComboBox
            {
                Name = name,
                Width = 200,
                ItemsSource = PickerViewModes,
                SelectedIndex = 0,
                Margin = new Thickness(0, 8, 0, 0)
            };
            ControlHelper.SetHeader(comboBox, "View mode");
            return comboBox;
        }

        private static Button CreateSourceButton(string uniqueId, string name, string content)
        {
            var button = new Button
            {
                Name = name,
                Content = content,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            AutomationProperties.SetName(button, content);
            GalleryAutomation.WithAutomationId(button, GalleryAutomation.SampleElementId(uniqueId, name));
            return button;
        }

        private static string GetOpenFileDialogFilter(ComboBox fileTypeComboBox)
        {
            var item = fileTypeComboBox == null ? null : fileTypeComboBox.SelectedItem as ComboBoxItem;
            switch (item == null ? null : item.Tag as string)
            {
                case ".txt":
                    return "Text Files (*.txt)|*.txt";
                case "images":
                    return "Images (*.jpg;*.png)|*.jpg;*.png";
                default:
                    return "All Files (*.*)|*.*";
            }
        }

        private static string GetSaveFileDialogFilter(DependencyObject root)
        {
            var filters = new List<string>();
            var txtCheckBox = FindNamedElement<CheckBox>(root, "TxtCheckBox");
            var jsonCheckBox = FindNamedElement<CheckBox>(root, "JsonCheckBox");
            var xmlCheckBox = FindNamedElement<CheckBox>(root, "XmlCheckBox");
            if (txtCheckBox != null && txtCheckBox.IsChecked == true)
            {
                filters.Add("Text Files (*.txt)|*.txt");
            }
            if (jsonCheckBox != null && jsonCheckBox.IsChecked == true)
            {
                filters.Add("JSON Files (*.json)|*.json");
            }
            if (xmlCheckBox != null && xmlCheckBox.IsChecked == true)
            {
                filters.Add("XML Files (*.xml)|*.xml");
            }

            return filters.Count == 0 ? "All Files (*.*)|*.*" : string.Join("|", filters);
        }

        private static T FindNamedElement<T>(DependencyObject root, string name)
            where T : FrameworkElement
        {
            if (root == null)
            {
                return null;
            }

            var frameworkElement = root as FrameworkElement;
            var typedElement = frameworkElement as T;
            if (frameworkElement != null && frameworkElement.Name == name && typedElement != null)
            {
                return typedElement;
            }

            var childCount = VisualTreeHelper.GetChildrenCount(root);
            for (var i = 0; i < childCount; i++)
            {
                var match = FindNamedElement<T>(VisualTreeHelper.GetChild(root, i), name);
                if (match != null)
                {
                    return match;
                }
            }

            return null;
        }

        private static bool? ShowDialog(FileDialog dialog, FrameworkElement ownerElement)
        {
            var owner = Window.GetWindow(ownerElement);
            return owner == null ? dialog.ShowDialog() : dialog.ShowDialog(owner);
        }

        private static StackPanel CreateCommandRow()
        {
            return new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 12, 0, 0)
            };
        }

        private static StackPanel CreateSamplePanel(string description)
        {
            var panel = new StackPanel
            {
                Margin = new Thickness(0, 0, 0, 12)
            };
            panel.Children.Add(new TextBlock
            {
                Text = description,
                TextWrapping = TextWrapping.Wrap,
                Opacity = 0.72,
                Margin = new Thickness(0, 0, 0, 12)
            });
            return panel;
        }

        private static Button CreateButton(string text)
        {
            return new Button
            {
                Content = text,
                Padding = new Thickness(16, 6, 16, 6),
                Margin = new Thickness(0, 0, 8, 0),
                HorizontalAlignment = HorizontalAlignment.Left
            };
        }

        private static TextBlock CreateOutput(string text)
        {
            return new TextBlock
            {
                Text = text,
                Margin = new Thickness(0, 12, 0, 0),
                TextWrapping = TextWrapping.Wrap
            };
        }

        private static SolidColorBrush CreateBrush(string color)
        {
            return (SolidColorBrush)new BrushConverter().ConvertFromString(color);
        }
    }
}
