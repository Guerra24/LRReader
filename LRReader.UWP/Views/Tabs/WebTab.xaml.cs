using LRReader.Shared.Services;
using LRReader.UWP.Views.Controls;
using Windows.UI.Xaml;

namespace LRReader.UWP.Views.Tabs
{
	public sealed partial class WebTab : ModernTab
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
			TabContent.Navigate(page);
		}

		public override void Unload()
		{
			base.Unload();
			TabContent.Close();
		}

		private void TabContent_OnCloseRequested()
		{
			Service.Tabs.CloseTabWithId(CustomTabId);
		}
	}
}
