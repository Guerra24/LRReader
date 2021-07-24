using LRReader.UWP.Views.Controls;
using Windows.UI.Xaml;

namespace LRReader.UWP.Views.Tabs
{
	public sealed partial class BookmarksTab : ModernTab
	{
		public BookmarksTab()
		{
			this.InitializeComponent();
		}

		private void RefreshButton_Click(object sender, RoutedEventArgs e)
		{
			TabContent.Refresh();
		}

		private void ExportButton_Click(object sender, RoutedEventArgs e)
		{
			TabContent.ExportBookmarks();
		}

		private void ImportButton_Click(object sender, RoutedEventArgs e)
		{
			TabContent.ImportBookmarks();
		}
	}
}
