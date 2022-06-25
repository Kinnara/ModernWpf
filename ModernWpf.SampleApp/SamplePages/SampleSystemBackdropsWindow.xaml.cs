using ModernWpf.Controls.Primitives;
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
using System.Windows.Shapes;

namespace ModernWpf.SampleApp.SamplePages
{
    /// <summary>
    /// SampleSystemBackdropsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SampleSystemBackdropsWindow : Window
    {
        BackdropType m_currentBackdrop => WindowHelper.GetSystemBackdropType(this);

        public SampleSystemBackdropsWindow()
        {
            InitializeComponent();
        }

        void ChangeBackdropButton_Click(object sender, RoutedEventArgs e)
        {
            BackdropType newType;
            switch (m_currentBackdrop)
            {
                case BackdropType.Mica: newType = BackdropType.Tabbed; break;
                case BackdropType.Tabbed: newType = BackdropType.Acrylic; break;
                case BackdropType.Acrylic: newType = BackdropType.None; break;
                default:
                case BackdropType.None: newType = BackdropType.Mica; break;
            }
            WindowHelper.SetSystemBackdropType(this, newType);
        }
    }
}
