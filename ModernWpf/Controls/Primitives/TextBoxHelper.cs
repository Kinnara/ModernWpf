using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.Controls.Primitives
{
    public static class TextBoxHelper
    {
        private const string ButtonStatesGroup = "ButtonStates";
        private const string ButtonVisibleState = "ButtonVisible";
        private const string ButtonCollapsedState = "ButtonCollapsed";

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
                typeof(TextBoxHelper),
                new PropertyMetadata(OnIsEnabledChanged));

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var textBox = (TextBox)d;

            if ((bool)e.NewValue)
            {
                textBox.Loaded += OnLoaded;
                textBox.IsKeyboardFocusedChanged += OnIsKeyboardFocusedChanged;
                textBox.TextChanged += OnTextChanged;
            }
            else
            {
                textBox.Loaded -= OnLoaded;
                textBox.IsKeyboardFocusedChanged -= OnIsKeyboardFocusedChanged;
                textBox.TextChanged -= OnTextChanged;
            }
        }

        #endregion

        #region IsDeleteButton

        public static bool GetIsDeleteButton(Button button)
        {
            return (bool)button.GetValue(IsDeleteButtonProperty);
        }

        public static void SetIsDeleteButton(Button button, bool value)
        {
            button.SetValue(IsDeleteButtonProperty, value);
        }

        public static readonly DependencyProperty IsDeleteButtonProperty =
            DependencyProperty.RegisterAttached(
                "IsDeleteButton",
                typeof(bool),
                typeof(TextBoxHelper),
                new PropertyMetadata(OnIsDeleteButtonChanged));

        private static void OnIsDeleteButtonChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var button = (Button)d;

            if ((bool)e.OldValue)
            {
                button.Click -= OnDeleteButtonClick;
            }

            if ((bool)e.NewValue)
            {
                button.Click += OnDeleteButtonClick;
            }
        }

        #endregion

        private static void OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateVisualStates((TextBox)sender);
        }

        private static void OnIsKeyboardFocusedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UpdateVisualStates((TextBox)sender);
        }

        private static void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateVisualStates((TextBox)sender);
        }

        private static void OnDeleteButtonClick(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            if (button.TemplatedParent is TextBox textBox)
            {
                textBox.SetCurrentValue(TextBox.TextProperty, null);
            }
        }

        private static void UpdateVisualStates(TextBox textBox)
        {
            bool buttonVisible = textBox.IsKeyboardFocused && !string.IsNullOrEmpty(textBox.Text);
            VisualStateManager.GoToState(textBox, buttonVisible ? ButtonVisibleState : ButtonCollapsedState, true);
        }
    }
}
