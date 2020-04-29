using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace SamplesCommon
{
    public static class Extensions
    {
        public static void AddKeyboardAccelerator(this ButtonBase button, Key key)
        {
            AddKeyboardAccelerator(button, new KeyGesture(key));
        }

        public static void AddKeyboardAccelerator(this ButtonBase button, Key key, ModifierKeys modifiers)
        {
            AddKeyboardAccelerator(button, new KeyGesture(key, modifiers));
        }

        public static void AddKeyboardAccelerator(this ButtonBase button, Key key, ModifierKeys modifiers, string displayString)
        {
            AddKeyboardAccelerator(button, new KeyGesture(key, modifiers, displayString));
        }

        private static void AddKeyboardAccelerator(this ButtonBase button, KeyGesture keyGesture)
        {
            var command = new RoutedCommand { InputGestures = { keyGesture } };
            button.Command = command;
            button.CommandBindings.Add(new CommandBinding(command, null, (s, e) => e.CanExecute = true));
        }
    }
}
