using System.Windows;
using System.Windows.Automation;

namespace ModernWpf.Gallery.Pages
{
    internal static class GalleryAutomation
    {
        public static T WithAutomationId<T>(T element, string automationId)
            where T : UIElement
        {
            AutomationProperties.SetAutomationId(element, automationId);
            return element;
        }

        public static string SampleRootId(string uniqueId)
        {
            return "GallerySample_" + uniqueId + "_Root";
        }

        public static string SampleElementId(string uniqueId, string elementName)
        {
            return "GallerySample_" + uniqueId + "_" + elementName;
        }
    }
}
