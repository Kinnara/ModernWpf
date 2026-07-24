using System;
using System.Collections.Generic;
using System.Windows.Input;
using ModernWpf.Gallery.Models;

namespace ModernWpf.Gallery.Pages.WpfGallery
{
    public partial class DashboardPageViewModel : WpfGalleryPageViewModel
    {
        private IReadOnlyList<GalleryGroup> _navigationCards = GalleryCatalog.OverviewGroups;
        private IReadOnlyList<GalleryItem> _recentlyAddedOrUpdatedSamplesInfo = GalleryCatalog.NewOrUpdatedItems;
        private readonly Action<object> _navigate;

        public DashboardPageViewModel(Action<object> navigate)
            : base(string.Empty, string.Empty)
        {
            _navigate = navigate;
            NavigateCommand = new GalleryCommand(Navigate);
        }

        public IReadOnlyList<GalleryGroup> NavigationCards
        {
            get { return _navigationCards; }
            set { SetProperty(ref _navigationCards, value ?? Array.Empty<GalleryGroup>()); }
        }

        public IReadOnlyList<GalleryItem> RecentlyAddedOrUpdatedSamplesInfo
        {
            get { return _recentlyAddedOrUpdatedSamplesInfo; }
            set { SetProperty(ref _recentlyAddedOrUpdatedSamplesInfo, value ?? Array.Empty<GalleryItem>()); }
        }

        public ICommand NavigateCommand { get; }

        public void Navigate(object pageType)
        {
            if (pageType is Type page)
            {
                if (_navigate != null)
                {
                    _navigate(page);
                }
            }
            else if (pageType != null && _navigate != null)
            {
                _navigate(pageType);
            }
        }
    }

    public partial class WhatsNewPageViewModel : WpfGalleryPageViewModel
    {
        private IReadOnlyList<GalleryItem> _newOrUpdatedItems = GalleryCatalog.NewOrUpdatedItems;
        private string _recommendedResourcesXamlCode = _recommendedResourcesXamlUsage;
        private readonly Action<object> _navigate;

        public WhatsNewPageViewModel(Action<object> navigate)
            : base(
                GalleryBranding.WhatsNewTitle,
                GalleryBranding.WhatsNewDescription)
        {
            _navigate = navigate;
            NavigateCommand = new GalleryCommand(Navigate);
        }

        public IReadOnlyList<GalleryItem> NewOrUpdatedItems
        {
            get { return _newOrUpdatedItems; }
            set { SetProperty(ref _newOrUpdatedItems, value ?? Array.Empty<GalleryItem>()); }
        }

        public string RecommendedResourcesXamlCode
        {
            get { return _recommendedResourcesXamlCode; }
            set { SetProperty(ref _recommendedResourcesXamlCode, value); }
        }

        public ICommand NavigateCommand { get; }

        public void Navigate(object pageType)
        {
            if (pageType is Type page)
            {
                if (_navigate != null)
                {
                    _navigate(page);
                }
            }
            else if (pageType != null && _navigate != null)
            {
                _navigate(pageType);
            }
        }

        private const string _recommendedResourcesXamlUsage =
            "<Application\n"
            + "    ...\n"
            + "    xmlns:ui=\"http://schemas.modernwpf.com/2019\">\n"
            + "    <Application.Resources>\n"
            + "        <ResourceDictionary>\n"
            + "            <ResourceDictionary.MergedDictionaries>\n"
            + "                <ui:ThemeResources />\n"
            + "                <ui:FluentControlsResources UseCompactResources=\"False\" />\n"
            + "            </ResourceDictionary.MergedDictionaries>\n"
            + "        </ResourceDictionary>\n"
            + "    </Application.Resources>\n"
            + "</Application>";
    }

    public class WpfGalleryNavigationPageViewModel : WpfGalleryPageViewModel
    {
        private IReadOnlyList<GalleryItem> _navigationCards;
        private readonly Action<object> _navigate;

        public WpfGalleryNavigationPageViewModel(
            string pageTitle,
            string pageDescription,
            IReadOnlyList<GalleryItem> navigationCards,
            Action<object> navigate)
            : base(pageTitle, pageDescription)
        {
            _navigationCards = navigationCards ?? Array.Empty<GalleryItem>();
            _navigate = navigate;
            NavigateCommand = new GalleryCommand(Navigate);
        }

        public IReadOnlyList<GalleryItem> NavigationCards
        {
            get { return _navigationCards; }
            set { SetProperty(ref _navigationCards, value ?? Array.Empty<GalleryItem>()); }
        }

        public ICommand NavigateCommand { get; }

        public void Navigate(object pageType)
        {
            if (pageType is Type page)
            {
                if (_navigate != null)
                {
                    _navigate(page);
                }
            }
            else if (pageType != null && _navigate != null)
            {
                _navigate(pageType);
            }
        }

