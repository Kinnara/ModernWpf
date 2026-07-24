namespace ModernWpf.Gallery.Pages.WpfGallery
{
    public class WpfGalleryPageViewModel : WpfGalleryObservableObject
    {
        private string _pageTitle;
        private string _pageDescription;

        public WpfGalleryPageViewModel(string pageTitle, string pageDescription)
        {
            _pageTitle = pageTitle;
            _pageDescription = pageDescription;
        }

        public string PageTitle
        {
            get { return _pageTitle; }
            set { SetProperty(ref _pageTitle, value); }
        }

        public string PageDescription
        {
            get { return _pageDescription; }
            set { SetProperty(ref _pageDescription, value); }
        }
    }
}
