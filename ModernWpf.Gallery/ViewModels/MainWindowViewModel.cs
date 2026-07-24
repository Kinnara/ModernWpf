using System;
using System.ComponentModel;
using System.Windows.Input;

namespace ModernWpf.Gallery.ViewModels
{
    public sealed class MainWindowViewModel : INotifyPropertyChanged
    {
        private string _applicationTitle = GalleryBranding.DisplayName;
        private readonly Action _backAction;
        private readonly Action _settingsAction;
        private readonly Action _forwardAction;
        private readonly Func<bool> _canNavigateBack;
        private bool _canNavigateback;

        public MainWindowViewModel(
            Action backAction,
            Action settingsAction,
            Action forwardAction = null,
            Func<bool> canNavigateBack = null)
        {
            if (backAction == null)
            {
                throw new ArgumentNullException(nameof(backAction));
            }

            if (settingsAction == null)
            {
                throw new ArgumentNullException(nameof(settingsAction));
            }

            _backAction = backAction;
            _settingsAction = settingsAction;
            _forwardAction = forwardAction;
            _canNavigateBack = canNavigateBack;

            BackCommand = new RelayCommand(delegate { Back(); });
            SettingsCommand = new RelayCommand(delegate { Settings(); });
            ForwardCommand = new RelayCommand(delegate { Forward(); });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string ApplicationTitle
        {
            get { return _applicationTitle; }
        }

        public ICommand BackCommand { get; }

        public ICommand SettingsCommand { get; }

        public ICommand ForwardCommand { get; }

        public void Back()
        {
            _backAction();
        }

        public void Settings()
        {
            _settingsAction();
        }

        public void Forward()
        {
            if (_forwardAction != null)
            {
                _forwardAction();
            }
        }

        public void UpdateCanNavigateBack()
        {
            if (_canNavigateBack != null)
            {
                CanNavigateback = _canNavigateBack();
            }
        }

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
