using ModernWpf.Gallery.Pages.WpfGallery;

namespace ModernWpf.Gallery.Pages.WpfGallery.Text
{
    public partial class LabelPageViewModel : WpfGalleryPageViewModel
    {
        public LabelPageViewModel()
            : base("Label", "")
        {
        }
    }

    public partial class TextBoxPageViewModel : WpfGalleryPageViewModel
    {
        private string _validatedText = string.Empty;

        public TextBoxPageViewModel()
            : base("TextBox", "")
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
            : base("TextBlock", "")
        {
        }
    }

    public partial class HyperlinkPageViewModel : WpfGalleryPageViewModel
    {
        public HyperlinkPageViewModel()
            : base("Hyperlink", "")
        {
        }
    }

    public partial class RichTextEditPageViewModel : WpfGalleryPageViewModel
    {
        public RichTextEditPageViewModel()
            : base("RichTextBox", "")
        {
        }
    }

    public partial class PasswordBoxPageViewModel : WpfGalleryPageViewModel
    {
        public PasswordBoxPageViewModel()
            : base("PasswordBox", "")
        {
        }
    }
}
