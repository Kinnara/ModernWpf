using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.Gallery.Pages.WpfGallery.SystemPages
{
    public sealed partial class MessageBoxPage : Page
    {
        public MessageBoxPage(MessageBoxPageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
        }

        public MessageBoxPageViewModel ViewModel { get; }

        private void ShowDefaultMessageButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(Window.GetWindow(this), "This is a simple message box!");
            ViewModel.DefaultMessageResult = $"Result: {result}";
        }

        private void ShowCustomTitleButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                Window.GetWindow(this),
                "This is a detailed description of what happened or what action is needed.",
                "Custom Title");
            ViewModel.CustomTitleResult = $"Result: {result}";
        }

        private void ShowButtonFromComboBox_Click(object sender, RoutedEventArgs e)
        {
            var buttonType = GetMessageBoxButton(ViewModel.SelectedButtonIndex);
            var buttonName = GetMessageBoxButtonName(ViewModel.SelectedButtonIndex);
            var result = MessageBox.Show(
                Window.GetWindow(this),
                "This MessageBox has " + buttonName + " button(s).",
                buttonName + " Button(s)",
                buttonType);
            ViewModel.DifferentButtonsResult = $"Result: {result}";
        }

        private void ShowImageFromComboBox_Click(object sender, RoutedEventArgs e)
        {
            var imageType = GetMessageBoxImage(ViewModel.SelectedImageIndex);
            var imageName = GetMessageBoxImageName(ViewModel.SelectedImageIndex);
            var result = MessageBox.Show(
                Window.GetWindow(this),
                "This MessageBox displays the " + imageName + " icon.",
                imageName + " Icon",
                MessageBoxButton.OK,
                imageType);
            ViewModel.DifferentImagesResult = $"Result: {result}";
        }

        private void ShowCommonInformation_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(Window.GetWindow(this), "The operation completed successfully.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            ViewModel.CommonMessagesResult = $"Type: Information | Result: {result}";
        }

        private void ShowCommonError_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(Window.GetWindow(this), "An error occurred! The operation could not be completed.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            ViewModel.CommonMessagesResult = $"Type: Error | Result: {result}";
        }

        private void ShowCommonWarning_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(Window.GetWindow(this), "This action cannot be undone! Do you want to continue?", "Warning", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
            ViewModel.CommonMessagesResult = $"Type: Warning | Result: {result}";
        }

        private void ShowCustomDefaultButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                Window.GetWindow(this),
                "Do you want to save changes? Press Enter to select the default 'No' button.",
                "Save Changes",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question,
                MessageBoxResult.No);
            ViewModel.CustomDefaultResult = $"User selected: {result}";
        }

        private static MessageBoxButton GetMessageBoxButton(int index)
        {
            switch (index)
            {
                case 1:
                    return MessageBoxButton.OKCancel;
                case 3:
                    return MessageBoxButton.YesNoCancel;
                case 4:
                    return MessageBoxButton.YesNo;
                default:
                    return MessageBoxButton.OK;
            }
        }

        private static string GetMessageBoxButtonName(int index)
        {
            switch (index)
            {
                case 1:
                    return "OK/Cancel";
                case 2:
                    return "Abort/Retry/Ignore";
                case 3:
                    return "Yes/No/Cancel";
                case 4:
                    return "Yes/No";
                case 5:
                    return "Retry/Cancel";
                case 6:
                    return "Cancel/Try/Continue";
                default:
                    return "OK";
            }
        }

        private static MessageBoxImage GetMessageBoxImage(int index)
        {
            switch (index)
            {
                case 1:
                    return MessageBoxImage.Error;
                case 2:
                    return MessageBoxImage.Question;
                case 3:
                    return MessageBoxImage.Warning;
                case 4:
                    return MessageBoxImage.Information;
                default:
                    return MessageBoxImage.None;
            }
        }

        private static string GetMessageBoxImageName(int index)
        {
            switch (index)
            {
                case 1:
                    return "Error";
                case 2:
                    return "Question";
                case 3:
                    return "Warning";
                case 4:
                    return "Information";
                default:
                    return "None";
            }
        }
    }
}
