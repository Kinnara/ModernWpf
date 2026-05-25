using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using ModernWpf.Gallery.Models;
using ModernWpf.Gallery.Pages.WpfGallery;

namespace ModernWpf.Gallery.Pages
{
    public partial class SectionPage
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
            if (IsModernWpfExtensionSection(group.UniqueId))
            {
                GetOfficialGroupItemsControl().Visibility = System.Windows.Visibility.Collapsed;
                ModernWpfGroupScrollViewer.Visibility = System.Windows.Visibility.Visible;
            }
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

        private static bool IsModernWpfExtensionSection(string uniqueId)
        {
            return string.Equals(uniqueId, "ModernWpfControls", StringComparison.OrdinalIgnoreCase);
        }

        private ItemsControl GetOfficialGroupItemsControl()
        {
            var root = (Grid)Content;
            return root.Children.OfType<ItemsControl>().Single();
        }
    }
}
