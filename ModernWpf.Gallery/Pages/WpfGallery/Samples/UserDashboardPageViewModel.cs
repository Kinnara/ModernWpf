using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;
using ModernWpf.Gallery.Testing;

namespace ModernWpf.Gallery.Pages.WpfGallery.Samples
{
    public class UserDashboardPageViewModel : INotifyPropertyChanged
    {
        private const int UsersVisualTestSeed = 32043;
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
        private ObservableCollection<UserDashboardUser> _users;

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
                    RestartTimer(_deletedMessageTimer);
                }
            }
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
                }
            }
        }

        public ObservableCollection<UserDashboardUser> Users
        {
            get { return _users; }
            set { SetProperty(ref _users, value, "Users"); }
        }

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
            var random = GalleryDiagnostics.IsEnabled
                ? new GallerySampleRandom(UsersVisualTestSeed)
                : new GallerySampleRandom();
            var users = new ObservableCollection<UserDashboardUser>();

            DateTime startDate = new DateTime(2020, 1, 1);
            DateTime endDate = DateTime.Now.Date;
            int range = (endDate - startDate).Days;

            var imageids = new[]
            {
                "64", "65", "91", "103", "177", "334", "338", "342", "349", "366", "367", "373",
                "375", "378", "399", "447", "453", "473", "469", "505"
            };

            var names = new[]
            {
                "John",
                "Winston",
                "Adrianna",
                "Spencer",
                "Phoebe",
                "Lucas",
                "Carl",
                "Marissa",
                "Brandon",
                "Antoine",
                "Arielle"
            };

            var surnames = new[]
            {
                "Doe",
                "Tapia",
                "Cisneros",
                "Lynch",
                "Munoz",
                "Marsh",
                "Hudson",
                "Bartlett",
                "Gregory",
                "Banks",
                "Hood",
                "Fry",
                "Carroll"
            };

            var companies = new[]
            {
                "Luminary Nexus",
                "CrestWave Dynamics",
                "Horizon Ventures",
                "Sapphire Pulse Technologies",
                "EmberLight Industries",
                "StellarEdge Ventrues"
            };

            var addresses = new[]
            {
                "Room 1450, 9819 Rutledge Parkway, Saint Louis, Missouri, United States",
                "18th Floor, 3631 Manitowish Point, Mobile, Alabama, United States",
                "Apt 1145, Kansas, United States",
                "PO Box 54647, 252 Derek Way, Flushing, New York, United States",
                "Apt 687, 47182 Superior Avenue, Kansas City, Missouri, ",
                "20th Floor, 5524 Badeau Pass, Glendale, Arizona, United States",
                "Room 1121, 9 Kipling Terrace, Winston Salem, North Carolina, United States",
                "16th Floor, Odessa, Texas, United States",
                "Suite 82, 44 Shasta Terrace, Las Cruces, United States",
                "Room 1930, 45779 Anhalt Junction, Detroit, Michigan, United States",
                "PO Box 54206, 14 Waubesa Street, Greenville, South Carolina, United States",
                "1st Floor, 78 Barby Park, South Dakota, United States",
                "Room 1426, 7394 Welch Alley, Huntsville, Alabama, United States",
                "20th Floor, 11 Eastwood Road, El Paso, Texas, United States",
                "Suite 92, 9 Hermina Point, Bakersfield, United States",
                string.Empty
            };

            for (int i = 0; i < 20; i++)
            {
                int randomDays = random.Next(range + 1);
                users.Add(
                    new UserDashboardUser(
                        imageids[random.Next(0, imageids.Length)],
                        names[random.Next(0, names.Length)],
                        surnames[random.Next(0, surnames.Length)],
                        companies[random.Next(0, companies.Length)],
                        addresses[random.Next(0, addresses.Length)],
                        random.Next(21, 63),
                        startDate.AddDays(randomDays),
                        random.Next(2) == 1));
            }

            return users;
        }

        private void AddUser()
        {
            var user = new UserDashboardUser("New User", string.Empty);
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
