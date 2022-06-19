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
        public FrameworkElement Target { get; set; }

        public string TargetName { get; set; }

        public double? Amount { get; set; }

        public Color? TintColor { get; set; }

        public double? TintOpacity { get; set; }

        public double? NoiseOpacity { get; set; }


        public AcrylicBrushExtension()
        {

        }

        public AcrylicBrushExtension(string target)
        {
            TargetName = target;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (Target == null)
            {
                var pvt = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
                Target = pvt.TargetObject as FrameworkElement;
            }
            else
            {
                TargetName = Target.Name;
            }

            var target = Target;

            var acrylicPanel = new AcrylicPanel();
            if (Amount != null) { acrylicPanel.Amount = (double)Amount; }
            if (TintColor != null) { acrylicPanel.TintColor = (Color)TintColor; }
            if (TintOpacity != null) { acrylicPanel.TintOpacity = (double)TintOpacity; }
            if (NoiseOpacity != null) { acrylicPanel.NoiseOpacity = (double)NoiseOpacity; }

            acrylicPanel.SetBinding(AcrylicPanel.TargetProperty, new Binding() { Source = target });
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

        public Brush CreatAcrylicBrush()
        {
            if (Target == null)
            {
                return new SolidColorBrush(TintColor ?? Colors.Transparent);
            }
            else
            {
                TargetName = Target.Name;
            }

            var target = Target;

            var acrylicPanel = new AcrylicPanel();
            if (Amount != null) { acrylicPanel.Amount = (double)Amount; }
            if (TintColor != null) { acrylicPanel.TintColor = (Color)TintColor; }
            if (TintOpacity != null) { acrylicPanel.TintOpacity = (double)TintOpacity; }
            if (NoiseOpacity != null) { acrylicPanel.NoiseOpacity = (double)NoiseOpacity; }

            acrylicPanel.SetBinding(AcrylicPanel.TargetProperty, new Binding() { Source = target });
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
