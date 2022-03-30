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
    /// IconElementPage.xaml 的交互逻辑
    /// </summary>
    public partial class IconElementPage : Page
    {
        public IconElementPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ControlExampleSubstitution Substitution = new ControlExampleSubstitution
            {
                Key = "ShowAsMonochrome",
            };
            BindingOperations.SetBinding(Substitution, ControlExampleSubstitution.ValueProperty, new Binding
            {
                Source = MonochromeButton,
                Path = new PropertyPath("IsChecked"),
            });
            List<ControlExampleSubstitution> Substitutions = new List<ControlExampleSubstitution>() { Substitution };
            Example1.Substitutions = Substitutions;
        }
    }
}
