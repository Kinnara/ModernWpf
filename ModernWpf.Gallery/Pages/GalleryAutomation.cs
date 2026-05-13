using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;

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

    internal sealed class GallerySamplePanel : StackPanel
    {
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new GallerySamplePanelAutomationPeer(this);
        }
    }

    internal sealed class GallerySamplePanelAutomationPeer : FrameworkElementAutomationPeer
    {
        public GallerySamplePanelAutomationPeer(GallerySamplePanel owner)
            : base(owner)
        {
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.Group;
        }

        protected override string GetClassNameCore()
        {
            return nameof(GallerySamplePanel);
        }

        protected override bool IsControlElementCore()
        {
            return true;
        }

        protected override bool IsContentElementCore()
        {
            return false;
        }
    }
}
