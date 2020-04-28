using System.Windows.Controls;

namespace ModernWpf.SampleApp.ControlPages
{
    public partial class MenuFlyoutPage
    {
        public MenuFlyoutPage()
        {
            InitializeComponent();
        }

        private void MenuFlyoutItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            MenuItem selectedItem = sender as MenuItem;

            if (selectedItem != null)
            {
                string sortOption = selectedItem.Tag.ToString();
                switch (sortOption)
                {
                    case "rating":
                        //SortByRating();
                        break;
                    case "match":
                        //SortByMatch();
                        break;
                    case "distance":
                        //SortByDistance();
                        break;
                }
                Control1Output.Text = "Sort by: " + sortOption;
            }
        }
    }
}
