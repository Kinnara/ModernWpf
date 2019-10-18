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
    /// Interaction logic for ItemsRepeaterPage.xaml
    /// </summary>
    public partial class ItemsRepeaterPage : UserControl
    {
        public ItemsRepeaterPage()
        {
            InitializeComponent();
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            //Debug.Assert(repeater1.VerticalCacheLength == 0);
            DataContext = await Contact.GetContactsAsync();
        }
    }
}
