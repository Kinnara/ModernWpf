using System;
using System.Windows;
using System.Windows.Controls;

namespace SamplesCommon
{
    public partial class LayoutDensitySelector : UserControl
    {
        private ResourceDictionary _compactResources;

        public LayoutDensitySelector()
        {
            InitializeComponent();
        }

        #region TargetElement

        public static readonly DependencyProperty TargetElementProperty =
            DependencyProperty.Register(
                nameof(TargetElement),
                typeof(FrameworkElement),
                typeof(LayoutDensitySelector),
                null);

        public FrameworkElement TargetElement
        {
            get => (FrameworkElement)GetValue(TargetElementProperty);
            set => SetValue(TargetElementProperty, value);
        }

        #endregion

        private void Standard_Checked(object sender, RoutedEventArgs e)
        {
            if (_compactResources != null)
            {
                TargetElement?.Resources.MergedDictionaries.Remove(_compactResources);
                _compactResources = null;
            }
        }

        private void Compact_Checked(object sender, RoutedEventArgs e)
        {
            if (_compactResources == null)
            {
                _compactResources = new ResourceDictionary { Source = new Uri("/ModernWpf;component/DensityStyles/Compact.xaml", UriKind.Relative) };
                TargetElement?.Resources.MergedDictionaries.Add(_compactResources);
            }
        }
    }
}
