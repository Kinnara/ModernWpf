using ModernWpf.Media.Animation;
using System;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Linq;

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

        public static bool NavigateEx(this Frame frame, Type sourcePageType)
        {
            return frame.Navigate(Activator.CreateInstance(sourcePageType));
        }

        public static bool NavigateEx(this Frame frame, Type sourcePageType, object parameter)
        {
            return frame.Navigate(Activator.CreateInstance(sourcePageType), parameter);
        }

        public static bool Navigate(this Frame frame, Type sourcePageType, object parameter, NavigationTransitionInfo infoOverride)
        {
            return frame.Navigate(Activator.CreateInstance(sourcePageType), parameter);
        }

        public static bool NavigateToType(this Frame frame, Type sourcePageType)
        {
            return frame.Navigate(Activator.CreateInstance(sourcePageType));
        }

        public static bool NavigateToType(this NavigationService navigationService, Type sourcePageType)
        {
            return navigationService.Navigate(Activator.CreateInstance(sourcePageType));
        }

        public static int BackStackDepth(this Frame frame)
        {
            return frame.BackStack?.Cast<object>().Count() ?? 0;
        }

        public static Type CurrentSourcePageType(this Frame frame)
        {
            return frame.Content?.GetType();
        }

        public static Type SourcePageType(this NavigationEventArgs e)
        {
            return e.Content?.GetType();
        }
    }
}
