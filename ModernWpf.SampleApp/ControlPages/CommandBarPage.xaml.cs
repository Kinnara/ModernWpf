using ModernWpf.Controls;
using SamplesCommon;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ModernWpf.SampleApp.ControlPages
{
    public partial class CommandBarPage
    {
        public CommandBarPage()
        {
            InitializeComponent();
            AddKeyboardAccelerators();
        }

        private void OnElementClicked(object sender, RoutedEventArgs e)
        {
            SelectedOptionText.Text = "You clicked: " + (sender as AppBarButton).Label;
        }

        private void AddSecondaryCommands_Click(object sender, RoutedEventArgs e)
        {
            // Add compact button to the command bar. It provides functionality specific
            // to this page, and is removed when leaving the page.

            if (PrimaryCommandBar.SecondaryCommands.Count == 1)
            {
                var newButton = new AppBarButton();
                newButton.Icon = new SymbolIcon(Symbol.Add);
                newButton.Label = "Button 1";
                newButton.AddKeyboardAccelerator(Key.N, ModifierKeys.Control);
                PrimaryCommandBar.SecondaryCommands.Add(newButton);

                newButton = new AppBarButton();
                newButton.Icon = new SymbolIcon(Symbol.Delete);
                newButton.Label = "Button 2";
                PrimaryCommandBar.SecondaryCommands.Add(newButton);
                newButton.AddKeyboardAccelerator(Key.Delete);
                PrimaryCommandBar.SecondaryCommands.Add(new AppBarSeparator());

                newButton = new AppBarButton();
                newButton.Icon = new SymbolIcon(Symbol.FontDecrease);
                newButton.Label = "Button 3";
                newButton.AddKeyboardAccelerator(Key.Subtract, ModifierKeys.Control, "Ctrl+-");
                PrimaryCommandBar.SecondaryCommands.Add(newButton);

                newButton = new AppBarButton();
                newButton.Icon = new SymbolIcon(Symbol.FontIncrease);
                newButton.Label = "Button 4";
                newButton.AddKeyboardAccelerator(Key.Add, ModifierKeys.Control, "Ctrl++");
                PrimaryCommandBar.SecondaryCommands.Add(newButton);

            }
        }

        private void RemoveSecondaryCommands_Click(object sender, RoutedEventArgs e)
        {
            RemoveSecondaryCommands();
        }

        private void RemoveSecondaryCommands()
        {
            while (PrimaryCommandBar.SecondaryCommands.Count > 1)
            {
                PrimaryCommandBar.SecondaryCommands.RemoveAt(PrimaryCommandBar.SecondaryCommands.Count - 1);
            }
        }

        private void AddKeyboardAccelerators()
        {
            editButton.AddKeyboardAccelerator(Key.E, ModifierKeys.Control);
            shareButton.AddKeyboardAccelerator(Key.F4);
            addButton.AddKeyboardAccelerator(Key.A, ModifierKeys.Control);
            settingsButton.AddKeyboardAccelerator(Key.I, ModifierKeys.Control);
        }
    }
}
