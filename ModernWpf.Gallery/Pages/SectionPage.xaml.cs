using System;
using System.Windows.Input;
using ModernWpf.Gallery.Models;

namespace ModernWpf.Gallery.Pages
{
    public sealed partial class SectionPage
    {
        private readonly GalleryGroup _group;

        public SectionPage(GalleryGroup group)
        {
            _group = group ?? throw new ArgumentNullException(nameof(group));
            NavigateCommand = new GalleryCommand(OnNavigateCard);
            InitializeComponent();
            DataContext = this;
        }

        public Action<GalleryItem> ItemRequested { get; set; }
        public SectionPage ViewModel
        {
            get { return this; }
        }

        public ICommand NavigateCommand { get; }

        public string PageTitle
        {
            get { return _group.Title; }
        }

        public string PageDescription
        {
            get { return _group.PageDescription; }
        }

        public object NavigationCards
        {
            get { return _group.Items; }
        }

        private void OnNavigateCard(object parameter)
        {
            if (parameter is GalleryItem item)
            {
                ItemRequested?.Invoke(item);
            }
        }
    }
}
