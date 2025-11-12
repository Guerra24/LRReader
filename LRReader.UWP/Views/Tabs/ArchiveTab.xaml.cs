using LRReader.Shared.Models;
using LRReader.Shared.Models.Main;
using LRReader.UWP.Views.Controls;
using System.Collections.Generic;

namespace LRReader.UWP.Views.Tabs
{
	public sealed partial class ArchiveTab : ModernTab
	{

		public ArchiveTab(Archive archive, List<Archive> next)
		{
			this.InitializeComponent();
			this.CustomTabId = "Archive_" + archive.arcid;
			TabContent.LoadArchive(archive, next);
			//AutoplayButton.Content = Service.Platform.GetLocalizedString("/Tabs/Archive/AutoplayState/Play");
		}

		public ArchiveTab(Archive archive, ArchiveTabState state)
		{
			this.InitializeComponent();
			this.CustomTabId = "Archive_" + archive.arcid;
			TabContent.LoadArchive(archive, state: state);
			//AutoplayButton.Content = Service.Platform.GetLocalizedString("/Tabs/Archive/AutoplayState/Play");
		}

		public override void Dispose()
		{
			base.Dispose();
			TabContent.RemoveEvent();
		}

		public override TabState GetTabState() => TabContent.GetTabState();

		//private void AutoplayButton_Checked(object sender, RoutedEventArgs e) => AutoplayButton.Content = Service.Platform.GetLocalizedString("/Tabs/Archive/AutoplayState/Stop");

		//private void AutoplayButton_Unchecked(object sender, RoutedEventArgs e) => AutoplayButton.Content = Service.Platform.GetLocalizedString("/Tabs/Archive/AutoplayState/Play");
	}
}
