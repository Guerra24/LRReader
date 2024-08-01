using LRReader.Shared.Models.Main;
using LRReader.UWP.Views.Controls;
using Windows.UI.Xaml;

namespace LRReader.UWP.Views.Tabs
{
	public sealed partial class TankoubonEditTab : ModernTab
	{
		private Tankoubon tankoubon;

		private bool loaded;

		public TankoubonEditTab(Tankoubon tankoubon)
		{
			this.InitializeComponent();
			this.tankoubon = tankoubon;
			CustomTabId = "TankoubonEdit_" + tankoubon.id;
		}

		private void TabViewItem_Loaded(object sender, RoutedEventArgs e)
		{
			if (loaded)
				return;
			loaded = true;
			TabContent.Load(tankoubon);
		}

		private async void RefreshButton_Click(object sender, RoutedEventArgs e)
		{
			await TabContent.Refresh();
		}
	}
}
