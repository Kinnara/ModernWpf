using ModernWpf.Controls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Page = ModernWpf.Controls.Page;

namespace ModernWpf.SampleApp.ControlPages
{
    public partial class AppBarSeparatorPage : Page
    {
        private AppBarToggleButton compactButton = null;
        private AppBarSeparator separator = null;

        public AppBarSeparatorPage()
        {
            InitializeComponent();
            Loaded += AppBarButtonPage_Loaded;
            Unloaded += AppBarSeparatorPage_Unloaded;
        }

        private void AppBarSeparatorPage_Unloaded(object sender, RoutedEventArgs e)
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
            if ((sender as AppBarToggleButton).IsChecked == true)
            {
                Control1.DefaultLabelPosition = CommandBarDefaultLabelPosition.Collapsed;
            }
            else
            {
                Control1.DefaultLabelPosition = CommandBarDefaultLabelPosition.Bottom;
            }
        }
    }
}
