using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Windows.Input;
using ModernWpf.Gallery.Pages.WpfGallery;

namespace ModernWpf.Gallery.Pages.WpfGallery.DesignGuidance
{
    public partial class IconsPageViewModel : WpfGalleryPageViewModel
    {
        private readonly RelayCommand _previousPageCommand;
        private readonly RelayCommand _nextPageCommand;
        private ICollection<IconData> _allIcons = new List<IconData>();
        private IconData _selectedIcon;
        private string _searchText = string.Empty;
        private ObservableCollection<IconData> _searchFilteredIcons = new ObservableCollection<IconData>();
        private ObservableCollection<IconData> _displayedIcons = new ObservableCollection<IconData>();
        private int _currentPage = 1;
        private int _totalPages = 1;
        private int _selectedPageSizeIndex = 1;

        public IconsPageViewModel()
            : base("Icons", "Guide showing how to use icons in your application.")
        {
            LoadDataCommand = new RelayCommand(delegate { LoadData(); });
            ApplyTagFilterCommand = new RelayCommand(parameter => ApplyTagFilter(parameter as string));
            _previousPageCommand = new RelayCommand(delegate { PreviousPage(); }, delegate { return CanGoToPreviousPage(); });
            _nextPageCommand = new RelayCommand(delegate { NextPage(); }, delegate { return CanGoToNextPage(); });
        }

        public ICollection<IconData> AllIcons
        {
            get { return _allIcons; }
            set { SetProperty(ref _allIcons, value ?? new List<IconData>()); }
        }

        public ObservableCollection<IconData> SearchFilteredIcons
        {
            get { return _searchFilteredIcons; }
            set { SetProperty(ref _searchFilteredIcons, value ?? new ObservableCollection<IconData>()); }
        }

        public ObservableCollection<IconData> DisplayedIcons
        {
            get { return _displayedIcons; }
            set { SetProperty(ref _displayedIcons, value ?? new ObservableCollection<IconData>()); }
        }

        public IconData SelectedIcon
        {
            get { return _selectedIcon; }
            set { SetProperty(ref _selectedIcon, value); }
        }

        public string SearchText
        {
            get { return _searchText; }
            set
            {
                value = value ?? string.Empty;
                if (SetProperty(ref _searchText, value))
                {
                    UpdateSearchFilter();
                }
            }
        }

        public int CurrentPage
        {
            get { return _currentPage; }
            private set
            {
                if (SetProperty(ref _currentPage, value))
                {
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
                if (SetProperty(ref _totalPages, value))
                {
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
                    if (SetProperty(ref _selectedPageSizeIndex, value))
                    {
                        CurrentPage = 1;
                        UpdatePagination();
                    }
                }
            }
        }

        public List<string> PageSizeOptions { get; } = new List<string> { "100", "250", "500", "1000", "All" };

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
            AllIcons = ReadIconData().ToList();
            SelectedIcon = AllIcons.FirstOrDefault();
            SearchFilteredIcons = new ObservableCollection<IconData>(AllIcons);
            UpdatePagination();
        }

        private int PageSize => SelectedPageSizeIndex == 4
            ? int.MaxValue
            : int.Parse(PageSizeOptions[SelectedPageSizeIndex]);

        private void UpdateSearchFilter()
        {
            var previousSelectedIcon = SelectedIcon;
            var selectedIconName = previousSelectedIcon == null ? null : previousSelectedIcon.Name;
            var comparison = StringComparison.OrdinalIgnoreCase;
            var filterText = SearchText ?? string.Empty;
            SearchFilteredIcons.Clear();

            var searchFilteredIconData = AllIcons.Where(icon =>
                    icon.Name.IndexOf(filterText, comparison) >= 0 ||
                    (icon.Tags?.Any(tag => tag.IndexOf(filterText, comparison) >= 0) ?? false));
            foreach (var item in searchFilteredIconData)
            {
                SearchFilteredIcons.Add(item);
            }

            CurrentPage = 1;
            UpdatePagination(false);

            if (SearchFilteredIcons.Count == 0)
            {
                SelectedIcon = previousSelectedIcon;
                return;
            }

            if (string.IsNullOrWhiteSpace(filterText))
            {
                SelectedIcon = DisplayedIcons.FirstOrDefault();
                return;
            }

            Func<IconData, bool> predicate =
                !string.IsNullOrWhiteSpace(selectedIconName) &&
                DisplayedIcons.Any(icon => icon.Name.Equals(selectedIconName)) ?
                icon => icon.Name.Equals(selectedIconName) :
                icon => true;

            SelectedIcon = DisplayedIcons.FirstOrDefault(predicate);
        }

        private void ApplyTagFilter(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
            {
                return;
            }

            var trimmedTag = tag.Trim();
            if (string.Equals(trimmedTag, SearchText, StringComparison.Ordinal))
            {
                return;
            }

            SearchText = trimmedTag;
        }

        private void PreviousPage()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
                UpdateDisplayedIcons();
            }
        }

        private bool CanGoToPreviousPage() => CurrentPage > 1;

        private void NextPage()
        {
            if (CurrentPage < TotalPages)
            {
                CurrentPage++;
                UpdateDisplayedIcons();
            }
        }

        private bool CanGoToNextPage() => CurrentPage < TotalPages;

        private void UpdatePagination(bool resetSelectedIcon = true)
        {
            var pageSize = PageSize;
            TotalPages = pageSize == int.MaxValue ? 1 : (int)Math.Ceiling((double)SearchFilteredIcons.Count / pageSize);
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
            var skip = (CurrentPage - 1) * pageSize;
            var iconsToDisplay = pageSize == int.MaxValue
                ? SearchFilteredIcons
                : SearchFilteredIcons.Skip(skip).Take(pageSize);

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
