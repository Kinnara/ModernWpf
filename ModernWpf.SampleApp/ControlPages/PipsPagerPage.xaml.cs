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
    /// PipsPagerPage.xaml 的交互逻辑
    /// </summary>
    public partial class PipsPagerPage : Page
    {
        public List<string> Pictures = new List<string>()
        {
            "/Assets/SampleMedia/LandscapeImage1.jpg",
            "/Assets/SampleMedia/LandscapeImage2.jpg",
            "/Assets/SampleMedia/LandscapeImage3.jpg",
            "/Assets/SampleMedia/LandscapeImage4.jpg",
            "/Assets/SampleMedia/LandscapeImage5.jpg",
            "/Assets/SampleMedia/LandscapeImage6.jpg",
            "/Assets/SampleMedia/LandscapeImage7.jpg",
            "/Assets/SampleMedia/LandscapeImage8.jpg",
        };

        public PipsPagerPage()
        {
            this.InitializeComponent();
        }

        private void OrientationComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string orientation = e.AddedItems[0].ToString();

            switch (orientation)
            {
                case "Vertical":
                    TestPipsPager2.Orientation = Orientation.Vertical;
                    break;

                case "Horizontal":
                default:
                    TestPipsPager2.Orientation = Orientation.Horizontal;
                    break;
            }
        }

        private void PrevButtonComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string prevButtonVisibility = e.AddedItems[0].ToString();

            switch (prevButtonVisibility)
            {
                case "Visible":
                    TestPipsPager2.PreviousButtonVisibility = PipsPagerButtonVisibility.Visible;
                    break;

                case "VisibleOnPointerOver":
                    TestPipsPager2.PreviousButtonVisibility = PipsPagerButtonVisibility.VisibleOnPointerOver;
                    break;

                case "Collapsed":
                default:
                    TestPipsPager2.PreviousButtonVisibility = PipsPagerButtonVisibility.Collapsed;
                    break;
            }
        }

        private void NextButtonComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string nextButtonVisibility = e.AddedItems[0].ToString();

            switch (nextButtonVisibility)
            {
                case "Visible":
                    TestPipsPager2.NextButtonVisibility = PipsPagerButtonVisibility.Visible;
                    break;

                case "VisibleOnPointerOver":
                    TestPipsPager2.NextButtonVisibility = PipsPagerButtonVisibility.VisibleOnPointerOver;
                    break;

                case "Collapsed":
                default:
                    TestPipsPager2.NextButtonVisibility = PipsPagerButtonVisibility.Collapsed;
                    break;
            }
        }
    }
}
