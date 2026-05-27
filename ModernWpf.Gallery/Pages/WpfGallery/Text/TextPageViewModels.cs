using ModernWpf.Gallery.Pages.WpfGallery;

namespace ModernWpf.Gallery.Pages.WpfGallery.Text
{
    public partial class LabelPageViewModel : WpfGalleryPageViewModel
    {
        public LabelPageViewModel()
            : base("Label", string.Empty)
        {
        }
    }

    public partial class TextBoxPageViewModel : WpfGalleryPageViewModel
    {
        private string _validatedText = string.Empty;

        public TextBoxPageViewModel()
            : base("TextBox", string.Empty)
        {
        }

        public string ValidatedText
        {
            get { return _validatedText; }
            set { SetProperty(ref _validatedText, value); }
        }
    }

    public partial class TextBlockPageViewModel : WpfGalleryPageViewModel
    {
        public TextBlockPageViewModel()
            : base("TextBlock", string.Empty)
        {
        }
    }

    public partial class HyperlinkPageViewModel : WpfGalleryPageViewModel
    {
        public HyperlinkPageViewModel()
            : base("Hyperlink", string.Empty)
        {
        }
    }

    public partial class RichTextEditPageViewModel : WpfGalleryPageViewModel
    {
        public RichTextEditPageViewModel()
            : base("RichTextEdit", string.Empty)
        {
        }
    }

    public partial class PasswordBoxPageViewModel : WpfGalleryPageViewModel
    {
        public PasswordBoxPageViewModel()
            : base("PasswordBox", string.Empty)
        {
        }
    }
}
