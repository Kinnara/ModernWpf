using ModernWpf.Controls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace ModernWpf.SampleApp.ControlPages
{
    public partial class AppBarButtonPage
    {
        private TextBlock Control1Output;
        private TextBlock Control2Output;
        private TextBlock Control3Output;
        private TextBlock Control4Output;
        private TextBlock Control5Output;

        private AppBarButton Button1;
        private AppBarButton Button2;
        private AppBarButton Button3;
        private AppBarButton Button4;
        private AppBarButton Button5;

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
                string name = b.Tag.ToString();

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
                    case "Control3Output":
                        Control3Output = b;
                        break;
                    case "Control4Output":
                        Control4Output = b;
                        break;
                    case "Control5Output":
                        Control5Output = b;
                        break;
                }
            }
        }

        private void AppBarButton_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is AppBarButton b)
            {
                string name = b.Tag.ToString();

                switch (name)
                {
                    case "Button1":
                        Button1 = b;
                        break;
                    case "Button2":
                        Button2 = b;
                        break;
                    case "Button3":
                        Button3 = b;
                        break;
                    case "Button4":
                        Button4 = b;
                        break;
                    case "Button5":
                        Button5 = b;
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
