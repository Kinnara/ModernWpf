using ModernWpf.Controls.Primitives;

namespace System.Windows.Controls
{
    public class ToolTipEx : ToolTip
    {
        #region CornerRadius

        public static readonly DependencyProperty CornerRadiusProperty = System.Windows.Controls.Border.CornerRadiusProperty.AddOwner(typeof(ToolTipEx));

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
