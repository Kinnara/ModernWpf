using ModernWpf.Controls;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Page = ModernWpf.Controls.Page;

namespace ModernWpf.SampleApp.ControlPages
{
    public partial class ExpanderPage : Page
    {
        public ExpanderPage()
        {
            InitializeComponent();
        }

        private void ExpandDirectionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string expandDirection = e.AddedItems[0].ToString();

            switch (expandDirection)
            {
                case "Down":
                default:
                    Expander1.ExpandDirection = ExpandDirection.Down;
                    Expander1.VerticalAlignment = VerticalAlignment.Top;
                    break;

                case "Up":
                    Expander1.ExpandDirection = ExpandDirection.Up;
                    Expander1.VerticalAlignment = VerticalAlignment.Bottom;
                    break;

                case "Left":
                    Expander1.ExpandDirection = ExpandDirection.Left;
                    Expander1.HorizontalAlignment = HorizontalAlignment.Right;
                    break;

                case "Right":
                    Expander1.ExpandDirection = ExpandDirection.Right;
                    Expander1.HorizontalAlignment = HorizontalAlignment.Left;
                    break;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ControlExampleSubstitution Substitution1 = new ControlExampleSubstitution
            {
                Key = "IsExpanded",
            };
            BindingOperations.SetBinding(Substitution1, ControlExampleSubstitution.ValueProperty, new Binding
            {
                Source = Expander1,
                Path = new PropertyPath("IsExpanded"),
            });

            ControlExampleSubstitution Substitution2 = new ControlExampleSubstitution
            {
                Key = "ExpandDirection",
            };
            BindingOperations.SetBinding(Substitution2, ControlExampleSubstitution.ValueProperty, new Binding
            {
                Source = ExpandDirectionComboBox,
                Path = new PropertyPath("SelectedValue"),
            });

            ControlExampleSubstitution Substitution3 = new ControlExampleSubstitution
            {
                Key = "VerticalAlignment",
            };
            BindingOperations.SetBinding(Substitution3, ControlExampleSubstitution.ValueProperty, new Binding
            {
                Source = Expander1,
                Path = new PropertyPath("VerticalAlignment"),
            });

            ControlExampleSubstitution Substitution4 = new ControlExampleSubstitution
            {
                Key = "HorizontalAlignment",
            };
            BindingOperations.SetBinding(Substitution4, ControlExampleSubstitution.ValueProperty, new Binding
            {
                Source = Expander1,
                Path = new PropertyPath("HorizontalAlignment"),
            });

            ObservableCollection<ControlExampleSubstitution> Substitutions = new ObservableCollection<ControlExampleSubstitution> { Substitution1, Substitution2, Substitution3, Substitution4 };
            Example1.Substitutions = Substitutions;
        }
    }
}
