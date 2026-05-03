using LRReader.Shared.Models;
using LRReader.Shared.Models.Main;

namespace LRReader.Avalonia.Views.Tabs.Content;

public partial class SearchResults : UserControl
{
	public SearchResults()
	{
		InitializeComponent();
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