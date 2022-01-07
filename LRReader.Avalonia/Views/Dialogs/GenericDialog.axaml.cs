using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace LRReader.Avalonia.Views.Dialogs
{
	public partial class GenericDialog : Window
	{
		public GenericDialog()
		{
			InitializeComponent();
#if DEBUG
			this.AttachDevTools();
#endif
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}
	}
}
