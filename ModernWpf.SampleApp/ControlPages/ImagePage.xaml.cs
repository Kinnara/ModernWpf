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
    /// ImagePage.xaml 的交互逻辑
    /// </summary>
    public partial class ImagePage : Page
    {
        public ImagePage()
        {
            InitializeComponent();
        }

        private void ImageStretch_Checked(object sender, RoutedEventArgs e)
        {
            if (StretchImage != null)
            {
                var strStretch = (sender as RadioButton).Content.ToString();
                var stretch = (Stretch)Enum.Parse(typeof(Stretch), strStretch);
                StretchImage.Stretch = stretch;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ControlExampleSubstitution Substitution = new ControlExampleSubstitution
            {
                Key = "Stretch",
            };
            BindingOperations.SetBinding(Substitution, ControlExampleSubstitution.ValueProperty, new Binding
            {
                Source = StretchImage,
                Path = new PropertyPath("Stretch"),
            });
            List<ControlExampleSubstitution> Substitutions = new List<ControlExampleSubstitution>() { Substitution };
            Example3.Substitutions = Substitutions;
        }
    }
}
