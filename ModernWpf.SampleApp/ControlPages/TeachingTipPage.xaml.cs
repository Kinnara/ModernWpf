using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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
    /// TeachingTipPage.xaml 的交互逻辑
    /// </summary>
    public partial class TeachingTipPage : Page
    {
        public TeachingTipPage()
        {
            InitializeComponent();
        }

        private void TestButtonClick1(object sender, RoutedEventArgs e)
        {
            if (NavigationRootPage.Current?.PageHeader != null)
            {
                NavigationRootPage.Current.PageHeader.TeachingTip1.IsOpen = true;
            }
        }

        private void TestButtonClick2(object sender, RoutedEventArgs e)
        {
            if (NavigationRootPage.Current?.PageHeader != null)
            {
                NavigationRootPage.Current.PageHeader.TeachingTip2.IsOpen = true;
            }
        }

        private void TestButtonClick3(object sender, RoutedEventArgs e)
        {
            if (NavigationRootPage.Current?.PageHeader != null)
            {
                NavigationRootPage.Current.PageHeader.TeachingTip3.IsOpen = true;
            }
        }
    }
}
