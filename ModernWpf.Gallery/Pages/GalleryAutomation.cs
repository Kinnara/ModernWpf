using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;

namespace ModernWpf.Gallery.Pages
{
    public enum GalleryAutomationHeadingLevel
    {
        None,
        Level1,
        Level2,
        Level3,
        Level4,
        Level5,
        Level6,
        Level7,
        Level8,
        Level9
    }

    public static class GalleryAutomation
    {
        public static readonly DependencyProperty HeadingLevelProperty =
            DependencyProperty.RegisterAttached(
                "HeadingLevel",
                typeof(GalleryAutomationHeadingLevel),
                typeof(GalleryAutomation),
                new PropertyMetadata(GalleryAutomationHeadingLevel.None, OnHeadingLevelChanged));

        public static GalleryAutomationHeadingLevel GetHeadingLevel(DependencyObject element)
        {
            return (GalleryAutomationHeadingLevel)element.GetValue(HeadingLevelProperty);
        }

        public static void SetHeadingLevel(DependencyObject element, GalleryAutomationHeadingLevel value)
        {
            element.SetValue(HeadingLevelProperty, value);
        }

        public static string SampleRootId(string uniqueId)
        {
            return SampleElementId(uniqueId, "Root");
        }

        public static string SampleElementId(string uniqueId, string elementName)
        {
            return "GallerySample_" + uniqueId + "_" + elementName;
        }

        public static T WithAutomationId<T>(T element, string automationId)
            where T : DependencyObject
        {
            AutomationProperties.SetAutomationId(element, automationId);
            return element;
        }

#if NET8_0_OR_GREATER
        private static void OnHeadingLevelChanged(DependencyObject element, DependencyPropertyChangedEventArgs e)
        {
            AutomationProperties.SetHeadingLevel(element, ToAutomationHeadingLevel((GalleryAutomationHeadingLevel)e.NewValue));
        }

        private static AutomationHeadingLevel ToAutomationHeadingLevel(GalleryAutomationHeadingLevel headingLevel)
        {
            switch (headingLevel)
            {
                case GalleryAutomationHeadingLevel.Level1:
                    return AutomationHeadingLevel.Level1;
                case GalleryAutomationHeadingLevel.Level2:
                    return AutomationHeadingLevel.Level2;
                case GalleryAutomationHeadingLevel.Level3:
                    return AutomationHeadingLevel.Level3;
                case GalleryAutomationHeadingLevel.Level4:
                    return AutomationHeadingLevel.Level4;
                case GalleryAutomationHeadingLevel.Level5:
                    return AutomationHeadingLevel.Level5;
                case GalleryAutomationHeadingLevel.Level6:
                    return AutomationHeadingLevel.Level6;
                case GalleryAutomationHeadingLevel.Level7:
                    return AutomationHeadingLevel.Level7;
                case GalleryAutomationHeadingLevel.Level8:
                    return AutomationHeadingLevel.Level8;
                case GalleryAutomationHeadingLevel.Level9:
                    return AutomationHeadingLevel.Level9;
                default:
                    return AutomationHeadingLevel.None;
            }
        }
#else
        private static void OnHeadingLevelChanged(DependencyObject element, DependencyPropertyChangedEventArgs e)
        {
        }
#endif
    }

    internal sealed class GallerySamplePanel : StackPanel
    {
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new GallerySamplePanelAutomationPeer(this);
        }

        private sealed class GallerySamplePanelAutomationPeer : FrameworkElementAutomationPeer
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
}
