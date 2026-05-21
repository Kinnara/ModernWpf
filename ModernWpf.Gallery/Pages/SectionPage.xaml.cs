using System;
using System.Windows.Automation;
using ModernWpf.Gallery.Models;

namespace ModernWpf.Gallery.Pages
{
    public sealed partial class SectionPage
    {
        public SectionPage(GalleryGroup group)
        {
            InitializeComponent();
            DataContext = group;
            AutomationProperties.SetName(TitleLabel, group.Title + " Page");
            GalleryAutomation.SetHeadingLevel(TitleLabel, GalleryAutomationHeadingLevel.Level1);
            GalleryAutomation.SetHeadingLevel(DescriptionLabel, GalleryAutomationHeadingLevel.Level2);
        }

        public Action<GalleryItem> ItemRequested { get; set; }

        private void OnItemCardClick(object sender, System.Windows.RoutedEventArgs e)
        {
            var item = ((System.Windows.FrameworkElement)sender).DataContext as GalleryItem;
            if (item != null)
            {
                ItemRequested?.Invoke(item);
            }
        }
    }
}
