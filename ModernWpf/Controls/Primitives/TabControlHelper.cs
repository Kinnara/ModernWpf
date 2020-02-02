using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.Controls.Primitives
{
    public static class TabControlHelper
    {
        #region TabStripHeader

        public static readonly DependencyProperty TabStripHeaderProperty =
            DependencyProperty.RegisterAttached(
                "TabStripHeader",
                typeof(object),
                typeof(TabControlHelper));

        public static object GetTabStripHeader(TabControl tabControl)
        {
            return tabControl.GetValue(TabStripHeaderProperty);
        }

        public static void SetTabStripHeader(TabControl tabControl, object value)
        {
            tabControl.SetValue(TabStripHeaderProperty, value);
        }

        #endregion

        #region TabStripHeaderTemplate

        public static readonly DependencyProperty TabStripHeaderTemplateProperty =
            DependencyProperty.RegisterAttached(
                "TabStripHeaderTemplate",
                typeof(DataTemplate),
                typeof(TabControlHelper));

        public static DataTemplate GetTabStripHeaderTemplate(TabControl tabControl)
        {
            return (DataTemplate)tabControl.GetValue(TabStripHeaderTemplateProperty);
        }

        public static void SetTabStripHeaderTemplate(TabControl tabControl, DataTemplate value)
        {
            tabControl.SetValue(TabStripHeaderTemplateProperty, value);
        }

        #endregion

        #region TabStripFooter

        public static readonly DependencyProperty TabStripFooterProperty =
            DependencyProperty.RegisterAttached(
                "TabStripFooter",
                typeof(object),
                typeof(TabControlHelper));

        public static object GetTabStripFooter(TabControl tabControl)
        {
            return tabControl.GetValue(TabStripFooterProperty);
        }

        public static void SetTabStripFooter(TabControl tabControl, object value)
        {
            tabControl.SetValue(TabStripFooterProperty, value);
        }

        #endregion

        #region TabStripFooterTemplate

        public static readonly DependencyProperty TabStripFooterTemplateProperty =
            DependencyProperty.RegisterAttached(
                "TabStripFooterTemplate",
                typeof(DataTemplate),
                typeof(TabControlHelper));

        public static DataTemplate GetTabStripFooterTemplate(TabControl tabControl)
        {
            return (DataTemplate)tabControl.GetValue(TabStripFooterTemplateProperty);
        }

        public static void SetTabStripFooterTemplate(TabControl tabControl, DataTemplate value)
        {
            tabControl.SetValue(TabStripFooterTemplateProperty, value);
        }

        #endregion
    }
}
