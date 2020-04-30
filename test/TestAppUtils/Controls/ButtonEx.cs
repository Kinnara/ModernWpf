using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;

namespace System.Windows.Controls
{
    public class ButtonEx : Button
    {
        #region CornerRadius

        public static readonly DependencyProperty CornerRadiusProperty = ControlHelper.CornerRadiusProperty.AddOwner(typeof(ButtonEx));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        #endregion

        #region Flyout

        public static readonly DependencyProperty FlyoutProperty = FlyoutService.FlyoutProperty.AddOwner(typeof(ButtonEx));

        public FlyoutBase Flyout
        {
            get => (FlyoutBase)GetValue(FlyoutProperty);
            set => SetValue(FlyoutProperty, value);
        }

        #endregion

        #region ContextFlyout

        public static readonly DependencyProperty ContextFlyoutProperty = ContextFlyoutService.ContextFlyoutProperty.AddOwner(typeof(ButtonEx));

        public FlyoutBase ContextFlyout
        {
            get => (FlyoutBase)GetValue(ContextFlyoutProperty);
            set => SetValue(ContextFlyoutProperty, value);
        }

        #endregion

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            this.InitializeStyle(DefaultStyleKey);
        }
    }
}
