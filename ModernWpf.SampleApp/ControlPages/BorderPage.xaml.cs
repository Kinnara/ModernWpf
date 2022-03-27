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
    /// BorderPage.xaml 的交互逻辑
    /// </summary>
    public partial class BorderPage : Page
    {
        private Border Control1;

        public BorderPage()
        {
            InitializeComponent();
        }

        private void ThicknessSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Control1 != null) Control1.BorderThickness = new Thickness(e.NewValue);
        }

        private void BGRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && Control1 != null)
            {
                string colorName = rb.Content.ToString();
                switch (colorName)
                {
                    case "Yellow":
                        Control1.Background = new SolidColorBrush(Colors.Yellow);
                        break;
                    case "Green":
                        Control1.Background = new SolidColorBrush(Colors.Green);
                        break;
                    case "Blue":
                        Control1.Background = new SolidColorBrush(Colors.Blue);
                        break;
                    case "White":
                        Control1.Background = new SolidColorBrush(Colors.White);
                        break;
                }
            }
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && Control1 != null)
            {
                string colorName = rb.Content.ToString();
                switch (colorName)
                {
                    case "Yellow":
                        Control1.BorderBrush = new SolidColorBrush(Colors.Gold);
                        break;
                    case "Green":
                        Control1.BorderBrush = new SolidColorBrush(Colors.DarkGreen);
                        break;
                    case "Blue":
                        Control1.BorderBrush = new SolidColorBrush(Colors.DarkBlue);
                        break;
                    case "White":
                        Control1.BorderBrush = new SolidColorBrush(Colors.White);
                        break;
                }
            }
        }

        private void Border_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is Border b)
            {
                Control1 = b;

                ControlExampleSubstitution Substitution1 = new ControlExampleSubstitution
                {
                    Key = "BorderThickness",
                };
                BindingOperations.SetBinding(Substitution1, ControlExampleSubstitution.ValueProperty, new Binding
                {
                    Source = b,
                    Path = new PropertyPath("BorderThickness.Top"),
                });

                ControlExampleSubstitution Substitution2 = new ControlExampleSubstitution
                {
                    Key = "BorderBrush",
                };
                BindingOperations.SetBinding(Substitution2, ControlExampleSubstitution.ValueProperty, new Binding
                {
                    Source = b,
                    Path = new PropertyPath("BorderBrush"),
                });

                ControlExampleSubstitution Substitution3 = new ControlExampleSubstitution
                {
                    Key = "Background",
                };
                BindingOperations.SetBinding(Substitution3, ControlExampleSubstitution.ValueProperty, new Binding
                {
                    Source = b,
                    Path = new PropertyPath("Background"),
                });

                List<ControlExampleSubstitution> Substitutions = new List<ControlExampleSubstitution>() { Substitution1, Substitution2, Substitution3 };
                Example1.Substitutions = Substitutions;
            }
        }
    }
}
