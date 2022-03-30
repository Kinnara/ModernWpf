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
    /// CanvasPage.xaml 的交互逻辑
    /// </summary>
    public partial class CanvasPage : Page
    {
        private List<ControlExampleSubstitution> Substitutions = new List<ControlExampleSubstitution>();

        public CanvasPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ControlExampleSubstitution Substitution1 = new ControlExampleSubstitution
            {
                Key = "Left",
            };
            BindingOperations.SetBinding(Substitution1, ControlExampleSubstitution.ValueProperty, new Binding
            {
                Source = TopSlider,
                Path = new PropertyPath("Value"),
            });
            ControlExampleSubstitution Substitution2 = new ControlExampleSubstitution
            {
                Key = "Top",
            };
            BindingOperations.SetBinding(Substitution2, ControlExampleSubstitution.ValueProperty, new Binding
            {
                Source = LeftSlider,
                Path = new PropertyPath("Value"),
            });
            ControlExampleSubstitution Substitution3 = new ControlExampleSubstitution
            {
                Key = "Z",
            };
            BindingOperations.SetBinding(Substitution3, ControlExampleSubstitution.ValueProperty, new Binding
            {
                Source = ZSlider,
                Path = new PropertyPath("Value"),
            });
            Example1.Substitutions = new List<ControlExampleSubstitution> { Substitution1, Substitution2, Substitution3 };
        }
    }
}
