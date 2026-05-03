using Avalonia.Interactivity;
using LRReader.Avalonia.Views.Controls;
using LRReader.Shared.Models;
using LRReader.Shared.Models.Main;

namespace LRReader.Avalonia.Views.Tabs;

public partial class SearchResultsTab : ModernTab
{
	public SearchResultsTab()
	{
		InitializeComponent();
		CustomTabId = "Search_" + new Random().Next();
	}

	public SearchResultsTab(string query = "") : this()
	{
		TabContent.Search(query);
	}

	public SearchResultsTab(Category category)
	{
		this.InitializeComponent();
		CustomTabId = "Search_" + category.id;
		Header = category.name;
		TabContent.Search(category);
	}

	public SearchResultsTab(SearchState state) : this()
	{
		TabContent.Search(state);
	}

	private async void RefreshButton_Click(object sender, RoutedEventArgs e)
	{
		await TabContent.Refresh();
	}

	public override TabState GetTabState() => TabContent.GetTabState();
}