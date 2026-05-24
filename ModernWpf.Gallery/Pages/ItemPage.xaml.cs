using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
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
            SampleSnippets = LoadSampleSnippets(_item.UniqueId);
            var xamlSnippet = FindSampleSnippet(SampleSnippets, IsXamlSnippet);
            var csharpSnippet = FindSampleSnippet(SampleSnippets, IsCSharpSnippet);
            DirectPageContent = WpfGalleryPageRegistry.CreatePageContent(_item.UniqueId);
            UsesWpfGalleryPageMode = DirectPageContent != null;

            Examples = DirectPageContent == null
                ? CreateWorkingSampleExamples(_item.UniqueId, SampleSnippets, xamlSnippet, csharpSnippet)
                : Array.Empty<GalleryExample>();

            AdditionalSampleSnippets = GetAdditionalSampleSnippets(SampleSnippets, Examples);
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
            get { return _item.PageDescription; }
        }

        public string ApiNamespace
        {
            get { return string.IsNullOrWhiteSpace(_item.ApiNamespace) ? "WPF sample or guidance page" : _item.ApiNamespace; }
        }

        public string BaseClassText
        {
            get { return string.IsNullOrWhiteSpace(_item.BaseClassText) ? "No base class metadata" : _item.BaseClassText; }
        }

        public string GroupTitle
        {
            get
            {
                var displayGroup = GalleryCatalog.FindDisplayGroupForItem(_item.UniqueId);
                return displayGroup == null ? _item.GroupTitle : displayGroup.Title;
            }
        }

        public IReadOnlyList<GalleryDocLink> Docs
        {
            get { return _item.Docs; }
        }

        public bool HasDocs
        {
            get { return Docs.Count != 0; }
        }

        public IReadOnlyList<GalleryExample> Examples { get; }

        public bool UsesWpfGalleryPageMode { get; }

        public object DirectPageContent { get; }

        public Thickness DirectPageContentMargin
        {
            get
            {
                return new Thickness(0);
            }
        }

        public bool HasDirectPageContent
        {
            get { return DirectPageContent != null; }
        }

        public bool HasWpfSampleContent
        {
            get { return Examples.Count != 0 || DirectPageContent != null; }
        }

        public bool ShowExamples
        {
            get { return Examples.Count != 0; }
        }

        public IReadOnlyList<SampleSnippet> SampleSnippets { get; }
        public IReadOnlyList<SampleSnippet> AdditionalSampleSnippets { get; }
        public IReadOnlyList<GalleryItem> RelatedItems { get; }

        public bool HasRelatedItems
        {
            get { return RelatedItems.Count != 0; }
        }

        public bool ShowCatalogDetails
        {
            get { return !UsesWpfGalleryPageMode; }
        }

        public bool ShowDocs
        {
            get { return !UsesWpfGalleryPageMode && HasDocs; }
        }

        public bool ShowAdditionalSampleSnippets
        {
            get { return !UsesWpfGalleryPageMode && HasAdditionalSampleSnippets; }
        }

        public bool ShowRelatedItems
        {
            get { return !UsesWpfGalleryPageMode && HasRelatedItems; }
        }

        public bool HasSampleSnippets
        {
            get { return SampleSnippets.Count != 0; }
        }

        public bool ShowPageHeader
        {
            get { return !HasDirectPageContent; }
        }

        public bool ShowPageDescription
        {
            get { return ShowPageHeader && (!UsesWpfGalleryPageMode || OfficialWpfPageShowsDescription(_item.UniqueId)); }
        }

        public string PageHeaderDescription
        {
            get { return ShowPageDescription ? Description : null; }
        }

        public bool ShowScrolledPageContent
        {
            get { return !HasDirectPageContent; }
        }

        public string ContentRootAutomationId
        {
            get { return HasDirectPageContent ? "GalleryItemPageRoot" : "ContentRootGrid"; }
        }

        public bool HasAdditionalSampleSnippets
        {
            get { return AdditionalSampleSnippets.Count != 0; }
        }

        private static IReadOnlyList<SampleSnippet> LoadSampleSnippets(string uniqueId)
        {
            var folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Samples", "SampleCode", uniqueId);
            if (!Directory.Exists(folder))
            {
                return Array.Empty<SampleSnippet>();
            }

            var maxSnippetCount = string.Equals(uniqueId, "NavigationView", StringComparison.Ordinal)
                ? 12
                : 6;

            return Directory.GetFiles(folder, "*.txt")
                .OrderBy(Path.GetFileName)
                .Take(maxSnippetCount)
                .Select(path => new SampleSnippet(Path.GetFileName(path), File.ReadAllText(path)))
                .ToArray();
        }

        private static object CreateWorkingSampleContent(string uniqueId)
        {
            return FundamentalsSampleFactory.Create(uniqueId)
                ?? BasicInputSampleFactory.Create(uniqueId)
                ?? StatusInfoSampleFactory.Create(uniqueId)
                ?? DialogsFlyoutsSampleFactory.Create(uniqueId)
                ?? DesignAccessibilitySampleFactory.Create(uniqueId)
                ?? MenusToolbarsSampleFactory.Create(uniqueId)
                ?? CollectionsSampleFactory.Create(uniqueId)
                ?? DateTimeSampleFactory.Create(uniqueId)
                ?? ScrollingSampleFactory.Create(uniqueId)
                ?? LayoutSampleFactory.Create(uniqueId)
                ?? NavigationSampleFactory.Create(uniqueId)
                ?? MediaSampleFactory.Create(uniqueId)
                ?? StylesSampleFactory.Create(uniqueId)
                ?? TextSampleFactory.Create(uniqueId)
                ?? MotionSampleFactory.Create(uniqueId)
                ?? WindowingSampleFactory.Create(uniqueId)
                ?? SystemSampleFactory.Create(uniqueId)
                ?? ShellSampleFactory.Create(uniqueId);
        }

        private static IReadOnlyList<GalleryExample> CreateWorkingSampleExamples(
            string uniqueId,
            IReadOnlyList<SampleSnippet> sampleSnippets,
            SampleSnippet xamlSnippet,
            SampleSnippet csharpSnippet)
        {
            var dialogFlyoutExamples = DialogsFlyoutsSampleFactory.CreateExamples(uniqueId, sampleSnippets);
            if (dialogFlyoutExamples.Count != 0)
            {
                return dialogFlyoutExamples;
            }

            var statusInfoExamples = StatusInfoSampleFactory.CreateExamples(uniqueId, sampleSnippets);
            if (statusInfoExamples.Count != 0)
            {
                return statusInfoExamples;
            }

            var basicInputExamples = BasicInputSampleFactory.CreateExamples(uniqueId, sampleSnippets);
            if (basicInputExamples.Count != 0)
            {
                return basicInputExamples;
            }

            var menuToolbarExamples = MenusToolbarsSampleFactory.CreateExamples(uniqueId, sampleSnippets);
            if (menuToolbarExamples.Count != 0)
            {
                return menuToolbarExamples;
            }

            var collectionsExamples = CollectionsSampleFactory.CreateExamples(uniqueId, sampleSnippets);
            if (collectionsExamples.Count != 0)
            {
                return collectionsExamples;
            }

            var dateTimeExamples = DateTimeSampleFactory.CreateExamples(uniqueId);
            if (dateTimeExamples.Count != 0)
            {
                return dateTimeExamples;
            }

            var textExamples = TextSampleFactory.CreateExamples(uniqueId, sampleSnippets);
            if (textExamples.Count != 0)
            {
                return textExamples;
            }

            var scrollingExamples = ScrollingSampleFactory.CreateExamples(uniqueId);
            if (scrollingExamples.Count != 0)
            {
                return scrollingExamples;
            }

            var layoutExamples = LayoutSampleFactory.CreateExamples(uniqueId);
            if (layoutExamples.Count != 0)
            {
                return layoutExamples;
            }

            var mediaExamples = MediaSampleFactory.CreateExamples(uniqueId);
            if (mediaExamples.Count != 0)
            {
                return mediaExamples;
            }

            var navigationExamples = NavigationSampleFactory.CreateExamples(uniqueId, sampleSnippets);
            if (navigationExamples.Count != 0)
            {
                return navigationExamples;
            }

            var sampleContent = CreateWorkingSampleContent(uniqueId);
            if (sampleContent == null)
            {
                return Array.Empty<GalleryExample>();
            }

            return new[]
            {
                new GalleryExample(
                    "Working WPF sample",
                    sampleContent,
                    xamlSnippet == null ? null : xamlSnippet.Text,
                    csharpSnippet == null ? null : csharpSnippet.Text)
            };
        }

        private static IReadOnlyList<SampleSnippet> GetAdditionalSampleSnippets(
            IReadOnlyList<SampleSnippet> snippets,
            IReadOnlyList<GalleryExample> examples)
        {
            var consumedText = new HashSet<string>(StringComparer.Ordinal);
            foreach (var example in examples)
            {
                if (example.XamlCode != null)
                {
                    consumedText.Add(example.XamlCode);
                }

                if (example.CSharpCode != null)
                {
                    consumedText.Add(example.CSharpCode);
                }
            }

            return snippets
                .Where(snippet => !consumedText.Contains(snippet.Text))
                .ToArray();
        }

        private static SampleSnippet FindSampleSnippet(IReadOnlyList<SampleSnippet> snippets, Func<SampleSnippet, bool> predicate)
        {
            return snippets.FirstOrDefault(predicate);
        }

        private static bool IsXamlSnippet(SampleSnippet snippet)
        {
            return Contains(snippet.Title, "xaml") || snippet.Text.TrimStart().StartsWith("<", StringComparison.Ordinal);
        }

        private static bool IsCSharpSnippet(SampleSnippet snippet)
        {
            return Contains(snippet.Title, "_cs") ||
                Contains(snippet.Title, "csharp") ||
                Contains(snippet.Text, "using System") ||
                Contains(snippet.Text, "public ");
        }

        private static bool Contains(string value, string token)
        {
            return !string.IsNullOrEmpty(value) && value.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool OfficialWpfPageShowsDescription(string uniqueId)
        {
            switch (uniqueId)
            {
                case "Canvas":
                case "Color":
                case "Iconography":
                case "Image":
                case "Label":
                case "PasswordBox":
                case "RichTextEdit":
                case "Spacing":
                case "TextBlock":
                case "TextBox":
                case "Typography":
                    return true;
                default:
                    return false;
            }
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

    public sealed class GalleryExample
    {
        public GalleryExample(string headerText, object exampleContent, string xamlCode, string csharpCode)
            : this(headerText, exampleContent, xamlCode, csharpCode, new Thickness(10))
        {
        }

        public GalleryExample(string headerText, object exampleContent, string xamlCode, string csharpCode, Thickness margin)
        {
            HeaderText = headerText;
            ExampleContent = exampleContent;
            XamlCode = xamlCode;
            CSharpCode = csharpCode;
            Margin = margin;
        }

        public string HeaderText { get; }
        public object ExampleContent { get; }
        public string XamlCode { get; }
        public string CSharpCode { get; }
        public Thickness Margin { get; }

        public GalleryExample WithMargin(Thickness margin)
        {
            return new GalleryExample(HeaderText, ExampleContent, XamlCode, CSharpCode, margin);
        }
    }
}
