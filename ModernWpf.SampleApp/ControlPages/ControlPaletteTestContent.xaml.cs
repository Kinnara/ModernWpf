using System;
using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.SampleApp.ControlPages
{
    public partial class ControlPaletteTestContent : UserControl
    {
        public ControlPaletteTestContent()
        {
            InitializeComponent();
        }

        #region Title

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(ControlPaletteTestContent),
            new PropertyMetadata(string.Empty));

        #endregion

        private void RadioButton_Initialized(object sender, EventArgs e)
        {
            ((RadioButton)sender).GroupName = Guid.NewGuid().ToString();
        }
    }
}
