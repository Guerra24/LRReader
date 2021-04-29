using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace LRReader.Avalonia.Views.Main
{
	public class FirstRunPage : UserControl
	{
		public FirstRunPage()
		{
			InitializeComponent();
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}
	}
}
