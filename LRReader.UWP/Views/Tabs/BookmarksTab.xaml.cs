using LRReader.Internal;
using Windows.UI.Xaml;

namespace LRReader.UWP.Views.Tabs
{
	public sealed partial class BookmarksTab : CustomTab
	{
		public BookmarksTab()
		{
			this.InitializeComponent();
		}

		private void RefreshButton_Click(object sender, RoutedEventArgs e)
		{
			TabContent.Refresh();
		}
	}
}
