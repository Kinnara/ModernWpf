using System;
using System.ComponentModel;

namespace ModernWpf.Gallery.Pages.WpfGallery.Samples
{
    public class UserDashboardUser : INotifyPropertyChanged
    {
        private string _firstName;
        private string _lastName;
        private string _company;
        private string _address;
        private bool _isNewGraduate;
        private string _imageId = "91";
        private int _age;
        private DateTime _dateOfJoining;

        public string FirstName
        {
            get { return _firstName; }
            set
            {
                if (SetProperty(ref _firstName, value, nameof(FirstName)))
                {
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        public string LastName
        {
            get { return _lastName; }
            set
            {
                if (SetProperty(ref _lastName, value, nameof(LastName)))
                {
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        public string Name => $"{FirstName} {LastName}";

        public string ImageId
        {
            get { return _imageId; }
            set
            {
                if (SetProperty(ref _imageId, value, nameof(ImageId)))
                {
                    OnPropertyChanged(nameof(ImageKey));
                }
            }
        }

        public string ImageKey => $"p{ImageId}";

        public string Company
        {
            get { return _company; }
            set { SetProperty(ref _company, value, nameof(Company)); }
        }

        public string Address
        {
            get { return _address; }
            set { SetProperty(ref _address, value, nameof(Address)); }
        }

        public int Age
        {
            get { return _age; }
            set { SetProperty(ref _age, value, nameof(Age)); }
        }

        public DateTime DateOfJoining
        {
            get { return _dateOfJoining; }
            set { SetProperty(ref _dateOfJoining, value, nameof(DateOfJoining)); }
        }

        public bool IsNewGraduate
        {
            get { return _isNewGraduate; }
            set { SetProperty(ref _isNewGraduate, value, nameof(IsNewGraduate)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public UserDashboardUser(string firstName, string lastName)
        {
            FirstName = firstName;
            LastName = lastName;
        }

        public UserDashboardUser(UserDashboardUser user)
            : this(
                user.ImageId,
                user.FirstName,
                user.LastName,
                user.Company,
                user.Address,
                user.Age,
                user.DateOfJoining,
                user.IsNewGraduate)
        {
        }

        public UserDashboardUser(string imageId, string firstName, string lastName, string company, string address, int age, DateTime dateOfJoining, bool isNewGraduate)
        {
            ImageId = imageId;
            FirstName = firstName;
            LastName = lastName;
            Company = company;
            Address = address;
            Age = age;
            DateOfJoining = dateOfJoining;
            IsNewGraduate = isNewGraduate;
        }

        private bool SetProperty<T>(ref T field, T value, string propertyName)
        {
            if (Equals(field, value))
            {
                return false;
            }

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
