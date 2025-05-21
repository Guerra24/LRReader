using LRReader.UWP.Views.Controls;

namespace LRReader.UWP.Views.Tabs
{
	public sealed partial class ArchivesTab : ModernTab
	{
		public ArchivesTab()
		{
			this.InitializeComponent();
		}

		private async void RefreshButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
		{
			await TabContent.Refresh();
		}
	}
}
