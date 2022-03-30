using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Page = ModernWpf.Controls.Page;

namespace ModernWpf.SampleApp.ControlPages
{
    public partial class CommandBarFlyoutPage : Page
    {
        private CommandBarFlyout CommandBarFlyout1;

        public CommandBarFlyoutPage()
        {
            InitializeComponent();
            CommandBarFlyout1 = (CommandBarFlyout)Resources[nameof(CommandBarFlyout1)];
        }

        private void OnElementClicked(object sender, RoutedEventArgs e)
        {
            // Do custom logic
            SelectedOptionText.Text = "You clicked: " + (sender as AppBarButton).Label;
        }

        private void ShowMenu(bool isTransient)
        {
            CommandBarFlyout1.ShowAt(Image1);
        }

        private void MyImageButton_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            // always show a context menu in standard mode
            ShowMenu(false);
        }

        private void MyImageButton_Click(object sender, RoutedEventArgs e)
        {
            ShowMenu((sender as Button).IsMouseOver);
        }
    }
}
