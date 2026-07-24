using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ModernWpf.Controls.Primitives
{
    public class PasswordBoxHelper : DependencyObject
    {
        private const string ButtonStatesGroup = "ButtonStates";
        private const string ButtonVisibleState = "ButtonVisible";
        private const string ButtonCollapsedState = "ButtonCollapsed";

        private static readonly CommandBinding TextBoxCutBinding;
        private static readonly CommandBinding TextBoxCopyBinding;

        private readonly PasswordBox _passwordBox;

        private bool _hideRevealButton;
        private bool _isUpdatingPasswordBox;
        private bool _isUpdatingTextBox;
        private TextBox _textBox;

        static PasswordBoxHelper()
        {
            TextBoxCutBinding = new CommandBinding(ApplicationCommands.Cut);
            TextBoxCutBinding.CanExecute += OnDisabledCommandCanExecute;

            TextBoxCopyBinding = new CommandBinding(ApplicationCommands.Copy);
            TextBoxCopyBinding.CanExecute += OnDisabledCommandCanExecute;
        }

        public PasswordBoxHelper(PasswordBox passwordBox)
        {
            _passwordBox = passwordBox;
        }

        #region PasswordRevealMode

        /// <summary>
        /// Gets a value that specifies whether the password is always, never, or
        /// optionally obscured.
        /// </summary>
        /// <param name="passwordBox">The element from which to read the property value.</param>
        /// <returns>
        /// A value of the enumeration that specifies whether the password is always, never,
        /// or optionally obscured. The default is **Peek**.
        /// </returns>
        public static PasswordRevealMode GetPasswordRevealMode(PasswordBox passwordBox)
        {
            return (PasswordRevealMode)passwordBox.GetValue(PasswordRevealModeProperty);
        }

        /// <summary>
        /// Sets a value that specifies whether the password is always, never, or
        /// optionally obscured.
        /// </summary>
        /// <param name="passwordBox">The element on which to set the attached property.</param>
        /// <param name="value">The property value to set.</param>
        public static void SetPasswordRevealMode(PasswordBox passwordBox, PasswordRevealMode value)
        {
            passwordBox.SetValue(PasswordRevealModeProperty, value);
        }

        /// <summary>
        /// Identifies the PasswordRevealMode dependency property.
        /// </summary>
        public static readonly DependencyProperty PasswordRevealModeProperty =
            DependencyProperty.RegisterAttached(
                "PasswordRevealMode",
                typeof(PasswordRevealMode),
                typeof(PasswordBoxHelper),
                new FrameworkPropertyMetadata(PasswordRevealMode.Peek, OnPasswordRevealModeChanged));

        private static void OnPasswordRevealModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var helper = GetHelperInstance((PasswordBox)d);
            if (helper != null)
            {
                helper.UpdateTextBox();
                helper.UpdateVisualState(true);
            }
        }

        #endregion

        #region IsEnabled

        public static bool GetIsEnabled(PasswordBox passwordBox)
        {
            return (bool)passwordBox.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(PasswordBox passwordBox, bool value)
        {
            passwordBox.SetValue(IsEnabledProperty, value);
        }

        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsEnabled",
                typeof(bool),
                typeof(PasswordBoxHelper),
                new FrameworkPropertyMetadata(OnIsEnabledChanged));

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var passwordBox = (PasswordBox)d;
            if ((bool)e.NewValue)
            {
                SetHelperInstance(passwordBox, new PasswordBoxHelper(passwordBox));
            }
            else
            {
                passwordBox.ClearValue(HelperInstanceProperty);
            }
        }

        #endregion

        #region PlaceholderTextVisibility

        public static Visibility GetPlaceholderTextVisibility(Control control)
        {
            return (Visibility)control.GetValue(PlaceholderTextVisibilityProperty);
        }

        private static void SetPlaceholderTextVisibility(Control control, Visibility value)
        {
            control.SetValue(PlaceholderTextVisibilityPropertyKey, value);
        }

        private static readonly DependencyPropertyKey PlaceholderTextVisibilityPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly(
                "PlaceholderTextVisibility",
                typeof(Visibility),
                typeof(PasswordBoxHelper),
                new FrameworkPropertyMetadata(Visibility.Visible));

        public static readonly DependencyProperty PlaceholderTextVisibilityProperty = PlaceholderTextVisibilityPropertyKey.DependencyProperty;

        #endregion

        #region HelperInstance

        private static PasswordBoxHelper GetHelperInstance(PasswordBox passwordBox)
        {
            return (PasswordBoxHelper)passwordBox.GetValue(HelperInstanceProperty);
        }

        private static void SetHelperInstance(PasswordBox passwordBox, PasswordBoxHelper value)
        {
            passwordBox.SetValue(HelperInstanceProperty, value);
        }

        private static readonly DependencyProperty HelperInstanceProperty =
            DependencyProperty.RegisterAttached(
                "HelperInstance",
                typeof(PasswordBoxHelper),
                typeof(PasswordBoxHelper),
                new PropertyMetadata(OnHelperInstanceChanged));

        private static void OnHelperInstanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is PasswordBoxHelper oldHelper)
            {
                oldHelper.Detach();
            }

            if (e.NewValue is PasswordBoxHelper newHelper)
            {
                newHelper.Attach();
            }
        }

        #endregion

        private PasswordRevealMode PasswordRevealMode => GetPasswordRevealMode(_passwordBox);

        private static void OnDisabledCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
            e.Handled = true;
        }

        private void Attach()
        {
            _passwordBox.PasswordChanged += OnPasswordChanged;
            _passwordBox.GotFocus += OnGotFocus;
            _passwordBox.LostFocus += OnLostFocus;

            if (_passwordBox.IsLoaded)
            {
                OnApplyTemplate();
            }
            else
            {
                _passwordBox.Loaded += OnLoaded;
            }
        }

        private void Detach()
        {
            _passwordBox.PasswordChanged -= OnPasswordChanged;
            _passwordBox.GotFocus -= OnGotFocus;
            _passwordBox.LostFocus -= OnLostFocus;
            _passwordBox.Loaded -= OnLoaded;

            DetachTextBox();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _passwordBox.Loaded -= OnLoaded;
            OnApplyTemplate();
        }

        private void OnApplyTemplate()
        {
            DetachTextBox();
            _passwordBox.ApplyTemplate();

            _textBox = _passwordBox.GetTemplateChild<TextBox>("TextBox");

            if (_textBox != null)
            {
                _textBox.IsUndoEnabled = false;
                SpellCheck.SetIsEnabled(_textBox, false);
                _textBox.CommandBindings.Add(TextBoxCutBinding);
                _textBox.CommandBindings.Add(TextBoxCopyBinding);
                _textBox.TextChanged += OnTextBoxTextChanged;
                _textBox.IsVisibleChanged += OnTextBoxIsVisibleChanged;
                UpdateTextBox();
            }

            UpdateVisualState(false);
        }

        private void OnGotFocus(object sender, RoutedEventArgs e)
        {
            if (PasswordRevealMode == PasswordRevealMode.Visible && _textBox != null)
            {
                if (e.OriginalSource == _passwordBox)
                {
                    _textBox.Focus();
                    e.Handled = true;
                }
            }

            if (HasPassword())
            {
                _hideRevealButton = true;
            }

            UpdateVisualState(true);
        }

        private void OnLostFocus(object sender, RoutedEventArgs e)
        {
            UpdateVisualState(true);
        }

        private void OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            bool hasPassword = HasPassword();

            if (!hasPassword)
            {
                _hideRevealButton = false;
            }

            SetPlaceholderTextVisibility(_passwordBox, hasPassword ? Visibility.Collapsed : Visibility.Visible);
            UpdateTextBox();
            UpdateVisualState(true);
        }

        private void OnTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_isUpdatingTextBox &&
                PasswordRevealMode == PasswordRevealMode.Visible &&
                _textBox.IsVisible)
            {
                _isUpdatingPasswordBox = true;
                try
                {
                    _passwordBox.Password = ((TextBox)sender).Text;
                }
                finally
                {
                    _isUpdatingPasswordBox = false;
                }
            }
        }

        private void OnTextBoxIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UpdateTextBox();
        }

        private void DetachTextBox()
        {
            if (_textBox == null)
            {
                return;
            }

            _textBox.CommandBindings.Remove(TextBoxCutBinding);
            _textBox.CommandBindings.Remove(TextBoxCopyBinding);
            _textBox.TextChanged -= OnTextBoxTextChanged;
            _textBox.IsVisibleChanged -= OnTextBoxIsVisibleChanged;
            _textBox.Text = string.Empty;
            _textBox = null;
        }

        private bool HasPassword()
        {
            using var password = _passwordBox.SecurePassword;
            return password.Length > 0;
        }

        private void UpdateTextBox()
        {
            if (_isUpdatingPasswordBox || _textBox == null)
            {
                return;
            }

            string text = string.Empty;
            if (PasswordRevealMode != PasswordRevealMode.Hidden && _textBox.IsVisible)
            {
                // Password creates a managed plaintext string, so only read it
                // while the template is intentionally displaying plaintext.
                text = _passwordBox.Password;
            }

            if (_textBox.Text != text)
            {
                _isUpdatingTextBox = true;
                try
                {
                    _textBox.Text = text;
                }
                finally
                {
                    _isUpdatingTextBox = false;
                }
            }
        }

        private void UpdateVisualState(bool useTransitions)
        {
            bool buttonVisible = false;
            if (_passwordBox.IsFocused)
            {
                switch (PasswordRevealMode)
                {
                    case PasswordRevealMode.Peek:
                        buttonVisible = !_hideRevealButton && HasPassword();
                        break;
                    case PasswordRevealMode.Hidden:
                    case PasswordRevealMode.Visible:
                        buttonVisible = false;
                        break;
                }
            }

            VisualStateManager.GoToState(_passwordBox, buttonVisible ? ButtonVisibleState : ButtonCollapsedState, useTransitions);
        }
    }
}
