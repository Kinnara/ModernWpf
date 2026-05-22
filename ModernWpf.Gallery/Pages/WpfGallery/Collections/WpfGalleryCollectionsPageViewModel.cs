using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using ModernWpf.Gallery.Models;

namespace ModernWpf.Gallery.Pages.WpfGallery.Collections
{
    public sealed class WpfGalleryCollectionsPageViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Product> _productsCollection;
        private ObservableCollection<string> _listBoxItems;
        private ObservableCollection<Person> _basicListViewItems;
        private ObservableCollection<Person> _gridViewItems;
        private int _listViewSelectionModeComboBoxSelectedIndex;
        private SelectionMode _listViewSelectionMode = SelectionMode.Single;

        public WpfGalleryCollectionsPageViewModel(string pageTitle)
        {
            PageTitle = pageTitle;
            PageDescription = string.Empty;
            ProductsCollection = GenerateProducts();
            ListBoxItems = new ObservableCollection<string>
            {
                "Arial",
                "Comic Sans MS",
                "Courier New",
                "Segoe UI",
                "Times New Roman"
            };
            BasicListViewItems = GeneratePersons();
            GridViewItems = GeneratePersons();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string PageTitle { get; }

        public string PageDescription { get; }

        public ObservableCollection<Product> ProductsCollection
        {
            get { return _productsCollection; }
            set
            {
                if (_productsCollection == value)
                {
                    return;
                }

                _productsCollection = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<string> ListBoxItems
        {
            get { return _listBoxItems; }
            set
            {
                if (_listBoxItems == value)
                {
                    return;
                }

                _listBoxItems = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Person> BasicListViewItems
        {
            get { return _basicListViewItems; }
            set
            {
                if (_basicListViewItems == value)
                {
                    return;
                }

                _basicListViewItems = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Person> GridViewItems
        {
            get { return _gridViewItems; }
            set
            {
                if (_gridViewItems == value)
                {
                    return;
                }

                _gridViewItems = value;
                OnPropertyChanged();
            }
        }

        public int ListViewSelectionModeComboBoxSelectedIndex
        {
            get { return _listViewSelectionModeComboBoxSelectedIndex; }
            set
            {
                if (_listViewSelectionModeComboBoxSelectedIndex == value)
                {
                    return;
                }

                _listViewSelectionModeComboBoxSelectedIndex = value;
                OnPropertyChanged();
                ListViewSelectionMode = value == 1 ? SelectionMode.Multiple : value == 2 ? SelectionMode.Extended : SelectionMode.Single;
            }
        }

        public SelectionMode ListViewSelectionMode
        {
            get { return _listViewSelectionMode; }
            private set
            {
                if (_listViewSelectionMode == value)
                {
                    return;
                }

                _listViewSelectionMode = value;
                OnPropertyChanged();
            }
        }

        private static ObservableCollection<Product> GenerateProducts()
        {
            var random = new Random(0);
            var products = new ObservableCollection<Product>();
            var adjectives = new[] { "Red", "Blueberry" };
            var names = new[] { "Marmalade", "Dumplings", "Soup" };

            for (var i = 0; i < 50; i++)
            {
                products.Add(new Product
                {
                    ProductId = i,
                    ProductCode = i,
                    ProductName = adjectives[random.Next(0, adjectives.Length)] + " " + names[random.Next(0, names.Length)],
                    UnitPrice = Math.Round(random.NextDouble() * 20.0, 3),
                    UnitsInStock = random.Next(0, 100)
                });
            }

            return products;
        }

        private static ObservableCollection<Person> GeneratePersons()
        {
            var random = new Random(0);
            var persons = new ObservableCollection<Person>();
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
                "Arielle",
                "Arielle",
                "Jamie",
                "Alexander"
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
                "StellarEdge Ventrues",
                "Elysium Crest Holdings"
            };

            for (var i = 0; i < 50; i++)
            {
                persons.Add(new Person(
                    names[random.Next(0, names.Length)],
                    surnames[random.Next(0, surnames.Length)],
                    companies[random.Next(0, companies.Length)]));
            }

            return persons;
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
