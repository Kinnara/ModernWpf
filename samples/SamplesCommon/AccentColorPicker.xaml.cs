using ModernWpf;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SamplesCommon
{
    public partial class AccentColorPicker : UserControl
    {
        public AccentColorPicker()
        {
            InitializeComponent();
        }

        private void ResetAccentColor(object sender, RoutedEventArgs e)
        {
            DispatcherHelper.RunOnMainThread(() =>
            {
                ThemeManager.Current.AccentColor = null;
            });
        }
    }

    public class AccentColors : List<AccentColor>
    {
        public AccentColors()
        {
            Add("#FFB900", "Yellow gold");
            Add("#FF8C00", "Gold");
            Add("#F7630C", "Orange bright");
            Add("#CA5010", "Orange dark");
            Add("#DA3B01", "Rust");
            Add("#EF6950", "Pale rust");
            Add("#D13438", "Brick red");
            Add("#FF4343", "Mod red");
            Add("#E74856", "Pale red");
            Add("#E81123", "Red");
            Add("#EA005E", "Rose bright");
            Add("#C30052", "Rose");
            Add("#E3008C", "Plum light");
            Add("#BF0077", "Plum");
            Add("#C239B3", "Orchid light");
            Add("#9A0089", "Orchid");
            Add("#0078D7", "Default blue");
            Add("#0063B1", "Navy blue");
            Add("#8E8CD8", "Purple shadow");
            Add("#6B69D6", "Purple shadow Dark");
            Add("#8764B8", "Iris pastel");
            Add("#744DA9", "Iris spring");
            Add("#B146C2", "Violet red light");
            Add("#881798", "Violet red");
            Add("#0099BC", "Cool blue bright");
            Add("#2D7D9A", "Cool blue");
            Add("#00B7C3", "Seafoam");
            Add("#038387", "Seafoam team");
            Add("#00B294", "Mint light");
            Add("#018574", "Mint dark");
            Add("#00CC6A", "Turf green");
            Add("#10893E", "Sport green");
            Add("#7A7574", "Gray");
            Add("#5D5A58", "Gray brown");
            Add("#68768A", "Steel blue");
            Add("#515C6B", "Metal blue");
            Add("#567C73", "Pale moss");
            Add("#486860", "Moss");
            Add("#498205", "Meadow green");
            Add("#107C10", "Green");
            Add("#767676", "Overcast");
            Add("#4C4A48", "Storm");
            Add("#69797E", "Blue gray");
            Add("#4A5459", "Gray dark");
            Add("#647C64", "Liddy green");
            Add("#525E54", "Sage");
            Add("#847545", "Camouflage desert");
            Add("#7E735F", "Camouflage");
        }

        private void Add(string color, string name)
        {
            Add(new AccentColor((Color)ColorConverter.ConvertFromString(color), name));
        }
    }

    public class AccentColor
    {
        public AccentColor(Color color, string name)
        {
            Color = color;
            Name = name;
            Brush = new SolidColorBrush(color);
        }

        public Color Color { get; }
        public string Name { get; }
        public SolidColorBrush Brush { get; }

        public override string ToString()
        {
            return Name;
        }
    }
}
