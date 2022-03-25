using ModernWpf.Controls;
using ModernWpf.SampleApp.Common;
using ModernWpf.SampleApp.Controls;
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
    /// ButtonPage.xaml 的交互逻辑
    /// </summary>
    public partial class ButtonPage : Page
    {
        private Button Button1;
        private TextBlock Control1Output;
        private TextBlock Control2Output;

        public ButtonPage()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button b)
            {
                string name = b.Tag.ToString();

                switch (name)
                {
                    case "Button1":
                        Control1Output.Text = "You clicked: " + name;
                        break;
                    case "Button2":
                        Control2Output.Text = "You clicked: " + name;
                        break;

                }
            }
        }

        private void TextBlock_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is TextBlock b)
            {
                string name = b.Tag.ToString();

                switch (name)
                {
                    case "Control1Output":
                        Control1Output = b;
                        break;
                    case "Control2Output":
                        Control2Output = b;
                        break;

                }
            }
        }

        private void Button_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is Button b)
            {
                string name = b.Tag.ToString();

                switch (name)
                {
                    case "Button1":
                        Button1 = b;
                        break;
                }
            }
        }

        private void CheckBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox b)
            {
                string name = b.Tag.ToString();

                switch (name)
                {
                    case "DisableButton1":
                        Button1.SetBinding(IsEnabledProperty, new Binding
                        {
                            Source = b,
                            Path = new PropertyPath("IsChecked"),
                            Converter = new BoolNegationConverter()
                        });

                        ControlExampleSubstitution Substitution = new ControlExampleSubstitution
                        {
                            Key = "IsEnabled",
                            Value = @"IsEnabled=""False"" "
                        };
                        BindingOperations.SetBinding(Substitution, ControlExampleSubstitution.IsEnabledProperty, new Binding
                        {
                            Source = b,
                            Path = new PropertyPath("IsChecked"),
                        });
                        List<ControlExampleSubstitution> Substitutions = new List<ControlExampleSubstitution>() { Substitution };
                        Example1.Substitutions = Substitutions;

                        break;
                }
            }
        }
    }
}
