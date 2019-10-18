using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.Controls.Primitives
{
    public class RadioButtonsListView : ListBox
    {
        static RadioButtonsListView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RadioButtonsListView),
                new FrameworkPropertyMetadata(typeof(RadioButtonsListView)));
        }

        #region Header

        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register(
                nameof(Header),
                typeof(object),
                typeof(RadioButtonsListView));

        public object Header
        {
            get => GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        #endregion

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new RadioButtonsListViewItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is RadioButtonsListViewItem;
        }
    }
}
