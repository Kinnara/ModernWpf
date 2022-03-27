using ModernWpf.Controls;
using SamplesCommon;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Navigation;
using Page = ModernWpf.Controls.Page;

namespace ModernWpf.SampleApp.ControlPages
{
    public partial class CommandBarPage : Page, INotifyPropertyChanged
    {
        private AppBarButton AddButton;
        private AppBarButton EditButton;
        private AppBarButton ShareButton;
        private AppBarButton SettingsButton;
        private CommandBar PrimaryCommandBar;
        private TextBlock SelectedOptionText;

        private bool multipleButtons = false;
        public bool MultipleButtons
        {
            get => multipleButtons;
            set
            {
                multipleButtons = value;
                OnPropertyChanged("MultipleButtons");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }

        public CommandBarPage()
        {
            InitializeComponent();
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            PrimaryCommandBar.IsOpen = true;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            PrimaryCommandBar.IsOpen = false;
        }

        private void OnElementClicked(object sender, RoutedEventArgs e)
        {
            var selectedFlyoutItem = sender as AppBarButton;
            SelectedOptionText.Text = "You clicked: " + (sender as AppBarButton).Label;
        }

        private void AddSecondaryCommands_Click(object sender, RoutedEventArgs e)
        {
            // Add compact button to the command bar. It provides functionality specific
            // to this page, and is removed when leaving the page.

            if (PrimaryCommandBar.SecondaryCommands.Count == 1)
            {
                var newButton = new AppBarButton
                {
                    Icon = new SymbolIcon(Symbol.Add),
                    Label = "Button 1"
                };
                newButton.AddKeyboardAccelerator(Key.N, ModifierKeys.Control);
                PrimaryCommandBar.SecondaryCommands.Add(newButton);

                newButton = new AppBarButton
                {
                    Icon = new SymbolIcon(Symbol.Delete),
                    Label = "Button 2"
                };
                PrimaryCommandBar.SecondaryCommands.Add(newButton);
                newButton.AddKeyboardAccelerator(Key.Delete);
                PrimaryCommandBar.SecondaryCommands.Add(new AppBarSeparator());

                newButton = new AppBarButton
                {
                    Icon = new SymbolIcon(Symbol.FontDecrease),
                    Label = "Button 3"
                };
                newButton.AddKeyboardAccelerator(Key.Subtract, ModifierKeys.Control);
                PrimaryCommandBar.SecondaryCommands.Add(newButton);

                newButton = new AppBarButton
                {
                    Icon = new SymbolIcon(Symbol.FontIncrease),
                    Label = "Button 4"
                };
                newButton.AddKeyboardAccelerator(Key.Add, ModifierKeys.Control);
                PrimaryCommandBar.SecondaryCommands.Add(newButton);

            }
            MultipleButtons = true;
        }

        private void RemoveSecondaryCommands_Click(object sender, RoutedEventArgs e)
        {
            RemoveSecondaryCommands();
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            RemoveSecondaryCommands();
            base.OnNavigatingFrom(e);
        }

        private void RemoveSecondaryCommands()
        {
            while (PrimaryCommandBar.SecondaryCommands.Count > 1)
            {
                PrimaryCommandBar.SecondaryCommands.RemoveAt(PrimaryCommandBar.SecondaryCommands.Count - 1);
            }
            MultipleButtons = false;
        }

        private void AppBarButton_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is AppBarButton b)
            {
                string name = b.Tag.ToString();

                switch (name)
                {
                    case "AddButton":
                        AddButton = b;
                        b.AddKeyboardAccelerator(Key.A, ModifierKeys.Control);
                        break;
                    case "EditButton":
                        EditButton = b;
                        b.AddKeyboardAccelerator(Key.E, ModifierKeys.Control);
                        break;
                    case "ShareButton":
                        ShareButton = b;
                        b.AddKeyboardAccelerator(Key.F4);
                        break;
                    case "SettingsButton":
                        SettingsButton = b;
                        b.AddKeyboardAccelerator(Key.I, ModifierKeys.Control);
                        break;
                }
            }
        }

        private void TextBlock_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is TextBlock b)
            {
                SelectedOptionText = b;
            }
        }

        private void CommandBar_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is CommandBar b)
            {
                PrimaryCommandBar = b;
                ControlExampleSubstitution Substitution1 = new ControlExampleSubstitution
                {
                    Key = "IsOpen",
                    IsEnabled = true,
                };
                BindingOperations.SetBinding(Substitution1, ControlExampleSubstitution.ValueProperty, new Binding
                {
                    Source = b,
                    Path = new PropertyPath("IsOpen"),
                });
                ControlExampleSubstitution Substitution2 = new ControlExampleSubstitution
                {
                    Key = "MultipleButtonsSecondaryCommands",
                    Value = (string)Resources["MultipleButtonsSecondaryCommands"],
                };
                BindingOperations.SetBinding(Substitution2, ControlExampleSubstitution.IsEnabledProperty, new Binding
                {
                    Source = this,
                    Path = new PropertyPath("MultipleButtons"),
                });
                List<ControlExampleSubstitution> Substitutions = new List<ControlExampleSubstitution>() { Substitution1, Substitution2 };
                Example3.Substitutions = Substitutions;
            }
        }
    }
}
