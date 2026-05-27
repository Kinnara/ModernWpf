using ModernWpf.Gallery.Pages.WpfGallery;

namespace ModernWpf.Gallery.Pages.WpfGallery.Text
{
    public class LabelPageViewModel : WpfGalleryPageViewModel
    {
        public LabelPageViewModel()
            : base("Label", string.Empty)
        {
        }
    }

    public class TextBoxPageViewModel : WpfGalleryPageViewModel
    {
        public TextBoxPageViewModel()
            : base("TextBox", string.Empty)
        {
            ValidatedText = string.Empty;
        }

        public string ValidatedText { get; set; }
    }

    public class TextBlockPageViewModel : WpfGalleryPageViewModel
    {
        public TextBlockPageViewModel()
            : base("TextBlock", string.Empty)
        {
        }
    }

    public class HyperlinkPageViewModel : WpfGalleryPageViewModel
    {
        public HyperlinkPageViewModel()
            : base("Hyperlink", string.Empty)
        {
        }
    }

    public class RichTextEditPageViewModel : WpfGalleryPageViewModel
    {
        public RichTextEditPageViewModel()
            : base("RichTextEdit", string.Empty)
        {
        }
    }

    public class PasswordBoxPageViewModel : WpfGalleryPageViewModel
    {
        public PasswordBoxPageViewModel()
            : base("PasswordBox", string.Empty)
        {
        }
    }
}
