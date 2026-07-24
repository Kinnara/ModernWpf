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
        private const string SampleAutomationIdPrefix = "GallerySample_";
        private const char SampleAutomationIdSeparator = '_';

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
            ValidateSampleAutomationIdSegment(uniqueId, nameof(uniqueId));
            ValidateSampleAutomationIdSegment(elementName, nameof(elementName));

            return SampleAutomationIdPrefix + uniqueId + SampleAutomationIdSeparator + elementName;
        }

        public static T WithAutomationId<T>(T element, string automationId)
            where T : DependencyObject
        {
            if (!IsSampleAutomationId(automationId))
            {
                throw new System.ArgumentException(
                    "Gallery sample automation IDs must be created by GalleryAutomation.SampleRootId or GalleryAutomation.SampleElementId.",
                    nameof(automationId));
            }

            AutomationProperties.SetAutomationId(element, automationId);
            return element;
        }

        private static void ValidateSampleAutomationIdSegment(string value, string parameterName)
        {
            if (!IsSampleAutomationIdSegment(value))
            {
                throw new System.ArgumentException(
                    "Gallery sample automation ID segments must be non-empty alphanumeric values.",
                    parameterName);
            }
        }

        internal static bool IsSampleAutomationId(string automationId)
        {
            if (string.IsNullOrEmpty(automationId) ||
                !automationId.StartsWith(SampleAutomationIdPrefix, System.StringComparison.Ordinal))
            {
                return false;
            }

            var suffix = automationId.Substring(SampleAutomationIdPrefix.Length);
            var separatorIndex = suffix.IndexOf(SampleAutomationIdSeparator);
            if (separatorIndex <= 0 ||
                separatorIndex == suffix.Length - 1 ||
                suffix.IndexOf(SampleAutomationIdSeparator, separatorIndex + 1) >= 0)
            {
                return false;
            }

            return IsSampleAutomationIdSegment(suffix.Substring(0, separatorIndex)) &&
                IsSampleAutomationIdSegment(suffix.Substring(separatorIndex + 1));
        }

        private static bool IsSampleAutomationIdSegment(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            for (var i = 0; i < value.Length; i++)
            {
                if (!char.IsLetterOrDigit(value[i]))
                {
                    return false;
                }
            }

            return true;
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
