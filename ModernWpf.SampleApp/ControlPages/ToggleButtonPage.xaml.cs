using ModernWpf.Controls;
using ModernWpf.SampleApp.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
    /// ToggleButtonPage.xaml 的交互逻辑
    /// </summary>
    public partial class ToggleButtonPage : Page
    {
        private TextBlock Control1Output;
        private ToggleButton Toggle1;

        public ToggleButtonPage()
        {
            InitializeComponent();
        }

        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            Control1Output.Text = "On";
        }

        private void ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            Control1Output.Text = "Off";
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
                        b.Text = (bool)Toggle1?.IsChecked ? "On" : "Off";
                        break;
                }
            }
        }

        private void Toggle_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton b)
            {
                string name = b.Tag.ToString();

                switch (name)
                {
                    case "Toggle1":
                        Toggle1 = b;
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
                    case "DisableToggle1":
                        Toggle1.SetBinding(IsEnabledProperty, new Binding
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
