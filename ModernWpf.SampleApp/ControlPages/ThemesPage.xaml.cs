using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.SampleApp.ControlPages
{
    public partial class ThemesPage : UserControl
    {
        public ThemesPage()
        {
            InitializeComponent();

            var controlCornerRadius = (CornerRadius)Application.Current.FindResource("ControlCornerRadius");
            ControlCornerRadiusSlider.Value = controlCornerRadius.TopLeft;
            ControlCornerRadiusSlider.ValueChanged += ControlCornerRadiusSlider_ValueChanged;

            var overlayCornerRadius = (CornerRadius)Application.Current.FindResource("OverlayCornerRadius");
            OverlayCornerRadiusSlider.Value = overlayCornerRadius.TopLeft;
            OverlayCornerRadiusSlider.ValueChanged += OverlayCornerRadiusSlider_ValueChanged;

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

        private void ControlCornerRadiusSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SetThemeResource("ControlCornerRadius", new CornerRadius(e.NewValue));
        }

        private void OverlayCornerRadiusSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SetThemeResource("OverlayCornerRadius", new CornerRadius(e.NewValue));
        }

        private void SetThemeResource(object key, object value)
        {
            var tr = Application.Current.Resources.MergedDictionaries.OfType<ThemeResources>().First();
            tr.ThemeDictionaries["Light"][key] = value;
            tr.ThemeDictionaries["Dark"][key] = value;
            tr.ThemeDictionaries["HighContrast"][key] = value;
        }
    }
}
