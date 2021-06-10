using LRReader.Internal;
using LRReader.Shared.Models.Main;
using Windows.UI.Xaml;

namespace LRReader.UWP.Views.Tabs
{
	public sealed partial class ArchiveEditTab : CustomTab
	{

		public ArchiveEditTab(Archive archive)
		{
			this.InitializeComponent();
			TabContent.LoadArchive(archive);
			this.CustomTabId = "Edit_" + archive.arcid;
		}

	}
}
