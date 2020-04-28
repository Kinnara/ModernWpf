using ModernWpf.Controls;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ModernWpf.SampleApp.ControlPages
{
    public partial class ButtonsPage
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
            Button clickedBullet = (Button)sender;
            SymbolIcon symbol = (SymbolIcon)clickedBullet.Content;

            if (symbol.Symbol == Symbol.List)
            {
                mySymbolIcon.Symbol = Symbol.List;
                myListButton.SetValue(AutomationProperties.NameProperty, "Bullets");
            }
            else if (symbol.Symbol == Symbol.Bullets)
            {
                mySymbolIcon.Symbol = Symbol.Bullets;
                myListButton.SetValue(AutomationProperties.NameProperty, "Roman Numerals");
            }

            myListButton.IsChecked = true;
            myListButton.Flyout.Hide();
        }

        private void MyListButton_IsCheckedChanged(ToggleSplitButton sender, ToggleSplitButtonIsCheckedChangedEventArgs args)
        {

        }
    }
}
