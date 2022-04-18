using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.Controls.Primitives
{
    public static class ListViewHelper
    {
        #region Header

        /// <summary>
        /// Identifies the Header dependency property.
        /// </summary>
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.RegisterAttached(
                "Header",
                typeof(object),
                typeof(ListViewHelper),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets the content for the list header.
        /// </summary>
        /// <param name="listbox">The element from which to read the property value.</param>
        /// <returns>The content of the list header. The default value is null.</returns>
        public static object GetHeader(ListBox listbox)
        {
            return listbox.GetValue(HeaderProperty);
        }

        /// <summary>
        /// Sets the content for the list header.
        /// </summary>
        /// <param name="listbox">The element from which to read the property value.</param>
        /// <param name="value">The content of the list header. The default value is null.</param>
        public static void SetHeader(ListBox listbox, object value)
        {
            listbox.SetValue(HeaderProperty, value);
        }

        #endregion

        #region HeaderTemplate

        /// <summary>
        /// Identifies the HeaderTemplate dependency property.
        /// </summary>
        public static readonly DependencyProperty HeaderTemplateProperty =
            DependencyProperty.RegisterAttached(
                "HeaderTemplate",
                typeof(DataTemplate),
                typeof(ListViewHelper));

        /// <summary>
        /// Gets the DataTemplate used to display the content of the view header.
        /// </summary>
        /// <param name="listbox">The element from which to read the property value.</param>
        /// <returns>The DataTemplate used to display the content of the view header.</returns>
        public static DataTemplate GetHeaderTemplate(ListBox listbox)
        {
            return (DataTemplate)listbox.GetValue(HeaderTemplateProperty);
        }

        /// <summary>
        /// Sets the DataTemplate used to display the content of the view header.
        /// </summary>
        /// <param name="listbox">The element from which to read the property value.</param>
        /// <param name="value">The DataTemplate used to display the content of the view header.</param>
        public static void SetHeaderTemplate(ListBox listbox, DataTemplate value)
        {
            listbox.SetValue(HeaderTemplateProperty, value);
        }

        #endregion

        #region Footer

        /// <summary>
        /// Identifies the Footer dependency property.
        /// </summary>
        public static readonly DependencyProperty FooterProperty =
            DependencyProperty.RegisterAttached(
                "Footer",
                typeof(object),
                typeof(ListViewHelper),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets the content for the list footer.
        /// </summary>
        /// <param name="listbox">The element from which to read the property value.</param>
        /// <returns>The content of the list footer. The default value is null.</returns>
        public static object GetFooter(ListBox listbox)
        {
            return listbox.GetValue(FooterProperty);
        }

        /// <summary>
        /// Sets the content for the list footer.
        /// </summary>
        /// <param name="listbox">The element from which to read the property value.</param>
        /// <param name="value">The content of the list footer. The default value is null.</param>
        public static void SetFooter(ListBox listbox, object value)
        {
            listbox.SetValue(FooterProperty, value);
        }

        #endregion

        #region HeaderTemplate

        /// <summary>
        /// Identifies the FooterTemplate dependency property.
        /// </summary>
        public static readonly DependencyProperty FooterTemplateProperty =
            DependencyProperty.RegisterAttached(
                "FooterTemplate",
                typeof(DataTemplate),
                typeof(ListViewHelper));

        /// <summary>
        /// Gets the DataTemplate used to display the content of the view footer.
        /// </summary>
        /// <param name="listbox">The element from which to read the property value.</param>
        /// <returns>The DataTemplate used to display the content of the view footer.</returns>
        public static DataTemplate GetFooterTemplate(ListBox listbox)
        {
            return (DataTemplate)listbox.GetValue(FooterTemplateProperty);
        }

        /// <summary>
        /// Sets the DataTemplate used to display the content of the view footer.
        /// </summary>
        /// <param name="listbox">The element from which to read the property value.</param>
        /// <param name="value">The DataTemplate used to display the content of the view footer.</param>
        public static void SetFooterTemplate(ListBox listbox, DataTemplate value)
        {
            listbox.SetValue(FooterTemplateProperty, value);
        }

        #endregion
    }
}
