using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using ModernWpf.Gallery.Models;
using ModernWpf.Gallery.Testing;

namespace ModernWpf.Gallery.Pages.WpfGallery.Collections
{
    public abstract class CollectionsPageViewModelBase : INotifyPropertyChanged
    {
        // Visual audits compare two processes; fixed seeds remove false drift from source-matching random samples.
        private const int ProductsVisualTestSeed = 12043;
        private const int BasicListViewVisualTestSeed = 22043;
        private const int GridViewVisualTestSeed = 22044;

        protected CollectionsPageViewModelBase(string pageTitle)
        {
            PageTitle = pageTitle;
            PageDescription = string.Empty;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string PageTitle { get; }

        public string PageDescription { get; }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected static ObservableCollection<Product> GenerateProducts()
        {
            var random = CreateSampleRandom(ProductsVisualTestSeed);
            var products = new ObservableCollection<Product> { };

            var adjectives = new[] { "Red", "Blueberry" };
            var names = new[] { "Marmalade", "Dumplings", "Soup" };
            //var units = new[] { "grams", "kilograms", "milliliters" };

            for (int i = 0; i < 50; i++)
            {
                products.Add(
                    new Product
                    {
                        ProductId = i,
                        ProductCode = i,
                        ProductName =
                            adjectives[random.Next(0, adjectives.Length)]
                            + " "
                            + names[random.Next(0, names.Length)],
                        UnitPrice = Math.Round(random.NextDouble() * 20.0, 3),
                        UnitsInStock = random.Next(0, 100)
                    }
                );
            }

            return products;
        }

        protected static ObservableCollection<Person> GenerateBasicListViewPersons()
        {
            return GeneratePersons(BasicListViewVisualTestSeed);
        }

        protected static ObservableCollection<Person> GenerateGridViewPersons()
        {
            return GeneratePersons(GridViewVisualTestSeed);
        }

        private static ObservableCollection<Person> GeneratePersons(int visualTestSeed)
        {
            var random = CreateSampleRandom(visualTestSeed);
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

            for (int i = 0; i < 50; i++)
                persons.Add(
                    new Person(
                        names[random.Next(0, names.Length)],
                        surnames[random.Next(0, surnames.Length)],
                        companies[random.Next(0, companies.Length)]
                    )
                );

            return persons;
        }

        private static GallerySampleRandom CreateSampleRandom(int visualTestSeed)
        {
            return GalleryDiagnostics.IsEnabled
                ? new GallerySampleRandom(visualTestSeed)
                : new GallerySampleRandom();
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

    public partial class DataGridPageViewModel : CollectionsPageViewModelBase
    {
        private ObservableCollection<Product> _productsCollection;

        public DataGridPageViewModel()
            : base("DataGrid")
        {
            _productsCollection = GenerateProducts();
        }

        public ObservableCollection<Product> ProductsCollection
        {
            get { return _productsCollection; }
            set { SetProperty(ref _productsCollection, value); }
        }
    }

    public partial class ListBoxPageViewModel : CollectionsPageViewModelBase
    {
        private ObservableCollection<string> _listBoxItems;

        public ListBoxPageViewModel()
            : base("ListBox")
        {
            _listBoxItems = new ObservableCollection<string>
            {
                "Arial",
                "Comic Sans MS",
                "Courier New",
                "Segoe UI",
                "Times New Roman"
            };
        }

        public ObservableCollection<string> ListBoxItems
        {
            get { return _listBoxItems; }
            set { SetProperty(ref _listBoxItems, value); }
        }
    }

    public partial class ListViewPageViewModel : CollectionsPageViewModelBase
    {
        private int _listViewSelectionModeComboBoxSelectedIndex = 0;

        public int ListViewSelectionModeComboBoxSelectedIndex
        {
            get { return _listViewSelectionModeComboBoxSelectedIndex; }
            set
            {
                SetProperty(ref _listViewSelectionModeComboBoxSelectedIndex, value);
                UpdateListViewSelectionMode(value);
            }
        }

        private SelectionMode _listViewSelectionMode = SelectionMode.Single;
        private ObservableCollection<Person> _basicListViewItems;
        private ObservableCollection<Person> _gridViewItems;

        public ListViewPageViewModel()
            : base("ListView")
        {
            _basicListViewItems = GenerateBasicListViewPersons();
            _gridViewItems = GenerateGridViewPersons();
        }

        public SelectionMode ListViewSelectionMode
        {
            get { return _listViewSelectionMode; }
            set { SetProperty(ref _listViewSelectionMode, value); }
        }

        public ObservableCollection<Person> BasicListViewItems
        {
            get { return _basicListViewItems; }
            set { SetProperty(ref _basicListViewItems, value); }
        }

        public ObservableCollection<Person> GridViewItems
        {
            get { return _gridViewItems; }
            set { SetProperty(ref _gridViewItems, value); }
        }

        private void UpdateListViewSelectionMode(int selectionModeIndex)
        {
            ListViewSelectionMode = selectionModeIndex switch
            {
                1 => SelectionMode.Multiple,
                2 => SelectionMode.Extended,
                _ => SelectionMode.Single
            };
        }
    }

    public partial class TreeViewPageViewModel : CollectionsPageViewModelBase
    {
        public TreeViewPageViewModel()
            : base("TreeView")
        {
        }
    }
}
