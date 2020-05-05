using ModernWpf.Controls.Primitives;

namespace System.Windows.Controls
{
    public class Pivot : TabControl
    {
        #region Title

        public static readonly DependencyProperty TitleProperty = PivotHelper.TitleProperty.AddOwner(typeof(Pivot));

        public object Title
        {
            get => GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        #endregion

        #region RightHeader

        public static readonly DependencyProperty RightHeaderProperty = PivotHelper.RightHeaderProperty.AddOwner(typeof(Pivot));

        public object RightHeader
        {
            get => GetValue(RightHeaderProperty);
            set => SetValue(RightHeaderProperty, value);
        }

        #endregion

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            this.InitializeStyle("TabControlPivotStyle");
        }
    }

    public class PivotItem : TabItem
    {
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            this.InitializeStyle("TabItemPivotStyle");
        }
    }
}
