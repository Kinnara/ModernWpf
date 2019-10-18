using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace ModernWpf.SampleApp.ControlPages
{
    /// <summary>
    /// Interaction logic for ContextMenuPage.xaml
    /// </summary>
    public partial class ContextMenuPage
    {
        public ContextMenuPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //output.Text = System.Windows.Markup.XamlWriter.Save(button.ContextMenu.Template);
        }

        private void MenuItem_Loaded(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem)sender;
            Debug.WriteLine(menuItem.Role);
            Debug.WriteLine(menuItem.IsChecked);
        }

        private void LayoutPanel_Click(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is Button button)
            {
                var menu = button.ContextMenu;
                if (menu != null)
                {
                    menu.IsOpen = true;
                }
            }
        }
    }
}
