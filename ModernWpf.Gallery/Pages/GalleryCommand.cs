using System;
using System.Windows.Input;

namespace ModernWpf.Gallery.Pages
{
    internal sealed class GalleryCommand : ICommand
    {
        private readonly Action<object> _execute;

        public GalleryCommand(Action<object> execute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }
    }
}
