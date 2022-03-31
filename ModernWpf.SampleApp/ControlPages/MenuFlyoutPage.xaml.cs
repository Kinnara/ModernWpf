using ModernWpf.Controls;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Page = ModernWpf.Controls.Page;

namespace ModernWpf.SampleApp.ControlPages
{
    public partial class MenuFlyoutPage : Page
    {
        public MenuFlyoutPage()
        {
            InitializeComponent();
        }

        private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem selectedItem)
            {
                string sortOption = selectedItem.Tag.ToString();
                switch (sortOption)
                {
                    case "rating":
                        //SortByRating();
                        break;
                    case "match":
                        //SortByMatch();
                        break;
                    case "distance":
                        //SortByDistance();
                        break;
                }
                Control1Output.Text = "Sort by: " + sortOption;
            }
        }

        private void Example5_Loaded(object sender, RoutedEventArgs e)
        {
            ControlExampleSubstitution Substitution1 = new ControlExampleSubstitution
            {
                Key = "RepeatToggle",
            };
            BindingOperations.SetBinding(Substitution1, ControlExampleSubstitution.ValueProperty, new Binding
            {
                Source = RepeatToggleMenuFlyoutItem,
                Path = new PropertyPath("IsChecked"),
            });
            ControlExampleSubstitution Substitution2 = new ControlExampleSubstitution
            {
                Key = "ShuffleToggle",
            };
            BindingOperations.SetBinding(Substitution2, ControlExampleSubstitution.ValueProperty, new Binding
            {
                Source = ShuffleToggleMenuFlyoutItem,
                Path = new PropertyPath("IsChecked"),
            });
            List<ControlExampleSubstitution> Substitutions = new List<ControlExampleSubstitution> { Substitution1, Substitution2 };
            Example2.Substitutions = Substitutions;
        }
    }
}
