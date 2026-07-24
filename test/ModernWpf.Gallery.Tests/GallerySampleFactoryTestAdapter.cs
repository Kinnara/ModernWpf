using System.Windows;
using ModernWpf.Gallery.Pages;

namespace ModernWpf.Gallery.Tests
{
    internal static class GallerySampleFactoryTestAdapter
    {
        public static UIElement Create(string uniqueId)
        {
            return WpfGalleryPageRegistry.CreatePageContent(uniqueId)
                ?? BasicInputSampleFactory.Create(uniqueId)
                ?? StatusInfoSampleFactory.Create(uniqueId)
                ?? DialogsFlyoutsSampleFactory.Create(uniqueId)
                ?? MenusToolbarsSampleFactory.Create(uniqueId)
                ?? CollectionsSampleFactory.Create(uniqueId)
                ?? ScrollingSampleFactory.Create(uniqueId)
                ?? LayoutSampleFactory.Create(uniqueId)
                ?? NavigationSampleFactory.Create(uniqueId)
                ?? MediaSampleFactory.Create(uniqueId)
                ?? StylesSampleFactory.Create(uniqueId)
                ?? TextSampleFactory.Create(uniqueId)
                ?? WindowingSampleFactory.Create(uniqueId);
        }
    }
}
