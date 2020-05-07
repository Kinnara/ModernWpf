using System.Diagnostics;
using System.Windows;
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

        public static void SealValues(this ResourceDictionary rd)
        {
            foreach (var md in rd.MergedDictionaries)
            {
                SealValues(md);
            }

            foreach (var value in rd.Values)
            {
                if (value is Freezable freezable)
                {
                    if (!freezable.CanFreeze)
                    {
                        var enumerator = freezable.GetLocalValueEnumerator();
                        while (enumerator.MoveNext())
                        {
                            var property = enumerator.Current.Property;
                            if (DependencyPropertyHelper.GetValueSource(freezable, property).IsExpression)
                            {
                                freezable.SetValue(property, freezable.GetValue(property));
                                Debug.Assert(!DependencyPropertyHelper.GetValueSource(freezable, property).IsExpression);
                            }
                        }
                    }

                    if (!freezable.IsFrozen)
                    {
                        freezable.Freeze();
                    }
                }
                else if (value is Style style)
                {
                    if (!style.IsSealed)
                    {
                        style.Seal();
                    }
                }
            }
        }
    }
}
