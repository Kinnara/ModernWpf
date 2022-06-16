using ModernWpf.Media;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace ModernWpf.Markup
{
    public class AcrylicBrushExtension : MarkupExtension
    {
        public string TargetName { get; set; }

        public Color TintColor { get; set; } = Colors.White;

        public double TintOpacity { get; set; } = 0.0;

        public double NoiseOpacity { get; set; } = 0.03;


        public AcrylicBrushExtension()
        {

        }

        public AcrylicBrushExtension(string target)
        {
            TargetName = target;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var pvt = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
            var target = pvt.TargetObject as FrameworkElement;

            var acrylicPanel = new AcrylicPanel()
            {
                TintColor = TintColor,
                TintOpacity = TintOpacity,
                NoiseOpacity = NoiseOpacity,
            };

            acrylicPanel.SetBinding(AcrylicPanel.TargetProperty, new Binding() { ElementName = TargetName });
            acrylicPanel.SetBinding(AcrylicPanel.SourceProperty, new Binding() { Source = target });
            acrylicPanel.SetBinding(FrameworkElement.WidthProperty, new Binding("ActualWidth") { Source = target });
            acrylicPanel.SetBinding(FrameworkElement.HeightProperty, new Binding("ActualHeight") { Source = target });

            var brush = new VisualBrush(acrylicPanel)
            {
                Stretch = Stretch.None,
                AlignmentX = AlignmentX.Left,
                AlignmentY = AlignmentY.Top,
                ViewboxUnits = BrushMappingMode.Absolute,
            };

            return brush;
        }
    }
}
