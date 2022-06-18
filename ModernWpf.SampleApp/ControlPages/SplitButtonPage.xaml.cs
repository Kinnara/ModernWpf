using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    /// <summary>
    /// SplitButtonPage.xaml 的交互逻辑
    /// </summary>
    public partial class SplitButtonPage : Page
    {
        private Color currentColor = Colors.Green;

        public SplitButtonPage()
        {
            InitializeComponent();
            myRichEditBox.Foreground = new SolidColorBrush(currentColor);
            myRichEditBox.Selection.Text=
                "Lorem ipsum dolor sit amet, consectetur adipiscing elit, " +
                "sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Tempor commodo ullamcorper a lacus.";
        }

        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var rect = (Rectangle)e.ClickedItem;
            var color = ((SolidColorBrush)rect.Fill).Color;
            myRichEditBox.Foreground = new SolidColorBrush(color);
            CurrentColor.Background = new SolidColorBrush(color);

            myRichEditBox.Focus();
            currentColor = color;

            // Delay required to circumvent GridView bug: https://github.com/microsoft/microsoft-ui-xaml/issues/6350
            Task.Delay(10).ContinueWith(_ => myColorButton.Flyout.Hide(), TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void RevealColorButton_Click(object sender, RoutedEventArgs e)
        {
            myColorButtonReveal.Flyout.Hide();
        }

        private void myColorButton_Click(SplitButton sender, SplitButtonClickEventArgs args)
        {
            var border = (Border)sender.Content;
            var color = ((SolidColorBrush)border.Background).Color;

            myRichEditBox.Foreground = new SolidColorBrush(color);
            currentColor = color;
        }

        private void MyRichEditBox_TextChanged(object sender, RoutedEventArgs e)
        {
            if (((SolidColorBrush)myRichEditBox.Foreground).Color != currentColor)
            {
                myRichEditBox.Foreground = new SolidColorBrush(currentColor);
            }
        }
    }
}
