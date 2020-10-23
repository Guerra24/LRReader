using LRReader.Internal;
using Windows.UI.Xaml;

namespace LRReader.UWP.Views.Tabs
{
	public sealed partial class ArchivesTab : CustomTab
	{
		public ArchivesTab()
		{
			this.InitializeComponent();
		}

		private async void RefreshButton_Click(object sender, RoutedEventArgs e)
		{
			await TabContent.Refresh();
		}
	}
}
