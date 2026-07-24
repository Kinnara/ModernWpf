using System;
using ModernWpf.Gallery.Models;

namespace ModernWpf.Gallery.Pages.WpfGallery
{
    internal static class WpfGallerySectionPageFactory
    {
        public static SectionPage Create(GalleryGroup group)
        {
            if (group == null)
            {
                throw new ArgumentNullException(nameof(group));
            }

            switch (GalleryCatalog.NormalizeLookupId(group.UniqueId))
            {
                case "DesignGuidance":
                    return new DesignGuidancePage();
                case "Samples":
                    return new SamplesPage();
                case "BasicInput":
                    return new BasicInputPage();
                case "Collections":
                    return new CollectionsPage();
                case "DateAndCalendar":
                    return new DateAndTimePage();
                case "Layout":
                    return new LayoutPage();
                case "Media":
                    return new MediaPage();
                case "Navigation":
                    return new NavigationPage();
                case "StatusAndInfo":
                    return new StatusAndInfoPage();
                case "Text":
                    return new TextPage();
                case "System":
                    return new SystemPage();
                default:
                    return new SectionPage(group);
            }
        }

        internal static GalleryGroup GetRequiredGroup(string uniqueId)
        {
            var group = GalleryCatalog.FindGroup(uniqueId);
            if (group == null)
            {
                throw new InvalidOperationException("Missing WPF Gallery section group '" + uniqueId + "'.");
            }

            return group;
        }
    }

    public partial class DesignGuidancePage : SectionPage
    {
        public DesignGuidancePage()
            : base(WpfGallerySectionPageFactory.GetRequiredGroup("DesignGuidance"))
        {
        }

        public DesignGuidancePage(DesignGuidancePageViewModel viewModel)
            : base(WpfGallerySectionPageFactory.GetRequiredGroup("DesignGuidance"), viewModel)
        {
        }
    }

    public partial class SamplesPage : SectionPage
    {
        public SamplesPage()
            : base(WpfGallerySectionPageFactory.GetRequiredGroup("Samples"))
        {
        }

        public SamplesPage(SamplesPageViewModel viewModel)
            : base(WpfGallerySectionPageFactory.GetRequiredGroup("Samples"), viewModel)
        {
        }
    }

    public partial class BasicInputPage : SectionPage
    {
        public BasicInputPage()
            : base(WpfGallerySectionPageFactory.GetRequiredGroup("BasicInput"))
        {
        }

        public BasicInputPage(BasicInputPageViewModel viewModel)
            : base(WpfGallerySectionPageFactory.GetRequiredGroup("BasicInput"), viewModel)
        {
        }
    }

    public partial class CollectionsPage : SectionPage
    {
        public CollectionsPage()
            : base(WpfGallerySectionPageFactory.GetRequiredGroup("Collections"))
        {
        }

        public CollectionsPage(CollectionsPageViewModel viewModel)
            : base(WpfGallerySectionPageFactory.GetRequiredGroup("Collections"), viewModel)
        {
        }
    }

    public partial class DateAndTimePage : SectionPage
    {
        public DateAndTimePage()
            : base(WpfGallerySectionPageFactory.GetRequiredGroup("DateAndCalendar"))
        {
        }

        public DateAndTimePage(DateAndTimePageViewModel viewModel)
            : base(WpfGallerySectionPageFactory.GetRequiredGroup("DateAndCalendar"), viewModel)
        {
        }
    }

    public partial class LayoutPage : SectionPage
    {
        public LayoutPage()
            : base(WpfGallerySectionPageFactory.GetRequiredGroup("Layout"))
        {
        }

        public LayoutPage(LayoutPageViewModel viewModel)
            : base(WpfGallerySectionPageFactory.GetRequiredGroup("Layout"), viewModel)
        {
        }
    }

    public partial class MediaPage : SectionPage
    {
        public MediaPage()
            : base(WpfGallerySectionPageFactory.GetRequiredGroup("Media"))
        {
        }

        public MediaPage(MediaPageViewModel viewModel)
            : base(WpfGallerySectionPageFactory.GetRequiredGroup("Media"), viewModel)
        {
        }
    }

    public partial class NavigationPage : SectionPage
    {
        public NavigationPage()
            : base(WpfGallerySectionPageFactory.GetRequiredGroup("Navigation"))
        {
        }

        public NavigationPage(NavigationPageViewModel viewModel)
            : base(WpfGallerySectionPageFactory.GetRequiredGroup("Navigation"), viewModel)
        {
        }
    }

    public partial class StatusAndInfoPage : SectionPage
    {
        public StatusAndInfoPage()
            : base(WpfGallerySectionPageFactory.GetRequiredGroup("StatusAndInfo"))
        {
        }

        public StatusAndInfoPage(StatusAndInfoPageViewModel viewModel)
            : base(WpfGallerySectionPageFactory.GetRequiredGroup("StatusAndInfo"), viewModel)
        {
        }
    }

    public partial class TextPage : SectionPage
    {
        public TextPage()
            : base(WpfGallerySectionPageFactory.GetRequiredGroup("Text"))
        {
        }

        public TextPage(TextPageViewModel viewModel)
            : base(WpfGallerySectionPageFactory.GetRequiredGroup("Text"), viewModel)
        {
        }
    }

    public partial class SystemPage : SectionPage
    {
        public SystemPage()
            : base(WpfGallerySectionPageFactory.GetRequiredGroup("System"))
        {
        }

        public SystemPage(SystemPageViewModel viewModel)
            : base(WpfGallerySectionPageFactory.GetRequiredGroup("System"), viewModel)
        {
        }
    }
}
