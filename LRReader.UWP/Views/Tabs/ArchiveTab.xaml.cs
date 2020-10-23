using LRReader.Internal;
using LRReader.Shared.Models.Main;
using Windows.UI.Xaml;

namespace LRReader.UWP.Views.Tabs
{
	public sealed partial class ArchiveTab : CustomTab
	{
		private Archive archive;

		private bool loaded;

		public ArchiveTab(Archive archive)
		{
			this.InitializeComponent();
			this.archive = archive;
			this.CustomTabId = "Archive_" + archive.title;
		}

		private void TabViewItem_Loaded(object sender, RoutedEventArgs e)
		{
			if (loaded)
				return;
			loaded = true;
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
