using LRReader.Shared.Models.Main;
using LRReader.UWP.Views.Controls;

namespace LRReader.UWP.Views.Tabs
{
	public sealed partial class ArchiveEditTab : ModernTab
	{

		public ArchiveEditTab(Archive archive)
		{
			this.InitializeComponent();
			TabContent.LoadArchive(archive);
			this.CustomTabId = "Edit_" + archive.arcid;
		}

	}
}
