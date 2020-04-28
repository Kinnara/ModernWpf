using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.SampleApp.ControlPages
{
    public partial class ExpanderPage
    {
        public ExpanderPage()
        {
            InitializeComponent();
            UpdateVisualState();
        }

        private void ExpandDirection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateVisualState();
        }

        private void UpdateVisualState()
        {
            VisualStateManager.GoToElementState((FrameworkElement)Content, expander.ExpandDirection.ToString(), false);
        }
    }
}
