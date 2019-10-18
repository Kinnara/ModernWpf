using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ModernWpf.Controls.Primitives
{
    public static class TextBoxCommandsHelper
    {
        private static readonly CommandBinding _selectAllBinding;

        static TextBoxCommandsHelper()
        {
            _selectAllBinding = new CommandBinding(ApplicationCommands.SelectAll);
            _selectAllBinding.PreviewCanExecute += OnSelectAllPreviewCanExecute;
        }

        #region IsEnabled

        public static bool GetIsEnabled(TextBox textBox)
        {
            return (bool)textBox.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(TextBox textBox, bool value)
        {
            textBox.SetValue(IsEnabledProperty, value);
        }

        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsEnabled",
                typeof(bool),
                typeof(TextBoxCommandsHelper),
                new PropertyMetadata(OnIsEnabledChanged));

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var textBox = (TextBox)d;
            if ((bool)e.NewValue)
            {
                textBox.CommandBindings.Add(_selectAllBinding);
            }
            else
            {
                textBox.CommandBindings.Remove(_selectAllBinding);
            }
        }

        #endregion

        private static void OnSelectAllPreviewCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (sender is TextBox textBox && string.IsNullOrEmpty(textBox.Text))
            {
                e.CanExecute = false;
                e.Handled = true;
            }
        }
    }
}
