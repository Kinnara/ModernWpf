using SamplesCommon;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Navigation;

namespace ModernWpf.SampleApp.ControlPages
{
    public partial class HyperlinkButtonPage
    {
        public HyperlinkButtonPage()
        {
            InitializeComponent();
        }

        private void GoToHyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationRootPage.RootFrame.Navigate(typeof(ItemPage), "ToggleButton");
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ControlExampleSubstitution Substitution = new ControlExampleSubstitution
            {
                Key = "IsEnabled",
                Value = @"IsEnabled=""False"" "
            };
            BindingOperations.SetBinding(Substitution, ControlExampleSubstitution.IsEnabledProperty, new Binding
            {
                Source = DisableControl1,
                Path = new PropertyPath("IsChecked"),
            });
            ObservableCollection<ControlExampleSubstitution> Substitutions = new ObservableCollection<ControlExampleSubstitution>() { Substitution };
            Example1.Substitutions = Substitutions;
        }
    }
}
