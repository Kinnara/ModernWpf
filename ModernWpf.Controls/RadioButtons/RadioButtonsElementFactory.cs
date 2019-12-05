using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.Controls
{
    internal class RadioButtonsElementFactory : ElementFactory
    {
        public RadioButtonsElementFactory()
        {
        }

        protected override UIElement GetElementCore(ElementFactoryGetArgs args)
        {
            if (args.Data is RadioButton radioButton)
            {
                return radioButton;
            }
            else
            {
                var newRadioButton = new RadioButton();
                newRadioButton.Content = args.Data;
                return newRadioButton;
            }
        }

        protected override void RecycleElementCore(ElementFactoryRecycleArgs args)
        {
        }
    }
}
