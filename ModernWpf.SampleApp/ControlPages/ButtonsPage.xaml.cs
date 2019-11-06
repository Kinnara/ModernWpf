using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ModernWpf.SampleApp.ControlPages
{
    public partial class ButtonsPage : UserControl
    {
        public ButtonsPage()
        {
            InitializeComponent();
        }

        private void ColorButton_Click(object sender, RoutedEventArgs e)
        {
            // Extract the color of the button that was clicked.
            Button clickedColor = (Button)sender;
            var rectangle = (Rectangle)clickedColor.Content;
            var color = ((SolidColorBrush)rectangle.Fill).Color;

            CurrentColor.Fill = new SolidColorBrush(color);

            myColorButton.Flyout.Hide();
        }

        private void BulletButton_Click(object sender, RoutedEventArgs e)
        {
            myListButton.IsChecked = true;
            myListButton.Flyout.Hide();
        }

        private void MyListButton_IsCheckedChanged(ModernWpf.Controls.ToggleSplitButton sender, ModernWpf.Controls.ToggleSplitButtonIsCheckedChangedEventArgs args)
        {

        }
    }
}
