using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Page = ModernWpf.Controls.Page;

namespace ModernWpf.SampleApp.ControlPages
{
    /// <summary>
    /// ToggleSplitButtonPage.xaml 的交互逻辑
    /// </summary>
    public partial class ToggleSplitButtonPage : Page
    {
        private string _type = "•";
        public ToggleSplitButtonPage()
        {
            InitializeComponent();
        }

        private void BulletButton_Click(object sender, RoutedEventArgs e)
        {
            Button clickedBullet = (Button)sender;
            SymbolIcon symbol = (SymbolIcon)clickedBullet.Content;

            if (symbol.Symbol == Symbol.List)
            {
                _type = "•";
                mySymbolIcon.Symbol = Symbol.List;
                myListButton.SetValue(AutomationProperties.NameProperty, "Bullets");
            }
            else if (symbol.Symbol == Symbol.Bullets)
            {
                _type = "I)";
                mySymbolIcon.Symbol = Symbol.Bullets;
                myListButton.SetValue(AutomationProperties.NameProperty, "Roman Numerals");
            }
            myRichEditBox.Selection.Text = _type;

            myListButton.IsChecked = true;
            myListButton.Flyout.Hide();
            myRichEditBox.Focus();
        }

        private void MyListButton_IsCheckedChanged(ToggleSplitButton sender, ToggleSplitButtonIsCheckedChangedEventArgs args)
        {
            if (sender.IsChecked)
            {
                //add bulleted list
                myRichEditBox.Selection.Text = _type;
            }
            else
            {
                //remove bulleted list
                myRichEditBox.Selection.Text = "";
            }
        }
    }
}
