using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Windows.Input;

namespace ModernWpf.Gallery.Pages.WpfGallery.DesignGuidance
{
    public partial class IconographyPageViewModel : INotifyPropertyChanged
    {
        private readonly RelayCommand _previousPageCommand;
        private readonly RelayCommand _nextPageCommand;
        private List<IconData> _allIcons = new List<IconData>();
        private List<IconData> _searchFilteredIcons = new List<IconData>();
        private IconData _selectedIcon;
        private string _searchText = string.Empty;
        private int _currentPage = 1;
        private int _totalPages = 1;
        private int _selectedPageSizeIndex = 1;

        public IconographyPageViewModel()
        {
            LoadDataCommand = new RelayCommand(delegate { LoadData(); });
            ApplyTagFilterCommand = new RelayCommand(parameter => ApplyTagFilter(parameter as string));
            _previousPageCommand = new RelayCommand(delegate { PreviousPage(); }, delegate { return CanGoToPreviousPage(); });
            _nextPageCommand = new RelayCommand(delegate { NextPage(); }, delegate { return CanGoToNextPage(); });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string PageTitle
        {
            get { return "Icons"; }
        }

        public string PageDescription
        {
            get { return "Guide showing how to use icons in your application."; }
        }

        public ObservableCollection<IconData> DisplayedIcons { get; } = new ObservableCollection<IconData>();

        public IconData SelectedIcon
        {
            get { return _selectedIcon; }
            set
            {
                if (!ReferenceEquals(_selectedIcon, value))
                {
                    _selectedIcon = value;
                    OnPropertyChanged("SelectedIcon");
                }
            }
        }

        public string SearchText
        {
            get { return _searchText; }
            set
            {
                value = value ?? string.Empty;
                if (!string.Equals(_searchText, value, StringComparison.Ordinal))
                {
                    _searchText = value;
                    OnPropertyChanged("SearchText");
                    UpdateSearchFilter();
                }
            }
        }

        public int CurrentPage
        {
            get { return _currentPage; }
            private set
            {
                if (_currentPage != value)
                {
                    _currentPage = value;
                    OnPropertyChanged("CurrentPage");
                    _previousPageCommand.RaiseCanExecuteChanged();
                    _nextPageCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public int TotalPages
        {
            get { return _totalPages; }
            private set
            {
                if (_totalPages != value)
                {
                    _totalPages = value;
                    OnPropertyChanged("TotalPages");
                    _previousPageCommand.RaiseCanExecuteChanged();
                    _nextPageCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public int SelectedPageSizeIndex
        {
            get { return _selectedPageSizeIndex; }
            set
            {
                if (value < 0)
                {
                    return;
                }

                if (_selectedPageSizeIndex != value)
                {
                    _selectedPageSizeIndex = value;
                    OnPropertyChanged("SelectedPageSizeIndex");
                    CurrentPage = 1;
                    UpdatePagination();
                }
            }
        }

        public IReadOnlyList<string> PageSizeOptions { get; } = new[] { "100", "250", "500", "1000", "All" };

        public ICommand LoadDataCommand { get; }

        public ICommand ApplyTagFilterCommand { get; }

        public ICommand PreviousPageCommand
        {
            get { return _previousPageCommand; }
        }

        public ICommand NextPageCommand
        {
            get { return _nextPageCommand; }
        }

        private void LoadData()
        {
            _allIcons = ReadIconData().ToList();
            _searchFilteredIcons = new List<IconData>(_allIcons);
            SelectedIcon = _allIcons.FirstOrDefault();
            CurrentPage = 1;
            UpdatePagination();
        }

        private int PageSize
        {
            get
            {
                return SelectedPageSizeIndex == PageSizeOptions.Count - 1
                    ? int.MaxValue
                    : int.Parse(PageSizeOptions[SelectedPageSizeIndex]);
            }
        }

        private void UpdateSearchFilter()
        {
            var previousSelectedIcon = SelectedIcon;
            var selectedIconName = previousSelectedIcon == null ? null : previousSelectedIcon.Name;
            var comparison = StringComparison.OrdinalIgnoreCase;
            var filterText = SearchText ?? string.Empty;

            if (string.IsNullOrWhiteSpace(filterText))
            {
                _searchFilteredIcons = new List<IconData>(_allIcons);
            }
            else
            {
                _searchFilteredIcons = _allIcons
                    .Where(icon =>
                        icon.Name.IndexOf(filterText, comparison) >= 0 ||
                        (icon.Tags != null && icon.Tags.Any(tag => tag.IndexOf(filterText, comparison) >= 0)))
                    .ToList();
            }

            CurrentPage = 1;
            UpdatePagination(false);

            if (_searchFilteredIcons.Count == 0)
            {
                SelectedIcon = previousSelectedIcon;
                return;
            }

            if (string.IsNullOrWhiteSpace(filterText))
            {
                SelectedIcon = DisplayedIcons.FirstOrDefault();
                return;
            }

            var retainedIcon = !string.IsNullOrWhiteSpace(selectedIconName)
                ? DisplayedIcons.FirstOrDefault(icon => string.Equals(icon.Name, selectedIconName, StringComparison.Ordinal))
                : null;
            SelectedIcon = retainedIcon ?? DisplayedIcons.FirstOrDefault();
        }

        private void ApplyTagFilter(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
            {
                return;
            }

            SearchText = tag.Trim();
        }

        private void PreviousPage()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
                UpdateDisplayedIcons();
            }
        }

        private bool CanGoToPreviousPage()
        {
            return CurrentPage > 1;
        }

        private void NextPage()
        {
            if (CurrentPage < TotalPages)
            {
                CurrentPage++;
                UpdateDisplayedIcons();
            }
        }

        private bool CanGoToNextPage()
        {
            return CurrentPage < TotalPages;
        }

        private void UpdatePagination(bool resetSelectedIcon = true)
        {
            var pageSize = PageSize;
            TotalPages = pageSize == int.MaxValue ? 1 : (int)Math.Ceiling((double)_searchFilteredIcons.Count / pageSize);
            if (TotalPages == 0)
            {
                TotalPages = 1;
            }

            if (CurrentPage > TotalPages)
            {
                CurrentPage = TotalPages;
            }

            UpdateDisplayedIcons(resetSelectedIcon);
        }

        private void UpdateDisplayedIcons(bool resetSelectedIcon = true)
        {
            DisplayedIcons.Clear();

            var pageSize = PageSize;
            var iconsToDisplay = pageSize == int.MaxValue
                ? _searchFilteredIcons
                : _searchFilteredIcons.Skip((CurrentPage - 1) * pageSize).Take(pageSize);

            foreach (var icon in iconsToDisplay)
            {
                DisplayedIcons.Add(icon);
            }

            if (resetSelectedIcon)
            {
                SelectedIcon = DisplayedIcons.FirstOrDefault();
            }
        }

        private static IEnumerable<IconData> ReadIconData()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Samples", "Data", "IconsData.json");
            try
            {
                if (File.Exists(path))
                {
                    using (var stream = File.OpenRead(path))
                    {
                        var serializer = new DataContractJsonSerializer(typeof(List<IconData>));
                        var icons = serializer.ReadObject(stream) as List<IconData>;
                        if (icons != null && icons.Count != 0)
                        {
                            return icons;
                        }
                    }
                }
            }
            catch (Exception)
            {
            }

            return new[]
            {
                new IconData { Code = "E700", Name = "GlobalNavButton", Tags = new List<string> { "menu", "hamburger", "symbol-icon" } },
                new IconData { Code = "E8C8", Name = "Copy", Tags = new List<string> { "clipboard", "duplicate" } },
                new IconData { Code = "E73E", Name = "Accept", Tags = new List<string> { "check", "success" } },
                new IconData { Code = "E721", Name = "Find", Tags = new List<string> { "search" } }
            };
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
            private readonly Predicate<object> _canExecute;

            public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
            {
                _execute = execute;
                _canExecute = canExecute;
            }

            public event EventHandler CanExecuteChanged;

            public bool CanExecute(object parameter)
            {
                return _canExecute == null || _canExecute(parameter);
            }

            public void Execute(object parameter)
            {
                _execute(parameter);
            }

            public void RaiseCanExecuteChanged()
            {
                var handler = CanExecuteChanged;
                if (handler != null)
                {
                    handler(this, EventArgs.Empty);
                }
            }
        }
    }
}
