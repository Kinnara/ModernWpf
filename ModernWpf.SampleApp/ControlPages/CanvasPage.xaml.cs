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
        private Rectangle Rectangle;

        private Slider TopSlider;
        private Slider LeftSlider;
        private Slider ZSlider;

        private List<ControlExampleSubstitution> Substitutions = new List<ControlExampleSubstitution>();

        public CanvasPage()
        {
            InitializeComponent();
        }

        private void Slider_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is Slider b)
            {
                string name = b.Tag.ToString();

                switch (name)
                {
                    case "TopSlider":
                        TopSlider = b;
                        Rectangle.SetBinding(Canvas.TopProperty, new Binding
                        {
                            Source = TopSlider,
                            Path = new PropertyPath("Value")
                        });
                        ControlExampleSubstitution Substitution1 = new ControlExampleSubstitution
                        {
                            Key = "Left",
                        };
                        BindingOperations.SetBinding(Substitution1, ControlExampleSubstitution.ValueProperty, new Binding
                        {
                            Source = b,
                            Path = new PropertyPath("Value"),
                        });
                        Substitutions.Add(Substitution1);
                        break;
                    case "LeftSlider":
                        LeftSlider = b;
                        Rectangle.SetBinding(Canvas.LeftProperty, new Binding
                        {
                            Source = LeftSlider,
                            Path = new PropertyPath("Value")
                        });
                        ControlExampleSubstitution Substitution2 = new ControlExampleSubstitution
                        {
                            Key = "Top",
                        };
                        BindingOperations.SetBinding(Substitution2, ControlExampleSubstitution.ValueProperty, new Binding
                        {
                            Source = b,
                            Path = new PropertyPath("Value"),
                        });
                        Substitutions.Add(Substitution2);
                        break;
                    case "ZSlider":
                        ZSlider = b;
                        Rectangle.SetBinding(Panel.ZIndexProperty, new Binding
                        {
                            Source = ZSlider,
                            Path = new PropertyPath("Value")
                        });
                        ControlExampleSubstitution Substitution3 = new ControlExampleSubstitution
                        {
                            Key = "Z",
                        };
                        BindingOperations.SetBinding(Substitution3, ControlExampleSubstitution.ValueProperty, new Binding
                        {
                            Source = b,
                            Path = new PropertyPath("Value"),
                        });
                        Substitutions.Add(Substitution3);
                        break;
                }

                if (Substitutions.Count >= 3) { Example1.Substitutions = Substitutions; }
            }
        }

        private void Rectangle_Loaded(object sender, RoutedEventArgs e)
        {
            Rectangle = sender as Rectangle;
        }
    }
}
