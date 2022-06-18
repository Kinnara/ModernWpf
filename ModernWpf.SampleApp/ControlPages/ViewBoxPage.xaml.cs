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
    /// ViewBoxPage.xaml 的交互逻辑
    /// </summary>
    public partial class ViewBoxPage : Page
    {
        public ViewBoxPage()
        {
            InitializeComponent();
        }

        private void StretchDirectionButton_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && Control1 != null)
            {
                string direction = rb.Tag?.ToString();
                switch (direction)
                {
                    case "UpOnly":
                        Control1.StretchDirection = StretchDirection.UpOnly;
                        break;
                    case "DownOnly":
                        Control1.StretchDirection = StretchDirection.DownOnly;
                        break;
                    case "Both":
                        Control1.StretchDirection = StretchDirection.Both;
                        break;
                }
            }
        }

        private void StretchButton_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && Control1 != null)
            {
                string stretch = rb.Tag?.ToString();
                switch (stretch)
                {
                    case "None":
                        Control1.Stretch = Stretch.None;
                        break;
                    case "Fill":
                        Control1.Stretch = Stretch.Fill;
                        break;
                    case "Uniform":
                        Control1.Stretch = Stretch.Uniform;
                        break;
                    case "UniformToFill":
                        Control1.Stretch = Stretch.UniformToFill;
                        break;
                }
            }
        }
    }
}
