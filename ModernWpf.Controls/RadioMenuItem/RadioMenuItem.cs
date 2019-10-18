using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.Controls
{
    public class RadioMenuItem : MenuItem
    {
        static RadioMenuItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RadioMenuItem), new FrameworkPropertyMetadata(typeof(RadioMenuItem)));

            IsCheckableProperty.OverrideMetadata(typeof(RadioMenuItem), new FrameworkPropertyMetadata(true));
        }

        public static readonly DependencyProperty GroupNameProperty =
            DependencyProperty.Register(
                nameof(GroupName),
                typeof(string),
                typeof(RadioMenuItem),
                new FrameworkPropertyMetadata(string.Empty, OnGroupNameChanged));

        public string GroupName
        {
            get => (string)GetValue(GroupNameProperty);
            set => SetValue(GroupNameProperty, value);
        }

        private static void OnGroupNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((RadioMenuItem)d).UpdateSiblings();
        }

        protected override void OnChecked(RoutedEventArgs e)
        {
            if (_surpressOnChecked)
            {
                e.Handled = true;
                return;
            }

            UpdateSiblings();

            base.OnChecked(e);
        }

        protected override void OnUnchecked(RoutedEventArgs e)
        {
            if (!_isSafeUncheck)
            {
                _surpressOnChecked = true;
                SetCurrentValue(IsCheckedProperty, true);
                _surpressOnChecked = false;
                e.Handled = true;
                return;
            }

            base.OnUnchecked(e);
        }

        private void UpdateSiblings()
        {
            if (IsChecked)
            {
                // Since this item is checked, uncheck all siblings
                if (Parent is ItemsControl parent)
                {
                    int childrenCount = parent.Items.Count;
                    for (int i = 0; i < childrenCount; i++)
                    {
                        var child = parent.Items[i];
                        if (child is RadioMenuItem radioItem)
                        {
                            if (radioItem != this
                                && radioItem.GroupName == GroupName)
                            {
                                radioItem._isSafeUncheck = true;
                                radioItem.SetCurrentValue(IsCheckedProperty, false);
                                radioItem._isSafeUncheck = false;
                            }
                        }
                    }
                }
            }
        }

        private bool _isSafeUncheck;
        private bool _surpressOnChecked;
    }
}
