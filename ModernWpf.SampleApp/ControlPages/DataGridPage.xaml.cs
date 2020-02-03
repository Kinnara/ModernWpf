using ModernWpf.SampleApp.Data;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace ModernWpf.SampleApp.ControlPages
{
    public partial class DataGridPage
    {
        private readonly Stopwatch _stopwatch;
        private DataGridDataSource _viewModel = new DataGridDataSource();
        private CollectionViewSource _cvs;

        public DataGridPage()
        {
            _stopwatch = Stopwatch.StartNew();
            Loaded += OnLoaded;

            InitializeComponent();

            _cvs = (CollectionViewSource)Resources["cvs"];

            //GroupingToggle.IsChecked = true;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnLoaded;

            DataContext = await _viewModel.GetDataAsync();

            var comboBoxColumn = dataGrid.Columns.FirstOrDefault(x => x.Header.Equals("Mountain")) as DataGridComboBoxColumn;
            if (comboBoxColumn != null)
            {
                comboBoxColumn.ItemsSource = await _viewModel.GetMountains();
            }

            _ = Dispatcher.BeginInvoke(() =>
              {
                  _stopwatch.Stop();
                  LoadTimeTextBlock.Text = _stopwatch.ElapsedMilliseconds + " ms";
              }, DispatcherPriority.ApplicationIdle);
        }

        private void ToggleTheme(object sender, RoutedEventArgs e)
        {
            this.ToggleTheme();
        }

        private void GroupingToggle_Checked(object sender, RoutedEventArgs e)
        {
            _cvs.GroupDescriptions.Add(new PropertyGroupDescription(nameof(DataGridDataItem.Range)));
        }

        private void GroupingToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            _cvs.GroupDescriptions.Clear();
        }

        private void LoadTimeTextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            LoadTimeTextBlock.Visibility = Visibility.Collapsed;
        }
    }
}
