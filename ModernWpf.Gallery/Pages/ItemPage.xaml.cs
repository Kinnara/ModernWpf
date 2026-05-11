using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Navigation;
using ModernWpf.Gallery.Models;

namespace ModernWpf.Gallery.Pages
{
    public sealed partial class ItemPage
    {
        private readonly GalleryItem _item;

        public ItemPage(GalleryItem item)
        {
            InitializeComponent();

            _item = item ?? GalleryCatalog.Items.First();
            WpfSampleContent = FundamentalsSampleFactory.Create(_item.UniqueId)
                ?? BasicInputSampleFactory.Create(_item.UniqueId)
                ?? StatusInfoSampleFactory.Create(_item.UniqueId)
                ?? DialogsFlyoutsSampleFactory.Create(_item.UniqueId)
                ?? DesignAccessibilitySampleFactory.Create(_item.UniqueId)
                ?? MenusToolbarsSampleFactory.Create(_item.UniqueId)
                ?? CollectionsSampleFactory.Create(_item.UniqueId)
                ?? DateTimeSampleFactory.Create(_item.UniqueId)
                ?? ScrollingSampleFactory.Create(_item.UniqueId)
                ?? LayoutSampleFactory.Create(_item.UniqueId)
                ?? NavigationSampleFactory.Create(_item.UniqueId)
                ?? MediaSampleFactory.Create(_item.UniqueId)
                ?? StylesSampleFactory.Create(_item.UniqueId)
                ?? TextSampleFactory.Create(_item.UniqueId)
                ?? MotionSampleFactory.Create(_item.UniqueId)
                ?? WindowingSampleFactory.Create(_item.UniqueId)
                ?? SystemSampleFactory.Create(_item.UniqueId)
                ?? ShellSampleFactory.Create(_item.UniqueId);
            SampleSnippets = LoadSampleSnippets(_item.UniqueId);
            RelatedItems = _item.RelatedControlIds
                .Select(GalleryCatalog.FindItem)
                .Where(related => related != null)
                .ToArray();
            DataContext = this;
        }

        public Action<GalleryItem> ItemRequested { get; set; }

        public string UniqueId
        {
            get { return _item.UniqueId; }
        }

        public string Title
        {
            get { return _item.Title; }
        }

        public string Subtitle
        {
            get { return _item.Subtitle; }
        }

        public string ImagePath
        {
            get { return _item.ImagePath; }
        }

        public string Description
        {
            get { return _item.Description; }
        }

        public string ApiNamespace
        {
            get { return string.IsNullOrWhiteSpace(_item.ApiNamespace) ? "WinUI Gallery metadata only" : _item.ApiNamespace; }
        }

        public string BaseClassText
        {
            get { return string.IsNullOrWhiteSpace(_item.BaseClassText) ? "No base class metadata" : _item.BaseClassText; }
        }

        public string GroupTitle
        {
            get { return _item.GroupTitle; }
        }

        public IReadOnlyList<GalleryDocLink> Docs
        {
            get { return _item.Docs; }
        }

        public object WpfSampleContent { get; }

        public bool HasWpfSampleContent
        {
            get { return WpfSampleContent != null; }
        }

        public IReadOnlyList<SampleSnippet> SampleSnippets { get; }
        public IReadOnlyList<GalleryItem> RelatedItems { get; }

        private static IReadOnlyList<SampleSnippet> LoadSampleSnippets(string uniqueId)
        {
            var folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Samples", "SampleCode", uniqueId);
            if (!Directory.Exists(folder))
            {
                return Array.Empty<SampleSnippet>();
            }

            return Directory.GetFiles(folder, "*.txt")
                .OrderBy(Path.GetFileName)
                .Take(6)
                .Select(path => new SampleSnippet(Path.GetFileName(path), File.ReadAllText(path)))
                .ToArray();
        }

        private void OnRelatedItemClick(object sender, System.Windows.RoutedEventArgs e)
        {
            var item = ((System.Windows.FrameworkElement)sender).DataContext as GalleryItem;
            if (item != null)
            {
                ItemRequested?.Invoke(item);
            }
        }

        private void OnDocumentationRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }
    }
}
