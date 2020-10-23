using LRReader.Internal;
using Windows.UI.Xaml;

namespace LRReader.UWP.Views.Tabs
{
	public sealed partial class WebTab : CustomTab
	{

		private string page;

		private bool loaded;

		public WebTab(string page)
		{
			this.InitializeComponent();
			this.page = page;
			this.CustomTabId = "Web_" + page;
		}

		private void TabViewItem_Loaded(object sender, RoutedEventArgs e)
		{
			if (loaded)
				return;
			loaded = true;
			TabContent.LoadPage(page);
		}

		private void RefreshButton_Click(object sender, RoutedEventArgs e) => TabContent.RefreshPage();
	}
}
