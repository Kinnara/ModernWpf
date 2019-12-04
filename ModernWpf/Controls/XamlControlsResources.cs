using System.Windows;

namespace ModernWpf.Controls
{
    /// <summary>
    /// Default styles for controls.
    /// </summary>
    public class XamlControlsResources : ResourceDictionary
    {
        /// <summary>
        /// Initializes a new instance of the XamlControlsResources class.
        /// </summary>
        public XamlControlsResources()
        {
            MergedDictionaries.Add(new ResourceDictionary { Source = PackUriHelper.GetAbsoluteUri("ControlsResources.xaml") });
        }

        public bool UseCompactResources
        {
            get => _useCompactResources;
            set
            {
                if (_useCompactResources != value)
                {
                    _useCompactResources = value;
                    if (_useCompactResources)
                    {
                        MergedDictionaries.Add(CompactResources);
                    }
                    else
                    {
                        MergedDictionaries.Remove(CompactResources);
                    };
                }
            }
        }

        internal static ResourceDictionary CompactResources
        {
            get
            {
                if (_compactResources == null)
                {
                    _compactResources = new ResourceDictionary { Source = PackUriHelper.GetAbsoluteUri("DensityStyles/Compact.xaml") };
                }
                return _compactResources;
            }
        }

        private static ResourceDictionary _compactResources;
        private bool _useCompactResources;
    }
}
