using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ModernWpf.Gallery.Pages.WpfGallery
{
    public class WpfGalleryPageViewModel : INotifyPropertyChanged
    {
        private string _pageTitle;
        private string _pageDescription;

        public WpfGalleryPageViewModel(string pageTitle, string pageDescription)
        {
            _pageTitle = pageTitle;
            _pageDescription = pageDescription;
        }

        public event PropertyChangedEventHandler PropertyChanged;

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

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
