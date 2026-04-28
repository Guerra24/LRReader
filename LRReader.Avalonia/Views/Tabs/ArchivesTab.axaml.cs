using Avalonia.Interactivity;
using LRReader.Avalonia.Views.Content;
using LRReader.Avalonia.Views.Controls;

namespace LRReader.Avalonia.Views.Tabs
{
	public partial class ArchivesTab : ModernTab
	{
		public ArchivesTab()
		{
			InitializeComponent();
		}

		private async void RefreshButton_Click(object? sender, RoutedEventArgs e)
		{
			await TabContent.Refresh();
		}
	}
}
