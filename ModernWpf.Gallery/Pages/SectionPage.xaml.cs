using System;
using System.Windows.Automation;
using System.Windows.Input;
using ModernWpf.Gallery.Models;

namespace ModernWpf.Gallery.Pages
{
    public sealed partial class SectionPage
    {
        public SectionPage(GalleryGroup group)
        {
            NavigateCommand = new GalleryCommand(OnNavigateCard);
            InitializeComponent();
            DataContext = group;
            AutomationProperties.SetName(TitleLabel, group.Title + " Page");
            GalleryAutomation.SetHeadingLevel(TitleLabel, GalleryAutomationHeadingLevel.Level1);
            GalleryAutomation.SetHeadingLevel(DescriptionLabel, GalleryAutomationHeadingLevel.Level2);
        }

        public Action<GalleryItem> ItemRequested { get; set; }
        public ICommand NavigateCommand { get; }

        private void OnNavigateCard(object parameter)
        {
            if (parameter is GalleryItem item)
            {
                ItemRequested?.Invoke(item);
            }
        }
    }
}
