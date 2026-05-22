using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;

namespace ModernWpf.Gallery.Pages.WpfGallery.Samples
{
    public sealed class UserDashboardPageViewModel : INotifyPropertyChanged
    {
        private readonly RelayCommand _addUserCommand;
        private readonly RelayCommand _editUserCancelCommand;
        private readonly RelayCommand _editUserCommitCommand;
        private readonly RelayCommand _editUserStartCommand;
        private readonly RelayCommand _removeUserCommand;
        private readonly DispatcherTimer _deletedMessageTimer;
        private readonly DispatcherTimer _savedMessageTimer;
        private string _deletedName = string.Empty;
        private UserDashboardUser _editableUser;
        private bool _isEditing;
        private bool _isReadOnly = true;
        private bool _isSaved;
        private UserDashboardUser _selectedUser;

        public UserDashboardPageViewModel()
        {
            Users = GenerateUsers();
            _addUserCommand = new RelayCommand(delegate { AddUser(); });
            _editUserCancelCommand = new RelayCommand(delegate { EditUserCancel(); });
            _editUserCommitCommand = new RelayCommand(delegate { EditUserCommit(); });
            _editUserStartCommand = new RelayCommand(delegate { EditUserStart(); });
            _removeUserCommand = new RelayCommand(parameter => RemoveUser(parameter as UserDashboardUser));

            _deletedMessageTimer = CreateMessageTimer(delegate { DeletedName = string.Empty; });
            _savedMessageTimer = CreateMessageTimer(delegate { IsSaved = false; });
            SelectedUser = Users.FirstOrDefault();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand AddUserCommand
        {
            get { return _addUserCommand; }
        }

        public string DeletedName
        {
            get { return _deletedName; }
            set
            {
                if (SetProperty(ref _deletedName, value, "DeletedName") && !string.IsNullOrEmpty(value))
                {
                    OnPropertyChanged("DeletedStatusText");
                    RestartTimer(_deletedMessageTimer);
                }
            }
        }

        public string DeletedStatusText
        {
            get { return "User " + (string.IsNullOrEmpty(DeletedName) && SelectedUser != null ? SelectedUser.Name : DeletedName) + " Deleted!"; }
        }

        public UserDashboardUser EditableUser
        {
            get { return _editableUser; }
            set { SetProperty(ref _editableUser, value, "EditableUser"); }
        }

        public ICommand EditUserCancelCommand
        {
            get { return _editUserCancelCommand; }
        }

        public ICommand EditUserCommitCommand
        {
            get { return _editUserCommitCommand; }
        }

        public ICommand EditUserStartCommand
        {
            get { return _editUserStartCommand; }
        }

        public bool IsEditing
        {
            get { return _isEditing; }
            set { SetProperty(ref _isEditing, value, "IsEditing"); }
        }

        public bool IsReadOnly
        {
            get { return _isReadOnly; }
            set { SetProperty(ref _isReadOnly, value, "IsReadOnly"); }
        }

        public bool IsSaved
        {
            get { return _isSaved; }
            set
            {
                if (SetProperty(ref _isSaved, value, "IsSaved") && value)
                {
                    RestartTimer(_savedMessageTimer);
                }
            }
        }

        public ICommand RemoveUserCommand
        {
            get { return _removeUserCommand; }
        }

        public UserDashboardUser SelectedUser
        {
            get { return _selectedUser; }
            set
            {
                if (SetProperty(ref _selectedUser, value, "SelectedUser") && value != null && value != EditableUser)
                {
                    EditableUser = new UserDashboardUser(value);
                    IsReadOnly = true;
                    IsEditing = false;
                    OnPropertyChanged("DeletedStatusText");
                }
            }
        }

        public ObservableCollection<UserDashboardUser> Users { get; }

        private static DispatcherTimer CreateMessageTimer(Action tick)
        {
            var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2)
            };
            timer.Tick += delegate
            {
                timer.Stop();
                tick();
            };
            return timer;
        }

