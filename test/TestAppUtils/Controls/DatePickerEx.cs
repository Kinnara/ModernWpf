using ModernWpf.Controls.Primitives;

namespace System.Windows.Controls
{
    public class DatePickerEx : DatePicker
    {
        #region Header

        public static readonly DependencyProperty HeaderProperty = ControlHelper.HeaderProperty.AddOwner(typeof(DatePickerEx));

        public object Header
        {
            get => GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        #endregion

        #region CornerRadius

        public static readonly DependencyProperty CornerRadiusProperty = ControlHelper.CornerRadiusProperty.AddOwner(typeof(DatePickerEx));

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

    public class CalendarDatePicker : DatePickerEx { }
}
