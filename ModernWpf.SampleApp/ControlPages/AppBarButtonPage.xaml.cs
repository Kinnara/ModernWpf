using ModernWpf.Controls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Page = ModernWpf.Controls.Page;

namespace ModernWpf.SampleApp.ControlPages
{
    public partial class AppBarButtonPage : Page
    {
        private AppBarToggleButton compactButton = null;
        private AppBarSeparator separator = null;

        public AppBarButtonPage()
        {
            InitializeComponent();
            Loaded += AppBarButtonPage_Loaded;
            Unloaded += AppBarButtonPage_Unloaded;
        }

        private void AppBarButtonPage_Unloaded(object sender, RoutedEventArgs e)
        {
            CommandBar appBar = NavigationRootPage.Current.PageHeader.TopCommandBar;
            compactButton.Click -= CompactButton_Click;
            appBar.PrimaryCommands.Remove(compactButton);
            appBar.PrimaryCommands.Remove(separator);
        }

        void AppBarButtonPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Add compact button to the command bar. It provides functionality specific
            // to this page, and is removed when leaving the page.

            CommandBar appBar = NavigationRootPage.Current.PageHeader.TopCommandBar;
            separator = new AppBarSeparator();
            appBar.PrimaryCommands.Insert(0, separator);

            compactButton = new AppBarToggleButton
            {
                Icon = new SymbolIcon(Symbol.FontSize),
                Label = "IsCompact"
            };
            compactButton.Click += CompactButton_Click;
            appBar.PrimaryCommands.Insert(0, compactButton);
        }

        private void CompactButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton toggle && toggle.IsChecked != null)
            {
                Button1.IsCompact =
                Button2.IsCompact =
                Button3.IsCompact =
                Button4.IsCompact = (bool)toggle.IsChecked;
            }
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button b)
            {
                string name = b.Name;

                switch (name)
                {
                    case "Button1":
                        Control1Output.Text = "You clicked: " + name;
                        break;
                    case "Button2":
                        Control2Output.Text = "You clicked: " + name;
                        break;
                    case "Button3":
                        Control3Output.Text = "You clicked: " + name;
                        break;
                    case "Button4":
                        Control4Output.Text = "You clicked: " + name;
                        break;
                    case "Button5":
                        Control5Output.Text = "You clicked: " + name;
                        break;
                }
            }
        }

        private void Save_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
    }
}
