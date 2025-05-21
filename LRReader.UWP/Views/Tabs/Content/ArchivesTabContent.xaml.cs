using LRReader.Shared.Services;
using LRReader.Shared.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace LRReader.UWP.Views.Tabs.Content
{
	public sealed partial class ArchivesTabContent : UserControl
	{

		private ArchivesPageViewModel Data;

		public ArchivesTabContent()
		{
			this.InitializeComponent();
			Data = Service.Services.GetRequiredService<ArchivesPageViewModel>();
		}

		public async Task Refresh() => await ArchiveList.Refresh();

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
