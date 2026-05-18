using System.Windows;
using System.Windows.Controls;

namespace DragablzSample
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            TabView.NewItemFactory = () =>
            {
                return new TabItem { Header = "New Document" };
            };
        }
    }
}
