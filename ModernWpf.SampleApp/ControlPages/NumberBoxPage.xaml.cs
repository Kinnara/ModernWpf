using ModernWpf.Controls;
using System;
using System.Windows.Controls;

namespace ModernWpf.SampleApp.ControlPages
{
    public partial class NumberBoxPage : UserControl
    {
        public NumberBoxPage()
        {
            InitializeComponent();
            FormattedNumberBox.NumberFormatter = new CustomNumberFormatter();
        }

        private class CustomNumberFormatter : INumberBoxNumberFormatter
        {
            public string FormatDouble(double value)
            {
                return value.ToString("F");
            }

            public double? ParseDouble(string text)
            {
                if (double.TryParse(text, out double result))
                {
                    return Math.Round(result * 4, MidpointRounding.AwayFromZero) / 4;
                }
                return null;
            }
        }
    }
}
