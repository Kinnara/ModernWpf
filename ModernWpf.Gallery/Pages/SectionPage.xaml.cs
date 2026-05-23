using System;
using System.Collections.Generic;
using System.Windows.Input;
using ModernWpf.Gallery.Models;
using ModernWpf.Gallery.Pages.WpfGallery;

namespace ModernWpf.Gallery.Pages
{
    public sealed partial class SectionPage
    {
        public SectionPage(GalleryGroup group)
            : this(group, null)
        {
        }

        public SectionPage(GalleryGroup group, WpfGalleryNavigationPageViewModel viewModel)
        {
            if (group == null)
            {
                throw new ArgumentNullException(nameof(group));
            }

            InitializeComponent();
            ViewModel = viewModel ?? WpfGalleryNavigationPageViewModel.CreateForGroup(group, OnNavigateCard);
            DataContext = this;
            Title = GetOfficialSectionPageTitle(group.UniqueId);
        }

        public Action<GalleryItem> ItemRequested { get; set; }
        public WpfGalleryNavigationPageViewModel ViewModel { get; }

        public ICommand NavigateCommand
        {
            get { return ViewModel.NavigateCommand; }
        }

        public string PageTitle
        {
            get { return ViewModel.PageTitle; }
        }

        public string PageDescription
        {
            get { return ViewModel.PageDescription; }
        }

        public IReadOnlyList<GalleryItem> NavigationCards
        {
            get { return ViewModel.NavigationCards; }
        }

        private void OnNavigateCard(object parameter)
        {
            if (parameter is GalleryItem item)
            {
                ItemRequested?.Invoke(item);
            }
        }

        private static string GetOfficialSectionPageTitle(string uniqueId)
        {
            switch (uniqueId)
            {
                case "DesignGuidance":
                    return "DesignGuidancePage";
                case "Samples":
                    return "SamplesPage";
                case "BasicInput":
                    return "BasicInputPage";
                case "Collections":
                    return "CollectionsPage";
                case "DateAndCalendar":
                    return "DateAndTimePage";
                case "Layout":
                    return "LayoutPage";
                case "Media":
                    return "MediaPage";
                case "Navigation":
                    return "NavigationPage";
                case "StatusAndInfo":
                    return "StatusAndInfoPage";
                case "Text":
                    return "TextPage";
                case "System":
                    return "SystemPage";
                default:
                    return uniqueId + "Page";
            }
        }
    }
}
