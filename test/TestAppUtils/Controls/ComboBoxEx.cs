using ModernWpf.Controls.Primitives;

namespace System.Windows.Controls
{
    public class ComboBoxEx : ComboBox
    {
        #region Header

        public static readonly DependencyProperty HeaderProperty = ControlHelper.HeaderProperty.AddOwner(typeof(ComboBoxEx));

        public object Header
        {
            get => GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        #endregion

        #region CornerRadius

        public static readonly DependencyProperty CornerRadiusProperty = System.Windows.Controls.Border.CornerRadiusProperty.AddOwner(typeof(ComboBoxEx));

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
