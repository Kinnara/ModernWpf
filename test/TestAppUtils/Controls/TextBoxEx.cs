using ModernWpf.Controls.Primitives;

namespace System.Windows.Controls
{
    public class TextBoxEx : TextBox
    {
        #region CornerRadius

        public static readonly DependencyProperty CornerRadiusProperty = ControlHelper.CornerRadiusProperty.AddOwner(typeof(TextBoxEx));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        #endregion

        #region Header

        public static readonly DependencyProperty HeaderProperty = ControlHelper.HeaderProperty.AddOwner(typeof(TextBoxEx));

        public object Header
        {
            get => GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        #endregion

        #region PlaceholderText

        public static readonly DependencyProperty PlaceholderTextProperty = ControlHelper.PlaceholderTextProperty.AddOwner(typeof(TextBoxEx));

        public string PlaceholderText
        {
            get => (string)GetValue(PlaceholderTextProperty);
            set => SetValue(PlaceholderTextProperty, value);
        }

        #endregion

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            this.InitializeStyle(DefaultStyleKey);
        }
    }
}
