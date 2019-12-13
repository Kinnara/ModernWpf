using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;

namespace MahAppsSample.ControlPages
{
    public partial class FlipViewPage : Page
    {
        public FlipViewPage()
        {
            InitializeComponent();

            FlipView2.ItemsSource = typeof(Colors).GetProperties().Select(p => p.Name).ToList();
        }
    }
}
