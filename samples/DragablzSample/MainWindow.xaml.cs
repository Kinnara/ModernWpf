using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
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
                var newItem = new TabItem { Header = "New Document" };
                TabItemHelper.SetIcon(newItem, new SymbolIcon(Symbol.Document));
                return newItem;
            };
        }
    }
}
