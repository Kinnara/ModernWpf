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
    /// MenuBarPage.xaml 的交互逻辑
    /// </summary>
    public partial class MenuBarPage : Page
    {
        public MenuBarPage()
        {
            InitializeComponent();
        }

        private void OnElementClicked(object sender, RoutedEventArgs e)
        {
            var selectedFlyoutItem = sender as MenuItem;
            string exampleNumber = selectedFlyoutItem.Name.Substring(0, 1);
            if (exampleNumber == "o")
            {
                SelectedOptionText.Text = "You clicked: " + (sender as MenuItem).Header;
            }
            else if (exampleNumber == "t")
            {
                SelectedOptionText1.Text = "You clicked: " + (sender as MenuItem).Header;
            }
            else if (exampleNumber == "z")
            {
                SelectedOptionText2.Text = "You clicked: " + (sender as MenuItem).Header;
            }
        }
    }
}
