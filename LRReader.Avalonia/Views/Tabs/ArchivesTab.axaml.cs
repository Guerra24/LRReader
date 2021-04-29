using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace LRReader.Avalonia.Views.Tabs
{
	public class ArchivesTab : CustomTab
	{
		public ArchivesTab()
		{
			InitializeComponent();
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}
	}
}
