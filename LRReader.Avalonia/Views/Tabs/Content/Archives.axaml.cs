using LRReader.Shared.Services;
using LRReader.Shared.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace LRReader.Avalonia.Views.Tabs.Content
{
	public partial class Archives : UserControl
	{

		private ArchivesPageViewModel Data;

		public Archives()
		{
			InitializeComponent();
			Data = Service.Services.GetRequiredService<ArchivesPageViewModel>();
		}

		//public async Task Refresh() => await ArchiveList.Refresh();

		private async Task ArchiveList_OnRefresh()
		{
			await Data.Refresh();
		}

		private async Task ArchiveList_OnLoad()
		{
			await Data.LoadBookmarks();
		}

	}
}
