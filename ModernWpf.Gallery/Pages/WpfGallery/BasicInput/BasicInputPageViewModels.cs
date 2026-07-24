using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Input;
using ModernWpf.Gallery.Pages.WpfGallery;

namespace ModernWpf.Gallery.Pages.WpfGallery.BasicInput
{
    public abstract class BasicInputPageViewModelBase : WpfGalleryPageViewModel
    {
        protected BasicInputPageViewModelBase(string pageTitle)
            : base(pageTitle, "")
        {
        }

        protected static ICommand CreateCommand(Action<object> execute)
        {
            return new RelayCommand(execute);
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

    /// <summary>
    /// Interaction logic for Button.xaml
    /// </summary>
    public partial class ButtonPageViewModel : BasicInputPageViewModelBase
    {
        private string _message = "Hello World!";
        private bool _isSimpleButtonEnabled = true;
        private bool _isUiButtonEnabled = true;

        public ButtonPageViewModel()
            : base("Button")
        {
            SimpleButtonCheckboxCheckedCommand = CreateCommand(OnSimpleButtonCheckboxChecked);
            UiButtonCheckboxCheckedCommand = CreateCommand(OnUiButtonCheckboxChecked);
        }

        public ICommand SimpleButtonCheckboxCheckedCommand { get; }

        public ICommand UiButtonCheckboxCheckedCommand { get; }

        public string Message
        {
            get { return _message; }
            set { SetProperty(ref _message, value); }
        }

        public bool IsSimpleButtonEnabled
        {
            get { return _isSimpleButtonEnabled; }
            set { SetProperty(ref _isSimpleButtonEnabled, value); }
        }

        public bool IsUiButtonEnabled
        {
            get { return _isUiButtonEnabled; }
            set { SetProperty(ref _isUiButtonEnabled, value); }
        }

        private void OnSimpleButtonCheckboxChecked(object sender)
        {
            if (sender is not CheckBox checkbox)
                return;

            IsSimpleButtonEnabled = !(checkbox?.IsChecked ?? false);
        }

        private void OnUiButtonCheckboxChecked(object sender)
        {
            if (sender is not CheckBox checkbox)
                return;

            IsUiButtonEnabled = !(checkbox?.IsChecked ?? false);
        }
    }

    public partial class CheckBoxPageViewModel : BasicInputPageViewModelBase
    {
        private bool? _selectAllCheckBoxChecked = null;
        private bool _optionOneCheckBoxChecked = false;
        private bool _optionTwoCheckBoxChecked = true;
        private bool _optionThreeCheckBoxChecked = false;

        public CheckBoxPageViewModel()
            : base("CheckBox")
        {
            SelectAllCheckedCommand = CreateCommand(OnSelectAllChecked);
            SingleCheckedCommand = CreateCommand(option => OnSingleChecked(option as string));
        }

        public ICommand SelectAllCheckedCommand { get; }

        public ICommand SingleCheckedCommand { get; }

        public bool? SelectAllCheckBoxChecked
        {
            get { return _selectAllCheckBoxChecked; }
            set { SetProperty(ref _selectAllCheckBoxChecked, value); }
        }

        public bool OptionOneCheckBoxChecked
        {
            get { return _optionOneCheckBoxChecked; }
            set { SetProperty(ref _optionOneCheckBoxChecked, value); }
        }

        public bool OptionTwoCheckBoxChecked
        {
            get { return _optionTwoCheckBoxChecked; }
            set { SetProperty(ref _optionTwoCheckBoxChecked, value); }
        }

        public bool OptionThreeCheckBoxChecked
        {
            get { return _optionThreeCheckBoxChecked; }
            set { SetProperty(ref _optionThreeCheckBoxChecked, value); }
        }

        private void OnSelectAllChecked(object sender)
        {
            if (sender is not CheckBox checkBox)
                return;

            if (checkBox.IsChecked == null)
                checkBox.IsChecked = !(
                    OptionOneCheckBoxChecked && OptionTwoCheckBoxChecked && OptionThreeCheckBoxChecked
                );

            if (checkBox.IsChecked == true)
            {
                OptionOneCheckBoxChecked = true;
                OptionTwoCheckBoxChecked = true;
                OptionThreeCheckBoxChecked = true;
            }
            else if (checkBox.IsChecked == false)
            {
                OptionOneCheckBoxChecked = false;
                OptionTwoCheckBoxChecked = false;
                OptionThreeCheckBoxChecked = false;
            }
        }

        private void OnSingleChecked(string option)
        {
            if (OptionOneCheckBoxChecked && OptionTwoCheckBoxChecked && OptionThreeCheckBoxChecked)
                SelectAllCheckBoxChecked = true;
            else if (!OptionOneCheckBoxChecked && !OptionTwoCheckBoxChecked && !OptionThreeCheckBoxChecked)
                SelectAllCheckBoxChecked = false;
            else
                SelectAllCheckBoxChecked = null;
        }
    }

    public partial class ComboBoxPageViewModel : BasicInputPageViewModelBase
    {
        private IList<string> _comboBoxFontFamilies = new ObservableCollection<string>
        {
            "Arial",
            "Comic Sans MS",
            "Segoe UI",
            "Times New Roman"
        };

        private IList<int> _comboBoxFontSizes = new ObservableCollection<int>
        {
            8,
            9,
            10,
            11,
            12,
            14,
            16,
            18,
            20,
            24,
            28,
            36,
            48,
            72
        };

        public ComboBoxPageViewModel()
            : base("ComboBox")
        {
        }

        public IList<string> ComboBoxFontFamilies
        {
            get { return _comboBoxFontFamilies; }
            set { SetProperty(ref _comboBoxFontFamilies, value); }
        }

        public IList<int> ComboBoxFontSizes
        {
            get { return _comboBoxFontSizes; }
            set { SetProperty(ref _comboBoxFontSizes, value); }
        }
    }

    public partial class RadioButtonPageViewModel : BasicInputPageViewModelBase
    {
        private bool _isRadioButtonEnabled = true;

        public RadioButtonPageViewModel()
            : base("RadioButton")
        {
            RadioButtonCheckboxCheckedCommand = CreateCommand(OnRadioButtonCheckboxChecked);
        }

        public ICommand RadioButtonCheckboxCheckedCommand { get; }

        public bool IsRadioButtonEnabled
        {
            get { return _isRadioButtonEnabled; }
            set { SetProperty(ref _isRadioButtonEnabled, value); }
        }

        private void OnRadioButtonCheckboxChecked(object sender)
        {
            if (sender is not CheckBox checkbox)
                return;

            IsRadioButtonEnabled = !(checkbox?.IsChecked ?? false);
        }
    }

    public partial class SliderPageViewModel : BasicInputPageViewModelBase
    {
        private int _simpleSliderValue = 0;
        private int _rangeSliderValue = 500;
        private int _marksSliderValue = 0;
        private int _verticalSliderValue = 0;

        public SliderPageViewModel()
            : base("Slider")
        {
        }

        public int SimpleSliderValue
        {
            get { return _simpleSliderValue; }
            set { SetProperty(ref _simpleSliderValue, value); }
        }

        public int RangeSliderValue
        {
            get { return _rangeSliderValue; }
            set { SetProperty(ref _rangeSliderValue, value); }
        }

        public int MarksSliderValue
        {
            get { return _marksSliderValue; }
            set { SetProperty(ref _marksSliderValue, value); }
        }

        public int VerticalSliderValue
        {
            get { return _verticalSliderValue; }
            set { SetProperty(ref _verticalSliderValue, value); }
        }
    }
}
