using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ModernWpf.SampleApp.ControlPages
{
    /// <summary>
    /// Interaction logic for TreeViewPage.xaml
    /// </summary>
    public partial class TreeViewPage : UserControl
    {
        private const int ItemCountInEachLevel = 5;

        public TreeViewPage()
        {
            InitializeComponent();

            DataContext = new TreeViewData();
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new EmptyAutomationPeer(this);
        }

        private class EmptyAutomationPeer : UserControlAutomationPeer
        {
            public EmptyAutomationPeer(UserControl owner) : base(owner)
            {
            }

            protected override List<AutomationPeer> GetChildrenCore()
            {
                return new List<AutomationPeer>();
            }
        }

        public class TreeViewData : ObservableCollection<TopLevelItemData>
        {
            public TreeViewData()
            {
                for (int i = 0; i < ItemCountInEachLevel; ++i)
                {
                    Add(new TopLevelItemData("Item " + i));
                }
            }
        }

        public class TopLevelItemData
        {
            public TopLevelItemData(string title)
            {
                Title = title;

                for (int i = 0; i < ItemCountInEachLevel; ++i)
                {
                    SecondLevelItems.Add(new SecondLevelItemData("Second Level " + i));
                }
            }

            public string Title { get; set; }

            public ObservableCollection<SecondLevelItemData> SecondLevelItems { get; } = new ObservableCollection<SecondLevelItemData>();
        }

        public class SecondLevelItemData
        {
            public SecondLevelItemData(string title)
            {
                Title = title;

                for (int i = 0; i < ItemCountInEachLevel; ++i)
                {
                    ThirdLevelItems.Add("Third Level " + i);
                }
            }

            public string Title { get; set; }

            public ObservableCollection<string> ThirdLevelItems { get; } = new ObservableCollection<string>();
        }
    }
}
