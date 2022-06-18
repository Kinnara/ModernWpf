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
using Windows.UI.Xaml.Controls.Primitives;
using Page = ModernWpf.Controls.Page;

namespace ModernWpf.SampleApp.ControlPages
{
    /// <summary>
    /// ScrollViewerPage.xaml 的交互逻辑
    /// </summary>
    public partial class ScrollViewerPage : Page
    {
        public ScrollViewerPage()
        {
            InitializeComponent();
        }

        private void hsbvCombo_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            if (Control1 != null)
            {
                if (sender is ComboBox cb)
                {
                    switch (cb.SelectedIndex)
                    {
                        case 0: // Auto
                            Control1.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                            break;
                        case 1: //Visible
                            Control1.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
                            break;
                        case 2: // Hidden
                            Control1.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
                            break;
                        case 3: // Disabled
                            Control1.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                            break;
                        default:
                            Control1.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                            break;
                    }
                }
            }
        }

        private void vsbvCombo_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            if (Control1 != null)
            {
                if (sender is ComboBox cb)
                {
                    switch (cb.SelectedIndex)
                    {
                        case 0: // Auto
                            Control1.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                            break;
                        case 1: //Visible
                            Control1.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
                            break;
                        case 2: // Hidden
                            Control1.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
                            break;
                        case 3: // Disabled
                            Control1.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
                            break;
                        default:
                            Control1.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
                            break;
                    }
                }
            }
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (Grid.GetColumnSpan(Control1) == 1)
            {
                Control1.Width = (e.NewSize.Width / 2) - 50;
            }
            else
            {
                Control1.Width = e.NewSize.Width - 50;
            }

        }
    }
}
