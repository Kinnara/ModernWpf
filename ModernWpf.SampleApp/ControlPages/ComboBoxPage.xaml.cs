using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using Page = ModernWpf.Controls.Page;

namespace ModernWpf.SampleApp.ControlPages
{
    public partial class ComboBoxPage : Page
    {
        private ComboBox Combo1;
        private ComboBox Combo2;
        private ComboBox Combo3;

        private Rectangle Control1Output;
        private TextBlock Control2Output;
        private TextBlock Control3Output;

        public List<Tuple<string, FontFamily>> Fonts { get; } = new List<Tuple<string, FontFamily>>()
            {
                new Tuple<string, FontFamily>("Arial", new FontFamily("Arial")),
                new Tuple<string, FontFamily>("Comic Sans MS", new FontFamily("Comic Sans MS")),
                new Tuple<string, FontFamily>("Courier New", new FontFamily("Courier New")),
                new Tuple<string, FontFamily>("Segoe UI", new FontFamily("Segoe UI")),
                new Tuple<string, FontFamily>("Times New Roman", new FontFamily("Times New Roman"))
            };

        public List<double> FontSizes { get; } = new List<double>()
            {
                8,
                9,
                10,
                11,
                12,
                14,
                16,
                18,
                20,
                24,
                28,
                36,
                48,
                72
            };

        public ComboBoxPage()
        {
            this.InitializeComponent();
        }

        private void ColorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string colorName = e.AddedItems[0].ToString();
            Color color = ((SolidColorBrush)Control1Output.Fill).Color;
            switch (colorName)
            {
                case "Yellow":
                    color = Colors.Yellow;
                    break;
                case "Green":
                    color = Colors.Green;
                    break;
                case "Blue":
                    color = Colors.Blue;
                    break;
                case "Red":
                    color = Colors.Red;
                    break;
            }
            Control1Output.Fill = new SolidColorBrush(color);
        }

        private void Combo1_Loaded(object sender, RoutedEventArgs e)
        {
            Combo1 = sender as ComboBox;
        }

        private void Combo2_Loaded(object sender, RoutedEventArgs e)
        {
            Combo2 = sender as ComboBox;
            Combo2.SelectedIndex = 2;
        }

        private void Combo3_Loaded(object sender, RoutedEventArgs e)
        {
            Combo3 = sender as ComboBox;
            Combo3.SelectedIndex = 2;
        }

        private void TextBlock_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is TextBlock b)
            {
                string name = b.Tag.ToString();

                switch (name)
                {
                    case "Control2Output":
                        Control2Output = b;
                        b.SetBinding(FontFamilyProperty, new Binding
                        {
                            Source = Combo2,
                            Path = new PropertyPath("SelectedValue")
                        });
                        break;
                    case "Control3Output":
                        Control3Output = b;
                        b.SetBinding(FontSizeProperty, new Binding
                        {
                            Source = Combo3,
                            Path = new PropertyPath("SelectedValue")
                        });
                        break;
                }
            }
        }

        private void Rectangle_Loaded(object sender, RoutedEventArgs e) => Control1Output = (Rectangle)sender;
    }
}
