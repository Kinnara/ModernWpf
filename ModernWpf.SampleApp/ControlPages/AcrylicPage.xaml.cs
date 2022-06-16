using ModernWpf.Controls;
using ModernWpf.Media;
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
    /// AcrylicPage.xaml 的交互逻辑
    /// </summary>
    public partial class AcrylicPage : Page
    {
        public AcrylicPage()
        {
            InitializeComponent();
            Loaded += AcrylicPage_Loaded;
        }

        private void AcrylicPage_Loaded(object sender, RoutedEventArgs e)
        {
            ColorSelectorInApp.SelectedIndex = 0;
        }

        private void ColorSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AcrylicPanel shape = CustomAcrylicShapeInApp;
            shape.TintColor = ((SolidColorBrush)e.AddedItems[0]).Color;
        }
    }
}
