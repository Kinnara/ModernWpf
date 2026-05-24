using System;
using System.ComponentModel;
using System.Windows.Input;

namespace ModernWpf.Gallery.ViewModels
{
    public sealed class MainWindowViewModel : INotifyPropertyChanged
    {
        private bool _canNavigateback;

        public MainWindowViewModel(Action backAction, Action settingsAction)
        {
            if (backAction == null)
            {
                throw new ArgumentNullException(nameof(backAction));
            }

            if (settingsAction == null)
            {
                throw new ArgumentNullException(nameof(settingsAction));
            }

            BackCommand = new RelayCommand(delegate { backAction(); });
            SettingsCommand = new RelayCommand(delegate { settingsAction(); });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string ApplicationTitle { get; } = "WPF Gallery";

        public ICommand BackCommand { get; }

        public ICommand SettingsCommand { get; }

        public bool CanNavigateback
        {
            get { return _canNavigateback; }
            set
            {
                if (_canNavigateback == value)
                {
                    return;
                }

                _canNavigateback = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanNavigateback)));
            }
        }

        private sealed class RelayCommand : ICommand
        {
            private readonly Action<object> _execute;

            public RelayCommand(Action<object> execute)
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
}
