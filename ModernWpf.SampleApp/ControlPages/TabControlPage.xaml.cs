using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using SamplesCommon.SamplePages;
using System.Windows.Controls;

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
            }
        }

        private TabItem CreateNewTab(int index)
        {
            TabItem newItem = new TabItem();

            newItem.Header = $"Document {index}";
            TabItemHelper.SetIcon(newItem, new FontIcon { Glyph = "\uE130 " });

            // The content of the tab is often a frame that contains a page, though it could be any UIElement.
            Frame frame = new Frame();

            switch (index % 3)
            {
                case 0:
                    frame.Navigate(new SamplePage1());
                    break;
                case 1:
                    frame.Navigate(new SamplePage2());
                    break;
                case 2:
                    frame.Navigate(new SamplePage3());
                    break;
            }

            newItem.Content = frame;

            return newItem;
        }
    }
}
