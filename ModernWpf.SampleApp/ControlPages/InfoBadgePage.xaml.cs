using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Page = ModernWpf.Controls.Page;

namespace ModernWpf.SampleApp.ControlPages
{
    /// <summary>
    /// InfoBadgePage.xaml 的交互逻辑
    /// </summary>
    public partial class InfoBadgePage : Page
    {
        public InfoBadgePage()
        {
            InitializeComponent();
        }

        public double InfoBadgeOpacity
        {
            get { return (double)GetValue(InfoBadgeOpacityProperty); }
            set { SetValue(InfoBadgeOpacityProperty, value); }
        }

        public static readonly DependencyProperty InfoBadgeOpacityProperty =
            DependencyProperty.Register(
                "ShadowOpacity",
                typeof(double),
                typeof(InfoBadgePage),
                new PropertyMetadata(0.0));

        public void NavigationViewDisplayMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string paneDisplayMode = e.AddedItems[0].ToString();

            switch (paneDisplayMode)
            {
                case "LeftExpanded":
                    nvSample1.PaneDisplayMode = NavigationViewPaneDisplayMode.Left;
                    nvSample1.IsPaneOpen = true;
                    break;

                case "LeftCompact":
                    nvSample1.PaneDisplayMode = NavigationViewPaneDisplayMode.LeftCompact;
                    nvSample1.IsPaneOpen = false;
                    break;

                case "Top":
                    nvSample1.PaneDisplayMode = NavigationViewPaneDisplayMode.Top;
                    nvSample1.IsPaneOpen = true;
                    break;
            }
        }

        private void ToggleInfoBadgeOpacity_Toggled(object sender, RoutedEventArgs e)
        {
            InfoBadgeOpacity = (InfoBadgeOpacity == 0.0) ? 1.0 : 0.0;
        }

        public void InfoBadgeStyleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string infoBadgeStyle = e.AddedItems[0].ToString();
            ResourceDictionary Resources = new ResourceDictionary { Source = new Uri("/ModernWpf.Controls;component/InfoBadge/InfoBadge.xaml", UriKind.RelativeOrAbsolute) };

            switch (infoBadgeStyle)
            {
                case "Attention":
                    infoBadge2.Style = Resources["AttentionIconInfoBadgeStyle"] as Style;
                    infoBadge3.Style = Resources["AttentionValueInfoBadgeStyle"] as Style;
                    infoBadge4.Style = Resources["AttentionDotInfoBadgeStyle"] as Style;
                    break;

                case "Informational":
                    infoBadge2.Style = Resources["InformationalIconInfoBadgeStyle"] as Style;
                    infoBadge3.Style = Resources["InformationalValueInfoBadgeStyle"] as Style;
                    infoBadge4.Style = Resources["InformationalDotInfoBadgeStyle"] as Style;
                    break;

                case "Success":
                    infoBadge2.Style = Resources["SuccessIconInfoBadgeStyle"] as Style;
                    infoBadge3.Style = Resources["SuccessValueInfoBadgeStyle"] as Style;
                    infoBadge4.Style = Resources["SuccessDotInfoBadgeStyle"] as Style;
                    break;

                case "Critical":
                    infoBadge2.Style = Resources["CriticalIconInfoBadgeStyle"] as Style;
                    infoBadge3.Style = Resources["CriticalValueInfoBadgeStyle"] as Style;
                    infoBadge4.Style = Resources["CriticalDotInfoBadgeStyle"] as Style;
                    break;
            }
        }

        private void ValueNumberBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if ((int)args.NewValue >= -1)
            {
                DynamicInfoBadge.Value = (int)args.NewValue;
            }
        }
    }
}
