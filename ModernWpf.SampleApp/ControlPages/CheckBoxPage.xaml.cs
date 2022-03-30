using ModernWpf.Controls;
using System.Windows;

namespace ModernWpf.SampleApp.ControlPages
{
    public partial class CheckBoxPage : Page
    {
        public CheckBoxPage()
        {
            InitializeComponent();
            Loaded += CheckBoxPage_Loaded;
        }

        void CheckBoxPage_Loaded(object sender, RoutedEventArgs e)
        {
            SetCheckedState();
        }

        private void Control1_Checked(object sender, RoutedEventArgs e)
        {
            Control1Output.Text = "You checked the box.";
        }

        private void Control1_Unchecked(object sender, RoutedEventArgs e)
        {
            Control1Output.Text = "You unchecked the box.";
        }

        private void Control2_Checked(object sender, RoutedEventArgs e)
        {
            Control2Output.Text = "CheckBox is checked.";
        }

        private void Control2_Unchecked(object sender, RoutedEventArgs e)
        {
            Control2Output.Text = "CheckBox is unchecked.";
        }

        private void Control2_Indeterminate(object sender, RoutedEventArgs e)
        {
            Control2Output.Text = "CheckBox state is indeterminate.";
        }

        #region SelectAllMethods
        private void SelectAll_Checked(object sender, RoutedEventArgs e)
        {
            Option1CheckBox.IsChecked = Option2CheckBox.IsChecked = Option3CheckBox.IsChecked = true;
        }

        private void SelectAll_Unchecked(object sender, RoutedEventArgs e)
        {
            Option1CheckBox.IsChecked = Option2CheckBox.IsChecked = Option3CheckBox.IsChecked = false;
        }

        private void SelectAll_Indeterminate(object sender, RoutedEventArgs e)
        {
            // If the SelectAll box is checked (all options are selected),
            // clicking the box will change it to its indeterminate state.
            // Instead, we want to uncheck all the boxes,
            // so we do this programatically. The indeterminate state should
            // only be set programatically, not by the user.

            if (Option1CheckBox.IsChecked == true &&
                Option2CheckBox.IsChecked == true &&
                Option3CheckBox.IsChecked == true)
            {
                // This will cause SelectAll_Unchecked to be executed, so
                // we don't need to uncheck the other boxes here.
                OptionsAllCheckBox.IsChecked = false;
            }
        }

        private void SetCheckedState()
        {
            // Controls are null the first time this is called, so we just
            // need to perform a null check on any one of the controls.
            if (Option1CheckBox != null)
            {
                if (Option1CheckBox.IsChecked == true &&
                    Option2CheckBox.IsChecked == true &&
                    Option3CheckBox.IsChecked == true)
                {
                    OptionsAllCheckBox.IsChecked = true;
                }
                else if (Option1CheckBox.IsChecked == false &&
                    Option2CheckBox.IsChecked == false &&
                    Option3CheckBox.IsChecked == false)
                {
                    OptionsAllCheckBox.IsChecked = false;
                }
                else
                {
                    // Set third state (indeterminate) by setting IsChecked to null.
                    OptionsAllCheckBox.IsChecked = null;
                }
            }
        }

        private void Option_Checked(object sender, RoutedEventArgs e)
        {
            SetCheckedState();
        }

        private void Option_Unchecked(object sender, RoutedEventArgs e)
        {
            SetCheckedState();
        }
        #endregion
    }
}
