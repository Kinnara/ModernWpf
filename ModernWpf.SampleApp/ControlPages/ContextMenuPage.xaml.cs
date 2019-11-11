using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.SampleApp.ControlPages
{
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
