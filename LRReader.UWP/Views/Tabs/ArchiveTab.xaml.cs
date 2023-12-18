using System.Collections.Generic;
using LRReader.Shared.Models.Main;
using LRReader.Shared.Services;
using LRReader.UWP.Views.Controls;
using Windows.UI.Xaml;

namespace LRReader.UWP.Views.Tabs
{
	public sealed partial class ArchiveTab : ModernTab
	{

		public ArchiveTab(Archive archive, IList<Archive> next)
		{
			this.InitializeComponent();
			this.CustomTabId = "Archive_" + archive.arcid;
			TabContent.LoadArchive(archive, next);
			AutoplayButton.Content = Service.Platform.GetLocalizedString("/Tabs/Archive/AutoplayState/Play");
		}

		public override void Dispose()
		{
			base.Dispose();
			TabContent.RemoveEvent();
		}

		private void AutoplayButton_Checked(object sender, RoutedEventArgs e) => AutoplayButton.Content = Service.Platform.GetLocalizedString("/Tabs/Archive/AutoplayState/Stop");

		private void AutoplayButton_Unchecked(object sender, RoutedEventArgs e) => AutoplayButton.Content = Service.Platform.GetLocalizedString("/Tabs/Archive/AutoplayState/Play");
	}
}
