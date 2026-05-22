using System;
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
