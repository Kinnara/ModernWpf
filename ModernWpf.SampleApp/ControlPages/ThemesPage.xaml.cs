using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.SampleApp.ControlPages
{
    public partial class ThemesPage : UserControl
    {
        public ThemesPage()
        {
            InitializeComponent();

            //WindowThemeSelector.SetBinding(
            //    RadioButtons.SelectedItemProperty,
            //    new Binding
            //    {
            //        Path = new PropertyPath(ThemeManager.RequestedThemeProperty),
            //        Source = Application.Current.MainWindow,
            //        Mode = BindingMode.TwoWay
            //    });
        }

        ~ThemesPage()
        {
        }

        private void OpenNewWindow(object sender, RoutedEventArgs e)
        {
            new Window { Title = "New Window" }.Show();
        }

        private Window GetWindow()
        {
            return Application.Current.MainWindow;
        }
    }
}
