using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.Controls.Primitives
{
    public static class PivotHelper
    {
        #region Title

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.RegisterAttached(
                "Title",
                typeof(object),
                typeof(PivotHelper));

        public static object GetTitle(TabControl tabControl)
        {
            return tabControl.GetValue(TitleProperty);
        }

        public static void SetTitle(TabControl tabControl, object value)
        {
            tabControl.SetValue(TitleProperty, value);
        }

        #endregion

        #region TitleTemplate

        public static readonly DependencyProperty TitleTemplateProperty =
            DependencyProperty.RegisterAttached(
                "TitleTemplate",
                typeof(DataTemplate),
                typeof(PivotHelper));

        public static DataTemplate GetTitleTemplate(TabControl tabControl)
        {
            return (DataTemplate)tabControl.GetValue(TitleTemplateProperty);
        }

        public static void SetTitleTemplate(TabControl tabControl, DataTemplate value)
        {
            tabControl.SetValue(TitleTemplateProperty, value);
        }

        #endregion

        #region LeftHeader

        public static readonly DependencyProperty LeftHeaderProperty =
            DependencyProperty.RegisterAttached(
                "LeftHeader",
                typeof(object),
                typeof(PivotHelper));

        public static object GetLeftHeader(TabControl tabControl)
        {
            return tabControl.GetValue(LeftHeaderProperty);
        }

        public static void SetLeftHeader(TabControl tabControl, object value)
        {
            tabControl.SetValue(LeftHeaderProperty, value);
        }

        #endregion

        #region LeftHeaderTemplate

        public static readonly DependencyProperty LeftHeaderTemplateProperty =
            DependencyProperty.RegisterAttached(
                "LeftHeaderTemplate",
                typeof(DataTemplate),
                typeof(PivotHelper));

        public static DataTemplate GetLeftHeaderTemplate(TabControl tabControl)
        {
            return (DataTemplate)tabControl.GetValue(LeftHeaderTemplateProperty);
        }

        public static void SetLeftHeaderTemplate(TabControl tabControl, DataTemplate value)
        {
            tabControl.SetValue(LeftHeaderTemplateProperty, value);
        }

        #endregion

        #region RightHeader

        public static readonly DependencyProperty RightHeaderProperty =
            DependencyProperty.RegisterAttached(
                "RightHeader",
                typeof(object),
                typeof(PivotHelper));

        public static object GetRightHeader(TabControl tabControl)
        {
            return tabControl.GetValue(RightHeaderProperty);
        }

        public static void SetRightHeader(TabControl tabControl, object value)
        {
            tabControl.SetValue(RightHeaderProperty, value);
        }

        #endregion

        #region RightHeaderTemplate

        public static readonly DependencyProperty RightHeaderTemplateProperty =
            DependencyProperty.RegisterAttached(
                "RightHeaderTemplate",
                typeof(DataTemplate),
                typeof(PivotHelper));

        public static DataTemplate GetRightHeaderTemplate(TabControl tabControl)
        {
            return (DataTemplate)tabControl.GetValue(RightHeaderTemplateProperty);
        }

        public static void SetRightHeaderTemplate(TabControl tabControl, DataTemplate value)
        {
            tabControl.SetValue(RightHeaderTemplateProperty, value);
        }

        #endregion
    }
}
