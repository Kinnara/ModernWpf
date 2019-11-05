using System.Windows;

namespace ItemsRepeaterTestApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnBackRequested(object sender, ModernWpf.Controls.BackRequestedEventArgs e)
        {
            RootFrame.GoBack();
        }
    }
}