        private static ObservableCollection<UserDashboardUser> GenerateUsers()
        {
            return new ObservableCollection<UserDashboardUser>
            {
                new UserDashboardUser("91", "John", "Doe", "Luminary Nexus", "Room 1450, 9819 Rutledge Parkway, Saint Louis, Missouri, United States", 37, new DateTime(2022, 8, 15), false),
                new UserDashboardUser("65", "Adrianna", "Cisneros", "CrestWave Dynamics", "18th Floor, 3631 Manitowish Point, Mobile, Alabama, United States", 29, new DateTime(2021, 3, 22), true),
                new UserDashboardUser("64", "Spencer", "Lynch", "Horizon Ventures", "Apt 1145, Kansas, United States", 44, new DateTime(2020, 11, 4), false),
                new UserDashboardUser("103", "Phoebe", "Munoz", "Sapphire Pulse Technologies", "PO Box 54647, 252 Derek Way, Flushing, New York, United States", 25, new DateTime(2024, 1, 9), true),
                new UserDashboardUser("177", "Lucas", "Marsh", "EmberLight Industries", "20th Floor, 5524 Badeau Pass, Glendale, Arizona, United States", 32, new DateTime(2023, 6, 5), false),
                new UserDashboardUser("334", "Marissa", "Bartlett", "StellarEdge Ventrues", "Room 1121, 9 Kipling Terrace, Winston Salem, North Carolina, United States", 41, new DateTime(2021, 9, 18), false),
                new UserDashboardUser("338", "Brandon", "Gregory", "Luminary Nexus", "16th Floor, Odessa, Texas, United States", 28, new DateTime(2024, 4, 12), true),
                new UserDashboardUser("342", "Antoine", "Banks", "CrestWave Dynamics", "Suite 82, 44 Shasta Terrace, Las Cruces, United States", 36, new DateTime(2022, 12, 1), false),
                new UserDashboardUser("349", "Winston", "Tapia", "Horizon Ventures", "Room 1930, 45779 Anhalt Junction, Detroit, Michigan, United States", 52, new DateTime(2020, 5, 29), false),
                new UserDashboardUser("366", "Carl", "Hudson", "Sapphire Pulse Technologies", "PO Box 54206, 14 Waubesa Street, Greenville, South Carolina, United States", 34, new DateTime(2023, 2, 20), true),
                new UserDashboardUser("367", "Arielle", "Hood", "EmberLight Industries", "1st Floor, 78 Barby Park, South Dakota, United States", 31, new DateTime(2021, 7, 11), false),
                new UserDashboardUser("373", "Clara", "Fry", "StellarEdge Ventrues", "Room 1426, 7394 Welch Alley, Huntsville, Alabama, United States", 49, new DateTime(2020, 10, 8), false),
                new UserDashboardUser("375", "Elliot", "Carroll", "Luminary Nexus", "20th Floor, 11 Eastwood Road, El Paso, Texas, United States", 27, new DateTime(2024, 3, 6), true),
                new UserDashboardUser("378", "Amelia", "Doe", "CrestWave Dynamics", "Suite 92, 9 Hermina Point, Bakersfield, United States", 39, new DateTime(2022, 1, 24), false),
                new UserDashboardUser("399", "Grant", "Tapia", "Horizon Ventures", "Apt 687, 47182 Superior Avenue, Kansas City, Missouri", 46, new DateTime(2021, 12, 19), false),
                new UserDashboardUser("447", "Nora", "Cisneros", "Sapphire Pulse Technologies", "Room 1450, 9819 Rutledge Parkway, Saint Louis, Missouri, United States", 26, new DateTime(2024, 5, 3), true),
                new UserDashboardUser("453", "Milo", "Lynch", "EmberLight Industries", "18th Floor, 3631 Manitowish Point, Mobile, Alabama, United States", 43, new DateTime(2020, 4, 17), false),
                new UserDashboardUser("469", "Leah", "Munoz", "StellarEdge Ventrues", "Apt 1145, Kansas, United States", 35, new DateTime(2022, 9, 27), false),
                new UserDashboardUser("473", "Theo", "Marsh", "Luminary Nexus", "PO Box 54647, 252 Derek Way, Flushing, New York, United States", 58, new DateTime(2021, 5, 14), false),
                new UserDashboardUser("505", "Iris", "Banks", "CrestWave Dynamics", "20th Floor, 5524 Badeau Pass, Glendale, Arizona, United States", 30, new DateTime(2023, 11, 2), true)
            };
        }

        private void AddUser()
        {
            var user = new UserDashboardUser("New User", string.Empty)
            {
                DateOfJoining = DateTime.Today
            };
            Users.Add(user);
            SelectedUser = user;
            IsReadOnly = false;
            IsEditing = true;
        }

        private void EditUserCancel()
        {
            EditableUser = SelectedUser == null ? null : new UserDashboardUser(SelectedUser);
            IsReadOnly = true;
            IsEditing = false;
        }

        private void EditUserCommit()
        {
            if (EditableUser == null || SelectedUser == null)
            {
                return;
            }

            var index = Users.IndexOf(SelectedUser);
            if (index < 0)
            {
                return;
            }

            Users.RemoveAt(index);
            Users.Insert(index, EditableUser);
            SelectedUser = Users[index];
            IsReadOnly = true;
            IsEditing = false;
            IsSaved = true;
        }

        private void EditUserStart()
        {
            if (SelectedUser == null)
            {
                return;
            }

            EditableUser = new UserDashboardUser(SelectedUser);
            IsReadOnly = false;
            IsEditing = true;
        }

        private void RemoveUser(UserDashboardUser selectedUser)
        {
            if (selectedUser == null)
            {
                return;
            }

            DeletedName = selectedUser.Name;
            var index = Users.Last().Equals(selectedUser) ? Users.IndexOf(selectedUser) - 1 : Users.IndexOf(selectedUser) + 1;
            SelectedUser = index >= 0 && index < Users.Count ? Users[index] : null;
            Users.Remove(selectedUser);
            IsReadOnly = true;
            IsEditing = false;
        }

        private void RestartTimer(DispatcherTimer timer)
        {
            timer.Stop();
            timer.Start();
        }

        private bool SetProperty<T>(ref T field, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        private void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private sealed class RelayCommand : ICommand
        {
            private readonly Action<object> _execute;

            public RelayCommand(Action<object> execute)
            {
                _execute = execute;
            }

            public event EventHandler CanExecuteChanged
            {
                add { CommandManager.RequerySuggested += value; }
                remove { CommandManager.RequerySuggested -= value; }
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
