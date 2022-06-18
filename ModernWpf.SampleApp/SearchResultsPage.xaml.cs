using ModernWpf.Controls;
using ModernWpf.Navigation;
using ModernWpf.SampleApp.DataModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ModernWpf.SampleApp
{
    /// <summary>
    /// SearchResultsPage.xaml 的交互逻辑
    /// </summary>
    public partial class SearchResultsPage : ItemsPageBase
    {
        private IEnumerable<Filter> _filters;
        private Filter _selectedFilter;
        string _queryText;

        public IEnumerable<Filter> Filters
        {
            get { return _filters; }
            set { this.SetProperty(ref _filters, value); }
        }

        public SearchResultsPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var queryText = e.ExtraData?.ToString().ToLower();

            BuildFilterList(queryText);

            NavigationRootPage.Current.NavigationView.Header = "Search";
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);

            _selectedFilter = (Filter)resultsNavView.SelectedItem;
        }

        private void OnResultsNavViewLoaded(object sender, RoutedEventArgs e)
        {
            resultsNavView.Focus();
        }

        private void OnResultsNavViewSelectionChanged(object sender, NavigationViewSelectionChangedEventArgs e)
        {
            if (e.SelectedItem != null)
            {
                _selectedFilter = (Filter)e.SelectedItem;
            }
        }

        private void BuildFilterList(string queryText)
        {
            if (!string.IsNullOrEmpty(queryText))
            {
                // Application-specific searching logic.  The search process is responsible for
                // creating a list of user-selectable result categories:
                var filterList = new List<Filter>();

                // Query is already lowercase
                var querySplit = queryText.ToLower().Split(' ');
                foreach (var group in ControlInfoDataSource.Instance.Groups)
                {
                    var matchingItems =
                        group.Items.Where(item =>
                        {
                            // Idea: check for every word entered (separated by space) if it is in the name,
                            // e.g. for query "split button" the only result should "SplitButton" since its the only query to contain "split" and "button"
                            // If any of the sub tokens is not in the string, we ignore the item. So the search gets more precise with more words
                            bool flag = true;
                            foreach (string queryToken in querySplit)
                            {
                                // Check if token is in title or subtitle
                                if (!item.Title.ToLower().Contains(queryToken) && !item.Subtitle.ToLower().Contains(queryToken))
                                {
                                    // Neither title nor sub title contain one of the tokens so we discard this item!
                                    flag = false;
                                }
                            }
                            return flag;
                        }).ToList();
                    int numberOfMatchingItems = matchingItems.Count();

                    if (numberOfMatchingItems > 0)
                    {
                        filterList.Add(new Filter(group.Title, numberOfMatchingItems, matchingItems));
                    }
                }

                if (filterList.Count == 0)
                {
                    // Display informational text when there are no search results.
                    VisualStateManager.GoToState(this, "NoResultsFound", false);
                    var textbox = NavigationRootPage.GetForElement(this)?.PageHeader?.FindDescendants<AutoSuggestBox>().FirstOrDefault();
                    textbox?.Focus();
                }
                else
                {
                    // When there are search results, set Filters
                    var allControls = filterList.SelectMany(s => s.Items).ToList();
                    filterList.Insert(0, new Filter("All", allControls.Count, allControls, true));
                    Filters = filterList;

                    // Check to see if the current query matches the last
                    if (_queryText == queryText && _selectedFilter != null)
                    {
                        // If so try to restore any previously selected nav view item
                        resultsNavView.SelectedItem = Filters.Where(f => f.Name == _selectedFilter.Name).SingleOrDefault();
                    }
                    else
                    {
                        // Otherwise reset query text and nav view filter
                        _queryText = queryText;
                        resultsNavView.SelectedItem = Filters.FirstOrDefault();
                    }

                    VisualStateManager.GoToState(this, "ResultsFound", false);
                }
            }
        }
    }

    /// <summary>
    /// View model describing one of the filters available for viewing search results.
    /// </summary>
    public sealed class Filter : INotifyPropertyChanged
    {
        private string _name;
        private int _count;
        private bool? _active;
        private List<ControlInfoDataItem> _items;

        public Filter(string name, int count, List<ControlInfoDataItem> controlInfoList, bool active = false)
        {
            this.Name = name;
            this.Count = count;
            this.Active = active;
            this.Items = controlInfoList;
        }

        public override string ToString()
        {
            return Description;
        }

        public List<ControlInfoDataItem> Items
        {
            get { return _items; }
            set { this.SetProperty(ref _items, value); }
        }

        public string Name
        {
            get { return _name; }
            set { if (this.SetProperty(ref _name, value)) this.NotifyPropertyChanged(nameof(Description)); }
        }

        public int Count
        {
            get { return _count; }
            set { if (this.SetProperty(ref _count, value)) this.NotifyPropertyChanged(nameof(Description)); }
        }

        public bool? Active
        {
            get { return _active; }
            set { this.SetProperty(ref _active, value); }
        }

        public string Description
        {
            get { return string.Format("{0} ({1})", _name, _count); }
        }

        /// <summary>
        /// Multicast event for property change notifications.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Checks if a property already matches a desired value.  Sets the property and
        /// notifies listeners only when necessary.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="storage">Reference to a property with both getter and setter.</param>
        /// <param name="value">Desired value for the property.</param>
        /// <param name="propertyName">Name of the property used to notify listeners.  This
        /// value is optional and can be provided automatically when invoked from compilers that
        /// support CallerMemberName.</param>
        /// <returns>True if the value was changed, false if the existing value matched the
        /// desired value.</returns>
        private bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (object.Equals(storage, value)) return false;

            storage = value;
            this.NotifyPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// Notifies listeners that a property value has changed.
        /// </summary>
        /// <param name="propertyName">Name of the property used to notify listeners.  This
        /// value is optional and can be provided automatically when invoked from compilers
        /// that support <see cref="CallerMemberNameAttribute"/>.</param>
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
