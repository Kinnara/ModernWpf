using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WinUIResourcesConverter
{
    public class ResourcesFile : INotifyPropertyChanged
    {
        internal const string DefaultResourcesFileName = "Resources";

        public ResourcesFile(string languageName)
        {
            LanguageName = languageName;
            IsDefaultResource = RESXConverter.IsDefaultLanguage(languageName);
        }

        public string LanguageName { get; }

        public bool IsDefaultResource { get; }

        private bool hasConverted;

        public bool HasConverted
        {
            get => hasConverted;
            set
            {
                if (value != hasConverted)
                {
                    hasConverted = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
