using ModernWpf.Gallery.Pages.WpfGallery;

namespace ModernWpf.Gallery.Pages.WpfGallery.Text
{
    public sealed class LabelPageViewModel : WpfGalleryPageViewModel
    {
        public LabelPageViewModel()
            : base("Label", string.Empty)
        {
        }
    }

    public sealed class TextBoxPageViewModel : WpfGalleryPageViewModel
    {
        public TextBoxPageViewModel()
            : base("TextBox", string.Empty)
        {
            ValidatedText = string.Empty;
        }

        public string ValidatedText { get; set; }
    }

    public sealed class TextBlockPageViewModel : WpfGalleryPageViewModel
    {
        public TextBlockPageViewModel()
            : base("TextBlock", string.Empty)
        {
        }
    }

    public sealed class HyperlinkPageViewModel : WpfGalleryPageViewModel
    {
        public HyperlinkPageViewModel()
            : base("Hyperlink", string.Empty)
        {
        }
    }

    public sealed class RichTextEditPageViewModel : WpfGalleryPageViewModel
    {
        public RichTextEditPageViewModel()
            : base("RichTextEdit", string.Empty)
        {
        }
    }

    public sealed class PasswordBoxPageViewModel : WpfGalleryPageViewModel
    {
        public PasswordBoxPageViewModel()
            : base("PasswordBox", string.Empty)
        {
        }
    }
}
