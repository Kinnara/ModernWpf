using System.Windows.Controls;

namespace ModernWpf.Gallery.Pages.WpfGallery.DesignGuidance
{
    internal static class WpfGalleryColorSectionFactory
    {
        public static Page Create(int index)
        {
            switch (index)
            {
                case 1:
                    return new FillSection();
                case 2:
                    return new StrokeSection();
                case 3:
                    return new BackgroundSection();
                case 4:
                    return new SignalSection();
                case 5:
                    return new HighContrastSection();
                default:
                    return new TextSection();
            }
        }
    }
}
