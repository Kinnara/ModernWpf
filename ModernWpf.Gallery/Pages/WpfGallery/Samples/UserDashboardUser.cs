using System;
using System.ComponentModel;

namespace ModernWpf.Gallery.Pages.WpfGallery.Samples
{
    public sealed class UserDashboardUser : INotifyPropertyChanged
    {
        private string _address;
        private int _age;
        private string _company;
        private DateTime _dateOfJoining;
        private string _firstName;
        private string _imageId = "91";
        private bool _isNewGraduate;
        private string _lastName;

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

        public event PropertyChangedEventHandler PropertyChanged;

        public string Address
        {
            get { return _address; }
            set { SetProperty(ref _address, value, "Address"); }
        }

        public int Age
        {
            get { return _age; }
            set { SetProperty(ref _age, value, "Age"); }
        }

        public string Company
        {
            get { return _company; }
            set { SetProperty(ref _company, value, "Company"); }
        }

        public DateTime DateOfJoining
        {
            get { return _dateOfJoining; }
            set { SetProperty(ref _dateOfJoining, value, "DateOfJoining"); }
        }

        public string FirstName
        {
            get { return _firstName; }
            set
            {
                if (SetProperty(ref _firstName, value, "FirstName"))
                {
                    OnPropertyChanged("Name");
                }
            }
        }

        public string ImageId
        {
            get { return _imageId; }
            set
            {
                if (SetProperty(ref _imageId, value, "ImageId"))
                {
                    OnPropertyChanged("ImageKey");
                }
            }
        }

        public string ImageKey
        {
            get { return "p" + ImageId; }
        }

        public bool IsNewGraduate
        {
            get { return _isNewGraduate; }
            set { SetProperty(ref _isNewGraduate, value, "IsNewGraduate"); }
        }

        public string LastName
        {
            get { return _lastName; }
            set
            {
                if (SetProperty(ref _lastName, value, "LastName"))
                {
                    OnPropertyChanged("Name");
                }
            }
        }

        public string Name
        {
            get { return FirstName + " " + LastName; }
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

        private void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
