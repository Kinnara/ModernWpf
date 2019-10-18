using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ModernWpf.Controls.Primitives
{
    public static class PasswordBoxCommandsHelper
    {
        private static readonly CommandBinding _selectAllBinding;

        static PasswordBoxCommandsHelper()
        {
            _selectAllBinding = new CommandBinding(ApplicationCommands.SelectAll);
            _selectAllBinding.PreviewCanExecute += OnSelectAllPreviewCanExecute;
        }

        #region IsEnabled

        public static bool GetIsEnabled(PasswordBox passwordBox)
        {
            return (bool)passwordBox.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(PasswordBox passwordBox, bool value)
        {
            passwordBox.SetValue(IsEnabledProperty, value);
        }

        public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(PasswordBoxCommandsHelper),
            new PropertyMetadata(OnIsEnabledChanged));

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var passwordBox = (PasswordBox)d;
            if ((bool)e.NewValue)
            {
                passwordBox.CommandBindings.Add(_selectAllBinding);
            }
            else
            {
                passwordBox.CommandBindings.Remove(_selectAllBinding);
            }
        }

        #endregion

        private static void OnSelectAllPreviewCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox && string.IsNullOrEmpty(passwordBox.Password))
            {
                e.CanExecute = false;
                e.Handled = true;
            }
        }
    }
}
