using LRReader.Shared.Models.Main;
using LRReader.UWP.Views.Controls;
using System.Collections.Generic;
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
		}

		public override void Unload()
		{
			base.Unload();
			TabContent.RemoveEvent();
		}
	}
}
