using ModernWpf.Controls.Primitives;

namespace System.Windows.Controls
{
    public class RichEditBox : RichTextBox
    {
        #region CornerRadius

        public static readonly DependencyProperty CornerRadiusProperty = ControlHelper.CornerRadiusProperty.AddOwner(typeof(RichEditBox));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        #endregion

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            this.InitializeStyle(DefaultStyleKey);
        }
    }
}
