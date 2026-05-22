using System;
using System.Collections.Generic;
using System.Windows.Input;
using ModernWpf.Gallery.Models;

namespace ModernWpf.Gallery.Pages.WpfGallery
{
    public sealed class WpfGalleryDashboardPageViewModel
    {
        public WpfGalleryDashboardPageViewModel(Action<object> navigate)
        {
            NavigateCommand = new GalleryCommand(navigate);
        }

        public IReadOnlyList<GalleryGroup> NavigationCards
        {
            get { return GalleryCatalog.OverviewGroups; }
        }

        public IReadOnlyList<GalleryItem> RecentlyAddedOrUpdatedSamplesInfo
        {
            get { return GalleryCatalog.NewOrUpdatedItems; }
        }

        public ICommand NavigateCommand { get; }
    }

    public sealed class WpfGalleryNavigationPageViewModel
    {
        public WpfGalleryNavigationPageViewModel(
            string pageTitle,
            string pageDescription,
            IReadOnlyList<GalleryItem> navigationCards,
            Action<object> navigate)
        {
            PageTitle = pageTitle;
            PageDescription = pageDescription;
            NavigationCards = navigationCards ?? Array.Empty<GalleryItem>();
            NavigateCommand = new GalleryCommand(navigate);
        }

        public string PageTitle { get; }

        public string PageDescription { get; }

        public IReadOnlyList<GalleryItem> NavigationCards { get; }

        public ICommand NavigateCommand { get; }
    }
}
