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

namespace ModernWpf.SampleApp.ControlPages
{
    /// <summary>
    /// Interaction logic for ProgressRingPerfPage.xaml
    /// </summary>
    public partial class ProgressRingPerfPage
    {
        public ProgressRingPerfPage()
        {
            InitializeComponent();

            for (int i = 0; i < 300; i++)
            {
                panel.Children.Add(new ProgressRing { Width = 60, Height = 60, IsActive = true });
            }
        }
    }
}
