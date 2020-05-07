using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.SampleApp.ControlPages
{
    public partial class ThemesPage
    {
        public ThemesPage()
        {
            InitializeComponent();

            ShapePresetsComboBox.ItemsSource = new[]
            {
                new ShapePreset("Default", "Default"),
                new ShapePreset("PreFluent", "No Rounding, Thicker Borders"),
            };

            if (App.IsMultiThreaded)
            {
                ColorPresetsComboBox.Visibility = Visibility.Collapsed;
                ShapePresetsComboBox.Visibility = Visibility.Collapsed;
                AccentColorPicker.Visibility = Visibility.Collapsed;
            }
        }

        ~ThemesPage()
        {
        }
    }

    public class ShapePreset
    {
        public ShapePreset(string value, string displayName)
        {
            Value = value;
            DisplayName = displayName;
        }

        public string Value { get; }

        public string DisplayName { get; }
    }
}
