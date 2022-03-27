using ModernWpf.Controls;
using ModernWpf.SampleApp.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Page = ModernWpf.Controls.Page;

namespace ModernWpf.SampleApp.ControlPages
{
    /// <summary>
    /// CalendarViewPage.xaml 的交互逻辑
    /// </summary>
    public partial class CalendarViewPage : Page
    {
        private Calendar Control1;
        private ComboBox SelectionMode;
        private ComboBox CalendarLanguages;

        public CalendarViewPage()
        {
            InitializeComponent();
        }

        private void SelectionMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Enum.TryParse((sender as ComboBox).SelectedItem.ToString(), out CalendarSelectionMode selectionMode) && Control1 != null)
            {
                Control1.SelectionMode = selectionMode;
            }
        }

        private void CalendarLanguages_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedLang = CalendarLanguages.SelectedValue.ToString();
            if (Windows.Globalization.Language.IsWellFormed(selectedLang) && Control1 != null)
            {
                Control1.Language = XmlLanguage.GetLanguage(selectedLang);
            }
        }

        private void ComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is ComboBox b)
            {
                string name = b.Tag.ToString();

                switch (name)
                {
                    case "SelectionMode":
                        SelectionMode = b;
                        break;
                    case "CalendarLanguages":
                        CalendarLanguages = b;
                        var langs = new LanguageList();
                        CalendarLanguages.ItemsSource = langs.Languages;
                        break;
                }
            }
        }

        private void Calendar_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is Calendar b)
            {
                Control1 = b;

                if (SelectionMode != null && Enum.TryParse(SelectionMode.SelectedItem.ToString(), out CalendarSelectionMode selectionMode))
                {
                    b.SelectionMode = selectionMode;
                }

                if (CalendarLanguages != null)
                {
                    string selectedLang = CalendarLanguages.SelectedValue.ToString();
                    if (Windows.Globalization.Language.IsWellFormed(selectedLang))
                    {
                        b.Language = XmlLanguage.GetLanguage(selectedLang);
                    }
                }

                ControlExampleSubstitution Substitution1 = new ControlExampleSubstitution
                {
                    Key = "SelectionMode",
                };
                BindingOperations.SetBinding(Substitution1, ControlExampleSubstitution.ValueProperty, new Binding
                {
                    Source = b,
                    Path = new PropertyPath("SelectionMode"),
                });

                ControlExampleSubstitution Substitution2 = new ControlExampleSubstitution
                {
                    Key = "Language",
                };
                BindingOperations.SetBinding(Substitution2, ControlExampleSubstitution.ValueProperty, new Binding
                {
                    Source = b,
                    Path = new PropertyPath("Language"),
                });

                List<ControlExampleSubstitution> Substitutions = new List<ControlExampleSubstitution>() { Substitution1, Substitution2 };
                ExampleAccessories.Substitutions = Substitutions;
            }
        }
    }
}