        public static WpfGalleryNavigationPageViewModel CreateForGroup(GalleryGroup group, Action<object> navigate)
        {
            if (group == null)
            {
                throw new ArgumentNullException(nameof(group));
            }

            switch (GalleryCatalog.NormalizeLookupId(group.UniqueId))
            {
                case "DesignGuidance":
                    return new DesignGuidancePageViewModel(navigate);
                case "Samples":
                    return new SamplesPageViewModel(navigate);
                case "BasicInput":
                    return new BasicInputPageViewModel(navigate);
                case "Collections":
                    return new CollectionsPageViewModel(navigate);
                case "DateAndCalendar":
                    return new DateAndTimePageViewModel(navigate);
                case "Layout":
                    return new LayoutPageViewModel(navigate);
                case "Media":
                    return new MediaPageViewModel(navigate);
                case "Navigation":
                    return new NavigationPageViewModel(navigate);
                case "StatusAndInfo":
                    return new StatusAndInfoPageViewModel(navigate);
                case "Text":
                    return new TextPageViewModel(navigate);
                case "System":
                    return new SystemPageViewModel(navigate);
                default:
                    return new WpfGalleryNavigationPageViewModel(group.Title, group.PageDescription, group.Items, navigate);
            }
        }

        protected static IReadOnlyList<GalleryItem> GetControlsInfo(string uniqueId)
        {
            var group = GalleryCatalog.FindGroup(uniqueId);
            return group == null ? Array.Empty<GalleryItem>() : group.Items;
        }
    }

    public partial class AllSamplesPageViewModel : WpfGalleryNavigationPageViewModel
    {
        public AllSamplesPageViewModel(Action<object> navigate)
            : base("All Controls", "", GalleryCatalog.AllControlsItems, navigate)
        {
        }
    }

    public partial class DesignGuidancePageViewModel : WpfGalleryNavigationPageViewModel
    {
        public DesignGuidancePageViewModel(Action<object> navigate)
            : base("Design Guidance", "Design guidelines on how to use colors, typography, and icons in your app.", GetControlsInfo("Design Guidance"), navigate)
        {
        }
    }

    public partial class SamplesPageViewModel : WpfGalleryNavigationPageViewModel
    {
        public SamplesPageViewModel(Action<object> navigate)
            : base("Samples", "Sample pages for common scenarios", GetControlsInfo("Samples"), navigate)
        {
        }
    }

    public partial class BasicInputPageViewModel : WpfGalleryNavigationPageViewModel
    {
        public BasicInputPageViewModel(Action<object> navigate)
            : base("Basic Input", "Controls for getting user input", GetControlsInfo("Basic Input"), navigate)
        {
        }
    }

    public partial class CollectionsPageViewModel : WpfGalleryNavigationPageViewModel
    {
        public CollectionsPageViewModel(Action<object> navigate)
            : base("Collections", "Controls for collection presentation", GetControlsInfo("Collections"), navigate)
        {
        }
    }

    public partial class DateAndTimePageViewModel : WpfGalleryNavigationPageViewModel
    {
        public DateAndTimePageViewModel(Action<object> navigate)
            : base("Date & Calendar", "Controls for date and calendar", GetControlsInfo("Date & Calendar"), navigate)
        {
        }
    }

    public partial class LayoutPageViewModel : WpfGalleryNavigationPageViewModel
    {
        public LayoutPageViewModel(Action<object> navigate)
            : base("Layout", "Controls for layouting", GetControlsInfo("Layout"), navigate)
        {
        }
    }

    public partial class MediaPageViewModel : WpfGalleryNavigationPageViewModel
    {
        public MediaPageViewModel(Action<object> navigate)
            : base("Media Controls", "Controls for media presentation", GetControlsInfo("Media"), navigate)
        {
        }
    }

    public partial class NavigationPageViewModel : WpfGalleryNavigationPageViewModel
    {
        public NavigationPageViewModel(Action<object> navigate)
            : base("Navigation", "Controls for navigation and actions", GetControlsInfo("Navigation"), navigate)
        {
        }
    }

    public partial class StatusAndInfoPageViewModel : WpfGalleryNavigationPageViewModel
    {
        public StatusAndInfoPageViewModel(Action<object> navigate)
            : base("Status & Info", "Controls to show progress and extra information", GetControlsInfo("Status & Info"), navigate)
        {
        }
    }

    public partial class TextPageViewModel : WpfGalleryNavigationPageViewModel
    {
        public TextPageViewModel(Action<object> navigate)
            : base("Text", "Controls for displaying and editing text", GetControlsInfo("Text"), navigate)
        {
        }
    }

    public partial class SystemPageViewModel : WpfGalleryNavigationPageViewModel
    {
        public SystemPageViewModel(Action<object> navigate)
            : base("System", "System-level controls and dialogs", GetControlsInfo("System"), navigate)
        {
        }
    }
}
