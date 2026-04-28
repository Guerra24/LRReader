using LRReader.Avalonia.Views.Controls;
using LRReader.Shared.Models;
using LRReader.Shared.Models.Main;

namespace LRReader.Avalonia.Views.Tabs;

public partial class ArchiveTab : ModernTab
{
	public ArchiveTab()
	{
		this.InitializeComponent();
	}

	public ArchiveTab(Archive archive, List<Archive> next) : this()
	{
		this.CustomTabId = "Archive_" + archive.arcid;
		TabContent.LoadArchive(archive, next);
		//AutoplayButton.Content = Service.Platform.GetLocalizedString("/Tabs/Archive/AutoplayState/Play");
	}

	public ArchiveTab(Archive archive, ArchiveTabState state) : this()
	{
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
}