using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Gallery.Pages
{
    internal static class SystemSampleFactory
    {
        public static UIElement Create(string uniqueId)
        {
            switch (uniqueId)
            {
                case "Clipboard":
                    return CreateClipboardSample();
                case "ContentIsland":
                    return CreateContentIslandSample();
                case "StoragePickers":
                    return CreateStoragePickersSample();
                default:
                    return null;
            }
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
            var panel = CreateSamplePanel("Storage pickers map to WPF OpenFileDialog and SaveFileDialog, with folder paths represented as validated text input.");
            var selectedFiles = new ListBox
            {
                Width = 520,
                Height = 128
            };
            ControlHelper.SetHeader(selectedFiles, "Selected files");

            var savePath = new TextBox
            {
                Width = 520,
                Margin = new Thickness(0, 12, 0, 0)
            };
            ControlHelper.SetHeader(savePath, "Save path");

            var folderPath = new TextBox
            {
                Width = 520,
                Text = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                Margin = new Thickness(0, 12, 0, 0)
            };
            ControlHelper.SetHeader(folderPath, "Folder path");
            var output = CreateOutput("Ready.");

            var commands = CreateCommandRow();
            var openFile = CreateButton("Open file");
            var saveFile = CreateButton("Save file");
            var useAppFolder = CreateButton("Use app folder");
            var validateFolder = CreateButton("Validate folder");
            openFile.Click += delegate
            {
                var dialog = new OpenFileDialog
                {
                    Title = "Open files",
                    Filter = "All files (*.*)|*.*|Text files (*.txt)|*.txt|Images (*.png;*.jpg)|*.png;*.jpg",
                    Multiselect = true
                };
                if (ShowDialog(dialog, (FrameworkElement)openFile) == true)
                {
                    selectedFiles.ItemsSource = dialog.FileNames;
                    output.Text = "Selected " + dialog.FileNames.Length + " file(s).";
                }
                else
                {
                    output.Text = "Open file picker canceled.";
                }
            };
            saveFile.Click += delegate
            {
                var dialog = new SaveFileDialog
                {
                    Title = "Choose save path",
                    Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                    FileName = "ModernWpfGallery.txt"
                };
                if (ShowDialog(dialog, (FrameworkElement)saveFile) == true)
                {
                    savePath.Text = dialog.FileName;
                    output.Text = "Save path selected.";
                }
                else
                {
                    output.Text = "Save file picker canceled.";
                }
            };
            useAppFolder.Click += delegate
            {
                folderPath.Text = AppDomain.CurrentDomain.BaseDirectory;
                output.Text = "Folder path set to the running app folder.";
            };
            validateFolder.Click += delegate
            {
                output.Text = Directory.Exists(folderPath.Text) ? "Folder exists." : "Folder does not exist.";
            };
            commands.Children.Add(openFile);
            commands.Children.Add(saveFile);
            commands.Children.Add(useAppFolder);
            commands.Children.Add(validateFolder);

            panel.Children.Add(selectedFiles);
            panel.Children.Add(savePath);
            panel.Children.Add(folderPath);
            panel.Children.Add(commands);
            panel.Children.Add(output);
            return panel;
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
