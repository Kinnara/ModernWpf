using ModernWpf.Controls;
using ModernWpf.SampleApp.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ModernWpf.SampleApp
{
    /// <summary>
    /// SectionPage.xaml 的交互逻辑
    /// </summary>
    public partial class SectionPage : ItemsPageBase
    {
        public SectionPage()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var group = await ControlInfoDataSource.Instance.GetGroupAsync((string)e.ExtraData);

            var menuItem = NavigationRootPage.Current.NavigationView.MenuItems.Cast<NavigationViewItemBase>().Single(i => (string)i.Tag == group.UniqueId);
            menuItem.IsSelected = true;
            NavigationRootPage.Current.NavigationView.Header = menuItem.Content;

            Items = group.Items.OrderBy(i => i.Title).ToList();
            DataContext = Items;
        }
    }
}
