namespace ModernWpf.Gallery.Pages.WpfGallery
{
    public class WpfGalleryPageViewModel
    {
        public WpfGalleryPageViewModel(string pageTitle, string pageDescription)
        {
            PageTitle = pageTitle;
            PageDescription = pageDescription;
        }

        public string PageTitle { get; }

        public string PageDescription { get; }
    }
}
