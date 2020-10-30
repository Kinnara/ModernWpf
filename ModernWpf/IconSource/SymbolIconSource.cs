using System.Windows;

namespace ModernWpf.Controls
{
    public class SymbolIconSource : IconSource
    {
		public SymbolIconSource()
		{
		}

		public static readonly DependencyProperty SymbolProperty =
			DependencyProperty.Register(
				nameof(Symbol),
				typeof(Symbol),
				typeof(SymbolIconSource),
				new PropertyMetadata(Symbol.Emoji));

		public Symbol Symbol
		{
			get => (Symbol)GetValue(SymbolProperty);
			set => SetValue(SymbolProperty, value);
		}
	}
}
