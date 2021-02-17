using LRReader.Internal;
using LRReader.Shared.Models.Main;
using Windows.UI.Xaml;

namespace LRReader.UWP.Views.Tabs
{
	public sealed partial class ArchiveTab : CustomTab
	{

		public ArchiveTab(Archive archive)
		{
			this.InitializeComponent();
			this.CustomTabId = "Archive_" + archive.arcid;
			TabContent.LoadArchive(archive);
		}

		public override void Unload()
		{
			base.Unload();
			TabContent.RemoveEvent();
		}

		private void RefreshButton_Click(object sender, RoutedEventArgs e)
		{
			TabContent.Refresh();
		}
	}
}
