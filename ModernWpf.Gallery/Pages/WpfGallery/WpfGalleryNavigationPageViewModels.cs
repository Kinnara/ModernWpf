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
            if (_navigate != null)
            {
                _navigate(pageType);
            }
        }
    }

    public partial class WhatsNewPageViewModel : WpfGalleryPageViewModel
    {
        private string _accentColorXamlCode = _accentColorBrushApiXamlUsage;
        private string _hyphenBasedLigatureXamlCode = _hyphenBasedLiagatureXamlUsage;
        private string _gridShorthandSyntaxXamlCode = _gridShorthandSyntaxXamlUsage;
        private readonly Action<object> _navigate;

        public WhatsNewPageViewModel(Action<object> navigate)
            : base("What's new in WPF", "Discover all the new features, enhancements and APIs introduced in WPF")
        {
            _navigate = navigate;
            NavigateCommand = new GalleryCommand(Navigate);
        }

        public string AccentColorXamlCode
        {
            get { return _accentColorXamlCode; }
            set { SetProperty(ref _accentColorXamlCode, value); }
        }

        public string HyphenBasedLigatureXamlCode
        {
            get { return _hyphenBasedLigatureXamlCode; }
            set { SetProperty(ref _hyphenBasedLigatureXamlCode, value); }
        }

        public string GridShorthandSyntaxXamlCode
        {
            get { return _gridShorthandSyntaxXamlCode; }
            set { SetProperty(ref _gridShorthandSyntaxXamlCode, value); }
        }

        public ICommand NavigateCommand { get; }

        public void Navigate(object pageType)
        {
            if (_navigate != null)
            {
                _navigate(pageType);
            }
        }

        private const string _accentColorBrushApiXamlUsage =
            "<StackPanel Orientation=\"Horizontal\" Height=\"50\">\n"
            + "    <StackPanel.Resources>\n"
            + "        <Style TargetType=\"Border\">\n"
            + "            <Setter Property=\"Height\" Value=\"50\" />\n"
            + "            <Setter Property=\"Width\" Value=\"30\" />\n"
            + "        </Style>\n"
            + "    </StackPanel.Resources>\n"
            + "    <Border CornerRadius=\"2 0 0 2\" Background=\"{DynamicResource {x:Static SystemColors.AccentColorDark3BrushKey}}\" />\n"
            + "    <Border Background=\"{DynamicResource {x:Static SystemColors.AccentColorDark2BrushKey}}\" />\n"
            + "    <Border Background=\"{DynamicResource {x:Static SystemColors.AccentColorDark1BrushKey}}\" />\n"
            + "    <Border Background=\"{DynamicResource {x:Static SystemColors.AccentColorBrushKey}}\" />\n"
            + "    <Border Background=\"{DynamicResource {x:Static SystemColors.AccentColorLight1BrushKey}}\" />\n"
            + "    <Border Background=\"{DynamicResource {x:Static SystemColors.AccentColorLight2BrushKey}}\" />\n"
            + "    <Border CornerRadius=\"0 2 2 0\" Background=\"{DynamicResource {x:Static SystemColors.AccentColorLight3BrushKey}}\" />\n"
            + "</StackPanel>";

        private const string _hyphenBasedLiagatureXamlUsage =
            "<StackPanel Orientation=\"Horizontal\">\n"
            + "    <TextBlock Margin=\"0 0 16 0\" FontFamily=\"Cascadia Code\" Text=\"-->\" />\n"
            + "    <TextBlock Margin=\"0 0 16 0\" FontFamily=\"Cascadia Code\" Text=\"&lt;!--\" />\n"
            + "    <TextBlock Margin=\"0 0 16 0\" FontFamily=\"Cascadia Code\" Text=\"&lt;--\" />\n"
            + "</StackPanel>";

        private const string _gridShorthandSyntaxXamlUsage =
            "<Grid RowDefinitions=\"Auto,Auto,Auto\" ColumnDefinitions=\"Auto 80 *\" HorizontalAlignment=\"Left\">\n"
            + "    <TextBlock Grid.Row=\"0\" Grid.Column=\"0\" FontWeight=\"Bold\" Margin=\"0 0 10 0\">Sl. No.</TextBlock>\n"
            + "    <TextBlock Grid.Row=\"0\" Grid.Column=\"1\" FontWeight=\"Bold\">Name</TextBlock>\n"
            + "    <TextBlock Grid.Row=\"0\" Grid.Column=\"2\" FontWeight=\"Bold\">Description</TextBlock>\n"
            + "    <TextBlock Grid.Row=\"1\" Grid.Column=\"0\">1</TextBlock>\n"
            + "    <TextBlock Grid.Row=\"1\" Grid.Column=\"1\">Rectangle</TextBlock>\n"
            + "    <TextBlock Grid.Row=\"1\" Grid.Column=\"2\" TextWrapping=\"Wrap\">Quadrilateral where all the adjacent sides form a right angle.</TextBlock>\n"
            + "    <TextBlock Grid.Row=\"2\" Grid.Column=\"0\">2</TextBlock>\n"
            + "    <TextBlock Grid.Row=\"2\" Grid.Column=\"1\">Circle</TextBlock>\n"
            + "    <TextBlock Grid.Row=\"2\" Grid.Column=\"2\" TextWrapping=\"Wrap\">Set of all points that are equidistant from a fixed point.</TextBlock>\n"
            + "</Grid>";
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
            if (_navigate != null)
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
            : base("All Controls", string.Empty, GalleryCatalog.AllControlsItems, navigate)
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
            : base("Media Controls", "Controls for media presentation", GetControlsInfo("Media Controls"), navigate)
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
