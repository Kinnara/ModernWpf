using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using SamplesCommon;
using SamplesCommon.SamplePages;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace ModernWpf.SampleApp.ControlPages
{
    public partial class TabControlPage
    {
        public TabControlPage()
        {
            InitializeComponent();

            for (int i = 0; i < 3; i++)
            {
                tabControl.Items.Add(CreateNewTab(i));
                tabControl2.Items.Add(CreateNewTab(i));
                tabControl3.Items.Add(CreateNewTab(i));
            }
        }

        private void TabView_Loaded(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < 3; i++)
            {
                (sender as TabControl).Items.Add(CreateNewTab(i));
            }
        }

        private TabItem CreateNewTab(int index)
        {
            TabItem newItem = new TabItem();

            newItem.Header = $"Document {index}";
            TabItemHelper.SetIcon(newItem, new SymbolIcon(Symbol.Document));

            // The content of the tab is often a frame that contains a page, though it could be any UIElement.
            Frame frame = new Frame
            {
                NavigationUIVisibility = NavigationUIVisibility.Hidden,
                IsTabStop = false
            };

            frame.Navigated += (s, e) =>
            {
                ((Page)frame.Content).Margin = new Thickness(-18, 0, -18, 0);
            };

            switch (index % 3)
            {
                case 0:
                    frame.NavigateToType(typeof(SamplePage1));
                    break;
                case 1:
                    frame.NavigateToType(typeof(SamplePage2));
                    break;
                case 2:
                    frame.NavigateToType(typeof(SamplePage3));
                    break;
            }

            newItem.Content = frame;

            return newItem;
        }
    }
}
