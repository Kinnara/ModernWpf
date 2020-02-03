using ModernWpf;
using System;
using System.Windows;
using System.Windows.Input;

namespace SamplesCommon
{
    public static class CommonCommands
    {
        public static ICommand ToggleTheme { get; } = new ToggleThemeCommand();

        private class ToggleThemeCommand : ICommand
        {
            public event EventHandler CanExecuteChanged;

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public void Execute(object parameter)
            {
                if (parameter is FrameworkElement fe)
                {
                    if (ThemeManager.GetActualTheme(fe) == ElementTheme.Dark)
                    {
                        ThemeManager.SetRequestedTheme(fe, ElementTheme.Light);
                    }
                    else
                    {
                        ThemeManager.SetRequestedTheme(fe, ElementTheme.Dark);
                    }
                }
                else
                {
                    var tm = ThemeManager.Current;
                    if (tm.ActualApplicationTheme == ApplicationTheme.Dark)
                    {
                        tm.ApplicationTheme = ApplicationTheme.Light;
                    }
                    else
                    {
                        tm.ApplicationTheme = ApplicationTheme.Dark;
                    }
                }
            }
        }
    }
}
