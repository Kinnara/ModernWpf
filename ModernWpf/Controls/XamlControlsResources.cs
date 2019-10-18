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
            Source = PackUriHelper.GetAbsoluteUri("XamlControlsResources.xaml");
        }
    }
}
