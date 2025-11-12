using LRReader.Shared.Models;
using LRReader.Shared.Models.Main;
using LRReader.UWP.Views.Controls;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace LRReader.UWP.Views.Tabs.Content
{
	public sealed partial class SearchResults : UserControl
	{
		public SearchResults()
		{
			this.InitializeComponent();
		}

		public async Task Refresh()
		{
			await ArchiveList.Refresh();
		}

		public void Search(string query)
		{
			ArchiveList.Search(query);
		}

		public void Search(Category category)
		{
			ArchiveList.Search(category);
		}

		public void Search(SearchState state)
		{
			ArchiveList.Search(state);
		}

		public SearchTabState GetTabState() => ArchiveList.GetTabState();
	}
}
